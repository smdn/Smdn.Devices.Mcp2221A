// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Provides a logical group of GP (General Purpose) pin controllers for the MCP2221A.
/// </summary>
/// <remarks>
/// <para>
/// This interface treats the four GP pins (<c>GP0</c> to <c>GP3</c>) as a single
/// unit, providing efficient bulk operations that leverage the MCP2221A's hardware
/// capabilities. It extends <see cref="IReadOnlyList{GpController}"/>, allowing
/// indexed access to each GP pin controller.
/// </para>
/// <para>
/// Use this interface when you need to perform atomic status synchronization
/// (via <see cref="FetchGpioStates"/>) or when you want to access specific pin
/// controllers through the <see cref="Gp0"/> to <see cref="Gp3"/> properties.
/// </para>
/// </remarks>
[CLSCompliant(false)]
public interface IGpControllerGroup : IReadOnlyList<GpController> {
  /// <summary>
  /// Gets the controller for the <c>GP0</c> pin.
  /// </summary>
  /// <value>
  /// A <see cref="Gp0Controller"/> instance representing the <c>GP0</c> pin.
  /// </value>
  /// <seealso cref="Gp0Controller"/>
  Gp0Controller Gp0 { get; }

  /// <summary>
  /// Gets the controller for the <c>GP1</c> pin.
  /// </summary>
  /// <value>
  /// A <see cref="Gp1Controller"/> instance representing the <c>GP1</c> pin.
  /// </value>
  /// <seealso cref="Gp1Controller"/>
  Gp1Controller Gp1 { get; }

  /// <summary>
  /// Gets the controller for the <c>GP2</c> pin.
  /// </summary>
  /// <value>
  /// A <see cref="Gp2Controller"/> instance representing the <c>GP2</c> pin.
  /// </value>
  /// <seealso cref="Gp2Controller"/>
  Gp2Controller Gp2 { get; }

  /// <summary>
  /// Gets the controller for the <c>GP3</c> pin.
  /// </summary>
  /// <value>
  /// A <see cref="Gp3Controller"/> instance representing the <c>GP3</c> pin.
  /// </value>
  /// <seealso cref="Gp3Controller"/>
  Gp3Controller Gp3 { get; }

  /// <summary>
  /// Gets the current voltage reference source configured for the ADC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the ADC module
  /// of the MCP2221A. Changing the reference source on one GP pin will
  /// affect all other GP pins configured as ADC inputs.
  /// </remarks>
  VoltageReferenceSource CurrentAdcReferenceSource { get; }

