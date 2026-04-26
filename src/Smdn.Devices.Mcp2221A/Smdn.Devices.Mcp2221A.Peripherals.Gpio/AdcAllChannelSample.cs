// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Represents a set of raw 10-bit ADC samples for all channels
/// (ADC1, ADC2, and ADC3) of the MCP2221/MCP2221A.
/// </summary>
/// <remarks>
/// <para>
/// This structure holds the snapshot of all ADC inputs retrieved
/// from the device at a single point in time.
/// </para>
/// <para>
/// Note that if a specific GP pin is not configured as an ADC input,
/// the value of the corresponding field is undefined.
/// </para>
/// </remarks>
[CLSCompliant(false)]
public readonly record struct AdcAllChannelSample {
  /// <summary>
  /// The number of quantization steps for a 10-bit ADC (2^10).
  /// </summary>
  /// <remarks>
  /// In ADC conversion formulas, the denominator should be 2^n (1024) rather
  /// than 2^n - 1 (1023). This is because each digital value represents a
  /// voltage range (quantization bucket) of width Vref / 2^n, and the
  /// full-scale range is divided into 2^n equal intervals.
  /// </remarks>
  private const double AdcResolution = 1024.0;

  /// <summary>
  /// Gets the 10-bit raw analog value from ADC1 (GP1).
  /// </summary>
  public ushort Adc1 { get; init; }

  /// <summary>
  /// Gets the 10-bit raw analog value from ADC2 (GP2).
  /// </summary>
  public ushort Adc2 { get; init; }

  /// <summary>
  /// Gets the 10-bit raw analog value from ADC3 (GP3).
  /// </summary>
  public ushort Adc3 { get; init; }

  /// <summary>
  /// Initializes a new instance of the <see cref="AdcAllChannelSample"/> struct
  /// with ADC raw values for all channels.
  /// </summary>
  /// <param name="adc1">10-bit raw analog value from ADC1 (GP1).</param>
  /// <param name="adc2">10-bit raw analog value from ADC2 (GP2).</param>
  /// <param name="adc3">10-bit raw analog value from ADC3 (GP3).</param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="adc1"/>, <paramref name="adc2"/>, or <paramref name="adc3"/>
  /// is greater than 1023 (the maximum value for a 10-bit ADC).
  /// </exception>
  public AdcAllChannelSample(ushort adc1, ushort adc2, ushort adc3)
  {
    Adc1 = Mcp2221AGpioDriver.ThrowIfAdcRawValueOutOfRange(adc1, nameof(adc1));
    Adc2 = Mcp2221AGpioDriver.ThrowIfAdcRawValueOutOfRange(adc2, nameof(adc2));
    Adc3 = Mcp2221AGpioDriver.ThrowIfAdcRawValueOutOfRange(adc3, nameof(adc3));
  }

  /// <summary>
  /// Returns the raw analog values for all channels converted to <see cref="int"/>.
  /// </summary>
  /// <returns>
  /// A tuple containing the 10-bit raw analog values as <see cref="int"/>
  /// for (ADC1, ADC2, ADC3).
  /// </returns>
  /// <remarks>
  /// This method is provided for convenience when performing arithmetic operations
  /// or interfacing with APIs that require <see cref="int"/> instead of <see cref="ushort"/>.
  /// </remarks>
  public (int Adc1, int Adc2, int Adc3) AsInt32()
    => (Adc1, Adc2, Adc3);

  /// <summary>
  /// Converts the raw analog values of all channels to voltage values [V]
  /// based on the specified reference source.
  /// </summary>
  /// <param name="adcVoltageReference">
  /// The <see cref="VoltageReferenceSource"/> currently configured for the ADC module.
  /// </param>
  /// <returns>
  /// A tuple containing the converted voltage values [V] for (Adc1, Adc2, Adc3).
  /// </returns>
  /// <remarks>
  /// <para>
  /// If <paramref name="adcVoltageReference"/> is <see cref="VoltageReferenceSource.VrmOff"/>,
  /// this method always returns 0.0 for all channels regardless of the raw values.
  /// </para>
  /// <para>
  /// Note that this method does not account for cases where the actual VDD supplied
  /// to the MCP2221A is lower than the reference voltage specified by <paramref name="adcVoltageReference"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <paramref name="adcVoltageReference"/> is <see cref="VoltageReferenceSource.Vdd"/>,
  /// as the Vdd voltage value is required for calculation but not provided here.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="adcVoltageReference"/> is an undefined or unsupported
  /// <see cref="VoltageReferenceSource"/> value.
  /// </exception>
  public
  (double Adc1, double Adc2, double Adc3)
  AsVoltage(VoltageReferenceSource adcVoltageReference)
    => adcVoltageReference switch {
      VoltageReferenceSource.VrmOff => default, // (0.0, 0.0, 0.0)
      VoltageReferenceSource.Vrm1024 => AsVoltage(referenceVoltage: 1.024),
      VoltageReferenceSource.Vrm2048 => AsVoltage(referenceVoltage: 2.048),
      VoltageReferenceSource.Vrm4096 => AsVoltage(referenceVoltage: 4.096),

      VoltageReferenceSource.Vdd
        => throw new InvalidOperationException(
          $"{nameof(VoltageReferenceSource)}.{nameof(VoltageReferenceSource.Vdd)} is not supported in this method. Use a method that accepts a specific Vdd voltage value instead."
        ),

      var invalid
        => throw new ArgumentException(
          message: $"Undefined {nameof(VoltageReferenceSource)} value: {invalid}",
          paramName: nameof(adcVoltageReference)
        ),
    };

  /// <summary>
  /// Converts the raw analog values to voltage values [V]
  /// based on the specified reference voltage value.
  /// </summary>
  /// <param name="referenceVoltage">
  /// The reference voltage [V] used for ADC conversion (e.g., VDD or VRM voltage).
  /// </param>
  /// <returns>
  /// A tuple containing the converted voltage values [V] for (Adc1, Adc2, Adc3).
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// Thrown when <paramref name="referenceVoltage"/> is negative.
  /// </exception>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="referenceVoltage"/> is <see cref="double.NaN"/>,
  /// <see cref="double.PositiveInfinity"/> or <see cref="double.NegativeInfinity"/>.
  /// </exception>
  public
  (double Adc1, double Adc2, double Adc3)
  AsVoltage(double referenceVoltage)
  {
    if (double.IsNaN(referenceVoltage) || double.IsInfinity(referenceVoltage))
      throw new ArgumentException(message: "Reference voltage must be a finite number.", paramName: nameof(referenceVoltage));

    if (referenceVoltage < 0.0)
      throw new ArgumentOutOfRangeException(message: "Reference voltage cannot be negative.", paramName: nameof(referenceVoltage));

    if (referenceVoltage == 0.0)
      return default; // (0.0, 0.0, 0.0)

    // Calculation: (Reference voltage [V] * RawValue) / Resolution
    return (
      referenceVoltage * Adc1 / AdcResolution,
      referenceVoltage * Adc2 / AdcResolution,
      referenceVoltage * Adc3 / AdcResolution
    );
  }
}
