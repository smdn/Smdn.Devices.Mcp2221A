// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0055
public sealed class Gp2Controller :
  GpController,
  IAdcController,
  IDacController
{
#pragma warning restore IDE0055
  private protected override int GPIndex => 2;

  internal Gp2Controller(Mcp2221A device)
    : base(device)
  {
  }

  public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "DAC1",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsDac(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "DAC1",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "ADC2",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "ADC2",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsUsbCfgAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "USBCFG",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsUsbCfg(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "USBCFG",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
