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
  private protected void ThrowIfInvalidConfiguration(GpFunction requiredFunction)
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
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes not only retrieval operations (e.g., <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or <see cref="Read"/>), but also configuration and write operations (e.g.,
  /// <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>, <see cref="IGpControllerGroup.ApplyGpioStates"/>,
  /// <see cref="ConfigureAsGpio"/> or <see cref="Write"/>).
  /// </para>
  /// <para>
  /// Since the MCP2221A handles logic levels for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's value or mode will refresh the
  /// <see cref="LastUpdatedValue"/> and <see cref="CurrentMode"/> for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettingsAsync"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
  /// <seealso cref="Read(CancellationToken)"/>
  /// <seealso cref="ReadAsync(CancellationToken)"/>
  /// <seealso cref="Write(PinValue, CancellationToken)"/>
  /// <seealso cref="WriteAsync(PinValue, CancellationToken)"/>
  [CLSCompliant(false)]
  public PinValue LastUpdatedValue => GpioDriver.GetLastUpdatedValueOrThrow(gp: Index);

  /// <inheritdoc/>
  /// <remarks>
  /// <para>
  /// This property returns a cached value and does not perform new
  /// I/O communication. To retrieve the most up-to-date status directly from
  /// the hardware, call <see cref="IGpControllerGroup.FetchGpioStates"/>
  /// or pin-specific retrieval methods.
  /// </para>
  /// <para>
  /// This property is updated whenever the state of the GP pins is synchronized.
  /// This includes both retrieval operations (e.g., <see cref="IGpControllerGroup.FetchGpioStates"/>)
  /// and configuration operations (e.g., <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// or <see cref="SetMode"/>).
  /// Since the mode is determined solely by these software operations and
  /// does not change spontaneously on the hardware, this property reflects
  /// the true current state of the pin's I/O direction.
  /// </para>
  /// <para>
  /// Since the MCP2221A handles I/O modes for all GP pins (GP0-GP3)
  /// simultaneously, an update to any pin's mode or value will refresh the
  /// <see cref="CurrentMode"/> and <see cref="LastUpdatedValue"/> for all pins at once.
  /// </para>
  /// <para>
  /// When you need to obtain the status of multiple GP pins at the same time,
  /// you can minimize communication overhead by calling a retrieval method on just one
  /// pin and then referencing this property for the other pins, rather than calling
  /// methods on each pin individually.
  /// </para>
  /// </remarks>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettings"/>
  /// <seealso cref="IGpControllerGroup.ConfigureAllGpSettingsAsync"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
  /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
  /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
  /// <seealso cref="GetMode(CancellationToken)"/>
  /// <seealso cref="GetModeAsync(CancellationToken)"/>
  /// <seealso cref="SetMode(PinMode, CancellationToken)"/>
  /// <seealso cref="SetModeAsync(PinMode, CancellationToken)"/>
  [CLSCompliant(false)]
  public PinMode CurrentMode => GpioDriver.GetLastUpdatedDirectionOrThrow(gp: Index);

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

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
    GpioDriver.ThrowIfDisposed();

    ThrowIfInvalidConfiguration(GpFunction.Gpio);

    GpioDriver.ApplyGpioStates(
      pinValuePairs: [new(Index, value)],
      pinModePairs: default,
      cancellationToken: cancellationToken
    );
  }
}
