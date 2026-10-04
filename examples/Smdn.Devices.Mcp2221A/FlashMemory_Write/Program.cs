// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Text;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Configurations;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, _) => cts.Cancel();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// -----------------------------------------------------------------------------------------
// STEP 1: Unlock hardware Flash write access on the physical device.
// -----------------------------------------------------------------------------------------
// Perform necessary setup based on the Flash write protection level evaluated when the
// instance was created.
// [IMPORTANT]
// To check the write protection level at the time of instance creation, refer to the
// `Mcp2221AController.FlashWriteProtection` property.
// The `Flash.WriteProtectionLevel` property retrieves the currently staged protection
// level, which may have been changed by calling `ModifyWriteProtection()`.
if (device.FlashWriteProtection == DeviceConfigurationProtectionLevel.PermanentlyLocked) {
  Console.Error.WriteLine("The device Flash is permanently locked. No write operations can be performed.");
  return;
}
else if (device.FlashWriteProtection == DeviceConfigurationProtectionLevel.PasswordProtected) {
  Console.WriteLine("This MCP2221A Flash memory write access is password-protected.");
  Console.Write("Enter the 8-byte access password to unlock write protection: ");

  var accessPassword = Encoding.ASCII.GetBytes(Console.ReadLine() ?? string.Empty);

  // Entering an incorrect password here will not immediately result in an error or
  // exception; whether write protection was successfully unlocked remains unknown
  // until an actual write occurs.
  // If write protection is not unlocked, calling `Flash.Write()`/`WriteAsync()` will
  // throw a `FlashWriteAccessException`.
  await device.Flash.SendAccessPasswordAsync(accessPassword, cts.Token);
}

// -----------------------------------------------------------------------------------------
// STEP 2: Stage configuration changes and set the Flash password buffer.
// -----------------------------------------------------------------------------------------
// Flash memory configuration changes are staged using methods starting with
// `Modify` on the `Flash` property.
// These changes are not applied immediately to the physical Flash memory;
// instead, they are staged in the local machine's memory.
device
  .Flash
  .ModifyWriteProtection(DeviceConfigurationProtectionLevel.PasswordProtected)
  .ModifyCdcSerialNumberEnumeration(enabled: true)
  .ModifyClockOutputFrequency(ClockOutputFrequency.Frequency750kHz)
  .ModifyClockOutputDutyCycle(ClockOutputDutyCycle.Duty50)
  .ModifyDacVoltageReference(VoltageReferenceSource.Vrm2048)
  .ModifyDacInitialValue(16) // The DAC output voltage is set based on this value and VRM/VDD settings.
  .ModifyAdcVoltageReference(VoltageReferenceSource.Vrm2048)
  .ModifyInterruptOnChangeTrigger(InterruptOnChangeTrigger.Rising)
  .ModifyUsbPowerMode(UsbPowerMode.BusPowered)
  .ModifyUsbRemoteWakeUp(enabled: false)
  .ModifyUsbRequestedCurrentAmount(500) // [mA]
  .ModifyGpSetting(0, GpFunction.Gpio, PinMode.Output, PinValue.High)
  .ModifyGpSetting(1, GpFunction.Gpio, PinMode.Output)
  .ModifyGpSetting(2, GpFunction.Dac)
  .ModifyGpSetting(3, GpFunction.Adc)
  .ModifyUsbManufacturerString("Smdn.Devices.Mcp2221A")
  .ModifyUsbProductString("MCP2221A Flash write sample")
  .ModifyUsbSerialNumberString("SN-0123456789");

// [IMPORTANT]
// Changing the USB Vendor ID (VID) and Product ID (PID) must be done with extreme caution.
// Specifying arbitrary VIDs/PIDs without a specific requirement is strongly discouraged due to:
// 1. USB Specification Constraints (USB-IF Regulations):
//    Vendor ID (VID) is an identifier assigned and managed by USB-IF. There are no freely
//    available "test" or "unreserved" VIDs, and using an unauthorized VID violates USB
//    specifications.
// 2. Conflict with Other Devices and System Instability:
//    If the VID/PID collides with existing commercial devices or third-party products,
//    the OS may assign incorrect drivers, hindering device operation or causing
//    system-wide instability.
// 3. Risk of Inoperability and Brick-State Recovery Failure:
//    Changing to an unrecognized VID/PID can prevent tools and libraries from locating or
//    identifying the device, making re-configuration or recovery extremely difficult.
//
// Recommended Practices for VID/PID Configuration:
// - For personal development or testing, keeping the default VID (0x04D8) / PID (0x00DD)
//   is strongly recommended.
// - If PID modification is required for custom products, officially apply for Microchip's
//   free PID Sub-licensing Program and use the legitimately assigned PID.
#if false
device
  .Flash
  .ModifyUsbVendorId(...)
  .ModifyUsbProductId(...);
#endif

