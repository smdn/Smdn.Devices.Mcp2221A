// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.I2c;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <inheritdoc/>
[CLSCompliant(false)]
public sealed class Mcp2221AI2cDevice : I2cDevice, II2cDevice {
  private Mcp2221AI2cBus i2cBus;
  internal Mcp2221AI2cBus I2cBus => i2cBus ?? throw new ObjectDisposedException(GetType().FullName);

  II2cController II2cDevice.Controller => I2cBus;

  private readonly I2cAddress deviceAddress;
  I2cAddress II2cDevice.Address => deviceAddress;

  /// <inheritdoc/>
  /// <remarks>
  ///   <para>
  ///     Setting this property to <c>0</c> or a negative value will throw
  ///     an <see cref="ArgumentOutOfRangeException"/>. For any other value,
  ///     even if the transmission speed is unsupported by the device,
  ///     this property will not throw an exception. However, an <see cref="ArgumentOutOfRangeException"/>
  ///     will be thrown when the actual read or write operation is performed.
  ///   </para>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
#pragma warning disable SA1513
  public int TransmissionSpeedInKbps {
    get;
    set {
      if (value <= 0) {
        throw new ArgumentOutOfRangeException(
          paramName: nameof(TransmissionSpeedInKbps),
          actualValue: value,
          message: "Must be non-zero positive value."
        );
      }

      field = value;
    }
  } = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps;
#pragma warning restore SA1513

  private readonly bool shouldDisposeMcp2221A;

  private I2cConnectionSettings? connectionSettings;

  /// <inheritdoc/>
  /// <remarks>
  /// The <see cref="I2cConnectionSettings.BusId"/> of the object returned by
  /// this property is always <c>0</c>.
  /// </remarks>
  /// <see cref="II2cDevice.Address"/>
  public override I2cConnectionSettings ConnectionSettings {
    get {
      // lazy initialization
      connectionSettings ??= new(busId: 0, deviceAddress: (int)deviceAddress);

      return connectionSettings;
    }
  }

  internal Mcp2221AI2cDevice(
    Mcp2221AI2cBus i2cBus,
    I2cAddress deviceAddress,
    bool shouldDisposeMcp2221A
  )
  {
    this.i2cBus = i2cBus ?? throw new ArgumentNullException(nameof(i2cBus));
    this.deviceAddress = deviceAddress;
    this.shouldDisposeMcp2221A = shouldDisposeMcp2221A;
  }

  /// <inheritdoc/>
  protected override void Dispose(bool disposing)
  {
    if (disposing) {
      if (shouldDisposeMcp2221A)
        i2cBus?.Device?.Dispose();

      i2cBus = null!;
    }

    base.Dispose(disposing);
  }

  /// <inheritdoc/>
  public override byte ReadByte()
  {
    var ret = II2cDeviceExtensions.ReadByte(this);

    return ret == -1
      ? throw new I2cReadException()
      : (byte)ret;
  }

  /// <inheritdoc/>
  public override void Read(Span<byte> buffer)
    => II2cDeviceExtensions.Read(this, buffer);

  /// <inheritdoc/>
  public override void WriteByte(byte value)
    => II2cDeviceExtensions.WriteByte(this, value);

  /// <inheritdoc/>
  public override void Write(ReadOnlySpan<byte> buffer)
    => II2cDeviceExtensions.Write(this, buffer);

  /// <inheritdoc/>
  /// <remarks>
  /// The current implementation does not provide fully atomic operations;
  /// it calls <see cref="Write(ReadOnlySpan{byte})"/> and <see cref="Read(Span{byte})"/>
  /// consecutively. If an exception is thrown during <see cref="Write(ReadOnlySpan{byte})"/>,
  /// <see cref="Read(Span{byte})"/> will not be performed.
  /// </remarks>
  public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer)
  {
    Write(writeBuffer);
    Read(readBuffer);
  }

  /// <summary>
  /// Sets the I2C bus speed to Standard Mode (100 kbps).
  /// </summary>
  /// <returns>The current <see cref="Mcp2221AI2cDevice"/> instance for fluent chaining.</returns>
  public Mcp2221AI2cDevice WithStandardMode()
  {
    TransmissionSpeedInKbps = 100;
    return this;
  }

  /// <summary>
  /// Sets the I2C bus speed to Fast Mode (400 kbps).
  /// </summary>
  /// <returns>The current <see cref="Mcp2221AI2cDevice"/> instance for fluent chaining.</returns>
  public Mcp2221AI2cDevice WithFastMode()
  {
    TransmissionSpeedInKbps = 400;
    return this;
  }
}
