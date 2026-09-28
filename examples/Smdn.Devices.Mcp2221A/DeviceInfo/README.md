# `DeviceInfo`
This example demonstrates how to retrieve and display the device status and configuration currently loaded in the SRAM of the MCP2221A.

It covers the following information categories:
- **Read-only Hardware Information**: Fixed data such as the firmware version and hardware revision.
- **SRAM-based Configuration**: Settings currently loaded in the SRAM (e.g., write-protection levels and power attributes), which are retrieved using the `GET SRAM SETTINGS` command.
- **Flash-based USB Descriptors**: Persistent identity strings stored in the Flash memory, including the Manufacturer, Product, and Serial Number.

The `Mcp2221AController` retrieves these details during initialization. While the hardware configuration is reflected from the currently active SRAM settings, the USB descriptor strings are accessed directly from the device's Flash storage. This sample provides an overview of the device's identity and its currently applied operational parameters.

For information on how to read all settings stored in flash memory, see the [FlashMemory_Read](../FlashMemory_Read/README.md) example.
