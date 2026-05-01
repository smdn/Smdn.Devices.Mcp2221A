// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Provides control over the GP2 pin of the MCP2221/MCP2221A.
/// </summary>
/// <remarks>
/// <para>The GP2 pin supports the following functions:</para>
/// <list type="bullet">
/// <item><description><b>GPIO:</b> General purpose input/output (GPIO2).</description></item>
/// <item><description><b>USBCFG:</b> USB configured indicator.</description></item>
/// <item><description><b>ADC:</b> Analog-to-Digital Converter input (ADC2).</description></item>
/// <item><description><b>DAC:</b> Digital-to-Analog Converter output (DAC1).</description></item>
/// </list>
/// </remarks>
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
    var unsupported => throw CreateUnsupportedGpDesignationException(Index, unsupported),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO2",
    GpDesignation.DedicatedFunctionOperation => "USBCFG",
    GpDesignation.AlternateFunction0 => "ADC2",
    GpDesignation.AlternateFunction1 => "DAC1",
    var unsupported => throw CreateUnsupportedGpDesignationException(Index, unsupported),
  };

  /// <inheritdoc/>
  VoltageReferenceSource IDacController.CurrentDacReferenceSource
    => GpioDriver.CurrentDacReferenceSource;

  /// <inheritdoc/>
  int IDacController.LastWriteAnalogRawValue
    => GpioDriver.LastAppliedDacRawValue;

  /// <inheritdoc/>
  VoltageReferenceSource IAdcController.CurrentAdcReferenceSource
    => GpioDriver.CurrentAdcReferenceSource;

  /// <inheritdoc/>
  public int LastReadAnalogRawValue
    => GpioDriver.GetLastFetchedAdcRawValue(Index);

  internal Gp2Controller(Mcp2221AGpioDriver gpioDriver)
    : base(gpioDriver)
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
  public ValueTask ConfigureAsDacAsync(
    VoltageReferenceSource? voltageReferenceSource = VoltageReferenceSource.Vdd,
    int? initialOutputValue = null,
    CancellationToken cancellationToken = default
  )
    => ConfigureAsDacAsyncCore(
      voltageReferenceSource: voltageReferenceSource,
      initialOutputValue: initialOutputValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsDac(
    VoltageReferenceSource? voltageReferenceSource = VoltageReferenceSource.Vdd,
    int? initialOutputValue = null,
    CancellationToken cancellationToken = default
  )
    => ConfigureAsDacCore(
      voltageReferenceSource: voltageReferenceSource,
      initialOutputValue: initialOutputValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void WriteAnalogRaw(
    int value,
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Dac);

    GpioDriver.ApplyDacRawValue(
      Mcp2221AGpioDriver.ThrowIfDacOutputValueOutOfRange(value, nameof(value)),
      cancellationToken
    );
  }

  /// <inheritdoc/>
  public ValueTask WriteAnalogRawAsync(
    int value,
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Dac);

    return GpioDriver.ApplyDacRawValueAsync(
      value: Mcp2221AGpioDriver.ThrowIfDacOutputValueOutOfRange(value, nameof(value)),
      cancellationToken
    );
  }

  /// <inheritdoc/>
  public ValueTask ConfigureAsAdcAsync(
    VoltageReferenceSource? voltageReferenceSource = VoltageReferenceSource.Vdd,
    CancellationToken cancellationToken = default
  )
    => ConfigureAsAdcAsyncCore(
      voltageReferenceSource: voltageReferenceSource,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  public void ConfigureAsAdc(
    VoltageReferenceSource? voltageReferenceSource = VoltageReferenceSource.Vdd,
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
    GpioDriver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Adc);

    GpioDriver.FetchAdcRawValues(cancellationToken);

    return GpioDriver.GetLastFetchedAdcRawValue(Index);
  }

  /// <inheritdoc/>
  public async ValueTask<int> ReadAnalogRawAsync(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Adc);

    await GpioDriver.FetchAdcRawValuesAsync(cancellationToken).ConfigureAwait(false);

    return GpioDriver.GetLastFetchedAdcRawValue(Index);
  }

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
