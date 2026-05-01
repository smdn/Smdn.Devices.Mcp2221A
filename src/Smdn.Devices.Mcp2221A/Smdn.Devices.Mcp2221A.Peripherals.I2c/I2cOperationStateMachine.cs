// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Transport;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

internal partial class I2cOperationStateMachine {
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  private static OperationState ThrowUnexpectedResponseException(string command, I2cAddress? address, byte response)
  {
    if (address.HasValue)
      throw new I2cCommandException(address.Value, $"The '{command}' command returned no successful response. (Code: 0x{response:X2})");
    else
      Mcp2221ACommandException.ThrowNoSuccessfulResponse(command, response);

    return default;
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  private static OperationState ThrowI2cErrorException(I2cAddress address, byte? stateValue, string message, string? i2cEngineState = null)
    => throw new I2cCommandException(address, $"{message} (0x{stateValue?.ToString("X2", provider: null) ?? "??"}, {i2cEngineState ?? "(details not available)"})");

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  private static OperationState ThrowUnknownEngineStateException(I2cAddress address, byte? stateValue, string? i2cEngineState = null)
    => throw new I2cCommandException(address, $"unknown I2C engine state (0x{stateValue?.ToString("X2", provider: null) ?? "??"}, {i2cEngineState ?? "(details not available)"})");

  private enum OperationState {
    Initial,
    CancelAndRetry,
    Continue,
    AdvanceToNextStep,
  }

  public I2cOperationStateMachine(ILogger? logger, byte busSpeedDivider)
  {
    this.logger = logger;
    this.busSpeedDivider = busSpeedDivider;
  }

  [LoggerMessage(
    EventId = 220,
    EventName = "I2C Communication Speed",
    Level = LogLevel.Warning,
    Message = "New I2C/SMBus communication speed might not be considered. (Code: {Response}, Speed divider: {BusSpeedDivider})"
  )]
  private static partial void LogWarningI2cCommunicationSpeedMightNotBeConsidered(
    ILogger logger,
    byte busSpeedDivider,
    string response
  );

  [LoggerMessage(
    EventId = 221,
    EventName = "I2C Engine State",
    Level = LogLevel.Debug,
    Message = "Engine state: {EngineState}"
  )]
  private static partial void LogDebugI2cEngineState(ILogger logger, I2cEngineState engineState);

  [LoggerMessage(
    EventId = 222,
    EventName = "I2C Operation State Machine",
    Level = LogLevel.Debug,
    Message = "{NewState}"
  )]
  private static partial void LogDebugTransitState(ILogger logger, string newState);

  private readonly ILogger? logger;
  private readonly byte busSpeedDivider;
  private OperationState operationState;
  private I2cEngineState lastEngineState;
  public int ReadLength { get; private set; } = -1;

#pragma warning disable CS0164, CA1508
  public IEnumerable<(
    Mcp2221AConstructCommandWithSpanAction<I2cAddress> ConstructCommand,
    Mcp2221AParseResponseWithSpanFunc<I2cAddress, bool> ParseResponse
  )>
  IterateWriteCommands()
  {
    operationState = OperationState.Initial;
    lastEngineState = default;

  WRITE_INIT:
    if (logger is { } logWriteInit && logWriteInit.IsEnabled(LogLevel.Debug))
      LogDebugTransitState(logWriteInit, "WRITE_INIT");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

    if (operationState == OperationState.CancelAndRetry)
      goto WRITE_INIT;

#pragma warning disable IDE0055
  WRITE_DO:
    if (logger is { } logWriteDo && logWriteDo.IsEnabled(LogLevel.Debug))
      LogDebugTransitState(logWriteDo, "WRITE_DO");
#pragma warning restore IDE0055

    yield return (
      WriteConstructCommand,
      WriteParseResponse
    );

  WRITE_STATUS:
    if (logger is { } logWriteStatus && logWriteStatus.IsEnabled(LogLevel.Debug))
      LogDebugTransitState(logWriteStatus, "WRITE_STATUS");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

    if (operationState == OperationState.Continue)
      goto WRITE_STATUS;
    if (lastEngineState.RequestedTransferLength == 0)
      yield break;
  }
#pragma warning restore CS0164, CA1508

#pragma warning disable CS0164, CA1508
  public IEnumerable<(
    Mcp2221AConstructCommandWithSpanAction<I2cAddress> ConstructCommand,
    Mcp2221AParseResponseWithSpanFunc<I2cAddress, bool> ParseResponse
  )>
  IterateReadCommands()
  {
    operationState = OperationState.Initial;
    lastEngineState = default;
    ReadLength = -1;

  READ_INIT:
    if (logger is { } logReadInit && logReadInit.IsEnabled(LogLevel.Debug))
      LogDebugTransitState(logReadInit, "READ_INIT");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

    if (operationState == OperationState.CancelAndRetry)
      goto READ_INIT;

#pragma warning disable IDE0055
  READ_DO:
    if (logger is { } logReadDo && logReadDo.IsEnabled(LogLevel.Debug))
      LogDebugTransitState(logReadDo, "READ_DO");
#pragma warning restore IDE0055

    yield return (
      ReadConstructCommand,
      ReadParseResponse
    );

#if false
    if (lastEngineState.RequestedTransferLength == 0)
      yield break; // no need to do READ_GET
    if (canAdvanceToNextStep)
      goto READ_GET;
#endif

  READ_GET:
    yield return (
      GetConstructCommand,
      GetParseResponse
    );

    if (operationState == OperationState.Continue)
      goto READ_GET;

    yield break;
  }
#pragma warning disable CS0164, CA1508

  private static OperationState TransitStateOrThrowIfEngineStateInvalid(OperationState currentState, I2cAddress address, I2cEngineState engineState)
  {
    if (currentState == OperationState.Initial && (engineState.LineValueScl.IsLow || engineState.LineValueSda.IsLow))
      ThrowI2cErrorException(address, engineState.StateMachineStateValue, "The line level of SDA and/or SCL is invalid. Try pull-up the bus lines. It may need to be reset or powered off.", engineState.ToString());

    if (engineState.BusStatus == I2cEngineTransferStatus.MarkedForCancellation)
      ThrowI2cErrorException(address, engineState.StateMachineStateValue, "I2C engine has been marked for cancellation unexpectedly. It may need to be reset or powered off.", engineState.ToString());

    return engineState.StateMachineStateValue switch {
      /*
        * success / can advance
        */
      0x00 => OperationState.AdvanceToNextStep, // completed successfully?
      // 0x10: ACK? transferring?

      0x55 => OperationState.AdvanceToNextStep, // ACK? transferring?
      0x60 => OperationState.AdvanceToNextStep, // all buffer transferred?

      /*
        * still in progress / NACK reply
        */
      // 0x25: write operation still in progress?
      0x25 when currentState == OperationState.Initial => OperationState.CancelAndRetry, // remains previous operation state(?)
      0x25 when 0 < engineState.TimeoutValue => OperationState.Continue, // current operation in progress
      0x25 => throw new I2cNackException(address), // time out

      // 0x61: read operation still in progress?
      // 0x61 when (currentState == OperationState.Initial) => OperationState.CancelAndRetry, // issuing cancellation in this state will transit state to 0x62, and will be in state which cannot reset with command
      0x61 when 0 < engineState.TimeoutValue => OperationState.Continue, // current operation in progress
      // 0x62: has been marked for cancellation?
      0x62 => ThrowI2cErrorException(address, engineState.StateMachineStateValue, "I2C engine has been in invalid state. It may need to be reset or powered off.", engineState.ToString()),

      /*
        * exceptional / unknown states
        */
      _ => ThrowUnknownEngineStateException(address, engineState.StateMachineStateValue, engineState.ToString()),
    };
  }

  private void StatusConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
    comm[0] = 0x10; // Status/Set Parameters
    comm[1] = 0x00; // Don't care
    comm[2] = 0x00; // Cancel current I2C/SMBus transfer (0x00: No effect)

    if (operationState == OperationState.Initial) {
      comm[3] = 0x20; // Set I2C/SMBus communication speed
      comm[4] = busSpeedDivider; // The I2C/SMBus system clock divider
    }
    else if (operationState == OperationState.CancelAndRetry) {
      comm[2] = 0x10; // Cancel current I2C/SMBus transfer (0x10: Cancel transfer)
    }
  }

