// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.Transport;

#pragma warning disable IDE0040
partial class Mcp2221ATransceiver {
#pragma warning restore IDE0040
  private static string ConvertByteSequenceToString(
    ReadOnlySpan<byte> sequence,
    int? actualLength = default
  )
  {
    if (actualLength is int actualSequenceLength) {
      if (actualSequenceLength <= 0)
        return string.Empty;

      if (actualSequenceLength < sequence.Length)
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

  [LoggerMessage(
    EventId = 1,
    EventName = "Sent Command",
    Level = LogLevel.Trace,
    Message = "> {CommandSequenceString}"
  )]
  private static partial void LogTraceCommand(ILogger logger, string commandSequenceString);

  [LoggerMessage(
    EventId = 2,
    EventName = "Received Response",
    Level = LogLevel.Trace,
    Message = "< {ResponseSequenceString}"
  )]
  private static partial void LogTraceResponse(ILogger logger, string responseSequenceString);
}
