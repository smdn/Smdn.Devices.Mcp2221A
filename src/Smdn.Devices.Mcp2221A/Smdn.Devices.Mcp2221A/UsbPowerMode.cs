// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Defines the USB power configuration mode of the device.
/// This value is used to get or set the power source attribute
/// in the USB configuration descriptor.
/// </summary>
public enum UsbPowerMode {
  // The values of each member correspond to the bit 6 (SELFPWR)
  // defined in the 'USB Power Attributes' register (USBPWRATTR)
  // of the MCP2221A.
  // See Register 1-9 in the datasheet for more details.

  /// <summary>
  /// The device is bus-powered.
  /// </summary>
  BusPowered = 0,

  /// <summary>
  /// The device is self-powered.
  /// </summary>
  SelfPowered = 1,
}
