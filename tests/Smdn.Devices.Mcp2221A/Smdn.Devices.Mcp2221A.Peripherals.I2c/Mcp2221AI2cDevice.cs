// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

[TestFixture]
public class Mcp2221AI2cDeviceTests {
  private static Mcp2221A CreateFromPseudoDevice()
  {
    var baseDevice = Mcp2221ATests.CreatePseudoDevice();

    return Mcp2221A.Create(baseDevice, shouldDisposeUsbHidDevice: true);
  }

  [Test]
  public void Dispose(
    [Values] bool shouldDisposeMcp2221A
  )
  {
    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40, shouldDisposeMcp2221A: shouldDisposeMcp2221A);

    Assert.That(i2cDevice.Dispose, Throws.Nothing);

    Assert.That(() => i2cDevice.Read(buffer: default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = i2cDevice.ReadByte(), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cDevice.ReadAsync(buffer: default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cDevice.Write(buffer: default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cDevice.WriteByte(value: default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cDevice.WriteAsync(buffer: default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cDevice.WriteRead(writeBuffer: default, readBuffer: default), Throws.TypeOf<ObjectDisposedException>());

    Assert.That(
      () => _ = device.I2c,
      shouldDisposeMcp2221A
        ? Throws.TypeOf<ObjectDisposedException>()
        : Throws.Nothing
    );

    Assert.That(i2cDevice.Dispose, Throws.Nothing, "dispose again");
  }

  [Test]
  public void II2cDevice_Controller()
  {
    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40);

    Assert.That((i2cDevice as II2cDevice).Controller, Is.SameAs(device.I2c));
  }

  [Test]
  public void II2cDevice_Address()
  {
    const int DeviceAddress = 0x40;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(DeviceAddress);

    Assert.That((i2cDevice as II2cDevice).Address, Is.EqualTo(new I2cAddress(DeviceAddress)));
  }

  [Test]
  public void ConnectionSettings()
  {
    const int DeviceAddress = 0x40;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(DeviceAddress);

    var connectionSettings = i2cDevice.ConnectionSettings;

    Assert.That(connectionSettings, Is.Not.Null);
    Assert.That(connectionSettings.DeviceAddress, Is.EqualTo(DeviceAddress));

    Assert.That(i2cDevice.ConnectionSettings, Is.SameAs(connectionSettings));
  }

  [TestCase(1)]
  [TestCase(100)]
  [TestCase(400)]
  [TestCase(1000)]
  [TestCase(int.MaxValue)]
  public void TransmissionSpeedInKbps(
    int newTransmissionSpeedInKbps
  )
  {
    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40);

    Assert.That(() => i2cDevice.TransmissionSpeedInKbps = newTransmissionSpeedInKbps, Throws.Nothing);
    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(newTransmissionSpeedInKbps));
  }

  [TestCase(0)]
  [TestCase(-1)]
  [TestCase(int.MinValue)]
  public void TransmissionSpeedKbps_OutOfRange(
    int newTransmissionSpeedInKbps
  )
  {
    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40);

    Assert.That(
      () => i2cDevice.TransmissionSpeedInKbps = newTransmissionSpeedInKbps,
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
          .Property(nameof(ArgumentOutOfRangeException.ParamName))
          .EqualTo(nameof(i2cDevice.TransmissionSpeedInKbps))
        .With
          .Property(nameof(ArgumentOutOfRangeException.ActualValue))
          .EqualTo(newTransmissionSpeedInKbps)
    );
  }

  [Test]
  public void WithStandardMode()
  {
    const int InitialTransmissionSpeed = 200;
    const int ExpectedTransmissionSpeed = 100;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40, transmissionSpeedInKbps: InitialTransmissionSpeed);

    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(InitialTransmissionSpeed));

    Assert.That(
      i2cDevice.WithStandardMode(),
      Is.SameAs(i2cDevice)
    );

    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(ExpectedTransmissionSpeed));
  }

  [Test]
  public void WithFastMode()
  {
    const int InitialTransmissionSpeed = 200;
    const int ExpectedTransmissionSpeed = 400;

    using var device = CreateFromPseudoDevice();
    using var i2cDevice = device.I2c.CreateDevice(0x40, transmissionSpeedInKbps: InitialTransmissionSpeed);

    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(InitialTransmissionSpeed));

    Assert.That(
      i2cDevice.WithFastMode(),
      Is.SameAs(i2cDevice)
    );

    Assert.That(i2cDevice.TransmissionSpeedInKbps, Is.EqualTo(ExpectedTransmissionSpeed));
  }
}