// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  [TestCase(0b_0_11111_00, false)] // factory default
  [TestCase(0b_1_11111_00, true)]
  public void CdcSerialNumberEnumerationEnabled_Initial(
    byte chipSetting0,
    bool expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: chipSetting0
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.CdcSerialNumberEnumerationEnabled, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(true)]
  [TestCase(false)]
  public void CdcSerialNumberEnumerationEnabled_Staged(bool enable)
  {
    // FlashSettings.ModifyCdcSerialNumberEnumerationEnabled
  }
#endif

  [TestCase(0b_0_11111_11, DeviceConfigurationProtectionLevel.Reserved)]
  [TestCase(0b_0_11111_10, DeviceConfigurationProtectionLevel.PermanentlyLocked)]
  [TestCase(0b_0_11111_01, DeviceConfigurationProtectionLevel.PasswordProtected)]
  [TestCase(0b_0_11111_00, DeviceConfigurationProtectionLevel.None)] // factory default
  public void WriteProtectionLevel_Initial(
    byte chipSetting0,
    DeviceConfigurationProtectionLevel expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: chipSetting0
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.WriteProtectionLevel, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(DeviceConfigurationProtectionLevel.Reserved)]
  [TestCase(DeviceConfigurationProtectionLevel.PermanentlyLocked)]
  [TestCase(DeviceConfigurationProtectionLevel.PasswordProtected)]
  [TestCase(DeviceConfigurationProtectionLevel.None)]
  public void WriteProtectionLevel_Staged(DeviceConfigurationProtectionLevel level)
  {
    // FlashSettings.ModifyWriteProtectionLevel
  }
#endif

  [TestCase(0b_000_11_010, ClockOutputDutyCycle.Duty75)]
  [TestCase(0b_000_10_010, ClockOutputDutyCycle.Duty50)] // factory default
  [TestCase(0b_000_01_010, ClockOutputDutyCycle.Duty25)]
  [TestCase(0b_000_00_010, ClockOutputDutyCycle.Duty0)]
  public void ClockOutputDutyCycle_Initial(
    byte chipSetting1,
    ClockOutputDutyCycle expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting1: chipSetting1
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.ClockOutputDutyCycle, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(ClockOutputDutyCycle.Duty75)]
  [TestCase(ClockOutputDutyCycle.Duty50)]
  [TestCase(ClockOutputDutyCycle.Duty25)]
  [TestCase(ClockOutputDutyCycle.Duty0)]
  public void ClockOutputDutyCycle_Staged(ClockOutputDutyCycle dutyCycle)
  {
    // FlashSettings.ModifyClockOutputDutyCycle
  }
#endif

  [TestCase(0b_000_10_111, ClockOutputFrequency.Frequency375kHz)]
  [TestCase(0b_000_10_110, ClockOutputFrequency.Frequency750kHz)]
  [TestCase(0b_000_10_101, ClockOutputFrequency.Frequency1500kHz)]
  [TestCase(0b_000_10_100, ClockOutputFrequency.Frequency3MHz)]
  [TestCase(0b_000_10_011, ClockOutputFrequency.Frequency6MHz)]
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency12MHz)] // factory default
  [TestCase(0b_000_10_001, ClockOutputFrequency.Frequency24MHz)]
  [TestCase(0b_000_10_000, ClockOutputFrequency.Reserved)]
  public void ClockOutputFrequency_Initial(
    byte chipSetting1,
    ClockOutputFrequency expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting1: chipSetting1
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.ClockOutputFrequency, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(ClockOutputFrequency.Frequency375kHz)]
  [TestCase(ClockOutputFrequency.Frequency750kHz)]
  [TestCase(ClockOutputFrequency.Frequency1500kHz)]
  [TestCase(ClockOutputFrequency.Frequency3MHz)]
  [TestCase(ClockOutputFrequency.Frequency6MHz)]
  [TestCase(ClockOutputFrequency.Frequency12MHz)]
  [TestCase(ClockOutputFrequency.Frequency24MHz)]
  [TestCase(ClockOutputFrequency.Reserved)]
  public void ClockOutputFrequency_Staged(ClockOutputFrequency frequency)
  {
    // FlashSettings.ModifyClockOutputFrequency
  }
