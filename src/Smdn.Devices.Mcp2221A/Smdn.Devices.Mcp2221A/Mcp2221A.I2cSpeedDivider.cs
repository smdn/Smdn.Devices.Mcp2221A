// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221A {
#pragma warning restore IDE0040
  /// <summary>
  /// Internal oscillator frequency of the MCP2221/MCP2221A chip (12 MHz).
  /// </summary>
  // [MCP2221A] 1.12 Internal Oscillator
  // [MCP2221A] 3.1.1 Status/Set Parameters TABLE 3-1 Note 1
  private const int InternalOscillatorFrequencyInHz = 12_000_000;

  /// <summary>
  /// Tries to calculate the I2C speed divider for the MCP2221A.
  /// </summary>
  /// <param name="i2cBusSpeedInKbps">The target I2C bus speed in kbps (e.g., 100 for Standard mode, 400 for Fast mode).</param>
  /// <param name="i2cSpeedDivider">When this method returns, contains the 8-bit divider value if the calculation succeeds.</param>
  /// <returns><c>true</c> if the speed is within the supported range and the divider was calculated successfully; otherwise, <c>false</c>.</returns>
  /// <remarks>
  /// Based on [MCP2221A] Datasheet Section 3.1.1 "Status/Set Parameters", TABLE 3-1, Note 1.
  /// <code>
  /// Formula: Divider = (12 MHz / I2C CLOCK RATE) - 2
  /// </code>
  /// </remarks>
  public static bool TryCalculateMcp2221AI2cSpeedDivider(int i2cBusSpeedInKbps, out byte i2cSpeedDivider)
  {
    i2cSpeedDivider = default;

    if (i2cBusSpeedInKbps <= 0)
      return false;

    var i2cBusSpeedInHz = i2cBusSpeedInKbps * 1000L;

    // [MCP2221A] 3.1.1 Status/Set Parameters TABLE 3-1 Note 1
    // Divider = (12 MHz/I2C CLOCK RATE) - 2

    // Perform ceiling division to ensure the calculated divider is rounded up.
    // This prevents the actual I2C bus speed from exceeding the target rate,
    // maintaining a safety margin for reliable communication.
    var quotient = (InternalOscillatorFrequencyInHz + i2cBusSpeedInHz - 1) / i2cBusSpeedInHz;
    var divider = quotient - 2;

    if (divider < 0)
      return false;
    if (divider > byte.MaxValue)
      return false;

    i2cSpeedDivider = (byte)divider;

    return true;
  }

  /// <summary>
  /// Tries to calculate the I2C speed divider for the MCP2221.
  /// </summary>
  /// <param name="i2cBusSpeedInKbps">The target I2C bus speed in kbps.</param>
  /// <param name="i2cSpeedDivider">When this method returns, contains the 8-bit divider value if the calculation succeeds.</param>
  /// <returns><c>true</c> if the speed is within the supported range and the divider was calculated successfully; otherwise, <c>false</c>.</returns>
  /// <remarks>
  /// Based on the legacy MCP2221 communication protocol.
  /// <code>
  /// Formula: Divider = ((12 MHz / I2C CLOCK RATE) - 20) / 4
  /// </code>
  /// </remarks>
  public static bool TryCalculateMcp2221I2cSpeedDivider(int i2cBusSpeedInKbps, out byte i2cSpeedDivider)
  {
    i2cSpeedDivider = default;

    if (i2cBusSpeedInKbps <= 0)
      return false;

    var i2cBusSpeedInHz = i2cBusSpeedInKbps * 1000L;

    // Divider = ((12 MHz/I2C CLOCK RATE) - 20) / 4
    // Simplified as: (3 MHz / I2C CLOCK RATE) - 5
    const long FrequencyDividedBy4 = InternalOscillatorFrequencyInHz / 4L;
    // Perform ceiling division to ensure the calculated divider is rounded up.
    // This prevents the actual I2C bus speed from exceeding the target rate,
    // maintaining a safety margin for reliable communication.
    var divider = ((FrequencyDividedBy4 + i2cBusSpeedInHz - 1) / i2cBusSpeedInHz) - 5;

    if (divider < 0)
      return false;
    if (divider > byte.MaxValue)
      return false;

    i2cSpeedDivider = (byte)divider;

    return true;
  }

  internal byte CalculateBusSpeedDividerOrThrow(
    int transmissionSpeed,
    string paramName
  )
  {
    byte busSpeedDivider;

    var isValid = IsMcp2221A
      ? TryCalculateMcp2221AI2cSpeedDivider(transmissionSpeed, out busSpeedDivider)
      : TryCalculateMcp2221I2cSpeedDivider(transmissionSpeed, out busSpeedDivider);

    if (!isValid) {
      throw new ArgumentOutOfRangeException(
        message: $"The specified bus transmission speed '{transmissionSpeed} kbps' is out of range for this device.",
        actualValue: transmissionSpeed,
        paramName: paramName
      );
    }

    return busSpeedDivider;
  }
}
