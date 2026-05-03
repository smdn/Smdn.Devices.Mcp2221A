// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// Use LibUsbDotNet version 3 (LGPL-3.0, alpha release)
// (Add `Smdn.IO.UsbHid.Providers.LibUsbDotNetV3` to PackageReference)
services.AddLibUsbDotNetV3UsbHid(
  configure: static (builder, options) => {
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
