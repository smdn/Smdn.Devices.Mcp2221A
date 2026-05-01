// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Transport;

internal delegate TResponse Mcp2221AParseResponseWithSpanFunc<TArg, TResponse>(
  ReadOnlySpan<byte> response,
  Span<byte> responseOutput,
  TArg arg
);
