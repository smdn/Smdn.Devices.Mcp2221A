# Integration with `I2cBus` and device bindings
The following demonstrates how to use the MCP2221A's I2C functionality via [System.Device.I2c.I2cBus](https://learn.microsoft.com/dotnet/api/system.device.i2c.i2cbus) and [System.Device.I2c.I2cDevice](https://learn.microsoft.com/dotnet/api/system.device.i2c.i2cdevice), and control various devices using the device bindings provided by [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings/).

- [I2cDevice_BME280](./I2cDevice_BME280/) BME280 atmospheric sensor
- [I2cDevice_HT16K33_4Digit14SegmentDisplay](./I2cDevice_HT16K33_4Digit14SegmentDisplay/) HT16K33 16-Anodes×8-Cathodes LED driver and Adafruit 0.54" Quad Alphanumeric FeatherWing Display
- [I2cDevice_MCP23017_Input](./I2cDevice_MCP23017_Input/) Read input values from MCP23017 8×2 IO expander
- [I2cDevice_MCP23017_Output](./I2cDevice_MCP23017_Output/) Write output values of MCP23017 8×2 IO expander
- [I2cDevice_US2066_SO1602A](./I2cDevice_US2066_SO1602A/) SO1602A OLED character display with WiseChip US2066 controller chip
