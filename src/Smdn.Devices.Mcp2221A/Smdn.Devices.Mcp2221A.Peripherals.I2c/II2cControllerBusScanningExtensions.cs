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
#pragma warning restore IDE0051

  private static void ValidateAddressRange(
    ref I2cAddress addressRangeMin,
    ref I2cAddress addressRangeMax
  )
  {
    if (addressRangeMax < addressRangeMin)
      throw new ArgumentException($"{nameof(addressRangeMax)}({addressRangeMax}) must be greater than or equals to {nameof(addressRangeMin)}({addressRangeMin})", nameof(addressRangeMax));

    if (addressRangeMin.Equals(I2cAddress.Zero))
      addressRangeMin = I2cAddress.DeviceMinValue;
    if (addressRangeMax.Equals(I2cAddress.Zero))
      addressRangeMax = I2cAddress.DeviceMaxValue;
  }

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
      I2cAddress addressRangeMin = default,
      I2cAddress addressRangeMax = default,
      int i2cBusTransmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
      IProgress<I2cScanBusProgress>? progress = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

      ValidateAddressRange(
        addressRangeMin: ref addressRangeMin,
        addressRangeMax: ref addressRangeMax
      );

      var writeAddressSet = new SortedSet<I2cAddress>();
      var readAddressSet = new SortedSet<I2cAddress>();

      for (var addr = (int)addressRangeMin; addr <= (int)addressRangeMax; addr++) {
        var address = new I2cAddress(addr);

        progress?.Report(new I2cScanBusProgress(address, addressRangeMin, addressRangeMax));

        try {
          await controller.WriteAsync(
            address,
            i2cBusTransmissionSpeedInKbps,
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
            i2cBusTransmissionSpeedInKbps,
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
    /// <param name="addressRangeMin">
    /// The inclusive lower bound of the address range to scan.
    /// If set to <see langword="default"/>, <see cref="I2cAddress.DeviceMinValue"/> (0x08) is used.
    /// </param>
    /// <param name="addressRangeMax">
    /// The inclusive upper bound of the address range to scan.
    /// If set to <see langword="default"/>, <see cref="I2cAddress.DeviceMaxValue"/> (0x77) is used.
    /// </param>
    /// <param name="i2cBusTransmissionSpeedInKbps">
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
    /// Thrown when <paramref name="addressRangeMax"/> is less than <paramref name="addressRangeMin"/>.
    /// </exception>
    /// <remarks>
    ///   <include
    ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
    ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
    ///   />
    /// </remarks>
    public (IReadOnlyI2cAddressSet WriteAddressSet, IReadOnlyI2cAddressSet ReadAddressSet) ScanBus(
      I2cAddress addressRangeMin = default,
      I2cAddress addressRangeMax = default,
      int i2cBusTransmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
      IProgress<I2cScanBusProgress>? progress = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfReceiverIsNull(controller, nameof(controller));

      ValidateAddressRange(
        addressRangeMin: ref addressRangeMin,
        addressRangeMax: ref addressRangeMax
      );

      var writeAddressSet = new SortedSet<I2cAddress>();
      var readAddressSet = new SortedSet<I2cAddress>();

      for (var addr = (int)addressRangeMin; addr <= (int)addressRangeMax; addr++) {
        var address = new I2cAddress(addr);

        progress?.Report(new I2cScanBusProgress(address, addressRangeMin, addressRangeMax));

        try {
          controller.Write(
            address,
            i2cBusTransmissionSpeedInKbps,
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
            i2cBusTransmissionSpeedInKbps,
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
