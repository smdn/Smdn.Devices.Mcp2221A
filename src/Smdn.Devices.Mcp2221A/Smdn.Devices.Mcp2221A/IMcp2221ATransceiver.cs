// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

internal interface IMcp2221ATransceiver {
  ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  );

  TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  );
}
