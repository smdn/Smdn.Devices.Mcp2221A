// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221A.CreateAsync(serviceProvider);

var initialCursorPosition = (left: Console.CursorLeft, top: Console.CursorTop);

var scanBusProgress = new Progress<I2cScanBusProgress>(progress => {
  Console.SetCursorPosition(initialCursorPosition.left, initialCursorPosition.top);
  Console.Write(
    "Scanning 0x{0}: Min=0x{1} {3}{4} Max=0x{2}",
    progress.ScanningAddress,
    progress.AddressRangeMin,
    progress.AddressRangeMax,
    new string('|', progress.ProgressInPercent),
    new string('-', 100 - progress.ProgressInPercent)
  );

  if (progress.ProgressInPercent == 100)
    Console.WriteLine();
});

I2cAddress addressRangeMin = I2cAddress.DeviceMinValue;
I2cAddress addressRangeMax = I2cAddress.DeviceMaxValue;
// I2cAddress addressRangeMin = 0x20;
// I2cAddress addressRangeMax = 0x27;
const int TransmissionSpeedInKbps = 100;

var (writeAddressSet, readAddressSet) = await device.I2c.ScanBusAsync(
  addressRangeMin,
  addressRangeMax,
  TransmissionSpeedInKbps,
  scanBusProgress
);

foreach (var writeRead in new[] {
  new {Header = "[I2C write]", AddressSet = writeAddressSet},
  new {Header = "[I2C read]", AddressSet = readAddressSet},
}) {
  Console.WriteLine(writeRead.Header);

  Console.Write("    |");
  for (var hcol = 0x00; hcol <= 0x0F; hcol += 0x01) {
    Console.Write($"{hcol,3:X1} ");
  }
  Console.WriteLine();

  Console.WriteLine(new string('-', 72));

  for (var row = 0x00; row <= 0x70; row += 0x10) {
    Console.Write($"0x{row:X2}|");
    for (var col = 0x00; col <= 0x0F; col += 0x01) {
      var address = row | col;

      if (writeRead.AddressSet.Contains((I2cAddress)address))
        Console.Write($"{address,3:X2} ");
      else
        Console.Write($"{("--"),3} ");
    }
    Console.WriteLine();
  }

  Console.WriteLine();
}

