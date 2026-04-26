// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

internal sealed class SramSettings {
  public const int SizeOfSelf = 10;
  public const int SizeOfGpSettings = 4;

  // [MCP2221A] 3.1.13 SET SRAM SETTINGS
  // [-] Set SRAM settings (not included in this field)
  // [-] Don't care (not included in this field)
  // [0] Clock Output Divider Value
  // [1] DAC Voltage Reference
  // [2] Set DAC Output Value
  // [3] ADC Voltage Reference
  // [4] Setup the interrupt detection mechanism and clear the detection flag
  // [5] Alter GPIO configuration
  // [6] GP0 Settings
  // [7] GP1 Settings
  // [8] GP2 Settings
  // [9] GP3 Settings
  private const int OffsetOfClockOutputDividerValue = 0;
  private const int OffsetOfDacVoltageReference = 1;
  private const int OffsetOfDacOutputValue = 2;
  private const int OffsetOfAdcVoltageReference = 3;
  private const int OffsetOfInterruptDetectionModuleSetup = 4;
  private const int OffsetOfAlterGpioConfiguration = 5;
  private const int OffsetOfGpSettings = 6;

  private const byte MaintainGpioConfiguration = 0;

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
  private readonly byte[] currentSettings = new byte[SizeOfSelf];

  /// <remarks>
  /// <para>
  /// This field represents the settings to be updated,
  /// which will be applied by the next command.
  /// </para>
  /// </remarks>
  private readonly byte[] unsentSettings = new byte[SizeOfSelf];

  public bool IsDirty
    => !currentSettings.SequenceEqual(unsentSettings);

  public void Store()
  {
    unsentSettings.CopyTo(currentSettings);

    // For each of the following byte entries, set the bit that
    // commands the alteration of settings to 0:
    //   [0] Clock Output Divider Value
    //   [1] DAC Voltage Reference
    //   [2] Set DAC Output Value
    //   [3] ADC Voltage Reference
    //   [4] Setup the interrupt detection mechanism and clear the detection flag
    for (var i = OffsetOfClockOutputDividerValue; i <= OffsetOfInterruptDetectionModuleSetup; i++) {
      currentSettings[i] &= 0b_0_1111111;
      unsentSettings[i] &= 0b_0_1111111;
    }

    // For the following byte entry, set the byte that
    // commands the alteration of settings to 0:
    //   [5] Alter GPIO configuration
    currentSettings[OffsetOfAlterGpioConfiguration] = MaintainGpioConfiguration;
    unsentSettings[OffsetOfAlterGpioConfiguration] = MaintainGpioConfiguration;
  }

  public void Restore()
    => currentSettings.CopyTo(unsentSettings);

  public void WriteAsSetSramSettingsCommand(Span<byte> destination)
    => unsentSettings.CopyTo(destination);

  public void StoreClockOutputDividerValueByte(byte clockOutputDividerValue)
  {
    currentSettings[OffsetOfClockOutputDividerValue] = clockOutputDividerValue;
    unsentSettings[OffsetOfClockOutputDividerValue] = clockOutputDividerValue;
  }

  public void StoreDacVoltageReferenceByte(byte dacVoltageReferenceByte)
  {
    currentSettings[OffsetOfDacVoltageReference] = dacVoltageReferenceByte;
    unsentSettings[OffsetOfDacVoltageReference] = dacVoltageReferenceByte;
  }

  public void StoreDacOutputValueByte(byte dacOutputValueByte)
  {
    currentSettings[OffsetOfDacOutputValue] = dacOutputValueByte;
    unsentSettings[OffsetOfDacOutputValue] = dacOutputValueByte;
  }

  public void StoreAdcVoltageReferenceByte(byte adcVoltageReferenceByte)
  {
    currentSettings[OffsetOfAdcVoltageReference] = adcVoltageReferenceByte;
    unsentSettings[OffsetOfAdcVoltageReference] = adcVoltageReferenceByte;
  }

  public void StoreInterruptDetectionModuleSetupByte(byte interruptDetectionModuleSetup)
  {
    currentSettings[OffsetOfInterruptDetectionModuleSetup] = interruptDetectionModuleSetup;
    unsentSettings[OffsetOfInterruptDetectionModuleSetup] = interruptDetectionModuleSetup;
  }

  public void StoreGpSettingsBytes(ReadOnlySpan<byte> gpSettingBytes)
  {
    gpSettingBytes.CopyTo(currentSettings.AsSpan(OffsetOfGpSettings, SizeOfGpSettings));
    gpSettingBytes.CopyTo(unsentSettings.AsSpan(OffsetOfGpSettings, SizeOfGpSettings));
  }

  public byte ReadClockOutputDividerValueByte()
    => currentSettings[OffsetOfClockOutputDividerValue];

  public byte ReadDacVoltageReferenceByte()
    => currentSettings[OffsetOfDacVoltageReference];

  public byte ReadDacOutputValueByte()
    => currentSettings[OffsetOfDacOutputValue];

