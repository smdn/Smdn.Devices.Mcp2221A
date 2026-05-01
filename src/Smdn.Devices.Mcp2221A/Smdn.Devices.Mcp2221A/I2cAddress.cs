// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if !GENERIC_MATH_INTERFACES
#pragma warning disable CS1591, SA1648
#endif

using System;
using System.Collections.Generic;
#if GENERIC_MATH_INTERFACES
using System.Numerics;
#endif

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0055
/// <summary>
/// Represents an I2C device address.
/// </summary>
/// <remarks>
/// The I2C address consists of 7 bits: the upper 4 bits represent the device
/// address, and the lower 3 bits represent the hardware address pins.
/// </remarks>
#pragma warning disable IDE0055, SA1001
public readonly struct I2cAddress :
  IEquatable<I2cAddress>,
  IEquatable<int>,
  IEquatable<byte>,
  IComparable<I2cAddress>
#if GENERIC_MATH_INTERFACES
  ,
  IComparisonOperators<I2cAddress, I2cAddress, bool>,
  IEqualityOperators<I2cAddress, I2cAddress, bool>
#endif
{
#pragma warning restore IDE0055, SA1001
  /// <summary>
  /// Gets the default (zero) I2C address.
  /// </summary>
  public static I2cAddress Zero { get; } = default;

  /// <summary>
  /// Gets the minimum value of a valid I2C device address.
  /// </summary>
  public static I2cAddress DeviceMinValue { get; } = new((byte)0b_0_0001_000u);

  /// <summary>
  /// Gets the maximum value of a valid I2C device address.
  /// </summary>
  public static I2cAddress DeviceMaxValue { get; } = new((byte)0b_0_1110_111u);

  private static byte ValidateDeviceAddressBits(uint address, string paramName)
  {
    const uint AddressRangeLower = 0b_0_0001_000u;
    const uint AddressRangeUpper = 0b_0_1110_000u;

    var actualValue = address;

    address &= 0b_0_1111_000u;

    if (address is not (>= AddressRangeLower and <= AddressRangeUpper))
      throw new ArgumentOutOfRangeException(paramName, actualValue, $"must be in range between {AddressRangeLower}(0x{AddressRangeLower:X2}) and {AddressRangeUpper}(0x{AddressRangeUpper:X2})");

    return (byte)address;
  }

  private static byte ValidateHardwareAddressBits(uint address, string paramName)
  {
    const uint AddressRangeLower = 0b_0_0000_000u;
    const uint AddressRangeUpper = 0b_0_0000_111u;

    if (address is not (>= AddressRangeLower and <= AddressRangeUpper))
      throw new ArgumentOutOfRangeException(paramName, address, $"must be in range between {AddressRangeLower}(0x{AddressRangeLower:X2}) and {AddressRangeUpper}(0x{AddressRangeUpper:X2})");

    return (byte)(address & 0b0_0000_111u);
  }

  /*
   * instance members
   */
  private readonly byte address;

  /// <summary>
  /// Initializes a new instance of the <see cref="I2cAddress" /> struct from
  /// the device address bits and hardware address bits.
  /// </summary>
  /// <param name="deviceAddressBits">
  /// The device address bits (upper 4 bits of the 7-bit I2C address).
  /// </param>
  /// <param name="hardwareAddressBits">
  /// The hardware address bits (lower 3 bits of the 7-bit I2C address).
  /// </param>
  public I2cAddress(int deviceAddressBits, int hardwareAddressBits)
    : this(
      (byte)(
        ValidateDeviceAddressBits((uint)deviceAddressBits, nameof(deviceAddressBits)) |
        ValidateHardwareAddressBits((uint)hardwareAddressBits, nameof(hardwareAddressBits))
      )
    )
  {
  }

  private static byte ValidateAddress(uint address, string paramName)
  {
    if (!(DeviceMinValue.address <= address && address <= DeviceMaxValue.address))
      throw new ArgumentOutOfRangeException(paramName, address, $"must be in range between {DeviceMinValue.address}(0x{DeviceMinValue.address:X2}) and {DeviceMaxValue.address}(0x{DeviceMaxValue.address:X2})");

    return (byte)(address & 0b_0_1111_111u);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="I2cAddress" /> struct from
  /// a 7-bit I2C address value.
  /// </summary>
  /// <param name="address">
  /// The 7-bit I2C address value (must be between <see cref="DeviceMinValue" />
  /// and <see cref="DeviceMaxValue" />).
  /// </param>
  public I2cAddress(int address)
    : this(ValidateAddress((uint)address, nameof(address)))
  {
  }

  private I2cAddress(byte address)
  {
    this.address = address;
  }

  /// <summary>
  /// Determines whether this address is equal to another <see cref="I2cAddress" />.
  /// </summary>
  /// <param name="other">The address to compare.</param>
  /// <returns>
  /// <see langword="true" /> if the addresses are equal;
  /// otherwise, <see langword="false" />.
  /// </returns>
  public bool Equals(I2cAddress other) => address == other.address;

  /// <summary>
  /// Determines whether this address is equal to an <see cref="int"/> value.
  /// </summary>
  /// <param name="other">The <see cref="int"/> value to compare.</param>
  /// <returns>
  /// <see langword="true" /> if the values are equal; otherwise,
  /// <see langword="false" />.
  /// </returns>
  public bool Equals(int other) => address == other;

  /// <summary>
  /// Determines whether this address is equal to a <see cref="byte"/> value.
  /// </summary>
  /// <param name="other">The <see cref="byte"/> value to compare.</param>
  /// <returns>
  /// <see langword="true" /> if the values are equal;
  /// otherwise, <see langword="false" />.
  /// </returns>
  public bool Equals(byte other) => address == other;

  /// <inheritdoc/>
  public override bool Equals(object? obj) => obj switch {
    null => false,
    I2cAddress other => Equals(other),
    int other => Equals(other),
    byte other => Equals(other),
    _ => false,
  };

  /// <inheritdoc/>
  public override int GetHashCode() => address.GetHashCode();

  /// <inheritdoc/>
  public static bool operator ==(I2cAddress x, I2cAddress y) => x.Equals(y);

  /// <inheritdoc/>
  public static bool operator !=(I2cAddress x, I2cAddress y) => !x.Equals(y);

  /// <summary>
  /// Compares this address to another <see cref="I2cAddress" />.
  /// </summary>
  /// <param name="other">The address to compare.</param>
  /// <returns>
  /// A signed integer that indicates the relative order of the addresses.
  /// </returns>
  public int CompareTo(I2cAddress other)
    => Comparer<byte>.Default.Compare(address, other.address);

  /// <inheritdoc/>
  public static bool operator <(I2cAddress left, I2cAddress right)
    => left.address < right.address;

  /// <inheritdoc/>
  public static bool operator <=(I2cAddress left, I2cAddress right)
    => left.address <= right.address;

  /// <inheritdoc/>
  public static bool operator >(I2cAddress left, I2cAddress right)
    => left.address > right.address;

  /// <inheritdoc/>
  public static bool operator >=(I2cAddress left, I2cAddress right)
    => left.address >= right.address;

  /// <summary>
  /// Converts an <see cref="I2cAddress" /> to a <see cref="byte" />.
  /// </summary>
  /// <param name="address">The address to convert.</param>
  /// <returns>The address as a byte value.</returns>
  public static explicit operator byte(I2cAddress address) => address.address;

  /// <summary>
  /// Converts an <see cref="I2cAddress" /> to an <see cref="int" />.
  /// </summary>
  /// <param name="address">The address to convert.</param>
  /// <returns>The address as an integer value.</returns>
  public static explicit operator int(I2cAddress address) => address.address;

  /// <summary>
  /// Converts a <see cref="byte" /> to an <see cref="I2cAddress" />.
  /// </summary>
  /// <param name="address">The byte value to convert.</param>
  /// <returns>The created <see cref="I2cAddress" /> instance.</returns>
  public static implicit operator I2cAddress(byte address) => new(address);

  /// <summary>
  /// Converts the address to a <see cref="byte" />.
  /// </summary>
  /// <returns>
  /// The address as a byte value.
  /// </returns>
  public byte ToByte() => address;

  /// <summary>
  /// Converts the address to an <see cref="int" />.
  /// </summary>
  /// <returns>
  /// The address as an integer value.
  /// </returns>
  public int ToInt32() => address;

  /// <summary>
  /// Creates an <see cref="I2cAddress" /> from a <see cref="byte" /> value.
  /// </summary>
  /// <param name="address">
  /// The byte value representing the I2C address.
  /// </param>
  /// <returns>
  /// The created <see cref="I2cAddress" /> instance.
  /// </returns>
  public static I2cAddress FromByte(byte address) => new(address);

  internal byte GetReadAddress() => (byte)((address << 1) | 0b_0000_0001);
  internal byte GetWriteAddress() => (byte)(address << 1);

  /// <summary>
  /// Converts the I2C address represented by the current instance
  /// into a hexadecimal string prefixed with <c>0x</c>.
  /// </summary>
  public override string ToString() => $"0x{address:X2}";
}
