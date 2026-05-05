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
  /// Gets the digital logic level of the GP pin as of the last communication
  /// that updated the device settings or retrieved its state.
  /// </summary>
  /// <value>
  /// The <see cref="PinValue"/> reflected from the device at the time of
  /// the last successful I/O operation.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="Read"/> or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes not only retrieval operations (e.g., <see cref="Read"/>), but
  /// also configuration and write operations (e.g., <see cref="ConfigureAsGpio"/>
  /// or <see cref="Write"/>).
  /// </para>
  /// <para>
  /// Since the MCP2221A handles logic levels for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's value or mode will refresh the
  /// <see cref="LastUpdatedValue"/> and <see cref="CurrentMode"/> for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the pin function is not assigned to <see cref="GpFunction.Gpio"/>.
  /// </exception>
  /// <seealso cref="Read(CancellationToken)"/>
  /// <seealso cref="ReadAsync(CancellationToken)"/>
  /// <seealso cref="Write(PinValue, CancellationToken)"/>
  /// <seealso cref="WriteAsync(PinValue, CancellationToken)"/>
  [CLSCompliant(false)]
  PinValue LastUpdatedValue { get; }

  /// <summary>
  /// Gets the current I/O direction (mode) of the GP pin.
  /// </summary>
  /// <value>
  /// The <see cref="PinMode"/> that is currently applied to the pin.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="GetMode"/> or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes both retrieval operations (e.g., <see cref="GetMode"/>)
  /// and configuration operations (e.g., <see cref="ConfigureAsGpio"/>
  /// or <see cref="SetMode"/>).
  /// Since the mode is determined solely by these software operations and
  /// does not change spontaneously on the hardware, this property reflects
  /// the true current state of the pin's I/O direction.
  /// </para>
  /// <para>
  /// Since the MCP2221A handles I/O modes for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's mode or value will refresh the
  /// <see cref="CurrentMode"/> and <see cref="LastUpdatedValue"/> for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the pin function is not assigned to <see cref="GpFunction.Gpio"/>.
  /// </exception>
  /// <seealso cref="GetMode(CancellationToken)"/>
  /// <seealso cref="GetModeAsync(CancellationToken)"/>
  /// <seealso cref="SetMode(PinMode, CancellationToken)"/>
  /// <seealso cref="SetModeAsync(PinMode, CancellationToken)"/>
  [CLSCompliant(false)]
  PinMode CurrentMode { get; }

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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
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
  /// <exception cref="Mcp2221AConfigurationException">
  /// Thrown when the GP pin is not currently configured as <see cref="GpFunction.Gpio"/>.
  /// </exception>
  void Write(
    PinValue value,
    CancellationToken cancellationToken = default
  );
}
