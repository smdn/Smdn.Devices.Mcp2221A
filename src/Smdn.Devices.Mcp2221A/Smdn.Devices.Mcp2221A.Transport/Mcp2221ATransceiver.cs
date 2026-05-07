// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A.Transport;

internal sealed partial class Mcp2221ATransceiver : IDisposable {
  private const int LengthOfReportId = 1;

  private const int CommandLength = 64;
  private const int ResponseLength = 64;
  private const int CommandReportLength = LengthOfReportId + CommandLength;
  private const int ResponseReportLength = LengthOfReportId + ResponseLength;

  private static void VerifyResponseReport(
    ReadOnlySpan<byte> command,
    ReadOnlySpan<byte> response,
    int actualResponseReportLength
  )
  {
    if (actualResponseReportLength != ResponseReportLength)
      throw new Mcp2221ACommandException($"The length of the response report received exceeds or does not reach the expected length; expected {ResponseReportLength} bytes, but was actually {actualResponseReportLength} bytes.");

    var commandCode = command[0];
    var commandCodeEcho = response[0];

    if (commandCode != commandCodeEcho)
      throw new Mcp2221ACommandException($"The command echo in the received response does not match; command code '0x{commandCode:X2}' was expected, but the actual command echo was 0x{commandCodeEcho:X2}.");
  }

  internal readonly struct CommandTransaction(SemaphoreSlim semaphore) : IDisposable {
    public void Dispose()
      => semaphore.Release();
  }

  /*
   * instance members
   */
  private IUsbHidEndPoint? endPoint;
  public IUsbHidEndPoint EndPoint => endPoint ?? throw new ObjectDisposedException(GetType().FullName);

  private SemaphoreSlim? transactionSemaphore = new(1, 1);

  private readonly ILogger? logger;

  private bool HasResetChipCommandIssued { get; set; }

  public Mcp2221ATransceiver(
    IUsbHidEndPoint endPoint,
    ILogger? logger
  )
  {
    this.endPoint = endPoint ?? throw new ArgumentNullException(nameof(endPoint));
    this.logger = logger;
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(endPoint))]
  [MemberNotNull(nameof(transactionSemaphore))]
