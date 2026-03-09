// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public partial class Mcp2221ATests {
  private const byte ReportInput = 0x00;
  private const byte ReportOutput = 0x00;

  internal const string DefaultManufacturer = "Microchip Technology Inc.";
  internal const string DefaultProduct = "MCP2221 USB-I2C/UART Combo";
  internal const string DefaultSerialNumber = "XXXXXXXXXX";
  internal const string DefaultChipFactorySerialNumber = "01234567";

  internal static PseudoUsbHidDevice CreatePseudoDevice(
    int vendorId = Mcp2221A.DeviceVendorId,
    int productId = Mcp2221A.DeviceProductId,
    byte hardwareRevisionMajor = (byte)'A', // = MCP2221/MCP2221A,
    byte hardwareRevisionMinor = (byte)'6', // = MCP2221/MCP2221A,
    byte firmwareRevisionMajor = (byte)'1', // = MCP2221/MCP2221A,
    byte firmwareRevisionMinor = (byte)'2', // = MCP2221A,
    string manufacturer = DefaultManufacturer,
    string product = DefaultProduct,
    string serialNumber = DefaultSerialNumber,
    string chipFactorySerialNumber = DefaultChipFactorySerialNumber
  )
    => new(
      vendorId: vendorId,
      productId: productId,
      productName: product,
      manufacturer: manufacturer,
      serialNumber: serialNumber,
      createWriteStream: () => new MemoryStream(capacity: (1 + 64) * 5),
      createReadStream: () => {
        var readStream = new MemoryStream(capacity: (1 + 64) * 5);

        readStream.Write([
          // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
          ReportInput, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x03, 0x00, 0x03, 0x14, 0x00, 0x40, 0x00, 0x10, 0x28, 0x00, 0x60, 0x01, 0x01, 0x00, 0x00, 0xF1, 0x79, 0xF0, 0x00, 0x00, 0x00, 0x30, 0x30, 0x0B, 0x30, 0x14, 0x23, 0x17, 0x7D, 0x06, 0x00, 0x00, 0x26, 0x94, 0x14, hardwareRevisionMajor, hardwareRevisionMinor, firmwareRevisionMajor, firmwareRevisionMinor, 0xFB, 0x03, 0x00, 0x00, 0xFA, 0x03, 0x76, 0x03, 0x5B, 0x02, 0x00, 0x00, 0x00, 0x00,
        ]);

        const int DescriptorOffset = 2;

        var manufacturerDescriptor = new byte[64 - 4];
        var manufacturerDescriptorLength = (byte)Encoding.Unicode.GetBytes(manufacturer, manufacturerDescriptor);

        readStream.Write([
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-7 RESPONSE STRUCTURE - READ USB MANUFACTURER DESCRIPTOR STRING SUB-COMMAND
          ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + manufacturerDescriptorLength), 0x03, .. manufacturerDescriptor
        ]);

        var productDescriptor = new byte[64 - 4];
        var productDescriptorLength = Encoding.Unicode.GetBytes(product, productDescriptor);

        readStream.Write([
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-8 RESPONSE STRUCTURE - READ USB PRODUCT DESCRIPTOR STRING SUB-COMMAND
          ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + productDescriptorLength), 0x03, .. productDescriptor
        ]);

        var serialNumberDescriptor = new byte[64 - 4];
        var serialNumberDescriptorLength = Encoding.Unicode.GetBytes(serialNumber, serialNumberDescriptor);

        readStream.Write([
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-9 RESPONSE STRUCTURE - READ USB SERIAL NUMBER DESCRIPTOR STRING SUB-COMMAND
          ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + serialNumberDescriptorLength), 0x03, .. serialNumberDescriptor
        ]);

        var chipFactorySerialNumberDescriptor = new byte[64 - 4];
        var chipFactorySerialNumberDescriptorLength = Encoding.ASCII.GetBytes(chipFactorySerialNumber, chipFactorySerialNumberDescriptor);

        readStream.Write([
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-10 RESPONSE STRUCTURE - READ CHIP FACTORY SERIAL NUMBER SUB-COMMAND
          ReportInput, 0xB0, 0x00, (byte)chipFactorySerialNumberDescriptorLength, 0x00, .. chipFactorySerialNumberDescriptor
        ]);

        readStream.Position = 0L;

        return readStream;
      }
    );

  [Test]
  public void CreateAsync()
  {
    Mcp2221A? device = null;

    try {
      Assert.That(async () => device = await Mcp2221A.CreateAsync(CreatePseudoDevice(), shouldDisposeUsbHidDevice: true), Throws.Nothing);

      Assert.That(device, Is.Not.Null);
      Assert.That(device.HidDevice, Is.Not.Null);

      AssertPseudoDeviceWithDefaultConfiguration(device);
    }
    finally {
      device?.Dispose();
    }
  }

  [Test]
  public void Create()
  {
    Mcp2221A? device = null;

    try {
      Assert.That(() => device = Mcp2221A.Create(CreatePseudoDevice(), shouldDisposeUsbHidDevice: true), Throws.Nothing);

      Assert.That(device, Is.Not.Null);
      Assert.That(device.HidDevice, Is.Not.Null);

      AssertPseudoDeviceWithDefaultConfiguration(device);
    }
    finally {
      device?.Dispose();
    }
  }

  private static void AssertPseudoDeviceWithDefaultConfiguration(Mcp2221A device)
  {
    Assert.That(device.FirmwareRevision, Is.EqualTo("1.2"), nameof(device.FirmwareRevision));
    Assert.That(device.HardwareRevision, Is.EqualTo("A.6"), nameof(device.HardwareRevision));
    Assert.That(device.ManufacturerDescriptor, Is.EqualTo(DefaultManufacturer), nameof(device.ManufacturerDescriptor));
    Assert.That(device.ProductDescriptor, Is.EqualTo(DefaultProduct), nameof(device.ProductDescriptor));
    Assert.That(device.SerialNumberDescriptor, Is.EqualTo(DefaultSerialNumber), nameof(device.SerialNumberDescriptor));
    Assert.That(device.ChipFactorySerialNumber, Is.EqualTo(DefaultChipFactorySerialNumber), nameof(device.ChipFactorySerialNumber));
  }

  [Test]
  public async Task Dispose(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => await TestDispose(shouldDisposeUsbHidDevice, d => { d.Dispose(); return Task.CompletedTask; });

  [Test]
  public async Task DisposeAsync(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => await TestDispose(shouldDisposeUsbHidDevice, async d => await d.DisposeAsync());

  private async Task TestDispose(bool shouldDisposeUsbHidDevice, Func<Mcp2221A, Task> disposeAction)
  {
    using var baseDevice = CreatePseudoDevice();
    await using var device = await Mcp2221A.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice);

    Assert.That(() => _ = device.HidDevice, Throws.Nothing);

    Assert.That(() =>_ = device.HardwareRevision, Throws.Nothing);
    Assert.That(() =>_ = device.FirmwareRevision, Throws.Nothing);
    Assert.That(() =>_ = device.ManufacturerDescriptor, Throws.Nothing);
    Assert.That(() =>_ = device.ProductDescriptor, Throws.Nothing);
    Assert.That(() =>_ = device.SerialNumberDescriptor, Throws.Nothing);
    Assert.That(() =>_ = device.ChipFactorySerialNumber, Throws.Nothing);
    Assert.That(() =>_ = device.GPs, Throws.Nothing);
    Assert.That(() =>_ = device.GP0, Throws.Nothing);
    Assert.That(() =>_ = device.GP1, Throws.Nothing);
    Assert.That(() =>_ = device.GP2, Throws.Nothing);
    Assert.That(() =>_ = device.GP3, Throws.Nothing);
    Assert.That(() =>_ = device.I2c, Throws.Nothing);

    var i2c = device.I2c;

    await disposeAction(device);

    Assert.That(() => _ = device.HidDevice, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.I2c, Throws.TypeOf<ObjectDisposedException>());

    Assert.That(() => _ = device.HardwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.FirmwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.ManufacturerDescriptor, Throws.Nothing);
    Assert.That(() => _ = device.ProductDescriptor, Throws.Nothing);
    Assert.That(() => _ = device.SerialNumberDescriptor, Throws.Nothing);
    Assert.That(() => _ = device.ChipFactorySerialNumber, Throws.Nothing);
    Assert.That(() => _ = device.GPs, Throws.Nothing);
    Assert.That(() => _ = device.GP0, Throws.Nothing);
    Assert.That(() => _ = device.GP1, Throws.Nothing);
    Assert.That(() => _ = device.GP2, Throws.Nothing);
    Assert.That(() => _ = device.GP3, Throws.Nothing);

    Assert.That(async () => await device.GP0.SetValueAsync(default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => device.GP0.SetValueAsync(default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => device.GP0.SetValue(default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(async () => await device.GP0.GetValueAsync(), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => device.GP0.GetValueAsync(), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => device.GP0.GetValue(), Throws.TypeOf<ObjectDisposedException>());

    Assert.That(async () => await i2c.WriteAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2c.WriteAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2c.Write(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(async () => await i2c.ReadAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2c.ReadAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2c.Read(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(baseDevice.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice), "USB-HID device disposed");

    Assert.That(async () => await disposeAction(device), Throws.Nothing, "dispose again");
  }

  private static System.Collections.IEnumerable YieldTestCases_Create_HardwareOrFirmwareNotSupported()
  {
    const bool ThrowsException = true;
    const bool ThrowsNothing = false;

    // MCP2221
    yield return new object[] { (byte)'A', (byte)'6', (byte)'1', (byte)'1', ThrowsNothing };

    // MCP2221A
    yield return new object[] { (byte)'A', (byte)'6', (byte)'1', (byte)'2', ThrowsNothing };

    // unknown hardware revision (major)
    yield return new object[] { (byte)'B', (byte)'6', (byte)'1', (byte)'1', ThrowsException };

    // unknown hardware revision (minor)
    yield return new object[] { (byte)'A', (byte)'7', (byte)'1', (byte)'1', ThrowsException };

    // unknown firmware revision (major)
    yield return new object[] { (byte)'A', (byte)'6', (byte)'0', (byte)'1', ThrowsException };
    yield return new object[] { (byte)'A', (byte)'6', (byte)'2', (byte)'0', ThrowsException };

    // unknown firmware revision (minor)
    yield return new object[] { (byte)'A', (byte)'6', (byte)'1', (byte)'0', ThrowsException };
    yield return new object[] { (byte)'A', (byte)'6', (byte)'1', (byte)'3', ThrowsException };
  }

  [TestCaseSource(nameof(YieldTestCases_Create_HardwareOrFirmwareNotSupported))]
  public void CreateAsync_HardwareOrFirmwareNotSupported(
    char hardwareRevisionMajor,
    char hardwareRevisionMinor,
    char firmwareRevisionMajor,
    char firmwareRevisionMinor,
    bool expectExceptionThrown
  )
  {
    using var baseDevice = CreatePseudoDevice(
      hardwareRevisionMajor: (byte)hardwareRevisionMajor,
      hardwareRevisionMinor: (byte)hardwareRevisionMinor,
      firmwareRevisionMajor: (byte)firmwareRevisionMajor,
      firmwareRevisionMinor: (byte)firmwareRevisionMinor
    );

    Mcp2221A? device = null;

    Assert.That(
      async () => device = await Mcp2221A.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: true),
      expectExceptionThrown ? Throws.TypeOf<Mcp2221ANotSupportedException>() : Throws.Nothing
    );
    Assert.That(baseDevice.IsDisposed, Is.EqualTo(expectExceptionThrown));

    device?.Dispose();
  }

  [TestCaseSource(nameof(YieldTestCases_Create_HardwareOrFirmwareNotSupported))]
  public void Create_HardwareOrFirmwareNotSupported(
    char hardwareRevisionMajor,
    char hardwareRevisionMinor,
    char firmwareRevisionMajor,
    char firmwareRevisionMinor,
    bool expectExceptionThrown
  )
  {
    using var baseDevice = CreatePseudoDevice(
      hardwareRevisionMajor: (byte)hardwareRevisionMajor,
      hardwareRevisionMinor: (byte)hardwareRevisionMinor,
      firmwareRevisionMajor: (byte)firmwareRevisionMajor,
      firmwareRevisionMinor: (byte)firmwareRevisionMinor
    );

    Mcp2221A? device = null;

    Assert.That(
      () => device = Mcp2221A.Create(baseDevice, shouldDisposeUsbHidDevice: true),
      expectExceptionThrown ? Throws.TypeOf<Mcp2221ANotSupportedException>() : Throws.Nothing
    );
    Assert.That(baseDevice.IsDisposed, Is.EqualTo(expectExceptionThrown));

    device?.Dispose();
  }
}
