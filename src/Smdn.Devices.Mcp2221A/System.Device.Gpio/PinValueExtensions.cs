// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace System.Device.Gpio;

internal static class PinValueExtensions {
  extension(PinValue value) {
    public bool IsHigh => PinValue.High.Equals(value);
    public bool IsLow => PinValue.Low.Equals(value);
  }
}
