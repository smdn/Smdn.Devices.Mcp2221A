// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Formats.Binary;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Provides a base class for controlling a General Purpose (GP) pin of the MCP2221A.
/// </summary>
/// <remarks>
/// <para>
/// The MCP2221A has four GP pins, each represented by a specific subclass providing
/// access to its unique hardware functions:
/// </para>
/// <list type="table">
/// <listheader>
/// <term>Pin</term>
/// <description>Controller Class and Primary Special Functions</description>
/// </listheader>
/// <item>
/// <term>GP0</term>
/// <description>
/// <see cref="Gp0Controller"/>: UART RX LED, USB suspend indicator.
/// </description>
/// </item>
/// <item>
/// <term>GP1</term>
/// <description>
/// <see cref="Gp1Controller"/>: ADC1, UART TX LED, Reference clock output, Interrupt-on-Change.
/// </description>
/// </item>
/// <item>
/// <term>GP2</term>
/// <description>
/// <see cref="Gp2Controller"/>: ADC2, DAC1, USB configured indicator.
/// </description>
/// </item>
/// <item>
/// <term>GP3</term>
/// <description>
/// <see cref="Gp3Controller"/>: ADC3, DAC2, I2C Traffic LED, Clock Output.
/// </description>
/// </item>
/// </list>
/// <para>
/// All GP pins support standard GPIO (General Purpose Input/Output) functionality.
/// </para>
/// </remarks>
#pragma warning disable IDE0040
public abstract partial class GpController {
#pragma warning restore IDE0040
  private protected static NotSupportedException CreateUnsupportedGpDesignationException(
    int gpIndex,
    GpDesignation designation
  )
  {
    var formattedDesignation = BinaryFormat.IsBinaryFormatSpecifierSupported
      ? ((int)designation).ToString("B3", provider: null)
      : Convert.ToString((int)designation, 2).PadLeft(3, '0');

    return new(
      message: $"The value '0b{formattedDesignation}' of the GP{gpIndex} designation bits designates a function that is not supported or defined for GP{gpIndex}."
    );
  }

  private protected Mcp2221AGpioDriver GpioDriver { get; }

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
  /// Gets the logic level for the GP pin as defined in the device's
  /// SRAM configuration, regardless of the current pin function.
  /// </summary>
  /// <value>
  /// The output value that is intended to be applied when the pin is
  /// in output mode.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property represents the "desired" or "configured" state held
  /// in the device's SRAM. It is initialized from the Flash default
  /// settings upon power-up and is updated whenever configuration
  /// methods (such as <see cref="ConfigureAsGpio"/>) are called.
  /// </para>
  /// <para>
  /// Unlike <see cref="LastUpdatedValue"/>, which reflects the actual
  /// logic level captured during the most recent I/O operation,
  /// <see cref="ConfiguredOutputValue"/> remains unchanged by GPIO read/write
  /// commands. It serves as the baseline configuration for the pin.
  /// </para>
  /// <para>
  /// This property can be accessed even if the pin is currently assigned to
  /// a dedicated function (non-GPIO). Unlike <see cref="LastUpdatedValue"/>,
  /// it does not throw an <see cref="Mcp2221AConfigurationException"/> in
  /// such cases.
  /// </para>
  /// </remarks>
  /// <seealso cref="ConfigureAsGpio"/>
  /// <seealso cref="ConfigureAsGpioAsync"/>
  [CLSCompliant(false)]
  public PinValue ConfiguredOutputValue
    => GpioDriver.GetConfiguredOutputValue(Index);

  /// <summary>
  /// Gets the functional mode for the GP pin as defined in the device's
  /// SRAM configuration, regardless of the current pin function.
  /// </summary>
  /// <value>
  /// The <see cref="PinMode"/> that is intended to be applied.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property indicates whether the pin is logically defined as
  /// <see cref="PinMode.Input"/> or <see cref="PinMode.Output"/>. Similar to
  /// <see cref="ConfiguredOutputValue"/>, this value is loaded from Flash
  /// at startup and updated via explicit configuration commands.
  /// </para>
  /// <para>
  /// Note that this property represents the intended GPIO direction stored
  /// in SRAM, even if the pin is currently functioning as a dedicated
  /// peripheral (non-GPIO).
  /// </para>
  /// <para>
  /// While <see cref="CurrentMode"/> provides the mode status as reported by
  /// the device during runtime communication, <see cref="ConfiguredMode"/>
  /// represents the persistent setting that defines the pin's intended role.
  /// </para>
  /// <para>
  /// This property can be accessed even if the pin is currently assigned to
  /// a dedicated function (non-GPIO). Unlike <see cref="LastUpdatedValue"/>,
  /// it does not throw an <see cref="Mcp2221AConfigurationException"/> in
  /// such cases.
  /// </para>
  /// </remarks>
  /// <seealso cref="ConfigureAsGpio"/>
  /// <seealso cref="ConfigureAsGpioAsync"/>
  [CLSCompliant(false)]
  public PinMode ConfiguredMode
    => GpioDriver.GetConfiguredMode(Index);

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
        voltageReferenceSource: arg.VoltageReferenceSource,
        outputValue: arg.OutputValue
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
        voltageReferenceSource: voltageReferenceSource
      );
}
