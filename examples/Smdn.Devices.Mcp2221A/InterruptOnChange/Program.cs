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

// Configure GP1 as an Interrupt-on-Change.
device.GpPin1.ConfigureAsInterruptOnChange(
  // By specifying `InterruptOnChangeTrigger`, you can specify
  // which edge (rising, falling, or both) to detect.
  detectionTrigger: InterruptOnChangeTrigger.Both,
  // You can also specify whether to clear the detection status
  // at the same time as the GP1 configuration.
  clearDetectionFlag: true
);

// Get the currently configured trigger condition for the
// Interrupt-on-Change.
Console.WriteLine(device.GpPin1.CurrentInterruptOnChangeTrigger);

// Get a value indicating whether an interrupt-on-change event was
// detected or not.
// Note: This property returns false unless the latest state is
// fetched by ReadInterruptDetection().
Console.WriteLine(device.GpPin1.LastReadInterruptDetectionFlag);

for (;;) {
  // Reads the latest detection status.
  // Note: The MCP2221A cannot determine whether a rising or falling edge was detected.
  if (device.GpPin1.ReadInterruptDetection()) {
    Console.WriteLine($"Detected a change in the GP1 input. (Trigger: {device.GpPin1.CurrentInterruptOnChangeTrigger})");

    // Clear the current detection status.
    // Note: Unless explicitly cleared, ReadInterruptDetection() will
    // continue to return the detected status.
    device.GpPin1.ClearInterruptDetection();
  }
}
