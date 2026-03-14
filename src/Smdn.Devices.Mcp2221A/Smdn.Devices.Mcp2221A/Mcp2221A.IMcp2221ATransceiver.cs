// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
public partial class Mcp2221A : IMcp2221ATransceiver {
#pragma warning restore IDE0040, CA1724
  TResponse IMcp2221ATransceiver.Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.Command(
      userData: userData,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );

  internal TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.Command(
      userData: userData,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );

  ValueTask<TResponse> IMcp2221ATransceiver.CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.CommandAsync(
      userData: userData,
      arg: arg,
      constructCommand: constructCommand,
      parseResponse: parseResponse,
      cancellationToken: cancellationToken
    );

  internal ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> userData,
    TArg arg,
    Mcp2221AConstructCommandAction<TArg> constructCommand,
    Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.CommandAsync(
      userData: userData,
      arg: arg,
      constructCommand: constructCommand,
      parseResponse: parseResponse,
      cancellationToken: cancellationToken
    );
}
