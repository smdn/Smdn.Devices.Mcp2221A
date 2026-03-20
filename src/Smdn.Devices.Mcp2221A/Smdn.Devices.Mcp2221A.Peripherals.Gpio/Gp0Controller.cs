// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

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
    _ => throw new NotSupportedException(),
  };

  /// <inheritdoc/>
  public override string CurrentDesignation => CurrentGpDesignation switch {
    GpDesignation.GpioOperation => "GPIO0",
    GpDesignation.DedicatedFunctionOperation => "SSPND",
    GpDesignation.AlternateFunction0 => "LED_URX",
    _ => throw new NotSupportedException(),
  };

  internal Gp0Controller(Mcp2221AGpioDriver gpio)
    : base(gpio)
  {
  }

  private protected override GpDesignation? GetDesignationForFunction(GpFunction function)
    => function switch {
      GpFunction.Gpio => GpDesignation.GpioOperation, // GPIO
      GpFunction.UsbSuspendStatus => GpDesignation.DedicatedFunctionOperation, // SSPND
      GpFunction.LedOutput => GpDesignation.AlternateFunction0, // LED_URX
      _ => null,
    };

  /// <seealso cref="GpFunction.LedOutput"/>
  public ValueTask ConfigureAsUrxLedOutputAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <seealso cref="GpFunction.LedOutput"/>
  public void ConfigureAsUrxLedOutput(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.AlternateFunction0,
      cancellationToken: cancellationToken
    );

  /// <seealso cref="GpFunction.UsbSuspendStatus"/>
  public ValueTask ConfigureAsUsbSuspendStatusAsync(CancellationToken cancellationToken = default)
    => ConfigureGpDesignationAsync(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );

  /// <seealso cref="GpFunction.UsbSuspendStatus"/>
  public void ConfigureAsUsbSuspendStatus(CancellationToken cancellationToken = default)
    => ConfigureGpDesignation(
      gpDesignation: GpDesignation.DedicatedFunctionOperation,
      cancellationToken: cancellationToken
    );
}
