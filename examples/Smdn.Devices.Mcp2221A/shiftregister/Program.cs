// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221A.Create(serviceProvider);

// configure GP0-GP3 as GPIO output
device.GpPin0.ConfigureAsGpioOutput();
device.GpPin1.ConfigureAsGpioOutput();
device.GpPin2.ConfigureAsGpioOutput();

// construct shift register
var shiftRegister = new ShiftRegister(
  gpioLatch: device.GpPin0,
  gpioClock: device.GpPin1,
  gpioData: device.GpPin2
);

const int maxBits = 16;

for (;;) {
  for (var shift = 0; shift < maxBits; shift++) {
    var data = 0b1u << shift;

    Console.WriteLine($"0b_{Convert.ToString(data, 2)}");

    await shiftRegister.WriteAsync(data, Endianness.BigEndian, BitOrder.HSBFirst);

    await Task.Delay(100);
  }
}
