// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

namespace Smdn.Devices.Mcp2221A.Configurations;

/// <summary>
/// Defines a factory interface for creating <see cref="IFlashMemory"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// This interface is typically resolved from an <see cref="IServiceProvider"/>
/// and used internally by <see cref="FlashSettings"/> to instantiate the underlying
/// memory buffers for current and staged Flash configurations.
/// </para>
/// <para>
/// <b>Note:</b> This interface is primarily intended for dependency injection and
/// testability, allowing test suites to inject custom <see cref="IFlashMemory"/>
/// implementations to inspect or mock Flash operations.
/// </para>
/// </remarks>
public interface IFlashMemoryFactory {
  /// <summary>
  /// Creates a new, uninitialized <see cref="IFlashMemory"/> instance.
  /// </summary>
  /// <returns>
  /// A newly created <see cref="IFlashMemory"/> instance that can be
  /// populated with Flash memory data.
  /// </returns>
  IFlashMemory Create();
}
