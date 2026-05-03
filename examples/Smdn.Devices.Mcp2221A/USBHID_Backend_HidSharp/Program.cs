// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// Use HidSharp (Apache License 2.0)
// (Add `Smdn.IO.UsbHid.Providers.HidSharp` to PackageReference)
services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221AController.CreateAsync(serviceProvider);

// You can retrieve the HidSharp device object (IUsbHidDevice) used by
// the Mcp2221AController via the HidDevice property.
// For more information about IUsbHidDevice, see https://github.com/smdn/Smdn.IO.UsbHid/
// or 'USBHID_SelectDevice' example.
Console.WriteLine($"{nameof(device.HidDevice)}: {device.HidDevice}");
Console.WriteLine();

Console.WriteLine($"{nameof(device.Manufacturer)}: {device.Manufacturer}");
Console.WriteLine($"{nameof(device.Product)}: {device.Product}");
Console.WriteLine($"{nameof(device.SerialNumber)}: {device.SerialNumber}");
