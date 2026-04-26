// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines an interface for controlling the Digital-to-Analog
/// Converter (DAC) pins of the MCP2221/MCP2221A.
/// </summary>
public interface IDacController {
  /// <summary>
  /// Gets the current voltage reference source configured for the DAC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the DAC module
  /// of the MCP2221A. Changing the reference source on one GP pin will
  /// affect all other GP pins configured as DAC outputs, but does not affect
  /// the ADC module configuration.
  /// </remarks>
  VoltageReferenceSource CurrentDacReferenceSource { get; }

  /// <summary>
  /// Gets the 5-bit raw output value (0-31) that was last written to the
  /// DAC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the DAC module
  /// of the MCP2221A. If no write operation has been performed yet, this
  /// property returns the value currently held by the controller (e.g.,
  /// the default value from Flash settings).
  /// </remarks>
  int LastWriteAnalogRawValue { get; }

  /// <summary>
  /// Configures the GP pin to function as an Digital-to-Analog Converter (DAC)
  /// output and sets the voltage reference source for the DAC module.
  /// </summary>
  /// <param name="voltageReferenceSource">
  /// The <see cref="VoltageReferenceSource"/> to be used for the DAC.
  /// If <see langword="null"/>, the current voltage reference source of
  /// the DAC module is maintained.
  /// </param>
  /// <param name="initialOutputValue">
  /// The initial 5-bit raw analog value (0-31) to be set for the DAC output.
  /// If <see langword="null"/>, the current DAC output value is maintained.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="initialOutputValue"/> is negative, or greater than 31 (the maximum
  /// value for a 5-bit DAC).
  /// </exception>
  /// <remarks>
  /// <para>
  /// Note that the <paramref name="voltageReferenceSource"/> is a global setting
  /// for the entire DAC module. Updating this value through one GP pin will
  /// simultaneously change the reference source for all other DAC-enabled pins.
  /// </para>
  /// <para>
  /// Unlike ADC configuration, this method allows specifying an initial output
  /// value to ensure a predictable voltage level immediately upon pin function
  /// switching.
  /// </para>
  /// </remarks>
  /// <seealso cref="CurrentDacReferenceSource"/>
  /// <seealso cref="GpFunction.Dac"/>
  void ConfigureAsDac(
    VoltageReferenceSource? voltageReferenceSource,
    int? initialOutputValue = null,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously configures the GP pin to function as an Digital-to-Analog
  /// Converter (DAC) output and sets the voltage reference source for the
  /// DAC module.
  /// </summary>
  /// <inheritdoc cref="ConfigureAsDac(VoltageReferenceSource?, int?, CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ConfigureAsDacAsync(
    VoltageReferenceSource? voltageReferenceSource,
    int? initialOutputValue = null,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Writes the 5-bit raw analog value (0-31) to the DAC module.
  /// </summary>
  /// <param name="value">
  /// The 5-bit raw analog value (0-31) to be set for the DAC output.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="value"/> is negative, or greater than 31 (the maximum
  /// value for a 5-bit DAC).
  /// </exception>
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Dac"/>.
  /// </exception>
  /// <remarks>
  /// Note that the analog output value is a global setting for the DAC module.
  /// Updating this value through one GP pin will simultaneously change
  /// the output value for all other DAC-enabled pins.
  /// </remarks>
  void WriteAnalogRaw(
    int value,
    CancellationToken cancellationToken = default
  );

  /// <inheritdoc cref="WriteAnalogRaw(int, CancellationToken)"/>
  /// <summary>
  /// Asynchronously writes the 5-bit raw analog value (0-31) to the DAC module.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask WriteAnalogRawAsync(
    int value,
    CancellationToken cancellationToken = default
  );
}
