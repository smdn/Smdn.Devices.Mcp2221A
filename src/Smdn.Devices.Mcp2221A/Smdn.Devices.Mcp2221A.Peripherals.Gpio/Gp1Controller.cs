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
  private protected override int GpPinNumber => 1;

  /// <inheritdoc/>
  /// <value>
  /// Always <c>GP1</c>.
  /// </value>
  public override string PinName { get; } = "GP1";

  internal Gp1Controller(Mcp2221ATransceiver transceiver)
    : base(transceiver)
  {
  }

  public ValueTask ConfigureAsExternalInterruptAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "Interrupt Detection",
      gpDesignation: GpDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsExternalInterrupt(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "Interrupt Detection",
      gpDesignation: GpDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsUtxLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "LED_UTX",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsUtxLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "LED_UTX",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "ADC1",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "ADC1",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "Clock Output",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsClockOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "Clock Output",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
