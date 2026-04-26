// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0055
public readonly struct I2cAddress :
  IEquatable<I2cAddress>,
  IEquatable<int>,
  IEquatable<byte>,
  IComparable<I2cAddress>
{
#pragma warning disable IDE0055
  public static I2cAddress Zero { get; } = default;
  public static I2cAddress DeviceMinValue { get; } = new((byte)0b_0_0001_000u);
  public static I2cAddress DeviceMaxValue { get; } = new((byte)0b_0_1110_111u);

  private readonly byte address;

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

  public I2cAddress(int address)
    : this(ValidateAddress((uint)address, nameof(address)))
  {
  }

  private I2cAddress(byte address)
  {
    this.address = address;
  }

  public bool Equals(I2cAddress other) => this.address == other.address;
  public bool Equals(int other) => this.address == other;
  public bool Equals(byte other) => this.address == other;
  public override bool Equals(object? obj) => obj switch {
    null => false,
    I2cAddress other => this.Equals(other),
    int other => this.Equals(other),
    byte other => this.Equals(other),
    _ => false,
  };
  public override int GetHashCode() => address.GetHashCode();
  public static bool operator ==(I2cAddress x, I2cAddress y) => x.Equals(y);
  public static bool operator !=(I2cAddress x, I2cAddress y) => !x.Equals(y);

  public int CompareTo(I2cAddress other) => Comparer<byte>.Default.Compare(this.address, other.address);
  public static bool operator <(I2cAddress left, I2cAddress right) => left.address < right.address;
  public static bool operator <=(I2cAddress left, I2cAddress right) => left.address <= right.address;
  public static bool operator >(I2cAddress left, I2cAddress right) => left.address > right.address;
  public static bool operator >=(I2cAddress left, I2cAddress right) => left.address >= right.address;

  public static explicit operator byte(I2cAddress address) => address.address;
  public static explicit operator int(I2cAddress address) => address.address;
  public static implicit operator I2cAddress(byte address) => new(address);

  public byte ToByte() => address;
  public int ToInt32() => address;

  public static I2cAddress FromByte(byte address) => new(address);

  internal byte GetReadAddress() => (byte)((address << 1) | 0b_0000_0001);
  internal byte GetWriteAddress() => (byte)(address << 1);

  public override string ToString() => $"0x{address:X2}";
}
