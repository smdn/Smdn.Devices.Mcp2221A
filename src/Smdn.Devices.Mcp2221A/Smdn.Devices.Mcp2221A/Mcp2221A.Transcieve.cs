// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
public partial class Mcp2221A {
#pragma warning restore IDE0040, CA1724
#pragma warning disable CA1068 // CA1068: CancellationToken parameters must come last
  internal TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    CancellationToken cancellationToken,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse
  )
#pragma warning restore CA1068
    => Transceiver.Command(
      userData: userData,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );

#pragma warning disable CA1068 // CA1068: CancellationToken parameters must come last
  internal ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    CancellationToken cancellationToken,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse
  )
#pragma warning restore CA1068
    => Transceiver.CommandAsync(
      userData: userData,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );
}
