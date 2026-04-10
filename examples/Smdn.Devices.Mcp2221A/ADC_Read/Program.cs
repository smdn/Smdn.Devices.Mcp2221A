// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// configure GP1-GP3 as ADC input
device.GpPin1.ConfigureAsAdc();
device.GpPin2.ConfigureAsAdc();
device.GpPin3.ConfigureAsAdc(VoltageReferenceSource.Vdd);
// By specifying the VoltageReferenceSource, you can select and set
// the reference voltage for the ADC module. The reference voltage
// can be set to one of the following: VDD, 4.096 V, 2.048 V, 1.024 V,
// or off (0 V).
// Note that the reference voltage applies to the entire MCP2221A
// ADC module, not to individual ADC pins. Therefore, the following
// settings apply to all GP1–GP3.

// get the current reference voltage setting
var voltageReferenceSource = device.CurrentAdcReferenceSource;

// read GP1 ADC 10-bit raw analog value (0-1023)
var gp1Val = device.GpPin1.ReadAnalogRaw();

// The ReadAnalogRaw() method reads the values of all ADC pins
// in a single call. The value read most recently can be referenced
// using LastReadAnalogRawValue property.
var gp2Val = device.GpPin2.LastReadAnalogRawValue;
var gp3Val = device.GpPin3.LastReadAnalogRawValue;

// read and display GP1-GP3 pin values every 20 ms
Console.Clear();

var initialCursorPosition = Console.GetCursorPosition();

const string ColumnFormat = "|{0,-6}|{1,-6}|{2,-6}|";

var pinNames = string.Format(
  ColumnFormat,
  device.GpPin1.PinName,
  device.GpPin2.PinName,
  device.GpPin3.PinName
);

while (true) {
  // read GP1-GP3 values all at once
  var (gp1, gp2, gp3) = device.GpPins.ReadAnalogRaw();

  Console.SetCursorPosition(initialCursorPosition.Left, initialCursorPosition.Top);

  Console.WriteLine(pinNames);
  Console.WriteLine(
    ColumnFormat,
    gp1,
    gp2,
    gp3
  );

  Thread.Sleep(20);
}
