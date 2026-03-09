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

internal sealed class Mcp2221ATransceiver : IDisposable {
  private const int CommandLength = 64;
  private const int ResponseLength = 64;
  private const int CommandReportLength = 1 + CommandLength;
  private const int ResponseReportLength = 1 + ResponseLength;

  private static readonly EventId EventIdCommand = new(1, "sent command");
  private static readonly EventId EventIdResponse = new(2, "received response");

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

  private static string ConvertByteSequenceToString(ReadOnlySpan<byte> sequence)
  {
    var buffer = ArrayPool<byte>.Shared.Rent(sequence.Length);

    try {
      sequence.CopyTo(buffer);

      return BitConverter.ToString(buffer, 0, sequence.Length);
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }

#pragma warning disable CA1068 // CA1068: CancellationToken parameters must come last
  public async ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    CancellationToken cancellationToken,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse
  )
#pragma warning restore CA1068
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

      constructCommand(
        commandReportMemory.Span.Slice(1, CommandLength),
        userData.Span,
        arg
      );

      logger?.LogTrace(EventIdCommand, "> " + ConvertByteSequenceToString(commandReportMemory.Span.Slice(1, CommandLength)));

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
      catch (Exception ex) {
        throw new Mcp2221ACommandException("writing command report failed", ex);
      }

      try {
        await endPoint.ReadAsync(
          responseReportMemory,
          cancellationToken
        ).ConfigureAwait(false);
      }
      catch (Exception ex) {
        throw new Mcp2221ACommandException("reading response report failed", ex);
      }

      logger?.LogTrace(EventIdResponse, "< " + ConvertByteSequenceToString(responseReportMemory.Span.Slice(1, ResponseLength)));

      if (commandReportMemory.Span[0] != responseReportMemory.Span[0])
        throw new Mcp2221ACommandException($"unexpected command echo (command code: {commandReportMemory.Span[0]:X2}, command code echo: {responseReportMemory.Span[0]:X2})");

      return parseResponse(
        responseReportMemory.Span.Slice(1, ResponseLength),
        arg
      );
    }
    finally {
      ArrayPool<byte>.Shared.Return(commandReport);
      ArrayPool<byte>.Shared.Return(responseReport);
    }
  }

#pragma warning disable CA1068 // CA1068: CancellationToken parameters must come last
  public TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    CancellationToken cancellationToken,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse
  )
#pragma warning restore CA1068
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

    constructCommand(
      commandReport.Slice(1),
      userData,
      arg
    );

    logger?.LogTrace(EventIdCommand, "> " + ConvertByteSequenceToString(commandReport.Slice(1)));

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
    catch (Exception ex) {
      throw new Mcp2221ACommandException("writing command report failed", ex);
    }

    try {
      endPoint.Read(
        responseReport,
        cancellationToken
      );
    }
    catch (Exception ex) {
      throw new Mcp2221ACommandException("reading response report failed", ex);
    }

    logger?.LogTrace(EventIdResponse, "< " + ConvertByteSequenceToString(responseReport.Slice(1)));

    if (commandReport[0] != responseReport[0])
      throw new Mcp2221ACommandException($"unexpected command echo (command code: {commandReport[0]:X2}, command code echo: {responseReport[0]:X2})");

    return parseResponse(
      responseReport.Slice(1),
      arg
    );
  }
}
