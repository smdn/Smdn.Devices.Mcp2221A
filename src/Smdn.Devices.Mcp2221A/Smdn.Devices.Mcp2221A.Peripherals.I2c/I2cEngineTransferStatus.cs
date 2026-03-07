// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

internal enum I2cEngineTransferStatus : byte {
  NoSpecialOperation = 0x00, // No special operation
  MarkedForCancellation = 0x10, // The current I2C/SMBus transfer was marked for cancellation
  AlreadyInIdleMode = 0x11, // The I2C engine was already in idle mode
}
