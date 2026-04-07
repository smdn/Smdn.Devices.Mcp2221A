// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;

using Iot.Device.Multiplexing;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// construct ShiftRegister
const int MaxBits = 16;

using var shiftRegister = new ShiftRegister(
  pinMapping: new ShiftRegisterPinMapping(
    latchEnable: 0, // GP0 for RCLK/ST_CP
    clock: 1, // GP1 for SRCLK/SH_CP
    serialData: 2 // GP2 for SER
  ),
  bitLength: MaxBits,
  gpioController: device.GpioController,
  shouldDispose: false
);

shiftRegister.ShiftClear();

var bitDataFormat = $"B{MaxBits}";

for (;;) {
  for (var shift = 0; shift < MaxBits; shift++) {
    var data = 0b1u << shift;

    Console.WriteLine($"0b_{data.ToString(bitDataFormat)}");

    shiftRegister.ShiftByte((byte)((data >> 8) & 0xFF), latch: false);
    shiftRegister.ShiftByte((byte)(data & 0xFF), latch: true);

    Thread.Sleep(100);
  }
}

