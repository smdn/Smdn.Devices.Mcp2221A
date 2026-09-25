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

// The settings stored in flash memory are loaded when an instance
// of the `Mcp2221AController` is created and are persistently retained
// within the instance. These settings can be accessed via the
// `Mcp2221AController.Flash` property.
//
// Although the settings written to Flash memory can be changed using
// the `ModifyXxx()` methods, you can always use the `Restore()` method
// to return them to their initial state -- that is, the settings they
// had when the instance was created.
using var device = Mcp2221AController.Create(serviceProvider);

// CHIPSETTING0 register
Console.WriteLine("[CHIPSETTING0]");
Console.WriteLine($"{nameof(device.Flash.CdcSerialNumberEnumerationEnabled)} (CDCSNEN): {device.Flash.CdcSerialNumberEnumerationEnabled}");
Console.WriteLine($"{nameof(device.Flash.WriteProtectionLevel)} (CHIPPROT): {device.Flash.WriteProtectionLevel}");
Console.WriteLine();

// CHIPSETTING1 register
Console.WriteLine("[CHIPSETTING1]");
Console.WriteLine($"{nameof(device.Flash.ClockOutputDutyCycle)} (CLKDC): {device.Flash.ClockOutputDutyCycle}");
Console.WriteLine($"{nameof(device.Flash.ClockOutputFrequency)} (CLKDIV): {device.Flash.ClockOutputFrequency}");
Console.WriteLine();

// CHIPSETTING2 register
Console.WriteLine("[CHIPSETTING2]");
Console.WriteLine($"{nameof(device.Flash.DacVoltageReference)} (DACVRM/DACREF): {device.Flash.DacVoltageReference}");
Console.WriteLine($"{nameof(device.Flash.DacInitialValue)} (DACVAL): {device.Flash.DacInitialValue}");
Console.WriteLine();

// CHIPSETTING3 register
Console.WriteLine("[CHIPSETTING3]");
Console.WriteLine($"{nameof(device.Flash.InterruptOnChangeTrigger)} (INTDETFEEN/INTDETREEN): {device.Flash.InterruptOnChangeTrigger}");
Console.WriteLine($"{nameof(device.Flash.AdcVoltageReference)} (ADCVRM/ADCREF): {device.Flash.AdcVoltageReference}");
Console.WriteLine();

// USBVIDL/USBVIDH register
Console.WriteLine("[USBVIDL/USBVIDH]");
Console.WriteLine($"{nameof(device.Flash.UsbVendorId)} (USBVIDL/USBVIDH): 0x{device.Flash.UsbVendorId:X4}");
Console.WriteLine();

// USBPIDL/USBPIDH register
Console.WriteLine("[USBPIDL/USBPIDH]");
Console.WriteLine($"{nameof(device.Flash.UsbProductId)} (USBPIDL/USBPIDH): 0x{device.Flash.UsbProductId:X4}");
Console.WriteLine();

// USBPWRATTR register
Console.WriteLine("[USBPWRATTR]");
Console.WriteLine($"{nameof(device.Flash.UsbPowerMode)} (SELFPWR): {device.Flash.UsbPowerMode}");
Console.WriteLine($"{nameof(device.Flash.UsbRemoteWakeUpEnabled)} (REMWKUP): {device.Flash.UsbRemoteWakeUpEnabled}");
Console.WriteLine();

// USBREQCRT register
Console.WriteLine("[USBREQCRT]");
Console.WriteLine($"{nameof(device.Flash.UsbRequestedCurrentAmount)} (USBREQCRT): {device.Flash.UsbRequestedCurrentAmount} [mA]");
Console.WriteLine();

// GPSETTING0-GPSETTING3 register
foreach (var gpSetting in device.Flash.GpPins) {
  var (index, designation, function, gpioMode, gpioOutputValue) = gpSetting;

  Console.WriteLine($"[GPSETTING{index}]");
  Console.WriteLine($"{nameof(gpSetting.Designation)} (GPDES): 0b{designation:B3} ({function})");
  Console.WriteLine($"{nameof(gpSetting.GpioMode)} (GPIODIR): {gpioMode}");
  Console.WriteLine($"{nameof(gpSetting.GpioOutputValue)} (GPIOOUTVAL): {gpioOutputValue}");
  Console.WriteLine();
}

Console.WriteLine($"USB Manufacturer Descriptor String: {device.Flash.GetUsbManufacturerString()}");
Console.WriteLine($"USB Product Descriptor String: {device.Flash.GetUsbProductString()}");
Console.WriteLine($"USB Serial Number Descriptor String: {device.Flash.GetUsbSerialNumberString()}");
Console.WriteLine($"Chip Factory Serial Number: {device.Flash.GetChipFactorySerialNumber()}");
Console.WriteLine();

// If there are settings that have been changed since the
// instance was created but have not yet been written to
// Flash memory, `IsDirty` will be `true`.
Console.WriteLine($"IsDirty: {device.Flash.IsDirty}");


