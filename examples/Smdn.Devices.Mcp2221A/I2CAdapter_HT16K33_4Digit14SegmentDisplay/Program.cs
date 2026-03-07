// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid.DependencyInjection;

using Iot.Device.Display;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221A.CreateAsync(serviceProvider);

await device.GP3.ConfigureAsLedI2cAsync();

// If an I2cCommandException is thrown when using the
// HidSharp backend, try WithFastMode() instead of WithStandardMode().
var i2cDevices = new[] {
  device.I2c.CreateDevice(Ht16k33.DefaultI2cAddress | 0b_000).WithStandardMode(),
  device.I2c.CreateDevice(Ht16k33.DefaultI2cAddress | 0b_001).WithStandardMode(),
};

FourDigitFourteenSegmentDisplay[] displays = [
  new(i2cDevices[0]) { Brightness = Ht16k33.MaxBrightness, BufferingEnabled = true },
  new(i2cDevices[1]) { Brightness = Ht16k33.MaxBrightness, BufferingEnabled = true },
];

// clear display
displays[0].Clear();
displays[1].Clear();

displays[0].Flush();
displays[1].Flush();

// write string and display it
const int numOfDigits = FourDigitFourteenSegmentDisplay.NumberOfDigits;
var str = $"    Hello, MCP2221/MCP2221A. {device.GetType().Assembly.GetName().Name} {device.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}    ";

Console.WriteLine(str);

for (var i = 0; i < str.Length; i++) {
  displays[0].Clear();
  displays[1].Clear();

  if (i < str.Length)
    displays[0].Write(str.AsSpan(i));

  if (i + numOfDigits < str.Length)
    displays[1].Write(str.AsSpan(i + numOfDigits));

  displays[0].Flush();
  displays[1].Flush();

  await Task.Delay(200);
}
