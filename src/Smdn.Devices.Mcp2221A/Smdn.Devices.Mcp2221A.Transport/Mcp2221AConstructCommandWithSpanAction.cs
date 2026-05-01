// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Transport;

internal delegate void Mcp2221AConstructCommandWithSpanAction<TArg>(
  Span<byte> command,
  ReadOnlySpan<byte> commandInput,
  TArg arg
);
