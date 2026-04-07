// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController {
#pragma warning restore IDE0040

  /// <inheritdoc cref="Reset(CancellationToken)"/>
  /// <summary>
  /// Asynchronously sends a 'RESET CHIP' command to the MCP2221/MCP2221A
  /// to perform a hardware reset.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  public async ValueTask ResetAsync(
    CancellationToken cancellationToken = default
  )
  {
    await Transceiver.ResetChipAsync(cancellationToken).ConfigureAwait(false);

    // Performing a reset will invalidate the current USB HID endpoint,
    // and subsequent communication will no longer be possible;
    // therefore, this instance should also be disposed.
    await DisposeAsync().ConfigureAwait(false);
  }

  /// <summary>
  /// Sends a 'RESET CHIP' command to the MCP2221/MCP2221A to perform a hardware reset.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <remarks>
  /// <para>
  /// When this method is called, the MCP2221/MCP2221A performs an internal reset,
  /// which immediately invalidates the current USB HID endpoint. As a result, the
  /// underlying communication channel is closed, and any subsequent attempts to
  /// send or receive commands through this instance will fail.
  /// </para>
  /// <para>
  /// To reflect this hardware state, this method automatically calls <see cref="Dispose()"/>
  /// after transmitting the reset command. If you need to communicate with the
  /// device again after the reset is complete, you must discover the device and
  /// create a new <see cref="Mcp2221AController"/> instance.
  /// </para>
  /// </remarks>
  /// <seealso cref="Dispose()"/>
  /// <seealso cref="DisposeAsync()"/>
  public void Reset(
    CancellationToken cancellationToken = default
  )
  {
    Transceiver.ResetChip(cancellationToken);

    // Performing a reset will invalidate the current USB HID endpoint,
    // and subsequent communication will no longer be possible;
    // therefore, this instance should also be disposed.
    Dispose();
  }
}