#endif
  internal void ThrowIfDisposed()
  {
    if (endPoint is null || transactionSemaphore is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public void Dispose()
  {
    endPoint?.Dispose();
    endPoint = null;

    transactionSemaphore?.Dispose();
    transactionSemaphore = null;
  }

  public async ValueTask DisposeAsync()
  {
    if (endPoint is not null) {
      await endPoint.DisposeAsync().ConfigureAwait(false);
      endPoint = null;
    }

    transactionSemaphore?.Dispose(); // SemaphoreSlim does not implement IAsyncDisposable
    transactionSemaphore = null;
  }

  public async ValueTask<CommandTransaction> EnterCommandTransactionAsync(
    CancellationToken cancellationToken
  )
  {
    ThrowIfDisposed();

    await transactionSemaphore
#if !SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
      !
#endif
      .WaitAsync(cancellationToken)
      .ConfigureAwait(false);

    return new(transactionSemaphore);
  }

  public CommandTransaction EnterCommandTransaction(
    CancellationToken cancellationToken
  )
  {
    ThrowIfDisposed();

    transactionSemaphore
#if !SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
      !
#endif
      .Wait(cancellationToken);

    return new(transactionSemaphore);
  }

  public async ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> commandInput,
    Memory<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
  {
    if (constructCommand is null)
      throw new ArgumentNullException(nameof(constructCommand));
    if (parseResponse is null)
      throw new ArgumentNullException(nameof(parseResponse));

    ThrowIfDisposed();

    var commandReport = ArrayPool<byte>.Shared.Rent(CommandReportLength);
    var responseReport = ArrayPool<byte>.Shared.Rent(ResponseReportLength);

    try {
      var commandReportMemory = commandReport.AsMemory(0, CommandReportLength);
      var responseReportMemory = responseReport.AsMemory(0, ResponseReportLength);

      commandReportMemory.Span.Clear();
      // commandReportMemory.Span[0] = 0x00; // Report ID

      cancellationToken.ThrowIfCancellationRequested();

      var commandSpan = commandReportMemory.Slice(LengthOfReportId, CommandLength).Span; // span except first byte (report IN)

      constructCommand(
        command: commandSpan,
        commandInput: commandInput.Span,
        arg: arg
      );

      if (logger is { } lc && lc.IsEnabled(LogLevel.Trace)) {
#pragma warning disable CA1873
        LogTraceCommand(lc, ConvertByteSequenceToString(commandSpan));
#pragma warning restore CA1873
      }

      try {
        await
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
          endPoint
#else
          endPoint!
#endif
          .WriteAsync(
            commandReportMemory,
            cancellationToken
          ).ConfigureAwait(false);
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        throw new Mcp2221ACommandException("Failed to send the USB HID command report to MCP2221/MCP2221A.", ex);
      }

      if (HasResetChipCommandIssued) {
        // Performing a reset will invalidate the current USB HID endpoint,
        // and subsequent communication will no longer be possible;
        // therefore, this instance should also be disposed.
        await DisposeAsync().ConfigureAwait(false);
        return default!;
      }

      int readReportLength = default;

      try {
        readReportLength = await endPoint.ReadAsync(
          responseReportMemory,
          cancellationToken
        ).ConfigureAwait(false);
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        throw new Mcp2221ACommandException("Failed to receive a USB HID response report from MCP2221/MCP2221A.", ex);
      }

      // recreate and reassign Span/ReadOnlySpan since they cannot cross await boundaries
      commandSpan = commandReportMemory.Slice(LengthOfReportId, CommandLength).Span; // span except first byte (report IN)

      var responseSpan = responseReportMemory.Slice(LengthOfReportId, ResponseLength).Span; // span except first byte (report OUT)

      if (logger is { } lr && lr.IsEnabled(LogLevel.Trace)) {
#pragma warning disable CA1873
        LogTraceResponse(lr, ConvertByteSequenceToString(responseSpan, readReportLength - LengthOfReportId));
#pragma warning restore CA1873
      }

      VerifyResponseReport(commandSpan, responseSpan, readReportLength);

      return parseResponse(
        response: responseSpan,
        responseOutput: responseOutput.Span,
        arg: arg
      );
    }
    finally {
      ArrayPool<byte>.Shared.Return(commandReport);
      ArrayPool<byte>.Shared.Return(responseReport);
    }
  }

  public TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> commandInput,
    Span<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
  {
    if (constructCommand is null)
      throw new ArgumentNullException(nameof(constructCommand));
    if (parseResponse is null)
      throw new ArgumentNullException(nameof(parseResponse));

    ThrowIfDisposed();

    Span<byte> commandReport = stackalloc byte[CommandReportLength];
    Span<byte> responseReport = stackalloc byte[ResponseReportLength];

    commandReport.Clear();
    // commandReport[0] = 0x00; // Report ID

    cancellationToken.ThrowIfCancellationRequested();

    var commandSpan = commandReport.Slice(LengthOfReportId, CommandLength); // span except first byte (report IN)
    var responseSpan = responseReport.Slice(LengthOfReportId, ResponseLength); // span except first byte (report OUT)

    constructCommand(
      command: commandSpan,
      commandInput: commandInput,
      arg: arg
    );

    if (logger is { } lc && lc.IsEnabled(LogLevel.Trace)) {
#pragma warning disable CA1873
      LogTraceCommand(lc, ConvertByteSequenceToString(commandSpan));
#pragma warning restore CA1873
    }

    try {
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
      endPoint
#else
      endPoint!
#endif
      .Write(
        commandReport,
        cancellationToken
      );
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      throw new Mcp2221ACommandException("Failed to send the USB HID command report to MCP2221/MCP2221A.", ex);
    }

    if (HasResetChipCommandIssued) {
      // Performing a reset will invalidate the current USB HID endpoint,
      // and subsequent communication will no longer be possible;
      // therefore, this instance should also be disposed.
      Dispose();
      return default!;
    }

    int readReportLength = default;

    try {
      readReportLength = endPoint.Read(
        responseReport,
        cancellationToken
      );
    }
    catch (OperationCanceledException) {
      throw;
    }
    catch (Exception ex) {
      throw new Mcp2221ACommandException("Failed to receive a USB HID response report from MCP2221/MCP2221A.", ex);
    }

    if (logger is { } lr && lr.IsEnabled(LogLevel.Trace)) {
#pragma warning disable CA1873
      LogTraceResponse(lr, ConvertByteSequenceToString(responseSpan, readReportLength - LengthOfReportId));
#pragma warning restore CA1873
    }

    VerifyResponseReport(commandSpan, responseSpan, readReportLength);

    return parseResponse(
      response: responseSpan,
      responseOutput: responseOutput,
      arg: arg
    );
  }
}
