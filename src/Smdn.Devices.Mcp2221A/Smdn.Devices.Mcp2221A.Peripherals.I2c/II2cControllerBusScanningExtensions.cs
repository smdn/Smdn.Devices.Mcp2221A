// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using IReadOnlyI2cAddressSet =
#if SYSTEM_COLLECTIONS_GENERIC_IREADONLYSET
System.Collections.Generic.IReadOnlySet
#else
System.Collections.Generic.IReadOnlyCollection
#endif
<Smdn.Devices.Mcp2221A.I2cAddress>;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// Provides extension methods for <see cref="II2cController"/> to
/// perform I2C bus scanning operations.
/// </summary>
public static class II2cControllerBusScanningExtensions {
#pragma warning disable IDE0051
  private static II2cController ThrowIfReceiverIsNull(II2cController controller, string paramName)
    => controller ?? throw new ArgumentNullException(paramName: paramName);

  private static void ValidateAddressRange(
    ref I2cAddress fromAddress,
    ref I2cAddress toAddress
  )
  {
    if (toAddress < fromAddress)
      throw new ArgumentException($"{nameof(toAddress)}({toAddress}) must be greater than or equals to {nameof(fromAddress)}({fromAddress})", nameof(toAddress));

    if (fromAddress.Equals(I2cAddress.Zero))
      fromAddress = I2cAddress.DeviceMinValue;
    if (toAddress.Equals(I2cAddress.Zero))
      toAddress = I2cAddress.DeviceMaxValue;
  }
#pragma warning restore IDE0051

#pragma warning disable CA1034
  extension(II2cController controller) {
#pragma warning restore CA1034
    /// <summary>
    /// Asynchronously scans the I2C bus for responding devices by attempting
    /// Write and Read operations across a specified range of addresses.
    /// </summary>
    /// <inheritdoc cref="ScanBus(II2cController, I2cAddress, I2cAddress, int, IProgress{I2cScanBusProgress}?, CancellationToken)" path="/param|/exception"/>
    /// <returns>
    /// A task representing the asynchronous operation, containing the <c>WriteAddressSet</c>
    /// and <c>ReadAddressSet</c> of responding addresses.
    /// </returns>
    /// <remarks>
    ///   <include
    ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
    ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
    ///   />
    /// </remarks>
    public async ValueTask<(IReadOnlyI2cAddressSet WriteAddressSet, IReadOnlyI2cAddressSet ReadAddressSet)> ScanBusAsync(
      I2cAddress fromAddress = default,
      I2cAddress toAddress = default,
      int transmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
      IProgress<I2cScanBusProgress>? progress = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

      ValidateAddressRange(
        fromAddress: ref fromAddress,
        toAddress: ref toAddress
      );

      var writeAddressSet = new SortedSet<I2cAddress>();
      var readAddressSet = new SortedSet<I2cAddress>();

      for (var addr = (int)fromAddress; addr <= (int)toAddress; addr++) {
        var address = new I2cAddress(addr);

        progress?.Report(new I2cScanBusProgress(address, fromAddress, toAddress));

        try {
          await controller.WriteAsync(
            address,
            transmissionSpeedInKbps,
            default,
            cancellationToken
          ).ConfigureAwait(false);

          writeAddressSet.Add(address);
        }
        catch (I2cNackException ex) when (ex.Address.Equals(address)) {
          // expected exception
        }

        try {
          _ = await controller.ReadByteAsync(
            address,
            transmissionSpeedInKbps,
            cancellationToken
          ).ConfigureAwait(false);

          readAddressSet.Add(address);
        }
        catch (I2cReadException ex) when (ex.Address.Equals(address)) {
          // expected exception
        }
      }

      return (writeAddressSet, readAddressSet);
    }

    /// <summary>
    /// Scans the I2C bus for responding devices by attempting
    /// Write and Read operations across a specified range of addresses.
    /// </summary>
    /// <param name="fromAddress">
    /// The starting I2C address of the scan range (inclusive).
    /// If set to <see langword="default"/>, <see cref="I2cAddress.DeviceMinValue"/> (0x08) is used.
    /// </param>
    /// <param name="toAddress">
    /// The ending I2C address of the scan range (inclusive).
    /// If set to <see langword="default"/>, <see cref="I2cAddress.DeviceMaxValue"/> (0x77) is used.
    /// </param>
    /// <param name="transmissionSpeedInKbps">
    /// The I2C transmission speed in kbps used during the scan.
    /// Defaults to <see cref="Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps"/> (100 kbps).
    /// </param>
    /// <param name="progress">
    /// An optional <see cref="IProgress{I2cScanBusProgress}"/> to receive
    /// updates on the scan progress.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A tuple containing two sets: <c>WriteAddressSet</c> (addresses that
    /// responded to a Write operation) and <c>ReadAddressSet</c> (addresses that
    /// responded to a Read operation).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="fromAddress"/> is less than <paramref name="toAddress"/>.
    /// </exception>
    /// <remarks>
    ///   <include
    ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
    ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
    ///   />
    /// </remarks>
    public (IReadOnlyI2cAddressSet WriteAddressSet, IReadOnlyI2cAddressSet ReadAddressSet) ScanBus(
      I2cAddress fromAddress = default,
      I2cAddress toAddress = default,
      int transmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
      IProgress<I2cScanBusProgress>? progress = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

      ValidateAddressRange(
        fromAddress: ref fromAddress,
        toAddress: ref toAddress
      );

      var writeAddressSet = new SortedSet<I2cAddress>();
      var readAddressSet = new SortedSet<I2cAddress>();

      for (var addr = (int)fromAddress; addr <= (int)toAddress; addr++) {
        var address = new I2cAddress(addr);

        progress?.Report(new I2cScanBusProgress(address, fromAddress, toAddress));

        try {
          controller.Write(
            address,
            transmissionSpeedInKbps,
            default,
            cancellationToken
          );

          writeAddressSet.Add(address);
        }
        catch (I2cNackException ex) when (ex.Address.Equals(address)) {
          // expected exception
        }

        try {
          _ = controller.ReadByte(
            address,
            transmissionSpeedInKbps,
            cancellationToken
          );

          readAddressSet.Add(address);
        }
        catch (I2cReadException ex) when (ex.Address.Equals(address)) {
          // expected exception
        }
      }

      return (writeAddressSet, readAddressSet);
    }
  }
}
