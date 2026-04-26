// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides extension methods for <see cref="IGpioController"/>.
/// </summary>
public static class IGpioControllerExtensions {
#pragma warning disable IDE0051
  private static IGpioController ThrowIfReceiverIsNull(IGpioController controller, string paramName)
    => controller ?? throw new ArgumentNullException(paramName: paramName);
#pragma warning restore IDE0051

#pragma warning disable CA1034
  extension(IGpioController controller) {
#pragma warning restore CA1034
    /// <summary>
    /// Asynchronously configures the pin as a GPIO and sets its
    /// direction to output (<see cref="PinMode.Output"/>).
    /// </summary>
    /// <param name="initialValue">
    /// The initial <see cref="PinValue"/> to be set to the output pin.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
    /// </exception>
    /// <seealso cref="IGpioController.ConfigureAsGpioAsync(PinMode?, PinValue?, CancellationToken)"/>
    [CLSCompliant(false)]
    public ValueTask ConfigureAsGpioOutputAsync(
      PinValue? initialValue = default,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller))
        .ConfigureAsGpioAsync(PinMode.Output, initialValue, cancellationToken);

    /// <summary>
    /// Asynchronously configures the pin as a GPIO and sets its
    /// direction to input (<see cref="PinMode.Input"/>).
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
    /// </exception>
    /// <remarks>
    /// The pin operates in a high-impedance state. Note that the MCP2221/MCP2221A
    /// does not support internal pull-up or pull-down resistors.
    /// </remarks>
    /// <seealso cref="IGpioController.ConfigureAsGpioAsync(PinMode?, PinValue?, CancellationToken)"/>
    [CLSCompliant(false)]
    public ValueTask ConfigureAsGpioInputAsync(
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller))
        .ConfigureAsGpioAsync(PinMode.Input, initialValue: null, cancellationToken);

    /// <summary>
    /// Configures the pin as a GPIO and sets its direction to output
    /// (<see cref="PinMode.Output"/>).
    /// </summary>
    /// <param name="initialValue">
    /// The initial <see cref="PinValue"/> to be set to the output pin.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
    /// </exception>
    /// <seealso cref="IGpioController.ConfigureAsGpio(PinMode?, PinValue?, CancellationToken)"/>
    [CLSCompliant(false)]
    public void ConfigureAsGpioOutput(
      PinValue? initialValue = default,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller))
        .ConfigureAsGpio(PinMode.Output, initialValue, cancellationToken);

    /// <summary>
    /// Configures the pin as a GPIO and sets its direction to input
    /// (<see cref="PinMode.Input"/>).
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
    /// </exception>
    /// <remarks>
    /// The pin operates in a high-impedance state. Note that the MCP2221/MCP2221A
    /// does not support internal pull-up or pull-down resistors.
    /// </remarks>
    /// <seealso cref="IGpioController.ConfigureAsGpio(PinMode?, PinValue?, CancellationToken)"/>
    [CLSCompliant(false)]
    public void ConfigureAsGpioInput(
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller))
        .ConfigureAsGpio(PinMode.Input, initialValue: null, cancellationToken);
  }
}
