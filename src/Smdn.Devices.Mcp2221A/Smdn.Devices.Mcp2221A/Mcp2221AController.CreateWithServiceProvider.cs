// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController {
#pragma warning restore IDE0040
  /// <summary>
  /// Finds and opens a <see cref="Mcp2221AController"/> device that is the first found among the
  /// USB HID devices on the system, and creates an instance for it asynchronously.
  /// </summary>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a <see cref="Mcp2221AController"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No MCP2221/MCP2221 was found on the system.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="CreateAsync(IUsbHidDevice, bool, IServiceProvider?, CancellationToken)"/>
  public static ValueTask<Mcp2221AController> CreateAsync(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default
  )
    => CreateFromFirstUsbHidDeviceAsyncCore(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: (object?)null,
      usbHidDeviceFilter: null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221AController"/> device that is the first found among the
  /// USB HID devices on the system, and creates an instance for it.
  /// </summary>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AController"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No MCP2221/MCP2221 was found on the system.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="Create(IUsbHidDevice, bool, IServiceProvider?, CancellationToken)"/>
  public static Mcp2221AController Create(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default
  )
    => CreateFromFirstUsbHidDeviceCore(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: (object?)null,
      usbHidDeviceFilter: null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221AController"/> device that is the first found among the
  /// USB HID devices on the system, and creates an instance for it asynchronously.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the <see cref="IUsbHidService"/> to be obtained
  /// from the <paramref name="serviceProvider"/>. If a keyed service is not found,
  /// it attempts to resolve a non-keyed <see cref="IUsbHidService"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a <see cref="Mcp2221AController"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No MCP2221/MCP2221 was found on the system.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="CreateAsync{TServiceKey}(IUsbHidDevice, IServiceProvider?, TServiceKey, bool, CancellationToken)"/>
  public static ValueTask<Mcp2221AController> CreateAsync<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey serviceKey,
    CancellationToken cancellationToken = default
  )
    => CreateFromFirstUsbHidDeviceAsyncCore(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: serviceKey,
      usbHidDeviceFilter: null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221AController"/> device that is the first found among the
  /// USB HID devices on the system, and creates an instance for it.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the <see cref="IUsbHidService"/> to be obtained
  /// from the <paramref name="serviceProvider"/>. If a keyed service is not found,
  /// it attempts to resolve a non-keyed <see cref="IUsbHidService"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AController"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No MCP2221/MCP2221 was found on the system.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="Create{TServiceKey}(IUsbHidDevice, IServiceProvider?, TServiceKey, bool, CancellationToken)"/>
  public static Mcp2221AController Create<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey serviceKey,
    CancellationToken cancellationToken = default
  )
    => CreateFromFirstUsbHidDeviceCore(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: serviceKey,
      usbHidDeviceFilter: null,
      cancellationToken: cancellationToken
    );
}
