// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A;

public static class IMcp2221AInfoExtensions {
#pragma warning disable CA1034
  extension(IMcp2221AInfo info) {
#pragma warning restore CA1034
    /// <remarks>
    /// If the <see cref="IMcp2221AInfo.FirmwareRevision"/> is not retrieved or is an unknown
    /// revision, assume it is an MCP2221A.
    /// </remarks>
    public bool IsMcp2221A
      => !string.Equals(info.FirmwareRevision, Mcp2221AController.FirmwareRevisionMcp2221, StringComparison.Ordinal);
  }
}
