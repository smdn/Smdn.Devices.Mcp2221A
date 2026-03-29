// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public class Gp2ControllerTests {
  private static System.Collections.IEnumerable YieldTestCases_IsFunctionSupported()
  {
    const bool IsSupported = true;
    const bool IsNotSupported = false;

    yield return new object[] { GpFunction.Gpio, IsSupported };
    yield return new object[] { GpFunction.UsbConfigureStatus, IsSupported };
    yield return new object[] { GpFunction.Adc, IsSupported };
    yield return new object[] { GpFunction.Dac, IsSupported };

    yield return new object[] { GpFunction.ExternalInterrupt, IsNotSupported };
    yield return new object[] { GpFunction.LedOutput, IsNotSupported };
    yield return new object[] { GpFunction.ClockOutput, IsNotSupported };
    yield return new object[] { GpFunction.UsbSuspendStatus, IsNotSupported };

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

    Assert.That(mcp2221A.GpPin2.IsFunctionSupported(function), Is.EqualTo(expected));
  }

  private static IEnumerable<(byte, PinValue?, PinMode?, GpFunction, string)> YieldTestCases_Gp2Settings()
  {
    yield return (0b_000_0_0_001, null, null, GpFunction.UsbConfigureStatus, "USBCFG");
    yield return (0b_000_0_0_010, null, null, GpFunction.Adc, "ADC2");
    yield return (0b_000_0_0_011, null, null, GpFunction.Dac, "DAC1");

    yield return (0b_000_0_0_000, PinValue.Low, PinMode.Output, GpFunction.Gpio, "GPIO2");
    yield return (0b_000_0_1_000, PinValue.Low, PinMode.Input, GpFunction.Gpio, "GPIO2");
    yield return (0b_000_1_0_000, PinValue.High, PinMode.Output, GpFunction.Gpio, "GPIO2");
    yield return (0b_000_1_1_000, PinValue.High, PinMode.Input, GpFunction.Gpio, "GPIO2");

    yield return (0b_000_1_1_001, null, null, GpFunction.UsbConfigureStatus, "USBCFG");
  }

  private static System.Collections.IEnumerable YieldTestCases_LastUpdatedValue_AtStartup()
  {
    foreach (var (gp2Settings, pinValue, _, _, _) in YieldTestCases_Gp2Settings()) {
      yield return new object?[] { gp2Settings, pinValue };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_LastUpdatedValue_AtStartup))]
  public void LastUpdatedValue_AtStartup(byte gp2Settings, PinValue? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp2Settings: gp2Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin2.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentMode_AtStartup()
  {
    foreach (var (gp2Settings, _, pinMode, _, _) in YieldTestCases_Gp2Settings()) {
      yield return new object?[] { gp2Settings, pinMode };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentMode_AtStartup))]
  public void CurrentMode_AtStartup(byte gp2Settings, PinMode? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp2Settings: gp2Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin2.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentFunction_AtStartup()
  {
    foreach (var (gp2Settings, _, _, function, _) in YieldTestCases_Gp2Settings()) {
      yield return new object[] { gp2Settings, function };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentFunction_AtStartup))]
  public void CurrentFunction_AtStartup(byte gp2Settings, GpFunction expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp2Settings: gp2Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentDesignation_AtStartup()
  {
    foreach (var (gp2Settings, _, _, _, designation) in YieldTestCases_Gp2Settings()) {
      yield return new object[] { gp2Settings, designation };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentDesignation_AtStartup))]
  public void CurrentDesignation_AtStartup(byte gp2Settings, string expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp2Settings: gp2Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin2.CurrentDesignation, Is.EqualTo(expected));
  }
}
