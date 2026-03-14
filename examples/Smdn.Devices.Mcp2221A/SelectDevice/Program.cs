// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

// When multiple MCP2221/MCP2221A devices are connected on the system,
// you can filter for the target device as follows.
// Also, if the MCP2221/MCP2221A has been configured with VID:PID other
// than the default, you can use filters to find the target device.
//
// In this example, the filter is configured to select the USB HID
// device recognized as VID:PID FFFF:FFFF and whose chip factory serial
// number is XXXXXXXX.

const int DeviceVendorId = 0xFFFF;
const int DeviceProductId = 0xFFFF;
const string DeviceChipFactorySerialNumber = "XXXXXXXX";

try {
  using var device = Mcp2221A.Create(
    serviceProvider: serviceProvider,
    usbHidDeviceFilter: (IUsbHidDevice usbHidDevice) => {
      return usbHidDevice.VendorId == DeviceVendorId && usbHidDevice.ProductId == DeviceProductId;
#if false
      // Additionally, you can filter based on properties that depend on the
      // implementation of the underlying USB HID library.
      // This enables more advanced device selection, such as filtering by
      // device path.

      // If you are using HidSharp, you can reference the underlying implementation as follows:
      if (usbHidDevice is IUsbHidDevice<HidSharp.HidDevice> { UnderlyingDevice: var hidSharpDevice }) {
        return
          hidSharpDevice.DevicePath == "/sys/devices/....../hidraw/hidraw0" ||
          hidSharpDevice.GetFileSystemName() == "/dev/hidraw0";
      }

      // If you are using LibUsbDotNet version 2, you can reference the
      // underlying implementation as follows:
      if (usbHidDevice is IUsbHidDevice<LibUsbDotNet.UsbDevice> { UnderlyingDevice: var libUsbDotNetDevice }) {
        return libUsbDotNetDevice.DevicePath == "/sys/devices/....../hidraw/hidraw0";
      }

      // If you are using LibUsbDotNet version 3, you can reference the
      // underlying implementation as follows:
      if (usbHidDevice is IUsbHidDevice<LibUsbDotNet.LibUsb.UsbDevice> { UnderlyingDevice: var libUsbDotNetV3Device }) {
        return
          libUsbDotNetV3Device.LocationId.ToString() == "1-2.3" ||
          libUsbDotNetV3Device.BusNumber == 1 ||
          libUsbDotNetV3Device.PortNumbers is [2, 3];
      }
#endif
    },
    mcp2221AFilter: (IMcp2221AInfo info) => {
      return info.ChipFactorySerialNumber == DeviceChipFactorySerialNumber;
    }
  );

  Console.WriteLine("A device matching the criteria has been found.");
}
// If no device matching the specified criteria exists,
// Mcp2221ANotFoundException will be thrown.
catch (Mcp2221ANotFoundException) {
  Console.Error.WriteLine(
    $"No devices matching the specified criteria were found ({DeviceVendorId:X4}:{DeviceProductId:X4}; {DeviceChipFactorySerialNumber})"
  );
}
