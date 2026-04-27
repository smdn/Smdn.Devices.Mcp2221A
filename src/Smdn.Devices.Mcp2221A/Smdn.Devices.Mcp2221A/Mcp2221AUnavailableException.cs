// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// The exception that is thrown when an MCP2221/MCP2221A device is
/// unavailable or the connection to the device cannot be established.
/// </summary>
/// <remarks>
/// <para>
/// This exception typically occurs when the application lacks the
/// necessary permissions to access the USB HID endpoint (e.g., udev
/// rules on Linux), the device has been disconnected, or it is
/// currently being used by another process or driver.
/// </para>
/// <para>
/// Since this library supports multiple USB HID backends, the underlying
/// cause of the failure may vary depending on the implementation.
/// Refer to the <see cref="Exception.InnerException"/> property for the
/// specific exception thrown by the backend library.
/// </para>
/// </remarks>
public class Mcp2221AUnavailableException : UnauthorizedAccessException {
  private const string DefaultMessage = "The requested MCP2221/MCP2221A is unavailable due to reasons such as unprivileged access, being disconnected, or being blocked by another driver.";

  public Mcp2221AUnavailableException()
    : base(DefaultMessage)
  {
  }

  public Mcp2221AUnavailableException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  public Mcp2221AUnavailableException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

  public Mcp2221AUnavailableException(Exception innerException, IUsbHidDevice? device = null)
    : base(
      message: $"{DefaultMessage} (device='{device?.ToIdentificationString() ?? "?"}')",
      inner: innerException
    )
  {
  }
}
