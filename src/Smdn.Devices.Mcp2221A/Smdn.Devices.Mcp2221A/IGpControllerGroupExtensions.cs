// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides extension methods for <see cref="IGpControllerGroup"/>.
/// </summary>
public static class IGpControllerGroupExtensions {
  private static void ThrowIfThisArgumentIsNull(IGpControllerGroup gpPins, string paramName)
  {
    if (gpPins is null)
      throw new ArgumentNullException(paramName: paramName);
  }

#pragma warning disable CA1034
  extension(IGpControllerGroup gpPins) {
#pragma warning restore CA1034
    /// <summary>
    /// Configures the assigned functions (GPIO, ADC, DAC, etc.) for all
    /// GP pins (GP0-GP3) in a single communication.
    /// </summary>
    /// <param name="gp0Function">
    /// The function to assign to GP0.
    /// If <see langword="null"/>, the current function is maintained.
    /// </param>
    /// <param name="gp1Function">
    /// The function to assign to GP1.
    /// If <see langword="null"/>, the current function is maintained.
    /// </param>
    /// <param name="gp2Function">
    /// The function to assign to GP2.
    /// If <see langword="null"/>, the current function is maintained.
    /// </param>
    /// <param name="gp3Function">
    /// The function to assign to GP3.
    /// If <see langword="null"/>, the current function is maintained.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method updates the SRAM settings of the MCP2221A. If all function
    /// parameters are <see langword="null"/>, the method returns immediately without
    /// performing any communication.
    /// </para>
    /// <para>
    /// Note that each GP pin supports a different set of functions. If a function
    /// not supported by a specific pin is specified, a <see cref="NotSupportedException"/>
    /// will be thrown during validation.
    /// </para>
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Thrown when a function is not supported by the specified GP pin.
    /// </exception>
    [CLSCompliant(false)]
    public void ConfigureAllGpFunctions(
      GpFunction? gp0Function = default,
      GpFunction? gp1Function = default,
      GpFunction? gp2Function = default,
      GpFunction? gp3Function = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.ConfigureAllGpSettings(
        gp0Function: gp0Function,
        gp1Function: gp1Function,
        gp2Function: gp2Function,
        gp3Function: gp3Function,
        cancellationToken: cancellationToken
      );
    }

