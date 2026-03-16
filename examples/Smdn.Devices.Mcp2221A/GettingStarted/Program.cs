using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// To operate the MCP2221/MCP2221A, you need to select one of
// the following as the USB HID backend:

// Use HidSharp (Apache License 2.0)
// (Add `Smdn.IO.UsbHid.Providers.HidSharp` to PackageReference)
services.AddHidSharpUsbHid();

// Use LibUsbDotNet version 3 (LGPL-3.0, alpha release)
// (Add `Smdn.IO.UsbHid.Providers.LibUsbDotNetV3` to PackageReference)
/*
services.AddLibUsbDotNetV3UsbHid(
  configure: static (builder, options) => {
    options.DebugLevel = LogLevel.None;
  }
);
*/

// Use LibUsbDotNet version 2 (LGPL-3.0, stable release)
// (Add `Smdn.IO.UsbHid.Providers.LibUsbDotNet` to PackageReference)
/*
services.AddLibUsbDotNetUsbHid(
  configure: static (builder, options) => {
    options.DebugLevel = LogLevel.None;
    // Specify the filename of the libusb-1.0 library installed on your
    // system or placed in the output directory.
    options.LibUsbLibraryPath = "libusb-1.0.so.0"; // Linux
    // options.LibUsbLibraryPath = "libusb-1.0.dll"; // Windows
    // options.LibUsbLibraryPath = "libusb-1.0.dylib"; // MacOS
  }
);
*/

using var serviceProvider = services.BuildServiceProvider();

// Find and open the first MCP2221 device connected to the USB port.
using var device = Mcp2221A.Create(serviceProvider);

// Configure the GP pins (GP0-GP3) as GPIO output.
device.GpPin0.ConfigureAsGpioOutput();
device.GpPin1.ConfigureAsGpioOutput();
device.GpPin2.ConfigureAsGpioOutput();
device.GpPin3.ConfigureAsGpioOutput();

// Blink the configured GPIO pins.
//
// This example assumes an LED is connected to each pin.
// See this code in action in the YouTube video:
// https://www.youtube.com/watch?v=MnIunESm71E
foreach (var gp in device.GpPins) {
  Console.WriteLine($"Blinking {gp.PinName}");

  for (var n = 0; n < 10; n++) {
    // Set the pin output to Low (logic 0)
    gp.Write(false);
    Thread.Sleep(100);

    // Set the pin output to High (logic 0)
    gp.Write(true);
    Thread.Sleep(100);
  }
}
