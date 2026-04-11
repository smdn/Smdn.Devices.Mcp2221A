// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Specifies the voltage reference source for the DAC (Digital-to-Analog Converter)
/// and ADC (Analog-to-Digital Converter) modules.
/// </summary>
/// <remarks>
/// The MCP2221A allows independent selection of the voltage reference source for
/// the ADC and DAC modules. Note that if an internal voltage reference (Vrm) is
/// selected, its voltage level must be less than VDD.
/// </remarks>
public enum VoltageReferenceSource {
  // The values of each member correspond to the bit 0-2 (VRM/VREF selector bits)
  // defined in the 'Write SRAM Settings' command (0x60) of the MCP2221A.
  // See Register 1-3 in the datasheet for more details.

  /// <summary>
  /// Uses the device's supply voltage (VDD) as the voltage reference.
  /// </summary>
  Vdd = 0b_0_0000_00_0,

  /// <summary>
  /// Disables the Internal Voltage Reference Module (VRM), resulting in a 0V reference.
  /// </summary>
  VrmOff = 0b_0_0000_00_1,

  /// <summary>
  /// Uses the Internal Voltage Reference Module (VRM) set to 1.024V.
  /// </summary>
  Vrm1024 = 0b_0_0000_01_1,

  /// <summary>
  /// Uses the Internal Voltage Reference Module (VRM) set to 2.048V.
  /// </summary>
  Vrm2048 = 0b_0_0000_10_1,

  /// <summary>
  /// Uses the Internal Voltage Reference Module (VRM) set to 4.096V.
  /// </summary>
  /// <remarks>
  /// This value is only valid when VDD is greater than 4.096V (e.g., VDD is 5V).
  /// </remarks>
  Vrm4096 = 0b_0_0000_11_1,
}
