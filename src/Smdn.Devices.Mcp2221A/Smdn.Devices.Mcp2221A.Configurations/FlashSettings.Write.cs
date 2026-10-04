// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers.Binary;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  /// <summary>
  /// Modifies the password required for accessing protected
  /// Flash configurations.
  /// </summary>
  /// <param name="password">
  /// A <see cref="ReadOnlySpan{T}"/> containing the password bytes.
  /// The length must be exactly 8 bytes.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Reflects <c>PASS0</c> to <c>PASS7</c>.
  /// </para>
  /// <para>
  /// Calling this method stages the new password and causes <see cref="IsDirty"/>
  /// to return <see langword="true"/>.
  /// Note that staged password modifications are <b>not</b> reverted by
  /// calling <see cref="Restore"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// The length of <paramref name="password"/> is not equal to 8.
  /// </exception>
  public FlashSettings ModifyPassword(ReadOnlySpan<byte> password)
  {
    ThrowIfPasswordLengthNotValid(password, nameof(password));

    if (!password.TryCopyTo(SettingsForWrite.Password))
      throw new InvalidOperationException("The destination password buffer is too short to store the password.");

    hasPasswordModified = true;
    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB Vendor ID (VID).
  /// </summary>
  /// <param name="vendorId">
  /// The Vendor ID value.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>USBVIDL</c>/<c>USBVIDH</c>.
  /// </remarks>
  public FlashSettings ModifyUsbVendorId(int vendorId)
  {
    BinaryPrimitives.WriteUInt16LittleEndian(
      SettingsForWrite.ChipSettings.Slice(OffsetOfUsbVendorId, 2),
      (ushort)vendorId
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB Product ID (PID).
  /// </summary>
  /// <param name="productId">
  /// The Product ID value.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>USBPIDL</c>/<c>USBPIDH</c>.
  /// </remarks>
  public FlashSettings ModifyUsbProductId(int productId)
  {
    BinaryPrimitives.WriteUInt16LittleEndian(
      SettingsForWrite.ChipSettings.Slice(OffsetOfUsbProductId, 2),
      (ushort)productId
    );

    hasWritten = false;

    return this;
  }
}
