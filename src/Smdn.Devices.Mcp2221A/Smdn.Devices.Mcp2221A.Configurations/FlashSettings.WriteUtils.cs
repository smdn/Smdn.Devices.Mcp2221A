// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  private static void ThrowUsbDescriptorStringArgumentTooLong(int length, string paramName)
    => throw new ArgumentException(
      message: $"The length of the specified string ({length} characters) exceeds the maximum allowed length of 30 characters (60 bytes in UTF-16).",
      paramName: paramName
    );

  /// <summary>
  /// Modifies the USB manufacturer string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the manufacturer name string.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// The length of <paramref name="value"/> exceeds the maximum allowed length
  /// (30 characters).
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method stages the manufacturer string to be written as a USB string
  /// descriptor. The string will be converted to a Unicode byte sequence when
  /// saved to the device.
  /// </para>
  /// <para>
  /// The maximum length of the string is 30 characters (60 bytes in UTF-16).
  /// </para>
  /// </remarks>
  /// <seealso cref="TryModifyUsbManufacturerString"/>
  /// <seealso cref="TryCopyUsbManufacturerStringTo"/>
  /// <seealso cref="GetUsbManufacturerString"/>
  public FlashSettings ModifyUsbManufacturerString(ReadOnlySpan<char> value)
  {
    if (!TryModifyUsbManufacturerString(value))
      ThrowUsbDescriptorStringArgumentTooLong(value.Length, nameof(value));

    return this;
  }

  /// <summary>
  /// Modifies the USB product string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the product name string.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// The length of <paramref name="value"/> exceeds the maximum allowed length
  /// (30 characters).
  /// </exception>
  /// <remarks>
  /// <para>
  /// While the Product ID (PID) is a fixed numeric value, the product string
  /// is a variable-length Unicode string. This method stages the string as it
  /// should appear when the device is enumerated.
  /// </para>
  /// <para>
  /// The maximum length of the string is 30 characters (60 bytes in UTF-16).
  /// </para>
  /// </remarks>
  /// <seealso cref="TryModifyUsbProductString"/>
  /// <seealso cref="TryCopyUsbProductStringTo"/>
  /// <seealso cref="GetUsbProductString"/>
  public FlashSettings ModifyUsbProductString(ReadOnlySpan<char> value)
  {
    if (!TryModifyUsbProductString(value))
      ThrowUsbDescriptorStringArgumentTooLong(value.Length, nameof(value));

    return this;
  }

  /// <summary>
  /// Modifies the USB serial number string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the serial number string.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// The length of <paramref name="value"/> exceeds the maximum allowed length
  /// (30 characters).
  /// </exception>
  /// <remarks>
  /// <para>
  /// According to the USB specification, the serial number is a Unicode string
  /// and can contain alphanumeric characters, not just digits. The maximum
  /// length is 30 characters (60 bytes in UTF-16).
  /// </para>
  /// <para>
  /// This serial number string is always used by the HID interface. To also
  /// expose this serial number through the USB CDC (Communication Device Class)
  /// interface, ensure that CDC serial number enumeration is enabled via
  /// <see cref="ModifyCdcSerialNumberEnumeration(bool)"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="TryModifyUsbSerialNumberString"/>
  /// <seealso cref="TryCopyUsbSerialNumberStringTo"/>
  /// <seealso cref="GetUsbSerialNumberString"/>
  public FlashSettings ModifyUsbSerialNumberString(ReadOnlySpan<char> value)
  {
    if (!TryModifyUsbSerialNumberString(value))
      ThrowUsbDescriptorStringArgumentTooLong(value.Length, nameof(value));

    return this;
  }
}
