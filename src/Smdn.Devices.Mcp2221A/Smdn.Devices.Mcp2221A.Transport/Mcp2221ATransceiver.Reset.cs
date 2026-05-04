// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Transport;

#pragma warning disable IDE0040
partial class Mcp2221ATransceiver {
#pragma warning restore IDE0040
  private static class ResetChipCommand {
    public static void ConstructCommand(
      Span<byte> comm,
      Mcp2221ATransceiver self
    )
    {
      // [MCP2221A] 3.1.15 RESET CHIP
      comm[0] = 0x70; // Reset Chip
      comm[1] = 0xAB;
      comm[2] = 0xCD;
      comm[3] = 0xEF;
      // [4-64] Don't care

      self.HasResetChipCommandIssued = true;
    }

    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      Mcp2221ATransceiver self
    )
      => throw new Mcp2221ACommandException("No response to the reset command is defined.");
  }

  public async ValueTask ResetChipAsync(
    CancellationToken cancellationToken = default
  )
  {
    // If the execution of the RESET CHIP command completes successfully,
    // this instance calls the Dispose method on its own, and since the
    // synchronization primitive is also disposed of at that point,
    // attempting to utilize automatic disposal via the `using` statement
    // here will cause an ObjectDisposedException.
    var transaction = await EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false);

    try {
      await CommandAsync(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: ResetChipCommand.ConstructCommand,
        parseResponse: ResetChipCommand.ParseResponse
      ).ConfigureAwait(false);
    }
    catch (ObjectDisposedException) {
      throw;
    }
    catch {
      // If the command execution fails due to an exception, the instance
      // remains available, so the synchronization primitive must be
      // released in that case.
      transaction.Dispose();
      throw;
    }
  }

  public void ResetChip(
    CancellationToken cancellationToken = default
  )
  {
    // If the execution of the RESET CHIP command completes successfully,
    // this instance calls the Dispose method on its own, and since the
    // synchronization primitive is also disposed of at that point,
    // attempting to utilize automatic disposal via the `using` statement
    // here will cause an ObjectDisposedException.
    var transaction = EnterCommandTransaction(cancellationToken);

    try {
      Command(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: ResetChipCommand.ConstructCommand,
        parseResponse: ResetChipCommand.ParseResponse
      );
    }
    catch (ObjectDisposedException) {
      throw;
    }
    catch {
      // If the command execution fails due to an exception, the instance
      // remains available, so the synchronization primitive must be
      // released in that case.
      transaction.Dispose();
      throw;
    }
  }
}
