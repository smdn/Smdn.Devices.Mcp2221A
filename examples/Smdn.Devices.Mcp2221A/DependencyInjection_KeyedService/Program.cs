// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

// For multiple MCP2221A devices connected, assign a service
// key to each one and register different services.
const string ServiceKey1 = "MCP2221A-1";
const string ServiceKey2 = "MCP2221A-2";

// Configure the first MCP2221A to use the HidSharp backend.
services.AddHidSharpUsbHid(serviceKey: ServiceKey1);

// Configure the second MCP2221A to use the LibUsbDotNet
// version 3 backend.
services.AddLibUsbDotNetV3UsbHid(
  serviceKey: ServiceKey2,
  configure: static (builder, options) => {
    options.DebugLevel = LogLevel.Information;
  }
);

// Add a console logger and specify a different log level
// for each key.
services.AddLogging(
  builder => builder
    .AddSimpleConsole(static options => {
      options.SingleLine = true;
      options.IncludeScopes = true;
    })
    .AddFilter(ServiceKey1, LogLevel.Debug)
    .AddFilter(ServiceKey2, LogLevel.Warning)
);

// Assign different category names to the ILogger instances
// retrieved with each service key
services.AddKeyedSingleton<ILogger>(
  serviceKey: ServiceKey1,
  implementationFactory: (provider, key)
    => provider.GetService<ILoggerFactory>()?.CreateLogger(ServiceKey1)
);
services.AddKeyedSingleton<ILogger>(
  serviceKey: ServiceKey2,
  implementationFactory: (provider, key)
    => provider.GetService<ILoggerFactory>()?.CreateLogger(ServiceKey2)
);

using var serviceProvider = services.BuildServiceProvider();

// Create an Mcp2221AController instance using ServiceKey1 as the
// service key. Here, specify that the MCP2221A with serial
// number "9999999999" should be selected as the first instance.
await using var device1 = await Mcp2221AController.CreateAsync(
  serviceProvider: serviceProvider,
  serviceKey: ServiceKey1,
  usbHidDeviceFilter: null,
  mcp2221AFilter: info => info.SerialNumber == "9999999999"
);

// Create an Mcp2221AController instance using ServiceKey2 as the
// service key. Here, specify that the MCP2221A with serial
// number "8888888888" should be selected as the first instance.
await using var device2 = await Mcp2221AController.CreateAsync(
  serviceProvider: serviceProvider,
  serviceKey: ServiceKey2,
  usbHidDeviceFilter: null,
  mcp2221AFilter: info => info.SerialNumber == "8888888888"
);

Console.WriteLine($"{ServiceKey1}: {device1.SerialNumber} {device1.HidDevice.GetType()}");
Console.WriteLine($"{ServiceKey2}: {device2.SerialNumber} {device2.HidDevice.GetType()}");