if (device.Flash.WriteProtectionLevel == DeviceConfigurationProtectionLevel.PasswordProtected) {
  Console.WriteLine("Protecting MCP2221A Flash memory write access with a password.");
  Console.Write("Enter the password (exact 8 bytes) to enable write protection: ");

  var writeProtectionPassword = Encoding.ASCII.GetBytes(Console.ReadLine() ?? string.Empty);

  // If the password is not exactly 8 bytes long, the following call will
  // throw an `ArgumentException`.
  device.Flash.ModifyPassword(writeProtectionPassword);

  // Why two password calls are required (for `PasswordProtected` devices):
  // 1. `SendAccessPassword()`: An immediate device command that authenticates with
  //    the physical MCP2221A hardware to temporarily unlock Flash write access.
  // 2. `ModifyPassword()`: A staging method that prepares the 8-byte password
  //    payload for the 64-byte monolithic 'Write Chip Settings' Flash command.

  // The MCP2221A writes protection settings and password bytes simultaneously
  // in a single command, but existing passwords cannot be read back from the device.
  // To prevent accidentally overwriting the Flash password with indeterminate
  // internal buffer data (which would permanently lock you out), the library requires
  // an explicit `ModifyPassword()` call whenever PasswordProtected level is active.
}

// Currently staged configuration changes can be inspected using the following properties and methods:
Console.WriteLine("The following settings are currently staged:");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"CHIPSETTING0 CDCSNEN: {nameof(device.Flash.CdcSerialNumberEnumerationEnabled)} = {device.Flash.CdcSerialNumberEnumerationEnabled}");
Console.WriteLine($"CHIPSETTING0 CHIPPROT: {nameof(device.Flash.WriteProtectionLevel)} = {device.Flash.WriteProtectionLevel}");
Console.WriteLine($"CHIPSETTING1 CLKDC: {nameof(device.Flash.ClockOutputDutyCycle)} = {device.Flash.ClockOutputDutyCycle}");
Console.WriteLine($"CHIPSETTING1 CLKDIV: {nameof(device.Flash.ClockOutputFrequency)} = {device.Flash.ClockOutputFrequency}");
Console.WriteLine($"CHIPSETTING2 DACVRM/DACREF: {nameof(device.Flash.DacVoltageReference)} = {device.Flash.DacVoltageReference}");
Console.WriteLine($"CHIPSETTING2 DACVAL: {nameof(device.Flash.DacInitialValue)} = {device.Flash.DacInitialValue}");
Console.WriteLine($"CHIPSETTING3 INTDETFEEN/INTDETREEN: {nameof(device.Flash.InterruptOnChangeTrigger)} = {device.Flash.InterruptOnChangeTrigger}");
Console.WriteLine($"CHIPSETTING3 ADCVRM/ADCREF: {nameof(device.Flash.AdcVoltageReference)} = {device.Flash.AdcVoltageReference}");
Console.WriteLine($"USBVIDL/USBVIDH: {nameof(device.Flash.UsbVendorId)} = 0x{device.Flash.UsbVendorId:X4}");
Console.WriteLine($"USBPIDL/USBPIDH: {nameof(device.Flash.UsbProductId)} = 0x{device.Flash.UsbProductId:X4}");
Console.WriteLine($"USBPWRATTR SELFPWR: {nameof(device.Flash.UsbPowerMode)} = {device.Flash.UsbPowerMode}");
Console.WriteLine($"USBPWRATTR REMWKUP: {nameof(device.Flash.UsbRemoteWakeUpEnabled)} = {device.Flash.UsbRemoteWakeUpEnabled}");
Console.WriteLine($"USBREQCRT: {nameof(device.Flash.UsbRequestedCurrentAmount)} = {device.Flash.UsbRequestedCurrentAmount} [mA]");
Console.WriteLine();

foreach (var gpSetting in device.Flash.GpPins) {
  var (index, designation, function, gpioMode, gpioOutputValue) = gpSetting;

  Console.WriteLine($"GPSETTING{index} GPDES: {nameof(gpSetting.Function)} = {function} (0b{designation:B3})");
  Console.WriteLine($"GPSETTING{index} GPIODIR: {nameof(gpSetting.GpioMode)} = {gpioMode}");
  Console.WriteLine($"GPSETTING{index} GPIOOUTVAL: {nameof(gpSetting.GpioOutputValue)} = {gpioOutputValue}");
  Console.WriteLine();
}

Console.WriteLine($"USB Manufacturer Descriptor String: {device.Flash.GetUsbManufacturerString()}");
Console.WriteLine($"USB Product Descriptor String: {device.Flash.GetUsbProductString()}");
Console.WriteLine($"USB Serial Number Descriptor String: {device.Flash.GetUsbSerialNumberString()}");
Console.WriteLine(new string('=', 60));

if (cts.Token.IsCancellationRequested) {
  Console.Error.WriteLine("Aborted.");
  return;
}

