// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Simple pair type like <see cref="PinValuePair"/> but for <see cref="PinMode"/>.
/// </summary>
[CLSCompliant(false)]
public readonly record struct PinModePair(int PinNumber, PinMode PinMode);
