// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0055
public sealed class Gp3Controller :
  GpController,
  IAdcController,
  IDacController
{
#pragma warning restore IDE0055
  private protected override int GPIndex => 3;

  internal Gp3Controller(Mcp2221A device)
    : base(device)
  {
  }

  public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "DAC2",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsDac(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "DAC2",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "ADC3",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "ADC3",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsLedI2cAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "LED_I2C",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsLedI2c(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "LED_I2C",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