  private bool StatusParseResponse(
    ReadOnlySpan<byte> resp,
    Span<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
    if (resp[1] != 0x00) // Command completed successfully
      ThrowUnexpectedResponseException("STATUS/SET PARAMETERS", address, resp[1]);

    lastEngineState = I2cEngineState.Parse(resp);

    if (logger is { } l) {
      if (l.IsEnabled(LogLevel.Debug))
        LogDebugI2cEngineState(l, lastEngineState);

      if (operationState == OperationState.Initial && l.IsEnabled(LogLevel.Warning)) {
        var warnIfSpeedMightNotBeConsidered = resp[3] switch {
          0x00 => false, // No Set I2C/SMBus communication speed was issued
          0x20 => false, // The new I2C/SMBus communication speed is now considered
          0x21 => false, // The I2C/SMBus communication speed was not set (e.g., I2C transfer in progress)
          _ => true, // throw
        };

        if (warnIfSpeedMightNotBeConsidered)
          LogWarningI2cCommunicationSpeedMightNotBeConsidered(l, busSpeedDivider, $"0x{resp[3]:X2}");
      }
    }

    operationState = TransitStateOrThrowIfEngineStateInvalid(
      operationState,
      address,
      lastEngineState
    );

    return operationState == OperationState.AdvanceToNextStep;
  }

