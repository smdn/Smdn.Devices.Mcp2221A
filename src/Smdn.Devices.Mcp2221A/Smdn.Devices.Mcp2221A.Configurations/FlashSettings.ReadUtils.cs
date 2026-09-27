// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  /// <summary>
  /// Specifies the maximum length, in characters, of the USB string descriptors
  /// stored in the Flash memory.
  /// </summary>
  /// <remarks>
  /// Based on the specification for the MCP2221A Flash data read/write commands,
  /// USB string descriptors are limited to a maximum of 60 bytes, which corresponds
  /// to 30 UTF-16 (16-bit Unicode) characters.
  /// </remarks>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 Read Flash Data
  /// </seealso>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.3 Write Flash Data
  /// </seealso>
  private const int MaxUsbDescriptorStringLength = 30;

  /// <summary>
  /// Specifies the maximum length, in characters, of the chip factory serial
  /// number stored in the Flash memory.
  /// </summary>
  /// <remarks>
  /// Based on the specification for the MCP2221A Read Flash Data command,
  /// the maximum structure length for the chip factory serial number is 60 bytes,
  /// which corresponds to at most 60 characters when converted to a sequence
  /// of <see cref="char"/>.
  /// </remarks>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 Read Flash Data
  /// </seealso>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.3 Write Flash Data
  /// </seealso>
  private const int MaxChipFactorySerialNumberLength = 60;

  /// <summary>
  /// Gets the USB manufacturer string configured in the Flash memory as a <see cref="string"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="string"/> that contains the USB manufacturer string.
  /// </returns>
  /// <seealso cref="TryCopyUsbManufacturerStringTo(Span{char}, out int)"/>
  public string GetUsbManufacturerString()
  {
    Span<char> destination = stackalloc char[MaxUsbDescriptorStringLength];

    if (!TryCopyUsbManufacturerStringTo(destination, out var charsWritten))
      throw new InvalidOperationException("Failed to copy the USB manufacturer string into the destination buffer.");

    return destination.Slice(0, charsWritten).ToString();
  }

  /// <summary>
  /// Gets the USB product string configured in the Flash memory as a <see cref="string"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="string"/> that contains the USB product string.
  /// </returns>
  /// <seealso cref="TryCopyUsbProductStringTo(Span{char}, out int)"/>
  public string GetUsbProductString()
  {
    Span<char> destination = stackalloc char[MaxUsbDescriptorStringLength];

    if (!TryCopyUsbProductStringTo(destination, out var charsWritten))
      throw new InvalidOperationException("Failed to copy the USB product string into the destination buffer.");

    return destination.Slice(0, charsWritten).ToString();
  }

  /// <summary>
  /// Gets the USB serial number string configured in the Flash memory as a <see cref="string"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="string"/> that contains the USB serial number string.
  /// </returns>
  /// <seealso cref="TryCopyUsbSerialNumberStringTo(Span{char}, out int)"/>
  public string GetUsbSerialNumberString()
  {
    Span<char> destination = stackalloc char[MaxUsbDescriptorStringLength];

    if (!TryCopyUsbSerialNumberStringTo(destination, out var charsWritten))
      throw new InvalidOperationException("Failed to copy the USB serial number string into the destination buffer.");

    return destination.Slice(0, charsWritten).ToString();
  }

  /// <summary>
  /// Gets the chip factory serial number stored in the Flash memory as a <see cref="string"/>.
  /// </summary>
  /// <returns>
  /// A <see cref="string"/> that contains the chip factory serial number.
  /// </returns>
  /// <seealso cref="TryCopyChipFactorySerialNumberTo(Span{char}, out int)"/>
  public string GetChipFactorySerialNumber()
  {
    Span<char> destination = stackalloc char[MaxChipFactorySerialNumberLength];

    if (!TryCopyChipFactorySerialNumberTo(destination, out var charsWritten))
      throw new InvalidOperationException("Failed to copy the chip factory serial number into the destination buffer.");

    return destination.Slice(0, charsWritten).ToString();
  }
}
