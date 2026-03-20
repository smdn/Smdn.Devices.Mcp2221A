// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
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
  /// <inheritdoc/>
  /// <value>
  /// Always <c>3</c>.
  /// </value>
  public override int Index { get; } = 3;

  /// <value>
  /// Always <c>GP3</c>.
  /// </value>
  public override string PinName { get; } = "GP3";

  /// <inheritdoc/>
  public override GpFunction CurrentFunction => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => GpFunction.Gpio, // GPIO
    GpDesignation.DedicatedFunctionOperation => GpFunction.LedOutput, // LED_I2C
    GpDesignation.AlternateFunction0 => GpFunction.Adc, // ADC3
    GpDesignation.AlternateFunction1 => GpFunction.Dac, // DAC2
    _ => throw new NotSupportedException(),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO3",
    GpDesignation.DedicatedFunctionOperation => "LED_I2C",
    GpDesignation.AlternateFunction0 => "ADC3",
    GpDesignation.AlternateFunction1 => "DAC2",
    _ => throw new NotSupportedException(),
  };

  internal Gp3Controller(Mcp2221AGpioDriver gpio)
    : base(gpio)
  {
  }

  private protected override GpDesignation? GetDesignationForFunction(GpFunction function)
    => function switch {
      GpFunction.Gpio => GpDesignation.GpioOperation, // GPIO
      GpFunction.LedOutput => GpDesignation.DedicatedFunctionOperation, // LED_I2C
      GpFunction.Adc => GpDesignation.AlternateFunction0, // ADC3
      GpFunction.Dac => GpDesignation.AlternateFunction1, // DAC2
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

  /// <seealso cref="GpFunction.LedOutput"/>
  public ValueTask ConfigureAsI2cLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <seealso cref="GpFunction.LedOutput"/>
  public void ConfigureAsI2cLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
