// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Text;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

[TestFixture]
public class IFlashMemoryExtensionsTests {
  private sealed class UncheckedLengthFlashMemory : IFlashMemory {
    public Span<byte> ChipSettings => throw new NotImplementedException();
    public Span<byte> Password => throw new NotImplementedException();
    public Span<byte> GpSettings => throw new NotImplementedException();

    private const int MaxLengthOfUsbDescriptorString = 60;
    private const int MaxLengthOfChipFactorySerialNumber = 60;

    private readonly byte[] usbManufacturerDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
    private readonly byte[] usbProductDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
    private readonly byte[] usbSerialNumberDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
    private readonly byte[] chipFactorySerialNumber = new byte[MaxLengthOfChipFactorySerialNumber];

    public Span<byte> UsbManufacturerDescriptorString => usbManufacturerDescriptorString;
    public int UsbManufacturerDescriptorStringLength { get; set; } // Omit out-of-range checks

    public Span<byte> UsbProductDescriptorString => usbProductDescriptorString;
    public int UsbProductDescriptorStringLength { get; set; } // Omit out-of-range checks

    public Span<byte> UsbSerialNumberDescriptorString => usbSerialNumberDescriptorString;
    public int UsbSerialNumberDescriptorStringLength { get; set; } // Omit out-of-range checks

    public Span<byte> ChipFactorySerialNumber => chipFactorySerialNumber;
    public int ChipFactorySerialNumberLength { get; set; } // Omit out-of-range checks
  }

  [TestCase(0)]
  [TestCase(30)]
  [TestCase(60)]
  public void StoredUsbManufacturerDescriptorStringSpan(int length)
  {
    var memory = new FlashMemory();

    for (var i = 0; i < memory.UsbManufacturerDescriptorString.Length; i++)
      memory.UsbManufacturerDescriptorString[i] = (byte)(i + 1);

    memory.UsbManufacturerDescriptorStringLength = length;

    var span = memory.StoredUsbManufacturerDescriptorStringSpan;

    Assert.That(span.Length, Is.EqualTo(length));

    if (0 < length) {
      Assert.That(
        span.SequenceEqual(memory.UsbManufacturerDescriptorString.Slice(0, length)),
        Is.True,
        "The span must be sliced starting from index 0 with the specified length."
      );
    }
  }

  [TestCase(-1)]
  [TestCase(61)]
  public void StoredUsbManufacturerDescriptorStringSpan_LengthOutOfRange(int length)
  {
    var memory = new UncheckedLengthFlashMemory() {
      UsbManufacturerDescriptorStringLength = length
    };

    Assert.That(
      () => _ = memory.StoredUsbManufacturerDescriptorStringSpan,
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );
  }

  [TestCase(0)]
  [TestCase(30)]
  [TestCase(60)]
  public void StoredUsbProductDescriptorStringSpan(int length)
  {
    var memory = new FlashMemory();

    for (var i = 0; i < memory.UsbProductDescriptorString.Length; i++)
      memory.UsbProductDescriptorString[i] = (byte)(i + 1);

    memory.UsbProductDescriptorStringLength = length;

    var span = memory.StoredUsbProductDescriptorStringSpan;

    Assert.That(span.Length, Is.EqualTo(length));

    if (0 < length) {
      Assert.That(
        span.SequenceEqual(memory.UsbProductDescriptorString.Slice(0, length)),
        Is.True,
        "The span must be sliced starting from index 0 with the specified length."
      );
    }
  }

  [TestCase(-1)]
  [TestCase(61)]
  public void StoredUsbProductDescriptorStringSpan_LengthOutOfRange(int length)
  {
    var memory = new UncheckedLengthFlashMemory() {
      UsbProductDescriptorStringLength = length
    };

    Assert.That(
      () => _ = memory.StoredUsbProductDescriptorStringSpan,
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );
  }