#endif

  [TestCase(0b_11_1_00000, VoltageReferenceSource.Vrm4096)]
  [TestCase(0b_10_1_00000, VoltageReferenceSource.Vrm2048)]
  [TestCase(0b_01_1_00000, VoltageReferenceSource.Vrm1024)]
  [TestCase(0b_00_1_00000, VoltageReferenceSource.VrmOff)]
  [TestCase(0b_11_0_00000, VoltageReferenceSource.Vdd)] // DACVRM should be ignored
  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vdd)] // factory default; DACVRM should be ignored
  [TestCase(0b_01_0_00000, VoltageReferenceSource.Vdd)] // DACVRM should be ignored
  [TestCase(0b_00_0_00000, VoltageReferenceSource.Vdd)] // DACVRM should be ignored
  public void DacVoltageReference_Initial(
    byte chipSetting2,
    VoltageReferenceSource expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting2: chipSetting2
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.DacVoltageReference, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(VoltageReferenceSource.Vrm4096)]
  [TestCase(VoltageReferenceSource.Vrm2048)]
  [TestCase(VoltageReferenceSource.Vrm1024)]
  [TestCase(VoltageReferenceSource.VrmOff)]
  [TestCase(VoltageReferenceSource.Vdd)]
  public void DacVoltageReference_Staged(VoltageReferenceSource dacVoltageReference)
  {
    // FlashSettings.ModifyDacVoltageReference
  }
#endif

  [TestCase(0b_10_0_11111, 31)]
  [TestCase(0b_10_0_01000, 8)] // factory default
  [TestCase(0b_00_1_00001, 1)]
  [TestCase(0b_00_1_00000, 0)]
  public void DacInitialValue_Initial(
    byte chipSetting2,
    int expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting2: chipSetting2
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.DacInitialValue, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(15)]
  [TestCase(8)]
  [TestCase(1)]
  [TestCase(0)]
  public void DacInitialValue_Staged(int dacInitialValue)
  {
    // FlashSettings.ModifyDacInitialValue
  }
#endif

  [TestCase(0b_0_1_1_01_1_00, InterruptOnChangeTrigger.Both)] // factory default
  [TestCase(0b_0_1_0_00_0_00, InterruptOnChangeTrigger.Falling)]
  [TestCase(0b_0_0_1_00_0_00, InterruptOnChangeTrigger.Rising)]
  [TestCase(0b_0_0_0_00_0_00, InterruptOnChangeTrigger.None)]
  public void InterruptOnChangeTrigger_Initial(
    byte chipSetting3,
    InterruptOnChangeTrigger expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting3: chipSetting3
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.InterruptOnChangeTrigger, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(InterruptOnChangeTrigger.Both)]
  [TestCase(InterruptOnChangeTrigger.Falling)]
  [TestCase(InterruptOnChangeTrigger.Rising)]
  [TestCase(InterruptOnChangeTrigger.None)]
  public void InterruptOnChangeTrigger_Staged(InterruptOnChangeTrigger trigger)
  {
    // FlashSettings.ModifyInterruptOnChangeTrigger
  }
