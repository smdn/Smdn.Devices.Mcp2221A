// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// The exception that is thrown when the hardware or firmware revision
/// retrieved from an MCP2221/MCP2221A device is identified as unsupported
/// during the initialization process.
/// </summary>
public class Mcp2221ANotSupportedException : NotSupportedException {
  private const string DefaultMessage = "The requested MCP2221/MCP2221A is a device with an unsupported hardware revision and/or firmware revision.";

  /// <inheritdoc/>
  public Mcp2221ANotSupportedException()
    : base(DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public Mcp2221ANotSupportedException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public Mcp2221ANotSupportedException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }
}
