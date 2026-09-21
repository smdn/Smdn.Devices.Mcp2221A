// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void ModifyUsbVendorId(int vendorId)
  {
    var stagedFlashMemory = new FlashMemory();
    var initialProductId = ~vendorId;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbVidHigherByte: (byte)((initialProductId & 0xFF00) >> 8),
        usbVidLowerByte: (byte)(initialProductId & 0xFF)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbVendorId(vendorId),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[4],
      Is.EqualTo((byte)(vendorId & 0x00FF))
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[5],
      Is.EqualTo((byte)((vendorId & 0xFF00) >> 8))
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after modification");
  }

  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void ModifyUsbProductId(int productId)
  {
    var stagedFlashMemory = new FlashMemory();
    var initialProductId = ~productId;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPidHigherByte: (byte)((initialProductId & 0xFF00) >> 8),
        usbPidLowerByte: (byte)(initialProductId & 0xFF)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbProductId(productId),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[6],
      Is.EqualTo((byte)(productId & 0x00FF))
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[7],
      Is.EqualTo((byte)((productId & 0xFF00) >> 8))
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after modification");
  }
}
