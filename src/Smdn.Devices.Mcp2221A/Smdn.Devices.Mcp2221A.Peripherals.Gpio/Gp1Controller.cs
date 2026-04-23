// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0055
public sealed class Gp1Controller :
  GpController,
  IInterruptOnChangeController,
  IAdcController,
  IClockOutputController
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
    GpDesignation.AlternateFunction2 => GpFunction.InterruptOnChange, // IOC
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

  /// <inheritdoc/>
  public ClockOutputFrequency CurrentClockOutputFrequency
    => GpioDriver.CurrentClockOutputFrequency;

  /// <inheritdoc/>
  public ClockOutputDutyCycle CurrentClockOutputDutyCycle
    => GpioDriver.CurrentClockOutputDutyCycle;

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
      GpFunction.InterruptOnChange => GpDesignation.AlternateFunction2, // IOC
      _ => null,
    };

  /// <inheritdoc/>
  public ValueTask ConfigureAsInterruptOnChangeAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction2,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsInterruptOnChange(CancellationToken cancellationToken = default)
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
    => ConfigureAsAdcAsyncCore(
      voltageReferenceSource: voltageReferenceSource,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(
    VoltageReferenceSource voltageReferenceSource = VoltageReferenceSource.Vdd,
    CancellationToken cancellationToken = default
  )
    => ConfigureAsAdcCore(
      voltageReferenceSource: voltageReferenceSource,
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
  public ValueTask ConfigureAsClockOutputAsync(
    ClockOutputFrequency? frequency = null,
    ClockOutputDutyCycle? dutyCycle = null,
    CancellationToken cancellationToken = default
  )
    => GpioDriver.ConfigureGpPinSettingsAsync(
      gpIndex: Index,
      arg: (frequency, dutyCycle),
      modifyGpPinSettings: ConfigureGpPinSettingsAsClockOutput,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsClockOutput(
    ClockOutputFrequency? frequency = null,
    ClockOutputDutyCycle? dutyCycle = null,
    CancellationToken cancellationToken = default
  )
    => GpioDriver.ConfigureGpPinSettings(
      gpIndex: Index,
      arg: (frequency, dutyCycle),
      modifyGpPinSettings: ConfigureGpPinSettingsAsClockOutput,
      cancellationToken: cancellationToken
    );

  private static void ConfigureGpPinSettingsAsClockOutput(
    SramSettings sramSettings,
    int gpIndex,
    (
      ClockOutputFrequency? Frequency,
      ClockOutputDutyCycle? DutyCycle
    ) arg
  )
    => sramSettings
      .ModifyGpSettings(
        gp: gpIndex,
        designation: GpDesignation.DedicatedFunctionOperation
      )
      .ModifyClockOutputSettings(
        frequency: arg.Frequency,
        dutyCycle: arg.DutyCycle
      );

  /// <inheritdoc/>
  public ValueTask SuspendClockOutputAsync(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.ClockOutput);

    return ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.GpioOperation,
      gpioDirection: PinMode.Output,
      gpioInitialValue: PinValue.Low,
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc/>
  public void SuspendClockOutput(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.ClockOutput);

    ConfigureGpDesignation(
      gpDesignation: GpDesignation.GpioOperation,
      gpioDirection: PinMode.Output,
      gpioInitialValue: PinValue.Low,
      cancellationToken: cancellationToken
    );
  }
}
