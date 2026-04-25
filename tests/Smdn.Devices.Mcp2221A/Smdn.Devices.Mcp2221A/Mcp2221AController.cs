// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public partial class Mcp2221AControllerTests {
  private const byte ReportInput = 0x00;
  private const byte ReportOutput = 0x00;
  private const int CommandLength = 64;
  private const int ReportLength = 1 + CommandLength;

  internal const string DefaultManufacturer = "Microchip Technology Inc.";
  internal const string DefaultProduct = "MCP2221 USB-I2C/UART Combo";
  internal const string DefaultSerialNumber = "XXXXXXXXXX";
  internal const string DefaultChipFactorySerialNumber = "01234567";

  internal static PseudoUsbHidDevice CreatePseudoDevice(
    int vendorId = Mcp2221AController.DefaultVendorId,
    int productId = Mcp2221AController.DefaultProductId,
    byte hardwareRevisionMajor = (byte)'A', // = MCP2221/MCP2221A,
    byte hardwareRevisionMinor = (byte)'6', // = MCP2221/MCP2221A,
    byte firmwareRevisionMajor = (byte)'1', // = MCP2221/MCP2221A,
    byte firmwareRevisionMinor = (byte)'2', // = MCP2221A,
    string manufacturer = DefaultManufacturer,
    string product = DefaultProduct,
    string serialNumber = DefaultSerialNumber,
    string chipFactorySerialNumber = DefaultChipFactorySerialNumber,
    byte gp0Settings = 0b_000_1_0_010, // Output: HIGH, Alternate Function 0 (LED UART RX)
    byte gp1Settings = 0b_000_1_0_011, // Output: HIGH, Alternate Function 1 (LED UART TX)
    byte gp2Settings = 0b_000_1_0_001, // Output: HIGH, Dedicated function operation (USBCFG)
    byte gp3Settings = 0b_000_1_0_001, // Output: HIGH, Dedicated function operation (LED I2C)
    byte chipSettings1 = 0b_000_00_000, // CLKDC(4-3): Duty cycle 0%, CLKDIV(2-0): Reserved
    byte chipSettings2 = 0b_00_0_00000, // DACVRM(7-6): VRM is OFF, DACREF(5): VDD, DACVAL(4-0): 0
    byte chipSettings3 = 0b_0_0_0_00_0_00, // INTDETFEEN(6): Disable, INTDETREEN(5): Disable, ADCVRM(4-3): VRM is off, ADCREF(2): VDD
    bool interruptEdgeDetectorState = false
  )
    => new(
      vendorId: vendorId,
      productId: productId,
      productName: product,
      manufacturer: manufacturer,
      serialNumber: serialNumber,
      createWriteStream: () => new MemoryStream(capacity: ReportLength * 5),
      createReadStream: () => {
        var readStream = new MemoryStream(capacity: ReportLength * 5);

        readStream.Write(
          // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
          [
            ReportInput,
            0x10, 0x00,
            0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, // [5-7] Don't care
            0x00, 0x03, 0x00, 0x03, 0x00, 0x03, 0x14, 0x00, 0x40, 0x00,
            0x10, 0x28, 0x00, 0x60, // [18-21] Don't care
            0x01, // [22] SCL line value as read from the pin
            0x01, // [23] SDA line value as read from the pin
            (byte)(interruptEdgeDetectorState ? 0x01 : 0x00),
            0x00, // [25] I2C Read pending value
            0xF1, 0x79, 0xF0, 0x00, 0x00, 0x00, 0x30, 0x30, 0x0B, 0x30, // [26-45] Don't care
            0x14, 0x23, 0x17, 0x7D, 0x06, 0x00, 0x00, 0x26, 0x94, 0x14, // [26-45] Don't care
            hardwareRevisionMajor, hardwareRevisionMinor, firmwareRevisionMajor, firmwareRevisionMinor,
            0xFB, 0x03, 0x00, 0x00, 0xFA, 0x03, // [50-55] ADC Data (16-bit) values
            0x76, 0x03, 0x5B, 0x02, 0x00, 0x00, 0x00, 0x00 // [56-63] Don't care
          ]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        const int DescriptorOffset = 2;

        var manufacturerDescriptor = new byte[64 - 4];
        var manufacturerDescriptorLength = (byte)Encoding.Unicode.GetBytes(
#if SYSTEM_TEXT_ENCODING_GETBYTES_READONLYSPAN_OF_CHAR
          manufacturer,
          manufacturerDescriptor
#else
          manufacturer,
          0,
          manufacturer.Length,
          manufacturerDescriptor,
          0
#endif
        );

        readStream.Write(
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-7 RESPONSE STRUCTURE - READ USB MANUFACTURER DESCRIPTOR STRING SUB-COMMAND
          [ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + manufacturerDescriptorLength), 0x03, .. manufacturerDescriptor]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        var productDescriptor = new byte[64 - 4];
        var productDescriptorLength = Encoding.Unicode.GetBytes(
#if SYSTEM_TEXT_ENCODING_GETBYTES_READONLYSPAN_OF_CHAR
          product,
          productDescriptor
#else
          product,
          0,
          product.Length,
          productDescriptor,
          0
#endif
        );

        readStream.Write(
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-8 RESPONSE STRUCTURE - READ USB PRODUCT DESCRIPTOR STRING SUB-COMMAND
          [ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + productDescriptorLength), 0x03, .. productDescriptor]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        var serialNumberDescriptor = new byte[64 - 4];
        var serialNumberDescriptorLength = Encoding.Unicode.GetBytes(
#if SYSTEM_TEXT_ENCODING_GETBYTES_READONLYSPAN_OF_CHAR
          serialNumber,
          serialNumberDescriptor
#else
          serialNumber,
          0,
          serialNumber.Length,
          serialNumberDescriptor,
          0
#endif
        );

        readStream.Write(
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-9 RESPONSE STRUCTURE - READ USB SERIAL NUMBER DESCRIPTOR STRING SUB-COMMAND
          [ReportInput, 0xB0, 0x00, (byte)(DescriptorOffset + serialNumberDescriptorLength), 0x03, .. serialNumberDescriptor]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        var chipFactorySerialNumberDescriptor = new byte[64 - 4];
        var chipFactorySerialNumberDescriptorLength = Encoding.ASCII.GetBytes(
#if SYSTEM_TEXT_ENCODING_GETBYTES_READONLYSPAN_OF_CHAR
          chipFactorySerialNumber,
          chipFactorySerialNumberDescriptor
#else
          chipFactorySerialNumber,
          0,
          chipFactorySerialNumber.Length,
          chipFactorySerialNumberDescriptor,
          0
#endif
        );

        readStream.Write(
          // [MCP2221A] 3.1.2 READ FLASH DATA - TABLE 3-10 RESPONSE STRUCTURE - READ CHIP FACTORY SERIAL NUMBER SUB-COMMAND
          [ReportInput, 0xB0, 0x00, (byte)chipFactorySerialNumberDescriptorLength, 0x00, .. chipFactorySerialNumberDescriptor]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        var vendorIdHigh = (byte)(vendorId >> 8);
        var vendorIdLow = (byte)vendorId;
        var productIdHigh = (byte)(productId >> 8);
        var productIdLow = (byte)productId;

        readStream.Write(
          // [MCP2221A] 3.1.14 GET SRAM SETTINGS
          [
            ReportInput,
            0x61, 0x00,
            0x00, 0x00,
            0x00, chipSettings1, chipSettings2, chipSettings3,
            vendorIdHigh, vendorIdLow, productIdHigh, productIdLow,
            0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            gp0Settings, gp1Settings, gp2Settings, gp3Settings,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
          ]
#if !SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
          ,
          0,
          ReportLength
#endif
        );

        readStream.Position = 0L;

        return readStream;
      }
    );

  internal static void AppendPseudoResponse(
    Mcp2221AController mcp2221A,
    params string[] responseSequences
  )
    => AppendPseudoResponse(
      mcp2221A,
      verifyCommandLength: true,
      responseSequences
    );

  internal static void AppendPseudoResponse(
    Mcp2221AController mcp2221A,
    bool verifyCommandLength,
    params string[] responseSequences
  )
  {
    static byte[] ToByteArray(string hexByteSequence)
      => hexByteSequence.Length == 0 ? Array.Empty<byte>() : hexByteSequence.Split('-').Select(hex => Convert.ToByte(hex, 16)).ToArray();

    var endPoint = (mcp2221A.HidDevice as PseudoUsbHidDevice)!.EndPoint;

    if (!endPoint.CanRead)
      throw new InvalidOperationException("endpoint does not support reading");

    var currentPosition = endPoint.ReadStream!.Position;

    foreach (var sequence in responseSequences) {
      endPoint.ReadStream.WriteByte(ReportInput);

      var sequenceBytes = ToByteArray(sequence);

      if (verifyCommandLength && sequenceBytes.Length != CommandLength)
        throw new InvalidOperationException($"response sequence must be {CommandLength}-byte length (length: {sequenceBytes.Length}, sequence: '{sequence}')");

      endPoint.ReadStream.Write(
#if SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
        sequenceBytes
#else
        sequenceBytes,
        0,
        sequenceBytes.Length
#endif
      );
    }

    endPoint.ReadStream.Position = currentPosition;
  }

  internal static void ClearSentCommands(Mcp2221AController mcp2221A)
  {
    var endPoint = (mcp2221A.HidDevice as PseudoUsbHidDevice)!.EndPoint!;

    endPoint.WriteStream!.Position = 0L;
    endPoint.WriteStream!.SetLength(0L);
  }

  internal static Stream GetEndPointWriteStream(Mcp2221AController mcp2221A)
    => (mcp2221A.HidDevice as PseudoUsbHidDevice)!.EndPoint!.WriteStream!;

  internal static ReadOnlyMemory<byte> GetSentCommand(Mcp2221AController mcp2221A, int commandNumber = 0)
  {
    var stream = GetEndPointWriteStream(mcp2221A);

    if (stream.Length < ReportLength)
      throw new InvalidOperationException("report too short");

    var buffer = new byte[ReportLength];

    stream.Position = commandNumber * ReportLength;

    stream.ReadExactly(buffer.AsSpan(0, ReportLength));

    stream.Position = 0L;

    return buffer.AsMemory(1); // except report
  }

  [Test]
  public void CreateAsync()
  {
    Mcp2221AController? device = null;

    try {
      Assert.That(async () => device = await Mcp2221AController.CreateAsync(CreatePseudoDevice(), shouldDisposeUsbHidDevice: true), Throws.Nothing);

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
    Mcp2221AController? device = null;

    try {
      Assert.That(() => device = Mcp2221AController.Create(CreatePseudoDevice(), shouldDisposeUsbHidDevice: true), Throws.Nothing);

      Assert.That(device, Is.Not.Null);
      Assert.That(device.HidDevice, Is.Not.Null);

      AssertPseudoDeviceWithDefaultConfiguration(device);
    }
    finally {
      device?.Dispose();
    }
  }

  private static void AssertPseudoDeviceWithDefaultConfiguration(Mcp2221AController device)
  {
    Assert.That(device.FirmwareRevision, Is.EqualTo("1.2"), nameof(device.FirmwareRevision));
    Assert.That(device.HardwareRevision, Is.EqualTo("A.6"), nameof(device.HardwareRevision));
    Assert.That(device.Manufacturer, Is.EqualTo(DefaultManufacturer), nameof(device.Manufacturer));
    Assert.That(device.Product, Is.EqualTo(DefaultProduct), nameof(device.Product));
    Assert.That(device.SerialNumber, Is.EqualTo(DefaultSerialNumber), nameof(device.SerialNumber));
    Assert.That(device.ChipFactorySerialNumber, Is.EqualTo(DefaultChipFactorySerialNumber), nameof(device.ChipFactorySerialNumber));

    var info = (IMcp2221AInfo)device;

    Assert.That(info.FirmwareRevision, Is.EqualTo("1.2"), nameof(info.FirmwareRevision));
    Assert.That(info.HardwareRevision, Is.EqualTo("A.6"), nameof(info.HardwareRevision));
    Assert.That(info.Manufacturer, Is.EqualTo(DefaultManufacturer), nameof(info.Manufacturer));
    Assert.That(info.Product, Is.EqualTo(DefaultProduct), nameof(info.Product));
    Assert.That(info.SerialNumber, Is.EqualTo(DefaultSerialNumber), nameof(info.SerialNumber));
    Assert.That(info.ChipFactorySerialNumber, Is.EqualTo(DefaultChipFactorySerialNumber), nameof(info.ChipFactorySerialNumber));
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

  private async Task TestDispose(bool shouldDisposeUsbHidDevice, Func<Mcp2221AController, Task> disposeAction)
  {
    const byte InitialGp0Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    using var baseDevice = CreatePseudoDevice(
      gp0Settings: InitialGp0Settings,
      gp1Settings: InitialGp1Settings,
      gp2Settings: InitialGp2Settings,
      gp3Settings: InitialGp3Settings
    );
    await using var device = await Mcp2221AController.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice);

    Assert.That(() => _ = device.HidDevice, Throws.Nothing);

    Assert.That(() => _ = device.HardwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.FirmwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.Manufacturer, Throws.Nothing);
    Assert.That(() => _ = device.Product, Throws.Nothing);
    Assert.That(() => _ = device.SerialNumber, Throws.Nothing);
    Assert.That(() => _ = device.ChipFactorySerialNumber, Throws.Nothing);
    Assert.That(() => _ = device.GpPins, Throws.Nothing);
    Assert.That(() => _ = device.GpPin0, Throws.Nothing);
    Assert.That(() => _ = device.GpPin1, Throws.Nothing);
    Assert.That(() => _ = device.GpPin2, Throws.Nothing);
    Assert.That(() => _ = device.GpPin3, Throws.Nothing);
    Assert.That(() => _ = device.I2cBus, Throws.Nothing);
    Assert.That(() => _ = device.GpioController, Throws.Nothing);
    Assert.That(() => _ = device.CurrentAdcReferenceSource, Throws.Nothing);
    Assert.That(() => _ = device.CurrentDacReferenceSource, Throws.Nothing);
    Assert.That(() => _ = device.LastWriteAnalogRawValue, Throws.Nothing);

    var i2cBus = device.I2cBus;
    var gp0 = device.GpPin0;
    var gp1 = device.GpPin1;
    var gp2 = device.GpPin2;
    var gp3 = device.GpPin3;
    var gpioController = device.GpioController;

    // To test the GetMode/SetMode and Read/Write method calls on the GpioController,
    // ensure the pin is open.
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    _ =
#endif
    device.GpioController.OpenPin(0);

    await disposeAction(device);

    Assert.That(() => _ = device.HidDevice, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpPins, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpPin0, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpPin1, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpPin2, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpPin3, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.I2cBus, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.GpioController, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.CurrentAdcReferenceSource, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.CurrentDacReferenceSource, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = device.LastWriteAnalogRawValue, Throws.TypeOf<ObjectDisposedException>());

    Assert.That(() => _ = device.HardwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.FirmwareRevision, Throws.Nothing);
    Assert.That(() => _ = device.Manufacturer, Throws.Nothing);
    Assert.That(() => _ = device.Product, Throws.Nothing);
    Assert.That(() => _ = device.SerialNumber, Throws.Nothing);
    Assert.That(() => _ = device.ChipFactorySerialNumber, Throws.Nothing);

    Assert.That(() => device.Reset(), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(async () => await device.ResetAsync(), Throws.TypeOf<ObjectDisposedException>());

    foreach (var gp in new GpController[] { gp0, gp1, gp2, gp3 }) {
      Assert.That(async () => await gp.WriteAsync(default), Throws.TypeOf<ObjectDisposedException>());
      Assert.That(() => gp.WriteAsync(default), Throws.TypeOf<ObjectDisposedException>());
      Assert.That(() => gp.Write(default), Throws.TypeOf<ObjectDisposedException>());
      Assert.That(async () => await gp.ReadAsync(), Throws.TypeOf<ObjectDisposedException>());
      Assert.That(() => gp.ReadAsync(), Throws.TypeOf<ObjectDisposedException>());
      Assert.That(() => gp.Read(), Throws.TypeOf<ObjectDisposedException>());
    }

    Assert.That(async () => await i2cBus.WriteAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cBus.WriteAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cBus.Write(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(async () => await i2cBus.ReadAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cBus.ReadAsync(default, 100, default), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => i2cBus.Read(default, 100, default), Throws.TypeOf<ObjectDisposedException>());

    Assert.That(() => gpioController.OpenPin(1, PinMode.Output), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => gpioController.ClosePin(0), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => gpioController.SetPinMode(0, PinMode.Output), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => gpioController.GetPinMode(0), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => gpioController.Write(0, PinValue.High), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => gpioController.Read(0), Throws.TypeOf<ObjectDisposedException>());

    Assert.That(baseDevice.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice), "USB HID device disposed");

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

    Mcp2221AController? device = null;

    Assert.That(
      async () => device = await Mcp2221AController.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: true),
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

    Mcp2221AController? device = null;

    Assert.That(
      () => device = Mcp2221AController.Create(baseDevice, shouldDisposeUsbHidDevice: true),
      expectExceptionThrown ? Throws.TypeOf<Mcp2221ANotSupportedException>() : Throws.Nothing
    );
    Assert.That(baseDevice.IsDisposed, Is.EqualTo(expectExceptionThrown));

    device?.Dispose();
  }

  [Test]
  public async ValueTask GpPins()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPins, Is.Not.Null);
    Assert.That(mcp2221A.GpPins.Count, Is.EqualTo(4));
  }

  [Test]
  public async ValueTask GpPins_IReadOnlyList_Items()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPins[0], Is.TypeOf<Gp0Controller>());
    Assert.That(mcp2221A.GpPins[1], Is.TypeOf<Gp1Controller>());
    Assert.That(mcp2221A.GpPins[2], Is.TypeOf<Gp2Controller>());
    Assert.That(mcp2221A.GpPins[3], Is.TypeOf<Gp3Controller>());
  }

  [TestCase(int.MinValue)]
  [TestCase(-1)]
  [TestCase(4)]
  [TestCase(int.MaxValue)]
  public async ValueTask GpPins_IReadOnlyList_Items_IndexOutOfRange(int index)
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => _ = mcp2221A.GpPins[index],
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("index")
        .And
        .Property(nameof(ArgumentOutOfRangeException.ActualValue))
        .EqualTo(index)
    );
  }

  [Test]
  public async ValueTask GpPins_IEnumerable()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.GpPins,
      Is.EqualTo(new GpController[] { mcp2221A.GpPin0, mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }).AsCollection
    );

    Assert.That(
      (System.Collections.Generic.IEnumerable<GpController>)mcp2221A.GpPins,
      Is.EqualTo(new GpController[] { mcp2221A.GpPin0, mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }).AsCollection
    );

    Assert.That(
      (System.Collections.IEnumerable)mcp2221A.GpPins,
      Is.EqualTo(new GpController[] { mcp2221A.GpPin0, mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }).AsCollection
    );
  }

  [Test]
  public async ValueTask GpPin0()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin0, Is.TypeOf<Gp0Controller>());
    Assert.That(mcp2221A.GpPin0.Index, Is.Zero);
    Assert.That(mcp2221A.GpPin0.PinName, Is.EqualTo("GP0"));
  }

  [Test]
  public async ValueTask GpPin1()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin1, Is.TypeOf<Gp1Controller>());
    Assert.That(mcp2221A.GpPin1.Index, Is.EqualTo(1));
    Assert.That(mcp2221A.GpPin1.PinName, Is.EqualTo("GP1"));
  }

  [Test]
  public async ValueTask GpPin2()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin2, Is.TypeOf<Gp2Controller>());
    Assert.That(mcp2221A.GpPin2.Index, Is.EqualTo(2));
    Assert.That(mcp2221A.GpPin2.PinName, Is.EqualTo("GP2"));
  }

  [Test]
  public async ValueTask GpPin3()
  {
    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin3, Is.TypeOf<Gp3Controller>());
    Assert.That(mcp2221A.GpPin3.Index, Is.EqualTo(3));
    Assert.That(mcp2221A.GpPin3.PinName, Is.EqualTo("GP3"));
  }

  [TestCase(0b_0_0_0_00_0_00, VoltageReferenceSource.Vdd)]
  [TestCase(0b_0_0_0_01_0_00, VoltageReferenceSource.Vdd)] // VRM 1.024V/VDD
  [TestCase(0b_0_0_0_10_0_00, VoltageReferenceSource.Vdd)] // VRM 2.048V/VDD
  [TestCase(0b_0_0_0_11_0_00, VoltageReferenceSource.Vdd)] // VRM 4.096V/VDD
  [TestCase(0b_0_0_0_00_1_00, VoltageReferenceSource.VrmOff)]
  [TestCase(0b_0_0_0_01_1_00, VoltageReferenceSource.Vrm1024)]
  [TestCase(0b_0_0_0_10_1_00, VoltageReferenceSource.Vrm2048)]
  [TestCase(0b_0_0_0_11_1_00, VoltageReferenceSource.Vrm4096)] // INTDETFEEN: 0, INTDETREEN: 0
  [TestCase(0b_0_0_1_01_1_00, VoltageReferenceSource.Vrm1024)] // INTDETFEEN: 0, INTDETREEN: 1
  [TestCase(0b_0_1_0_01_1_00, VoltageReferenceSource.Vrm1024)] // INTDETFEEN: 1, INTDETREEN: 0
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vrm1024)] // INTDETFEEN: 1, INTDETREEN: 1 (factory default)
  public void CurrentAdcReferenceSource(
    byte chipSettings3,
    VoltageReferenceSource expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      CreatePseudoDevice(
        chipSettings3: chipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.CurrentAdcReferenceSource,
      Is.EqualTo(expected)
    );
  }

  [TestCase(0b_00_0_00000, VoltageReferenceSource.Vdd)]
  [TestCase(0b_00_1_00000, VoltageReferenceSource.VrmOff)]
  [TestCase(0b_01_1_00000, VoltageReferenceSource.Vrm1024)]
  [TestCase(0b_10_1_00000, VoltageReferenceSource.Vrm2048)]
  [TestCase(0b_11_1_11111, VoltageReferenceSource.Vrm4096)] // DAC output: 31
  [TestCase(0b_11_0_10000, VoltageReferenceSource.Vdd)] // 4.096V/VDD DAC output: 16
  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vdd)] // 2.048V/VDD; DAC output: 8 (factory default)
  [TestCase(0b_01_0_00100, VoltageReferenceSource.Vdd)] // 1.024V/VDD; DAC output: 4
  [TestCase(0b_00_0_00010, VoltageReferenceSource.Vdd)] // Off/VDD; DAC output: 2
  public void CurrentDacReferenceSource(
    byte chipSettings2,
    VoltageReferenceSource expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      CreatePseudoDevice(
        chipSettings2: chipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.CurrentDacReferenceSource,
      Is.EqualTo(expected)
    );
  }

  [TestCase(0b_00_0_00001, 1)]
  [TestCase(0b_00_1_00011, 3)]
  [TestCase(0b_01_1_00111, 7)]
  [TestCase(0b_10_1_01111, 15)]
  [TestCase(0b_11_1_11111, 31)]
  [TestCase(0b_11_0_10000, 16)]
  [TestCase(0b_10_0_01000, 8)] // 2.048V/VDD; DAC output: 8 (factory default)
  [TestCase(0b_01_0_00100, 4)]
  [TestCase(0b_00_0_00010, 2)]
  public void LastWriteAnalogRawValue_InitialValue(
    byte chipSettings2,
    int expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      CreatePseudoDevice(
        chipSettings2: chipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.LastWriteAnalogRawValue,
      Is.EqualTo(expected)
    );
  }
}
