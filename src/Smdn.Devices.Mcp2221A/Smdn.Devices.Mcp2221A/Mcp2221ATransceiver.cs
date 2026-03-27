// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

#pragma warning disable CA1848, CA1873, CA2254

using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

internal sealed class Mcp2221ATransceiver : IMcp2221ATransceiver, IDisposable {
  private const int LengthOfReportId = 1;

  private const int CommandLength = 64;
  private const int ResponseLength = 64;
  private const int CommandReportLength = LengthOfReportId + CommandLength;
  private const int ResponseReportLength = LengthOfReportId + ResponseLength;

  private static readonly EventId EventIdCommand = new(1, "sent command");
  private static readonly EventId EventIdResponse = new(2, "received response");

  private static void VerifyResponseReport(
    ReadOnlySpan<byte> command,
    ReadOnlySpan<byte> response,
    int actualResponseReportLength
  )
  {
    if (actualResponseReportLength != ResponseReportLength)
      throw new Mcp2221ACommandException($"The length of the received response report does not reach the expected length; expected {ResponseReportLength} bytes, but was actually {actualResponseReportLength} bytes.");

    var commandCode = command[0];
    var commandCodeEcho = response[0];

    if (commandCode != commandCodeEcho)
      throw new Mcp2221ACommandException($"The command echo in the received response does not match; command code '{commandCode:X2}' was expected, but the actual command echo was {commandCodeEcho:X2}.");
  }

  /*
   * instance members
   */
  private IUsbHidEndPoint? endPoint;
  public IUsbHidEndPoint EndPoint => endPoint ?? throw new ObjectDisposedException(GetType().FullName);

  private readonly ILogger? logger;

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
#endif
  internal void ThrowIfDisposed()
  {
    if (endPoint is null)
      throw new ObjectDisposedException(GetType().FullName);
  }

  public void Dispose()
  {
    endPoint?.Dispose();
    endPoint = null;
  }

  public async ValueTask DisposeAsync()
  {
    if (endPoint is not null) {
      await endPoint.DisposeAsync().ConfigureAwait(false);
      endPoint = null;
    }
  }

  private static string ConvertByteSequenceToString(
    ReadOnlySpan<byte> sequence,
    int? actualLength = default
  )
  {
    if (actualLength is int actualSequenceLength) {
      if (actualSequenceLength <= 0)
        return string.Empty;

      sequence = sequence.Slice(0, actualSequenceLength);
    }

    var buffer = ArrayPool<byte>.Shared.Rent(sequence.Length);

    try {
      sequence.CopyTo(buffer);

      return BitConverter.ToString(buffer, 0, sequence.Length);
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }

  public async ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
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

      // commandReportMemory[0] = 0x00; // report
      commandReportMemory.Span.Clear();

      cancellationToken.ThrowIfCancellationRequested();

      var commandSpan = commandReportMemory.Slice(LengthOfReportId, CommandLength).Span; // span except first byte (report IN)

      constructCommand(
        commandSpan,
        userData.Span,
        arg
      );

      logger?.LogTrace(EventIdCommand, "> " + ConvertByteSequenceToString(commandSpan));

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
        throw new Mcp2221ACommandException("writing command report failed", ex);
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
        throw new Mcp2221ACommandException("reading response report failed", ex);
      }

      // recreate and reassign Span/ReadOnlySpan since they cannot cross await boundaries
      commandSpan = commandReportMemory.Slice(LengthOfReportId, CommandLength).Span; // span except first byte (report IN)

      var responseSpan = responseReportMemory.Slice(LengthOfReportId, ResponseLength).Span; // span except first byte (report OUT)

      logger?.LogTrace(
        EventIdResponse,
        "< " + ConvertByteSequenceToString(responseSpan, readReportLength - LengthOfReportId)
      );

      VerifyResponseReport(commandSpan, responseSpan, readReportLength);

      return parseResponse(responseSpan, arg);
    }
    finally {
      ArrayPool<byte>.Shared.Return(commandReport);
      ArrayPool<byte>.Shared.Return(responseReport);
    }
  }

  public TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
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

    // commandReport[0] = 0x00; // report
    commandReport.Clear();

    cancellationToken.ThrowIfCancellationRequested();

    var commandSpan = commandReport.Slice(LengthOfReportId, CommandLength); // span except first byte (report IN)
    var responseSpan = responseReport.Slice(LengthOfReportId, ResponseLength); // span except first byte (report OUT)

    constructCommand(
      commandSpan,
      userData,
      arg
    );

    logger?.LogTrace(EventIdCommand, "> " + ConvertByteSequenceToString(commandSpan));

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
      throw new Mcp2221ACommandException("writing command report failed", ex);
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
      throw new Mcp2221ACommandException("reading response report failed", ex);
    }

    logger?.LogTrace(
      EventIdResponse,
      "< " + ConvertByteSequenceToString(responseSpan, readReportLength - LengthOfReportId)
    );

    VerifyResponseReport(commandSpan, responseSpan, readReportLength);

    return parseResponse(responseSpan, arg);
  }
}
