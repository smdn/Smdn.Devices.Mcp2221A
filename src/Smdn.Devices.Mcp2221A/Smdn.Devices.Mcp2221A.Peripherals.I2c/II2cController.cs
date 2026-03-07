// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// Provides an abstract API for accessing the I2C functionality of the MCP2221/MCP2221A.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Mcp2221AI2cBus"/> class, which implements this interface, provides
/// access to I2C functionality through the API of its base class, <see cref="System.Device.I2c"/>.
/// However, it does not expose an API for canceling I2C transmission.
/// To use these functions, access must be made through the interface.
/// </para>
/// <para>
/// In contrast, the functionality provided using these I2C features, such as I2C bus scanning,
/// are provided by the extension methods defined in <see cref="II2cControllerExtensions"/>.
/// </para>
/// </remarks>
/// <seealso cref="II2cControllerExtensions"/>
/// <seealso cref="Mcp2221AI2cBus"/>
public interface II2cController {
  void CancelTransfer(I2cAddress address);

  ValueTask CancelTransferAsync(I2cAddress address);

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues reading command with 0-length in this case.
  ///   </para>
  /// </remarks>
  int Read(
    I2cAddress address,
    int transmissionSpeedInKbps,
    Span<byte> buffer,
    CancellationToken cancellationToken
  );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues reading command with 0-length in this case.
  ///   </para>
  /// </remarks>
  ValueTask<int> ReadAsync(
    I2cAddress address,
    int transmissionSpeedInKbps,
    Memory<byte> buffer,
    CancellationToken cancellationToken
  );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues writing command with 0-length in this case.
  ///   </para>
  /// </remarks>
  void Write(
    I2cAddress address,
    int transmissionSpeedInKbps,
    ReadOnlySpan<byte> buffer,
    CancellationToken cancellationToken
  );

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  ///   <para>
  ///     An empty buffer can be specified to <paramref name="buffer"/>.
  ///     This method issues writing command with 0-length in this case.
  ///   </para>
  /// </remarks>
  ValueTask WriteAsync(
    I2cAddress address,
    int transmissionSpeedInKbps,
    ReadOnlyMemory<byte> buffer,
    CancellationToken cancellationToken
  );
}
