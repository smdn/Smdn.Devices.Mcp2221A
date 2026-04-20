// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines an interface for controlling the clock output pin
/// (GP1) of the MCP2221/MCP2221A.
/// </summary>
public interface IClockOutputController {
  /// <summary>
  /// Gets the current clock frequency configured for the clock output module.
  /// </summary>
  /// <remarks>
  /// This property represents the current configuration held in the device's
  /// SRAM. This configuration is global to the clock output module; changing the
  /// frequency will affect the clock signal whenever the GP1 pin is configured
  /// as a clock output.
  /// </remarks>
  /// <seealso cref="CurrentClockOutputDutyCycle"/>
  ClockOutputFrequency CurrentClockOutputFrequency { get; }

  /// <summary>
  /// Gets the current duty cycle configured for the clock output module.
  /// </summary>
  /// <remarks>
  /// This property represents the current configuration held in the device's
  /// SRAM. This configuration is global to the clock output module; changing the
  /// duty cycle will affect the clock signal whenever the GP1 pin is configured
  /// as a clock output.
  /// </remarks>
  /// <seealso cref="CurrentClockOutputFrequency"/>
  ClockOutputDutyCycle CurrentClockOutputDutyCycle { get; }

  /// <summary>
  /// Configures the GP1 pin to function as the clock output and updates
  /// the configuration of the clock output module.
  /// </summary>
  /// <param name="frequency">
  /// The clock frequency to be set.
  /// If <see langword="null"/>, the current frequency configuration
  /// of the clock output module is maintained.
  /// </param>
  /// <param name="dutyCycle">
  /// The duty cycle to be set.
  /// If <see langword="null"/>, the current duty cycle configuration
  /// of the clock output module is maintained.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <remarks>
  /// <para>
  /// This method switches the GP1 pin function to <see cref="GpFunction.ClockOutput"/>.
  /// If both <paramref name="frequency"/> and <paramref name="dutyCycle"/> are
  /// <see langword="null"/>, the pin is configured as a clock output using the
  /// current configuration of the clock output module (which may be the factory
  /// defaults or values loaded from Flash memory at power-up).
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="frequency"/> is set to <see cref="ClockOutputFrequency.Reserved"/>.
  /// This exception is also thrown when <paramref name="frequency"/> or
  /// <paramref name="dutyCycle"/> is set to a value that is not defined in
  /// their respective enumerations.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.ClockOutput"/>
  /// <seealso cref="CurrentClockOutputFrequency"/>
  /// <seealso cref="CurrentClockOutputDutyCycle"/>
  void ConfigureAsClockOutput(
    ClockOutputFrequency? frequency = null,
    ClockOutputDutyCycle? dutyCycle = null,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously Configures the GP1 pin to function as the clock output
  /// and sets its frequency and duty cycle.
  /// </summary>
  /// <inheritdoc cref="ConfigureAsClockOutput(ClockOutputFrequency?, ClockOutputDutyCycle?, CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ConfigureAsClockOutputAsync(
    ClockOutputFrequency? frequency = null,
    ClockOutputDutyCycle? dutyCycle = null,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Suspends the clock output by reconfiguring the GP1 pin as a
  /// GPIO output set to low.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <remarks>
  /// Since setting the duty cycle to 0% may not completely stop the clock signal
  /// on the MCP2221A, this method provides a way to physically stop the signal
  /// by temporarily changing the pin function to GPIO.
  /// </remarks>
  /// <seealso cref="IGpioController.ConfigureAsGpio"/>
  void SuspendClockOutput(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously suspends the clock output by reconfiguring the
  /// GP1 pin as a GPIO output set to low.
  /// </summary>
  /// <inheritdoc cref="SuspendClockOutput(CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <seealso cref="IGpioController.ConfigureAsGpioAsync"/>
  ValueTask SuspendClockOutputAsync(
    CancellationToken cancellationToken = default
  );
}
