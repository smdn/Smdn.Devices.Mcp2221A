// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

#pragma warning disable IDE0040
partial class Mcp2221AI2cBus {
#pragma warning restore IDE0040
  /// <summary>
  /// A constant represents the maximum size of data that can be
  /// transferred over the I2C bus.
  /// </summary>
  public const int MaxBlockLength = 0xFFFF;

  private const int MaxTransferLengthPerCommand = 64 - 4;

  [LoggerMessage(
    EventId = 200,
    EventName = "I2C Write Error",
    Level = LogLevel.Error,
    Message = "Write failed ({Address}, {Reason})"
  )]
  private static partial void LogErrorI2cWriteFailed(ILogger logger, I2cAddress address, string reason);

  [LoggerMessage(
    EventId = 201,
    EventName = "I2C Read Error",
    Level = LogLevel.Error,
    Message = "Read failed ({Address}, {Reason})"
  )]
  private static partial void LogErrorI2cReadFailed(ILogger logger, I2cAddress address, string reason);

  [LoggerMessage(
    EventId = 202,
    EventName = "I2C Write Error",
    Level = LogLevel.Debug,
    Message = "Write {RequestedLength} bytes"
  )]
  private static partial void LogDebugI2cBeginWrite(ILogger logger, int requestedLength);

