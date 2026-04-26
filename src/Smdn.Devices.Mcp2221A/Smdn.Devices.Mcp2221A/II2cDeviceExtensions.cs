// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Peripherals.I2c;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides extension methods for <see cref="II2cDevice"/>.
/// </summary>
public static class II2cDeviceExtensions {
#pragma warning disable IDE0051
  private static II2cDevice ThrowIfReceiverIsNull(II2cDevice device, string paramName)
    => device ?? throw new ArgumentNullException(paramName: paramName);
#pragma warning restore IDE0051

#pragma warning disable CA1034
  extension(II2cDevice device) {
#pragma warning restore CA1034
    /// <seealso cref="II2cController.Read(I2cAddress, int, Span{byte},CancellationToken)"/>
    public void Read(
      Span<byte> buffer,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.Read(
        device.Address,
        device.TransmissionSpeedInKbps,
        buffer,
        cancellationToken
      );

    /// <seealso cref="II2cController.ReadAsync(I2cAddress, int, Memory{byte}, CancellationToken)"/>
    public ValueTask<int> ReadAsync(
      Memory<byte> buffer,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.ReadAsync(
        device.Address,
        device.TransmissionSpeedInKbps,
        buffer,
        cancellationToken
      );

    /// <seealso cref="II2cControllerExtensions.ReadByte(II2cController, I2cAddress, int, CancellationToken)"/>
    public int ReadByte(
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.ReadByte(
        device.Address,
        device.TransmissionSpeedInKbps,
        cancellationToken
      );

    /// <seealso cref="II2cControllerExtensions.ReadByteAsync(II2cController, I2cAddress, int, CancellationToken)"/>
    public ValueTask<int> ReadByteAsync(
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.ReadByteAsync(
        device.Address,
        device.TransmissionSpeedInKbps,
        cancellationToken
      );

    /// <seealso cref="II2cController.Write(I2cAddress, int, ReadOnlySpan{byte}, CancellationToken)"/>
    public void Write(
      ReadOnlySpan<byte> buffer,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.Write(
        device.Address,
        device.TransmissionSpeedInKbps,
        buffer,
        cancellationToken
      );

    /// <seealso cref="II2cController.WriteAsync(I2cAddress, int, ReadOnlyMemory{byte}, CancellationToken)"/>
    public ValueTask WriteAsync(
      ReadOnlyMemory<byte> buffer,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.WriteAsync(
        device.Address,
        device.TransmissionSpeedInKbps,
        buffer,
        cancellationToken
      );

    /// <seealso cref="II2cControllerExtensions.WriteByte(II2cController, I2cAddress, int, byte, CancellationToken)"/>
    public void WriteByte(
      byte value,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.WriteByte(
        device.Address,
        device.TransmissionSpeedInKbps,
        value,
        cancellationToken
      );

    /// <seealso cref="II2cControllerExtensions.WriteByteAsync(II2cController, I2cAddress, int, byte, CancellationToken)"/>
    public ValueTask WriteByteAsync(
      byte value,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(device, nameof(device)).Controller.WriteByteAsync(
        device.Address,
        device.TransmissionSpeedInKbps,
        value,
        cancellationToken
      );
  }
}
