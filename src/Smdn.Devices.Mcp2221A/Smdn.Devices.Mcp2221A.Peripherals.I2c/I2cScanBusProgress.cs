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
  public I2cAddress CurrentAddress { get; }

  /// <summary>
  /// Gets the starting I2C address of the scan range (inclusive).
  /// </summary>
  public I2cAddress FromAddress { get; }

  /// <summary>
  /// Gets the ending I2C address of the scan range (inclusive).
  /// </summary>
  public I2cAddress ToAddress { get; }

  /// <summary>
  /// Gets the scan progress as a percentage value from 0 to 100.
  /// </summary>
  public int ProgressInPercent
    => ToAddress == FromAddress
      ? 100
      : 100 * ((int)CurrentAddress - (int)FromAddress) / ((int)ToAddress - (int)FromAddress);

  internal I2cScanBusProgress(
    I2cAddress currentAddress,
    I2cAddress fromAddress,
    I2cAddress toAddress
  )
  {
    CurrentAddress = currentAddress;
    FromAddress = fromAddress;
    ToAddress = toAddress;
  }
}
