// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
public partial class Mcp2221AController : IMcp2221ATransceiver {
#pragma warning restore IDE0040
  TResponse IMcp2221ATransceiver.Command<TArg, TResponse>(
    ReadOnlySpan<byte> commandInput,
    Span<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.Command(
      commandInput: commandInput,
      responseOutput: responseOutput,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );

  internal TResponse Command<TArg, TResponse>(
    ReadOnlySpan<byte> commandInput,
    Span<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.Command(
      commandInput: commandInput,
      responseOutput: responseOutput,
      arg: arg,
      cancellationToken: cancellationToken,
      constructCommand: constructCommand,
      parseResponse: parseResponse
    );

  ValueTask<TResponse> IMcp2221ATransceiver.CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> commandInput,
    Memory<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.CommandAsync(
      commandInput: commandInput,
      responseOutput: responseOutput,
      arg: arg,
      constructCommand: constructCommand,
      parseResponse: parseResponse,
      cancellationToken: cancellationToken
    );

  internal ValueTask<TResponse> CommandAsync<TArg, TResponse>(
    ReadOnlyMemory<byte> commandInput,
    Memory<byte> responseOutput,
    TArg arg,
    Mcp2221AConstructCommandWithSpanAction<TArg> constructCommand,
    Mcp2221AParseResponseWithSpanFunc<TArg, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Transceiver.CommandAsync(
      commandInput: commandInput,
      responseOutput: responseOutput,
      arg: arg,
      constructCommand: constructCommand,
      parseResponse: parseResponse,
      cancellationToken: cancellationToken
    );
}
