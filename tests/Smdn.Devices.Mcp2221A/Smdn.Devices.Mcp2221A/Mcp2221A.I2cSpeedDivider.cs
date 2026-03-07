// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221ATests {
#pragma warning restore IDE0040
  [TestCase(4000, true, 1)] // High speed (Max ~4MHz)
  [TestCase(400, true, 28)] // Fast Mode
  [TestCase(333, true, 35)] // represents 324.32 kbps, meaning below the input value
  [TestCase(100, true, 118)] // Standard Mode
  [TestCase(50, true, 238)] // Valid lower speed
  [TestCase(47, true, 254)] // Near boundary (Min speed ~46.7kbps)
  [TestCase(0, false, 0)] // Zero division guard
  [TestCase(-1, false, 0)] // Negative value
  [TestCase(10, false, 0)] // Out of range (Too slow, divider > 255)
  [TestCase(int.MinValue, false, 0)] // Out of range
  [TestCase(int.MaxValue, false, 0)] // Out of range
  public void TryCalculateMcp2221AI2cSpeedDivider(int speedInKbps, bool expectedResult, byte expectedDivider)
  {
    var actualResult = Mcp2221A.TryCalculateMcp2221AI2cSpeedDivider(speedInKbps, out byte calculatedDivider);

    Assert.That(actualResult, Is.EqualTo(expectedResult));

    if (actualResult) {
      Assert.That(
        calculatedDivider,
        Is.EqualTo(expectedDivider),
        $"Divider for {speedInKbps}"
      );
    }
  }

  [TestCase(400, true, 3)] // Fast Mode
  [TestCase(333, true, 5)] // represents 300.00 kbps, meaning below the input value
  [TestCase(100, true, 25)] // Standard Mode
  [TestCase(20, true, 145)] // Valid lower speed
  [TestCase(12, true, 245)] // Near boundary (Min speed ~11.6kbps)
  [TestCase(0, false, 0)] // Zero division guard
  [TestCase(-1, false, 0)] // Negative value
  [TestCase(5, false, 0)] // Out of range (Too slow, divider > 255)
  [TestCase(1000, false, 0)] // Out of range (Too fast, divider < 0)
  [TestCase(int.MinValue, false, 0)] // Out of range
  [TestCase(int.MaxValue, false, 0)] // Out of range
  public void TryCalculateMcp2221I2cSpeedDivider(int speedInKbps, bool expectedResult, byte expectedDivider)
  {
    var actualResult = Mcp2221A.TryCalculateMcp2221I2cSpeedDivider(speedInKbps, out byte calculatedDivider);

    Assert.That(actualResult, Is.EqualTo(expectedResult));

    if (actualResult) {
      Assert.That(
        calculatedDivider,
        Is.EqualTo(expectedDivider),
        $"Divider for {speedInKbps}"
      );
    }
  }
}
