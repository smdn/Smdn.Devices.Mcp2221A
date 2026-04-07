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
}
