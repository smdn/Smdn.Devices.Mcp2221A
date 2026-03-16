// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221A.Create(serviceProvider);

// configure GP0-GP3 as GPIO input
device.GpPin0.ConfigureAsGpioInput();
device.GpPin1.ConfigureAsGpioInput();
device.GpPin2.ConfigureAsGpioInput();
device.GpPin3.ConfigureAsGpioInput();

// read GP0 value
var gp0Val = device.GpPin0.Read();

// read GP1 value as int (0 = LOW, 1 = HIGH)
int gp1Val = (int)device.GpPin1.Read();

// read GP2 value as byte (0 = LOW, 1 = HIGH)
byte gp2Val = (byte)device.GpPins[2].Read();

// read GP3 value as bool (false = LOW, true = HIGH)
bool gp3Val = (bool)device.GpPins[3].Read();

// read and display GP0-GP3 pin value every 20 ms
var initialCursorPosition = (left: Console.CursorLeft, top: Console.CursorTop);

while (true) {
  Console.SetCursorPosition(initialCursorPosition.left, initialCursorPosition.top);

  Console.WriteLine(string.Join("\t", device.GpPins.Select(gp => gp.PinName)));
  Console.WriteLine(string.Join("\t", device.GpPins.Select(gp => (bool)gp.Read() ? "H" : "L")));

  Thread.Sleep(20);
}
