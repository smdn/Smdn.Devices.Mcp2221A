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
  private protected override int GpPinNumber => 3;

  /// <value>
  /// Always <c>GP3</c>.
  /// </value>
  public override string PinName { get; } = "GP3";

  internal Gp3Controller(Mcp2221ATransceiver transceiver)
    : base(transceiver)
  {
  }

  /// <inheritdoc/>
  public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "DAC2",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsDac(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "DAC2",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "ADC3",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "ADC3",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsI2cLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "LED_I2C",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsI2cLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "LED_I2C",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
