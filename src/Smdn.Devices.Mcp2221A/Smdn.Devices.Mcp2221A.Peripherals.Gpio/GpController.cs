// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
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

  /// <summary>
  /// Gets the digital logic level of the GP pin obtained during the
  /// last successful communication.
  /// </summary>
  /// <value>
  /// The <see cref="PinValue"/> captured from the device at the time of
  /// the last fetch operation.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call
  /// <see cref="IGpioController.ReadAsync(System.Threading.CancellationToken)"/> or
  /// <see cref="IGpioController.GetModeAsync(System.Threading.CancellationToken)"/>
  /// (and their synchronous counterparts).
  /// </para>
  /// <para>
  /// The MCP2221A retrieves the logic levels and I/O modes for all GP pins (GP0-GP3)
  /// simultaneously in a single command. Therefore, whenever any read or mode-retrieving
  /// method is executed for any of the GP pins, both <see cref="LastFetchedValue"/>
  /// and <see cref="LastFetchedMode"/> are updated for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the current <see cref="CurrentFunction"/> of the pin is not
  /// <see cref="GpFunction.Gpio"/>.
  /// </exception>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="Read(CancellationToken)"/>
  /// <seealso cref="ReadAsync(CancellationToken)"/>
  [CLSCompliant(false)]
  public PinValue LastFetchedValue => gpio.GetLastFetchedValue(gp: Index);

  /// <summary>
  /// Gets the I/O direction (mode) of the GP pin obtained during the
  /// last successful communication.
  /// </summary>
  /// <value>
  /// The <see cref="PinMode"/> captured from the device at the time of
  /// the last fetch operation.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call
  /// <see cref="IGpioController.ReadAsync(System.Threading.CancellationToken)"/> or
  /// <see cref="IGpioController.GetModeAsync(System.Threading.CancellationToken)"/>
  /// (and their synchronous counterparts).
  /// </para>
  /// <para>
  /// The MCP2221A retrieves the logic levels and I/O modes for all GP pins (GP0-GP3)
  /// simultaneously in a single command. Therefore, whenever any read or mode-retrieving
  /// method is executed for any of the GP pins, both <see cref="LastFetchedValue"/>
  /// and <see cref="LastFetchedMode"/> are updated for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when the current <see cref="CurrentFunction"/> of the pin is not
  /// <see cref="GpFunction.Gpio"/>.
  /// </exception>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="GetMode(CancellationToken)"/>
  /// <seealso cref="GetModeAsync(CancellationToken)"/>
  [CLSCompliant(false)]
  public PinMode LastFetchedMode => gpio.GetLastFetchedDirection(gp: Index);

  private protected GpController(Mcp2221AGpioDriver gpio)
  {
    this.gpio = gpio;
  }

  /// <summary>
  /// Indicates whether the specified GP function is supported by this pin.
  /// </summary>
  /// <param name="function">
  /// The <see cref="GpFunction"/> to check for support.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the pin supports the specified <paramref name="function"/>;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// MCP2221A pins (GP0 to GP3) have different hardware capabilities. While all pins
  /// support GPIO, certain functions like ADC, DAC, Clock Output, or Interrupt-on-Change
  /// are exclusive to specific pins.
  /// </para>
  /// <para>
  /// Use this method to verify compatibility before calling configuration methods such as
  /// <see cref="IGpControllerGroup.ConfigureAllGpSettings"/> to avoid a
  /// <see cref="NotSupportedException"/>.
  /// </para>
  /// </remarks>
  public bool IsFunctionSupported(GpFunction function)
    => GetDesignationForFunction(function).HasValue;

  private protected abstract GpDesignation? GetDesignationForFunction(GpFunction function);

  internal GpDesignation GetDesignationForFunctionOrThrow(GpFunction function)
    => GetDesignationForFunction(function)
      ?? throw new NotSupportedException(
        message: $"{PinName} does not support the GP function '{function}'."
      );

  private protected ValueTask ConfigureGpDesignationAsync(
    GpDesignation gpDesignation,
    PinMode? gpioDirection = null,
    PinValue? gpioInitialValue = null,
    CancellationToken cancellationToken = default
  )
    => gpio.ConfigureGpDesignationAsync(
      gp: Index,
      gpDesignation: gpDesignation,
      gpioDirection: gpioDirection,
      gpioValue: gpioInitialValue,
      cancellationToken: cancellationToken
    );

  private protected void ConfigureGpDesignation(
    GpDesignation gpDesignation,
    PinMode? gpioDirection = null,
    PinValue? gpioInitialValue = null,
    CancellationToken cancellationToken = default
  )
    => gpio.ConfigureGpDesignation(
      gp: Index,
      gpDesignation: gpDesignation,
      gpioDirection: gpioDirection,
      gpioValue: gpioInitialValue,
      cancellationToken: cancellationToken
    );
}
