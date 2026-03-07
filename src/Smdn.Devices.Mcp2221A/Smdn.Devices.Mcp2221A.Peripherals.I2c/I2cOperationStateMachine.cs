// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1848, CA1873, CA2254

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

internal class I2cOperationStateMachine {
  [DoesNotReturn]
  private static OperationState ThrowUnexpectedResponseException(I2cAddress? address, byte response)
  {
    if (address.HasValue)
      throw new I2cCommandException(address.Value, $"unexpected response (0x{response:X2})");
    else
      throw new Mcp2221ACommandException($"unexpected response (0x{response:X2})");
  }

  [DoesNotReturn]
  private static OperationState ThrowI2cErrorException(I2cAddress address, byte? stateValue, string message, string? i2cEngineState = null)
    => throw new I2cCommandException(address, $"{message} (0x{stateValue?.ToString("X2", provider: null) ?? "??"}, {i2cEngineState ?? "(details not available)"})");

  [DoesNotReturn]
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

  private readonly ILogger? logger;
  private readonly byte busSpeedDivider;
  private OperationState operationState;
  private I2cEngineState lastEngineState;
  public int ReadLength { get; private set; } = -1;

#pragma warning disable CS0164
  public IEnumerable<(
    Mcp2221AConstructCommandAction<(I2cAddress Address, Memory<byte> Buffer)> ConstructCommand,
    Mcp2221AParseResponseFunc<(I2cAddress Address, Memory<byte> Buffer), bool> ParseResponse
  )>
  IterateWriteCommands()
  {
    operationState = OperationState.Initial;
    lastEngineState = default;

  WRITE_INIT:
    logger?.LogDebug(Mcp2221AI2cBus.EventIdI2cCommand, "WRITE_INIT");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

#pragma warning disable CA1508
    if (operationState == OperationState.CancelAndRetry)
      goto WRITE_INIT;
#pragma warning restore CA1508

#pragma warning disable IDE0055
  WRITE_DO:
    logger?.LogDebug(Mcp2221AI2cBus.EventIdI2cCommand, "WRITE_DO");
#pragma warning restore IDE0055

    yield return (
      WriteConstructCommand,
      WriteParseResponse
    );

  WRITE_STATUS:
    logger?.LogDebug(Mcp2221AI2cBus.EventIdI2cCommand, "WRITE_STATUS");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

#pragma warning disable CA1508
    if (operationState == OperationState.Continue)
      goto WRITE_STATUS;
#pragma warning restore CA1508
    if (lastEngineState.RequestedTransferLength == 0)
      yield break;
  }
#pragma warning restore CS0164

#pragma warning disable CS0164
  public IEnumerable<(
    Mcp2221AConstructCommandAction<(I2cAddress Address, Memory<byte> Buffer)> ConstructCommand,
    Mcp2221AParseResponseFunc<(I2cAddress Address, Memory<byte> Buffer), bool> ParseResponse
  )>
  IterateReadCommands()
  {
    operationState = OperationState.Initial;
    lastEngineState = default;
    ReadLength = -1;

  READ_INIT:
    logger?.LogDebug(Mcp2221AI2cBus.EventIdI2cCommand, "READ_INIT");

    yield return (
      StatusConstructCommand,
      StatusParseResponse
    );

#pragma warning disable CA1508
    if (operationState == OperationState.CancelAndRetry)
      goto READ_INIT;
#pragma warning restore CA1508

#pragma warning disable IDE0055
  READ_DO:
    logger?.LogDebug(Mcp2221AI2cBus.EventIdI2cCommand, "READ_DO");
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

#pragma warning disable CA1508
    if (operationState == OperationState.Continue)
      goto READ_GET;
#pragma warning restore CA1508

    yield break;
  }
#pragma warning disable CS0164

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
    ReadOnlySpan<byte> userData,
    (I2cAddress Address, Memory<byte> _) args
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
    (I2cAddress Address, Memory<byte> _) args)
  {
    var (address, _) = args;

    // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
    _ = resp[1] switch {
      0x00 => default, // Command completed successfully
      _ => ThrowUnexpectedResponseException(address, resp[1]),
    };

    lastEngineState = I2cEngineState.Parse(resp);

    logger?.LogInformation(Mcp2221AI2cBus.EventIdI2cEngineState, $"STATUS/SET PARAMETERS: {lastEngineState}");

    if (operationState == OperationState.Initial) {
      var isSpeedConsidered = resp[3] switch {
        0x00 => false, // No Set I2C/SMBus communication speed was issued
        0x20 => true, // The new I2C/SMBus communication speed is now considered
        // 0x21 => throw; // I2C transfer in progress
        _ => false, // throw
      };

      if (!isSpeedConsidered)
        logger?.LogWarning(Mcp2221AI2cBus.EventIdI2cEngineState, $"STATUS/SET PARAMETERS: new I2C/SMBus communication speed is not considered");
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
    ReadOnlySpan<byte> userData,
    (I2cAddress Address, Memory<byte> _) args
  )
  {
    var (address, _) = args;

    // [MCP2221A] 3.1.5 I2C WRITE DATA
    comm[0] = 0x90; // I2C Write Data
    comm[1] = (byte)(userData.Length & 0x00FF); // Requested I2C transfer length - low byte
    comm[2] = (byte)(userData.Length >> 8); // Requested I2C transfer length - high byte
    comm[3] = address.GetWriteAddress(); // I2C slave address to communicate with
    userData.CopyTo(comm.Slice(4));
  }

  private bool WriteParseResponse(
    ReadOnlySpan<byte> resp,
    (I2cAddress Address, Memory<byte> _) args
  )
  {
    var (address, _) = args;

    // [MCP2221A] 3.1.5 I2C WRITE DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      _ => ThrowUnexpectedResponseException(address, resp[1]),
    };

    return operationState == OperationState.AdvanceToNextStep;
  }

  private void ReadConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> userData,
    (I2cAddress Address, Memory<byte> Buffer) args
  )
  {
    var (address, _) = args;

    // [MCP2221A] 3.1.8 I2C READ DATA
    comm[0] = 0x91; // I2C Read Data
    comm[1] = (byte)(userData.Length & 0x00FF); // Requested I2C transfer length - low byte
    comm[2] = (byte)(userData.Length >> 8); // Requested I2C transfer length - high byte
    comm[3] = address.GetReadAddress(); // I2C slave address to communicate with
  }

  private bool ReadParseResponse(
    ReadOnlySpan<byte> resp,
    (I2cAddress Address, Memory<byte> Buffer) args
  )
  {
    var (address, _) = args;

    // [MCP2221A] 3.1.8 I2C READ DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      _ => ThrowUnexpectedResponseException(address, resp[1]),
    };

    return operationState == OperationState.AdvanceToNextStep;
  }

  private void GetConstructCommand(
    Span<byte> comm,
    ReadOnlySpan<byte> userData,
    (I2cAddress Address, Memory<byte> Buffer) args
  )
  {
    // [MCP2221A] 3.1.10 I2C READ DATA - GET I2C DATA
    comm[0] = 0x40; // I2C Read Data - Get I2C Data
    comm[1] = (byte)(userData.Length & 0x00FF); // [??] Requested I2C transfer length - low byte
    comm[2] = (byte)(userData.Length >> 8); // [??] Requested I2C transfer length - high byte
  }

  private bool GetParseResponse(
    ReadOnlySpan<byte> resp,
    (I2cAddress Address, Memory<byte> Buffer) args
  )
  {
    var (address, buffer) = args;

    // [MCP2221A] 3.1.10 I2C READ DATA - GET I2C DATA
    operationState = resp[1] switch {
      0x00 => OperationState.AdvanceToNextStep, // Command completed successfully
      0x01 => OperationState.Continue, // Command not completed (I2C engine is busy)
      0x41 => throw new I2cReadException(address, "can not read from I2C slave"), // Error reading the I2C slave data
      _ => ThrowUnexpectedResponseException(address, resp[1]),
    };

    if (operationState == OperationState.AdvanceToNextStep) {
      ReadLength = resp[3] switch {
        _ when resp[3] is >= 0 and <= 60 => resp[3],
        127 => throw new I2cCommandException("error has occurred on reading"),
        _ => throw new I2cCommandException(address, $"unexpected data length ({resp[3]})"),
      };

      resp.Slice(4, ReadLength).CopyTo(buffer.Span);
    }

    return operationState == OperationState.AdvanceToNextStep;
  }
}
