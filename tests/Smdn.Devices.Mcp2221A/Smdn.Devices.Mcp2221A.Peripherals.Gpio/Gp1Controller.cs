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
}
