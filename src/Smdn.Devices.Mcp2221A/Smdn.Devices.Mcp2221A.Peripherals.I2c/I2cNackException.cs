// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// The exception that is thrown when an I2C target device does not
/// return an acknowledgment (ACK) or returns a negative acknowledgment
/// (NACK) in response to an I2C command.
/// </summary>
/// /// <remarks>
/// <para>
/// This exception typically indicates that the target device at the
/// address specified by the <see cref="I2cCommandException.Address"/>
/// property did not respond.
/// </para>
/// <para>
/// Common causes include:
/// <list type="bullet">
/// <item>No device is connected at the specified address.</item>
/// <item>The device is busy and cannot process the command at this time.</item>
/// <item>The device does not recognize the command or data sent to it.</item>
/// </list>
/// </para>
/// </remarks>
public class I2cNackException : I2cCommandException {
  private const string DefaultMessage = "A NACK response was returned for the requested I2C command.";

  private static string CreateDefaultMessage(I2cAddress address)
    => $"The I2C target did not respond. (Address={address})";

  /// <inheritdoc/>
  public I2cNackException()
    : base(DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public I2cNackException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public I2cNackException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

  /// <inheritdoc/>
  public I2cNackException(I2cAddress address, string? message = null, Exception? innerException = null)
    : base(address, message ?? CreateDefaultMessage(address), innerException)
  {
  }
}
