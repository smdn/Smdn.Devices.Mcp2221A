// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Defines the trigger conditions for the Interrupt-on-Change (IOC) function.
/// </summary>
/// <remarks>
/// This enumeration is used to specify which edge transitions on the GP1 pin
/// will set the interrupt detection flag in the MCP2221A.
/// </remarks>
/// <seealso cref="Smdn.Devices.Mcp2221A.Peripherals.Gpio.Gp1Controller"/>
/// <seealso cref="Smdn.Devices.Mcp2221A.Peripherals.Gpio.IInterruptOnChangeController"/>
public enum InterruptOnChangeTrigger {
  /// <summary>
  /// No edge detection. The interrupt detection flag will not be set.
  /// </summary>
  None = 0b_00,

  /// <summary>
  /// Trigger on a rising edge (positive edge).
  /// </summary>
  Rising = 0b_01,

  /// <summary>
  /// Trigger on a falling edge (negative edge).
  /// </summary>
  Falling = 0b_10,

  /// <summary>
  /// Trigger on both rising and falling edges.
  /// </summary>
  Both = Rising | Falling,
}
