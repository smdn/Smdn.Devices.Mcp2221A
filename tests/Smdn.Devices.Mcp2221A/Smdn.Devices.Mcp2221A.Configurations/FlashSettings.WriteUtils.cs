// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbManufacturerString))]
  public void ModifyUsbManufacturerString(
    string initialUsbManufacturerString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialUsbManufacturerString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbManufacturerString(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.UsbManufacturerDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(
        stagedFlashMemory
          .StoredUsbManufacturerDescriptorStringSpan
#if !SYSTEM_TEXT_ENCODING_GETSTRING_READONLYSPAN_OF_BYTE
          .ToArray()
#endif
      ),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbManufacturerString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbManufacturerString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB manufacturer descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbManufacturerDescriptorString.CopyTo(stagedFlashMemory.UsbManufacturerDescriptorString);

    stagedFlashMemory.UsbManufacturerDescriptorStringLength = initialFlashMemory.UsbManufacturerDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void ModifyUsbManufacturerString_InvalidLength(
    string initialUsbManufacturerString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialUsbManufacturerString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyUsbManufacturerString(value: newValue),
      Throws
        .ArgumentException
        .ParamName.EqualTo("value")
    );
    Assert.That(
      mcp2221A.Flash.GetUsbManufacturerString(),
      Is.EqualTo(initialUsbManufacturerString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbProductString))]
  public void ModifyUsbProductString(
    string initialUsbProductString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: initialUsbProductString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbProductString(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.UsbProductDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(
        stagedFlashMemory
          .StoredUsbProductDescriptorStringSpan
#if !SYSTEM_TEXT_ENCODING_GETSTRING_READONLYSPAN_OF_BYTE
          .ToArray()
#endif
      ),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbProductString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbProductString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB product descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbProductDescriptorString.CopyTo(stagedFlashMemory.UsbProductDescriptorString);

    stagedFlashMemory.UsbProductDescriptorStringLength = initialFlashMemory.UsbProductDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void ModifyUsbProductString_InvalidLength(
    string initialUsbProductString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: initialUsbProductString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyUsbProductString(value: newValue),
      Throws
        .ArgumentException
        .ParamName.EqualTo("value")
    );
    Assert.That(
      mcp2221A.Flash.GetUsbProductString(),
      Is.EqualTo(initialUsbProductString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbSerialNumberString))]
  public void ModifyUsbSerialNumberString(
    string initialUsbSerialNumberString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: initialUsbSerialNumberString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbSerialNumberString(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.UsbSerialNumberDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(
        stagedFlashMemory
          .StoredUsbSerialNumberDescriptorStringSpan
#if !SYSTEM_TEXT_ENCODING_GETSTRING_READONLYSPAN_OF_BYTE
          .ToArray()
#endif
      ),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbSerialNumberString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbSerialNumberString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB serial number descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbSerialNumberDescriptorString.CopyTo(stagedFlashMemory.UsbSerialNumberDescriptorString);

    stagedFlashMemory.UsbSerialNumberDescriptorStringLength = initialFlashMemory.UsbSerialNumberDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void ModifyUsbSerialNumberString_InvalidLength(
    string initialUsbSerialNumberString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: initialUsbSerialNumberString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyUsbSerialNumberString(value: newValue),
      Throws
        .ArgumentException
        .ParamName.EqualTo("value")
    );
    Assert.That(
      mcp2221A.Flash.GetUsbSerialNumberString(),
      Is.EqualTo(initialUsbSerialNumberString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }
}
