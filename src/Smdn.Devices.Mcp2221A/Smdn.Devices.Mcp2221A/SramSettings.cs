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
  private const int OffsetOfDacVoltageReference = 1;
  private const int OffsetOfDacOutputValue = 2;
  private const int OffsetOfAdcVoltageReference = 3;
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

  public byte ReadAdcSettingsByte()
    => settings[OffsetOfAdcVoltageReference];

  public byte ReadGpSettingsByte(int gp)
    => settings[OffsetOfGpSettings + gp];

  public SramSettings ModifyDacSettings(
    VoltageReferenceSource? dacVoltageReferenceSource,
    int? dacOutputValue
  )
  {
    if (!dacVoltageReferenceSource.HasValue && dacOutputValue.HasValue)
      return this;

    // [1] DAC Voltage Reference
    if (dacVoltageReferenceSource.HasValue) {
      // Bit 7: Enable loading of a new DAC reference
      unsentSettings[OffsetOfDacVoltageReference] = 0b_1_0000_00_0;

      // Bit 6-3: Don't care

      // Bit 2-1: DAC V_RM voltage selection
      // Bit 0: DAC reference voltage (1: DAC V_RM, 0: VDD)
      unsentSettings[OffsetOfDacVoltageReference] |= GetVoltageSelectionAndReferenceVoltageBits(dacVoltageReferenceSource.Value);
    }

    // [2] Set DAC Output Value
    if (dacOutputValue.HasValue) {
      // Bit 7: Enable loading of a new DAC value
      unsentSettings[OffsetOfDacOutputValue] = 0b_1_00_00000;

      // Bit 6-5: Don't care

      // Bit 4-0: The new DAC value
      unsentSettings[OffsetOfDacOutputValue] |= (byte)((byte)dacOutputValue.Value & 0b_0_00_11111);
    }

    return this;
  }

  public SramSettings ModifyAdcSettings(
    VoltageReferenceSource? adcVoltageReferenceSource
  )
  {
    if (!adcVoltageReferenceSource.HasValue)
      return this;

    // [3] ADC Voltage Reference
    // Bit 7: Enable loading of a new ADC reference
    unsentSettings[OffsetOfAdcVoltageReference] = 0b_1_0000_00_0;

    // Bit 6-3: Don't care

    // Bit 2-1: ADC V_RM voltage selection
    // Bit 0: ADC reference voltage (1: ADC V_RM, 0: VDD)
    unsentSettings[OffsetOfAdcVoltageReference] |= GetVoltageSelectionAndReferenceVoltageBits(adcVoltageReferenceSource.Value);

    return this;
  }

  private static byte GetVoltageSelectionAndReferenceVoltageBits(
    VoltageReferenceSource voltageReferenceSource
  )
    // Bit 2-1: DAC/ADC V_RM voltage selection
    // Bit 0: DAC/ADC reference voltage (1: V_RM, 0: VDD)
    => voltageReferenceSource switch {
      VoltageReferenceSource.Vdd => 0b_0_0000_00_0,

      var vrm when vrm is
        VoltageReferenceSource.VrmOff or
        VoltageReferenceSource.Vrm1024 or
        VoltageReferenceSource.Vrm2048 or
        VoltageReferenceSource.Vrm4096 => (byte)((byte)vrm & 0b_0_0000_11_1),

      var invalid => throw new NotSupportedException(
        message: $"The voltage reference source value cannot set to {invalid}. The value must be one of the values defined in {nameof(VoltageReferenceSource)}."
      ),
    };

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

  /// <summary>
  /// Determines whether the DAC/ADC Voltage Reference (VRM) needs to be re-enabled
  /// or maintained after sending a SET SRAM SETTINGS command.
  /// </summary>
  /// <returns>
  /// <see langword="true"/> if the additional command must be sent to re-enable or
  /// maintained the VRM configuration; otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// According to the MCP2221A Datasheet (DS20005565E, Section 1.8.1.1, page 20):
  /// "When the Set SRAM settings command is used for GPIO control, the reference
  /// voltage for VRM is always reinitialized to the default value (VDD) if it is
  /// not explicitly set".
  /// </para>
  /// <para>
  /// This method implements a workaround for this hardware behavior. If the outgoing
  /// command intends to alter GPIO configurations (Byte Index 7 != 0), the VRM
  /// selection for ADC and DAC would be reset to VDD by the chip's internal logic.
  /// </para>
  /// <para>
  /// To counteract this, this method checks if either the current state or the
  /// pending settings are configured to use VRM. If VRM is in use, it ensures the
  /// "Bit 7: Enable loading of a DAC/ADC reference" flags are set in the outgoing
  /// command to re-assert the VRM selection, ensuring the reference does not
  /// unexpectedly revert to VDD.
  /// </para>
  /// </remarks>
  public bool ShouldReenableVrm()
  {
    if (unsentSettings[OffsetOfAlterGpioConfigurations] == 0)
      // GPIO configurations will remain unaltered; no need to re-enable VRM
      return false;

    var isConfiguredToReferVrm = false;

    // [1] DAC Voltage Reference
    // Bit 7: Enable loading of a new DAC reference
    // Bit 0: DAC reference voltage (1: DAC V_RM, 0: VDD)
    var shouldDacVrmBeEnabled =
      (
        (unsentSettings[OffsetOfDacVoltageReference] & 0b_1_0000_00_0) == 0 && // DAC reference will remain unaltered, and
        (settings[OffsetOfDacVoltageReference] & 0b_0_0000_00_1) != 0 // DAC VRM is currently enabled
      ) || // or
      ((unsentSettings[OffsetOfDacVoltageReference] & 0b_1_0000_00_1) == 0b_1_0000_00_1); // Altering DAC to VRM

    if (shouldDacVrmBeEnabled) {
      unsentSettings[OffsetOfDacVoltageReference] |= 0b_1_0000_00_0;
      isConfiguredToReferVrm = true;
    }

    // [3] ADC Voltage Reference
    // Bit 7: Enable loading of a new ADC reference
    // Bit 0: ADC reference voltage (1: ADC V_RM, 0: VDD)
    var shouldAdcVrmBeEnabled =
      (
        (unsentSettings[OffsetOfAdcVoltageReference] & 0b_1_0000_00_0) == 0 && // ADC reference will remain unaltered, and
        (settings[OffsetOfAdcVoltageReference] & 0b_0_0000_00_1) != 0 // ADC VRM is currently enabled
      ) || // or
      ((unsentSettings[OffsetOfAdcVoltageReference] & 0b_1_0000_00_1) == 0b_1_0000_00_1); // Altering ADC to VRM

    if (shouldAdcVrmBeEnabled) {
      unsentSettings[OffsetOfAdcVoltageReference] |= 0b_1_0000_00_0;
      isConfiguredToReferVrm = true;
    }

    if (isConfiguredToReferVrm) {
      // [5] Alter GPIO configuration
      // 0: Do not alter the current GP designation
      unsentSettings[OffsetOfAlterGpioConfigurations] = 0;
    }

    return isConfiguredToReferVrm;
  }
}
