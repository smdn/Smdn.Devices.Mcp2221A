// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221A.Create(serviceProvider);

Console.WriteLine("[MCP2221 Device information]");

if (device.HidDevice.TryGetSerialNumber(out var serialNumber))
  Console.WriteLine($"Serial number: {serialNumber}");

Console.WriteLine($"USB Manufacturer descriptor: {device.Manufacturer}");
Console.WriteLine($"USB Product descriptor: {device.Product}");
Console.WriteLine($"USB Serial number descriptor: {device.SerialNumber}");
Console.WriteLine($"Hardware revision: {device.HardwareRevision}");
Console.WriteLine($"Firmware revision: {device.FirmwareRevision}");
Console.WriteLine();

// configure GP0-GP3 as GPIO output
device.GpPin0.ConfigureAsGpioOutput();
device.GpPin1.ConfigureAsGpioOutput();
device.GpPin2.ConfigureAsGpioOutput();
device.GpPin3.ConfigureAsGpioOutput(initialValue: PinValue.Low); // initial value also can be specified

// set GPIO pin values
Console.WriteLine("set all GPs HIGH");

device.GpPins[0].Write(1); // set GP0 to HIGH with integer value (0 = LOW, any other value = HIGH)

device.GpPins[1].Write(true); // set GP1 to HIGH with boolean value

device.GpPin2.Write((byte)1); // set GP2 to HIGH with byte value

PinValue gp3Value = (PinValue)1;

device.GpPin3.Write(gp3Value); // set GP3 to HIGH with struct PinValue

Thread.Sleep(1000);

Console.WriteLine("set all GPs LOW");

// GP0-GP3 also can be accessed via `GpPins` read-only collection property
foreach (var gp in device.GpPins) {
  gp.Write(PinValue.Low);
}

Thread.Sleep(1000);

Console.WriteLine("set all GPs");

device.GpPins.Write(PinValue.High, PinValue.High, PinValue.High, PinValue.High);

Thread.Sleep(1000);

device.GpPins.Write(PinValue.Low, PinValue.Low, PinValue.Low, PinValue.Low);

// blink GP0-GP3
foreach (var gp in device.GpPins) {
  Console.WriteLine($"blink {gp.PinName}");

  for (var n = 0; n < 10; n++) {
    gp.Write(false);
    Thread.Sleep(100);

    gp.Write(true);
    Thread.Sleep(100);
  }
}
