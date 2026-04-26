// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;

namespace Smdn.Devices.Mcp2221A;

public class Mcp2221AConfigurationException : InvalidOperationException {
  private const string DefaultMessage = "The requested operation cannot be performed with the current configuration.";

  /// <summary>
  /// Gets the value representing the GP pin number of the target
  /// on which the requested operation was attempted.
  /// </summary>
  public int? GpIndex { get; }

  /// <summary>
  /// Gets the value of <see cref="GpFunction"/>, which represents the function
  /// that must be assigned to the GP pin for which the requested operation
  /// is to be performed.
  /// </summary>
  /// <seealso cref="GpIndex"/>
  public GpFunction? RequiredFunction { get; }

  /// <summary>
  /// Gets the value of <see cref="GpFunction"/> representing the function
  /// currently assigned to the GP pin on which the requested operation
  /// was attempted.
  /// </summary>
  /// <seealso cref="GpIndex"/>
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
