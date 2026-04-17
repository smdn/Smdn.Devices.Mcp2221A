// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

public static partial class II2cControllerExtensions {
  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.Read(I2cAddress,int,Span{byte},CancellationToken)"/>
  public static int Read(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte[] buffer,
    int offset,
    int count,
    CancellationToken cancellationToken = default
  )
    => (controller ?? throw new ArgumentNullException(nameof(controller))).Read(
      address,
      transmissionSpeedInKbps,
      (buffer ?? throw new ArgumentNullException(nameof(buffer))).AsSpan(offset, count),
      cancellationToken
    );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.ReadAsync(I2cAddress,int,Memory{byte},CancellationToken)"/>
  public static ValueTask<int> ReadAsync(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte[] buffer,
    int offset,
    int count,
    CancellationToken cancellationToken = default
  )
    => (controller ?? throw new ArgumentNullException(nameof(controller))).ReadAsync(
      address,
      transmissionSpeedInKbps,
      (buffer ?? throw new ArgumentNullException(nameof(buffer))).AsMemory(offset, count),
      cancellationToken
    );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.Read(I2cAddress,int,Span{byte},CancellationToken)"/>
  public static int ReadByte(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    CancellationToken cancellationToken = default
  )
  {
    if (controller is null)
      throw new ArgumentNullException(nameof(controller));

    Span<byte> buffer = stackalloc byte[1];

    var ret = controller.Read(address, transmissionSpeedInKbps, buffer, cancellationToken);

    return 0 == ret ? -1 : buffer[0];
  }

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.ReadAsync(I2cAddress,int,Memory{byte},CancellationToken)"/>
  public static async ValueTask<int> ReadByteAsync(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    CancellationToken cancellationToken = default
  )
  {
    if (controller is null)
      throw new ArgumentNullException(nameof(controller));

    var buffer = ArrayPool<byte>.Shared.Rent(1);

    try {
      var ret = await controller.ReadAsync(
        address,
        transmissionSpeedInKbps,
        buffer.AsMemory(0, 1),
        cancellationToken
      ).ConfigureAwait(false);

      return 0 == ret ? -1 : buffer[0];
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.Write(I2cAddress,int,ReadOnlySpan{byte},CancellationToken)"/>
  public static void Write(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte[] buffer,
    int offset,
    int count,
    CancellationToken cancellationToken = default
  )
    => (controller ?? throw new ArgumentNullException(nameof(controller))).Write(
      address,
      transmissionSpeedInKbps,
      (buffer ?? throw new ArgumentNullException(nameof(buffer))).AsSpan(offset, count),
      cancellationToken
    );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.WriteAsync(I2cAddress,int,ReadOnlyMemory{byte},CancellationToken)"/>
  public static ValueTask WriteAsync(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte[] buffer,
    int offset,
    int count,
    CancellationToken cancellationToken = default
  )
    => (controller ?? throw new ArgumentNullException(nameof(controller))).WriteAsync(
      address,
      transmissionSpeedInKbps,
      (buffer ?? throw new ArgumentNullException(nameof(buffer))).AsMemory(offset, count),
      cancellationToken
    );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.Write(I2cAddress,int,ReadOnlySpan{byte},CancellationToken)"/>
  public static void WriteByte(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte value,
    CancellationToken cancellationToken = default
  )
    => (controller ?? throw new ArgumentNullException(nameof(controller))).Write(
      address,
      transmissionSpeedInKbps,
      [value],
      cancellationToken
    );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  /// <seealso cref="II2cController.WriteAsync(I2cAddress,int,ReadOnlyMemory{byte},CancellationToken)"/>
  public static async ValueTask WriteByteAsync(
    this II2cController controller,
    I2cAddress address,
    int transmissionSpeedInKbps,
    byte value,
    CancellationToken cancellationToken = default
  )
  {
    if (controller is null)
      throw new ArgumentNullException(nameof(controller));

    var buffer = ArrayPool<byte>.Shared.Rent(1);

    try {
      buffer[0] = value;

      await controller.WriteAsync(
        address,
        transmissionSpeedInKbps,
        buffer.AsMemory(0, 1),
        cancellationToken
      ).ConfigureAwait(false);
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }
}
