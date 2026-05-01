// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Globalization;

namespace Smdn.Formats.Binary;

internal static class BinaryFormat {
  public static bool IsBinaryFormatSpecifierSupported { get; }
    = Enum.IsDefined(typeof(NumberStyles), 1024 /* = NumberStyles.AllowBinarySpecifier */);
}
