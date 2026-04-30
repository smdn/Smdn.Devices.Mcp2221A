# `SramDeviceConfiguration`
This example demonstrates how to retrieve and display the device status and configuration currently loaded in the SRAM of the MCP2221A.

It covers the following information categories:
- **Read-only Hardware Information**: Fixed data such as the firmware version and hardware revision.
- **SRAM-based Configuration**: Settings currently loaded in the SRAM (e.g., write-protection levels and power attributes), which are retrieved using the `GET SRAM SETTINGS` command.
- **Flash-based USB Descriptors**: Persistent identity strings stored in the Flash memory, including the Manufacturer, Product, and Serial Number.

The `Mcp2221AController` retrieves these details during initialization. While the hardware configuration is reflected from the currently active SRAM settings, the USB descriptor strings are accessed directly from the device's Flash storage. This sample provides an overview of the device's identity and its currently applied operational parameters.

### Example of output
When you run this example, you will see the following output.

```txt
[Hardware Information (Read-only)]
HardwareRevision: A.6
FirmwareRevision: 1.2

[USB Descriptor Strings (Stored in Flash memory)]
Manufacturer: Microchip Technology Inc.
Product: MCP2221 USB-I2C/UART Combo
SerialNumber: 0000099999
ChipFactorySerialNumber: 01234567

[Device Configurations (Currently loaded in SRAM)]
UsbVendorId: 0x04D8
UsbProductId: 0x00DD
UsbCdcSerialNumberEnabled: False
UsbPowerMode: BusPowered
UsbRemoteWakeUpEnabled: False
UsbRequestedCurrentAmount: 100 mA
FlashWriteProtection: None

[Active USB HID Interface IDs]
VendorId: 0x04D8
ProductId: 0x00DD

[GP0-GP3 Configurations (Currently loaded in SRAM)]
DAC Reference Voltage: Vdd
DAC Output Value: 8
ADC Reference Voltage: Vdd
Interrupt-on-change Trigger: Both
Clock Output Frequency: Frequency12MHz (12,000,000 Hz)
Clock Output Duty Cycle: Duty50 (50%)

|               |GP0                 |GP1                 |GP2                 |GP3                 |
|Function       |LedOutput           |LedOutput           |UsbConfigureStatus  |LedOutput           |
|Designation    |LED_URX             |LED_UTX             |USBCFG              |LED_I2C             |
|GPIO Direction |                    |                    |                    |                    |
```
