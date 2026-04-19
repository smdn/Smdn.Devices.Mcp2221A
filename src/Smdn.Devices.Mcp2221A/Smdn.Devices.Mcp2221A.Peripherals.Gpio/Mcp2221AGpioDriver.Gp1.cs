// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class Mcp2221AGpioDriver {
#pragma warning restore IDE0040
  public ClockOutputFrequency CurrentClockOutputFrequency
    => (ClockOutputFrequency)(sramSettings.ReadClockOutputDividerValueByte() & 0b_0_00_00_111);

  public ClockOutputDutyCycle CurrentClockOutputDutyCycle
    => (ClockOutputDutyCycle)((sramSettings.ReadClockOutputDividerValueByte() & 0b_0_00_11_000) >> 3);
}
