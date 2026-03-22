// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
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
  }
}
