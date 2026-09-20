// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Buffers.Binary;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  /// <summary>Gets the USB Vendor ID (VID).</summary>
  /// <remarks>Reflects <c>USBVIDL</c>/<c>USBVIDH</c>.</remarks>
  /// <seealso cref="ModifyUsbVendorId(int)"/>
  public int UsbVendorId
    => BinaryPrimitives.ReadUInt16LittleEndian(
      SettingsForRead.ChipSettings.Slice(OffsetOfUsbVendorId, 2)
    );

  /// <summary>Gets the USB Product ID (PID).</summary>
  /// <remarks>Reflects <c>USBPIDL</c>/<c>USBPIDH</c>.</remarks>
  /// <seealso cref="ModifyUsbProductId(int)"/>
  public int UsbProductId
    => BinaryPrimitives.ReadUInt16LittleEndian(
      SettingsForRead.ChipSettings.Slice(OffsetOfUsbProductId, 2)
    );
}
