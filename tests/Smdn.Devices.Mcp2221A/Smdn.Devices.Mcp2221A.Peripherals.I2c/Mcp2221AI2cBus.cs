// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

[TestFixture]
public class Mcp2221AI2cBusTests {
  private static Mcp2221A CreateFromPseudoDevice()
  {
    var baseDevice = Mcp2221ATests.CreatePseudoDevice();

    return Mcp2221A.Create(baseDevice, shouldDisposeUsbHidDevice: true);
  }

  [Test]
  public void Dispose()
  {
    using var device = CreateFromPseudoDevice();

    Assert.That(device.I2c.Dispose, Throws.Nothing);

    Assert.That(() => _ = device.I2c, Throws.Nothing);
    Assert.That(() => _ = device.I2c.CreateDevice(0x40), Throws.Nothing);

    Assert.That(device.I2c.Dispose, Throws.Nothing, "dispose again");
  }

  [Test]
  public void OwnerDisposed()
  {
    using var device = CreateFromPseudoDevice();

    Assert.That(() => _ = device.I2c, Throws.Nothing);

    var i2cBus = device.I2c;

    device.Dispose();

    Assert.That(() => _ = device.I2c, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = i2cBus.CreateDevice(0x40), Throws.TypeOf<ObjectDisposedException>());
  }

  [Test]
  public void CreateDevice_FromIntAddress()
  {
    const int DeviceAddress = 0x40;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(DeviceAddress);

    Assert.That(i2cDevice, Is.Not.Null);
    Assert.That(i2cDevice.ConnectionSettings, Is.Not.Null);
    Assert.That(i2cDevice.ConnectionSettings.DeviceAddress, Is.EqualTo(DeviceAddress));
    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(100));

    using var i2cSameAddressDevice = device.I2c.CreateDevice(DeviceAddress);

    Assert.That(
      i2cSameAddressDevice,
      Is.Not.SameAs(i2cDevice),
      "different instances may be created even for the same address"
    );
  }

  [Test]
  public void CreateDevice_FromI2cAddress()
  {
    const int DeviceAddressNumber = 0x40;
    var deviceAddress = new I2cAddress(DeviceAddressNumber);

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(deviceAddress);

    Assert.That(i2cDevice, Is.Not.Null);
    Assert.That(i2cDevice.ConnectionSettings, Is.Not.Null);
    Assert.That(i2cDevice.ConnectionSettings.DeviceAddress, Is.EqualTo(deviceAddress));
    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(100));

    using var i2cSameAddressDevice = device.I2c.CreateDevice(deviceAddress);

    Assert.That(
      i2cSameAddressDevice,
      Is.Not.SameAs(i2cDevice),
      "different instances may be created even for the same address"
    );
  }

  [TestCase(100)]
  [TestCase(200)]
  [TestCase(400)]
  public void CreateDevice_FromI2cAddress_WithTransmissionSpeed(int transmissionSpeedInKbps)
  {
    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40, transmissionSpeedInKbps: transmissionSpeedInKbps);

    Assert.That(i2cDevice, Is.Not.Null);
    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(transmissionSpeedInKbps));
  }

  [Test]
  public void RemoveDevice_WithIntAddress()
  {
    const int DeviceAddress = 0x40;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(DeviceAddress);

    Assert.That(
      () => device.I2c.RemoveDevice(DeviceAddress),
      Throws.Nothing
    );
    Assert.That(
      () => device.I2c.RemoveDevice(DeviceAddress),
      Throws.Nothing,
      "remove again"
    );

#if false
    Assert.That(
      () => _ = i2cDevice.ReadByte(),
      Throws.Nothing,
      "still available"
    );
#endif
  }

  [Test]
  public void RemoveDevice_WithI2cAddress()
  {
    const int DeviceAddressNumber = 0x40;
    var deviceAddress = new I2cAddress(DeviceAddressNumber);

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(deviceAddress);

    Assert.That(
      () => device.I2c.RemoveDevice(deviceAddress),
      Throws.Nothing
    );
    Assert.That(
      () => device.I2c.RemoveDevice(deviceAddress),
      Throws.Nothing,
      "remove again"
    );

#if false
    Assert.That(
      () => _ = i2cDevice.ReadByte(),
      Throws.Nothing,
      "still available"
    );
#endif
  }
}
