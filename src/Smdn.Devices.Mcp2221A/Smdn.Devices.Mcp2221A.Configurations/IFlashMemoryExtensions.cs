// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

/// <summary>
/// Provides extension members for the <see cref="IFlashMemory"/> interface.
/// </summary>
public static class IFlashMemoryExtensions {
  private static IFlashMemory ThrowIfReceiverIsNull(IFlashMemory flashMemory, string paramName)
    => flashMemory ?? throw new ArgumentNullException(paramName: paramName);

#pragma warning disable CA1034
  extension(IFlashMemory flashMemory) {
#pragma warning restore CA1034
    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing only the valid string bytes
    /// currently stored in <see cref="IFlashMemory.UsbManufacturerDescriptorString"/>.
    /// </summary>
    /// <value>
    /// A <see cref="ReadOnlySpan{T}"/> representing the stored portion of
    /// the USB Manufacturer Descriptor String,
    /// sliced up to <see cref="IFlashMemory.UsbManufacturerDescriptorStringLength"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> is <see langword="null"/>.
    /// </exception>
    public ReadOnlySpan<byte> StoredUsbManufacturerDescriptorStringSpan
      => ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory))
        .UsbManufacturerDescriptorString
        .Slice(0, flashMemory.UsbManufacturerDescriptorStringLength);

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing only the valid string bytes
    /// currently stored in <see cref="IFlashMemory.UsbProductDescriptorString"/>.
    /// </summary>
    /// <value>
    /// A <see cref="ReadOnlySpan{T}"/> representing the stored portion of
    /// the USB Product Descriptor String,
    /// sliced up to <see cref="IFlashMemory.UsbProductDescriptorStringLength"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> is <see langword="null"/>.
    /// </exception>
    public ReadOnlySpan<byte> StoredUsbProductDescriptorStringSpan
      => ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory))
        .UsbProductDescriptorString
        .Slice(0, flashMemory.UsbProductDescriptorStringLength);

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing only the valid string bytes
    /// currently stored in <see cref="IFlashMemory.UsbSerialNumberDescriptorString"/>.
    /// </summary>
    /// <value>
    /// A <see cref="ReadOnlySpan{T}"/> representing the stored portion of
    /// the USB Serial Number Descriptor String,
    /// sliced up to <see cref="IFlashMemory.UsbSerialNumberDescriptorStringLength"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> is <see langword="null"/>.
    /// </exception>
    public ReadOnlySpan<byte> StoredUsbSerialNumberDescriptorStringSpan
      => ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory))
        .UsbSerialNumberDescriptorString
        .Slice(0, flashMemory.UsbSerialNumberDescriptorStringLength);

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> containing only the valid string bytes
    /// currently stored in <see cref="IFlashMemory.ChipFactorySerialNumber"/>.
    /// </summary>
    /// <value>
    /// A <see cref="ReadOnlySpan{T}"/> representing the stored portion of
    /// the Chip Factory Serial Number,
    /// sliced up to <see cref="IFlashMemory.ChipFactorySerialNumberLength"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> is <see langword="null"/>.
    /// </exception>
    public ReadOnlySpan<byte> StoredChipFactorySerialNumberSpan
      => ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory))
        .ChipFactorySerialNumber
        .Slice(0, flashMemory.ChipFactorySerialNumberLength);

#pragma warning disable CS1574, CS1734
    /// <summary>
    /// Determines whether the non-password Flash memory contents of the current
    /// <see cref="IFlashMemory"/> instance differ from another
    /// <see cref="IFlashMemory"/> instance.
    /// </summary>
    /// <param name="other">
    /// An <see cref="IFlashMemory"/> instance to compare with the current instance,
    /// or <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if any configuration or descriptor data in <paramref name="other"/>
    /// differs byte-for-byte from this instance;
    /// <see langword="false"/> if both instances contain identical data (or if both represent
    /// the same reference).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method checks for differences across <see cref="IFlashMemory.ChipSettings"/>,
    /// <see cref="IFlashMemory.GpSettings"/>, and the valid stored string spans
    /// (<see cref="StoredUsbManufacturerDescriptorStringSpan"/>, <see cref="StoredUsbProductDescriptorStringSpan"/>,
    /// <see cref="StoredUsbSerialNumberDescriptorStringSpan"/>, and <see cref="StoredChipFactorySerialNumberSpan"/>).
    /// </para>
    /// <para>
    /// The <see cref="IFlashMemory.Password"/> area is explicitly excluded from
    /// this comparison because password bytes cannot be read back from the hardware device.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> is <see langword="null"/>.
    /// </exception>
