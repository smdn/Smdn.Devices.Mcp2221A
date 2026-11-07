# Selecting a USB HID Backend Provider
This library communicates with the MCP2221/MCP2221A device using the **USB HID** interface. To do this, you must add a `PackageReference` for one of the following USB HID backend provider packages (`Smdn.IO.UsbHid.Providers.*`).

The following demonstrates how to configure package references for available USB HID backend providers in a C# project (csproj), how to use them via dependency injection, and how to configure options for each backend.

- [USBHID_Backend_HidApi.Net](./USBHID_Backend_HidApi.Net/) HidApi.Net (MIT License)
- [USBHID_Backend_HIDSharp](./USBHID_Backend_HIDSharp/) HIDSharp (Apache License 2.0)
- [USBHID_Backend_LibUsbDotNet](./USBHID_Backend_LibUsbDotNet/) LibUsbDotNet version 2 (LGPL-3.0)
- [USBHID_Backend_LibUsbDotNetV3](./USBHID_Backend_LibUsbDotNetV3/) LibUsbDotNet version 3 (LGPL-3.0)

## Permitting device access

If an `Mcp2221AUnavailableException` occurs in Linux, you need to either run the command as the root user (e.g., `sudo dotnet run`) or use a [udev rule files](/misc/udev/) to allow non-root users to access the device.

```
Unhandled exception. Smdn.Devices.Mcp2221A.Mcp2221AUnavailableException: The requested MCP2221/MCP2221A is unavailable due to reasons such as unprivileged access, being disconnected, or being blocked by another driver. (device='/dev/hidraw4')
```
