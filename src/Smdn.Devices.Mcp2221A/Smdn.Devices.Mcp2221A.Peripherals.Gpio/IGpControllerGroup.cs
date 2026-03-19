// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

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
  /// A <see cref="Gp0Controller"/> instance representing the <c>GP1</c> pin.
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
  /// simultaneously, updating both <see cref="GpController.LastFetchedValue"/> and
  /// <see cref="GpController.LastFetchedMode"/> across all GP0-GP3 controllers.
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
  /// <seealso cref="GpController.LastFetchedMode"/>
  /// <seealso cref="GpController.LastFetchedValue"/>
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
  /// <seealso cref="GpController.LastFetchedMode"/>
  /// <seealso cref="GpController.LastFetchedValue"/>
  /// <seealso cref="IGpioController.GetModeAsync"/>
  /// <seealso cref="IGpioController.ReadAsync"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.12 GET GPIO VALUES
  /// </seealso>
  ValueTask FetchGpioStatesAsync(
    Memory<PinValuePair> pinValuePairs,
    Memory<PinModePair> pinModePairs,
    CancellationToken cancellationToken = default
  );
}
