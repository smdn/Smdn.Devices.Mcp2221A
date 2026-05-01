// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Transport;

internal interface IMcp2221ATransceiver {
  ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> commandInput,
    Memory<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  );

  TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> commandInput,
    Span<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  );
}
