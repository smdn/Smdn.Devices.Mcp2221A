// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Provides control over the GP0 pin of the MCP2221/MCP2221A.
/// </summary>
/// <remarks>
/// <para>The GP0 pin supports the following functions:</para>
/// <list type="bullet">
/// <item><description><b>GPIO:</b> General purpose input/output (GPIO0).</description></item>
/// <item><description><b>SSPND:</b> USB suspend indicator.</description></item>
/// <item><description><b>LED_URX:</b> UART receive LED indicator.</description></item>
/// </list>
/// </remarks>
public sealed class Gp0Controller : GpController {
  /// <inheritdoc/>
  /// <value>
  /// Always <c>0</c>.
  /// </value>
  public override int Index { get; } = 0;

  /// <inheritdoc/>
  /// <value>
  /// Always <c>GP0</c>.
  /// </value>
  public override string PinName { get; } = "GP0";

  /// <inheritdoc/>
  public override GpFunction CurrentFunction => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => GpFunction.Gpio, // GPIO
    GpDesignation.DedicatedFunctionOperation => GpFunction.UsbSuspendStatus, // SSPND
    GpDesignation.AlternateFunction0 => GpFunction.LedOutput, // LED_URX
    var unsupported => throw CreateUnsupportedGpDesignationException(Index, unsupported),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO0",
    GpDesignation.DedicatedFunctionOperation => "SSPND",
    GpDesignation.AlternateFunction0 => "LED_URX",
    var unsupported => throw CreateUnsupportedGpDesignationException(Index, unsupported),
  };

  internal Gp0Controller(Mcp2221AGpioDriver gpioDriver)
    : base(gpioDriver)
  {
  }

  private protected override GpDesignation? GetDesignationForFunction(GpFunction function)
    => function switch {
      GpFunction.Gpio => GpDesignation.GpioOperation, // GPIO
      GpFunction.UsbSuspendStatus => GpDesignation.DedicatedFunctionOperation, // SSPND
      GpFunction.LedOutput => GpDesignation.AlternateFunction0, // LED_URX
      _ => null,
    };

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.LedOutput"/>
  public ValueTask ConfigureAsUrxLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.LedOutput"/>
  public void ConfigureAsUrxLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.UsbSuspendStatus"/>
  public ValueTask ConfigureAsUsbSuspendStatusAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="GpController.IsUsedByGpioController"/> is <see langword="true"/>.
  /// </exception>
  /// <seealso cref="GpFunction.UsbSuspendStatus"/>
  public void ConfigureAsUsbSuspendStatus(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
