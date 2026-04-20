// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// Configure GP1 as a clock output.
device.GpPin1.ConfigureAsClockOutput(
  frequency: ClockOutputFrequency.Frequency24MHz,
  dutyCycle: ClockOutputDutyCycle.Duty75
);

// Gets the frequency (divider ratio) and duty cycle currently
// set as the clock output.
Console.WriteLine(device.GpPin1.CurrentClockOutputFrequency);
Console.WriteLine(device.GpPin1.CurrentClockOutputDutyCycle);
Thread.Sleep(1000);

// Suspend the clock output temporarily.
device.GpPin1.SuspendClockOutput();
Console.WriteLine("Suspended the clock output.");
Thread.Sleep(1000);

// Resume clock output using the current settings.
device.GpPin1.ResumeClockOutput();
Console.WriteLine("Resumed the clock output.");
Thread.Sleep(1000);

// Output the clock signal by changing the various combination of
// duty cycle and frequency.
var dutyCycles = new[] {
  ClockOutputDutyCycle.Duty75,
  ClockOutputDutyCycle.Duty50,
  ClockOutputDutyCycle.Duty25,
  ClockOutputDutyCycle.Duty0,
};

foreach (var frequency in new[] {
  ClockOutputFrequency.Frequency375kHz,
  ClockOutputFrequency.Frequency3MHz,
  ClockOutputFrequency.Frequency24MHz,
}) {
  device.GpPin1.ConfigureAsClockOutput(frequency: frequency);

  foreach (var dutyCycle in dutyCycles) {
    device.GpPin1.ConfigureAsClockOutput(dutyCycle: dutyCycle);

    Console.WriteLine(
      "{0:N0} [Hz] ({1:P0})",
      device.GpPin1.CurrentClockOutputFrequencyInHz,
      device.GpPin1.CurrentClockOutputDutyRatio
    );

    Thread.Sleep(3000);
  }
}

Console.WriteLine();

// Output the clock signal at a 50% duty cycle across
// all configurable frequencies.
var frequencies = new[] {
  ClockOutputFrequency.Frequency24MHz,
  ClockOutputFrequency.Frequency12MHz,
  ClockOutputFrequency.Frequency6MHz,
  ClockOutputFrequency.Frequency3MHz,
  ClockOutputFrequency.Frequency1500kHz,
  ClockOutputFrequency.Frequency750kHz,
  ClockOutputFrequency.Frequency375kHz,
};

device.GpPin1.ConfigureAsClockOutput(dutyCycle: ClockOutputDutyCycle.Duty50);

foreach (var frequency in frequencies) {
  device.GpPin1.ConfigureAsClockOutput(frequency);

  Console.WriteLine(
    "{0:N0} [Hz] ({1:P0})",
    device.GpPin1.CurrentClockOutputFrequencyInHz,
    device.GpPin1.CurrentClockOutputDutyRatio
  );

  Thread.Sleep(3000);
}