#pragma warning restore CS1574, CS1734
    public bool DiffersFrom(IFlashMemory? other)
    {
      var @this = ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory));

      if (other is null)
        return true;

      if (ReferenceEquals(flashMemory, other))
        return false;

      if (!@this.ChipSettings.SequenceEqual(other.ChipSettings))
        return true;

      if (!@this.GpSettings.SequenceEqual(other.GpSettings))
        return true;

      if (!@this.StoredUsbManufacturerDescriptorStringSpan.SequenceEqual(other.StoredUsbManufacturerDescriptorStringSpan))
        return true;

      if (!@this.StoredUsbProductDescriptorStringSpan.SequenceEqual(other.StoredUsbProductDescriptorStringSpan))
        return true;

      if (!@this.StoredUsbSerialNumberDescriptorStringSpan.SequenceEqual(other.StoredUsbSerialNumberDescriptorStringSpan))
        return true;

      if (!@this.StoredChipFactorySerialNumberSpan.SequenceEqual(other.StoredChipFactorySerialNumberSpan))
        return true;

      return false;
    }

    /// <summary>
    /// Copies all Flash memory contents from the specified <paramref name="source"/>
    /// instance into the current instance,
    /// excluding the <see cref="IFlashMemory.Password"/> area.
    /// </summary>
    /// <param name="source">
    /// The <see cref="IFlashMemory"/> instance from which data is copied.
    /// </param>
    /// <returns>
    /// The current <see cref="IFlashMemory"/> instance to enable method chaining.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a byte-for-byte copy across <see cref="IFlashMemory.ChipSettings"/>,
    /// <see cref="IFlashMemory.GpSettings"/>, and all descriptor string buffers
    /// (<see cref="IFlashMemory.UsbManufacturerDescriptorString"/>, <see cref="IFlashMemory.UsbProductDescriptorString"/>,
    /// <see cref="IFlashMemory.UsbSerialNumberDescriptorString"/>, and <see cref="IFlashMemory.ChipFactorySerialNumber"/>)
    /// along with their corresponding length properties.
    /// </para>
    /// <para>
    /// The <see cref="IFlashMemory.Password"/> area is explicitly excluded from this
    /// operation because password bytes cannot be read back from the hardware device.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="flashMemory"/> or <paramref name="source"/>
    /// is <see langword="null"/>.
    /// </exception>
    public IFlashMemory CopyFrom(IFlashMemory source)
    {
      var @this = ThrowIfReceiverIsNull(flashMemory, nameof(flashMemory));

      if (source is null)
        throw new ArgumentNullException(nameof(source));

      if (ReferenceEquals(@this, source))
        return @this;

      source.ChipSettings.CopyTo(@this.ChipSettings);
      source.GpSettings.CopyTo(@this.GpSettings);

      source.UsbManufacturerDescriptorString.CopyTo(@this.UsbManufacturerDescriptorString);
      source.UsbProductDescriptorString.CopyTo(@this.UsbProductDescriptorString);
      source.UsbSerialNumberDescriptorString.CopyTo(@this.UsbSerialNumberDescriptorString);
      source.ChipFactorySerialNumber.CopyTo(@this.ChipFactorySerialNumber);

      @this.UsbManufacturerDescriptorStringLength = source.UsbManufacturerDescriptorStringLength;
      @this.UsbProductDescriptorStringLength = source.UsbProductDescriptorStringLength;
      @this.UsbSerialNumberDescriptorStringLength = source.UsbSerialNumberDescriptorStringLength;
      @this.ChipFactorySerialNumberLength = source.ChipFactorySerialNumberLength;

      return @this;
    }
  }
}
