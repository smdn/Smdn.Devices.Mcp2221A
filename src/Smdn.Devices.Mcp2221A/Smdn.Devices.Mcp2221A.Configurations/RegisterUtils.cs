// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A.Configurations;

internal static class RegisterUtils {
  private const byte CdcSnenBitMask = 0b_1_00000_00; // CHIPSETTING0 bit 7 CDCSNEN
  private const byte ChipProtBitMask = 0b_0_00000_11; // CHIPSETTING0 bit 1-0 CHIPPROT

  private const byte ClkDcBitMask = 0b_000_11_000; // CHIPSETTING1 bit 4-3 CLKDC
  private const byte ClkDcShiftAmount = 3; // CHIPSETTING1 bit 4-3 CLKDC
  private const byte ClkDivBitMask = 0b_000_00_111; // CHIPSETTING1 bit 2-0 CLKDIV

  private const byte DacVrmBitMask = 0b_11_0_00000; // CHIPSETTING2 bit 7-6 DACVRM
  private const byte DacRefBitMask = 0b_00_1_00000; // CHIPSETTING2 bit 5 DACREF
  private const byte DacRefShiftAmount = 5; // CHIPSETTING2 bit 5 DACREF
  private const byte DacValBitMask = 0b_00_0_11111; // CHIPSETTING2 bit 4-0 DACVAL

  private const byte IntDetFEEnBitMask = 0b_0_1_0_00_0_00; // CHIPSETTING3 bit 6 INTDETFEEN
  private const byte IntDetREEnBitMask = 0b_0_0_1_00_0_00; // CHIPSETTING3 bit 5 INTDETREEN
  private const byte IntDetBitMask = IntDetFEEnBitMask | IntDetREEnBitMask;
  private const byte IntDetShiftAmount = 5; // CHIPSETTING2 bit 5 DACREF

  private const byte AdcVrmBitMask = 0b_0_0_0_11_0_00; // CHIPSETTING3 bit 4-3 ADCVRM
  private const byte AdcRefBitMask = 0b_0_0_0_00_1_00; // CHIPSETTING3 bit 2 ADCREF
  private const byte AdcRefShiftAmount = 2; // CHIPSETTING3 bit 2 ADCREF

  private const byte SelfPwrBitMask = 0b_0_1_0_00000; // USBPWRATTR bit 6 SELFPWR
  private const byte RemWkUpBitMask = 0b_0_0_1_00000; // USBPWRATTR bit 5 REMWKUP

  private const byte Zero = 0;

  public static bool ReadUsbCdcSerialNumberEnabled(byte chipSetting0RegisterValue)
    => (chipSetting0RegisterValue & CdcSnenBitMask) != 0;

  public static void WriteUsbCdcSerialNumberEnabled(
    ref byte chipSetting0Register,
    bool enabled
  )
  {
    chipSetting0Register &= unchecked((byte)~CdcSnenBitMask);
    chipSetting0Register |= enabled ? CdcSnenBitMask : Zero;
  }

  public static DeviceConfigurationProtectionLevel ReadFlashWriteProtection(byte chipSetting0RegisterValue)
    => (DeviceConfigurationProtectionLevel)(
      chipSetting0RegisterValue & ChipProtBitMask
    );

  public static void WriteFlashWriteProtection(
    ref byte chipSetting0Register,
    DeviceConfigurationProtectionLevel protectionLevel
  )
  {
    chipSetting0Register &= unchecked((byte)~ChipProtBitMask);
    chipSetting0Register |= (byte)((byte)protectionLevel & ChipProtBitMask);
  }

  public static ClockOutputDutyCycle ReadClockOutputDutyCycle(byte chipSetting1RegisterValue)
    => (ClockOutputDutyCycle)(
      (chipSetting1RegisterValue & ClkDcBitMask) >> ClkDcShiftAmount
    );

  public static void WriteClockOutputDutyCycle(
    ref byte chipSetting1Register,
    ClockOutputDutyCycle dutyCycle
  )
  {
    chipSetting1Register &= unchecked((byte)~ClkDcBitMask);
    chipSetting1Register |= (byte)(((byte)dutyCycle & (ClkDcBitMask >> ClkDcShiftAmount)) << ClkDcShiftAmount);
  }

  public static ClockOutputFrequency ReadClockOutputFrequency(byte chipSetting1RegisterValue)
    => (ClockOutputFrequency)(
      chipSetting1RegisterValue & ClkDivBitMask
    );

  public static void WriteClockOutputFrequency(
    ref byte chipSetting1Register,
    ClockOutputFrequency frequency
  )
  {
    chipSetting1Register &= unchecked((byte)~ClkDivBitMask);
    chipSetting1Register |= (byte)((byte)frequency & ClkDivBitMask);
  }

