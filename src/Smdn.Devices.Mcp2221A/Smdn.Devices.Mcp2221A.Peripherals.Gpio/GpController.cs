// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
public abstract partial class GpController {
#pragma warning restore IDE0040
  private readonly Mcp2221AGpioDriver gpio;

  private protected GpDesignation CurrentGpDesignation => gpio.GetCurrentGpDesignation(Index);

  /// <summary>
  /// Gets the logical index of General Purpose (GP) pin represented by the current instance,
  /// ranging from 0 to 3 (e.g., 0 for GP0, 1 for GP1).
  /// </summary>
  /// <remarks>
  /// This is a logical index used to identify the pin within the
  /// <see cref="Mcp2221A.GpPins"/> collection. It does not refer to the physical
  /// pin number (1-14) on the MCP2221/MCP2221A chip package.
  /// </remarks>
  public abstract int Index { get; }

  /// <summary>
  /// Gets the GP pin name represented by the current instance.
  /// </summary>
  public abstract string PinName { get; }

  /// <summary>
  /// Gets the current function assigned to the GP pin.
  /// </summary>
  /// <value>
  /// A <see cref="GpFunction"/> value indicating the current function of the pin.
  /// </value>
  /// <remarks>
  /// This value changes when a configuration method, such as
  /// <see cref="ConfigureAsGpio(PinMode, PinValue, CancellationToken)"/>,
  /// is successfully called.
  /// </remarks>
  /// <seealso cref="CurrentDesignation"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 1.7.1 CONFIGURABLE PIN FUNCTIONS
  /// [MCP2221A] TABLE 1-5: GP DESIGNATION TABLE
  /// </seealso>
  public abstract GpFunction CurrentFunction { get; }

  /// <summary>
  /// Gets the specific hardware designation label currently assigned to the GP pin.
  /// </summary>
  /// <value>
  /// A <see cref="string"/> representing the hardware-specific function name
  /// (e.g., <c>GPIO</c>, <c>ADC1</c>, <c>SSPND</c>, <c>LED_I2C</c>).
  /// </value>
  /// <remarks>
  /// This property returns the label corresponding to the current <see cref="CurrentFunction"/>,
  /// taking into account the specific GP pin index. For example, if <see cref="CurrentFunction"/>
  /// is <see cref="GpFunction.LedOutput"/>, this property may return <c>LED_URX</c>,
  /// <c>LED_UTX</c>, or <c>LED_I2C</c> depending on the pin.
  /// </remarks>
  /// <seealso cref="CurrentFunction"/>
  public abstract string CurrentDesignation { get; }

  private protected GpController(Mcp2221AGpioDriver gpio)
  {
    this.gpio = gpio;
  }

  private protected ValueTask ConfigureGpDesignationAsync(
    GpDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
    => gpio.ConfigureGpDesignationAsync(
      gp: Index,
      gpDesignation: gpDesignation,
      gpioDirection: gpioInitialDirection,
      gpioValue: gpioInitialValue,
      cancellationToken: cancellationToken
    );

  private protected void ConfigureGpDesignation(
    GpDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
    => gpio.ConfigureGpDesignation(
      gp: Index,
      gpDesignation: gpDesignation,
      gpioDirection: gpioInitialDirection,
      gpioValue: gpioInitialValue,
      cancellationToken: cancellationToken
    );
}
