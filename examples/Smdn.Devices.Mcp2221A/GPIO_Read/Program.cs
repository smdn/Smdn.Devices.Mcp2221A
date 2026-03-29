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
device.GpPins.ConfigureAllAsGpioInput();

// read GP0 value
var gp0Val = device.GpPin0.Read();

// read GP1 value as int (0 = LOW, 1 = HIGH)
int gp1Val = (int)device.GpPin1.Read();

// read GP2 value as byte (0 = LOW, 1 = HIGH)
byte gp2Val = (byte)device.GpPins[2].Read();

// read GP3 value as bool (false = LOW, true = HIGH)
bool gp3Val = (bool)device.GpPins[3].Read();

// read and display GP0-GP3 pin values every 20 ms
Console.Clear();

var initialCursorPosition = Console.GetCursorPosition();

const string ColumnFormat = "|{0,-6}|{1,-6}|{2,-6}|{3,-6}|";

var pinNames = string.Format(
  ColumnFormat,
  device.GpPin0.PinName,
  device.GpPin1.PinName,
  device.GpPin2.PinName,
  device.GpPin3.PinName
);

while (true) {
  // read GP0-GP3 values all at once
  var (gp0, gp1, gp2, gp3) = device.GpPins.Read();

  Console.SetCursorPosition(initialCursorPosition.Left, initialCursorPosition.Top);

  Console.WriteLine(pinNames);
  Console.WriteLine(
    ColumnFormat,
    gp0,
    gp1,
    gp2,
    gp3
  );

  Thread.Sleep(20);
}