  private void WriteConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> data,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.5 I2C WRITE DATA
    comm[0] = 0x90; // I2C Write Data
    comm[1] = (byte)(data.Length & 0x00FF); // Requested I2C transfer length - low byte
    comm[2] = (byte)(data.Length >> 8); // Requested I2C transfer length - high byte
    comm[3] = address.GetWriteAddress(); // I2C device address to communicate with
    data.CopyTo(comm.Slice(4));
  }

  private bool WriteParseResponse(
    ReadOnlySpan<byte> resp,
    Span<byte> data,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.5 I2C WRITE DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      _ => ThrowUnexpectedResponseException("I2C WRITE DATA", address, resp[1]),
    };

    return operationState == OperationState.AdvanceToNextStep;
  }

  private void ReadConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.8 I2C READ DATA
    comm[0] = 0x91; // I2C Read Data
    comm[1] = (byte)(buffer.Length & 0x00FF); // Requested I2C transfer length - low byte
    comm[2] = (byte)(buffer.Length >> 8); // Requested I2C transfer length - high byte
    comm[3] = address.GetReadAddress(); // I2C device address to communicate with
  }

  private bool ReadParseResponse(
    ReadOnlySpan<byte> resp,
    Span<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.8 I2C READ DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      _ => ThrowUnexpectedResponseException("I2C READ DATA", address, resp[1]),
    };

    return operationState == OperationState.AdvanceToNextStep;
  }

  private void GetConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.10 I2C READ DATA - GET I2C DATA
    comm[0] = 0x40; // I2C Read Data - Get I2C Data
    comm[1] = (byte)(buffer.Length & 0x00FF); // [??] Requested I2C transfer length - low byte
    comm[2] = (byte)(buffer.Length >> 8); // [??] Requested I2C transfer length - high byte
  }

  private bool GetParseResponse(
    ReadOnlySpan<byte> resp,
    Span<byte> buffer,
    I2cAddress address
  )
  {
    // [MCP2221A] 3.1.10 I2C READ DATA - GET I2C DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      0x41 => throw new I2cReadException(address),
      _ => ThrowUnexpectedResponseException("I2C READ DATA - GET I2C DATA", address, resp[1]),
    };

    if (operationState == OperationState.AdvanceToNextStep) {
      ReadLength = resp[3] switch {
        _ when resp[3] is >= 0 and <= 60 => resp[3],
        127 => throw new I2cCommandException("error has occurred on reading"),
        _ => throw new I2cCommandException(address, $"unexpected data length ({resp[3]})"),
      };

      resp.Slice(4, ReadLength).CopyTo(buffer);
    }

    return operationState == OperationState.AdvanceToNextStep;
  }
}
