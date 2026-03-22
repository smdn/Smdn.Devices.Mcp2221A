// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public partial class Mcp2221AGpioDriverTests {
  internal static byte GetGpDesignationBitsForFunction(int gp, GpFunction function)
    => gp switch {
      0 => function switch {
        GpFunction.Gpio => 0b_000_0_0_000, // GPIO
        GpFunction.UsbSuspendStatus => 0b_000_0_0_001, // SSPND
        GpFunction.LedOutput => 0b_000_0_0_010, // LED_URX
        _ => throw new NotSupportedException(),
      },
      1 => function switch {
        GpFunction.Gpio => 0b_000_0_0_000, // GPIO
        GpFunction.ClockOutput => 0b_000_0_0_001, // CLK OUT
        GpFunction.Adc => 0b_000_0_0_010, // ADC1
        GpFunction.LedOutput => 0b_000_0_0_011, // LED_UTX
        GpFunction.ExternalInterrupt => 0b_000_0_0_100, // IOC
        _ => throw new NotSupportedException(),
      },
      2 => function switch {
        GpFunction.Gpio => 0b_000_0_0_000, // GPIO
        GpFunction.UsbConfigureStatus => 0b_000_0_0_001, // USBCFG
        GpFunction.Adc => 0b_000_0_0_010, // ADC2
        GpFunction.Dac => 0b_000_0_0_011, // DAC1
        _ => throw new NotSupportedException(),
      },
      3 => function switch {
        GpFunction.Gpio => 0b_000_0_0_000, // GPIO
        GpFunction.LedOutput => 0b_000_0_0_001, // LED_I2C
        GpFunction.Adc => 0b_000_0_0_010, // ADC3
        GpFunction.Dac => 0b_000_0_0_011, // DAC2
        _ => throw new NotSupportedException(),
      },
      _ => throw new ArgumentOutOfRangeException(paramName: nameof(gp), actualValue: gp, message: $"{nameof(gp)} must be in range of 0-3."),
    };
}



