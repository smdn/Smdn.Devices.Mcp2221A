// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;

using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040, CA1724
partial class Mcp2221A {
#pragma warning restore IDE0040, CA1724
  private static Mcp2221A CreateFromInfoAndTransceiver(
    Mcp2221ATransceiver transceiver,
    Mcp2221AInfo info,
    ILogger? logger,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    ValidateHardwareRevision(info.HardwareRevision);
    ValidateFirmwareRevision(info.FirmwareRevision);

    return new(
      transceiver: transceiver,
      info: info,
      logger: logger
    );
  }
}
