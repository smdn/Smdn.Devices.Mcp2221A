# Native AOT
This sample demonstrates Native AOT compilation. Executing `dotnet publish` enables the deployment of a precompiled application via [Native AOT compilation](https://learn.microsoft.com/dotnet/core/deploying/native-aot/).

## Notes on AOT Compatibility

`Smdn.Devices.Mcp2221A` itself and the majority of its dependencies are AOT compatible. However, certain dependencies (including transitive ones) do not yet explicitly declare AOT compatibility (meaning they lack `IsAotCompatible=true` in their assembly metadata), despite their execution paths being implicitly AOT compatible. Consequently, AOT compilation may trigger warnings such as [IL3058](https://learn.microsoft.com/dotnet/core/deploying/native-aot/warnings/il3058), and potential runtime issues (such as unexpected missing metadata or execution failures) cannot be completely ruled out.

For example, `Smdn.Devices.Mcp2221A` depends on `System.Device.Gpio`. Because `System.Device.Gpio` version 4.1.0 is not marked with `IsAotCompatible=true`, AOT compilation reports warning `IL3058`. Nevertheless, `Smdn.Devices.Mcp2221A` restricts its usage of `System.Device.Gpio` APIs to fundamental types like `PinValue` and `PinMode`, alongside the inheritance of platform- and architecture-agnostic abstract types such as `GpioDriver`, `GpioController`, `I2cBus`, and `I2cDevice`. Thus, the absence of dynamic code requirements and reflection usage ensures that actual AOT compilation failures are practically nonexistent in this context.

Additionally, while the USB HID backend providers used by `Smdn.Devices.Mcp2221A` (`Smdn.IO.UsbHid.Providers.*`) are AOT compatible, most underlying USB HID backend libraries are not. `HidApi.Net` (available via `Smdn.IO.UsbHid.Providers.HidApiDotNet`) is the [only backend that declares AOT compatibility](https://github.com/badcel/HidApi.Net#native-aot). However, because a version explicitly marked with `IsAotCompatible=true` has not yet been released, warning IL3058 still occurs during AOT compilation.

When targeting Native AOT compilation, selecting `HidApi.Net` (`Smdn.IO.UsbHid.Providers.HidApiDotNet`) as the USB HID backend is currently recommended.
