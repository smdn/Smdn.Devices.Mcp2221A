// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Defines the functions that can be assigned to the General Purpose (GP) pins.
/// </summary>
/// <seealso cref="GpController.IsFunctionSupported"/>
public enum GpFunction {
  /// <summary>
  /// General Purpose Input/Output (GPIO).
  /// </summary>
  /// <remarks>
  /// Supported on: GP0, GP1, GP2, GP3.
  /// </remarks>
  /// <seealso cref="Mcp2221AController.GpPin0"/>
  /// <seealso cref="Mcp2221AController.GpPin1"/>
  /// <seealso cref="Mcp2221AController.GpPin2"/>
  /// <seealso cref="Mcp2221AController.GpPin3"/>
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
  /// <seealso cref="Mcp2221AController.GpPin1"/>
  /// <seealso cref="Mcp2221AController.GpPin2"/>
  /// <seealso cref="Mcp2221AController.GpPin3"/>
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
  /// <seealso cref="Mcp2221AController.GpPin2"/>
  /// <seealso cref="Mcp2221AController.GpPin3"/>
  Dac,

  /// <summary>
  /// Interrupt-on-Change (IOC) input.
  /// </summary>
  /// <remarks>
  /// Supported on: GP1 (IOC).
  /// This function detects signal edges (rising, falling, or both) on the
  /// pin and sets a detection flag.
  /// </remarks>
  /// <seealso cref="Mcp2221AController.GpPin1"/>
  InterruptOnChange,

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
  /// <seealso cref="Mcp2221AController.GpPin0"/>
  /// <seealso cref="Mcp2221AController.GpPin1"/>
  /// <seealso cref="Mcp2221AController.GpPin3"/>
  LedOutput,

  /// <summary>
  /// Reference Clock output.
  /// </summary>
  /// <remarks>
  /// Supported on: GP1 (CLK_OUT).
  /// </remarks>
  /// <seealso cref="Mcp2221AController.GpPin1"/>
  ClockOutput,

  /// <summary>
  /// USB Suspend state indicator.
  /// </summary>
  /// <remarks>
  /// Supported on: GP0 (SSPND).
  /// </remarks>
  /// <seealso cref="Mcp2221AController.GpPin0"/>
  UsbSuspendStatus,

  /// <summary>
  /// USB Configuration status indicator.
  /// </summary>
  /// <remarks>
  /// Supported on: GP2 (USBCFG).
  /// </remarks>
  /// <seealso cref="Mcp2221AController.GpPin2"/>
  UsbConfigureStatus,
}
