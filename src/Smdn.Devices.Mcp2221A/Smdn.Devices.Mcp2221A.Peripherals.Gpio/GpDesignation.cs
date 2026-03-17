// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

// [MCP2221A] 3.1.13 SET SRAM SETTINGS
// Byte Index 8-11 GP0-3 Settings
// Bit 2-0: GP<n> Designation
internal enum GpDesignation : byte {
  AlternateFunction2 = 0b_000_0_0_100,
  AlternateFunction1 = 0b_000_0_0_011,
  AlternateFunction0 = 0b_000_0_0_010,
  DedicatedFunctionOperation = 0b_000_0_0_001,
  GpioOperation = 0b_000_0_0_000,
  BitMask = 0b_000_0_0_111,
}
