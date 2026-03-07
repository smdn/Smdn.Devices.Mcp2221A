// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A;

public class Mcp2221ACommandException : InvalidOperationException {
  public Mcp2221ACommandException() : base("command failed") { }
  public Mcp2221ACommandException(string? message) : base(message) { }
  public Mcp2221ACommandException(string? message, Exception? innerException) : base(message, innerException) { }
}
