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
  private protected Mcp2221AGpioDriver GpioDriver { get; }

  private protected Mcp2221ATransceiver Transceiver => GpioDriver.Transceiver;

  private protected GpDesignation CurrentGpDesignation => GpioDriver.GetCurrentGpDesignation(Index);

  /// <summary>
  /// Gets the logical index of General Purpose (GP) pin represented by the current instance,
  /// ranging from 0 to 3 (e.g., 0 for GP0, 1 for GP1).
  /// </summary>
  /// <remarks>
  /// This is a logical index used to identify the pin within the
  /// <see cref="Mcp2221AController.GpPins"/> collection. It does not refer to the physical
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
  /// <see cref="ConfigureAsGpio(PinMode?, PinValue?, CancellationToken)"/>,
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
  /// Gets the digital logic level of the GP pin as of the last communication
  /// that updated the device settings or retrieved its state.
  /// </summary>
  /// <value>
  /// The <see cref="PinValue"/> reflected from the device at the time of
  /// the last successful I/O operation.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes not only retrieval operations (e.g., <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or  or <see cref="Read"/>), but also configuration and write operations (e.g.,
  /// <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>, <see cref="IGpControllerGroup.ApplyGpioStates"/>,
  /// <see cref="ConfigureAsGpio"/> or <see cref="Write"/>).
  /// </para>
  /// <para>
  /// Since the MCP2221A handles logic levels for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's value or mode will refresh the
  /// <see cref="LastUpdatedValue"/> and <see cref="CurrentMode"/> for all pins at once.
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
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettingsAsync"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
  /// <seealso cref="Read(CancellationToken)"/>
  /// <seealso cref="ReadAsync(CancellationToken)"/>
  /// <seealso cref="Write(PinValue, CancellationToken)"/>
  /// <seealso cref="WriteAsync(PinValue, CancellationToken)"/>
  [CLSCompliant(false)]
  public PinValue LastUpdatedValue => GpioDriver.GetLastUpdatedValueOrThrow(gp: Index);

  /// <summary>
  /// Gets the current I/O direction (mode) of the GP pin.
  /// </summary>
  /// <value>
  /// The <see cref="PinMode"/> that is currently applied to the pin.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes both retrieval operations (e.g., <see cref="IGpControllerGroup.FetchGpioStates"/>)
  /// and configuration operations (e.g., <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// or <see cref="SetMode"/>).
  /// Since the mode is determined solely by these software operations and
  /// does not change spontaneously on the hardware, this property reflects
  /// the true current state of the pin's I/O direction.
  /// </para>
  /// <para>
  /// Since the MCP2221A handles I/O modes for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's mode or value will refresh the
  /// <see cref="CurrentMode"/> and <see cref="LastUpdatedValue"/> for all pins at once.
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
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettingsAsync"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
  /// <seealso cref="GetMode(CancellationToken)"/>
  /// <seealso cref="GetModeAsync(CancellationToken)"/>
  /// <seealso cref="SetMode(PinMode, CancellationToken)"/>
  /// <seealso cref="SetModeAsync(PinMode, CancellationToken)"/>
  [CLSCompliant(false)]
  public PinMode CurrentMode => GpioDriver.GetLastUpdatedDirectionOrThrow(gp: Index);

  /// <summary>
  /// Gets a value indicating whether the GP pin is currently being
  /// used by <see cref="Mcp2221AController.GpioController"/>.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if the pin has been opened by <see cref="GpioController.OpenPin(int)"/>
  /// and has not yet been closed by <see cref="GpioController.ClosePin(int)"/>;
  /// otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// <para>
  /// To prevent configuration conflicts and maintain hardware integrity,
  /// <see cref="GpController"/> restricts direct modifications to the GP pin
  /// while it is under the management of a <see cref="GpioController"/>.
  /// </para>
  /// <para>
  /// When this property is <see langword="true"/>, any attempt to change the pin's
  /// function (e.g., via <see cref="ConfigureAsGpio"/>), mode (e.g., via <see cref="SetMode"/>),
  /// or logic level (e.g., via <see cref="Write"/>) will result in an
  /// <see cref="InvalidOperationException"/>.
  /// </para>
  /// <para>
  /// Internally, this safeguards all operations involving the MCP2221A commands
  /// 'SET SRAM SETTINGS' and 'SET GPIO OUTPUT VALUES' for this specific pin.
  /// </para>
  /// </remarks>
  /// <seealso cref="ConfigureAsGpio"/>
  /// <seealso cref="Write"/>
  /// <seealso cref="SetMode"/>
  public bool IsUsedByGpioController
    => GpioDriver.IsUsedByGpioController(gp: Index);

  private protected GpController(Mcp2221AGpioDriver gpioDriver)
  {
    GpioDriver = gpioDriver;
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
    => GpioDriver.ConfigureGpPinSettingsAsync(
      gpIndex: Index,
      arg: (gpDesignation, gpioDirection, gpioInitialValue),
      modifyGpPinSettings: ConfigureGpPinSettings,
      cancellationToken: cancellationToken
    );

  private protected void ConfigureGpDesignation(
    GpDesignation gpDesignation,
    PinMode? gpioDirection = null,
    PinValue? gpioInitialValue = null,
    CancellationToken cancellationToken = default
  )
    => GpioDriver.ConfigureGpPinSettings(
      gpIndex: Index,
      arg: (gpDesignation, gpioDirection, gpioInitialValue),
      modifyGpPinSettings: ConfigureGpPinSettings,
      cancellationToken: cancellationToken
    );

  private static void ConfigureGpPinSettings(
    SramSettings sramSettings,
    int gpIndex,
    (
      GpDesignation Designation,
      PinMode? Direction,
      PinValue? OutputValue
    ) arg
  )
    => sramSettings.ModifyGpSettings(
      gp: gpIndex,
      designation: arg.Designation,
      direction: arg.Direction,
      outputValue: arg.OutputValue
    );

  private protected ValueTask ConfigureAsDacAsyncCore(
    VoltageReferenceSource? voltageReferenceSource,
    int? initialOutputValue,
    CancellationToken cancellationToken = default
  )
    => GpioDriver.ConfigureGpPinSettingsAsync(
      gpIndex: Index,
      arg: (
        voltageReferenceSource,
        Mcp2221AGpioDriver.ThrowIfDacOutputValueOutOfRange(initialOutputValue, nameof(initialOutputValue))
      ),
      modifyGpPinSettings: ConfigureGpPinSettingsAsDac,
      cancellationToken: cancellationToken
    );

  private protected void ConfigureAsDacCore(
    VoltageReferenceSource? voltageReferenceSource,
    int? initialOutputValue,
    CancellationToken cancellationToken = default
  )
    => GpioDriver.ConfigureGpPinSettings(
      gpIndex: Index,
      arg: (
        voltageReferenceSource,
        Mcp2221AGpioDriver.ThrowIfDacOutputValueOutOfRange(initialOutputValue, nameof(initialOutputValue))
      ),
      modifyGpPinSettings: ConfigureGpPinSettingsAsDac,
      cancellationToken: cancellationToken
    );

  private static void ConfigureGpPinSettingsAsDac(
    SramSettings sramSettings,
    int gpIndex,
    (
      VoltageReferenceSource? VoltageReferenceSource,
      int? OutputValue
    ) arg
  )
    => sramSettings
      .ModifyGpSettings(
        gp: gpIndex,
        designation: GpDesignation.AlternateFunction1
      )
      .ModifyDacSettings(
        dacVoltageReferenceSource: arg.VoltageReferenceSource,
        dacOutputValue: arg.OutputValue
      );

  private protected ValueTask ConfigureAsAdcAsyncCore(
    VoltageReferenceSource? voltageReferenceSource,
    CancellationToken cancellationToken
  )
    => GpioDriver.ConfigureGpPinSettingsAsync(
      gpIndex: Index,
      arg: voltageReferenceSource,
      modifyGpPinSettings: ConfigureGpPinSettingsAsAdc,
      cancellationToken: cancellationToken
    );

  private protected void ConfigureAsAdcCore(
    VoltageReferenceSource? voltageReferenceSource,
    CancellationToken cancellationToken
  )
    => GpioDriver.ConfigureGpPinSettings(
      gpIndex: Index,
      arg: voltageReferenceSource,
      modifyGpPinSettings: ConfigureGpPinSettingsAsAdc,
      cancellationToken: cancellationToken
    );

  private static void ConfigureGpPinSettingsAsAdc(
    SramSettings sramSettings,
    int gpIndex,
    VoltageReferenceSource? voltageReferenceSource
  )
    => sramSettings
      .ModifyGpSettings(
        gp: gpIndex,
        designation: GpDesignation.AlternateFunction0
      )
      .ModifyAdcSettings(
        adcVoltageReferenceSource: voltageReferenceSource
      );
}
