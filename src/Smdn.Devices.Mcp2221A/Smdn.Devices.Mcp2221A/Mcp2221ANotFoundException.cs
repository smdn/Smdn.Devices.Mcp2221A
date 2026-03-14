// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

public class Mcp2221ANotFoundException : InvalidOperationException {
  private const string DefaultMessage = "The MCP2221/MCP2221A was not found on the current system.";

  public Mcp2221ANotFoundException()
    : base(DefaultMessage)
  {
  }

  public Mcp2221ANotFoundException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

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
