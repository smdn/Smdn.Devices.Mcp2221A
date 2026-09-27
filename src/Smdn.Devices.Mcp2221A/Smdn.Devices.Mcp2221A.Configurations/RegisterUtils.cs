// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A.Configurations;

internal static class RegisterUtils {
  public static bool ReadUsbCdcSerialNumberEnabled(byte chipSetting0RegisterValue)
    => (chipSetting0RegisterValue & 0b_1_00000_00) != 0; // CHIPSETTING0 bit 7 CDCSNEN

  public static DeviceConfigurationProtectionLevel ReadFlashWriteProtection(byte chipSetting0RegisterValue)
    => (DeviceConfigurationProtectionLevel)(
      chipSetting0RegisterValue & 0b_0_00000_11 // CHIPSETTING0 bit 1-0 CHIPPROT
    );

  public static ClockOutputDutyCycle ReadClockOutputDutyCycle(byte chipSetting1RegisterValue)
    => (ClockOutputDutyCycle)(
      (chipSetting1RegisterValue & 0b_000_11_000) >> 3 // CHIPSETTING1 bit 4-3 CLKDC
    );

  public static ClockOutputFrequency ReadClockOutputFrequency(byte chipSetting1RegisterValue)
    => (ClockOutputFrequency)(
      chipSetting1RegisterValue & 0b_000_00_111 // CHIPSETTING1 bit 2-0 CLKDIV
    );

  public static VoltageReferenceSource ReadDacVoltageReference(byte chipSetting2RegisterValue)
  {
    // CHIPSETTING2 bit 5 DACREF
    var vrmEnabled = 0 != (chipSetting2RegisterValue & 0b_00_1_00000);

    if (vrmEnabled) {
      return (VoltageReferenceSource)(
        // CHIPSETTING2 bit 7-6 DACVRM
        ((chipSetting2RegisterValue & 0b_11_0_00000) | 0b_00_1_00000) >> 5
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

  public static InterruptOnChangeTrigger ReadInterruptOnChangeTrigger(byte chipSetting3RegisterValue)
    => (InterruptOnChangeTrigger)(
      // CHIPSETTING3 bit 6 INTDETFEEN, bit 5 INTDETREEN
      (chipSetting3RegisterValue & 0b_0_1_1_00_0_00) >> 5
    );

  public static VoltageReferenceSource ReadAdcVoltageReference(byte chipSetting3RegisterValue)
  {
    // CHIPSETTING3 bit 2 ADCREF
    var vrmEnabled = 0 != (chipSetting3RegisterValue & 0b_0_0_0_00_1_00);

    if (vrmEnabled) {
      return (VoltageReferenceSource)(
        // CHIPSETTING3 bit 4-3 ADCVRM
        ((chipSetting3RegisterValue & 0b_0_0_0_11_0_00) | 0b_0_0_0_00_1_00) >> 2
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

  public static int ReadDacInitialValue(byte chipSetting2RegisterValue)
    => chipSetting2RegisterValue & 0b_00_0_11111; // CHIPSETTING1 bit 4-0 DACVAL

  public static UsbPowerMode ReadUsbPowerMode(byte usbPwrAttrRegisterValue)
    => (usbPwrAttrRegisterValue & 0b_0_1_0_00000) == 0 // USBPWRATTR bit 6 SELFPWR
      ? UsbPowerMode.BusPowered
      : UsbPowerMode.SelfPowered;

  public static bool ReadUsbRemoteWakeUpEnabled(byte usbPwrAttrRegisterValue)
    => (usbPwrAttrRegisterValue & 0b_0_0_1_00000) != 0; // USBPWRATTR bit 5 REMWKUP

  public static int ReadUsbRequestedCurrentAmount(byte usbReqCrtRegisterValue)
    => usbReqCrtRegisterValue << 1; /*USBREQCRT; in units of 2 mA */
}
