// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Iot.Device.Tm16xx;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// construct TM1637
using var tm1637 = new Tm1637(
  pinClk: 0, // use GP0 for CLK output
  pinDio: 1, // use GP1 for DIO output
  gpioController: device.GpioController,
  shouldDispose: false
);

tm1637.SetScreen(7, true, false);

var decimalChars = new[] {
  Character.Digit0,
  Character.Digit1,
  Character.Digit2,
  Character.Digit3,
  Character.Digit4,
  Character.Digit5,
  Character.Digit6,
  Character.Digit7,
  Character.Digit8,
  Character.Digit9
};
var dataBuffer = new byte[6];
var prevDataBuffer = new byte[6];

for (;;) {
  var time = TimeOnly.FromDateTime(DateTime.Now);

  dataBuffer[0] = (byte)decimalChars[time.Hour / 10];
  dataBuffer[1] = (byte)(decimalChars[time.Hour % 10] | Character.Dot);
  dataBuffer[2] = (byte)decimalChars[time.Minute / 10];
  dataBuffer[3] = (byte)(decimalChars[time.Minute % 10] | Character.Dot);
  dataBuffer[4] = (byte)decimalChars[time.Second / 10];
  dataBuffer[5] = (byte)decimalChars[time.Second % 10];

  if (time.Millisecond < 500)
    dataBuffer[5] |= (byte)Character.Dot;

  // If only the seconds part (index 4, 5) needs to be updated,
  // transmit only that part. If any other parts need to be
  // updated, re-transmit entire data.
  var index = Enumerable.Range(0, 6).FirstOrDefault(i => dataBuffer[i] != prevDataBuffer[i], -1);

  if (index < 0) {
    // No need to update.
    Thread.Sleep(50);
    continue;
  }
  else if (4 <= index) {
    // Transmit only the modified data.
    for (var i = index; i < 6; i++) {
      tm1637.Display(characterPosition: (byte)i, rawData: dataBuffer[i]);
    }
  }
  else {
    // Transmit entire data.
    tm1637.Display(dataBuffer);
  }

  Console.Clear();
  Console.WriteLine(time.ToString("o"));

  // Flip buffers.
  (prevDataBuffer, dataBuffer) = (dataBuffer, prevDataBuffer);
}
