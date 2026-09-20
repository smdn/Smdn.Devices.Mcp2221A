// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
#if false
  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbVendorId_Initial(int vendorId)
  {
    // TODO: Verify the unmodified Flash settings immediately after connecting the device
  }
#endif

  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbVendorId_Staged(int vendorId)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyUsbVendorId(vendorId);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True);
    Assert.That(mcp2221A.Flash.UsbVendorId, Is.EqualTo(vendorId));
  }

#if false
  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbProductId_Initial(int productId)
  {
    // TODO: Verify the unmodified Flash settings immediately after connecting the device
  }
#endif

  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbProductId_Staged(int productId)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyUsbProductId(productId);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True);
    Assert.That(mcp2221A.Flash.UsbProductId, Is.EqualTo(productId));
  }
}
