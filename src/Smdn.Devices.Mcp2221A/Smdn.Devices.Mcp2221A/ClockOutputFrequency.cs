// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Specifies the frequency of the clock output from the GP1 pin.
/// </summary>
public enum ClockOutputFrequency {
  // The values of each member correspond to the bits 0-2 (Clock Output Divider)
  // defined in the 'Write SRAM Settings' command (0x60) of the MCP2221A.
  // See Register 1-2 in the datasheet for more details.

  /// <summary>
  /// Reserved by the device.
  /// </summary>
  /// <remarks>
  /// This value is not currently supported and should not be used in
  /// <see cref="Smdn.Devices.Mcp2221A.Peripherals.Gpio.IClockOutputController.ConfigureAsClockOutput"/>.
  /// </remarks>
#pragma warning disable CA1700
  Reserved = 0b_000,
#pragma warning restore CA1700

  /// <summary>
  /// Outputs a 24 MHz clock (1/1 divider).
  /// </summary>
  Frequency24MHz = 0b_001,

  /// <summary>
  /// Outputs a 12 MHz clock (1/2 divider).
  /// </summary>
  Frequency12MHz = 0b_010,

  /// <summary>
  /// Outputs a 6 MHz clock (1/4 divider).
  /// </summary>
  Frequency6MHz = 0b_011,

  /// <summary>
  /// Outputs a 3 MHz clock (1/8 divider).
  /// </summary>
  Frequency3MHz = 0b_100,

  /// <summary>
  /// Outputs a 1.5 MHz clock (1/16 divider).
  /// </summary>
  Frequency1500kHz = 0b_101,

  /// <summary>
  /// Outputs a 750 kHz clock (1/32 divider).
  /// </summary>
  Frequency750kHz = 0b_110,

  /// <summary>
  /// Outputs a 375 kHz clock (1/64 divider).
  /// </summary>
  Frequency375kHz = 0b_111,
}