  [TestCase(0)]
  [TestCase(30)]
  [TestCase(60)]
  public void StoredUsbSerialNumberDescriptorStringSpan(int length)
  {
    var memory = new FlashMemory();

    for (var i = 0; i < memory.UsbSerialNumberDescriptorString.Length; i++)
      memory.UsbSerialNumberDescriptorString[i] = (byte)(i + 1);

    memory.UsbSerialNumberDescriptorStringLength = length;

    var span = memory.StoredUsbSerialNumberDescriptorStringSpan;

    Assert.That(span.Length, Is.EqualTo(length));

    if (0 < length) {
      Assert.That(
        span.SequenceEqual(memory.UsbSerialNumberDescriptorString.Slice(0, length)),
        Is.True,
        "The span must be sliced starting from index 0 with the specified length."
      );
    }
  }

  [TestCase(-1)]
  [TestCase(61)]
  public void StoredUsbSerialNumberDescriptorStringSpan_LengthOutOfRange(int length)
  {
    var memory = new UncheckedLengthFlashMemory() {
      UsbSerialNumberDescriptorStringLength = length
    };

    Assert.That(
      () => _ = memory.StoredUsbSerialNumberDescriptorStringSpan,
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );
  }

  [TestCase(0)]
  [TestCase(30)]
  [TestCase(60)]
  public void StoredChipFactorySerialNumberSpan(int length)
  {
    var memory = new FlashMemory();

    for (var i = 0; i < memory.ChipFactorySerialNumber.Length; i++)
      memory.ChipFactorySerialNumber[i] = (byte)(i + 1);

    memory.ChipFactorySerialNumberLength = length;

    var span = memory.StoredChipFactorySerialNumberSpan;

    Assert.That(span.Length, Is.EqualTo(length));

    if (0 < length) {
      Assert.That(
        span.SequenceEqual(memory.ChipFactorySerialNumber.Slice(0, length)),
        Is.True,
        "The span must be sliced starting from index 0 with the specified length."
      );
    }
  }

  [TestCase(-1)]
  [TestCase(61)]
  public void StoredChipFactorySerialNumberSpan_LengthOutOfRange(int length)
  {
    var memory = new UncheckedLengthFlashMemory() {
      ChipFactorySerialNumberLength = length
    };

    Assert.That(
      () => _ = memory.StoredChipFactorySerialNumberSpan,
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );
  }

  [Test]
  public void DiffersFrom_ReceiverArgumentNull()
  {
    FlashMemory memory = null!;

    Assert.That(
      () => memory.DiffersFrom(new FlashMemory()),
      Throws
        .ArgumentNullException
        .ParamName
        .EqualTo("flashMemory")
    );
  }

  [Test]
  public void DiffersFrom_Null()
  {
    var memory = new FlashMemory();

    Assert.That(
      memory.DiffersFrom(null),
      Is.True
    );
  }

  [Test]
  public void DiffersFrom_ReferenceEquals()
  {
    var memory = new FlashMemory();

    Assert.That(
      memory.DiffersFrom(memory),
      Is.False
    );
  }

  [TestCase("0")]
  [TestCase("01234567")]
  [TestCase("abc")]
  public void DiffersFrom_Password(string password)
  {
    var memory = new FlashMemory();
    var memoryOther = new FlashMemory();

    Encoding.ASCII.GetBytes(password).CopyTo(memoryOther.Password);

    Assert.That(
      memory.DiffersFrom(memory),
      Is.False
    );
  }

