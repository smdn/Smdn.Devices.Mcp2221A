// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.ComponentModel;
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
    if (CurrentFunction != requiredFunction) {
      throw new Mcp2221AConfigurationException(
        gpIndex: Index,
        requiredFunction: requiredFunction,
        currentFunction: CurrentFunction
      );
    }
  }

#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
  [DoesNotReturn]
#endif
  internal static int ThrowDirectionNotSupportedOrInvalidException(PinMode mode, string paramName)
  {
    if (mode == PinMode.InputPullDown || mode == PinMode.InputPullUp) {
      throw new NotSupportedException(
        message: $"The GPIO direction cannot be set to {mode}. The direction must be either {nameof(PinMode.Output)} or {nameof(PinMode.Input)}."
      );
    }

    throw new InvalidEnumArgumentException(
      argumentName: paramName,
      invalidValue: (int)mode,
      enumClass: typeof(PinMode)
    );
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask ConfigureAsGpioAsync(
    PinMode? mode = PinMode.Output,
    PinValue? initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.GpioOperation,
      gpioDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void ConfigureAsGpio(
    PinMode? mode = PinMode.Output,
    PinValue? initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.GpioOperation,
      gpioDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  /// <seealso cref="CurrentMode"/>
  /// <seealso cref="LastUpdatedValue"/>
  [CLSCompliant(false)]
  public async ValueTask<PinMode> GetModeAsync(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    await GpioDriver.FetchGpioStatesAsync(default, default, cancellationToken).ConfigureAwait(false);

    return GpioDriver.GetLastUpdatedDirectionOrThrow(gp: Index);
  }

  /// <inheritdoc/>
  /// <seealso cref="CurrentMode"/>
  /// <seealso cref="LastUpdatedValue"/>
  [CLSCompliant(false)]
  public PinMode GetMode(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    GpioDriver.FetchGpioStates(default, default, cancellationToken);

    return GpioDriver.GetLastUpdatedDirectionOrThrow(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask SetModeAsync(
    PinMode mode,
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    var modes = ArrayPool<PinModePair>.Shared.Rent(1);

    try {
      modes[0] = new(Index, mode);

      await GpioDriver.ApplyGpioStatesAsync(
        pinValuePairs: default,
        pinModePairs: modes.AsMemory(0, 1),
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
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    GpioDriver.ApplyGpioStates(
      pinValuePairs: default,
      pinModePairs: [new(Index, mode)],
      cancellationToken: cancellationToken
    );
  }

  /// <inheritdoc/>
  /// <seealso cref="CurrentMode"/>
  /// <seealso cref="LastUpdatedValue"/>
  [CLSCompliant(false)]
  public async ValueTask<PinValue> ReadAsync(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    await GpioDriver.FetchGpioStatesAsync(default, default, cancellationToken).ConfigureAwait(false);

    return GpioDriver.GetLastUpdatedValueOrThrow(gp: Index);
  }

  /// <inheritdoc/>
  /// <seealso cref="CurrentMode"/>
  /// <seealso cref="LastUpdatedValue"/>
  [CLSCompliant(false)]
  public PinValue Read(
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    GpioDriver.FetchGpioStates(default, default, cancellationToken);

    return GpioDriver.GetLastUpdatedValueOrThrow(gp: Index);
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public async ValueTask WriteAsync(
    PinValue value,
    CancellationToken cancellationToken = default
  )
  {
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    var values = ArrayPool<PinValuePair>.Shared.Rent(1);

    try {
      values[0] = new(Index, value);

      await GpioDriver.ApplyGpioStatesAsync(
        pinValuePairs: values.AsMemory(0, 1),
        pinModePairs: default,
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
    GpioDriver.Transceiver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    GpioDriver.ApplyGpioStates(
      pinValuePairs: [new(Index, value)],
      pinModePairs: default,
      cancellationToken: cancellationToken
    );
  }
}
