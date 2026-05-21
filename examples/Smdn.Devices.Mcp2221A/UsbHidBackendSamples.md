# Selecting a USB HID Backend Provider
This library communicates with the MCP2221/MCP2221A device using the **USB HID** interface. To do this, you must add a `PackageReference` for one of the following USB HID backend provider packages (`Smdn.IO.UsbHid.Providers.*`).

The following demonstrates how to configure package references for available USB HID backend providers in a C# project (csproj), how to use them via dependency injection, and how to configure options for each backend.

- [USBHID_Backend_HidSharp](./USBHID_Backend_HidSharp/) HidSharp (Apache License 2.0)
- [USBHID_Backend_LibUsbDotNet](./USBHID_Backend_LibUsbDotNet/) LibUsbDotNet version 2 (LGPL-3.0)
- [USBHID_Backend_LibUsbDotNetV3](./USBHID_Backend_LibUsbDotNetV3/) LibUsbDotNet version 3 (LGPL-3.0)