    /// <inheritdoc cref="ConfigureAllGpFunctions(IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, CancellationToken)"/>
    /// <summary>
    /// Asynchronously configures the assigned functions (GPIO, ADC, DAC, etc.)
    /// for all GP pins (GP0-GP3) in a single communication.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    [CLSCompliant(false)]
    public ValueTask ConfigureAllGpFunctionsAsync(
      GpFunction? gp0Function = default,
      GpFunction? gp1Function = default,
      GpFunction? gp2Function = default,
      GpFunction? gp3Function = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.ConfigureAllGpSettingsAsync(
        gp0Function: gp0Function,
        gp1Function: gp1Function,
        gp2Function: gp2Function,
        gp3Function: gp3Function,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Configures all GP pins (GP0-GP3) to function as GPIO and sets their modes and initial values in a single communication.
    /// </summary>
    /// <param name="gp0Mode">
    /// The <see cref="PinMode"/> for GP0.
    /// If <see langword="null"/>, the current mode is maintained.
    /// </param>
    /// <param name="gp0InitialValue">
    /// The initial <see cref="PinValue"/> for GP0.
    /// If <see langword="null"/>, the current value is maintained.
    /// </param>
    /// <param name="gp1Mode">
    /// The <see cref="PinMode"/> for GP1.
    /// If <see langword="null"/>, the current mode is maintained.
    /// </param>
    /// <param name="gp1InitialValue">
    /// The initial <see cref="PinValue"/> for GP1.
    /// If <see langword="null"/>, the current value is maintained.
    /// </param>
    /// <param name="gp2Mode">
    /// The <see cref="PinMode"/> for GP2.
    /// If <see langword="null"/>, the current mode is maintained.
    /// </param>
    /// <param name="gp2InitialValue">
    /// The initial <see cref="PinValue"/> for GP2.
    /// If <see langword="null"/>, the current value is maintained.
    /// </param>
    /// <param name="gp3Mode">
    /// The <see cref="PinMode"/> for GP3.
    /// If <see langword="null"/>, the current mode is maintained.
    /// </param>
    /// <param name="gp3InitialValue">
    /// The initial <see cref="PinValue"/> for GP3.
    /// If <see langword="null"/>, the current value is maintained.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method ensures that all GP pins are assigned the GPIO function.
    /// If a mode or initial value is <see langword="null"/>, the device's current
    /// setting for that parameter is preserved.
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    public void ConfigureAllAsGpio(
      PinMode? gp0Mode = default,
      PinValue? gp0InitialValue = default,
      PinMode? gp1Mode = default,
      PinValue? gp1InitialValue = default,
      PinMode? gp2Mode = default,
      PinValue? gp2InitialValue = default,
      PinMode? gp3Mode = default,
      PinValue? gp3InitialValue = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.ConfigureAllGpSettings(
        gp0Function: GpFunction.Gpio,
        gp0Mode: gp0Mode,
        gp0InitialValue: gp0InitialValue,
        gp1Function: GpFunction.Gpio,
        gp1Mode: gp1Mode,
        gp1InitialValue: gp1InitialValue,
        gp2Function: GpFunction.Gpio,
        gp2Mode: gp2Mode,
        gp2InitialValue: gp2InitialValue,
        gp3Function: GpFunction.Gpio,
        gp3Mode: gp3Mode,
        gp3InitialValue: gp3InitialValue,
        cancellationToken: cancellationToken
      );
    }

    /// <inheritdoc cref="ConfigureAllAsGpio(IGpControllerGroup, PinMode?, PinValue?, PinMode?, PinValue?, PinMode?, PinValue?, PinMode?, PinValue?, CancellationToken)"/>
    /// <summary>
    /// Asynchronously configures all GP pins (GP0-GP3) to function as GPIO and
    /// sets their modes and initial values in a single communication.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    [CLSCompliant(false)]
    public ValueTask ConfigureAllAsGpioAsync(
      PinMode? gp0Mode = default,
      PinValue? gp0InitialValue = default,
      PinMode? gp1Mode = default,
      PinValue? gp1InitialValue = default,
      PinMode? gp2Mode = default,
      PinValue? gp2InitialValue = default,
      PinMode? gp3Mode = default,
      PinValue? gp3InitialValue = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.ConfigureAllGpSettingsAsync(
        gp0Function: GpFunction.Gpio,
        gp0Mode: gp0Mode,
        gp0InitialValue: gp0InitialValue,
        gp1Function: GpFunction.Gpio,
        gp1Mode: gp1Mode,
        gp1InitialValue: gp1InitialValue,
        gp2Function: GpFunction.Gpio,
        gp2Mode: gp2Mode,
        gp2InitialValue: gp2InitialValue,
        gp3Function: GpFunction.Gpio,
        gp3Mode: gp3Mode,
        gp3InitialValue: gp3InitialValue,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Configures all GP pins (GP0-GP3) to the GPIO function and sets
    /// their modes to <see cref="PinMode.Output"/> in a single communication.
    /// </summary>
    /// <param name="gp0InitialValue">
    /// The initial value for GP0.
    /// If <see langword="null"/>, the current value in the SRAM is maintained.
    /// </param>
    /// <param name="gp1InitialValue">
    /// The initial value for GP1.
    /// If <see langword="null"/>, the current value in the SRAM is maintained.
    /// </param>
    /// <param name="gp2InitialValue">
    /// The initial value for GP2.
    /// If <see langword="null"/>, the current value in the SRAM is maintained.
    /// </param>
    /// <param name="gp3InitialValue">
    /// The initial value for GP3.
    /// If <see langword="null"/>, the current value in the SRAM is maintained.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <remarks>
    /// This is a convenience method that calls <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>
    /// with <see cref="GpFunction.Gpio"/> and <see cref="PinMode.Output"/> for all pins.
    /// </remarks>
    [CLSCompliant(false)]
    public void ConfigureAllAsGpioOutput(
      PinValue? gp0InitialValue = null,
      PinValue? gp1InitialValue = null,
      PinValue? gp2InitialValue = null,
      PinValue? gp3InitialValue = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.ConfigureAllGpSettings(
        gp0Function: GpFunction.Gpio,
        gp0Mode: PinMode.Output,
        gp0InitialValue: gp0InitialValue,
        gp1Function: GpFunction.Gpio,
        gp1Mode: PinMode.Output,
        gp1InitialValue: gp1InitialValue,
        gp2Function: GpFunction.Gpio,
        gp2Mode: PinMode.Output,
        gp2InitialValue: gp2InitialValue,
        gp3Function: GpFunction.Gpio,
        gp3Mode: PinMode.Output,
        gp3InitialValue: gp3InitialValue,
        cancellationToken: cancellationToken
      );
    }

    /// <inheritdoc cref="ConfigureAllAsGpioOutput(IGpControllerGroup, PinValue?, PinValue?, PinValue?, PinValue?, CancellationToken)"/>
    /// <summary>
    /// Asynchronously configures all GP pins (GP0-GP3) to the GPIO function
    /// and sets their modes to <see cref="PinMode.Output"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    [CLSCompliant(false)]
    public ValueTask ConfigureAllAsGpioOutputAsync(
      PinValue? gp0InitialValue = null,
      PinValue? gp1InitialValue = null,
      PinValue? gp2InitialValue = null,
      PinValue? gp3InitialValue = null,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.ConfigureAllGpSettingsAsync(
        gp0Function: GpFunction.Gpio,
        gp0Mode: PinMode.Output,
        gp0InitialValue: gp0InitialValue,
        gp1Function: GpFunction.Gpio,
        gp1Mode: PinMode.Output,
        gp1InitialValue: gp1InitialValue,
        gp2Function: GpFunction.Gpio,
        gp2Mode: PinMode.Output,
        gp2InitialValue: gp2InitialValue,
        gp3Function: GpFunction.Gpio,
        gp3Mode: PinMode.Output,
        gp3InitialValue: gp3InitialValue,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Configures all GP pins (GP0-GP3) to the GPIO function and sets
    /// their modes to <see cref="PinMode.Input"/> in a single communication.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <remarks>
    /// This is a convenience method that calls <see cref="IGpControllerGroup.ConfigureAllGpSettings"/>
    /// with <see cref="GpFunction.Gpio"/> and <see cref="PinMode.Input"/> for all pins.
    /// The logic levels (initial values) in the SRAM settings are maintained as-is.
    /// </remarks>
    [CLSCompliant(false)]
    public void ConfigureAllAsGpioInput(
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.ConfigureAllGpSettings(
        gp0Function: GpFunction.Gpio,
        gp0Mode: PinMode.Input,
        gp0InitialValue: null,
        gp1Function: GpFunction.Gpio,
        gp1Mode: PinMode.Input,
        gp1InitialValue: null,
        gp2Function: GpFunction.Gpio,
        gp2Mode: PinMode.Input,
        gp2InitialValue: null,
        gp3Function: GpFunction.Gpio,
        gp3Mode: PinMode.Input,
        gp3InitialValue: null,
        cancellationToken: cancellationToken
      );
    }

    /// <inheritdoc cref="ConfigureAllAsGpioInput(IGpControllerGroup, CancellationToken)"/>
    /// <summary>
    /// Asynchronously configures all GP pins (GP0-GP3) to the GPIO function
    /// and sets their modes to <see cref="PinMode.Input"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    [CLSCompliant(false)]
    public ValueTask ConfigureAllAsGpioInputAsync(
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.ConfigureAllGpSettingsAsync(
        gp0Function: GpFunction.Gpio,
        gp0Mode: PinMode.Input,
        gp0InitialValue: null,
        gp1Function: GpFunction.Gpio,
        gp1Mode: PinMode.Input,
        gp1InitialValue: null,
        gp2Function: GpFunction.Gpio,
        gp2Mode: PinMode.Input,
        gp2InitialValue: null,
        gp3Function: GpFunction.Gpio,
        gp3Mode: PinMode.Input,
        gp3InitialValue: null,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Reads the current digital logic levels for the specified pins
    /// in a single communication.
    /// </summary>
    /// <param name="pinValuePairs">
    /// A span of <see cref="PinValuePair"/> to specify target pins and
    /// receive their logic levels.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method calls <see cref="IGpControllerGroup.FetchGpioStates"/> with an
    /// empty mode span. Even if <paramref name="pinValuePairs"/> is empty,
    /// it still performs the communication to re-fetch the device settings and
    /// synchronize the internal cache states. The I/O modes of the pins are
    /// maintained and not changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
    [CLSCompliant(false)]
    public void Read(
      Span<PinValuePair> pinValuePairs,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.FetchGpioStates(
        pinValuePairs: pinValuePairs,
        pinModePairs: default,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Asynchronously reads the current digital logic levels for the
    /// specified pins in a single communication.
    /// </summary>
    /// <inheritdoc cref="Read(IGpControllerGroup, Span{PinValuePair}, CancellationToken)"/>
    /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
    [CLSCompliant(false)]
    public ValueTask ReadAsync(
      Memory<PinValuePair> pinValuePairs,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.FetchGpioStatesAsync(
        pinValuePairs: pinValuePairs,
        pinModePairs: default,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Reads the current digital logic levels for all GP pins (GP0-GP3)
    /// in a single communication and returns them as a tuple.
    /// </summary>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A tuple containing the <see cref="PinValue"/> of all GP pins.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is a convenience method that synchronizes the state of all pins
    /// and returns their values at once.
    /// </para>
    /// <para>
    /// This method calls <see cref="IGpControllerGroup.FetchGpioStates"/> with an
    /// empty mode span. It performs the communication to re-fetch the device
    /// settings and synchronize the internal cache states. The I/O modes of
    /// the pins are maintained and not changed.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGpControllerGroup.FetchGpioStates"/>
    [CLSCompliant(false)]
    public
    (PinValue Gp0Value, PinValue Gp1Value, PinValue Gp2Value, PinValue Gp3Value)
    Read(CancellationToken cancellationToken = default)
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.FetchGpioStates(
        pinValuePairs: default,
        pinModePairs: default,
        cancellationToken: cancellationToken
      );

      return (
        gpPins.Gp0.LastFetchedValue,
        gpPins.Gp1.LastFetchedValue,
        gpPins.Gp2.LastFetchedValue,
        gpPins.Gp3.LastFetchedValue
      );
    }

    /// <summary>
    /// Asynchronously reads the current digital logic levels for all
    /// GP pins (GP0-GP3) and returns them as a tuple.
    /// </summary>
    /// <inheritdoc cref="Read(IGpControllerGroup, CancellationToken)"/>
    /// <seealso cref="IGpControllerGroup.FetchGpioStatesAsync"/>
    [CLSCompliant(false)]
    public async
    ValueTask<(PinValue Gp0Value, PinValue Gp1Value, PinValue Gp2Value, PinValue Gp3Value)>
    ReadAsync(CancellationToken cancellationToken = default)
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      await gpPins.FetchGpioStatesAsync(
        pinValuePairs: default,
        pinModePairs: default,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return (
        gpPins.Gp0.LastFetchedValue,
        gpPins.Gp1.LastFetchedValue,
        gpPins.Gp2.LastFetchedValue,
        gpPins.Gp3.LastFetchedValue
      );
    }

    /// <summary>
    /// Writes the digital logic levels for the specified pins
    /// in a single communication.
    /// </summary>
    /// <param name="pinValuePairs">
    /// A read-only span of <see cref="PinValuePair"/> containing the
    /// logic levels to be applied.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method calls <see cref="IGpControllerGroup.ApplyGpioStates"/> with
    /// an empty mode span.
    /// </para>
    /// <para>
    /// Even if <paramref name="pinValuePairs"/> is empty, it still performs
    /// the communication to re-apply the current internally cached states to
    /// the physical device. The I/O modes of the pins are maintained and not
    /// changed during this operation.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
    [CLSCompliant(false)]
    public void Write(
      ReadOnlySpan<PinValuePair> pinValuePairs,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      gpPins.ApplyGpioStates(
        pinValuePairs: pinValuePairs,
        pinModePairs: default,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Asynchronously writes the digital logic levels for the
    /// specified pins in a single communication.
    /// </summary>
    /// <inheritdoc cref="Write(IGpControllerGroup, ReadOnlySpan{PinValuePair}, CancellationToken)"/>
    /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
    [CLSCompliant(false)]
    public ValueTask WriteAsync(
      ReadOnlyMemory<PinValuePair> pinValuePairs,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      return gpPins.ApplyGpioStatesAsync(
        pinValuePairs: pinValuePairs,
        pinModePairs: default,
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Writes the digital logic levels for the specified pins
    /// using optional parameters.
    /// </summary>
    /// <param name="gp0Value">
    /// The value for GP0. If <see langword="null"/>, the current state
    /// of the pin is maintained.
    /// </param>
    /// <param name="gp1Value">
    /// The value for GP1. If <see langword="null"/>, the current state
    /// of the pin is maintained.
    /// </param>
    /// <param name="gp2Value">
    /// The value for GP2. If <see langword="null"/>, the current state
    /// of the pin is maintained.
    /// </param>
    /// <param name="gp3Value">
    /// The value for GP3. If <see langword="null"/>, the current state
    /// of the pin is maintained.
    /// </param>
    /// <param name="cancellationToken">
    /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is optimized for named argument usage. It updates only the
    /// specified pins while preserving the current settings for any parameters
    /// that are <see langword="null"/>.
    /// </para>
    /// <para>
    /// Even if all value parameters are <see langword="null"/>, it still calls
    /// <see cref="IGpControllerGroup.ApplyGpioStates"/> to re-apply the current
    /// internally cached states to the physical device. The I/O modes are
    /// maintained and not changed during this operation.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGpControllerGroup.ApplyGpioStates"/>
    [CLSCompliant(false)]
    public void Write(
      PinValue? gp0Value = default,
      PinValue? gp1Value = default,
      PinValue? gp2Value = default,
      PinValue? gp3Value = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      Span<PinValuePair> pinValuePairs = stackalloc PinValuePair[Mcp2221AGpioDriver.NumberOfGpPins];
      var pinValuePairCount = 0;

      if (gp0Value.HasValue)
        pinValuePairs[pinValuePairCount++] = new(0, gp0Value.Value);

      if (gp1Value.HasValue)
        pinValuePairs[pinValuePairCount++] = new(1, gp1Value.Value);

      if (gp2Value.HasValue)
        pinValuePairs[pinValuePairCount++] = new(2, gp2Value.Value);

      if (gp3Value.HasValue)
        pinValuePairs[pinValuePairCount++] = new(3, gp3Value.Value);

      gpPins.Write(
        pinValuePairs: pinValuePairs.Slice(0, pinValuePairCount),
        cancellationToken: cancellationToken
      );
    }

    /// <summary>
    /// Asynchronously writes the digital logic levels for the specified
    /// pins using optional parameters.
    /// </summary>
    /// <inheritdoc cref="Write(IGpControllerGroup, PinValue?, PinValue?, PinValue?, PinValue?, CancellationToken)"/>
    /// <seealso cref="IGpControllerGroup.ApplyGpioStatesAsync"/>
    [CLSCompliant(false)]
    public async ValueTask WriteAsync(
      PinValue? gp0Value = default,
      PinValue? gp1Value = default,
      PinValue? gp2Value = default,
      PinValue? gp3Value = default,
      CancellationToken cancellationToken = default
    )
    {
      ThrowIfThisArgumentIsNull(gpPins, nameof(gpPins));

      var pinValuePairArray = ArrayPool<PinValuePair>.Shared.Rent(Mcp2221AGpioDriver.NumberOfGpPins);
      var pinValuePairCount = 0;

      if (gp0Value.HasValue)
        pinValuePairArray[pinValuePairCount++] = new(0, gp0Value.Value);

      if (gp1Value.HasValue)
        pinValuePairArray[pinValuePairCount++] = new(1, gp1Value.Value);

      if (gp2Value.HasValue)
        pinValuePairArray[pinValuePairCount++] = new(2, gp2Value.Value);

      if (gp3Value.HasValue)
        pinValuePairArray[pinValuePairCount++] = new(3, gp3Value.Value);

      try {
        await gpPins.WriteAsync(
          pinValuePairs: pinValuePairArray.AsMemory(0, pinValuePairCount),
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      finally {
        ArrayPool<PinValuePair>.Shared.Return(pinValuePairArray);
      }
    }
  }
}
