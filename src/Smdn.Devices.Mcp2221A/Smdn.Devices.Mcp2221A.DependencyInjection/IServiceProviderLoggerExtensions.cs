// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Smdn.Devices.Mcp2221A.DependencyInjection;

/// <summary>
/// Provides extension members for the <see cref="IServiceProvider"/> interface.
/// </summary>
public static class IServiceProviderLoggerExtensions {
  /// <summary>
  /// Retrieves a keyed <see cref="ILogger{T}"/> or <see cref="ILogger"/>
  /// from the service provider, or creates a new one using a registered
  /// <see cref="ILoggerFactory"/>.
  /// </summary>
  /// <typeparam name="T">
  /// The type whose name will be used as the logger category name.
  /// </typeparam>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> to retrieve the services from.
  /// </param>
  /// <param name="serviceKey">
  /// The key used to identify the specific logger or factory.
  /// </param>
  /// <returns>
  /// An <see cref="ILogger"/> instance if found or created;
  /// otherwise, <see langword="null"/> if the <paramref name="serviceProvider"/>
  /// is <see langword="null"/> or no logging services are registered.
  /// </returns>
  /// <remarks>
  /// This method attempts to resolve the logger in the following order of priority:
  /// <list type="number">
  /// <item>
  /// <description>A keyed <see cref="ILogger{T}"/> service.</description>
  /// </item>
  /// <item>
  /// <description>A keyed non-generic <see cref="ILogger"/> service.</description>
  /// </item>
  /// <item>
  /// <description>
  /// An <see cref="ILogger"/> created from a keyed <see cref="ILoggerFactory"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// An <see cref="ILogger"/> created from the default (non-keyed)
  /// <see cref="ILoggerFactory"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </remarks>
  public static ILogger? GetKeyedLoggerOrCreate<T>(
    this IServiceProvider? serviceProvider,
    object? serviceKey
  )
  {
    if (serviceProvider is null)
      return null;

    if (serviceProvider.GetKeyedService<ILogger<T>>(serviceKey) is { } typedLogger)
      return typedLogger; // returns keyed ILogger<T>, if registered
    if (serviceProvider.GetKeyedService<ILogger>(serviceKey) is { } logger)
      return logger; // returns keyed ILogger, if registered

    var loggerFactory =
      serviceProvider.GetKeyedService<ILoggerFactory>(serviceKey) ??
      serviceProvider.GetService<ILoggerFactory>();

    // if an ILoggerFactory with or without a key is registered,
    // use it to create and return an ILogger
    return loggerFactory?.CreateLogger<T>();
  }
}
