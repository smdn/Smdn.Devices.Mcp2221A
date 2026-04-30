// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AControllerTests {
#pragma warning restore IDE0040
  [TestCase(0b_0_11110_00, false)] // factory default
  [TestCase(0b_1_00000_00, true)]
  [TestCase(0b_1_00000_11, true)]
  public async ValueTask UsbCdcSerialNumberEnabled(byte chipSetting0, bool expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(chipSetting0: chipSetting0),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(chipSetting0: chipSetting0),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbCdcSerialNumberEnabled, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbCdcSerialNumberEnabled, Is.EqualTo(expected));
  }

  [TestCase(0b_0_11110_00, DeviceConfigurationProtectionLevel.None)] // factory default
  [TestCase(0b_1_00000_00, DeviceConfigurationProtectionLevel.None)]
  [TestCase(0b_0_00000_01, DeviceConfigurationProtectionLevel.PasswordProtected)]
  [TestCase(0b_1_00000_10, DeviceConfigurationProtectionLevel.PermanentlyLocked)]
  [TestCase(0b_0_00000_11, DeviceConfigurationProtectionLevel.Reserved)]
  public async ValueTask FlashWriteProtection(byte chipSetting0, DeviceConfigurationProtectionLevel expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(chipSetting0: chipSetting0),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(chipSetting0: chipSetting0),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.FlashWriteProtection, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.FlashWriteProtection, Is.EqualTo(expected));
  }

  [TestCase(0xD8, 0x04, 0x04D8)] // factory default
  [TestCase(0x00, 0xFF, 0xFF00)]
  [TestCase(0xFF, 0x00, 0x00FF)]
  [TestCase(0xFF, 0xFF, 0xFFFF)]
  public async ValueTask UsbVendorId(byte usbVidLowerByte, byte usbVidHigherByte, int expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(
        usbVidLowerByte: usbVidLowerByte,
        usbVidHigherByte: usbVidHigherByte
      ),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(
        usbVidLowerByte: usbVidLowerByte,
        usbVidHigherByte: usbVidHigherByte
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbVendorId, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbVendorId, Is.EqualTo(expected));
  }

  [TestCase(0xDD, 0x00, 0x00DD)] // factory default
  [TestCase(0x00, 0x12, 0x1200)]
  [TestCase(0x34, 0x00, 0x0034)]
  [TestCase(0x78, 0x56, 0x5678)]
  public async ValueTask UsbProductId(byte usbPidLowerByte, byte usbPidHigherByte, int expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(
        usbPidLowerByte: usbPidLowerByte,
        usbPidHigherByte: usbPidHigherByte
      ),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(
        usbPidLowerByte: usbPidLowerByte,
        usbPidHigherByte: usbPidHigherByte
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbProductId, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbProductId, Is.EqualTo(expected));
  }

  [TestCase(0b_1_0_0_00000, UsbPowerMode.BusPowered)] // factory default
  [TestCase(0b_0_0_0_00000, UsbPowerMode.BusPowered)]
  [TestCase(0b_0_0_1_00000, UsbPowerMode.BusPowered)]
  [TestCase(0b_1_1_0_00000, UsbPowerMode.SelfPowered)]
  [TestCase(0b_0_1_1_00000, UsbPowerMode.SelfPowered)]
  public async ValueTask UsbPowerMode_Getter(byte usbPowerAttributes, UsbPowerMode expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(usbPowerAttributes: usbPowerAttributes),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(usbPowerAttributes: usbPowerAttributes),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbPowerMode, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbPowerMode, Is.EqualTo(expected));
  }

  [TestCase(0b_1_0_0_00000, false)] // factory default
  [TestCase(0b_0_0_0_00000, false)]
  [TestCase(0b_0_0_1_00000, true)]
  [TestCase(0b_1_1_0_00000, false)]
  [TestCase(0b_0_1_1_00000, true)]
  public async ValueTask UsbRemoteWakeUpEnabled(byte usbPowerAttributes, bool expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(usbPowerAttributes: usbPowerAttributes),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(usbPowerAttributes: usbPowerAttributes),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbRemoteWakeUpEnabled, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbRemoteWakeUpEnabled, Is.EqualTo(expected));
  }

  [TestCase(0b_00110010, 100)] // factory default
  [TestCase(0x00, 0)]
  [TestCase(0x7D, 250)]
  [TestCase(0xFA, 500)]
  [TestCase(0xFF, 510)]
  public async ValueTask UsbRequestedCurrentAmount(byte usbRequiredCurrent, int expected)
  {
    await using var deviceCreateAsync = await Mcp2221AController.CreateAsync(
      CreatePseudoDevice(usbRequiredCurrent: usbRequiredCurrent),
      shouldDisposeUsbHidDevice: true
    );

    using var deviceCreateSync = Mcp2221AController.Create(
      CreatePseudoDevice(usbRequiredCurrent: usbRequiredCurrent),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(deviceCreateAsync.UsbRequestedCurrentAmount, Is.EqualTo(expected));
    Assert.That(deviceCreateSync.UsbRequestedCurrentAmount, Is.EqualTo(expected));
  }
}
