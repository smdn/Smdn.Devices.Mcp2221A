// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  [TestCase("Microchip Technology Inc.")] // factory default
  [TestCase("")]
  [TestCase("Vendor")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void GetUsbManufacturerString(string manufacturer)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: manufacturer
      )
    );

    Assert.That(
      mcp2221A.Flash.GetUsbManufacturerString(),
      Is.EqualTo(manufacturer)
    );
  }

  [TestCase("MCP2221 USB-I2C/UART Combo")] // factory default
  [TestCase("")]
  [TestCase("Product")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void GetUsbProductString(string product)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: product
      )
    );

    Assert.That(
      mcp2221A.Flash.GetUsbProductString(),
      Is.EqualTo(product)
    );
  }

  [TestCase("")] // factory default
  [TestCase("Serial Number")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void GetUsbSerialNumberString(string serialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: serialNumber
      )
    );

    Assert.That(
      mcp2221A.Flash.GetUsbSerialNumberString(),
      Is.EqualTo(serialNumber)
    );
  }

  [TestCase("01234567")] // factory default
  [TestCase("")]
  [TestCase("ABC")]
  [TestCase("012345678901234567890123456789012345678901234567890123456789")] // max length
  public void GetChipFactorySerialNumber(string chipFactorySerialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipFactorySerialNumber: chipFactorySerialNumber
      )
    );

    Assert.That(
      mcp2221A.Flash.GetChipFactorySerialNumber(),
      Is.EqualTo(chipFactorySerialNumber)
    );
  }
}
