// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public class Gp0ControllerTests {
  private static System.Collections.IEnumerable YieldTestCases_IsFunctionSupported()
  {
    const bool IsSupported = true;
    const bool IsNotSupported = false;

    yield return new object[] { GpFunction.Gpio, IsSupported };
    yield return new object[] { GpFunction.UsbSuspendStatus, IsSupported };
    yield return new object[] { GpFunction.LedOutput, IsSupported };

    yield return new object[] { GpFunction.Adc, IsNotSupported };
    yield return new object[] { GpFunction.Dac, IsNotSupported };
    yield return new object[] { GpFunction.ExternalInterrupt, IsNotSupported };
    yield return new object[] { GpFunction.ClockOutput, IsNotSupported };
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

    Assert.That(mcp2221A.GpPin0.IsFunctionSupported(function), Is.EqualTo(expected));
  }

  private static IEnumerable<(byte, PinValue?, PinMode?, GpFunction, string)> YieldTestCases_Gp0Settings()
  {
    yield return (0b_000_0_0_001, null, null, GpFunction.UsbSuspendStatus, "SSPND");
    yield return (0b_000_0_0_010, null, null, GpFunction.LedOutput, "LED_URX");

    yield return (0b_000_0_0_000, PinValue.Low, PinMode.Output, GpFunction.Gpio, "GPIO0");
    yield return (0b_000_0_1_000, PinValue.Low, PinMode.Input, GpFunction.Gpio, "GPIO0");
    yield return (0b_000_1_0_000, PinValue.High, PinMode.Output, GpFunction.Gpio, "GPIO0");
    yield return (0b_000_1_1_000, PinValue.High, PinMode.Input, GpFunction.Gpio, "GPIO0");

    yield return (0b_000_1_1_010, null, null, GpFunction.LedOutput, "LED_URX");
  }

  private static System.Collections.IEnumerable YieldTestCases_LastFetchedValue_AtStartup()
  {
    foreach (var (gp0Settings, pinValue, _, _, _) in YieldTestCases_Gp0Settings()) {
      yield return new object?[] { gp0Settings, pinValue };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_LastFetchedValue_AtStartup))]
  public void LastFetchedValue_AtStartup(byte gp0Settings, PinValue? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: gp0Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin0.LastFetchedValue, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
  }

  private static System.Collections.IEnumerable YieldTestCases_LastFetchedMode_AtStartup()
  {
    foreach (var (gp0Settings, _, pinMode, _, _) in YieldTestCases_Gp0Settings()) {
      yield return new object?[] { gp0Settings, pinMode };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_LastFetchedMode_AtStartup))]
  public void LastFetchedMode_AtStartup(byte gp0Settings, PinMode? expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: gp0Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    if (expected.HasValue)
      Assert.That(mcp2221A.GpPin0.LastFetchedMode, Is.EqualTo(expected.Value));
    else
      Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentFunction_AtStartup()
  {
    foreach (var (gp0Settings, _, _, function, _) in YieldTestCases_Gp0Settings()) {
      yield return new object[] { gp0Settings, function };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentFunction_AtStartup))]
  public void CurrentFunction_AtStartup(byte gp0Settings, GpFunction expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: gp0Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(expected));
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentDesignation_AtStartup()
  {
    foreach (var (gp0Settings, _, _, _, designation) in YieldTestCases_Gp0Settings()) {
      yield return new object[] { gp0Settings, designation };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentDesignation_AtStartup))]
  public void CurrentDesignation_AtStartup(byte gp0Settings, string expected)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: gp0Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(mcp2221A.GpPin0.CurrentDesignation, Is.EqualTo(expected));
  }
}
