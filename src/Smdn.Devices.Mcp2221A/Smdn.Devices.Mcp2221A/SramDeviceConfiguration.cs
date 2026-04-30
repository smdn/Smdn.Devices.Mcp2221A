// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Represents a partial set of the device configuration currently loaded in the SRAM.
/// </summary>
/// <remarks>
/// This structure holds values retrieved via the <c>GET SRAM SETTINGS</c> command
/// and provides decoded access to specific hardware configuration bits.
/// </remarks>
internal readonly struct SramDeviceConfiguration(
  byte chipSetting0, // GET SRAM SETTINGS Byte Index 4 / CHIPSETTING0 REGISTER
  ushort usbVendorId, // GET SRAM SETTINGS Byte Index 8-9 / USBVIDL/USBVIDH REGISTER
  ushort usbProductId, // GET SRAM SETTINGS Byte Index 10-11 / USBPIDL/USBPIDH REGISTER
  byte usbPowerAttributes, // GET SRAM SETTINGS Byte Index 12 / USBPWRATTR REGISTER
  byte usbRequiredCurrent // GET SRAM SETTINGS Byte Index 13 / USBREQCRT REGISTER
) {
  /// <summary>
  /// Gets the USB Vendor ID (VID).
  /// </summary>
  public ushort UsbVendorId { get; } = usbVendorId;

  /// <summary>
  /// Gets the USB Product ID (PID).
  /// </summary>
  public ushort UsbProductId { get; } = usbProductId;

  /// <summary>
  /// Gets a value indicating whether the CDC serial number enumeration is enabled.
  /// </summary>
  public bool UsbCdcSerialNumberEnabled
    => (chipSetting0 & 0b_1_00000_00) != 0;

  /// <summary>
  /// Gets the protection level applied to the Flash memory configuration areas.
  /// </summary>
  public DeviceConfigurationProtectionLevel FlashWriteProtection
    => (DeviceConfigurationProtectionLevel)(chipSetting0 & 0b_0_00000_11);

  /// <summary>
  /// Gets the USB power configuration mode (Bus-powered or Self-powered).
  /// </summary>
  public UsbPowerMode UsbPowerMode
    => (usbPowerAttributes & 0b_0_1_0_00000) == 0
      ? UsbPowerMode.BusPowered
      : UsbPowerMode.SelfPowered;

  /// <summary>
  /// Gets a value indicating whether the Remote Wake-Up capability is enabled.
  /// </summary>
  public bool UsbRemoteWakeUpEnabled
    => (usbPowerAttributes & 0b_0_0_1_00000) != 0;

  /// <summary>
  /// Gets the maximum amount of current requested from the USB bus,
  /// expressed in milliamperes (mA).
  /// </summary>
  public int UsbRequestedCurrentAmount
    => usbRequiredCurrent << 1;
}
