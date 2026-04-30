// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Defines the protection level applied to the device configuration
/// settings stored in Flash memory. This value is used to get or set
/// the write-access restrictions for Chip and GP settings.
/// </summary>
public enum DeviceConfigurationProtectionLevel {
  // The values of each member correspond to the bits 0-1 (<c>CHIPPROT</c>)
  // defined in the <c>CHIPSETTING0</c> register of the MCP2221A.
  // See Register 1-1 in the datasheet for more details.

  /// <summary>
  /// The settings are not protected and can be modified freely.
  /// </summary>
  None = 0b00,

  /// <summary>
  /// The settings are protected by a password.
  /// </summary>
  PasswordProtected = 0b01,

  /// <summary>
  /// The settings are permanently locked and cannot be modified.
  /// </summary>
  PermanentlyLocked = 0b10,

  /// <summary>
  /// Reserved by the device.
  /// </summary>
#pragma warning disable CA1700
  Reserved = 0b11,
#pragma warning restore CA1700
}
