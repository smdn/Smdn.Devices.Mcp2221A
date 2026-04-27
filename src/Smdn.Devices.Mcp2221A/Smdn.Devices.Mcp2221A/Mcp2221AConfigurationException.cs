// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// The exception that is thrown when an operation is attempted on a General
/// Purpose (GP) pin that is not configured for the required function.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when the configuration (<see cref="GpFunction"/>)
/// of a GP pin required to perform an operation does not match the current
/// configuration of the device.
/// </para>
/// <para>
/// This includes operations such as:
/// <list type="bullet">
/// <item>
///   GPIO: Changing digital output levels or I/O modes, and reading
///   input levels or current modes.
/// </item>
/// <item>DAC/ADC: Setting or getting analog input/output values.</item>
/// <item>IOC: Reading or clearing interrupt detection flags.</item>
/// <item>Clock Output: Changing clock frequency or duty cycle.</item>
/// </list>
/// </para>
/// <para>
/// The index of the GP pin that caused the exception can be referred to
/// via the <see cref="GpIndex"/> property, and the function required for
/// the operation can be referred to via the <see cref="RequiredFunction"/>
/// property.
/// </para>
/// </remarks>
public class Mcp2221AConfigurationException : InvalidOperationException {
  private const string DefaultMessage = "The requested operation cannot be performed with the current configuration.";

  /// <summary>
  /// Gets the index of the GP pin (<c>0</c>-<c>3</c>) that caused the
  /// exception.
  /// </summary>
  public int? GpIndex { get; }

  /// <summary>
  /// Gets the <see cref="GpFunction"/> that must be assigned to the GP
  /// pin to perform the requested operation.
  /// </summary>
  /// <seealso cref="GpIndex"/>
  /// <seealso cref="GpFunction"/>
  public GpFunction? RequiredFunction { get; }

  /// <summary>
  /// Gets the <see cref="GpFunction"/> currently assigned to the GP pin
  /// on which the requested operation was attempted.
  /// </summary>
  /// <seealso cref="GpIndex"/>
  /// <seealso cref="GpFunction"/>
  public GpFunction? CurrentFunction { get; }

  public Mcp2221AConfigurationException()
    : base(DefaultMessage)
  {
  }

  public Mcp2221AConfigurationException(string? message)
    : base(message ?? DefaultMessage)
  {
  }

  public Mcp2221AConfigurationException(string? message, Exception? innerException)
    : base(message ?? DefaultMessage, innerException)
  {
  }

  internal Mcp2221AConfigurationException(
    int gpIndex,
    GpFunction requiredFunction,
    GpFunction? currentFunction = null
  )
    : base(
      message: currentFunction.HasValue
        ? $"To perform this operation, GP{gpIndex} must be configured to {requiredFunction}, but the current configuration is {currentFunction}."
        : $"To perform this operation, GP{gpIndex} must be configured to {requiredFunction}."
    )
  {
    GpIndex = gpIndex;
    RequiredFunction = requiredFunction;
    CurrentFunction = currentFunction;
  }
}
