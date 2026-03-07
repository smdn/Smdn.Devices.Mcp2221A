// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A;

internal delegate void Mcp2221AConstructCommandAction<TArg>(Span<byte> command, ReadOnlySpan<byte> userData, TArg arg);
