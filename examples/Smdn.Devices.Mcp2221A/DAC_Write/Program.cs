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

// Configure GP2 and GP3 as DAC output.
device.GpPin2.ConfigureAsDac();

// By specifying the VoltageReferenceSource, you can select and set
// the reference voltage for the DAC module. The reference voltage
// can be set to one of the following: VDD, 4.096 V, 2.048 V, 1.024 V,
// or off (0 V).
// Note that the reference voltage and the output value applies to
// the entire MCP2221A DAC module, not to individual DAC pins.
// Therefore, the following settings apply to both GP2 and GP3.
device.GpPin3.ConfigureAsDac(VoltageReferenceSource.Vrm2048);

// Get the current reference voltage setting.
Console.WriteLine($"{nameof(device.CurrentDacReferenceSource)}: {device.CurrentDacReferenceSource}");

// Write DAC 5-bit raw analog value (0-31).
// Since the ConfigureAsDac() method specifies 2.048 V as the
// reference voltage, approximately 2.048 V will be output
// from the pin assigned to the DAC. Additionally, because
// the DAC output voltage is shared between GP2 and GP3,
// both pins are set to the same voltage.
device.GpPin2.WriteAnalogRaw(31);

// Get the last DAC output value set.
Console.WriteLine($"{nameof(device.LastWriteAnalogRawValue)}: {device.LastWriteAnalogRawValue}");

Thread.Sleep(1000);

// Set the output values for all DAC output pins by specifying
// the voltage value [V]. Since the DAC has a resolution of
// 5 bits, the resulting voltage values may not be as accurate
// as expected.
device.GpPins.WriteAnalogVoltage(1.0); // 1.0 [V]
