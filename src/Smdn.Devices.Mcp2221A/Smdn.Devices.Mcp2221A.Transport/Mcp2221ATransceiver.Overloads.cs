// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Transport;

#pragma warning disable IDE0040
partial class Mcp2221ATransceiver {
#pragma warning restore IDE0040
  public TResponse Command<TResponse>(
    Mcp2221AConstructCommandAction<None> constructCommand,
    Mcp2221AParseResponseFunc<None, TResponse> parseResponse,
    CancellationToken cancellationToken
  )
    => Command(
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
    => CommandAsync(
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
    => Command(
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
    => CommandAsync(
      commandInput: default,
      responseOutput: default,
      arg: (Argument: arg, ConstructCommand: constructCommand, ParseResponse: parseResponse),
      constructCommand: static (command, _, arg) => arg.ConstructCommand(command, arg.Argument),
      parseResponse: static (response, _, arg) => arg.ParseResponse(response, arg.Argument),
      cancellationToken: cancellationToken
    );
}
