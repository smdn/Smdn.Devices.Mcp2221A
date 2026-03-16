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
  private protected override int GpPinNumber => 2;

  /// <value>
  /// Always <c>GP2</c>.
  /// </value>
  public override string PinName { get; } = "GP2";

  internal Gp2Controller(Mcp2221ATransceiver transceiver)
    : base(transceiver)
  {
  }

  /// <inheritdoc/>
  public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "DAC1",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsDac(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "DAC1",
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "ADC2",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "ADC2",
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  public ValueTask ConfigureAsUsbConfigureStatusAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      pinDesignation: "USBCFG",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  public void ConfigureAsUsbConfigureStatus(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      pinDesignation: "USBCFG",
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
