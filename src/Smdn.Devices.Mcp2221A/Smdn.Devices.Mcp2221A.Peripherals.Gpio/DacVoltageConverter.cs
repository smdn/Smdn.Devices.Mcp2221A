// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.ComponentModel;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal static class DacVoltageConverter {
  /// <remarks>
  /// <para>
  /// If <paramref name="dacVoltageReference"/> is <see cref="VoltageReferenceSource.VrmOff"/>,
  /// this method always returns <c>0</c>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <paramref name="dacVoltageReference"/> is <see cref="VoltageReferenceSource.Vdd"/>.
  /// </exception>
  /// <exception cref="InvalidEnumArgumentException">
  /// Thrown when <paramref name="dacVoltageReference"/> is an undefined or unsupported
  /// <see cref="VoltageReferenceSource"/> value.
  /// </exception>
  internal static int ToOutputValue(double voltage, VoltageReferenceSource dacVoltageReference)
    => dacVoltageReference switch {
      VoltageReferenceSource.VrmOff => 0,
      VoltageReferenceSource.Vrm1024 => ToOutputValue(voltage: voltage, referenceVoltage: 1.024),
      VoltageReferenceSource.Vrm2048 => ToOutputValue(voltage: voltage, referenceVoltage: 2.048),
      VoltageReferenceSource.Vrm4096 => ToOutputValue(voltage: voltage, referenceVoltage: 4.096),

      VoltageReferenceSource.Vdd
        => throw new InvalidOperationException(
          $"{nameof(VoltageReferenceSource)}.{nameof(VoltageReferenceSource.Vdd)} is not supported in this method. Use a method that accepts a specific Vdd voltage value instead."
        ),

      var invalid
        => throw new InvalidEnumArgumentException(
          argumentName: nameof(dacVoltageReference),
          invalidValue: (int)dacVoltageReference,
          enumClass: typeof(VoltageReferenceSource)
        ),
    };

  /// <remarks>
  /// <para>
  /// This method calculates the 5-bit raw value (0-31) using the following formula,
  /// rounding the result to the nearest integer (away from zero):
  /// <c>
  ///   OutputValue = <see cref="Math.Round(double, MidpointRounding)"/>(31 * <paramref name="voltage"/> / <paramref name="referenceVoltage"/>, <see cref="MidpointRounding.AwayFromZero"/>);
  /// </c>
  /// The resulting value is clamped to the range 0 to 31 and applied to the DAC module.
  /// </para>
  /// </remarks>
  internal static int ToOutputValue(double voltage, double referenceVoltage)
  {
    if (double.IsNaN(voltage) || double.IsInfinity(voltage))
      throw new ArgumentException(message: "DAC output voltage must be a finite number.", paramName: nameof(voltage));
    if (double.IsNaN(referenceVoltage) || double.IsInfinity(referenceVoltage))
      throw new ArgumentException(message: "Reference voltage must be a finite number.", paramName: nameof(referenceVoltage));

    if (voltage < 0.0)
      throw new ArgumentOutOfRangeException(message: "DAC output voltage cannot be negative.", actualValue: voltage, paramName: nameof(voltage));
    if (referenceVoltage < 0.0)
      throw new ArgumentOutOfRangeException(message: "Reference voltage cannot be negative.", actualValue: referenceVoltage, paramName: nameof(referenceVoltage));

    if (referenceVoltage < voltage)
      throw new ArgumentOutOfRangeException(message: $"DAC output voltage cannot exceed reference voltage ({referenceVoltage} V).", actualValue: voltage, paramName: nameof(voltage));

    if (referenceVoltage == 0.0)
      return 0;

    const int MaxOutputValue = 31;

    var rawValue = (int)Math.Round(MaxOutputValue * voltage / referenceVoltage, MidpointRounding.AwayFromZero);

    return MaxOutputValue < rawValue
      ? throw new ArgumentOutOfRangeException(nameof(voltage), voltage, $"DAC output voltage cannot exceed reference voltage ({referenceVoltage} V).")
      : rawValue;
  }
}
