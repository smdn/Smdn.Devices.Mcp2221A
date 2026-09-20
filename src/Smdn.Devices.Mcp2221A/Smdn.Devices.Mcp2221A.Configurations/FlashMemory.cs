// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

internal sealed class FlashMemory : IFlashMemory {
  public const int SizeOfChipSettings = 10;
  public const int SizeOfChipSettingsWithPassword = SizeOfChipSettings + LengthOfPassword;
  public const int SizeOfGpSettings = 4;
  public const int MaxLengthOfUsbDescriptorString = 60;
  public const int MaxLengthOfChipFactorySerialNumber = 60;
  internal const int LengthOfPassword = 8; // PASS0-PASS7

  private readonly byte[] chipSettings = new byte[SizeOfChipSettingsWithPassword]; // includes password area
  private readonly byte[] gpSettings = new byte[SizeOfGpSettings];

  private readonly byte[] usbManufacturerDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
  private readonly byte[] usbProductDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
  private readonly byte[] usbSerialNumberDescriptorString = new byte[MaxLengthOfUsbDescriptorString];
  private readonly byte[] chipFactorySerialNumber = new byte[MaxLengthOfChipFactorySerialNumber];

  public Span<byte> ChipSettings => chipSettings.AsSpan(0, SizeOfChipSettings);
  public Span<byte> Password => chipSettings.AsSpan(SizeOfChipSettings, LengthOfPassword);
  public Span<byte> GpSettings => gpSettings;

  public Span<byte> UsbManufacturerDescriptorString => usbManufacturerDescriptorString;

  public int UsbManufacturerDescriptorStringLength {
    get;
    set {
      if (value < 0 || MaxLengthOfUsbDescriptorString < value) {
        throw new ArgumentOutOfRangeException(
          paramName: nameof(UsbManufacturerDescriptorStringLength),
          actualValue: value,
          message: $"The length must be between 0 and {MaxLengthOfUsbDescriptorString} bytes."
        );
      }

      field = value;
    }
  }

  public Span<byte> UsbProductDescriptorString => usbProductDescriptorString;

  public int UsbProductDescriptorStringLength {
    get;
    set {
      if (value < 0 || MaxLengthOfUsbDescriptorString < value) {
        throw new ArgumentOutOfRangeException(
          paramName: nameof(UsbProductDescriptorStringLength),
          actualValue: value,
          message: $"The length must be between 0 and {MaxLengthOfUsbDescriptorString} bytes."
        );
      }

      field = value;
    }
  }

  public Span<byte> UsbSerialNumberDescriptorString => usbSerialNumberDescriptorString;

  public int UsbSerialNumberDescriptorStringLength {
    get;
    set {
      if (value < 0 || MaxLengthOfUsbDescriptorString < value) {
        throw new ArgumentOutOfRangeException(
          paramName: nameof(UsbSerialNumberDescriptorStringLength),
          actualValue: value,
          message: $"The length must be between 0 and {MaxLengthOfUsbDescriptorString} bytes."
        );
      }

      field = value;
    }
  }

  public Span<byte> ChipFactorySerialNumber => chipFactorySerialNumber;

  public int ChipFactorySerialNumberLength {
    get;
    set {
      if (value < 0 || MaxLengthOfChipFactorySerialNumber < value) {
        throw new ArgumentOutOfRangeException(
          paramName: nameof(ChipFactorySerialNumberLength),
          actualValue: value,
          message: $"The length must be between 0 and {MaxLengthOfChipFactorySerialNumber} bytes."
        );
      }

      field = value;
    }
  }
}
