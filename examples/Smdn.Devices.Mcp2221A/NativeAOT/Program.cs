// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// Use HidApi.Net (MIT License)
// (Add `Smdn.IO.UsbHid.Providers.HidApiDotNet` to PackageReference)
services.AddHidApiDotNetUsbHid();
// Note that although HidApi.Net claims to be AOT-compatible, the assembly's
// metadata does not include `IsAotCompatible=true`, so warning IL3058 is
// reported during AOT compilation.

var serviceProvider = services.BuildServiceProvider();

// Find and open the first MCP2221 device connected to the USB port.
using var device = Mcp2221AController.Create(serviceProvider);

// Configure the all GP pins (GP0-GP3) as GPIO output.
device.GpPins.ConfigureAllAsGpioOutput();

// Blink the configured GPIO pins.
foreach (var gp in device.GpPins) {
  Console.WriteLine($"Blinking {gp.PinName}");

  for (var n = 0; n < 10; n++) {
    gp.Write(false);
    Thread.Sleep(100);

    gp.Write(true);
    Thread.Sleep(100);
  }
}

