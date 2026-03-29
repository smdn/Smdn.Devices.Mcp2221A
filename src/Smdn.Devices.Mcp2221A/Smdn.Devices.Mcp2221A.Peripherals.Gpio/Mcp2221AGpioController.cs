// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <remarks>
/// <para>
/// The pin numbers refer to logical GPIO pin index, not physical pin numbers.
/// Specifically, pin numbers <c>0</c>-<c>3</c> correspond to <c>GP0</c>-<c>GP3</c>.
/// </para>
/// </remarks>
internal sealed class Mcp2221AGpioController : GpioController {
  private Mcp2221AGpioDriver? driver;

  internal Mcp2221AGpioController(
    Mcp2221AGpioDriver driver
  )
    : base(
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
      driver: driver
#else
      driver: driver,
      numberingScheme: PinNumberingScheme.Logical
#endif
    )
  {
    this.driver = driver;
  }

  protected override void Dispose(bool disposing)
  {
    // Since the reference to the driver is also held by the base class,
    // and Dispose() is called there, this class simply releases
    // the reference.
    driver = null;

    base.Dispose(disposing);
  }

  /// <inheritdoc/>
  /// <remarks>
  /// In the base class, the <see cref="GpioController.GetPinMode"/> is called before writing,
  /// and the result is used to check if it is set to <see cref="PinMode.Output"/>.
  /// However, this requires sending the command twice for each write operation,
  /// resulting in significant overhead.
  /// Therefore, this class overrides it to change the its behavior to just
  /// reference the cached <see cref="PinMode"/>.
  /// Because the <see cref="GpioController.OpenPin(int, PinMode)"/> method ensures that only the
  /// <see cref="Mcp2221AGpioController"/> can exclusively modify the <see cref="PinMode"/>,
  /// there is no need to call <see cref="GpioController.GetPinMode"/> to check the <see cref="PinMode"/>.
  /// </remarks>
  public override void Write(int pinNumber, PinValue value)
  {
    if (driver is null)
      throw new ObjectDisposedException(GetType().FullName);

    driver.ThrowIfDisposed();

    if (!IsPinOpen(pinNumber))
      throw new InvalidOperationException($"Can not write to pin {pinNumber} because it is not open.");

    if (driver.GetLastUpdatedDirectionOrThrow(pinNumber) != PinMode.Output)
      return;

    driver.WriteWithoutModeCheck(pinNumber, value);
  }
}
