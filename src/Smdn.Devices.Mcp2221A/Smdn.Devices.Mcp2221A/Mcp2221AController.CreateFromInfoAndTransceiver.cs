// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
using Smdn.Devices.Mcp2221A.Transport;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController {
#pragma warning restore IDE0040
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

    mcp2221A.SramDeviceConfiguration = await ((Mcp2221AGpioDriver)mcp2221A.GpPins).FetchSramSettingsAsync(
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

    mcp2221A.SramDeviceConfiguration = ((Mcp2221AGpioDriver)mcp2221A.GpPins).FetchSramSettings(
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

    if (logger is { } l && l.IsEnabled(LogLevel.Information)) {
      using var scope = l.BeginScope("Device Information");

      if (transceiver.EndPoint is { } usbHidEndPoint) {
        LogInformationDeviceInfo(l, "HID End Point", usbHidEndPoint);

#if !NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
#pragma warning disable CS8604
#endif
        if (usbHidEndPoint.Device.TryGetDeviceIdentifier(out var deviceIdentifier))
          LogInformationDeviceInfo(l, "HID Device", deviceIdentifier);
#pragma warning restore CS8604
      }

      LogInformationDeviceInfo(l, nameof(info.HardwareRevision), info.HardwareRevision);
      LogInformationDeviceInfo(l, nameof(info.FirmwareRevision), info.FirmwareRevision);
      LogInformationDeviceInfo(l, nameof(info.Manufacturer), info.Manufacturer);
      LogInformationDeviceInfo(l, nameof(info.Product), info.Product);
      LogInformationDeviceInfo(l, nameof(info.SerialNumber), info.SerialNumber);
      LogInformationDeviceInfo(l, nameof(info.ChipFactorySerialNumber), info.ChipFactorySerialNumber);
    }

    ValidateHardwareRevision(info.HardwareRevision);
    ValidateFirmwareRevision(info.FirmwareRevision);

    return new(
      transceiver: transceiver,
      info: info,
      logger: logger
    );
  }

  [LoggerMessage(
    EventId = 10,
    EventName = "Device Information",
    Level = LogLevel.Information,
    Message = "{Name}: {Value}"
  )]
  private static partial void LogInformationDeviceInfo(ILogger logger, string name, object value);
}