#endif

  [TestCase(0b_0_0_0_11_1_00, VoltageReferenceSource.Vrm4096)]
  [TestCase(0b_0_0_0_10_1_00, VoltageReferenceSource.Vrm2048)]
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vrm1024)] // factory default
  [TestCase(0b_0_0_0_00_1_00, VoltageReferenceSource.VrmOff)]
  [TestCase(0b_0_0_0_11_0_00, VoltageReferenceSource.Vdd)] // ADCVRM should be ignored
  [TestCase(0b_0_0_0_10_0_00, VoltageReferenceSource.Vdd)] // ADCVRM should be ignored
  [TestCase(0b_0_0_0_01_0_00, VoltageReferenceSource.Vdd)] // ADCVRM should be ignored
  [TestCase(0b_0_0_0_00_0_00, VoltageReferenceSource.Vdd)] // ADCVRM should be ignored
  public void AdcVoltageReference_Initial(
    byte chipSetting3,
    VoltageReferenceSource expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting3: chipSetting3
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.AdcVoltageReference, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(VoltageReferenceSource.Vrm4096)]
  [TestCase(VoltageReferenceSource.Vrm2048)]
  [TestCase(VoltageReferenceSource.Vrm1024)]
  [TestCase(VoltageReferenceSource.VrmOff)]
  [TestCase(VoltageReferenceSource.Vdd)]
  public void AdcVoltageReference_Staged(VoltageReferenceSource adcVoltageReference)
  {
    // FlashSettings.ModifyAdcVoltageReference
  }
#endif

  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbVendorId_Initial(int vendorId)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbVidHigherByte: (byte)((vendorId & 0xFF00) >> 8),
        usbVidLowerByte: (byte)(vendorId & 0xFF)
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.UsbVendorId, Is.EqualTo(vendorId));
  }

  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbVendorId_Staged(int vendorId)
  {
    var initialVendorId = ~vendorId;

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbVidHigherByte: (byte)((initialVendorId & 0xFF00) >> 8),
        usbVidLowerByte: (byte)(initialVendorId & 0xFF)
      )
    );

    mcp2221A.Flash.ModifyUsbVendorId(vendorId);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True);
    Assert.That(mcp2221A.Flash.UsbVendorId, Is.EqualTo(vendorId));
  }

  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbProductId_Initial(int productId)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPidHigherByte: (byte)((productId & 0xFF00) >> 8),
        usbPidLowerByte: (byte)(productId & 0xFF)
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.UsbProductId, Is.EqualTo(productId));
  }

  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void UsbProductId_Staged(int productId)
  {
    var initialProductId = ~productId;

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPidHigherByte: (byte)((initialProductId & 0xFF00) >> 8),
        usbPidLowerByte: (byte)(initialProductId & 0xFF)
      )
    );

    mcp2221A.Flash.ModifyUsbProductId(productId);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True);
    Assert.That(mcp2221A.Flash.UsbProductId, Is.EqualTo(productId));
  }

  [TestCase(0b_0_1_0_00000, UsbPowerMode.SelfPowered)]
  [TestCase(0b_0_0_0_00000, UsbPowerMode.BusPowered)] // factory default
  public void UsbPowerMode_Initial(
    byte usbPowerAttributes,
    UsbPowerMode expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPowerAttributes: usbPowerAttributes
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.UsbPowerMode, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(UsbPowerMode.SelfPowered)]
  [TestCase(UsbPowerMode.BusPowered)]
  public void UsbPowerMode_Staged(UsbPowerMode powerMode)
  {
    // FlashSettings.ModifyUsbPowerMode
  }
#endif

  [TestCase(0b_0_0_1_00000, true)]
  [TestCase(0b_0_0_0_00000, false)] // factory default
  public void UsbRemoteWakeUpEnabled_Initial(
    byte usbPowerAttributes,
    bool expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPowerAttributes: usbPowerAttributes
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.UsbRemoteWakeUpEnabled, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(true)]
  [TestCase(false)]
  public void UsbRemoteWakeUpEnabled_Staged(bool enable)
  {
    // FlashSettings.ModifyUsbRemoteWakeUpEnabled
  }
#endif

  [TestCase(0b_00000000, 0)]
  [TestCase(0b_00110010, 100)] // factory default
  [TestCase(0b_11111010, 500)]
  [TestCase(0b_11111111, 510)]
  public void UsbRequestedCurrentAmount_Initial(
    byte usbRequiredCurrent,
    int expected
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbRequiredCurrent: usbRequiredCurrent
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.UsbRequestedCurrentAmount, Is.EqualTo(expected));
  }

