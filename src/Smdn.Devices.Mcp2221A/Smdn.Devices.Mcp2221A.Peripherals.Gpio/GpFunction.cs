// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

/// <summary>
/// Defines the functions that can be assigned to the General Purpose (GP) pins.
/// </summary>
public enum GpFunction {
  /// <summary>
  /// General Purpose Input/Output (GPIO).
  /// </summary>
  /// <remarks>
  /// Supported on: GP0, GP1, GP2, GP3.
  /// </remarks>
  Gpio,

  /// <summary>
  /// Analog-to-Digital Converter (ADC) input.
  /// </summary>
  /// <remarks>
  /// Supported on:
  /// <list type="bullet">
  /// <item><description>GP1 (ADC1)</description></item>
  /// <item><description>GP2 (ADC2)</description></item>
  /// <item><description>GP3 (ADC3)</description></item>
  /// </list>
  /// </remarks>
  Adc,

  /// <summary>
  /// Digital-to-Analog Converter (DAC) output.
  /// </summary>
  /// <remarks>
  /// Supported on:
  /// <list type="bullet">
  /// <item><description>GP2 (DAC1)</description></item>
  /// <item><description>GP3 (DAC2)</description></item>
  /// </list>
  /// </remarks>
  Dac,

  /// <summary>
  /// External Interrupt-on-Change (IOC) input.
  /// </summary>
  /// <remarks>
  /// Supported on: GP1 (IOC).
  /// </remarks>
  ExternalInterrupt,

  /// <summary>
  /// UART or I2C Status LED output.
  /// </summary>
  /// <remarks>
  /// Supported on:
  /// <list type="bullet">
  /// <item><description>GP0 (LED_URX)</description></item>
  /// <item><description>GP1 (LED_UTX)</description></item>
  /// <item><description>GP3 (LED_I2C)</description></item>
  /// </list>
  /// </remarks>
  LedOutput,

  /// <summary>
  /// Reference Clock output.
  /// </summary>
  /// <remarks>
  /// Supported on: GP1 (CLK_OUT).
  /// </remarks>
  ClockOutput,

  /// <summary>
  /// USB Suspend state indicator.
  /// </summary>
  /// <remarks>
  /// Supported on: GP0 (SSPND).
  /// </remarks>
  UsbSuspendStatus,

  /// <summary>
  /// USB Configuration status indicator.
  /// </summary>
  /// <remarks>
  /// Supported on: GP2 (USBCFG).
  /// </remarks>
  UsbConfigureStatus,
}