  public byte ReadAdcVoltageReferenceByte()
    => currentSettings[OffsetOfAdcVoltageReference];

  public byte ReadInterruptDetectionModuleSetupByte()
    => currentSettings[OffsetOfInterruptDetectionModuleSetup];

  public byte ReadGpSettingsByte(int gp)
    => currentSettings[OffsetOfGpSettings + gp];

  public SramSettings ModifyClockOutputSettings(
    ClockOutputFrequency? frequency,
    ClockOutputDutyCycle? dutyCycle
  )
  {
    if (!frequency.HasValue && !dutyCycle.HasValue)
      return this;

    ref var settings = ref unsentSettings[OffsetOfClockOutputDividerValue];

    // [0] Clock Output Divider Value
    // Bit 7: Enable loading of a new clock divider
    settings |= 0b_1_00_00_000;

    // Bit 6-5: Don't care

    // Bit 4-3: Duty cycle
    if (dutyCycle.HasValue)
      settings = (byte)((settings & 0b_1_11_00_111) | GetDutyCycleBits(dutyCycle.Value));

    // Bit 2-0: Clock divider value
    if (frequency.HasValue)
      settings = (byte)((settings & 0b_1_11_11_000) | GetClockDividerValueBits(frequency.Value));

    return this;

    static byte GetDutyCycleBits(ClockOutputDutyCycle duty)
      => duty switch {
        ClockOutputDutyCycle.Duty0 or
        ClockOutputDutyCycle.Duty25 or
        ClockOutputDutyCycle.Duty50 or
        ClockOutputDutyCycle.Duty75 => (byte)((int)duty << 3),

        var invalid => throw new ArgumentException(
          message: $"The clock duty cycle cannot set to {invalid}. The value must be one of the values defined in {nameof(ClockOutputDutyCycle)}."
        ),
      };

    static byte GetClockDividerValueBits(ClockOutputFrequency freq)
      => freq switch {
        ClockOutputFrequency.Frequency24MHz or
        ClockOutputFrequency.Frequency12MHz or
        ClockOutputFrequency.Frequency6MHz or
        ClockOutputFrequency.Frequency3MHz or
        ClockOutputFrequency.Frequency1500kHz or
        ClockOutputFrequency.Frequency750kHz or
        ClockOutputFrequency.Frequency375kHz => (byte)freq,

        ClockOutputFrequency.Reserved => throw new ArgumentException(
          message: $"The clock output frequency cannot set to {nameof(ClockOutputFrequency.Reserved)}. This value is reserved by the device."
        ),

        var invalid => throw new ArgumentException(
          message: $"The clock output frequency cannot set to {invalid}. The value must be one of the values defined in {nameof(ClockOutputFrequency)}."
        ),
      };
  }