  /// <summary>
  /// Configures all settings for GP pins (GP0-GP3), including functions, modes,
  /// and initial values, in a single communication.
  /// </summary>
  /// <param name="gp0Function">
  /// The function to assign to GP0. If <see langword="null"/>,
  /// the current function is maintained.
  /// </param>
  /// <param name="gp0Mode">
  /// The mode for GP0. This is applied only when GP0 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp0InitialValue">
  /// The initial value for GP0. This is applied only when GP0 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp1Function">
  /// The function to assign to GP1. If <see langword="null"/>,
  /// the current function is maintained.
  /// </param>
  /// <param name="gp1Mode">
  /// The mode for GP1. This is applied only when GP1 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp1InitialValue">
  /// The initial value for GP1. This is applied only when GP1 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp2Function">
  /// The function to assign to GP2. If <see langword="null"/>,
  /// the current function is maintained.
  /// </param>
  /// <param name="gp2Mode">
  /// The mode for GP2. This is applied only when GP2 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp2InitialValue">
  /// The initial value for GP2. This is applied only when GP2 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp3Function">
  /// The function to assign to GP3. If <see langword="null"/>,
  /// the current function is maintained.
  /// </param>
  /// <param name="gp3Mode">
  /// The mode for GP3. This is applied only when GP3 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="gp3InitialValue">
  /// The initial value for GP3. This is applied only when GP3 is
  /// set to <see cref="GpFunction.Gpio"/>. Otherwise, it is ignored.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <remarks>
  /// <para>
  /// If a parameter is <see langword="null"/>, the corresponding setting
  /// in the device is preserved. Parameters <paramref name="gp0InitialValue"/>-<paramref name="gp3InitialValue"/>
  /// and <paramref name="gp0Mode"/>-<paramref name="gp3Mode"/> are only effective
  /// when the pin is configured as <see cref="GpFunction.Gpio"/>. For other
  /// functions, these parameters behave as if <see langword="null"/> was specified.
  /// </para>
  /// <para>
  /// If all parameters are <see langword="null"/>, the method returns immediately
  /// without transmitting any command to the device.
  /// </para>
  /// </remarks>
  /// <exception cref="NotSupportedException">
  /// Thrown when a function is not supported by the specified GP pin.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when any of the GP pins (GP0-GP3) have <see cref="GpController.IsUsedByGpioController"/>
  /// set to <see langword="true"/>.
  /// </exception>
  void ConfigureAllGpSettings(
    GpFunction? gp0Function = default,
    PinMode? gp0Mode = default,
    PinValue? gp0InitialValue = default,
    GpFunction? gp1Function = default,
    PinMode? gp1Mode = default,
    PinValue? gp1InitialValue = default,
    GpFunction? gp2Function = default,
    PinMode? gp2Mode = default,
    PinValue? gp2InitialValue = default,
    GpFunction? gp3Function = default,
    PinMode? gp3Mode = default,
    PinValue? gp3InitialValue = default,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously configures all settings for GP pins (GP0-GP3)
  /// in a single communication.
  /// </summary>
  /// <inheritdoc cref="ConfigureAllGpSettings(GpFunction?, PinMode?, PinValue?, GpFunction?, PinMode?, PinValue?, GpFunction?, PinMode?, PinValue?, GpFunction?, PinMode?, PinValue?, CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ConfigureAllGpSettingsAsync(
    GpFunction? gp0Function = default,
    PinMode? gp0Mode = default,
    PinValue? gp0InitialValue = default,
    GpFunction? gp1Function = default,
    PinMode? gp1Mode = default,
    PinValue? gp1InitialValue = default,
    GpFunction? gp2Function = default,
    PinMode? gp2Mode = default,
    PinValue? gp2InitialValue = default,
    GpFunction? gp3Function = default,
    PinMode? gp3Mode = default,
    PinValue? gp3InitialValue = default,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetches the current digital logic levels and I/O modes for all
  /// GP pins (GP0-GP3) from the device in a single communication and
  /// updates the internal cache.
  /// </summary>
  /// <param name="pinValuePairs">
  /// A span of <see cref="PinValuePair"/> structures.
  /// The <see cref="PinValuePair.PinNumber"/> must be set to the GP index
  /// (0 to 3) to specify which pins to retrieve. Upon return, the
  /// <see cref="PinValuePair.PinValue"/> fields are updated with the latest
  /// logic levels.
  /// This parameter can be empty if only modes are needed.
  /// </param>
  /// <param name="pinModePairs">
  /// A span of <see cref="PinModePair"/> structures.
  /// The <see cref="PinModePair.PinNumber"/> must be set to the GP index
  /// (0 to 3) to specify which pins to retrieve. Upon return, the
  /// <see cref="PinModePair.PinMode"/> fields are updated with the latest
  /// I/O modes.
  /// This parameter can be empty if only logic levels are needed.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <remarks>
  /// <para>
  /// Both <paramref name="pinValuePairs"/> and <paramref name="pinModePairs"/>
  /// can be empty if you only intend to update the internal cache or retrieve
  /// only one type of state.
  /// </para>
  /// <para>
  /// This method performs an atomic fetch operation using the MCP2221A's
  /// <c>GET GPIO VALUES</c> command. It synchronizes the internal state for all pins
  /// simultaneously, updating both <see cref="GpController.LastUpdatedValue"/> and
  /// <see cref="GpController.CurrentMode"/> across all GP0-GP3 controllers.
  /// </para>
  /// <para>
  /// Compared to calling <see cref="IGpioController.Read"/> or <see cref="IGpioController.GetMode"/>
  /// for each pin individually, this method reduces communication overhead by
  /// consolidating multiple requests into a single USB HID transaction.
  /// When you need to monitor the status of multiple pins, it is recommended to
  /// call this method once and then reference the updated <c>LastFetched</c>
  /// properties for efficiency.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when any of the specified pins are not currently configured as GPIO,
  /// or when an invalid GP index (outside the range 0-3) is encountered
  /// while populating the results from the device response.
  /// </exception>
  /// <seealso cref="GpController.CurrentMode"/>
  /// <seealso cref="GpController.LastUpdatedValue"/>
  /// <seealso cref="IGpioController.GetMode"/>
  /// <seealso cref="IGpioController.Read"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.12 GET GPIO VALUES
  /// </seealso>
  void FetchGpioStates(
    Span<PinValuePair> pinValuePairs,
    Span<PinModePair> pinModePairs,
    CancellationToken cancellationToken = default
  );

  /// <inheritdoc cref="FetchGpioStates(Span{PinValuePair}, Span{PinModePair}, CancellationToken)"/>
  /// <summary>
  /// Asynchronously fetches the current digital logic levels and I/O modes
  /// for all GP pins (GP0-GP3) from the device in a single communication and
  /// updates the internal cache.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <seealso cref="GpController.CurrentMode"/>
  /// <seealso cref="GpController.LastUpdatedValue"/>
  /// <seealso cref="IGpioController.GetModeAsync"/>
  /// <seealso cref="IGpioController.ReadAsync"/>
  ValueTask FetchGpioStatesAsync(
    Memory<PinValuePair> pinValuePairs,
    Memory<PinModePair> pinModePairs,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Applies the specified digital logic levels and I/O modes to the
  /// GP pins (GP0-GP3) in a single communication.
  /// </summary>
  /// <param name="pinValuePairs">
  /// A read-only span of <see cref="PinValuePair"/> structures containing
  /// the logic levels to be applied. The <see cref="PinValuePair.PinNumber"/>
  /// specifies the target GP index (0 to 3). This parameter can be empty if
  /// no logic level changes are required.
  /// </param>
  /// <param name="pinModePairs">
  /// A read-only span of <see cref="PinModePair"/> structures containing
  /// the I/O modes to be applied. The <see cref="PinModePair.PinNumber"/>
  /// specifies the target GP index (0 to 3). This parameter can be empty if
  /// no mode changes are required.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <remarks>
  /// <para>
  /// This method performs an atomic update operation using the MCP2221A's
  /// <c>SET GPIO OUTPUT VALUES</c> command. It sends the specified values and
  /// modes to the device while maintaining the consistency of the internal cache.
  /// </para>
  /// <para>
  /// If both <paramref name="pinValuePairs"/> and <paramref name="pinModePairs"/>
  /// are empty, the command is still transmitted using the current internal
  /// cached states. This can be used to explicitly refresh the physical device
  /// state with the latest known values.
  /// </para>
  /// <para>
  /// Compared to calling <see cref="IGpioController.Write"/> or <see cref="IGpioController.SetMode"/>
  /// for each pin individually, this method reduces communication overhead by
  /// consolidating multiple updates into a single USB HID transaction.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when any of the specified pins are not currently configured as GPIO,
  /// or when an invalid GP index (outside the range 0-3) is detected before
  /// transmitting the command to the device.
  /// </exception>
  /// <seealso cref="IGpioController.SetMode"/>
  /// <seealso cref="IGpioController.Write"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
  /// </seealso>
  /// <exception cref="InvalidOperationException">
  /// Thrown when any of the GP pins (GP0-GP3) have <see cref="GpController.IsUsedByGpioController"/>
  /// set to <see langword="true"/>.
  /// </exception>
  void ApplyGpioStates(
    ReadOnlySpan<PinValuePair> pinValuePairs,
    ReadOnlySpan<PinModePair> pinModePairs,
    CancellationToken cancellationToken = default
  );

  /// <inheritdoc cref="ApplyGpioStates(ReadOnlySpan{PinValuePair}, ReadOnlySpan{PinModePair}, CancellationToken)"/>
  /// <summary>
  /// Asynchronously applies the specified digital logic levels and I/O modes
  /// to the GP pins (GP0-GP3) in a single communication.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <seealso cref="IGpioController.SetModeAsync"/>
  /// <seealso cref="IGpioController.WriteAsync"/>
  ValueTask ApplyGpioStatesAsync(
    ReadOnlyMemory<PinValuePair> pinValuePairs,
    ReadOnlyMemory<PinModePair> pinModePairs,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Fetches the current 10-bit raw input values from all ADC channels
  /// (ADC1, ADC2, and ADC3) by sending a command to the device and updates
  /// the internal cache.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// An <see cref="AdcAllChannelSample"/> containing the fetched 10-bit raw values.
  /// </returns>
  /// <exception cref="OperationCanceledException">
  /// The operation was canceled.
  /// </exception>
  AdcAllChannelSample FetchAdcRawValues(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously fetches the current 10-bit raw input values from all ADC channels
  /// (ADC1, ADC2, and ADC3) by sending a command to the device and updates
  /// the internal cache.
  /// </summary>
  /// <inheritdoc cref="FetchAdcRawValues(CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask{AdcAllChannelSample}"/> representing the asynchronous
  /// operation, containing the fetched 10-bit raw values.
  /// </returns>
  ValueTask<AdcAllChannelSample> FetchAdcRawValuesAsync(
    CancellationToken cancellationToken = default
  );
}
