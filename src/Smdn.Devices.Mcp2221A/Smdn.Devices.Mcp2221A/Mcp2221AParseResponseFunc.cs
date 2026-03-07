// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A;

internal delegate TResponse Mcp2221AParseResponseFunc<TArg, TResponse>(ReadOnlySpan<byte> response, TArg arg);