#if false // TODO
  [TestCase(0, 0)]
  [TestCase(1, 0)]
  [TestCase(2, 2)]
  [TestCase(100, 100)] // factory default
  [TestCase(500, 500)]
  [TestCase(509, 508)]
  [TestCase(510, 510)]
  public void UsbRequestedCurrentAmount_Staged(
    int currentAmount,
    int expectedCurrentAmount
  )
  {
    // FlashSettings.ModifyUsbRequestedCurrentAmount
  }
#endif

  [TestCase(0b_000_1_0_010, true, PinMode.Output, GpFunction.LedOutput, false)] // Alternate function 0 (LEDURX); factory default
  [TestCase(0b_000_0_1_001, false, PinMode.Input, GpFunction.UsbSuspendStatus, false)] // Dedicated function operation (SSPND)
  [TestCase(0b_000_1_1_000, true, PinMode.Input, GpFunction.Gpio, true)] // GPIO operation (GPIO0)
  [TestCase(0b_111_0_0_111, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_100_0_0_110, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_010_0_0_101, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_001_0_0_100, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_000_0_0_011, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  public void GpPin0_Initial(
    byte gp0Settings,
    bool expectedGpioOutputValue, // true for HIGH, false for LOW
    PinMode expectedGpioMode,
    GpFunction expectedFunction,
    bool expectedIsGpio
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gp0Settings
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.GpPin0.Index, Is.Zero);
    Assert.That(mcp2221A.Flash.GpPin0.Designation, Is.EqualTo(gp0Settings & 0b111));
    Assert.That(mcp2221A.Flash.GpPin0.Function, Is.EqualTo(expectedFunction));
    Assert.That(mcp2221A.Flash.GpPin0.IsGpio, Is.EqualTo(expectedIsGpio));
    Assert.That(mcp2221A.Flash.GpPin0.GpioMode, Is.EqualTo(expectedGpioMode));
    Assert.That(mcp2221A.Flash.GpPin0.GpioOutputValue, Is.EqualTo((PinValue)expectedGpioOutputValue));

    Assert.That(
      mcp2221A.Flash.GpPin0,
      Is.EqualTo(mcp2221A.Flash.GpPins[0])
    );
  }

#if false // TODO
  public void GpPin0_Staged()
  {
    // FlashSettings.ModifyGpSetting(index: 0, ...)
  }
#endif

  [TestCase(0b_000_1_1_100, true, PinMode.Input, GpFunction.InterruptOnChange, false)] // Alternate function 2 (Interrupt Detector)
  [TestCase(0b_000_1_0_011, true, PinMode.Output, GpFunction.LedOutput, false)] // Alternate function 1 (LEDUTX); factory default
  [TestCase(0b_000_0_1_010, false, PinMode.Input, GpFunction.Adc, false)] // Alternate function 0 (ADC1)
  [TestCase(0b_000_0_0_001, false, PinMode.Output, GpFunction.ClockOutput, false)] // Dedicated function operation (Clock Output)
  [TestCase(0b_000_1_0_000, true, PinMode.Output, GpFunction.Gpio, true)] // GPIO operation (GPIO1)
  [TestCase(0b_001_0_0_111, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_010_0_0_110, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_100_0_0_101, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  public void GpPin1_Initial(
    byte gp1Settings,
    bool expectedGpioOutputValue, // true for HIGH, false for LOW
    PinMode expectedGpioMode,
    GpFunction expectedFunction,
    bool expectedIsGpio
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: gp1Settings
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.GpPin1.Index, Is.EqualTo(1));
    Assert.That(mcp2221A.Flash.GpPin1.Designation, Is.EqualTo(gp1Settings & 0b111));
    Assert.That(mcp2221A.Flash.GpPin1.Function, Is.EqualTo(expectedFunction));
    Assert.That(mcp2221A.Flash.GpPin1.IsGpio, Is.EqualTo(expectedIsGpio));
    Assert.That(mcp2221A.Flash.GpPin1.GpioMode, Is.EqualTo(expectedGpioMode));
    Assert.That(mcp2221A.Flash.GpPin1.GpioOutputValue, Is.EqualTo((PinValue)expectedGpioOutputValue));

    Assert.That(
      mcp2221A.Flash.GpPin1,
      Is.EqualTo(mcp2221A.Flash.GpPins[1])
    );
  }

#if false // TODO
  public void GpPin1_Staged()
  {
    // FlashSettings.ModifyGpSetting(index: 1, ...)
  }
#endif

  [TestCase(0b_000_1_1_011, true, PinMode.Input, GpFunction.Dac, false)] // Alternate function 1 (DAC1)
  [TestCase(0b_000_0_1_010, false, PinMode.Input, GpFunction.Adc, false)] // Alternate function 0 (ADC2)
  [TestCase(0b_000_1_0_001, true, PinMode.Output, GpFunction.UsbConfigureStatus, false)] // Dedicated function operation (USBCFG); factory default
  [TestCase(0b_000_0_0_000, false, PinMode.Output, GpFunction.Gpio, true)] // GPIO operation (GPIO2)
  [TestCase(0b_001_0_0_111, false, PinMode.Output, GpFunction.UsbConfigureStatus, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_010_0_0_110, false, PinMode.Output, GpFunction.UsbConfigureStatus, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_100_0_0_101, false, PinMode.Output, GpFunction.UsbConfigureStatus, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_000_0_0_100, false, PinMode.Output, GpFunction.UsbConfigureStatus, false)] // GPDES=Reserved: falls back to factory default
  public void GpPin2_Initial(
    byte gp2Settings,
    bool expectedGpioOutputValue, // true for HIGH, false for LOW
    PinMode expectedGpioMode,
    GpFunction expectedFunction,
    bool expectedIsGpio
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp2Settings: gp2Settings
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.GpPin2.Index, Is.EqualTo(2));
    Assert.That(mcp2221A.Flash.GpPin2.Designation, Is.EqualTo(gp2Settings & 0b111));
    Assert.That(mcp2221A.Flash.GpPin2.Function, Is.EqualTo(expectedFunction));
    Assert.That(mcp2221A.Flash.GpPin2.IsGpio, Is.EqualTo(expectedIsGpio));
    Assert.That(mcp2221A.Flash.GpPin2.GpioMode, Is.EqualTo(expectedGpioMode));
    Assert.That(mcp2221A.Flash.GpPin2.GpioOutputValue, Is.EqualTo((PinValue)expectedGpioOutputValue));

    Assert.That(
      mcp2221A.Flash.GpPin2,
      Is.EqualTo(mcp2221A.Flash.GpPins[2])
    );
  }

#if false // TODO
  public void GpPin2_Staged()
  {
    // FlashSettings.ModifyGpSetting(index: 2, ...)
  }
#endif

  [TestCase(0b_000_1_1_011, true, PinMode.Input, GpFunction.Dac, false)] // Alternate function 1 (DAC2)
  [TestCase(0b_000_0_1_010, false, PinMode.Input, GpFunction.Adc, false)] // Alternate function 0 (ADC3)
  [TestCase(0b_000_1_0_001, true, PinMode.Output, GpFunction.LedOutput, false)] // Dedicated function operation (LEDI2C); factory default
  [TestCase(0b_000_0_0_000, false, PinMode.Output, GpFunction.Gpio, true)] // GPIO operation (GPIO3)
  [TestCase(0b_111_0_0_111, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_011_0_0_110, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_110_0_0_101, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  [TestCase(0b_000_0_0_100, false, PinMode.Output, GpFunction.LedOutput, false)] // GPDES=Reserved: falls back to factory default
  public void GpPin3_Initial(
    byte gp3Settings,
    bool expectedGpioOutputValue, // true for HIGH, false for LOW
    PinMode expectedGpioMode,
    GpFunction expectedFunction,
    bool expectedIsGpio
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp3Settings: gp3Settings
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(mcp2221A.Flash.GpPin3.Index, Is.EqualTo(3));
    Assert.That(mcp2221A.Flash.GpPin3.Designation, Is.EqualTo(gp3Settings & 0b111));
    Assert.That(mcp2221A.Flash.GpPin3.Function, Is.EqualTo(expectedFunction));
    Assert.That(mcp2221A.Flash.GpPin3.IsGpio, Is.EqualTo(expectedIsGpio));
    Assert.That(mcp2221A.Flash.GpPin3.GpioMode, Is.EqualTo(expectedGpioMode));
    Assert.That(mcp2221A.Flash.GpPin3.GpioOutputValue, Is.EqualTo((PinValue)expectedGpioOutputValue));

    Assert.That(
      mcp2221A.Flash.GpPin3,
      Is.EqualTo(mcp2221A.Flash.GpPins[3])
    );
  }

#if false // TODO
  public void GpPin3_Staged()
  {
    // FlashSettings.ModifyGpSetting(index: 3, ...)
  }
#endif

  [Test]
  public void GpPins_IReadOnlyList_Count()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(
      mcp2221A.Flash.GpPins.Count,
      Is.EqualTo(4)
    );
  }

  [TestCase(int.MinValue)]
  [TestCase(-1)]
  [TestCase(4)]
  [TestCase(int.MaxValue)]
  public void GpPins_IReadOnlyList_IndexOutOfRange(int index)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(
      () => _ = mcp2221A.Flash.GpPins[index],
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("index")
        .And
        .Property(nameof(ArgumentOutOfRangeException.ActualValue))
        .EqualTo(index)
    );
  }

  [Test]
  public void GpPins_IReadOnlyList_GetGenericEnumerator()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(
      mcp2221A.Flash.GpPins.Count(),
      Is.EqualTo(4)
    );
    Assert.That(
      mcp2221A.Flash.GpPins.ToArray(),
      Is
        .EqualTo([mcp2221A.Flash.GpPin0, mcp2221A.Flash.GpPin1, mcp2221A.Flash.GpPin2, mcp2221A.Flash.GpPin3])
        .AsCollection
    );
  }

  [Test]
  public void GpPins_IReadOnlyList_GetNonGenericEnumerator()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    System.Collections.IEnumerable enumerable = mcp2221A.Flash.GpPins;

    Assert.That(
      enumerable.GetEnumerator(),
      Is.Not.Null
    );
    Assert.That(
      enumerable,
      Is.Not.Empty
    );
    Assert.That(
      enumerable.Cast<GpSetting>().ToArray(),
      Is
        .EqualTo([mcp2221A.Flash.GpPin0, mcp2221A.Flash.GpPin1, mcp2221A.Flash.GpPin2, mcp2221A.Flash.GpPin3])
        .AsCollection
    );
  }

  [TestCase("Microchip Technology Inc.")] // factory default
  [TestCase("")]
  [TestCase("Vendor")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbManufacturerStringTo_Initial(string manufacturer)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: manufacturer
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    var buffer = new char[30];

    Assert.That(
      mcp2221A.Flash.TryCopyUsbManufacturerStringTo(buffer, out var charsWritten),
      Is.True
    );
    Assert.That(charsWritten, Is.EqualTo(manufacturer.Length));
    Assert.That(new string(buffer, 0, charsWritten), Is.EqualTo(manufacturer));
  }

  [TestCase("Microchip Technology Inc.")] // factory default
  [TestCase("Vendor")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbManufacturerStringTo_DestinationTooShort(string manufacturer)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: manufacturer
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    Assert.That(
      mcp2221A.Flash.TryCopyUsbManufacturerStringTo(Array.Empty<char>(), out _),
      Is.False
    );
  }

