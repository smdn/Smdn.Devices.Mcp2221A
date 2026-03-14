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
device.GP0.ConfigureAsGpio(PinMode.Output);
device.GP1.ConfigureAsGpio(PinMode.Output);
device.GP2.ConfigureAsGpio(PinMode.Output);
device.GP3.ConfigureAsGpio(PinMode.Output, initialValue: PinValue.Low); // initial value also can be specified

// set GPIO pin values
Console.WriteLine("set all GPs HIGH");

device.GPs[0].SetValue(1); // set GP0 to HIGH with integer value (0 = LOW, any other value = HIGH)

device.GPs[1].SetValue(true); // set GP1 to HIGH with boolean value

device.GP2.SetValue((byte)1); // set GP2 to HIGH with byte value

PinValue gp3Value = (PinValue)1;

device.GP3.SetValue(gp3Value); // set GP3 to HIGH with struct PinValue

Thread.Sleep(1000);

Console.WriteLine("set all GPs LOW");

// GP0-GP3 also can be accessed via `GPs` read-only collection property
foreach (var gp in device.GPs) {
  gp.SetValue(PinValue.Low);
}

Thread.Sleep(1000);

// blink GP0-GP3
foreach (var gp in device.GPs) {
  Console.WriteLine($"blink {gp.PinDesignation}");

  for (var n = 0; n < 10; n++) {
    gp.SetValue(false);
    Thread.Sleep(100);

    gp.SetValue(true);
    Thread.Sleep(100);
  }
}