// Compare staged changes with the initial configuration read at instance creation.
// If there are differences between staged settings and initial settings, IsDirty will be true.
if (!device.Flash.IsDirty) {
  Console.WriteLine("No configuration changes to write. Exiting without write operation.");
  return;
}

// Display a critical warning if the staged protection level is set to `PermanentlyLocked`.
// Once committed, the Flash configuration on the physical MCP2221A becomes permanently
// immutable and can NEVER be modified again.
if (device.Flash.WriteProtectionLevel == DeviceConfigurationProtectionLevel.PermanentlyLocked) {
  Console.WriteLine("⚠️ WARNING ⚠️");
  Console.WriteLine("Writing settings with 'PermanentlyLocked' protection level will permanently lock the device.");
  Console.WriteLine("Once applied, Flash memory configurations can NEVER be modified again.");
  Console.WriteLine();
}

Console.Write("Will write the above settings to the flash memory. Continue? (y/N) ");

var answerForContinueWriting = Console.ReadLine();

if (!"Y".Equals(answerForContinueWriting, StringComparison.OrdinalIgnoreCase)) {
  Console.Error.WriteLine("Aborted.");

  // To discard staged changes and restore settings back to the state when the
  // instance was created, call the `Restore()` method:

  // device.Flash.Restore();

  return;
}

// -----------------------------------------------------------------------------------------
// STEP 3: Persist staged settings to Flash memory.
// -----------------------------------------------------------------------------------------
// [IMPORTANT]
// Writing to the MCP2221A Flash memory has a maximum endurance limit (hardware restriction).
// Once this limit is reached, further write operations will fail.
// Although the exact maximum write endurance count is not explicitly specified in the
// datasheet, write operations should be designed to minimize Flash write calls.
Console.WriteLine("Writing staged settings...");

try {
  // Write the staged configuration changes to the MCP2221A physical device by sending commands.
  await device.Flash.WriteAsync(cts.Token);

  Console.WriteLine("Writing to the Flash memory is complete.");
}
catch (FlashWriteAccessException ex) {
  // Thrown when the physical device rejects a Flash-related command
  // (returning 0x03 Command Not Allowed).
  // This exception is thrown under the following conditions:
  //   1. Flash memory is set to `PasswordProtected`, but `SendAccessPassword()` was not called
  //      before `Write()` (the unlock password was not provided).
  //   2. `SendAccessPassword()` was called, but the provided password does not match the
  //      Flash password.
  //   3. Flash memory is set to `PermanentlyLocked` and permanently protected from writing.
  //   4. Flash write endurance limit has been reached, regardless of write protection settings.

  Console.Error.WriteLine(ex);

  if (device.Flash.WriteProtectionLevel == DeviceConfigurationProtectionLevel.PermanentlyLocked) {
    Console.Error.WriteLine("This MCP2221A is permanently locked. Flash memory settings cannot be changed.");
  }
  else {
    if (device.Flash.WriteProtectionLevel == DeviceConfigurationProtectionLevel.PasswordProtected)
      Console.Error.WriteLine("Failed to unlock write protection for this MCP2221A. Please verify the password.");

    Console.Error.WriteLine("If write protection is not enabled or the password is correct, the Flash write endurance limit may have been reached.");
  }

  return;
}
catch (InvalidOperationException ex) {
  // Thrown when `Write()` is called while write protection is set to `PasswordProtected`,
  // but `ModifyPassword()` has NOT been called.
  // Call `ModifyPassword()` with the desired 8-byte password before calling `Write()`.
  Console.Error.WriteLine(ex);
  return;
}
catch (Exception) {
  // To write the settings read at instance creation (initial state) instead of staged changes,
  // use the `WriteInitialSettings()` method.
  // This can be used to roll back to the initial configuration if an unexpected exception or
  // cancellation request interrupts the write process.
  // However, note the Flash write endurance limit; avoid invoking rollback routines repeatedly
  // or unnecessarily.

  // await device.Flash.WriteInitialSettingsAsync(cts.Token);

  throw;
}

// -----------------------------------------------------------------------------------------
// STEP 4: Reset MCP2221A to reload and apply written settings.
// -----------------------------------------------------------------------------------------
Console.Write("Do you want to reset the device to apply and reload the changes? (y/N) ");

var answerForResettingDevice = Console.ReadLine();

if (!"Y".Equals(answerForResettingDevice, StringComparison.OrdinalIgnoreCase)) {
  Console.WriteLine("Complete.");
  return;
}

await device.ResetAsync(cts.Token);

// Resetting the device causes the underlying USB connection to reconnect, which places
// the instance in a disposed state.
// Subsequent operations using the same instance will throw an ObjectDisposedException;
// recreate the instance to perform further operations.
// Note that new or updated password protection settings take effect only after a device
// reset or power cycle.
Console.WriteLine("The device has been reset.");
