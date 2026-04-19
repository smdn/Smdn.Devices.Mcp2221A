// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides extension members for the <see cref="IClockOutputController"/> interface.
/// </summary>
public static class IClockOutputControllerExtensions {
#pragma warning disable IDE0051
  private static IClockOutputController ThrowIfReceiverIsNull(IClockOutputController controller, string paramName)
    => controller ?? throw new ArgumentNullException(paramName: paramName);
#pragma warning restore IDE0051

#pragma warning disable CA1034
  extension(IClockOutputController controller) {
#pragma warning restore CA1034
    /// <summary>
    /// Gets the current clock output frequency in Hertz (Hz).
    /// </summary>
    /// <value>
    /// The current frequency in Hz, or an exception if the frequency is reserved.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="IClockOutputController.CurrentClockOutputFrequency"/>
    /// is set to <see cref="ClockOutputFrequency.Reserved"/>.
    /// </exception>
    public int CurrentClockOutputFrequencyInHz
      => ThrowIfReceiverIsNull(controller, nameof(controller)).CurrentClockOutputFrequency switch {
        ClockOutputFrequency.Frequency24MHz => 24_000_000,
        ClockOutputFrequency.Frequency12MHz => 12_000_000,
        ClockOutputFrequency.Frequency6MHz => 6_000_000,
        ClockOutputFrequency.Frequency3MHz => 3_000_000,
        ClockOutputFrequency.Frequency1500kHz => 1_500_000,
        ClockOutputFrequency.Frequency750kHz => 750_000,
        ClockOutputFrequency.Frequency375kHz => 375_000,

        ClockOutputFrequency.Reserved => throw new InvalidOperationException(
          $"The current clock output frequency is set to a '{nameof(ClockOutputFrequency.Reserved)}' value, which cannot be represented as a numeric Hz value."
        ),

        var invalid => throw new InvalidOperationException(
          $"The device returned an undefined clock frequency value (0x{invalid:X}), which cannot be converted to Hz."
        ),
      };

    /// <summary>
    /// Gets the current clock output duty cycle as a percentage (%).
    /// </summary>
    /// <value>
    /// The current duty cycle in percent (0, 25, 50, or 75),
    /// or an exception if the value is undefined.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="IClockOutputController.CurrentClockOutputDutyCycle"/>
    /// is set to a value that is not defined in <see cref="ClockOutputDutyCycle"/>.
    /// </exception>
    public int CurrentClockOutputDutyCycleInPercent
      => ThrowIfReceiverIsNull(controller, nameof(controller)).CurrentClockOutputDutyCycle switch {
        ClockOutputDutyCycle.Duty0 => 0,
        ClockOutputDutyCycle.Duty25 => 25,
        ClockOutputDutyCycle.Duty50 => 50,
        ClockOutputDutyCycle.Duty75 => 75,

        var invalid => throw new InvalidOperationException(
          $"The device returned an undefined duty cycle value (0x{invalid:X}), which cannot be converted to a percentage."
        ),
      };

    /// <summary>
    /// Gets the current clock output duty cycle as a ratio (0.0 to 1.0).
    /// </summary>
    /// <value>
    /// The current duty ratio (0.0, 0.25, 0.5, or 0.75),
    /// or an exception if the value is undefined.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="IClockOutputController.CurrentClockOutputDutyCycle"/>
    /// is set to a value that is not defined in <see cref="ClockOutputDutyCycle"/>.
    /// </exception>
    public double CurrentClockOutputDutyRatio
      => ThrowIfReceiverIsNull(controller, nameof(controller)).CurrentClockOutputDutyCycle switch {
        ClockOutputDutyCycle.Duty0 => 0.0,
        ClockOutputDutyCycle.Duty25 => 0.25,
        ClockOutputDutyCycle.Duty50 => 0.5,
        ClockOutputDutyCycle.Duty75 => 0.75,

        var invalid => throw new InvalidOperationException(
          $"The device returned an undefined duty cycle value (0x{invalid:X}), which cannot be converted to a duty ratio."
        ),
      };

    /// <summary>
    /// Resumes the clock output using the current frequency and duty cycle
    /// configuration.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <remarks>
    /// This method restores the GP1 pin function to <see cref="GpFunction.ClockOutput"/>
    /// while maintaining the frequency and duty cycle currently configured in the
    /// clock output module.
    /// </remarks>
    /// <seealso cref="IClockOutputController.SuspendClockOutput"/>
    /// <seealso cref="IClockOutputController.ConfigureAsClockOutput"/>
    public void ResumeClockOutput(
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).ConfigureAsClockOutput(
        frequency: null,
        dutyCycle: null,
        cancellationToken: cancellationToken
      );

    /// <summary>
    /// Asynchronously resumes the clock output using the current frequency and
    /// duty cycle configuration.
    /// </summary>
    /// <inheritdoc cref="ResumeClockOutput(IClockOutputController, CancellationToken)"/>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <seealso cref="IClockOutputController.SuspendClockOutputAsync"/>
    /// <seealso cref="IClockOutputController.ConfigureAsClockOutputAsync"/>
    public ValueTask ResumeClockOutputAsync(CancellationToken cancellationToken = default)
      => ThrowIfReceiverIsNull(controller, nameof(controller)).ConfigureAsClockOutputAsync(
        frequency: null,
        dutyCycle: null,
        cancellationToken: cancellationToken
      );
  }
}
