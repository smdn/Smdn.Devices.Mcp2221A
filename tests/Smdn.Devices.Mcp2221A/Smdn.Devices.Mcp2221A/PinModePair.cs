// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class PinModePairTests {
  private static System.Collections.IEnumerable YieldTestCases()
  {
    yield return new object[] { 0, PinMode.Output };
    yield return new object[] { 1, PinMode.Input };
    yield return new object[] { 2, PinMode.InputPullDown };
    yield return new object[] { 3, PinMode.InputPullUp };
    yield return new object[] { -1, (PinMode)(-1) };
  }

  [TestCaseSource(nameof(YieldTestCases))]
  public void Construct(int pinNumber, PinMode pinMode)
  {
    var pair = new PinModePair(pinNumber, pinMode);

    Assert.That(pair.PinNumber, Is.EqualTo(pinNumber));
    Assert.That(pair.PinMode, Is.EqualTo(pinMode));
  }

  [TestCaseSource(nameof(YieldTestCases))]
  public void Deconstruct(int pinNumber, PinMode pinMode)
  {
    var pair = new PinModePair(pinNumber, pinMode);

    var (number, mode) = pair;

    Assert.That(number, Is.EqualTo(pinNumber));
    Assert.That(mode, Is.EqualTo(pinMode));
  }

  [TestCaseSource(nameof(YieldTestCases))]
  public void Equals_OfPinModePair(int pinNumber, PinMode pinMode)
  {
    var pair = new PinModePair(pinNumber, pinMode);

    Assert.That(pair, Is.EqualTo(new PinModePair(pinNumber, pinMode)));
  }
}
