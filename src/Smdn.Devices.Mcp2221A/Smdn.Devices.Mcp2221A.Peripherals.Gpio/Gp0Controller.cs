// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

public sealed class Gp0Controller : GpController {
  private protected override int GPIndex => 0;

  internal Gp0Controller(Mcp2221A device)
    : base(device)
  {
  }

  public ValueTask ConfigureAsLedUrxAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "LED_URX",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsLedUrx(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "LED_URX",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsSspndAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "SSPND",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsSspnd(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "SSPND",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
