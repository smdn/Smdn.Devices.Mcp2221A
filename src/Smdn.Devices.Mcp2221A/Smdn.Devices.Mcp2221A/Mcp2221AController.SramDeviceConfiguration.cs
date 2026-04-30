// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController {
#pragma warning restore IDE0040
  private SramDeviceConfiguration SramDeviceConfiguration { get; set; }

  /// <summary>
  /// Gets the USB Vendor ID (VID) currently loaded in the SRAM.
  /// </summary>
  /// <remarks>
  /// This property reflects the values from the <c>USBVIDL</c>/<c>USBVIDH</c>
  /// registers currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public int UsbVendorId
    => SramDeviceConfiguration.UsbVendorId;

  /// <summary>
  /// Gets the USB Product ID (PID) currently loaded in the SRAM.
  /// </summary>
  /// <remarks>
  /// This property reflects the values from the <c>USBPIDL</c>/<c>USBPIDH</c>
  /// registers currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public int UsbProductId
    => SramDeviceConfiguration.UsbProductId;

  /// <summary>
  /// Gets a value indicating whether the CDC (Communication Device Class)
  /// interface currently reports the serial number string to the USB host.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if the CDC serial number is enabled;
  /// otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// This property reflects the <c>CDCSNEN</c> bit in the <c>CHIPSETTING0</c>
  /// register currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public bool UsbCdcSerialNumberEnabled
    => SramDeviceConfiguration.UsbCdcSerialNumberEnabled;

  /// <summary>
  /// Gets the currently loaded USB power configuration mode of the device,
  /// indicating its power source.
  /// </summary>
  /// <value>
  /// A <see cref="UsbPowerMode"/> value.
  /// </value>
  /// <remarks>
  /// This property reflects the <c>SELFPWR</c> bit in the <c>USBPWRATTR</c>
  /// register currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public UsbPowerMode UsbPowerMode
    => SramDeviceConfiguration.UsbPowerMode;

  /// <summary>
  /// Gets a value indicating whether the Remote Wake-Up capability
  /// is currently enabled in the SRAM.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if the device can wake up the host from suspend;
  /// otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// This property reflects the <c>REMWKUP</c> bit in the <c>USBPWRATTR</c>
  /// register currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public bool UsbRemoteWakeUpEnabled
    => SramDeviceConfiguration.UsbRemoteWakeUpEnabled;

  /// <summary>
  /// Gets the currently loaded maximum amount of current requested from
  /// the USB bus, expressed in milliamperes (mA).
  /// </summary>
  /// <value>
  /// The requested current amount in mA.
  /// </value>
  /// <remarks>
  /// This property reflects the <c>USBREQCRT</c> register currently loaded
  /// in the SRAM, as retrieved by the <c>GET SRAM SETTINGS</c> command.
  /// It corresponds to the <c>bMaxPower</c> field in the USB configuration
  /// descriptor.
  /// </remarks>
  public int UsbRequestedCurrentAmount
    => SramDeviceConfiguration.UsbRequestedCurrentAmount;

  /// <summary>
  /// Gets the write-protection level currently applied to the Flash memory
  /// areas where device configuration settings (Chip settings and GP settings)
  /// are stored.
  /// </summary>
  /// <value>
  /// A <see cref="DeviceConfigurationProtectionLevel"/> value.
  /// </value>
  /// <remarks>
  /// This property reflects the <c>CHIPPROT</c> bits in the <c>CHIPSETTING0</c>
  /// register currently loaded in the SRAM, as retrieved by the
  /// <c>GET SRAM SETTINGS</c> command.
  /// </remarks>
  public DeviceConfigurationProtectionLevel FlashWriteProtection
    => SramDeviceConfiguration.FlashWriteProtection;
}
