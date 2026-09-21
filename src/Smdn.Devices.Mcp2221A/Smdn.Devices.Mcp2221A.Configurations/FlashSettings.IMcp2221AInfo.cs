// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  private static string DecodeUsbDescriptorString(ReadOnlySpan<byte> bytes)
#pragma warning disable SA1114
    => Encoding.Unicode.GetString(
#if SYSTEM_TEXT_ENCODING_GETSTRING_READONLYSPAN_OF_BYTE
      bytes
#else
      bytes.ToArray(),
      0,
      bytes.Length
#endif
    );
#pragma warning restore SA1114

  private static string DecodeChipFactorySerialNumberString(ReadOnlySpan<byte> bytes)
  {
#if SYSTEM_STRING_CREATE_OF_TSTATE_ALLOWS_REF_STRUCT
    return string.Create(
      bytes.Length,
      bytes,
      static (s, by) => {
        for (var i = 0; i < s.Length; i++) {
          s[i] = (char)by[i];
        }
      }
    );
#else
    Span<char> serialNumberChars = stackalloc char[bytes.Length];

    for (var i = 0; i < bytes.Length; i++) {
      serialNumberChars[i] = (char)bytes[i];
    }

#pragma warning disable SA1114
    return new string(
#if SYSTEM_STRING_CTOR_READONLYSPAN_OF_CHAR
      serialNumberChars
#else
      serialNumberChars.ToArray(),
      0,
      serialNumberChars.Length
#endif
    );
#pragma warning restore SA1114
#endif
  }

  private readonly string hardwareRevision;
  private readonly string firmwareRevision;

  /// <inheritdoc/>
  string IMcp2221AInfo.HardwareRevision => hardwareRevision;

  /// <inheritdoc/>
  string IMcp2221AInfo.FirmwareRevision => firmwareRevision;

  /// <inheritdoc/>
  string IMcp2221AInfo.Manufacturer
    => field ??= DecodeUsbDescriptorString(initialSettings.StoredUsbManufacturerDescriptorStringSpan);

  /// <inheritdoc/>
  string IMcp2221AInfo.Product
    => field ??= DecodeUsbDescriptorString(initialSettings.StoredUsbProductDescriptorStringSpan);

  /// <inheritdoc/>
  string IMcp2221AInfo.SerialNumber
    => field ??= DecodeUsbDescriptorString(initialSettings.StoredUsbSerialNumberDescriptorStringSpan);

  /// <inheritdoc/>
  string IMcp2221AInfo.ChipFactorySerialNumber
    => field ??= DecodeChipFactorySerialNumberString(initialSettings.StoredChipFactorySerialNumberSpan);

  internal string ToMcp2221AInfoString()
  {
    IMcp2221AInfo info = this;

    return $"{{{nameof(info.HardwareRevision)}='{info.HardwareRevision}', {nameof(info.FirmwareRevision)}='{info.FirmwareRevision}', {nameof(info.Manufacturer)}='{info.Manufacturer}', {nameof(info.Product)}='{info.Product}', {nameof(info.SerialNumber)}='{info.SerialNumber}', {nameof(info.ChipFactorySerialNumber)}='{info.ChipFactorySerialNumber}'}}";
  }
}
