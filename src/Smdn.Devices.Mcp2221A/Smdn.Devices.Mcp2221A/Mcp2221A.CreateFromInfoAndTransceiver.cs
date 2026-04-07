// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040, CA1724
partial class Mcp2221AController {
#pragma warning restore IDE0040, CA1724
  private static async ValueTask<Mcp2221AController> CreateFromInfoAndTransceiverAsync(
    Mcp2221ATransceiver transceiver,
    Mcp2221AInfo info,
    ILogger? logger,
    CancellationToken cancellationToken
  )
  {
    var mcp2221A = CreateFromInfoAndTransceiverCore(
      transceiver: transceiver,
      info: info,
      logger: logger,
      cancellationToken: cancellationToken
    );

    await ((Mcp2221AGpioDriver)mcp2221A.GpPins).FetchGpSettingsAsync(
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    return mcp2221A;
  }

  private static Mcp2221AController CreateFromInfoAndTransceiver(
    Mcp2221ATransceiver transceiver,
    Mcp2221AInfo info,
    ILogger? logger,
    CancellationToken cancellationToken
  )
  {
    var mcp2221A = CreateFromInfoAndTransceiverCore(
      transceiver: transceiver,
      info: info,
      logger: logger,
      cancellationToken: cancellationToken
    );

    ((Mcp2221AGpioDriver)mcp2221A.GpPins).FetchGpSettings(
      cancellationToken: cancellationToken
    );

    return mcp2221A;
  }

  private static Mcp2221AController CreateFromInfoAndTransceiverCore(
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
