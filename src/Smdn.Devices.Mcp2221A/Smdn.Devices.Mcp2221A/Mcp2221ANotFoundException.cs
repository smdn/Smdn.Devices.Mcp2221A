// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// The exception that is thrown when no MCP2221/MCP2221A device
/// is found on the system, or no device matches the specified
/// search criteria.
/// </summary>
/// <remarks>
/// <para>
/// This exception occurs during the device discovery process when the
/// <see cref="IUsbHidService"/> cannot find any USB HID devices that
/// match the required Vendor ID (VID) and Product ID (PID), or when no
/// devices satisfy the custom filtering conditions provided via
/// predicates.
/// </para>
/// <para>
/// Common causes include:
/// <list type="bullet">
/// <item>
///   The device is not physically connected to the system.
/// </item>
/// <item>
///   The device is using a custom VID/PID that does not match the
///   default or specified filters.
/// </item>
/// <item>
///   Multiple devices are connected, but none match the specific
///   criteria provided in the device information filter.
/// </item>
/// </list>
/// </para>
/// </remarks>
public class Mcp2221ANotFoundException : InvalidOperationException {
  private const string DefaultMessage = "The MCP2221/MCP2221A was not found on the current system.";

  /// <inheritdoc/>
  public Mcp2221ANotFoundException()
    : base(DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public Mcp2221ANotFoundException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  /// <inheritdoc/>
  public Mcp2221ANotFoundException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

  internal Mcp2221ANotFoundException(
    IUsbHidService usbHidService,
    Predicate<IUsbHidDevice>? predicate
  )
    : base($"{nameof(IUsbHidService)} could not find an MCP2221/MCP2221A matching the specified predicate. ({nameof(IUsbHidService)}: {usbHidService}, {nameof(predicate)}: {predicate?.ToString() ?? "null"})")
  {
  }

  internal Mcp2221ANotFoundException(
    IUsbHidService usbHidService,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter
  )
    : base($"{nameof(IUsbHidService)} could not find an MCP2221/MCP2221A matching the specified predicate. ({nameof(IUsbHidService)}: {usbHidService}, {nameof(usbHidDeviceFilter)}: {usbHidDeviceFilter?.ToString() ?? "null"}, {nameof(mcp2221AFilter)}: {mcp2221AFilter?.ToString() ?? "null"})")
  {
  }
}
