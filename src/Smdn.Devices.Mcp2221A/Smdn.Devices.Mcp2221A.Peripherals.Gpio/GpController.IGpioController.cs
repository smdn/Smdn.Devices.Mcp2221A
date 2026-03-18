// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Device.Gpio;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpController : IGpioController {
#pragma warning restore IDE0040
  protected void ThrowIfInvalidConfiguration(GpFunction requiredFunction)
  {
    if (CurrentFunction != requiredFunction)
      throw new InvalidOperationException($"{requiredFunction} operation cannot be performed with the pin currently configured as {CurrentFunction} (GP{Index}: {CurrentDesignation}).");
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  internal static int ThrowDirectionNotSupportedException(PinMode mode)
    => throw new NotSupportedException(
      message: $"The GPIO direction cannot be set to {mode}. The direction must be either {nameof(PinMode.Output)} or {nameof(PinMode.Input)}."
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask ConfigureAsGpioAsync(
    PinMode mode = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.GpioOperation,
      gpioInitialDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void ConfigureAsGpio(
    PinMode mode = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.GpioOperation,
      gpioInitialDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask<PinMode> GetModeAsync(
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    await gpio.UpdateCurrentGpioValuesAsync(cancellationToken).ConfigureAwait(false);

    return gpio.GetCurrentDirection(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public PinMode GetMode(
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    gpio.UpdateCurrentGpioValues(cancellationToken);

    return gpio.GetCurrentDirection(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask SetModeAsync(
    PinMode mode,
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    var modes = ArrayPool<PinModePair>.Shared.Rent(1);

    try {
      modes[0] = new(Index, mode);

      await gpio.SetGpioOutputValuesAsync(
        values: default,
        modes: modes.AsMemory(0, 1),
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    finally {
      ArrayPool<PinModePair>.Shared.Return(modes);
    }
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void SetMode(
    PinMode mode,
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    gpio.SetGpioOutputValues(
      values: default,
      modes: [new(Index, mode)],
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask<PinValue> ReadAsync(
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    await gpio.UpdateCurrentGpioValuesAsync(cancellationToken).ConfigureAwait(false);

    return gpio.GetCurrentPinValue(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public PinValue Read(
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    gpio.UpdateCurrentGpioValues(cancellationToken);

    return gpio.GetCurrentPinValue(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask WriteAsync(
    PinValue value,
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    var values = ArrayPool<PinValuePair>.Shared.Rent(1);

    try {
      values[0] = new(Index, value);

      await gpio.SetGpioOutputValuesAsync(
        values: values.AsMemory(0, 1),
        modes: default,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    finally {
      ArrayPool<PinValuePair>.Shared.Return(values);
    }
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void Write(
    PinValue value,
    CancellationToken cancellationToken = default
  )
  {
    gpio.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    gpio.SetGpioOutputValues(
      values: [new(Index, value)],
      modes: default,
      cancellationToken: cancellationToken
    );
  }
}
