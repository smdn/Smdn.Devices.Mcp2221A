// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1815

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

public readonly struct I2cScanBusProgress {
  public I2cAddress ScanningAddress { get; }
  public I2cAddress AddressRangeMin { get; }
  public I2cAddress AddressRangeMax { get; }
  public int ProgressInPercent => 100 * ((int)ScanningAddress - (int)AddressRangeMin) / ((int)AddressRangeMax - (int)AddressRangeMin);

  internal I2cScanBusProgress(
    I2cAddress scanningAddress,
    I2cAddress addressRangeMin,
    I2cAddress addressRangeMax
  )
  {
    this.ScanningAddress = scanningAddress;
    this.AddressRangeMin = addressRangeMin;
    this.AddressRangeMax = addressRangeMax;
  }
}
