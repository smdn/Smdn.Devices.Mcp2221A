// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// The exception that is thrown when a read operation from
/// an I2C target device fails.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when the MCP2221/MCP2221A internal
/// I2C engine fails to retrieve the requested number of bytes
/// from a target device, even if the initial addressing was
/// successful.
/// </para>
/// <para>
/// For failures specifically caused by a missing acknowledgment
/// from the target, <see cref="I2cNackException"/> is thrown instead.
/// </para>
/// </remarks>
public class I2cReadException : I2cCommandException {
  private const string DefaultMessage = "The requested I2C read operation failed.";

  public I2cReadException()
    : base(DefaultMessage)
  {
  }

  public I2cReadException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  public I2cReadException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

  public I2cReadException(I2cAddress address, string? message)
    : base(address, message ?? DefaultMessage)
  {
  }

  public I2cReadException(I2cAddress address, string? message, Exception? innerException)
    : base(address, message ?? DefaultMessage, innerException)
  {
  }
}
