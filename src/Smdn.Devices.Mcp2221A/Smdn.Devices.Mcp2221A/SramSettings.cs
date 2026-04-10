// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
#if SYSTEM_RUNTIME_COMPILERSERVICES_INLINEARRAYATTRIBUTE
using System.Runtime.CompilerServices;
#endif

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

internal sealed class SramSettings {
  public const int SizeOfSelf = 10;
  public const int SizeOfGpSettings = 4;

  // [MCP2221A] 3.1.13 SET SRAM SETTINGS
  // [-] Set SRAM settings (not included in this field)
  // [-] Don't care (not included in this field)
  // [0] Clock Output Driver Value
  // [1] DAC Voltage Reference
  // [2] Set DAC Output Value
  // [3] ADC Voltage Reference
  // [4] Setup the interrupt detection mechanism and clear the detection flag
  // [5] Alter GPIO configuration
  // [6] GP0 Settings
  // [7] GP1 Settings
  // [8] GP2 Settings
  // [9] GP3 Settings
  private const int OffsetOfAlterGpioConfigurations = 5;
  private const int OffsetOfGpSettings = 6;

#if SYSTEM_RUNTIME_COMPILERSERVICES_INLINEARRAYATTRIBUTE
  [InlineArray(SizeOfSelf)]
  private struct SramSettingsByteArray {
    private byte element;
  }
#else
  private unsafe struct SramSettingsByteArray {
    private static int ValidateIndex(int index)
      => index is < 0 or >= SizeOfSelf
        ? throw new ArgumentOutOfRangeException(paramName: nameof(index), actualValue: index, message: "index out of range")
        : index;

    private fixed byte elements[SizeOfSelf];

    public byte this[int index] {
      get => elements[ValidateIndex(index)];
      set => elements[ValidateIndex(index)] = value;
    }
  }
#endif

  /// <remarks>
  /// <para>
  /// This field represents the current settings stored in SRAM.
  /// If the transmit/apply operation fails, restore `modifiedSettings` to this value.
  /// </para>
  /// <para>
  /// This field stores the settings configured by the 'SET SRAM SETTINGS' command.
  /// It corresponds to the 10 bytes from the 2nd to the 11th byte of the
  /// 'SET SRAM SETTINGS' command. The first two bytes (bytes 0 and 1) at the
  /// beginning of the command are not included. Therefore, the offset for each
  /// setting field is the offset used when sending the 'SET SRAM SETTINGS command',
  /// offset by -2.
  /// </para>
  /// </remarks>
  private SramSettingsByteArray settings;

  /// <remarks>
  /// <para>
  /// This field represents the settings to be updated,
  /// which will be applied by the next command.
  /// </para>
  /// </remarks>
  private SramSettingsByteArray unsentSettings;

  public bool IsDirty {
    get {
      for (var i = 0; i < SizeOfSelf; i++) {
        if (settings[i] != unsentSettings[i])
          return true;
      }

      return false;
    }
  }

#if DEBUG
  public byte[] ToArray()
  {
    var arr = new byte[SizeOfSelf];

    for (var i = 0; i < SizeOfSelf; i++) {
      arr[i] = settings[i];
    }

    return arr;
  }
#endif

  public void Store()
  {
    for (var i = 0; i < SizeOfSelf; i++) {
      settings[i] = unsentSettings[i];
    }

    // For each of the following byte entries, set the bit that
    // commands the alteration of settings to 0:
    //   [0] Clock Output Driver Value
    //   [1] DAC Voltage Reference
    //   [2] Set DAC Output Value
    //   [3] ADC Voltage Reference
    //   [4] Setup the interrupt detection mechanism and clear the detection flag
    for (var i = 0; i <= 4; i++) {
      settings[i] &= 0b_0_1111111;
      unsentSettings[i] &= 0b_0_1111111;
    }

    // For the following byte entry, set the byte that
    // commands the alteration of settings to 0:
    //   [5] Alter GPIO configuration
    settings[OffsetOfAlterGpioConfigurations] = 0;
    unsentSettings[OffsetOfAlterGpioConfigurations] = 0;
  }

  public void Restore()
  {
    for (var i = 0; i < SizeOfSelf; i++) {
      unsentSettings[i] = settings[i];
    }
  }

  public void WriteSetSramSettingsBytes(Span<byte> destination)
  {
    for (var i = 0; i < SizeOfSelf; i++) {
      destination[i] = unsentSettings[i];
    }
  }

  public void StoreGpSettingsBytes(ReadOnlySpan<byte> gpSettingBytes)
  {
    for (var i = 0; i < SizeOfGpSettings; i++) {
      settings[OffsetOfGpSettings + i] = gpSettingBytes[i];
      unsentSettings[OffsetOfGpSettings + i] = gpSettingBytes[i];
    }
  }

  public byte ReadGpSettingsByte(int gp)
    => settings[OffsetOfGpSettings + gp];

  public SramSettings ModifyGpSettings(
    int gp,
    GpDesignation designation,
    PinMode? direction,
    PinValue? outputValue
  )
  {
    if (designation != GpDesignation.GpioOperation) {
      // applies only when GP<n> is set to GPIO
      direction = null;
      outputValue = null;
    }

    // Alter GPIO configuration = Alter the GP designation (1)
    unsentSettings[OffsetOfAlterGpioConfigurations] |= 0b_1_0000000;

#if SYSTEM_RUNTIME_COMPILERSERVICES_INLINEARRAYATTRIBUTE
    ref var gpSettings = ref unsentSettings[OffsetOfGpSettings + gp];
#else
    var gpSettings = unsentSettings[OffsetOfGpSettings + gp];
#endif
    var currentGpSettings = gpSettings;

    gpSettings = 0b_000_0_0_000;

    // Bit 2-0: GP<n> Designation
    gpSettings |= (byte)(designation & GpDesignation.BitMask);

    // Bit 3: GPIO Direction
    gpSettings |= direction switch {
      null => (byte)(currentGpSettings & 0b_000_0_1_000), // maintain the current settings
      PinMode.Input => 0b_000_0_1_000,
      PinMode.Output => 0b_000_0_0_000,
      PinMode unsupportedMode => (byte)GpController.ThrowDirectionNotSupportedException(unsupportedMode),
    };

    // Bit 4: GPIO Output value
    gpSettings |= outputValue switch {
      null => (byte)(currentGpSettings & 0b_000_1_0_000), // maintain the current settings
      PinValue val => (byte)(val.IsHigh ? 0b_000_1_0_000 : 0b_000_0_0_000),
    };

    // Bit 7-5: Don't care
    // gpSettings |= 0b_000_0_0_000;

#if !SYSTEM_RUNTIME_COMPILERSERVICES_INLINEARRAYATTRIBUTE
    unsentSettings[OffsetOfGpSettings + gp] = gpSettings;
#endif

    return this;
  }
}