  [Test]
  public void DiffersFrom_ChipSettings_NoDifference()
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase(0, (byte)0xFF)]
  [TestCase(1, (byte)0x01)]
  [TestCase(9, (byte)0x55)]
  public void DiffersFrom_ChipSettings_HasDifference(int index, byte value)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    memory2.ChipSettings[index] = value;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [Test]
  public void DiffersFrom_GpSettings_NoDifference()
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase(0, (byte)0xFF)]
  [TestCase(1, (byte)0x01)]
  [TestCase(3, (byte)0x55)]
  public void DiffersFrom_GpSettings_HasDifference(int index, byte value)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    memory2.GpSettings[index] = value;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [TestCase("")]
  [TestCase("Vendor")]
  [TestCase("012345678901234567890123456789")] // max length
  public void DiffersFrom_UsbManufacturerDescriptorString_NoDifference(string text)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(memory1.UsbManufacturerDescriptorString);
    memory1.UsbManufacturerDescriptorStringLength = (byte)bytes.Length;

    bytes.CopyTo(memory2.UsbManufacturerDescriptorString);
    memory2.UsbManufacturerDescriptorStringLength = (byte)bytes.Length;

    // Verify that even if bytes outside the valid length are modified,
    // they are not detected as a difference (Is.True)
    if (bytes.Length < memory2.UsbManufacturerDescriptorString.Length)
      memory2.UsbManufacturerDescriptorString[bytes.Length] = 0xFF;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase("VendorA", "VendorB", 14, 14)] // content differs
  [TestCase("VendorA", "VendorA", 14, 12)] // length differs
  [TestCase("012345678901234567890123456789", "01234567890123456789012345678X", 60, 60)] // max length, content differs
  public void DiffersFrom_UsbManufacturerDescriptorString_HasDifference(
    string text1,
    string text2,
    int length1,
    int length2
  )
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes1 = Encoding.Unicode.GetBytes(text1);
    var bytes2 = Encoding.Unicode.GetBytes(text2);

    bytes1.CopyTo(memory1.UsbManufacturerDescriptorString);
    memory1.UsbManufacturerDescriptorStringLength = (byte)length1;

    bytes2.CopyTo(memory2.UsbManufacturerDescriptorString);
    memory2.UsbManufacturerDescriptorStringLength = (byte)length2;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [TestCase("")]
  [TestCase("Product")]
  [TestCase("012345678901234567890123456789")] // max length
  public void DiffersFrom_UsbProductDescriptorString_NoDifference(string text)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(memory1.UsbProductDescriptorString);
    memory1.UsbProductDescriptorStringLength = (byte)bytes.Length;

    bytes.CopyTo(memory2.UsbProductDescriptorString);
    memory2.UsbProductDescriptorStringLength = (byte)bytes.Length;

    // Verify that even if bytes outside the valid length are modified,
    // they are not detected as a difference (Is.True)
    if (bytes.Length < memory2.UsbProductDescriptorString.Length)
      memory2.UsbProductDescriptorString[bytes.Length] = 0xFF;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase("ProductA", "ProductB", 16, 16)] // content differs
  [TestCase("ProductA", "ProductA", 16, 14)] // length differs
  [TestCase("012345678901234567890123456789", "01234567890123456789012345678X", 60, 60)] // max length, content differs
  public void DiffersFrom_UsbProductDescriptorString_HasDifference(
    string text1,
    string text2,
    int length1,
    int length2
  )
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes1 = Encoding.Unicode.GetBytes(text1);
    var bytes2 = Encoding.Unicode.GetBytes(text2);

    bytes1.CopyTo(memory1.UsbProductDescriptorString);
    memory1.UsbProductDescriptorStringLength = (byte)length1;

    bytes2.CopyTo(memory2.UsbProductDescriptorString);
    memory2.UsbProductDescriptorStringLength = (byte)length2;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [TestCase("")]
  [TestCase("12345678")]
  [TestCase("012345678901234567890123456789")] // max length
  public void DiffersFrom_UsbSerialNumberDescriptorString_NoDifference(string text)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(memory1.UsbSerialNumberDescriptorString);
    memory1.UsbSerialNumberDescriptorStringLength = (byte)bytes.Length;

    bytes.CopyTo(memory2.UsbSerialNumberDescriptorString);
    memory2.UsbSerialNumberDescriptorStringLength = (byte)bytes.Length;

    // Verify that even if bytes outside the valid length are modified,
    // they are not detected as a difference (Is.True)
    if (bytes.Length < memory2.UsbSerialNumberDescriptorString.Length)
      memory2.UsbSerialNumberDescriptorString[bytes.Length] = 0xFF;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase("12345678", "87654321", 16, 16)] // content differs
  [TestCase("12345678", "12345678", 16, 14)] // length differs
  [TestCase("012345678901234567890123456789", "01234567890123456789012345678X", 60, 60)] // max length, content differs
  public void DiffersFrom_UsbSerialNumberDescriptorString_HasDifference(
    string text1,
    string text2,
    int length1,
    int length2
  )
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes1 = Encoding.Unicode.GetBytes(text1);
    var bytes2 = Encoding.Unicode.GetBytes(text2);

    bytes1.CopyTo(memory1.UsbSerialNumberDescriptorString);
    memory1.UsbSerialNumberDescriptorStringLength = (byte)length1;

    bytes2.CopyTo(memory2.UsbSerialNumberDescriptorString);
    memory2.UsbSerialNumberDescriptorStringLength = (byte)length2;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [TestCase("")]
  [TestCase("00012345")]
  [TestCase("012345678901234567890123456789")] // max length
  public void DiffersFrom_ChipFactorySerialNumber_NoDifference(string text)
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes = Encoding.ASCII.GetBytes(text);

    bytes.CopyTo(memory1.ChipFactorySerialNumber);
    memory1.ChipFactorySerialNumberLength = (byte)bytes.Length;

    bytes.CopyTo(memory2.ChipFactorySerialNumber);
    memory2.ChipFactorySerialNumberLength = (byte)bytes.Length;

    // Verify that even if bytes outside the valid length are modified,
    // they are not detected as a difference (Is.True)
    if (bytes.Length < memory2.ChipFactorySerialNumber.Length)
      memory2.ChipFactorySerialNumber[bytes.Length] = 0xFF;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.False
    );
  }

  [TestCase("00012345", "00054321", 8, 8)] // content differs
  [TestCase("00012345", "00012345", 8, 6)] // length differs
  [TestCase("012345678901234567890123456789", "01234567890123456789012345678X", 60, 60)] // max length, content differs
  public void DiffersFrom_ChipFactorySerialNumber_HasDifference(
    string text1,
    string text2,
    int length1,
    int length2
  )
  {
    var memory1 = new FlashMemory();
    var memory2 = new FlashMemory();

    var bytes1 = Encoding.ASCII.GetBytes(text1);
    var bytes2 = Encoding.ASCII.GetBytes(text2);

    bytes1.CopyTo(memory1.ChipFactorySerialNumber);
    memory1.ChipFactorySerialNumberLength = (byte)length1;

    bytes2.CopyTo(memory2.ChipFactorySerialNumber);
    memory2.ChipFactorySerialNumberLength = (byte)length2;

    Assert.That(
      memory1.DiffersFrom(memory2),
      Is.True
    );
  }

  [Test]
  public void CopyFrom_ReceiverArgumentNull()
  {
    FlashMemory memory = null!;

    Assert.That(
      () => memory.CopyFrom(new FlashMemory()),
      Throws
        .ArgumentNullException
        .ParamName
        .EqualTo("flashMemory")
    );
  }

  [Test]
  public void CopyFrom_ArgumentNull()
  {
    var memory = new FlashMemory();

    Assert.That(
      () => memory.CopyFrom(null!),
      Throws
        .ArgumentNullException
        .ParamName
        .EqualTo("source")
    );
  }

  [Test]
  public void CopyFrom_ReturnsSelf()
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    var result = destination.CopyFrom(source);

    Assert.That(result, Is.SameAs(destination));
  }

  [Test]
  public void CopyFrom_ReferenceEquals()
  {
    var memory = new FlashMemory();

    var result = memory.CopyFrom(memory);

    Assert.That(result, Is.SameAs(memory));
  }

  [Test]
  public void CopyFrom_PasswordIsNotCopied()
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    const string PasswordForDestination = "DST_PASS";
    const string PasswordForSource = "SRC_PASS";

    Encoding.ASCII.GetBytes(PasswordForDestination).CopyTo(destination.Password);
    Encoding.ASCII.GetBytes(PasswordForSource).CopyTo(source.Password);

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );

    Assert.That(
      destination.Password.SequenceEqual(Encoding.ASCII.GetBytes(PasswordForDestination)),
      Is.True,
      $"The Password area must explicitly be excluded from {nameof(IFlashMemoryExtensions.CopyFrom)}."
    );
  }

  [Test]
  public void CopyFrom_ChipSettingsAndGpSettings()
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    for (var i = 0; i < source.ChipSettings.Length; i++)
      source.ChipSettings[i] = (byte)(i + 1); // set test values

    for (var i = 0; i < source.GpSettings.Length; i++)
      source.GpSettings[i] = (byte)(i + 10); // set test values

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );
    Assert.That(
      destination.ChipSettings.SequenceEqual(source.ChipSettings),
      Is.True
    );
    Assert.That(
      destination.GpSettings.SequenceEqual(source.GpSettings),
      Is.True
    );
  }

  [TestCase(0, "")]
  [TestCase(20, "Vendor")]
  [TestCase(60, "012345678901234567890123456789")] // max length (Unicode)
  public void CopyFrom_UsbManufacturerDescriptorString(int length, string text)
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(source.UsbManufacturerDescriptorString);
    source.UsbManufacturerDescriptorStringLength = (byte)length;

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );
    Assert.That(
      destination.UsbManufacturerDescriptorStringLength,
      Is.EqualTo(source.UsbManufacturerDescriptorStringLength)
    );
    Assert.That(
      destination.UsbManufacturerDescriptorString.SequenceEqual(source.UsbManufacturerDescriptorString),
      Is.True
    );
  }

  [TestCase(0, "")]
  [TestCase(20, "Product")]
  [TestCase(60, "012345678901234567890123456789")] // max length (Unicode)
  public void CopyFrom_UsbProductDescriptorString(int length, string text)
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(source.UsbProductDescriptorString);
    source.UsbProductDescriptorStringLength = (byte)length;

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );
    Assert.That(
      destination.UsbProductDescriptorStringLength,
      Is.EqualTo(source.UsbProductDescriptorStringLength)
    );
    Assert.That(
      destination.UsbProductDescriptorString.SequenceEqual(source.UsbProductDescriptorString),
      Is.True
    );
  }

  [TestCase(0, "")]
  [TestCase(20, "1234567890")]
  [TestCase(60, "012345678901234567890123456789")] // max length (Unicode)
  public void CopyFrom_UsbSerialNumberDescriptorString(int length, string text)
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    var bytes = Encoding.Unicode.GetBytes(text);

    bytes.CopyTo(source.UsbSerialNumberDescriptorString);
    source.UsbSerialNumberDescriptorStringLength = (byte)length;

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );
    Assert.That(
      destination.UsbSerialNumberDescriptorStringLength,
      Is.EqualTo(source.UsbSerialNumberDescriptorStringLength)
    );
    Assert.That(
      destination.UsbSerialNumberDescriptorString.SequenceEqual(source.UsbSerialNumberDescriptorString),
      Is.True
    );
  }

  [TestCase(0, "")]
  [TestCase(8, "00012345")]
  [TestCase(60, "012345678901234567890123456789012345678901234567890123456789")] // max length (ASCII)
  public void CopyFrom_ChipFactorySerialNumber(int length, string text)
  {
    var destination = new FlashMemory();
    var source = new FlashMemory();

    var bytes = Encoding.ASCII.GetBytes(text);

    bytes.CopyTo(source.ChipFactorySerialNumber);
    source.ChipFactorySerialNumberLength = (byte)length;

    Assert.That(
      destination.CopyFrom(source),
      Is.SameAs(destination)
    );
    Assert.That(
      destination.ChipFactorySerialNumberLength,
      Is.EqualTo(source.ChipFactorySerialNumberLength)
    );
    Assert.That(
      destination.ChipFactorySerialNumber.SequenceEqual(source.ChipFactorySerialNumber),
      Is.True
    );
  }
}
