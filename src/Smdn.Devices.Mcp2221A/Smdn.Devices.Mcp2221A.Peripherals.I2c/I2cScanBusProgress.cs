// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// Represents the progress of an I2C bus scan operation.
/// </summary>
/// <remarks>
/// This type is used as the type parameter for <see cref="System.IProgress{T}"/>
/// when scanning the I2C bus with <see cref="II2cControllerBusScanningExtensions.ScanBus"/>
/// or <see cref="II2cControllerBusScanningExtensions.ScanBusAsync"/>.
/// </remarks>
/// <seealso cref="II2cControllerBusScanningExtensions.ScanBus"/>
/// <seealso cref="II2cControllerBusScanningExtensions.ScanBusAsync"/>
public readonly struct I2cScanBusProgress {
  /// <summary>
  /// Gets the I2C address currently being scanned.
  /// </summary>
  public I2cAddress ScanningAddress { get; }

  /// <summary>
  /// Gets the minimum address of the scan range.
  /// </summary>
  public I2cAddress AddressRangeMin { get; }

  /// <summary>
  /// Gets the maximum address of the scan range.
  /// </summary>
  public I2cAddress AddressRangeMax { get; }

  /// <summary>
  /// Gets the scan progress as a percentage value from 0 to 100.
  /// </summary>
  public int ProgressInPercent => 100 * ((int)ScanningAddress - (int)AddressRangeMin) / ((int)AddressRangeMax - (int)AddressRangeMin);

  internal I2cScanBusProgress(
    I2cAddress scanningAddress,
    I2cAddress addressRangeMin,
    I2cAddress addressRangeMax
  )
  {
    ScanningAddress = scanningAddress;
    AddressRangeMin = addressRangeMin;
    AddressRangeMax = addressRangeMax;
  }
}
