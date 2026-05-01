// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.Logging;

using Smdn.Formats.Binary;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class SramSettings {
#pragma warning restore IDE0040
  private static string FormatSettingsInBinary(ReadOnlySpan<byte> s)
  {
    // [0] Clock Output Divider Value
    // [1] DAC Voltage Reference
    // [2] Set DAC Output Value
    // [3] ADC Voltage Reference
    // [4] Setup the interrupt detection mechanism and clear the detection flag
    // [5] Alter GPIO configuration
    // [6] GP0 Settings
    // [7] GP1 Settings
    // [8] GP2 Settings
    // [9] GP3 Settings
    if (BinaryFormat.IsBinaryFormatSpecifierSupported)
      return $"CLK={s[0]:B8} DAC={s[1]:B8} DACValue={s[2]:B8} ADC={s[3]:B8} IOC={s[4]:B8} AlterGPIO={s[5]:B8} GP0={s[6]:B8} GP1={s[7]:B8} GP2={s[8]:B8} GP3={s[9]:B8}";

    return string.Format(
      provider: null,
      "CLK={0} DAC={1} DACValue={2} ADC={3} IOC={4} AlterGPIO={5} GP0={6} GP1={7} GP2={8} GP3={9}",
      Convert.ToString(s[0], 2).PadLeft(8, '0'),
      Convert.ToString(s[1], 2).PadLeft(8, '0'),
      Convert.ToString(s[2], 2).PadLeft(8, '0'),
      Convert.ToString(s[3], 2).PadLeft(8, '0'),
      Convert.ToString(s[4], 2).PadLeft(8, '0'),
      Convert.ToString(s[5], 2).PadLeft(8, '0'),
      Convert.ToString(s[6], 2).PadLeft(8, '0'),
      Convert.ToString(s[7], 2).PadLeft(8, '0'),
      Convert.ToString(s[8], 2).PadLeft(8, '0'),
      Convert.ToString(s[9], 2).PadLeft(8, '0')
    );
  }

  [LoggerMessage(
    EventId = 20,
    EventName = "SRAM settings",
    Level = LogLevel.Debug,
    Message = "Modified SRAM settings: {UnsentSettings}"
  )]
  private static partial void LogDebugModifiedSramSettings(ILogger logger, string unsentSettings);

  [LoggerMessage(
    EventId = 21,
    EventName = "SRAM settings",
    Level = LogLevel.Debug,
    Message = "Stored SRAM settings: {CurrentSettings}"
  )]
  private static partial void LogDebugStoredSramSettings(ILogger logger, string currentSettings);

  [LoggerMessage(
    EventId = 22,
    EventName = "SRAM settings",
    Level = LogLevel.Debug,
    Message = "Restored SRAM settings: {CurrentSettings}"
  )]
  private static partial void LogDebugRestoredSramSettings(ILogger logger, string currentSettings);
}
