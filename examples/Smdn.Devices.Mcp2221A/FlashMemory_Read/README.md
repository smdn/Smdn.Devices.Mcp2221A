# `FlashMemory_Read`
This example shows how to read settings stored in the Flash memory of the MCP2221/MCP2221A.

When an `Mcp2221AController` instance is created, it reads the settings (the register values of `Chip settings`, `GP settings` and USB descriptor strings) written to Flash memory and stores them in the local machine's memory as initial default settings.
These settings can be accessed via the `Mcp2221AController.Flash` property.

### Key Behavior of the `Flash` Property
- **Initial Configuration Snapshot**: The `Mcp2221AController.Flash` property internally maintains the initial Flash memory settings captured from the hardware device at the time of instance creation.
- **Reflecting Staged Changes**: Getter properties on `Mcp2221AController.Flash` (e.g., `WriteProtectionLevel`, `ClockOutputFrequency`, and `GpSettings`) dynamically reflect the staged buffer if modifications have been made via `ModifyXxx` methods. If no staging operations have been performed, they return the original read-back values from the device.

### Example of output
When you run this example, you will see the following output. All values, except for the "USB Serial Number Descriptor String," represent the factory default values.

```
[CHIPSETTING0]
CdcSerialNumberEnumerationEnabled (CDCSNEN): False
WriteProtectionLevel (CHIPPROT): None

[CHIPSETTING1]
ClockOutputDutyCycle (CLKDC): Duty50
ClockOutputFrequency (CLKDIV): Frequency12MHz

[CHIPSETTING2]
DacVoltageReference (DACVRM/DACREF): Vdd
DacInitialValue (DACVAL): 8

[CHIPSETTING3]
InterruptOnChangeTrigger (INTDETFEEN/INTDETREEN): Both
AdcVoltageReference (ADCVRM/ADCREF): Vrm1024

[USBVIDL/USBVIDH]
UsbVendorId (USBVIDL/USBVIDH): 0x04D8

[USBPIDL/USBPIDH]
UsbProductId (USBPIDL/USBPIDH): 0x00DD

[USBPWRATTR]
UsbPowerMode (SELFPWR): BusPowered
UsbRemoteWakeUpEnabled (REMWKUP): False

[USBREQCRT]
UsbRequestedCurrentAmount (USBREQCRT): 100 [mA]

[GPSETTING0]
Designation (GPDES): 0b010 (LedOutput)
GpioMode (GPIODIR): Output
GpioOutputValue (GPIOOUTVAL): High

[GPSETTING1]
Designation (GPDES): 0b011 (LedOutput)
GpioMode (GPIODIR): Output
GpioOutputValue (GPIOOUTVAL): High

[GPSETTING2]
Designation (GPDES): 0b001 (UsbConfigureStatus)
GpioMode (GPIODIR): Output
GpioOutputValue (GPIOOUTVAL): High

[GPSETTING3]
Designation (GPDES): 0b001 (LedOutput)
GpioMode (GPIODIR): Output
GpioOutputValue (GPIOOUTVAL): High

USB Manufacturer Descriptor String: Microchip Technology Inc.
USB Product Descriptor String: MCP2221 USB-I2C/UART Combo
USB Serial Number Descriptor String: 0000000000
Chip Factory Serial Number: 01234567

IsDirty: False
```
