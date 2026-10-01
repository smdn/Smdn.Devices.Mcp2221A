// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

/// <summary>
/// The exception that is thrown when a Flash memory access command fails,
/// such as when providing an invalid password, exceeding the Flash update limit,
/// or attempting a write operation while Flash write protection is active.
/// </summary>
/// <remarks>
/// <para>
/// This exception represents logical command failures specific to Flash memory
/// write operations and access control.
/// It is thrown under conditions including, but not limited to:
/// <list type="bullet">
///   <item>
///     <description>
///       An incorrect password is supplied or Flash update limits have been
///       reached during a call to <see cref="FlashSettings.SendAccessPassword"/> or
///       <see cref="FlashSettings.SendAccessPasswordAsync"/>.
///     </description>
///   </item>
///   <item>
///     <description>
///       A Flash write command is rejected by the device during a call to
///       <see cref="FlashSettings.Write"/> or <see cref="FlashSettings.WriteAsync"/>
///       because write protection remains active or write access is not permitted
///       (e.g., returning a <c>0x03 Command not allowed</c> status).
///     </description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="Mcp2221ACommandException"/>
/// <seealso cref="FlashSettings.SendAccessPassword"/>
/// <seealso cref="FlashSettings.SendAccessPasswordAsync"/>
/// <seealso cref="FlashSettings.Write"/>
/// <seealso cref="FlashSettings.WriteAsync"/>
public class FlashWriteAccessException : Mcp2221ACommandException {
  private const string DefaultMessage = "The Flash memory write operation or access request was denied.";

  /// <inheritdoc/>
  public FlashWriteAccessException()
    : base(DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public FlashWriteAccessException(string? message)
    : base(message)
  {
  }

  /// <inheritdoc/>
  public FlashWriteAccessException(string? message, Exception? innerException)
    : base(message, innerException)
  {
  }
}
