// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.I2c;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <inheritdoc/>
[CLSCompliant(false)]
public sealed partial class Mcp2221AI2cBus : I2cBus, II2cController {
  internal static readonly EventId EventIdI2cCommand = new(10, "I2C command");
  internal static readonly EventId EventIdI2cEngineState = new(11, "I2C engine state");

  internal const int DefaultTransmissionSpeedInKbps = 100;

  internal Mcp2221A Device { get; }

  private readonly ILogger? logger;

  internal Mcp2221AI2cBus(
    Mcp2221A device,
    ILogger? logger
  )
  {
    Device = device ?? throw new ArgumentNullException(nameof(device));
    this.logger = logger;
  }

  /// <inheritdoc/>
  public override
#if NET
  Mcp2221AI2cDevice
#else
  I2cDevice
#endif
  CreateDevice(int deviceAddress)
    => CreateDevice(
      deviceAddress: new(deviceAddress),
      transmissionSpeedInKbps: DefaultTransmissionSpeedInKbps,
      shouldDisposeMcp2221A: false
    );

  /// <summary>
  /// Creates a new <see cref="System.Device.I2c.I2cDevice"/> adapter for a specific I2C address.
  /// This adapter allows the MCP2221/MCP2221A to interface with the extensive collection of I2C device
  /// bindings provided by the <see href="https://www.nuget.org/packages/Iot.Device.Bindings">Iot.Device.Bindings</see>
  /// library.
  /// </summary>
  /// <param name="deviceAddress">The I2C address of the target device.</param>
  /// <param name="shouldDisposeMcp2221A">
  /// <see langword="true"/> to automatically dispose the underlying <see cref="Mcp2221A"/>
  /// when this adapter is disposed; <see langword="false"/> to keep the <see cref="Mcp2221A"/> open.
  /// The default is <see langword="false"/>.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AI2cDevice"/> instance that wraps the MCP2221 I2C functionality
  /// for the specified address.
  /// </returns>
  /// <exception cref="ObjectDisposedException">
  /// Thrown if the parent <see cref="Mcp2221A"/> has already been disposed.
  /// </exception>
  [CLSCompliant(false)]
  public Mcp2221AI2cDevice CreateDevice(
    I2cAddress deviceAddress,
    bool shouldDisposeMcp2221A = false
  )
    => CreateDevice(
      deviceAddress: deviceAddress,
      transmissionSpeedInKbps: DefaultTransmissionSpeedInKbps,
      shouldDisposeMcp2221A: shouldDisposeMcp2221A
    );

  /// <summary>
  /// Creates a new <see cref="System.Device.I2c.I2cDevice"/> adapter for a specific I2C address.
  /// This adapter allows the MCP2221/MCP2221A to interface with the extensive collection of I2C device
  /// bindings provided by the <see href="https://www.nuget.org/packages/Iot.Device.Bindings">Iot.Device.Bindings</see>
  /// library.
  /// </summary>
  /// <param name="deviceAddress">The I2C address of the target device.</param>
  /// <param name="transmissionSpeedInKbps">
  /// The transmission speed used for reading and writing to the I2C bus in [kbps] units.
  /// </param>
  /// <param name="shouldDisposeMcp2221A">
  /// <see langword="true"/> to automatically dispose the underlying <see cref="Mcp2221A"/>
  /// when this adapter is disposed; <see langword="false"/> to keep the <see cref="Mcp2221A"/> open.
  /// The default is <see langword="false"/>.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AI2cDevice"/> instance that wraps the MCP2221 I2C functionality
  /// for the specified address.
  /// </returns>
  /// <exception cref="ObjectDisposedException">
  /// Thrown if the parent <see cref="Mcp2221A"/> has already been disposed.
  /// </exception>
  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  [CLSCompliant(false)]
  public Mcp2221AI2cDevice CreateDevice(
    I2cAddress deviceAddress,
    int transmissionSpeedInKbps,
    bool shouldDisposeMcp2221A = false
  )
  {
    Device.ThrowIfDisposed();

    return new(
      i2cBus: this,
      deviceAddress: deviceAddress,
      shouldDisposeMcp2221A: shouldDisposeMcp2221A
    ) {
      TransmissionSpeedInKbps = transmissionSpeedInKbps,
    };
  }

  /// <inheritdoc/>
  /// <remarks>
  /// The current implementation of this method does nothing.
  /// After calling this method, the created <see cref="I2cDevice"/> remains
  /// available for use.
  /// </remarks>
  public override void RemoveDevice(int deviceAddress)
    => RemoveDevice(new I2cAddress(deviceAddress));

  /// <remarks>
  /// The current implementation of this method does nothing.
  /// After calling this method, the created <see cref="I2cDevice"/> remains
  /// available for use.
  /// </remarks>
#pragma warning disable IDE0060, CA1822
  public void RemoveDevice(I2cAddress deviceAddress)
#pragma warning restore IDE0060, CA1822
  {
    // do nothing in this class
  }
}
