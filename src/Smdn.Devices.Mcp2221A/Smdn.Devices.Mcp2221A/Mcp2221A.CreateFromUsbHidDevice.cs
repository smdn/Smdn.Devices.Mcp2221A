// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController {
#pragma warning restore IDE0040
  /// <summary>
  /// Creates an instance of <see cref="Mcp2221AController"/> from the specified
  /// <see cref="IUsbHidDevice"/> asynchronously.
  /// </summary>
  /// <param name="usbHidDevice">
  /// The <see cref="IUsbHidDevice"/> to create an instance from.
  /// </param>
  /// <param name="shouldDisposeUsbHidDevice">
  /// <see langword="true"/> to dispose the <paramref name="usbHidDevice"/> when the
  /// created <see cref="Mcp2221AController"/> instance is disposed; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="serviceProvider">
  /// An optional <see cref="IServiceProvider"/> for providing services like logging.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a new <see cref="Mcp2221AController"/> instance.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="usbHidDevice"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="Mcp2221AUnavailableException">
  /// Failed to open the endpoint for the specified <paramref name="usbHidDevice"/>.
  /// This can be due to lack of permissions, the device being disconnected,
  /// or being used by another process. The <see cref="Exception.InnerException"/>
  /// property holds the original exception.
  /// </exception>
  /// <exception cref="Mcp2221ANotSupportedException">
  /// The hardware or firmware revision of the device is not supported.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// If an <see cref="ILoggerFactory"/> is registered in the <paramref name="serviceProvider"/>,
  /// the communication process will be logged.
  /// </para>
  /// <para>
  /// If <paramref name="shouldDisposeUsbHidDevice"/> is set to <see langword="true"/>,
  /// the lifecycle of the provided <paramref name="usbHidDevice"/> is tied to the
  /// created <see cref="Mcp2221AController"/> instance. Disposing the <see cref="Mcp2221AController"/>
  /// instance will also dispose the <paramref name="usbHidDevice"/>.
  /// </para>
  /// </remarks>
  public static ValueTask<Mcp2221AController> CreateAsync(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice = false,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => CreateFromUsbHidDeviceAsyncCore(
      usbHidDevice: usbHidDevice ?? throw new ArgumentNullException(nameof(usbHidDevice)),
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      serviceKey: (object?)null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Creates an instance of <see cref="Mcp2221AController"/> from the specified
  /// <see cref="IUsbHidDevice"/>.
  /// </summary>
  /// <param name="usbHidDevice">
  /// The <see cref="IUsbHidDevice"/> to create an instance from.
  /// </param>
  /// <param name="shouldDisposeUsbHidDevice">
  /// <see langword="true"/> to dispose the <paramref name="usbHidDevice"/> when the
  /// created <see cref="Mcp2221AController"/> instance is disposed; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="serviceProvider">
  /// An optional <see cref="IServiceProvider"/> for providing services like logging.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AController"/> instance.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="usbHidDevice"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="Mcp2221AUnavailableException">
  /// Failed to open the endpoint for the specified <paramref name="usbHidDevice"/>.
  /// This can be due to lack of permissions, the device being disconnected,
  /// or being used by another process. The <see cref="Exception.InnerException"/>
  /// property holds the original exception.
  /// </exception>
  /// <exception cref="Mcp2221ANotSupportedException">
  /// The hardware or firmware revision of the device is not supported.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// If an <see cref="ILoggerFactory"/> is registered in the <paramref name="serviceProvider"/>,
  /// the communication process will be logged.
  /// </para>
  /// <para>
  /// If <paramref name="shouldDisposeUsbHidDevice"/> is set to <see langword="true"/>,
  /// the lifecycle of the provided <paramref name="usbHidDevice"/> is tied to the
  /// created <see cref="Mcp2221AController"/> instance. Disposing the <see cref="Mcp2221AController"/>
  /// instance will also dispose the <paramref name="usbHidDevice"/>.
  /// </para>
  /// </remarks>
  public static Mcp2221AController Create(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice = false,
    IServiceProvider? serviceProvider = null,
    CancellationToken cancellationToken = default
  )
    => CreateFromUsbHidDeviceCore(
      usbHidDevice: usbHidDevice ?? throw new ArgumentNullException(nameof(usbHidDevice)),
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      serviceKey: (object?)null,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Creates an instance of <see cref="Mcp2221AController"/> from the specified
  /// <see cref="IUsbHidDevice"/> asynchronously.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="usbHidDevice">
  /// The <see cref="IUsbHidDevice"/> to create an instance from.
  /// </param>
  /// <param name="serviceProvider">
  /// An optional <see cref="IServiceProvider"/> for providing services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the services to be obtained from the <paramref name="serviceProvider"/>.
  /// </param>
  /// <param name="shouldDisposeUsbHidDevice">
  /// <see langword="true"/> to dispose the <paramref name="usbHidDevice"/> when the
  /// created <see cref="Mcp2221AController"/> instance is disposed; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a new <see cref="Mcp2221AController"/> instance.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="usbHidDevice"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="Mcp2221AUnavailableException">
  /// Failed to open the endpoint for the specified <paramref name="usbHidDevice"/>.
  /// This can be due to lack of permissions, the device being disconnected,
  /// or being used by another process. The <see cref="Exception.InnerException"/>
  /// property holds the original exception.
  /// </exception>
  /// <exception cref="Mcp2221ANotSupportedException">
  /// The hardware or firmware revision of the device is not supported.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// If an <see cref="ILoggerFactory"/> is registered in the <paramref name="serviceProvider"/>,
  /// the communication process will be logged.
  /// </para>
  /// <para>
  /// If <paramref name="shouldDisposeUsbHidDevice"/> is set to <see langword="true"/>,
  /// the lifecycle of the provided <paramref name="usbHidDevice"/> is tied to the
  /// created <see cref="Mcp2221AController"/> instance. Disposing the <see cref="Mcp2221AController"/>
  /// instance will also dispose the <paramref name="usbHidDevice"/>.
  /// </para>
  /// </remarks>
  public static ValueTask<Mcp2221AController> CreateAsync<TServiceKey>(
    IUsbHidDevice usbHidDevice,
    IServiceProvider? serviceProvider,
    TServiceKey serviceKey,
    bool shouldDisposeUsbHidDevice = false,
    CancellationToken cancellationToken = default
  )
    => CreateFromUsbHidDeviceAsyncCore(
      usbHidDevice: usbHidDevice ?? throw new ArgumentNullException(nameof(usbHidDevice)),
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Creates an instance of <see cref="Mcp2221AController"/> from the specified
  /// <see cref="IUsbHidDevice"/>.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="usbHidDevice">
  /// The <see cref="IUsbHidDevice"/> to create an instance from.
  /// </param>
  /// <param name="serviceProvider">
  /// An optional <see cref="IServiceProvider"/> for providing services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the services to be obtained from the <paramref name="serviceProvider"/>.
  /// </param>
  /// <param name="shouldDisposeUsbHidDevice">
  /// <see langword="true"/> to dispose the <paramref name="usbHidDevice"/> when the
  /// created <see cref="Mcp2221AController"/> instance is disposed; otherwise, <see langword="false"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221AController"/> instance.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="usbHidDevice"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="Mcp2221AUnavailableException">
  /// Failed to open the endpoint for the specified <paramref name="usbHidDevice"/>.
  /// This can be due to lack of permissions, the device being disconnected,
  /// or being used by another process. The <see cref="Exception.InnerException"/>
  /// property holds the original exception.
  /// </exception>
  /// <exception cref="Mcp2221ANotSupportedException">
  /// The hardware or firmware revision of the device is not supported.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// If an <see cref="ILoggerFactory"/> is registered in the <paramref name="serviceProvider"/>,
  /// the communication process will be logged.
  /// </para>
  /// <para>
  /// If <paramref name="shouldDisposeUsbHidDevice"/> is set to <see langword="true"/>,
  /// the lifecycle of the provided <paramref name="usbHidDevice"/> is tied to the
  /// created <see cref="Mcp2221AController"/> instance. Disposing the <see cref="Mcp2221AController"/>
  /// instance will also dispose the <paramref name="usbHidDevice"/>.
  /// </para>
  /// </remarks>
  public static Mcp2221AController Create<TServiceKey>(
    IUsbHidDevice usbHidDevice,
    IServiceProvider? serviceProvider,
    TServiceKey serviceKey,
    bool shouldDisposeUsbHidDevice = false,
    CancellationToken cancellationToken = default
  )
    => CreateFromUsbHidDeviceCore(
      usbHidDevice: usbHidDevice ?? throw new ArgumentNullException(nameof(usbHidDevice)),
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      cancellationToken: cancellationToken
    );

  private static IUsbHidService GetUsbHidServiceOrThrow(
    IServiceProvider serviceProvider,
    object? serviceKey
  )
  {
    try {
      return
        serviceProvider.GetKeyedService<IUsbHidService>(serviceKey) ??
        serviceProvider.GetRequiredService<IUsbHidService>();
    }
    catch (InvalidOperationException ex) {
      throw new InvalidOperationException(
        message: $"{nameof(IUsbHidService)} could not be resolved. To use this method overload, {nameof(IUsbHidService)} must be registered in {nameof(IServiceProvider)}.",
        innerException: ex
      );
    }
  }

  private static IUsbHidDevice GetFirstUsbHidDeviceOrThrow(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    CancellationToken cancellationToken
  )
  {
    var usbHidService = GetUsbHidServiceOrThrow(serviceProvider, serviceKey);

    // Considering the possibility that custom VID/PIDs may be configured,
    // filtering by VID/PID is not performed here when Predicate<IUsbHidDevice>
    // is provided; instead, it is delegated to Predicate<IUsbHidDevice>.
    var usbHidDevice = usbHidService.FindDevice(
      vendorId: usbHidDeviceFilter is null ? Mcp2221AController.DefaultVendorId : null,
      productId: usbHidDeviceFilter is null ? Mcp2221AController.DefaultProductId : null,
      predicate: usbHidDeviceFilter,
      cancellationToken: cancellationToken
    );

    return usbHidDevice ?? throw new Mcp2221ANotFoundException(usbHidService, usbHidDeviceFilter);
  }

  private static ValueTask<Mcp2221AController> CreateFromFirstUsbHidDeviceAsyncCore<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    CancellationToken cancellationToken
  )
    => CreateFromUsbHidDeviceAsyncCore(
      usbHidDevice: GetFirstUsbHidDeviceOrThrow(
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        usbHidDeviceFilter: usbHidDeviceFilter,
        cancellationToken: cancellationToken
      ),
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      shouldDisposeUsbHidDevice: true,
      cancellationToken: cancellationToken
    );

  private static Mcp2221AController CreateFromFirstUsbHidDeviceCore<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    CancellationToken cancellationToken
  )
    => CreateFromUsbHidDeviceCore(
      usbHidDevice: GetFirstUsbHidDeviceOrThrow(
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        usbHidDeviceFilter: usbHidDeviceFilter,
        cancellationToken: cancellationToken
      ),
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      shouldDisposeUsbHidDevice: true,
      cancellationToken: cancellationToken
    );

  private static async ValueTask<Mcp2221AController> CreateFromUsbHidDeviceAsyncCore<TServiceKey>(
    IUsbHidDevice usbHidDevice,
    IServiceProvider? serviceProvider,
#pragma warning disable IDE0060
    TServiceKey? serviceKey, // for future extension
#pragma warning restore IDE0060
    bool shouldDisposeUsbHidDevice,
    CancellationToken cancellationToken
  )
  {
    IUsbHidEndPoint? usbHidEndPoint = null;

    try {
      try {
        // hereafter, the lifecycle of device will be delegated to its endpoint
        usbHidEndPoint = await usbHidDevice.OpenEndPointAsync(
          shouldDisposeDevice: shouldDisposeUsbHidDevice,
          cancellationToken: cancellationToken
        ).ConfigureAwait(false);
      }
      catch (Exception ex) when (ex is not OperationCanceledException) {
        throw new Mcp2221AUnavailableException(ex, usbHidDevice);
      }

      var logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<Mcp2221AController>();
#pragma warning disable CA2000
      var transceiver = new Mcp2221ATransceiver(
        endPoint: usbHidEndPoint,
        logger: logger
      );
#pragma warning restore CA2000
      var info = await Mcp2221AInfo.ReadFromAsync(
        transceiver: transceiver,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);

      return await CreateFromInfoAndTransceiverAsync(
        transceiver: transceiver,
        info: info,
        logger: logger,
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    catch {
      if (usbHidEndPoint is not null)
        await usbHidEndPoint.DisposeAsync().ConfigureAwait(false);
      if (shouldDisposeUsbHidDevice)
        await usbHidDevice.DisposeAsync().ConfigureAwait(false);

      throw;
    }
  }

  private static Mcp2221AController CreateFromUsbHidDeviceCore<TServiceKey>(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
#pragma warning disable IDE0060
    TServiceKey? serviceKey, // for future extension
#pragma warning restore IDE0060
    CancellationToken cancellationToken
  )
  {
    IUsbHidEndPoint? usbHidEndPoint = null;

    try {
      try {
        // hereafter, the lifecycle of device will be delegated to its endpoint
        usbHidEndPoint = usbHidDevice.OpenEndPoint(
          shouldDisposeDevice: shouldDisposeUsbHidDevice,
          cancellationToken: cancellationToken
        );
      }
      catch (Exception ex) when (ex is not OperationCanceledException) {
        throw new Mcp2221AUnavailableException(ex, usbHidDevice);
      }

      var logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<Mcp2221AController>();
#pragma warning disable CA2000
      var transceiver = new Mcp2221ATransceiver(
        endPoint: usbHidEndPoint,
        logger: logger
      );
#pragma warning restore CA2000
      var info = Mcp2221AInfo.ReadFrom(
        transceiver: transceiver,
        cancellationToken: cancellationToken
      );

      return CreateFromInfoAndTransceiver(
        transceiver: transceiver,
        info: info,
        logger: logger,
        cancellationToken: cancellationToken
      );
    }
    catch {
      usbHidEndPoint?.Dispose();

      if (shouldDisposeUsbHidDevice)
        usbHidDevice.Dispose();

      throw;
    }
  }
}
