// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// The exception that is thrown when a command sent to the
/// MCP2221/MCP2221A device fails, or when an invalid response is received.
/// </summary>
/// <remarks>
/// <para>
/// This exception represents failures during communication with
/// the device after a connection has been established. It covers
/// a wide range of errors, including:
/// <list type="bullet">
/// <item>
///   Failures in sending or receiving USB HID reports
///   (often wrapping an exception from the underlying backend).
/// </item>
/// <item>
///   Protocol mismatches, such as unexpected command echoes or
///   incorrect response report lengths.
/// </item>
/// <item>
///   Logical command failures where the device returns a non-zero
///   error code in its response.
/// </item>
/// </list>
/// </para>
/// <para>
/// If the failure was caused by an error in the underlying USB HID
/// backend library, the original exception can be retrieved from the
/// <see cref="Exception.InnerException"/> property.
/// </para>
/// </remarks>
public class Mcp2221ACommandException : InvalidOperationException {
  private const string DefaultMessage = "The command to the MCP2221/MCP2221A failed.";

  /// <inheritdoc/>
  public Mcp2221ACommandException()
    : base(DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public Mcp2221ACommandException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  /// <inheritdoc/>
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
