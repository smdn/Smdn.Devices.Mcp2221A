// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using Iot.Device.Mcp23xxx;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221AController.CreateAsync(serviceProvider);

await device.GpPin3.ConfigureAsI2cLedOutputAsync();

const int DeviceAddressMcp23017 = 0x20; // The address of MCP23017 which is connected to MCP2221/MCP2221A

var mcp23017 = new Mcp23017(
  i2cDevice: device.I2cBus.CreateDevice(DeviceAddressMcp23017).WithStandardMode(),
  shouldDispose: false, // Mcp23017 itself does not dispose supplied i2cDevice above in this case
  controller: null,
  reset: -1,        // disable RESET pin
  interruptA: -1,   // disable INTA pin
  interruptB: -1    // disable INTB pin
);

// disable interrupt-on-change of GPINT<0~7>
mcp23017.WriteUInt16(Register.GPINTEN, 0b_0000_0000_0000_0000);

// configure GPA<0~7> and GPB<0~7> as input (IODIRA, IODIRB)
mcp23017.WriteUInt16(Register.IODIR, 0b_1111_1111_1111_1111);

for (;;) {
  // read input values of GPA<0~7> and GPB<0~7>
  var data = mcp23017.ReadUInt16(Register.GPIO);

  Console.Clear();
  Console.WriteLine($"{data:B16}");

  await Task.Delay(20);
}
