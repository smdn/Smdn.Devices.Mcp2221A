// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public class Gp1ControllerTests {
  private static System.Collections.IEnumerable YieldTestCases_IsFunctionSupported()
  {
    const bool IsSupported = true;
    const bool IsNotSupported = false;

    yield return new object[] { GpFunction.Gpio, IsSupported };
    yield return new object[] { GpFunction.ClockOutput, IsSupported };
    yield return new object[] { GpFunction.Adc, IsSupported };
    yield return new object[] { GpFunction.LedOutput, IsSupported };
    yield return new object[] { GpFunction.ExternalInterrupt, IsSupported };

    yield return new object[] { GpFunction.Dac, IsNotSupported };
    yield return new object[] { GpFunction.UsbSuspendStatus, IsNotSupported };
    yield return new object[] { GpFunction.UsbConfigureStatus, IsNotSupported };

    yield return new object[] { (GpFunction)(-1), IsNotSupported };
    yield return new object[] { (GpFunction)int.MaxValue, IsNotSupported };
  }

  [TestCaseSource(nameof(YieldTestCases_IsFunctionSupported))]
  public void IsFunctionSupported(
    GpFunction function,
    bool expected
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin1.IsFunctionSupported(function), Is.EqualTo(expected));
  }

  private static IEnumerable<(byte, PinValue?, PinMode?, GpFunction, string)> YieldTestCases_Gp1Settings()
  {
    yield return (0b_000_0_0_001, null, null, GpFunction.ClockOutput, "CLK OUT");
    yield return (0b_000_0_0_010, null, null, GpFunction.Adc, "ADC1");
    yield return (0b_000_0_0_011, null, null, GpFunction.LedOutput, "LED_UTX");
    yield return (0b_000_0_0_100, null, null, GpFunction.ExternalInterrupt, "IOC");

    yield return (0b_000_0_0_000, PinValue.Low, PinMode.Output, GpFunction.Gpio, "GPIO1");
    yield return (0b_000_0_1_000, PinValue.Low, PinMode.Input, GpFunction.Gpio, "GPIO1");
    yield return (0b_000_1_0_000, PinValue.High, PinMode.Output, GpFunction.Gpio, "GPIO1");
    yield return (0b_000_1_1_000, PinValue.High, PinMode.Input, GpFunction.Gpio, "GPIO1");

    yield return (0b_000_1_1_011, null, null, GpFunction.LedOutput, "LED_UTX");
  }

  private static System.Collections.IEnumerable YieldTestCases_LastFetchedValue_AtStartup()
  {
    foreach (var (gp1Settings, pinValue, _, _, _) in YieldTestCases_Gp1Settings()) {
      yield return new object?[] { gp1Settings, pinValue };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_LastFetchedValue_AtStartup))]
  public void LastFetchedValue_AtStartup(byte gp1Settings, PinValue? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp1Settings: gp1Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin1.LastFetchedValue, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
  }

  private static System.Collections.IEnumerable YieldTestCases_LastFetchedMode_AtStartup()
  {
    foreach (var (gp1Settings, _, pinMode, _, _) in YieldTestCases_Gp1Settings()) {
      yield return new object?[] { gp1Settings, pinMode };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_LastFetchedMode_AtStartup))]
  public void LastFetchedMode_AtStartup(byte gp1Settings, PinMode? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp1Settings: gp1Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin1.LastFetchedMode, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentFunction_AtStartup()
  {
    foreach (var (gp1Settings, _, _, function, _) in YieldTestCases_Gp1Settings()) {
      yield return new object?[] { gp1Settings, function };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentFunction_AtStartup))]
  public void CurrentFunction_AtStartup(byte gp1Settings, GpFunction expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp1Settings: gp1Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentDesignation_AtStartup()
  {
    foreach (var (gp1Settings, _, _, _, designation) in YieldTestCases_Gp1Settings()) {
      yield return new object[] { gp1Settings, designation };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentDesignation_AtStartup))]
  public void CurrentDesignation_AtStartup(byte gp1Settings, string expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp1Settings: gp1Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin1.CurrentDesignation, Is.EqualTo(expected));
  }
}
