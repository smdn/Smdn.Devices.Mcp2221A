// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class I2cAddressTests {
  [Test] public void Constants_Zero() => Assert.That((byte)I2cAddress.Zero, Is.Zero);
  [Test] public void Constants_DeviceMinValue() => Assert.That((byte)I2cAddress.DeviceMinValue, Is.EqualTo(0x08));
  [Test] public void Constants_DeviceMaxValue() => Assert.That((byte)I2cAddress.DeviceMaxValue, Is.EqualTo(0x77));

  [TestCase(0x08)]
  [TestCase(0x10)]
  [TestCase(0x70)]
  [TestCase(0x77)]
  public void Construct_FromAddress(int address)
  {
    Assert.That(() => _ = new I2cAddress(address), Throws.Nothing);
    Assert.That((byte)new I2cAddress(address), Is.EqualTo(address));
  }

  [TestCase(0x00)]
  [TestCase(0x07)]
  [TestCase(0x78)]
  [TestCase(0xFF)]
  [TestCase(0x100)]
  public void Construct_FromAddress_ArgumentOutOfRange(int address)
    => Assert.That(
      () => _ = new I2cAddress(address: address),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("address")
    );

  [TestCase(0b_0_0001_000, 0b000, 0b_0_0001_000)]
  [TestCase(0b_0_0001_000, 0b111, 0b_0_0001_111)]
  [TestCase(0b_0_0001_111, 0b000, 0b_0_0001_000)]
  [TestCase(0b_0_1110_000, 0b000, 0b_0_1110_000)]
  [TestCase(0b_0_1110_000, 0b111, 0b_0_1110_111)]
  [TestCase(0b_0_1110_111, 0b000, 0b_0_1110_000)]
  public void Construct_FromDeviceAndHardwareAddressBits(int deviceBits, int hardwareBits, byte expectedAddress)
    => Assert.That(
      new I2cAddress(deviceBits, hardwareBits),
      Is.EqualTo(new I2cAddress(expectedAddress))
    );

  [TestCase(-1, 0)]
  [TestCase(0b_0_0000_000, 0b000)]
  [TestCase(0b_0_0000_000, 0b111)]
  [TestCase(0b_0_0000_000, 0b1000)]
  [TestCase(0b_0_1111_000, 0b000)]
  [TestCase(0b_0_1111_000, 0b111)]
  [TestCase(0b_0_1111_000, 0b1000)]
  public void Construct_FromDeviceAndHardwareAddressBits_DeviceBitsArgumentOutOfRange(int deviceBits, int hardwareBits)
    => Assert.That(
      () => _ = new I2cAddress(deviceAddressBits: deviceBits, hardwareBits),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("deviceAddressBits")
      );

  [TestCase(0b_0_0001_000, -1)]
  [TestCase(0b_0_0001_000, 0b1000)]
  [TestCase(0b_0_1110_000, -1)]
  [TestCase(0b_0_1110_000, 0b1000)]
  public void Construct_FromDeviceAndHardwareAddressBits_HardwareBitsArgumentOutOfRange(int deviceBits, int hardwareBits)
    => Assert.That(
      () => _ = new I2cAddress(deviceBits, hardwareAddressBits: hardwareBits),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("hardwareAddressBits")
    );

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, false)]
  public void Equals_ToI2cAddress(int address, int addressOther, bool expectedValue)
    => Assert.That((new I2cAddress(address)).Equals(new I2cAddress(addressOther)), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, false)]
  public void Equals_ToInt(int address, int addressOther, bool expectedValue)
    => Assert.That((new I2cAddress(address)).Equals(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, false)]
  public void Equals_ToByte(int address, byte addressOther, bool expectedValue)
    => Assert.That((new I2cAddress(address)).Equals(addressOther), Is.EqualTo(expectedValue));

  [Test]
  public void Equals_ToObject()
  {
    var address = new I2cAddress(0x10);

    object val0_0 = new I2cAddress(0x10); Assert.That(address.Equals(val0_0), Is.True);
    object val0_1 = new I2cAddress(0x11); Assert.That(address.Equals(val0_1), Is.False);
    object val1_0 = (int)0x10; Assert.That(address.Equals(val1_0), Is.True);
    object val1_1 = (int)0x11; Assert.That(address.Equals(val1_1), Is.False);
    object val2_0 = (byte)0x10; Assert.That(address.Equals(val2_0), Is.True);
    object val2_1 = (byte)0x11; Assert.That(address.Equals(val2_1), Is.False);
    object? val3 = null; Assert.That(address.Equals(val3), Is.False);
    object val4 = 16.0; Assert.That(address.Equals(val4), Is.False);
  }

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, false)]
  public void OperatorEquality(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) == new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, false)]
  [TestCase(0x10, 0x11, true)]
  public void OperatorInequality(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) != new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, false)]
  [TestCase(0x10, 0x11, false)]
  [TestCase(0x11, 0x10, true)]
  public void OperatorGreaterThan(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) > new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, false)]
  [TestCase(0x11, 0x10, true)]
  public void OperatorGreaterThanOrEqualsTo(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) >= new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, false)]
  [TestCase(0x10, 0x11, true)]
  [TestCase(0x11, 0x10, false)]
  public void OperatorLessThan(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) < new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, true)]
  [TestCase(0x10, 0x11, true)]
  [TestCase(0x11, 0x10, false)]
  public void OperatorLessThanOrEqualsTo(int address, int addressOther, bool expectedValue)
    => Assert.That(new I2cAddress(address) <= new I2cAddress(addressOther), Is.EqualTo(expectedValue));

  [TestCase(0x10, 0x10, 0)]
  [TestCase(0x10, 0x11, -1)]
  [TestCase(0x11, 0x10, 1)]
  public void CompareTo_I2cAddress(int address, int addressOther, int expectedValue)
    => Assert.That((new I2cAddress(address)).CompareTo(new I2cAddress(addressOther)), Is.EqualTo(expectedValue));

  [TestCase(0x08, 0x08)]
  [TestCase(0x10, 0x10)]
  [TestCase(0x70, 0x70)]
  [TestCase(0x77, 0x77)]
  public void ToByte(int address, byte expectedValue)
    => Assert.That(new I2cAddress(address).ToByte(), Is.EqualTo(expectedValue));

  [TestCase(0x08, 0x08)]
  [TestCase(0x10, 0x10)]
  [TestCase(0x70, 0x70)]
  [TestCase(0x77, 0x77)]
  public void ExplicitTypeConversion_ToByte(int address, byte expectedValue)
    => Assert.That((byte)new I2cAddress(address), Is.EqualTo(expectedValue));

  [TestCase(0x08, 0x08)]
  [TestCase(0x10, 0x10)]
  [TestCase(0x70, 0x70)]
  [TestCase(0x77, 0x77)]
  public void ToInt32(int address, int expectedValue)
    => Assert.That(new I2cAddress(address).ToInt32(), Is.EqualTo(expectedValue));

  [TestCase(0x08, 0x08)]
  [TestCase(0x10, 0x10)]
  [TestCase(0x70, 0x70)]
  [TestCase(0x77, 0x77)]
  public void ExplicitTypeConversion_ToInt32(int address, int expectedValue)
    => Assert.That((int)new I2cAddress(address), Is.EqualTo(expectedValue));

  [TestCase(0x08)]
  [TestCase(0x10)]
  [TestCase(0x70)]
  [TestCase(0x77)]
  public void FromByte(byte address)
    => Assert.That((byte)I2cAddress.FromByte(address), Is.EqualTo(address));

  [TestCase(0x08)]
  [TestCase(0x10)]
  [TestCase(0x70)]
  [TestCase(0x77)]
  public void ImplicitTypeConversion_FromByte(byte address)
  {
    I2cAddress addr = address;

    Assert.That((byte)addr, Is.EqualTo(address));
  }

  [TestCase(0x08, "0x08")]
  [TestCase(0x0F, "0x0F")] // upper case
  [TestCase(0x10, "0x10")]
  [TestCase(0x70, "0x70")]
  [TestCase(0x77, "0x77")]
  public void ToString_WithoutArguments(byte address, string expected)
    => Assert.That(((I2cAddress)address).ToString(), Is.EqualTo(expected));
}
