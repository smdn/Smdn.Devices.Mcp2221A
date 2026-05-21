// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// Use LibUsbDotNet version 2 (LGPL-3.0)
// (Add `Smdn.IO.UsbHid.Providers.LibUsbDotNet` to PackageReference)
services.AddLibUsbDotNetUsbHid(
  configure: static (builder, options) => {
    // Specify the filename of the libusb-1.0 library installed on your
    // system or placed in the output directory.
    options.LibUsbLibraryPath = "libusb-1.0.so.0"; // Linux
    // options.LibUsbLibraryPath = "libusb-1.0.dll"; // Windows
    // options.LibUsbLibraryPath = "libusb-1.0.dylib"; // MacOS

    // You can control the logs output by libusb by changing the
    // DebugLevel property.
    options.DebugLevel = LogLevel.None;
  }
);

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221AController.CreateAsync(serviceProvider);

// You can retrieve the LibUsbDotNet device object (IUsbHidDevice) used by
// the Mcp2221AController via the HidDevice property.
// For more information about IUsbHidDevice, see https://github.com/smdn/Smdn.IO.UsbHid/
// or 'USBHID_SelectDevice' example.
Console.WriteLine($"{nameof(device.HidDevice)}: {device.HidDevice}");
Console.WriteLine();

Console.WriteLine($"{nameof(device.Manufacturer)}: {device.Manufacturer}");
Console.WriteLine($"{nameof(device.Product)}: {device.Product}");
Console.WriteLine($"{nameof(device.SerialNumber)}: {device.SerialNumber}");
