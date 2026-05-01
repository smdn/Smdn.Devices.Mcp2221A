// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

#pragma warning disable IDE0040
partial class Mcp2221AI2cBus {
#pragma warning restore IDE0040
  private static class CancelTransferCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(
      Span<byte> comm,
      (I2cAddress Address, Exception? ExceptionCauseOfCancellation) args
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      comm[0] = 0x10; // Status/Set Parameters
      comm[1] = 0x00; // Don't care
      comm[2] = 0x10; // Cancel current I2C/SMBus transfer
    }

    public static I2cEngineState ParseResponse(
      ReadOnlySpan<byte> resp,
      (I2cAddress Address, Exception? ExceptionCauseOfCancellation) args
    )
    {
      if (resp[1] != 0x00) // Command completed successfully
        Mcp2221ACommandException.ThrowNoSuccessfulResponse("STATUS/SET PARAMETERS", resp[1], args.ExceptionCauseOfCancellation);

      var state = I2cEngineState.Parse(resp);
      var isBusStatusDefined =
#if SYSTEM_ENUM_ISDEFINED_OF_TENUM
        Enum.IsDefined<I2cEngineTransferStatus>(state.BusStatus);
#else
        Enum.IsDefined(typeof(I2cEngineTransferStatus), state.BusStatus);
#endif

      if (!isBusStatusDefined) {
        throw new I2cCommandException(
          args.Address,
          $"unexpected response while transfer cancellation (0x{resp[2]:X2})",
          args.ExceptionCauseOfCancellation
        );
      }

      return state;
    }
  }

  [LoggerMessage(
    EventId = 210,
    EventName = "I2C Cancel",
    Level = LogLevel.Warning,
    Message = "Cancel transfer (Engine state: {EngineState})"
  )]
  private static partial void LogWarningI2cCancelTransfer(ILogger logger, I2cEngineState engineState);

  ValueTask II2cController.CancelTransferAsync(I2cAddress address)
    => CancelTransferAsync(address, exceptionCauseOfCancellation: null);

  private async ValueTask CancelTransferAsync(I2cAddress address, Exception? exceptionCauseOfCancellation)
  {
    using (await Device.Transceiver.EnterCommandTransactionAsync(default).ConfigureAwait(false)) {
      var engineState = await Device.Transceiver.CommandAsync(
        arg: (address, exceptionCauseOfCancellation),
        cancellationToken: default,
        constructCommand: CancelTransferCommand.ConstructCommand,
        parseResponse: CancelTransferCommand.ParseResponse
      ).ConfigureAwait(false);

      if (logger is { } l && l.IsEnabled(LogLevel.Warning))
        LogWarningI2cCancelTransfer(l, engineState);
    }
  }

  void II2cController.CancelTransfer(I2cAddress address)
    => CancelTransfer(address, exceptionCauseOfCancellation: null);

  private void CancelTransfer(I2cAddress address, Exception? exceptionCauseOfCancellation)
  {
    using (Device.Transceiver.EnterCommandTransaction(default)) {
      var engineState = Device.Transceiver.Command(
        arg: (address, exceptionCauseOfCancellation),
        cancellationToken: default,
        constructCommand: CancelTransferCommand.ConstructCommand,
        parseResponse: CancelTransferCommand.ParseResponse
      );

      if (logger is { } l && l.IsEnabled(LogLevel.Warning))
        LogWarningI2cCancelTransfer(l, engineState);
    }
  }
}
