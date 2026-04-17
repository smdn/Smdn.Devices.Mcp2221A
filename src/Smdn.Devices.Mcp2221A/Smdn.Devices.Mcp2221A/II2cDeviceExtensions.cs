// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

public static class II2cDeviceExtensions {
  /// <seealso cref="II2cController.Read(I2cAddress, int, Span{byte},CancellationToken)"/>
  public static void Read(
    this II2cDevice device,
    Span<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    device.Controller.Read(
      device.Address,
      device.TransmissionSpeedInKbps,
      buffer,
      cancellationToken
    );
  }

  /// <seealso cref="II2cController.ReadAsync(I2cAddress, int, Memory{byte}, CancellationToken)"/>
  public static ValueTask<int> ReadAsync(
    this II2cDevice device,
    Memory<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    return device.Controller.ReadAsync(
      device.Address,
      device.TransmissionSpeedInKbps,
      buffer,
      cancellationToken
    );
  }

  /// <seealso cref="II2cControllerExtensions.ReadByte(II2cController, I2cAddress, int, CancellationToken)"/>
  public static int ReadByte(
    this II2cDevice device,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    return device.Controller.ReadByte(
      device.Address,
      device.TransmissionSpeedInKbps,
      cancellationToken
    );
  }

  /// <seealso cref="II2cControllerExtensions.ReadByteAsync(II2cController, I2cAddress, int, CancellationToken)"/>
  public static ValueTask<int> ReadByteAsync(
    this II2cDevice device,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    return device.Controller.ReadByteAsync(
      device.Address,
      device.TransmissionSpeedInKbps,
      cancellationToken
    );
  }

  /// <seealso cref="II2cController.Write(I2cAddress, int, ReadOnlySpan{byte}, CancellationToken)"/>
  public static void Write(
    this II2cDevice device,
    ReadOnlySpan<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    device.Controller.Write(
      device.Address,
      device.TransmissionSpeedInKbps,
      buffer,
      cancellationToken
    );
  }

  /// <seealso cref="II2cController.WriteAsync(I2cAddress, int, ReadOnlyMemory{byte}, CancellationToken)"/>
  public static ValueTask WriteAsync(
    this II2cDevice device,
    ReadOnlyMemory<byte> buffer,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    return device.Controller.WriteAsync(
      device.Address,
      device.TransmissionSpeedInKbps,
      buffer,
      cancellationToken
    );
  }

  /// <seealso cref="II2cControllerExtensions.WriteByte(II2cController, I2cAddress, int, byte, CancellationToken)"/>
  public static void WriteByte(
    this II2cDevice device,
    byte value,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    device.Controller.WriteByte(
      device.Address,
      device.TransmissionSpeedInKbps,
      value,
      cancellationToken
    );
  }

  /// <seealso cref="II2cControllerExtensions.WriteByteAsync(II2cController, I2cAddress, int, byte, CancellationToken)"/>
  public static ValueTask WriteByteAsync(
    this II2cDevice device,
    byte value,
    CancellationToken cancellationToken = default
  )
  {
    if (device is null)
      throw new ArgumentNullException(nameof(device));

    return device.Controller.WriteByteAsync(
      device.Address,
      device.TransmissionSpeedInKbps,
      value,
      cancellationToken
    );
  }
}