#if false // TODO
  [TestCase]
  public void TryCopyUsbManufacturerStringTo_Staged(string manufacturer)
  {
    // FlashSettings.ModifyUsbManufacturerString
  }
#endif

  [TestCase("MCP2221 USB-I2C/UART Combo")] // factory default
  [TestCase("")]
  [TestCase("Product")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbProductStringTo_Initial(string product)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: product
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    var buffer = new char[30];

    Assert.That(
      mcp2221A.Flash.TryCopyUsbProductStringTo(buffer, out var charsWritten),
      Is.True
    );
    Assert.That(charsWritten, Is.EqualTo(product.Length));
    Assert.That(new string(buffer, 0, charsWritten), Is.EqualTo(product));
  }

  [TestCase("MCP2221 USB-I2C/UART Combo")] // factory default
  [TestCase("Product")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbProductStringTo_DestinationTooShort(string product)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: product
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    Assert.That(
      mcp2221A.Flash.TryCopyUsbProductStringTo(Array.Empty<char>(), out _),
      Is.False
    );
  }

#if false // TODO
  [TestCase]
  public void TryCopyUsbProductStringTo_Staged(string product)
  {
    // FlashSettings.ModifyUsbProductString
  }
#endif

  [TestCase("")] // factory default
  [TestCase("Serial Number")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbSerialNumberStringTo_Initial(string serialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: serialNumber
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    var buffer = new char[30];

    Assert.That(
      mcp2221A.Flash.TryCopyUsbSerialNumberStringTo(buffer, out var charsWritten),
      Is.True
    );
    Assert.That(charsWritten, Is.EqualTo(serialNumber.Length));
    Assert.That(new string(buffer, 0, charsWritten), Is.EqualTo(serialNumber));
  }

  [TestCase("Serial Number")]
  [TestCase("🔌")]
  [TestCase("012345678901234567890123456789")] // max length
  public void TryCopyUsbSerialNumberStringTo_DestinationTooShort(string serialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: serialNumber
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    Assert.That(
      mcp2221A.Flash.TryCopyUsbSerialNumberStringTo(Array.Empty<char>(), out _),
      Is.False
    );
  }

#if false // TODO
  [TestCase]
  public void TryCopyUsbSerialNumberStringTo_Staged(string serialNumber)
  {
    // FlashSettings.ModifyUsbSerialNumberString
  }
#endif

  [TestCase("01234567")] // factory default
  [TestCase("")]
  [TestCase("ABC")]
  [TestCase("012345678901234567890123456789012345678901234567890123456789")] // max length
  public void TryCopyChipFactorySerialNumberTo_Initial(string chipFactorySerialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipFactorySerialNumber: chipFactorySerialNumber
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    var buffer = new char[60];

    Assert.That(
      mcp2221A.Flash.TryCopyChipFactorySerialNumberTo(buffer, out var charsWritten),
      Is.True
    );
    Assert.That(charsWritten, Is.EqualTo(chipFactorySerialNumber.Length));
    Assert.That(new string(buffer, 0, charsWritten), Is.EqualTo(chipFactorySerialNumber));
  }

  [TestCase("01234567")] // factory default
  [TestCase("ABC")]
  [TestCase("012345678901234567890123456789012345678901234567890123456789")] // max length
  public void TryCopyChipFactorySerialNumberTo_DestinationTooShort(string chipFactorySerialNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipFactorySerialNumber: chipFactorySerialNumber
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);

    Assert.That(
      mcp2221A.Flash.TryCopyChipFactorySerialNumberTo(Array.Empty<char>(), out _),
      Is.False
    );
  }

#if false // TODO
  [TestCase]
  public void TryCopyChipFactorySerialNumberTo_Staged(string manufacturer)
  {
    // FlashSettings.ModifyChipFactorySerialNumber
  }
#endif
}