  [LoggerMessage(
    EventId = 203,
    EventName = "I2C Read Error",
    Level = LogLevel.Debug,
    Message = "Read {RequestedLength} bytes"
  )]
  private static partial void LogDebugI2cBeginRead(ILogger logger, int requestedLength);

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues writing command with 0-length in this case.
  ///   </para>
  /// </remarks>
  /// <seealso cref="II2cController.WriteAsync(I2cAddress,int,ReadOnlyMemory{byte},CancellationToken)"/>
  public async ValueTask WriteAsync(
    I2cAddress address,
    int transmissionSpeedInKbps,
    ReadOnlyMemory<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (MaxBlockLength < buffer.Length)
      throw new ArgumentException($"transfer length must be up to {MaxBlockLength} bytes", nameof(buffer));

    Device.ThrowIfDisposed();

    var busSpeedDivider = Device.CalculateBusSpeedDividerOrThrow(
      transmissionSpeedInKbps,
      nameof(transmissionSpeedInKbps)
    );

    cancellationToken.ThrowIfCancellationRequested();

    using var scope = logger?.BeginScope($"I2C Write to {address}");

    try {
      if (logger is { } l && l.IsEnabled(LogLevel.Debug))
        LogDebugI2cBeginWrite(l, buffer.Length);

      for (; ; ) {
        var lengthToTransfer = Math.Min(buffer.Length, MaxTransferLengthPerCommand);
        var stateMachine = new I2cOperationStateMachine(logger, busSpeedDivider);

        foreach (var (constructCommand, parseResponse) in stateMachine.IterateWriteCommands()) {
          using (await Device.Transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
            await Device.Transceiver.CommandAsync(
              commandInput: buffer.Slice(0, lengthToTransfer),
              responseOutput: default,
              arg: address,
              cancellationToken: cancellationToken,
              constructCommand: constructCommand,
              parseResponse: parseResponse
            ).ConfigureAwait(false);
          }
        }

        buffer = buffer.Slice(lengthToTransfer);

        if (buffer.IsEmpty)
          break;
      }
    }
    catch (Exception ex) {
      if (logger is { } l && l.IsEnabled(LogLevel.Error))
        LogErrorI2cWriteFailed(l, address, ex.Message);

      if (ex is not I2cNackException)
        await CancelTransferAsync(address, ex).ConfigureAwait(false);

      throw;
    }
  }

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues writing command with 0-length in this case.
  ///   </para>
  /// </remarks>
  /// <seealso cref="II2cController.Write(I2cAddress,int,ReadOnlySpan{byte},CancellationToken)"/>
  public void Write(
    I2cAddress address,
    int transmissionSpeedInKbps,
    ReadOnlySpan<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (MaxBlockLength < buffer.Length)
      throw new ArgumentException($"transfer length must be up to {MaxBlockLength} bytes", nameof(buffer));

    Device.ThrowIfDisposed();

    var busSpeedDivider = Device.CalculateBusSpeedDividerOrThrow(
      transmissionSpeedInKbps,
      nameof(transmissionSpeedInKbps)
    );

    cancellationToken.ThrowIfCancellationRequested();

    using var scope = logger?.BeginScope($"I2C Write to {address}");

    try {
      if (logger is { } l && l.IsEnabled(LogLevel.Debug))
        LogDebugI2cBeginWrite(l, buffer.Length);

      for (; ; ) {
        var lengthToTransfer = Math.Min(buffer.Length, MaxTransferLengthPerCommand);
        var stateMachine = new I2cOperationStateMachine(logger, busSpeedDivider);

        foreach (var (constructCommand, parseResponse) in stateMachine.IterateWriteCommands()) {
          using (Device.Transceiver.EnterCommandTransaction(cancellationToken)) {
            Device.Transceiver.Command(
              commandInput: buffer.Slice(0, lengthToTransfer),
              responseOutput: default,
              arg: address,
              cancellationToken: cancellationToken,
              constructCommand: constructCommand,
              parseResponse: parseResponse
            );
          }
        }

        buffer = buffer.Slice(lengthToTransfer);

        if (buffer.IsEmpty)
          break;
      }
    }
    catch (Exception ex) {
      if (logger is { } l && l.IsEnabled(LogLevel.Error))
        LogErrorI2cWriteFailed(l, address, ex.Message);

      if (ex is not I2cNackException)
        CancelTransfer(address, ex);

      throw;
    }
  }

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues reading command with 0-length in this case.
  ///   </para>
  /// </remarks>
  /// <seealso cref="II2cController.ReadAsync(I2cAddress,int,Memory{byte},CancellationToken)"/>
  public async ValueTask<int> ReadAsync(
    I2cAddress address,
    int transmissionSpeedInKbps,
    Memory<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (MaxBlockLength < buffer.Length)
      throw new ArgumentException($"transfer length must be up to {MaxBlockLength} bytes", nameof(buffer));

    Device.ThrowIfDisposed();

    var busSpeedDivider = Device.CalculateBusSpeedDividerOrThrow(
      transmissionSpeedInKbps,
      nameof(transmissionSpeedInKbps)
    );

    cancellationToken.ThrowIfCancellationRequested();

    using var scope = logger?.BeginScope($"I2C Read from {address}");

    try {
      if (logger is { } l && l.IsEnabled(LogLevel.Debug))
        LogDebugI2cBeginRead(l, buffer.Length);

      var totalReadLength = 0;

      for (; ; ) {
        var lengthToTransfer = Math.Min(buffer.Length, MaxTransferLengthPerCommand);
        var stateMachine = new I2cOperationStateMachine(logger, busSpeedDivider);

        foreach (var (constructCommand, parseResponse) in stateMachine.IterateReadCommands()) {
          using (await Device.Transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
            await Device.Transceiver.CommandAsync(
              commandInput: buffer.Slice(0, lengthToTransfer),
              responseOutput: buffer.Slice(0, lengthToTransfer),
              arg: address,
              cancellationToken: cancellationToken,
              constructCommand: constructCommand,
              parseResponse: parseResponse
            ).ConfigureAwait(false);
          }
        }

        if (stateMachine.ReadLength < 0)
          break;

        buffer = buffer.Slice(stateMachine.ReadLength);

        totalReadLength += stateMachine.ReadLength;

        if (stateMachine.ReadLength < lengthToTransfer)
          break;
        if (buffer.IsEmpty)
          break;
      }

      return totalReadLength;
    }
    catch (Exception ex) {
      if (logger is { } l && l.IsEnabled(LogLevel.Error))
        LogErrorI2cReadFailed(l, address, ex.Message);

      if (ex is not I2cReadException)
        await CancelTransferAsync(address, ex).ConfigureAwait(false);

      throw;
    }
  }

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues reading command with 0-length in this case.
  ///   </para>
  /// </remarks>
  /// <seealso cref="II2cController.Read(I2cAddress,int,Span{byte},CancellationToken)"/>
  public int Read(
    I2cAddress address,
    int transmissionSpeedInKbps,
    Span<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (MaxBlockLength < buffer.Length)
      throw new ArgumentException($"transfer length must be up to {MaxBlockLength} bytes", nameof(buffer));

    Device.ThrowIfDisposed();

    var busSpeedDivider = Device.CalculateBusSpeedDividerOrThrow(
      transmissionSpeedInKbps,
      nameof(transmissionSpeedInKbps)
    );

    cancellationToken.ThrowIfCancellationRequested();

    using var scope = logger?.BeginScope($"I2C Read from {address}");

    try {
      if (logger is { } l && l.IsEnabled(LogLevel.Debug))
        LogDebugI2cBeginRead(l, buffer.Length);

      var totalReadLength = 0;

      for (; ; ) {
        var lengthToTransfer = Math.Min(buffer.Length, MaxTransferLengthPerCommand);
        var stateMachine = new I2cOperationStateMachine(logger, busSpeedDivider);

        foreach (var (constructCommand, parseResponse) in stateMachine.IterateReadCommands()) {
          using (Device.Transceiver.EnterCommandTransaction(cancellationToken)) {
            Device.Transceiver.Command(
              commandInput: buffer.Slice(0, lengthToTransfer),
              responseOutput: buffer.Slice(0, lengthToTransfer),
              arg: address,
              cancellationToken: cancellationToken,
              constructCommand: constructCommand,
              parseResponse: parseResponse
            );
          }
        }

        if (stateMachine.ReadLength < 0)
          break;

        buffer = buffer.Slice(stateMachine.ReadLength);

        totalReadLength += stateMachine.ReadLength;

        if (stateMachine.ReadLength < lengthToTransfer)
          break;
        if (buffer.IsEmpty)
          break;
      }

      return totalReadLength;
    }
    catch (Exception ex) {
      if (logger is { } l && l.IsEnabled(LogLevel.Error))
        LogErrorI2cReadFailed(l, address, ex.Message);

      if (ex is not I2cReadException)
        CancelTransfer(address, ex);

      throw;
    }
  }
}