  public SramSettings ModifyDacSettings(
    VoltageReferenceSource? dacVoltageReferenceSource,
    int? dacOutputValue
  )
  {
    if (!dacVoltageReferenceSource.HasValue && !dacOutputValue.HasValue)
      return this;

    // [1] DAC Voltage Reference
    if (dacVoltageReferenceSource.HasValue) {
      // Bit 7: Enable loading of a new DAC reference
      unsentSettings[OffsetOfDacVoltageReference] = 0b_1_0000_00_0;

      // Bit 6-3: Don't care

      // Bit 2-1: DAC V_RM voltage selection
      // Bit 0: DAC reference voltage (1: DAC V_RM, 0: VDD)
      ModifyVoltageSelectionAndReferenceVoltageBits(
        ref unsentSettings[OffsetOfDacVoltageReference],
        dacVoltageReferenceSource.Value
      );
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
    ModifyVoltageSelectionAndReferenceVoltageBits(
      ref unsentSettings[OffsetOfAdcVoltageReference],
      adcVoltageReferenceSource.Value
    );

    return this;
  }

  private static void ModifyVoltageSelectionAndReferenceVoltageBits(
    ref byte voltageReferenceBits,
    VoltageReferenceSource voltageReferenceSource
  )
  {
    // Bit 2-1: DAC/ADC V_RM voltage selection
    // Bit 0: DAC/ADC reference voltage (1: V_RM, 0: VDD)
    switch (voltageReferenceSource) {
      case VoltageReferenceSource.Vdd:
        // If the reference voltage is changed to VDD, set only the least
        // significant bit to 0. This ensures that the VRM voltage selection
        // bits are maintained internally.
        voltageReferenceBits &= 0b_1_1111_11_0;
        return;

      case VoltageReferenceSource.VrmOff:
      case VoltageReferenceSource.Vrm1024:
      case VoltageReferenceSource.Vrm2048:
      case VoltageReferenceSource.Vrm4096:
        voltageReferenceBits = (byte)((voltageReferenceBits & 0b_1_1111_00_0) | ((byte)voltageReferenceSource & 0b_0_0000_11_1));
        return;

      default:
        throw new NotSupportedException(
          message: $"The voltage reference source value cannot set to {voltageReferenceSource}. The value must be one of the values defined in {nameof(VoltageReferenceSource)}."
        );
    }
  }

  public SramSettings ModifyInterruptDetectionModuleSetup(
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag
  )
  {
    if (!detectionTrigger.HasValue && !clearDetectionFlag)
      return this;

    ref var settings = ref unsentSettings[OffsetOfInterruptDetectionModuleSetup];

    // [4] Setup the interrupt detection mechanism and clear the detection flag
    // Bit 7: Enable the modification of the interrupt detection conditions
    settings |= 0b_1_00_0_0_0_0_0;

    // Bit 6-5: Don't care

    if (detectionTrigger is { } trigger) {
      if (trigger is < InterruptOnChangeTrigger.None or > InterruptOnChangeTrigger.Both) {
        throw new ArgumentException(
          message: $"The interrupt detection trigger cannot set to {trigger}. The value must be one of the values defined in {nameof(InterruptOnChangeTrigger)}."
        );
      }

      settings &= 0b_1_11_0_0_0_0_1;

      // Bit 4: Enable the modification of the positive edge detection
      // Bit 3: The new value for the positive edge detector
      if (trigger.HasFlag(InterruptOnChangeTrigger.Rising))
        settings |= 0b_0_00_1_1_0_0_0;
      else
        settings |= 0b_0_00_1_0_0_0_0;

      // Bit 2: Enable the modification of the negative edge detection
      // Bit 1: The new value for the negative edge detector
      if (trigger.HasFlag(InterruptOnChangeTrigger.Falling))
        settings |= 0b_0_00_0_0_1_1_0;
      else
        settings |= 0b_0_00_0_0_1_0_0;
    }

    // Bit 0: Clear the interrupt detection flag
    if (clearDetectionFlag)
      settings |= 0b_0_00_0_0_0_0_1;
    else
      settings &= 0b_1_11_1_1_1_1_0;

    return this;
  }

  public SramSettings ModifyGpSettings(
    int gp,
    GpDesignation designation,
    PinMode? direction = null,
    PinValue? outputValue = null
  )
  {
    if (designation != GpDesignation.GpioOperation) {
      // applies only when GP<n> is set to GPIO
      direction = null;
      outputValue = null;
    }

    // Alter GPIO configuration = Alter the GP designation (1)
    unsentSettings[OffsetOfAlterGpioConfiguration] |= 0b_1_0000000;

    ref var gpSettings = ref unsentSettings[OffsetOfGpSettings + gp];
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
    if (unsentSettings[OffsetOfAlterGpioConfiguration] == MaintainGpioConfiguration)
      // GPIO configurations will remain unaltered; no need to re-enable VRM
      return false;

    // [1] DAC Voltage Reference
    var shouldDacVrmBeEnabled = ShouldVrmBeEnabled(
      currentSettings[OffsetOfDacVoltageReference],
      ref unsentSettings[OffsetOfDacVoltageReference]
    );

    // [3] ADC Voltage Reference
    var shouldAdcVrmBeEnabled = ShouldVrmBeEnabled(
      currentSettings[OffsetOfAdcVoltageReference],
      ref unsentSettings[OffsetOfAdcVoltageReference]
    );

    if (shouldDacVrmBeEnabled || shouldAdcVrmBeEnabled) {
      // [5] Alter GPIO configuration
      // 0: Do not alter the current GP designation
      unsentSettings[OffsetOfAlterGpioConfiguration] = MaintainGpioConfiguration;
      return true;
    }

    return false;

    static bool ShouldVrmBeEnabled(byte currentVoltageReference, ref byte unsentVoltageReference)
    {
      // [1] DAC Voltage Reference
      //   Bit 7: Enable loading of a new DAC reference
      //   Bit 0: DAC reference voltage (1: DAC V_RM, 0: VDD)
      // [3] ADC Voltage Reference
      //   Bit 7: Enable loading of a new ADC reference
      //   Bit 0: ADC reference voltage (1: ADC V_RM, 0: VDD)
      var shouldVrmBeEnabled =
        (
          (unsentVoltageReference & 0b_1_0000000) == 0 && // DAC/ADC reference will remain unaltered, and
          (currentVoltageReference & 0b_0000000_1) != 0 // DAC/ADC VRM is currently enabled
        ) || // or
        ((unsentVoltageReference & 0b_1_000000_1) == 0b_1_000000_1); // Altering DAC/ADC to VRM

      if (shouldVrmBeEnabled)
        unsentVoltageReference |= 0b_1_0000000;

      return shouldVrmBeEnabled;
    }
  }

  public bool ShouldResetInterruptDetectionFlag()
  {
    // [4] Setup the interrupt detection mechanism and clear the detection flag
    // Bit 7: Enable the modification of the interrupt detection conditions
    // Bit 0: Clear the interrupt detection flag
    const byte ClearInterruptDetectionFlag = 0b_1_00_0_0_0_0_1;

    return (unsentSettings[OffsetOfInterruptDetectionModuleSetup] & ClearInterruptDetectionFlag) == ClearInterruptDetectionFlag;
  }
}
