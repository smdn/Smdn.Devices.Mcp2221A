// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Peripherals.I2c;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides extension methods for <see cref="II2cController"/>.
/// </summary>
public static class II2cControllerExtensions {
#pragma warning disable IDE0051
  private static II2cController ThrowIfReceiverIsNull(II2cController controller, string paramName)
    => controller ?? throw new ArgumentNullException(paramName: paramName);
#pragma warning restore IDE0051

#pragma warning disable CA1034
  extension(II2cController controller) {
#pragma warning restore CA1034
    /// <remarks>
    ///   <include
    ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
    ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
    ///   />
    /// </remarks>
    /// <seealso cref="II2cController.Read(I2cAddress,int,Span{byte},CancellationToken)"/>
    public int Read(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).Read(
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
    public ValueTask<int> ReadAsync(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).ReadAsync(
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
    public int ReadByte(
      I2cAddress address,
      int transmissionSpeedInKbps,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

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
    public async ValueTask<int> ReadByteAsync(
      I2cAddress address,
      int transmissionSpeedInKbps,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

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
    public void Write(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).Write(
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
    public ValueTask WriteAsync(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte[] buffer,
      int offset,
      int count,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).WriteAsync(
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
    public void WriteByte(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte value,
      CancellationToken cancellationToken = default
    )
      => ThrowIfReceiverIsNull(controller, nameof(controller)).Write(
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
    public async ValueTask WriteByteAsync(
      I2cAddress address,
      int transmissionSpeedInKbps,
      byte value,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

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
}
