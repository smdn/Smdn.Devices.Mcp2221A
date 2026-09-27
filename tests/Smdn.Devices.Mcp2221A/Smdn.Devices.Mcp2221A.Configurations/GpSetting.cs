// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

[TestFixture]
public class GpSettingTests {
  [TestCase(0, 0b_000_1_0_010, true, PinMode.Output, GpFunction.LedOutput)] // Alternate function 0 (LEDURX); GP0 factory default
  [TestCase(1, 0b_000_1_0_011, true, PinMode.Output, GpFunction.LedOutput)] // Alternate function 1 (LEDUTX); GP1 factory default
  [TestCase(2, 0b_000_1_0_001, true, PinMode.Output, GpFunction.UsbConfigureStatus)] // Dedicated function operation (USBCFG); GP2 factory default
  [TestCase(3, 0b_000_1_0_001, true, PinMode.Output, GpFunction.LedOutput)] // Dedicated function operation (LEDI2C); GP3 factory default
  [TestCase(0, 0b_111_0_1_000, false, PinMode.Input, GpFunction.Gpio)] // GPIO0
  public void Deconstruct_ToFunctionModeValue(
    int gp,
    byte gpSetting,
    bool expectedValue, // true for HIGH, false for LOW
    PinMode expectedMode,
    GpFunction expectedFunction
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gp == 0 ? gpSetting : default,
        gp1Settings: gp == 1 ? gpSetting : default,
        gp2Settings: gp == 2 ? gpSetting : default,
        gp3Settings: gp == 3 ? gpSetting : default
      )
    );

    var (function, mode, value) = mcp2221A.Flash.GpPins[gp];

    Assert.That(function, Is.EqualTo(expectedFunction));
    Assert.That(mode, Is.EqualTo(expectedMode));
    Assert.That(value, Is.EqualTo((PinValue)expectedValue));
  }

  [TestCase(0, 0b_000_1_0_010, true, PinMode.Output, GpFunction.LedOutput)] // Alternate function 0 (LEDURX); GP0 factory default
  [TestCase(1, 0b_000_1_0_011, true, PinMode.Output, GpFunction.LedOutput)] // Alternate function 1 (LEDUTX); GP1 factory default
  [TestCase(2, 0b_000_1_0_001, true, PinMode.Output, GpFunction.UsbConfigureStatus)] // Dedicated function operation (USBCFG); GP2 factory default
  [TestCase(3, 0b_000_1_0_001, true, PinMode.Output, GpFunction.LedOutput)] // Dedicated function operation (LEDI2C); GP3 factory default
  [TestCase(0, 0b_111_0_1_000, false, PinMode.Input, GpFunction.Gpio)] // GPIO0
  public void Deconstruct_ToIndexFunctionModeValue(
    int gp,
    byte gpSetting,
    bool expectedValue, // true for HIGH, false for LOW
    PinMode expectedMode,
    GpFunction expectedFunction
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gp == 0 ? gpSetting : default,
        gp1Settings: gp == 1 ? gpSetting : default,
        gp2Settings: gp == 2 ? gpSetting : default,
        gp3Settings: gp == 3 ? gpSetting : default
      )
    );

    var (index, function, mode, value) = mcp2221A.Flash.GpPins[gp];

    Assert.That(index, Is.EqualTo(gp));
    Assert.That(function, Is.EqualTo(expectedFunction));
    Assert.That(mode, Is.EqualTo(expectedMode));
    Assert.That(value, Is.EqualTo((PinValue)expectedValue));
  }

  [TestCase(0, 0b_000_1_0_010, true, PinMode.Output, GpFunction.LedOutput, 0b010)] // Alternate function 0 (LEDURX); GP0 factory default
  [TestCase(1, 0b_000_1_0_011, true, PinMode.Output, GpFunction.LedOutput, 0b011)] // Alternate function 1 (LEDUTX); GP1 factory default
  [TestCase(2, 0b_000_1_0_001, true, PinMode.Output, GpFunction.UsbConfigureStatus, 0b001)] // Dedicated function operation (USBCFG); GP2 factory default
  [TestCase(3, 0b_000_1_0_001, true, PinMode.Output, GpFunction.LedOutput, 0b001)] // Dedicated function operation (LEDI2C); GP3 factory default
  [TestCase(0, 0b_111_0_1_000, false, PinMode.Input, GpFunction.Gpio, 0b000)] // GPIO0
  public void Deconstruct_ToIndexDesignationFunctionModeValue(
    int gp,
    byte gpSetting,
    bool expectedValue, // true for HIGH, false for LOW
    PinMode expectedMode,
    GpFunction expectedFunction,
    byte expectedDesignation
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gp == 0 ? gpSetting : default,
        gp1Settings: gp == 1 ? gpSetting : default,
        gp2Settings: gp == 2 ? gpSetting : default,
        gp3Settings: gp == 3 ? gpSetting : default
      )
    );

    var (index, designation, function, mode, value) = mcp2221A.Flash.GpPins[gp];

    Assert.That(index, Is.EqualTo(gp));
    Assert.That(designation, Is.EqualTo(expectedDesignation));
    Assert.That(function, Is.EqualTo(expectedFunction));
    Assert.That(mode, Is.EqualTo(expectedMode));
    Assert.That(value, Is.EqualTo((PinValue)expectedValue));
  }

  [Test]
  public void Equals_OfGpSetting_Default()
  {
    var defaultGpSetting = default(GpSetting);

    Assert.That(
      default(GpSetting).Equals(defaultGpSetting),
      Is.True
    );
  }

  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_0_010, true)] // equals
  [TestCase(0, 0b_000_1_0_010, 1, 0b_000_1_0_010, false)] // index differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_0_000, false)] // function differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_1_000, false)] // mode differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_0_0_000, false)] // value differs
  [TestCase(0, 0b_000_1_0_010 /* factory default */, 0, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(0, 0b_000_1_0_010 /* factory default */, 0, 0b_000_1_0_011 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(1, 0b_000_1_0_011 /* factory default */, 1, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(1, 0b_000_1_0_011 /* factory default */, 1, 0b_000_1_0_101 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(2, 0b_000_1_0_001 /* factory default */, 2, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(2, 0b_000_1_0_001 /* factory default */, 2, 0b_000_1_0_100 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(3, 0b_000_1_0_001 /* factory default */, 3, 0b_000_1_0_100 /* reserved */, true)] // designation differs but fallback function equals
  public void Equals_OfGpSetting(
    int gpForThis,
    byte gpSettingForThis,
    int gpForOther,
    byte gpSettingForOther,
    bool expected
  )
  {
    using var mcp2221AForThis = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gpForThis == 0 ? gpSettingForThis : default,
        gp1Settings: gpForThis == 1 ? gpSettingForThis : default,
        gp2Settings: gpForThis == 2 ? gpSettingForThis : default,
        gp3Settings: gpForThis == 3 ? gpSettingForThis : default
      )
    );
    using var mcp2221AForOther = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gpForOther == 0 ? gpSettingForOther : default,
        gp1Settings: gpForOther == 1 ? gpSettingForOther : default,
        gp2Settings: gpForOther == 2 ? gpSettingForOther : default,
        gp3Settings: gpForOther == 3 ? gpSettingForOther : default
      )
    );

    var thisGpSetting = mcp2221AForThis.Flash.GpPins[gpForThis];
    var otherGpSetting = mcp2221AForOther.Flash.GpPins[gpForOther];

    Assert.That(
      thisGpSetting.Equals(otherGpSetting),
      Is.EqualTo(expected),
      $"this: {thisGpSetting}, other: {otherGpSetting}"
    );
  }

  [Test]
  public void Equals_OfObject()
  {
    var gpSetting = default(GpSetting);
    object otherGpSetting = default(GpSetting);

    Assert.That(gpSetting.Equals(null), Is.False);
    Assert.That(gpSetting.Equals(0), Is.False);
    Assert.That(gpSetting.Equals(otherGpSetting), Is.True);
  }


  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_0_010, true)] // equals
  [TestCase(0, 0b_000_1_0_010, 1, 0b_000_1_0_010, false)] // index differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_0_000, false)] // function differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_1_1_000, false)] // mode differs
  [TestCase(0, 0b_000_1_0_010, 0, 0b_000_0_0_000, false)] // value differs
  [TestCase(0, 0b_000_1_0_010 /* factory default */, 0, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(0, 0b_000_1_0_010 /* factory default */, 0, 0b_000_1_0_011 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(1, 0b_000_1_0_011 /* factory default */, 1, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(1, 0b_000_1_0_011 /* factory default */, 1, 0b_000_1_0_101 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(2, 0b_000_1_0_001 /* factory default */, 2, 0b_000_1_0_111 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(2, 0b_000_1_0_001 /* factory default */, 2, 0b_000_1_0_100 /* reserved */, true)] // designation differs but fallback function equals
  [TestCase(3, 0b_000_1_0_001 /* factory default */, 3, 0b_000_1_0_100 /* reserved */, true)] // designation differs but fallback function equals
  public void OpEqualityOpInEquality(
    int gpForThis,
    byte gpSettingForThis,
    int gpForOther,
    byte gpSettingForOther,
    bool expected
  )
  {
    using var mcp2221AForThis = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gpForThis == 0 ? gpSettingForThis : default,
        gp1Settings: gpForThis == 1 ? gpSettingForThis : default,
        gp2Settings: gpForThis == 2 ? gpSettingForThis : default,
        gp3Settings: gpForThis == 3 ? gpSettingForThis : default
      )
    );
    using var mcp2221AForOther = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: gpForOther == 0 ? gpSettingForOther : default,
        gp1Settings: gpForOther == 1 ? gpSettingForOther : default,
        gp2Settings: gpForOther == 2 ? gpSettingForOther : default,
        gp3Settings: gpForOther == 3 ? gpSettingForOther : default
      )
    );

    var thisGpSetting = mcp2221AForThis.Flash.GpPins[gpForThis];
    var otherGpSetting = mcp2221AForOther.Flash.GpPins[gpForOther];

    Assert.That(
      thisGpSetting == otherGpSetting,
      Is.EqualTo(expected),
      $"op_Equality this: {thisGpSetting}, other: {otherGpSetting}"
    );
    Assert.That(
      thisGpSetting != otherGpSetting,
      Is.EqualTo(!expected),
      $"op_Inequality this: {thisGpSetting}, other: {otherGpSetting}"
    );
  }
}
