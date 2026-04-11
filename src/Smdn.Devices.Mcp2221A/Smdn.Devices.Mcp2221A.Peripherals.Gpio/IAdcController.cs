// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines an interface for controlling the Analog-to-Digital
/// Converter (ADC) pins of the MCP2221/MCP2221A.
/// </summary>
public interface IAdcController {
  /// <summary>
  /// Gets the current voltage reference source configured for the ADC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the ADC module
  /// of the MCP2221A. Changing the reference source on one GP pin will
  /// affect all other GP pins configured as ADC inputs.
  /// </remarks>
  VoltageReferenceSource CurrentAdcReferenceSource { get; }

  /// <summary>
  /// Gets the 10-bit raw analog value (0-1023) retrieved during the
  /// last read operation.
  /// </summary>
  /// <remarks>
  /// This property returns the cached value from the last call to
  /// <see cref="ReadAnalogRaw(CancellationToken)"/> or <see cref="ReadAnalogRawAsync(CancellationToken)"/>.
  /// If no read operation has been performed yet, this property returns 0.
  /// </remarks>
  /// <seealso cref="GpFunction.Adc"/>
  /// <seealso cref="ReadAnalogRaw(CancellationToken)"/>
  /// <seealso cref="ReadAnalogRawAsync(CancellationToken)"/>
  int LastReadAnalogRawValue { get; }

  /// <summary>
  /// Configures the GP pin to function as an Analog-to-Digital Converter (ADC) input
  /// and sets the voltage reference source for the analog module.
  /// </summary>
  /// <param name="voltageReferenceSource">
  /// The <see cref="VoltageReferenceSource"/> to be used for the ADC.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <remarks>
  /// Note that the <paramref name="voltageReferenceSource"/> is a global setting
  /// for the entire analog module. Updating this value through one GP pin will
  /// simultaneously change the reference source for all other ADC-enabled pins.
  /// </remarks>
  /// <seealso cref="CurrentAdcReferenceSource"/>
  /// <seealso cref="GpFunction.Adc"/>
  void ConfigureAsAdc(
    VoltageReferenceSource voltageReferenceSource,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously configures the GP pin to function as an Analog-to-Digital
  /// Converter (ADC) input and sets the voltage reference source for the analog module.
  /// </summary>
  /// <inheritdoc cref="ConfigureAsAdc(VoltageReferenceSource, CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ConfigureAsAdcAsync(
    VoltageReferenceSource voltageReferenceSource,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Reads the current 10-bit raw analog value (0-1023) from the GP pin.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>The 10-bit raw analog value (0-1023).</returns>
  int ReadAnalogRaw(
    CancellationToken cancellationToken = default
  );

  /// <inheritdoc cref="ReadAnalogRaw(CancellationToken)"/>
  /// <summary>
  /// Asynchronously reads the current 10-bit raw analog value (0-1023) from the GP pin.
  /// </summary>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation,
  /// containing the 10-bit raw analog value (0-1023).
  /// </returns>
  ValueTask<int> ReadAnalogRawAsync(
    CancellationToken cancellationToken = default
  );
}
