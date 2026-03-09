// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

internal static class IMcp2221ATransceiverExtensions {
  extension(IMcp2221ATransceiver transceiver) {
    public TResponse Command<TResponse>(
      Mcp2221AConstructCommandAction<None> constructCommand,
      Mcp2221AParseResponseFunc<None, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.Command(
        userData: default,
        arg: default,
        cancellationToken: cancellationToken,
        constructCommand: constructCommand,
        parseResponse: parseResponse
      );

    public ValueTask<TResponse> CommandAsync<TResponse>(
      Mcp2221AConstructCommandAction<None> constructCommand,
      Mcp2221AParseResponseFunc<None, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.CommandAsync(
        userData: default,
        arg: default,
        constructCommand: constructCommand,
        parseResponse: parseResponse,
        cancellationToken: cancellationToken
      );

    public TResponse Command<TArg, TResponse>(
      TArg arg,
      Mcp2221AConstructCommandAction<TArg> constructCommand,
      Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.Command(
        userData: default,
        arg: arg,
        cancellationToken: cancellationToken,
        constructCommand: constructCommand,
        parseResponse: parseResponse
      );

    public ValueTask<TResponse> CommandAsync<TArg, TResponse>(
      TArg arg,
      Mcp2221AConstructCommandAction<TArg> constructCommand,
      Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.CommandAsync(
        userData: default,
        arg: arg,
        constructCommand: constructCommand,
        parseResponse: parseResponse,
        cancellationToken: cancellationToken
      );
  }
}
