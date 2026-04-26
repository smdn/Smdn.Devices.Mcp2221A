// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines an interface for controlling the General Purpose
/// Input/Output (GPIO) pins of the MCP2221/MCP2221A.
/// </summary>
[CLSCompliant(false)]
public interface IGpioController {
  /// <summary>
  /// Asynchronously configures the pin as a GPIO and sets its direction
  /// and initial output value.
  /// </summary>
  /// <param name="mode">
  /// The <see cref="PinMode"/> to be set (e.g., <see cref="PinMode.Input"/> or <see cref="PinMode.Output"/>).
  /// If <see langword="null"/>, the current direction is maintained.
  /// </param>
  /// <param name="initialValue">
  /// The initial <see cref="PinValue"/> to be set if the
  /// <paramref name="mode"/> is <see cref="PinMode.Output"/>.
  /// If <see langword="null"/>, the current output value is maintained.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <exception cref="NotSupportedException">
  /// Thrown when <paramref name="mode"/> is set to <see cref="PinMode.InputPullUp"/>
  /// or <see cref="PinMode.InputPullDown"/> as these modes are not
  /// supported by the device.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <remarks>
  /// When <paramref name="mode"/> is set to <see cref="PinMode.Input"/>,
  /// the pin operates in a high-impedance state. Note that the MCP2221/MCP2221A does
  /// not support internal pull-up or pull-down resistors.
  /// </remarks>
  /// <seealso cref="GpFunction.Gpio"/>
  ValueTask ConfigureAsGpioAsync(
    PinMode? mode,
    PinValue? initialValue,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Configures the pin as a GPIO and sets its direction and initial output value.
  /// </summary>
  /// <param name="mode">
  /// The <see cref="PinMode"/> to be set (e.g., <see cref="PinMode.Input"/> or <see cref="PinMode.Output"/>).
  /// If <see langword="null"/>, the current direction is maintained.
  /// </param>
  /// <param name="initialValue">
  /// The initial <see cref="PinValue"/> to be set if the
  /// <paramref name="mode"/> is <see cref="PinMode.Output"/>.
  /// If <see langword="null"/>, the current output value is maintained.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="NotSupportedException">
  /// Thrown when <paramref name="mode"/> is set to <see cref="PinMode.InputPullUp"/>
  /// or <see cref="PinMode.InputPullDown"/> as these modes are not
  /// supported by the device.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <remarks>
  /// When <paramref name="mode"/> is set to <see cref="PinMode.Input"/>,
  /// the pin operates in a high-impedance state. Note that the MCP2221/MCP2221A does
  /// not support internal pull-up or pull-down resistors.
  /// </remarks>
  /// <seealso cref="GpFunction.Gpio"/>
  void ConfigureAsGpio(
    PinMode? mode,
    PinValue? initialValue,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously gets the current direction (<see cref="PinMode"/>) of the pin.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{T}"/> representing the asynchronous operation,
  /// containing the current <see cref="PinMode"/>.
  /// </returns>
  ValueTask<PinMode> GetModeAsync(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Gets the current direction (<see cref="PinMode"/>) of the pin.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// The current <see cref="PinMode"/>.
  /// </returns>
  PinMode GetMode(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously sets the direction (<see cref="PinMode"/>) of the pin.
  /// </summary>
  /// <param name="mode">
  /// The <see cref="PinMode"/> to be set.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <exception cref="NotSupportedException">
  /// Thrown when <paramref name="mode"/> is set to <see cref="PinMode.InputPullUp"/>
  /// or <see cref="PinMode.InputPullDown"/> as these modes are not
  /// supported by the device.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <remarks>
  /// When <paramref name="mode"/> is set to <see cref="PinMode.Input"/>,
  /// the pin operates in a high-impedance state. Note that the MCP2221/MCP2221A does
  /// not support internal pull-up or pull-down resistors.
  /// </remarks>
  ValueTask SetModeAsync(
    PinMode mode,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Sets the direction (<see cref="PinMode"/>) of the pin.
  /// </summary>
  /// <param name="mode">
  /// The <see cref="PinMode"/> to be set.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="NotSupportedException">
  /// Thrown when <paramref name="mode"/> is set to <see cref="PinMode.InputPullUp"/>
  /// or <see cref="PinMode.InputPullDown"/> as these modes are not
  /// supported by the device.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <remarks>
  /// When <paramref name="mode"/> is set to <see cref="PinMode.Input"/>,
  /// the pin operates in a high-impedance state. Note that the MCP2221/MCP2221A does
  /// not support internal pull-up or pull-down resistors.
  /// </remarks>
  void SetMode(
    PinMode mode,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously reads the digital logic level from the pin.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{PinValue}"/> representing the asynchronous operation,
  /// containing the <see cref="PinValue"/> read from the pin.
  /// </returns>
  ValueTask<PinValue> ReadAsync(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Reads the digital logic level from the pin.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// The <see cref="PinValue"/> read from the pin.
  /// </returns>
  PinValue Read(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously writes a digital logic level to the pin.
  /// </summary>
  /// <param name="value">
  /// The <see cref="PinValue"/> to be written.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  ValueTask WriteAsync(
    PinValue value,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Writes a digital logic level to the pin.
  /// </summary>
  /// <param name="value">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  void Write(
    PinValue value,
    CancellationToken cancellationToken = default
  );
}
