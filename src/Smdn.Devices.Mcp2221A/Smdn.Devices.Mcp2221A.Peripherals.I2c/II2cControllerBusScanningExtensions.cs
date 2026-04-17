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

#pragma warning disable IDE0040
partial class II2cControllerExtensions {
#pragma warning restore IDE0040
  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  public static async ValueTask<(IReadOnlyI2cAddressSet WriteAddressSet, IReadOnlyI2cAddressSet ReadAddressSet)> ScanBusAsync(
    this II2cController controller,
    I2cAddress addressRangeMin = default,
    I2cAddress addressRangeMax = default,
    int i2cBusTransmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
    IProgress<I2cScanBusProgress>? progress = null,
    CancellationToken cancellationToken = default
  )
  {
    if (controller is null)
      throw new ArgumentNullException(nameof(controller));
    if ((int)addressRangeMax < (int)addressRangeMin)
      throw new ArgumentException($"{nameof(addressRangeMax)}({addressRangeMax}) must be greater than or equals to {nameof(addressRangeMin)}({addressRangeMin})", nameof(addressRangeMax));

    if (addressRangeMin.Equals(I2cAddress.Zero))
      addressRangeMin = I2cAddress.DeviceMinValue;
    if (addressRangeMax.Equals(I2cAddress.Zero))
      addressRangeMax = I2cAddress.DeviceMaxValue;

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
        _ = await ReadByteAsync(
          controller,
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

  /// <remarks>
  ///   <include
  ///     file="../Smdn.Devices.Mcp2221A.docs.xml"
  ///     path="docs/I2cReadWriteTransmissionSpeedParameter/remarks/*"
  ///   />
  /// </remarks>
  public static (IReadOnlyI2cAddressSet WriteAddressSet, IReadOnlyI2cAddressSet ReadAddressSet) ScanBus(
    this II2cController controller,
    I2cAddress addressRangeMin = default,
    I2cAddress addressRangeMax = default,
    int i2cBusTransmissionSpeedInKbps = Mcp2221AI2cBus.DefaultTransmissionSpeedInKbps,
    IProgress<I2cScanBusProgress>? progress = null,
    CancellationToken cancellationToken = default
  )
  {
    if (controller is null)
      throw new ArgumentNullException(nameof(controller));
    if ((int)addressRangeMax < (int)addressRangeMin)
      throw new ArgumentException($"{nameof(addressRangeMax)}({addressRangeMax}) must be greater than or equals to {nameof(addressRangeMin)}({addressRangeMin})", nameof(addressRangeMax));

    if (addressRangeMin.Equals(I2cAddress.Zero))
      addressRangeMin = I2cAddress.DeviceMinValue;
    if (addressRangeMax.Equals(I2cAddress.Zero))
      addressRangeMax = I2cAddress.DeviceMaxValue;

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
        _ = ReadByte(
          controller,
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
