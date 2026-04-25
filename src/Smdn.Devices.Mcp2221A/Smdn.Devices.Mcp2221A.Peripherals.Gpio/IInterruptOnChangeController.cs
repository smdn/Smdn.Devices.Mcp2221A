// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines an interface for controlling the Interrupt-on-Change (IOC)
/// pins of the MCP2221/MCP2221A.
/// </summary>
public interface IInterruptOnChangeController {
  /// <summary>
  /// Gets the currently configured trigger condition for the
  /// Interrupt-on-Change (IOC) function.
  /// </summary>
  /// <value>
  /// An <see cref="InterruptOnChangeTrigger"/> indicating the edges
  /// that trigger an interrupt.
  /// </value>
  InterruptOnChangeTrigger CurrentInterruptOnChangeTrigger { get; }

  /// <summary>
  /// Gets a value indicating whether an interrupt-on-change event was
  /// detected during the last read operation.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if an interrupt-on-change event was detected;
  /// otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// <para>
  /// This property returns the cached state from the last call to
  /// <see cref="ReadInterruptDetection(CancellationToken)"/> or
  /// <see cref="ReadInterruptDetectionAsync(CancellationToken)"/>.
  /// If no read operation has been performed since the instance was created or
  /// since the flag was last cleared, this property returns the initial or current
  /// state retrieved from the device.
  /// </para>
  /// <para>
  /// Note that this value is also updated by calling
  /// <see cref="IAdcController.ReadAnalogRaw"/> or <see cref="IAdcController.ReadAnalogRawAsync"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="ReadInterruptDetection(CancellationToken)"/>
  /// <seealso cref="ReadInterruptDetectionAsync(CancellationToken)"/>
  bool LastReadInterruptDetectionFlag { get; }

  /// <summary>
  /// Configures the GP1 pin to function as an Interrupt-on-Change (IOC) input.
  /// </summary>
  /// <param name="detectionTrigger">
  /// The <see cref="InterruptOnChangeTrigger"/> that specifies which edge
  /// transitions will be detected. If <see langword="null"/>, the current edge
  /// detection settings in the device's SRAM will be maintained.
  /// </param>
  /// <param name="clearDetectionFlag">
  /// <see langword="true"/> to clear the existing interrupt detection flag
  /// simultaneously with the configuration; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <remarks>
  /// This method changes the function of the GP1 pin to <see cref="GpFunction.InterruptOnChange"/>.
  /// The IOC function is dedicated to the GP1 pin and detects changes in the
  /// input signal level.
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.InterruptOnChange"/>
  void ConfigureAsInterruptOnChange(
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously configures the GP1 pin to function as an
  /// Interrupt-on-Change (IOC) input.
  /// </summary>
  /// <inheritdoc cref="ConfigureAsInterruptOnChange(InterruptOnChangeTrigger?, bool, CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ConfigureAsInterruptOnChangeAsync(
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag,
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Reads the current interrupt detection flag from the device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if an interrupt-on-change event has been detected
  /// (i.e., the configured edge transition has occurred) since the flag was last
  /// cleared; otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method performs a communication with the device to fetch the latest
  /// 'interrupt edge detector state'.
  /// </para>
  /// <para>
  /// Calling this method also updates <see cref="IAdcController.LastReadAnalogRawValue"/>
  /// simultaneously, as both values are retrieved using the same device command.
  /// </para>
  /// </remarks>
  bool ReadInterruptDetection(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously reads the current interrupt detection flag from the device.
  /// </summary>
  /// <inheritdoc cref="ReadInterruptDetection(CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask{Boolean}"/> representing the asynchronous operation.
  /// The result is <see langword="true"/> if an interrupt-on-change event has been
  /// detected; otherwise, <see langword="false"/>.
  /// </returns>
  ValueTask<bool> ReadInterruptDetectionAsync(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Clears the interrupt detection flag.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <remarks>
  /// This method resets the 'interrupt edge detector state' in the device.
  /// After calling this method, <see cref="LastReadInterruptDetectionFlag"/> is set to <see langword="false"/>,
  /// and <see cref="ReadInterruptDetection"/> will return <see langword="false"/>
  /// until the next configured edge transition occurs.
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="ReadInterruptDetection(CancellationToken)"/>
  /// <seealso cref="LastReadInterruptDetectionFlag"/>
  void ClearInterruptDetection(
    CancellationToken cancellationToken = default
  );

  /// <summary>
  /// Asynchronously clears the interrupt detection flag.
  /// </summary>
  /// <inheritdoc cref="ClearInterruptDetection(CancellationToken)"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  ValueTask ClearInterruptDetectionAsync(
    CancellationToken cancellationToken = default
  );
}
