// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

public sealed class Gp0Controller : GpController {
  private protected override int GpPinNumber => 0;

  /// <inheritdoc/>
  /// <value>
  /// Always <c>GP0</c>.
  /// </value>
  public override string PinName { get; } = "GP0";

  internal Gp0Controller(Mcp2221ATransceiver transceiver)
    : base(transceiver)
  {
  }

  public ValueTask ConfigureAsUrxLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "LED_URX",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsUrxLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "LED_URX",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsUsbSuspendStatusAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "SSPND",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsUsbSuspendStatus(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "SSPND",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
