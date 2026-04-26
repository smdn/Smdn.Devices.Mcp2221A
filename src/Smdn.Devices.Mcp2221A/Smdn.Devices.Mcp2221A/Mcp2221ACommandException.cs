// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif

namespace Smdn.Devices.Mcp2221A;

public class Mcp2221ACommandException : InvalidOperationException {
  private const string DefaultMessage = "The command to the MCP2221/MCP2221A failed.";

  public Mcp2221ACommandException()
    : base(DefaultMessage)
  {
  }

  public Mcp2221ACommandException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  public Mcp2221ACommandException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  internal static void ThrowNoSuccessfulResponse(
    string command,
    byte response,
    Exception? innerException = null
  )
    => throw new Mcp2221ACommandException(
      message: $"The '{command}' command returned no successful response. (Code: 0x{response:X2})",
      innerException: innerException
    );
}
