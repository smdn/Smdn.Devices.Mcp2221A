// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Transport;

namespace Smdn.Devices.Mcp2221A;

internal static class IMcp2221ATransceiverExtensions {
  extension(IMcp2221ATransceiver transceiver) {
    public TResponse Command<TResponse>(
      Mcp2221AConstructCommandAction<None> constructCommand,
      Mcp2221AParseResponseFunc<None, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.Command(
        commandInput: default,
        responseOutput: default,
        arg: (ConstructCommand: constructCommand, ParseResponse: parseResponse),
        constructCommand: static (command, _, arg) => arg.ConstructCommand(command, default),
        parseResponse: static (response, _, arg) => arg.ParseResponse(response, default),
        cancellationToken: cancellationToken
      );

    public ValueTask<TResponse> CommandAsync<TResponse>(
      Mcp2221AConstructCommandAction<None> constructCommand,
      Mcp2221AParseResponseFunc<None, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.CommandAsync(
        commandInput: default,
        responseOutput: default,
        arg: (ConstructCommand: constructCommand, ParseResponse: parseResponse),
        constructCommand: static (command, _, arg) => arg.ConstructCommand(command, default),
        parseResponse: static (response, _, arg) => arg.ParseResponse(response, default),
        cancellationToken: cancellationToken
      );

    public TResponse Command<TArg, TResponse>(
      TArg arg,
      Mcp2221AConstructCommandAction<TArg> constructCommand,
      Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.Command(
        commandInput: default,
        responseOutput: default,
        arg: (Argument: arg, ConstructCommand: constructCommand, ParseResponse: parseResponse),
        constructCommand: static (command, _, arg) => arg.ConstructCommand(command, arg.Argument),
        parseResponse: static (response, _, arg) => arg.ParseResponse(response, arg.Argument),
        cancellationToken: cancellationToken
      );

    public ValueTask<TResponse> CommandAsync<TArg, TResponse>(
      TArg arg,
      Mcp2221AConstructCommandAction<TArg> constructCommand,
      Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse,
      CancellationToken cancellationToken
    )
      => transceiver.CommandAsync(
        commandInput: default,
        responseOutput: default,
        arg: (Argument: arg, ConstructCommand: constructCommand, ParseResponse: parseResponse),
        constructCommand: static (command, _, arg) => arg.ConstructCommand(command, arg.Argument),
        parseResponse: static (response, _, arg) => arg.ParseResponse(response, arg.Argument),
        cancellationToken: cancellationToken
      );
  }
}
