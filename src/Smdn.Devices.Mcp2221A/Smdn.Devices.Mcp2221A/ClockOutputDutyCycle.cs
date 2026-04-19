// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Specifies the duty cycle of the clock output from the GP1 pin.
/// </summary>
public enum ClockOutputDutyCycle {
  // The values of each member correspond to the bits 3-4 (Clock Output
  // Duty Cycle) defined in the 'Write SRAM Settings' command (0x60)
  // of the MCP2221A.
  // See Register 1-2 in the datasheet for more details.

  /// <summary>
  /// 0% duty cycle.
  /// </summary>
  /// <remarks>
  /// Note: According to actual device testing, setting this value may not
  /// completely disable the clock output. Some form of clock signal may
  /// still be observed on the GP1 pin.
  /// </remarks>
  Duty0 = 0b_00,

  /// <summary>
  /// 25% duty cycle.
  /// </summary>
  Duty25 = 0b_01,

  /// <summary>
  /// 50% duty cycle (Factory default).
  /// </summary>
  Duty50 = 0b_10,

  /// <summary>
  /// 75% duty cycle.
  /// </summary>
  Duty75 = 0b_11,
}
