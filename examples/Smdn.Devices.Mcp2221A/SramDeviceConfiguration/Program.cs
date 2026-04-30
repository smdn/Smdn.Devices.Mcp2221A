// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

Console.WriteLine("[Hardware Information (Read-only)]");
Console.WriteLine($"{nameof(device.HardwareRevision)}: {device.HardwareRevision}");
Console.WriteLine($"{nameof(device.FirmwareRevision)}: {device.FirmwareRevision}");
Console.WriteLine();

Console.WriteLine("[USB Descriptor Strings (Stored in Flash memory)]");
Console.WriteLine($"{nameof(device.Manufacturer)}: {device.Manufacturer}");
Console.WriteLine($"{nameof(device.Product)}: {device.Product}");
Console.WriteLine($"{nameof(device.SerialNumber)}: {device.SerialNumber}");
Console.WriteLine($"{nameof(device.ChipFactorySerialNumber)}: {device.ChipFactorySerialNumber}");
Console.WriteLine();

Console.WriteLine("[Device Configurations (Currently loaded in SRAM)]");
Console.WriteLine($"{nameof(device.UsbVendorId)}: 0x{device.UsbVendorId:X4}");
Console.WriteLine($"{nameof(device.UsbProductId)}: 0x{device.UsbProductId:X4}");
Console.WriteLine($"{nameof(device.UsbCdcSerialNumberEnabled)}: {device.UsbCdcSerialNumberEnabled}");
Console.WriteLine($"{nameof(device.UsbPowerMode)}: {device.UsbPowerMode}");
Console.WriteLine($"{nameof(device.UsbRemoteWakeUpEnabled)}: {device.UsbRemoteWakeUpEnabled}");
Console.WriteLine($"{nameof(device.UsbRequestedCurrentAmount)}: {device.UsbRequestedCurrentAmount} mA");
Console.WriteLine($"{nameof(device.FlashWriteProtection)}: {device.FlashWriteProtection}");
Console.WriteLine();

// [Active USB HID Interface IDs]
// Displays the VID/PID of the USB HID interface currently recognized by the OS.
// These values typically match the SRAM settings shown above.
// However, even if the IDs in Flash memory are modified, both the SRAM settings
// and the active HID interface IDs remain unchanged until the device is reset
// and re-enumerated by the host.
Console.WriteLine("[Active USB HID Interface IDs]");
Console.WriteLine($"{nameof(device.HidDevice.VendorId)}: 0x{device.HidDevice.VendorId:X4}");
Console.WriteLine($"{nameof(device.HidDevice.ProductId)}: 0x{device.HidDevice.ProductId:X4}");
Console.WriteLine();

Console.WriteLine("[GP0-GP3 Configurations (Currently loaded in SRAM)]");
Console.WriteLine($"DAC Reference Voltage: {device.CurrentDacReferenceSource}");
Console.WriteLine($"DAC Output Value: {device.LastWriteAnalogRawValue}");
Console.WriteLine($"ADC Reference Voltage: {device.CurrentDacReferenceSource}");
Console.WriteLine($"Interrupt-on-change Trigger: {device.GpPin1.CurrentInterruptOnChangeTrigger}");
Console.WriteLine($"Clock Output Frequency: {device.GpPin1.CurrentClockOutputFrequency} ({device.GpPin1.CurrentClockOutputFrequencyInHz:N0} Hz)");
Console.WriteLine($"Clock Output Duty Cycle: {device.GpPin1.CurrentClockOutputDutyCycle} ({device.GpPin1.CurrentClockOutputDutyRatio:P0})");
Console.WriteLine();

const string GpPinConfigurationTableFormat = "|{0,-15}|{1,-20}|{2,-20}|{3,-20}|{4,-20}|";

Console.WriteLine(
  GpPinConfigurationTableFormat,
  string.Empty,
  device.GpPin0.PinName, device.GpPin1.PinName, device.GpPin2.PinName, device.GpPin3.PinName
);
Console.WriteLine(
  GpPinConfigurationTableFormat,
  "Function",
  device.GpPin0.CurrentFunction, device.GpPin1.CurrentFunction, device.GpPin2.CurrentFunction, device.GpPin3.CurrentFunction
);
Console.WriteLine(
  GpPinConfigurationTableFormat,
  "Designation",
  device.GpPin0.CurrentDesignation, device.GpPin1.CurrentDesignation, device.GpPin2.CurrentDesignation, device.GpPin3.CurrentDesignation
);
Console.WriteLine(
  GpPinConfigurationTableFormat,
  "GPIO Direction",
  device.GpPin0.CurrentFunction == GpFunction.Gpio ? device.GpPin0.CurrentMode : null,
  device.GpPin1.CurrentFunction == GpFunction.Gpio ? device.GpPin1.CurrentMode : null,
  device.GpPin2.CurrentFunction == GpFunction.Gpio ? device.GpPin2.CurrentMode : null,
  device.GpPin3.CurrentFunction == GpFunction.Gpio ? device.GpPin3.CurrentMode : null
);
