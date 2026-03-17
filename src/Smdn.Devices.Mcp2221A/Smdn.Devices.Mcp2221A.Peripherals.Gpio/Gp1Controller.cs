// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0055
public sealed class Gp1Controller :
  GpController,
  IInterruptController,
  IAdcController,
  IClockController
{
#pragma warning restore IDE0055
  private protected override int GPIndex => 1;

  internal Gp1Controller(Mcp2221A device)
    : base(device)
  {
  }

  public ValueTask ConfigureAsInterruptDetectionAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "Interrupt Detection",
      gpDesignation: GPDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsInterruptDetection(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "Interrupt Detection",
      gpDesignation: GPDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsLedUtxAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "LED_UTX",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsLedUtx(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "LED_UTX",
      gpDesignation: GPDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "ADC1",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "ADC1",
      gpDesignation: GPDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGPDesignationAsync(
      pinDesignation: "Clock Output",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsClockOutput(CancellationToken cancellationToken = default)
    => ConfigureGPDesignation(
      pinDesignation: "Clock Output",
      gpDesignation: GPDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
