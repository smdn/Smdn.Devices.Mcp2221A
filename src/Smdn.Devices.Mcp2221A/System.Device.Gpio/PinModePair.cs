// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace System.Device.Gpio;

/// <summary>
/// Simple pair type like <see cref="PinValuePair"/> but for <see cref="PinMode"/>.
/// </summary>
internal readonly record struct PinModePair(int PinNumber, PinMode PinMode);