  public static VoltageReferenceSource ReadDacVoltageReference(byte chipSetting2RegisterValue)
  {
    var vrmEnabled = 0 != (chipSetting2RegisterValue & DacRefBitMask);

    if (vrmEnabled) {
      return (VoltageReferenceSource)(
        ((chipSetting2RegisterValue & DacVrmBitMask) | DacRefBitMask) >> DacRefShiftAmount
      );
    }
    else {
      // If Vdd is selected as the reference voltage by DACREF,
      // specifying a reference voltage via DACVRM is meaningless;
      // therefore, the setting value is normalized and returned as
      // VoltageReferenceSource.Vdd.
      return VoltageReferenceSource.Vdd;
    }
  }

  public static void WriteDacVoltageReference(
    ref byte chipSetting2Register,
    VoltageReferenceSource voltageReference
  )
  {
    const byte DacBitMask = DacVrmBitMask | DacRefBitMask;

    chipSetting2Register &= unchecked((byte)~DacBitMask);
    chipSetting2Register |= (byte)(((byte)voltageReference & (DacBitMask >> DacRefShiftAmount)) << DacRefShiftAmount);
  }

  public static int ReadDacInitialValue(byte chipSetting2RegisterValue)
    => chipSetting2RegisterValue & DacValBitMask;

  public static void WriteDacInitialValue(
    ref byte chipSetting2Register,
    int value
  )
  {
    chipSetting2Register &= unchecked((byte)~DacValBitMask);
    chipSetting2Register |= (byte)(value & DacValBitMask);
  }

  public static InterruptOnChangeTrigger ReadInterruptOnChangeTrigger(byte chipSetting3RegisterValue)
    => (InterruptOnChangeTrigger)(
      (chipSetting3RegisterValue & IntDetBitMask) >> IntDetShiftAmount
    );

  public static void WriteInterruptOnChangeTrigger(
    ref byte chipSetting3Register,
    InterruptOnChangeTrigger trigger
  )
  {
    chipSetting3Register &= unchecked((byte)~IntDetBitMask);
    chipSetting3Register |= (byte)(((byte)trigger & (IntDetBitMask >> IntDetShiftAmount)) << IntDetShiftAmount);
  }

  public static VoltageReferenceSource ReadAdcVoltageReference(byte chipSetting3RegisterValue)
  {
    var vrmEnabled = 0 != (chipSetting3RegisterValue & AdcRefBitMask);

    if (vrmEnabled) {
      return (VoltageReferenceSource)(
        ((chipSetting3RegisterValue & AdcVrmBitMask) | AdcRefBitMask) >> AdcRefShiftAmount
      );
    }
    else {
      // If Vdd is selected as the reference voltage by ADCREF,
      // specifying a reference voltage via ADCVRM is meaningless;
      // therefore, the setting value is normalized and returned as
      // VoltageReferenceSource.Vdd.
      return VoltageReferenceSource.Vdd;
    }
  }

  public static void WriteAdcVoltageReference(
    ref byte chipSetting3Register,
    VoltageReferenceSource voltageReference
  )
  {
    const byte AdcBitMask = AdcVrmBitMask | AdcRefBitMask;

    chipSetting3Register &= unchecked((byte)~AdcBitMask);
    chipSetting3Register |= (byte)(((byte)voltageReference & (AdcBitMask >> AdcRefShiftAmount)) << AdcRefShiftAmount);
  }

  public static UsbPowerMode ReadUsbPowerMode(byte usbPwrAttrRegisterValue)
    => (usbPwrAttrRegisterValue & SelfPwrBitMask) == 0
      ? UsbPowerMode.BusPowered
      : UsbPowerMode.SelfPowered;

  public static void WriteUsbPowerMode(
    ref byte usbPwrAttrRegister,
    UsbPowerMode powerMode
  )
  {
    usbPwrAttrRegister &= unchecked((byte)~SelfPwrBitMask);
    usbPwrAttrRegister |= (powerMode == UsbPowerMode.SelfPowered) ? SelfPwrBitMask : Zero;
  }

  public static bool ReadUsbRemoteWakeUpEnabled(byte usbPwrAttrRegisterValue)
    => (usbPwrAttrRegisterValue & RemWkUpBitMask) != 0;

  public static void WriteUsbRemoteWakeUpEnabled(
    ref byte usbPwrAttrRegister,
    bool enabled
  )
  {
    usbPwrAttrRegister &= unchecked((byte)~RemWkUpBitMask);
    usbPwrAttrRegister |= enabled ? RemWkUpBitMask : Zero;
  }

  public static int ReadUsbRequestedCurrentAmount(byte usbReqCrtRegisterValue)
    => usbReqCrtRegisterValue << 1; /* USBREQCRT; in units of 2 mA */

  public static void WriteUsbRequestedCurrentAmount(
    ref byte usbReqCrtRegister,
    int currentAmount
  )
    => usbReqCrtRegister = (byte)((currentAmount / 2) & 0xFF); /* USBREQCRT; in units of 2 mA */
}
