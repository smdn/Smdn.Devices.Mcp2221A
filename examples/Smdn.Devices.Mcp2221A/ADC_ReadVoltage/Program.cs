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
device.GpPin3.ConfigureAsAdc(VoltageReferenceSource.Vrm2048);

// read and display GP1-GP3 pin values every 20 ms
Console.Clear();

var initialCursorPosition = Console.GetCursorPosition();

const string HeaderColumnFormat = "|{0,-7}|{1,-7}|{2,-7}|";
const string ValueColumnFormat = "|{0,-6:N3}V|{1,-6:N3}V|{2,-6:N3}V|";

var pinNames = string.Format(
  HeaderColumnFormat,
  device.GpPin1.PinName,
  device.GpPin2.PinName,
  device.GpPin3.PinName
);

while (true) {
  // Reads GP1-GP3 voltages all at once.
  // Note that the ReadAnalogVoltage() method can only be used when VRM is
  // set as the reference voltage. Calling this method when VDD is set
  // as the reference voltage will result in an InvalidOperationException
  // being thrown.
  var (gp1Voltage, gp2Voltage, gp3Voltage) = device.GpPins.ReadAnalogVoltage();

  Console.SetCursorPosition(initialCursorPosition.Left, initialCursorPosition.Top);

  Console.WriteLine($"ADC voltage reference: {device.CurrentAdcReferenceSource}");
  Console.WriteLine(pinNames);
  Console.WriteLine(
    ValueColumnFormat,
    gp1Voltage,
    gp2Voltage,
    gp3Voltage
  );

  Thread.Sleep(20);
}
