// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
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
  /// <inheritdoc/>
  /// <value>
  /// Always <c>2</c>.
  /// </value>
  public override int Index { get; } = 2;

  /// <value>
  /// Always <c>GP2</c>.
  /// </value>
  public override string PinName { get; } = "GP2";

  /// <inheritdoc/>
  public override GpFunction CurrentFunction => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => GpFunction.Gpio, // GPIO
    GpDesignation.DedicatedFunctionOperation => GpFunction.UsbConfigureStatus, // USBCFG
    GpDesignation.AlternateFunction0 => GpFunction.Adc, // ADC2
    GpDesignation.AlternateFunction1 => GpFunction.Dac, // DAC1
    _ => throw new NotSupportedException(),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO2",
    GpDesignation.DedicatedFunctionOperation => "USBCFG",
    GpDesignation.AlternateFunction0 => "ADC2",
    GpDesignation.AlternateFunction1 => "DAC1",
    _ => throw new NotSupportedException(),
  };

  internal Gp2Controller(Mcp2221AGpioDriver gpio)
    : base(gpio)
  {
  }

  private protected override GpDesignation? GetDesignationForFunction(GpFunction function)
    => function switch {
      GpFunction.Gpio => GpDesignation.GpioOperation, // GPIO
      GpFunction.UsbConfigureStatus => GpDesignation.DedicatedFunctionOperation, // USBCFG
      GpFunction.Adc => GpDesignation.AlternateFunction0, // ADC2
      GpFunction.Dac => GpDesignation.AlternateFunction1, // DAC1
      _ => null,
    };

  /// <inheritdoc/>
  public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsDac(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.UsbConfigureStatus"/>
  public ValueTask ConfigureAsUsbConfigureStatusAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.UsbConfigureStatus"/>
  public void ConfigureAsUsbConfigureStatus(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
