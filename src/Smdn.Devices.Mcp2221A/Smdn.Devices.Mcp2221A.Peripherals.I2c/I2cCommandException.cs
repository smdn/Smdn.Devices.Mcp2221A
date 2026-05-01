// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// The exception that is thrown when an I2C-specific command fails
/// or when the I2C engine inside the MCP2221/MCP2221A enters an
/// error or unknown state.
/// </summary>
/// <remarks>
/// <para>
/// This exception is specifically for failures related to I2C operations
/// that are not covered by the general <see cref="Mcp2221ACommandException"/>.
/// It is thrown when the MCP2221/MCP2221A's internal I2C engine returns
/// an error response or reaches a state where it cannot continue the
/// requested operation.
/// </para>
/// <para>
/// The target I2C device address involved in the failed operation can be
/// retrieved from the <see cref="Address"/> property.
/// </para>
/// </remarks>
public class I2cCommandException : Mcp2221ACommandException {
  private const string DefaultMessage = "The requested I2C command failed.";

  /// <summary>
  /// Gets the <see cref="I2cAddress"/> of the I2C device that was
  /// the target of the failed operation.
  /// </summary>
  public I2cAddress Address { get; }

  /// <inheritdoc/>
  public I2cCommandException()
    : this(I2cAddress.Zero, DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public I2cCommandException(string? message)
    : this(I2cAddress.Zero, message)
  {
  }

  /// <inheritdoc/>
  public I2cCommandException(string? message, Exception? innerException)
    : this(I2cAddress.Zero, message, innerException)
  {
  }

  /// <inheritdoc cref="I2cCommandException(string?)"/>
  /// <param name="message">
  /// The message that describes the error.
  /// </param>
  /// <param name="address">
  /// The <see cref="I2cAddress"/> of the I2C device that was the target
  /// of the failed operation.
  /// </param>
  public I2cCommandException(I2cAddress address, string? message)
    : this(address, message, innerException: null)
  {
  }

  /// <inheritdoc cref="I2cCommandException(string?, Exception?)"/>
  /// <param name="address">
  /// The <see cref="I2cAddress"/> of the I2C device that was the target
  /// of the failed operation.
  /// </param>
  /// <param name="message">
  /// The message that describes the error.
  /// </param>
  /// <param name="innerException">
  /// The exception that is the cause of the current exception.
  /// </param>
  public I2cCommandException(I2cAddress address, string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
    Address = address;
  }
}
