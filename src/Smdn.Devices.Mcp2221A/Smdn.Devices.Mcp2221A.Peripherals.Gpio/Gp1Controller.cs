// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
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
  /// <inheritdoc/>
  /// <value>
  /// Always <c>1</c>.
  /// </value>
  public override int Index { get; } = 1;

  /// <inheritdoc/>
  /// <value>
  /// Always <c>GP1</c>.
  /// </value>
  public override string PinName { get; } = "GP1";

  /// <inheritdoc/>
  public override GpFunction CurrentFunction => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => GpFunction.Gpio, // GPIO
    GpDesignation.DedicatedFunctionOperation => GpFunction.ClockOutput, // CLK OUT
    GpDesignation.AlternateFunction0 => GpFunction.Adc, // ADC1
    GpDesignation.AlternateFunction1 => GpFunction.LedOutput, // LED_UTX
    GpDesignation.AlternateFunction2 => GpFunction.ExternalInterrupt, // IOC
    _ => throw new NotSupportedException(),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO1",
    GpDesignation.DedicatedFunctionOperation => "CLK OUT",
    GpDesignation.AlternateFunction0 => "ADC1",
    GpDesignation.AlternateFunction1 => "LED_UTX",
    GpDesignation.AlternateFunction2 => "IOC",
    _ => throw new NotSupportedException(),
  };

  /// <inheritdoc/>
  VoltageReferenceSource IAdcController.CurrentAdcReferenceSource
    => GpioDriver.CurrentAdcReferenceSource;

  /// <inheritdoc/>
  public int LastReadAnalogRawValue
    => GpioDriver.GetLastFetchedAdcRawValue(Index);

  internal Gp1Controller(Mcp2221AGpioDriver gpioDriver)
    : base(gpioDriver)
  {
  }

  private protected override GpDesignation? GetDesignationForFunction(GpFunction function)
    => function switch {
      GpFunction.Gpio => GpDesignation.GpioOperation, // GPIO
      GpFunction.ClockOutput => GpDesignation.DedicatedFunctionOperation, // CLK OUT
      GpFunction.Adc => GpDesignation.AlternateFunction0, // ADC1
      GpFunction.LedOutput => GpDesignation.AlternateFunction1, // LED_UTX
      GpFunction.ExternalInterrupt => GpDesignation.AlternateFunction2, // IOC
      _ => null,
    };

  /// <inheritdoc/>
  public ValueTask ConfigureAsExternalInterruptAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsExternalInterrupt(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.LedOutput"/>
  public ValueTask ConfigureAsUtxLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.LedOutput"/>
  public void ConfigureAsUtxLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction1,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(
    VoltageReferenceSource voltageReferenceSource = VoltageReferenceSource.Vdd,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction0,
      dacVoltageReferenceSource: null,
      dacOutputValue: null,
      adcVoltageReferenceSource: voltageReferenceSource,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(
    VoltageReferenceSource voltageReferenceSource = VoltageReferenceSource.Vdd,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction0,
      dacVoltageReferenceSource: null,
      dacOutputValue: null,
      adcVoltageReferenceSource: voltageReferenceSource,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public int ReadAnalogRaw(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Adc);

    GpioDriver.FetchAdcRawValues(cancellationToken);

    return GpioDriver.GetLastFetchedAdcRawValue(Index);
  }

  /// <inheritdoc/>
  public async ValueTask<int> ReadAnalogRawAsync(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Adc);

    await GpioDriver.FetchAdcRawValuesAsync(cancellationToken).ConfigureAwait(false);

    return GpioDriver.GetLastFetchedAdcRawValue(Index);
  }

  /// <inheritdoc/>
  public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsClockOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
