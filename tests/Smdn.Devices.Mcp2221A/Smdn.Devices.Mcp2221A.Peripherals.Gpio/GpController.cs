// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public partial class GpControllerTests {
  public delegate GpController SelectGpControllerFunc(Mcp2221AController mcp2221A);

  private static GpController SelectGp0Controller(Mcp2221AController mcp2221A) => mcp2221A.GpPin0;
  private static GpController SelectGp1Controller(Mcp2221AController mcp2221A) => mcp2221A.GpPin1;
  private static GpController SelectGp2Controller(Mcp2221AController mcp2221A) => mcp2221A.GpPin2;
  private static GpController SelectGp3Controller(Mcp2221AController mcp2221A) => mcp2221A.GpPin3;

  private static Mcp2221AController CreateMcp2221AConfiguredAsGpio(
    ReadOnlySpan<PinValuePair> initialValues = default,
    ReadOnlySpan<PinModePair> initialModes = default
  )
  {
    Span<byte> gpSettings = [
      0b_000_0_0_000, // GPIO operation
      0b_000_0_0_000, // GPIO operation
      0b_000_0_0_000, // GPIO operation
      0b_000_0_0_000 // GPIO operation
    ];

    foreach (var (gp, value) in initialValues) {
      gpSettings[gp] |= (byte)((bool)value ? 0b_000_1_0_000 : 0b_000_0_0_000);
    }

    foreach (var (gp, mode) in initialModes) {
      gpSettings[gp] |= mode switch {
        PinMode.Output => 0b_000_0_0_000,
        PinMode.Input => 0b_000_0_1_000,
        var invalid => throw new InvalidOperationException("invalid mode: {invalid}"),
      };
    }

    return Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gpSettings[0],
        gp1Settings: gpSettings[1],
        gp2Settings: gpSettings[2],
        gp3Settings: gpSettings[3]
      ),
      shouldDisposeUsbHidDevice: true
    );
  }

  private static Mcp2221AController CreateMcp2221AConfiguredAsAdc(
    byte chipSettings3 = 0b_0_1_1_01_1_00 // VRM 1.024V (factory default)
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp1Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC1)
    const byte InitialGp2Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC2)
    const byte InitialGp3Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC3)

    return Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings3: chipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );
  }

  private static Mcp2221AController CreateMcp2221AConfiguredAsDac(
    byte chipSettings2 = 0b_10_0_01000 // DAC: VDD(VRM 2.048V); Output = 8 (factory default)
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp1Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp2Settings = 0b_000_0_0_011; // Alternate Function 1 (DAC1)
    const byte InitialGp3Settings = 0b_000_0_0_011; // Alternate Function 1 (DAC2)

    return Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings2: chipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );
  }

  private static IEnumerable<byte> YieldTestCases_GP0_InvalidConfigurationSettings()
  {
    yield return 0b_000_1_0_010; // LED_URX
    yield return 0b_000_1_0_001; // SSPND
  }

  private static IEnumerable<byte> YieldTestCases_GP1_InvalidConfigurationSettings()
  {
    yield return 0b_000_1_0_100; // IOC
    yield return 0b_000_1_0_011; // LED_UTX
    yield return 0b_000_1_0_010; // ADC1
    yield return 0b_000_1_0_001; // CLK OUT
  }

  private static IEnumerable<byte> YieldTestCases_GP2_InvalidConfigurationSettings()
  {
    yield return 0b_000_1_0_011; // DAC1
    yield return 0b_000_1_0_010; // ADC2
    yield return 0b_000_1_0_001; // USBCFG
  }

  private static IEnumerable<byte> YieldTestCases_GP3_InvalidConfigurationSettings()
  {
    yield return 0b_000_1_0_011; // DAC2
    yield return 0b_000_1_0_010; // ADC3
    yield return 0b_000_1_0_001; // LED_I2C
  }

  private static IEnumerable<VoltageReferenceSource> YieldTestCases_UndefinedVoltageReferenceSource()
  {
    yield return (VoltageReferenceSource)(-1);
    yield return (VoltageReferenceSource)0b_0_0000_01_0; // VRM 4.096 + ADC voltage reference is VDD
    yield return (VoltageReferenceSource)0b_0_0000_10_0; // VRM 2.048 + ADC voltage reference is VDD
    yield return (VoltageReferenceSource)0b_0_0000_11_0; // VRM 1.024 + ADC voltage reference is VDD
    yield return (VoltageReferenceSource)0b_1_1111_00_0;
    yield return (VoltageReferenceSource)0b_1_1111_11_1;
  }
}
