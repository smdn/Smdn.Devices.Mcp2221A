// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221A.Create(serviceProvider);

// configure GP0-GP2 as GPIO output and set to LOW
device.GpPins.ConfigureAllAsGpio(
  gp0Mode: PinMode.Output,
  gp1Mode: PinMode.Output,
  gp2Mode: PinMode.Output,
  gp0InitialValue: PinValue.Low,
  gp1InitialValue: PinValue.Low,
  gp2InitialValue: PinValue.Low
);

// construct shift register
var shiftRegister = new ShiftRegister(
  gpioLatch: device.GpPin0, // GP0 for RCLK/ST_CP
  gpioClock: device.GpPin1, // GP1 for SRCLK/SH_CP
  gpioData: device.GpPin2 // GP2 for SER
);

const int MaxBits = 16;
var bitDataFormat = $"B{MaxBits}";

for (;;) {
  for (var shift = 0; shift < MaxBits; shift++) {
    var data = 0b1u << shift;

    Console.WriteLine($"0b_{data.ToString(bitDataFormat)}");

    await shiftRegister.WriteAsync(data, Endianness.BigEndian, BitOrder.HsbFirst);

    await Task.Delay(100);
  }
}
