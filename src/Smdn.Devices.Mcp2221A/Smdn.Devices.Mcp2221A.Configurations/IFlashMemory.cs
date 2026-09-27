// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

/// <summary>
/// Represents the raw byte buffer and provides accessors to individual sections of Flash memory.
/// </summary>
/// <remarks>
/// <para>
/// Instances implementing this interface hold either the current configuration read from the device's
/// Flash memory or the staged configuration to be written to Flash memory.
/// </para>
/// <para>
/// <b>Note:</b> This interface is intended for internal buffer management and is not meant for direct
/// consumption by end-user application code.
/// </para>
/// </remarks>
/// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
/// [MCP2221A] 1.4.2 CHIP SETTINGS MAP
/// </seealso>
/// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
/// [MCP2221A] 1.4.3 GP SETTINGS MAP
/// </seealso>
/// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
/// [MCP2221A] 3.1.2 Read Flash Data
/// </seealso>
/// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
/// [MCP2221A] 3.1.3 Write Flash Data
/// </seealso>
public interface IFlashMemory {
  /// <summary>
  /// Gets a 10-byte <see cref="Span{T}"/> providing access to the Chip settings area in
  /// Flash memory, excluding the password fields (<c>PASS0</c>–<c>PASS7</c>).
  /// </summary>
  /// <remarks>
  /// Reflects byte index 0–9 (<c>CHIPSETTING0</c>–<c>USBREQCRT</c>) of the Chip settings
  /// area residing in Flash memory.
  /// Represents the data retrieved via the "Read Flash Data" command and serves as
  /// the data payload for the "Write Flash Data" command.
  /// </remarks>
  Span<byte> ChipSettings { get; }

  /// <summary>
  /// Gets an 8-byte <see cref="Span{T}"/> providing access to the password range
  /// (<c>PASS0</c>–<c>PASS7</c>) in the Chip settings area.
  /// </summary>
  /// <remarks>
  /// Reflects byte index 10–17 (<c>PASS0</c>–<c>PASS7</c>) of the Chip settings area
  /// residing in Flash memory.
  /// Serves as the password payload for the "Write Flash Data" command.
  /// Note that these bytes cannot be read back from the device via "Read Flash Data".
  /// </remarks>
  Span<byte> Password { get; }

  /// <summary>
  /// Gets a 4-byte <see cref="Span{T}"/> providing access to the GP settings area in
  /// Flash memory.
  /// </summary>
  /// <remarks>
  /// Reflects <c>GPSETTING0</c>–<c>GPSETTING3</c> of the GP settings area residing
  /// in Flash memory.
  /// Represents the data retrieved via the "Read Flash Data" command and serves as
  /// the data payload for the "Write Flash Data" command.
  /// </remarks>
  Span<byte> GpSettings { get; }

  /// <summary>
  /// Gets a fixed-length 60-byte <see cref="Span{T}"/> providing access to
  /// the USB Manufacturer Descriptor String area in Flash memory.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Reflects the bytes of the USB Manufacturer Descriptor String stored in Flash memory.
  /// This property represents a fixed-size memory region (60 bytes) regardless of
  /// the actual length of the string stored within it.
  /// </para>
  /// <para>
  /// The actual length of the valid string data (in bytes) is held in <see cref="UsbManufacturerDescriptorStringLength"/>.
  /// Validation of the descriptor string contents (such as descriptor structure and UTF-16
  /// encoding validity) must be completed by the caller prior to populating this buffer.
  /// </para>
  /// </remarks>
  Span<byte> UsbManufacturerDescriptorString { get; }

  /// <summary>
  /// Gets or sets the actual length (in bytes) of the string stored in <see cref="UsbManufacturerDescriptorString"/>.
  /// </summary>
  int UsbManufacturerDescriptorStringLength { get; set; }

  /// <summary>
  /// Gets a fixed-length 60-byte <see cref="Span{T}"/> providing access to
  /// the USB Product Descriptor String area in Flash memory.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Reflects the bytes of the USB Product Descriptor String stored in Flash memory.
  /// This property represents a fixed-size memory region (60 bytes) regardless of
  /// the actual length of the string stored within it.
  /// </para>
  /// <para>
  /// The actual length of the valid string data (in bytes) is held in <see cref="UsbProductDescriptorStringLength"/>.
  /// Validation of the descriptor string contents (such as descriptor structure and UTF-16
  /// encoding validity) must be completed by the caller prior to populating this buffer.
  /// </para>
  /// </remarks>
  Span<byte> UsbProductDescriptorString { get; }

  /// <summary>
  /// Gets or sets the actual length (in bytes) of the string stored in <see cref="UsbProductDescriptorString"/>.
  /// </summary>
  int UsbProductDescriptorStringLength { get; set; }

  /// <summary>
  /// Gets a fixed-length 60-byte <see cref="Span{T}"/> providing access to
  /// the USB Serial Number Descriptor String area in Flash memory.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Reflects the bytes of the USB Serial Number Descriptor String stored in Flash memory.
  /// This property represents a fixed-size memory region (60 bytes) regardless of
  /// the actual length of the string stored within it.
  /// </para>
  /// <para>
  /// The actual length of the valid string data (in bytes) is held in <see cref="UsbSerialNumberDescriptorStringLength"/>.
  /// Validation of the descriptor string contents (such as descriptor structure and UTF-16
  /// encoding validity) must be completed by the caller prior to populating this buffer.
  /// </para>
  /// </remarks>
  Span<byte> UsbSerialNumberDescriptorString { get; }

  /// <summary>
  /// Gets or sets the actual length (in bytes) of the string stored in <see cref="UsbSerialNumberDescriptorString"/>.
  /// </summary>
  int UsbSerialNumberDescriptorStringLength { get; set; }

  /// <summary>
  /// Gets a fixed-length 60-byte <see cref="Span{T}"/> providing access to
  /// the Chip Factory Serial Number area in Flash memory.
  /// </summary>
  /// <remarks>
  /// Reflects the bytes of the Chip Factory Serial Number stored in Flash memory
  /// as retrieved by the "Read Flash Data" command.
  /// Because the Chip Factory Serial Number in Flash memory is read-only on the hardware,
  /// this buffer is not used during "Write Flash Data" operations.
  /// </remarks>
  Span<byte> ChipFactorySerialNumber { get; }

  /// <summary>
  /// Gets or sets the actual length (in bytes) of the string stored in <see cref="ChipFactorySerialNumber"/>.
  /// </summary>
  int ChipFactorySerialNumberLength { get; set; }
}
