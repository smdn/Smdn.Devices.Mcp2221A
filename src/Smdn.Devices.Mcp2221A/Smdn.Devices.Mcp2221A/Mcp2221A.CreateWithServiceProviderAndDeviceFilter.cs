// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#pragma warning disable CA1848

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040, CA1724
partial class Mcp2221A {
#pragma warning restore IDE0040, CA1724
  /// <summary>
  /// Finds and opens a <see cref="Mcp2221A"/> device that matches the specified conditions
  /// from among the USB HID devices available on the system, and creates an instance for
  /// it asynchronously.
  /// </summary>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="usbHidDeviceFilter">
  /// A <see cref="Predicate{T}"/> to filter the USB HID devices.
  /// If <see langword="null"/>, a filter using the default VID (0x04D8) and
  /// PID (0x00DD) is used to find a MCP2221/MCP2221A.
  /// </param>
  /// <param name="mcp2221AFilter">
  /// A <see cref="Predicate{T}"/> to filter the MCP2221/MCP2221A devices based on
  /// their chip information and configurations stored in their flash memory.
  /// If <see langword="null"/>, the method returns the first device
  /// found by <paramref name="usbHidDeviceFilter"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a <see cref="Mcp2221A"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No device matching the specified filters could be found.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// <para>
  /// This method communicates with each candidate device to retrieve chip
  /// information prior to creating an instance.
  /// </para>
  /// <para>
  /// This method enumerates all devices matching <paramref name="usbHidDeviceFilter"/>,
  /// acquires their chip information, and returns the first device that matches the
  /// <paramref name="mcp2221AFilter"/> if the filter is provided.
  /// </para>
  /// <para>
  /// This method is resilient to certain errors during device discovery. If an exception
  /// occurs while opening an endpoint or acquiring chip information from a candidate device,
  /// the method will log the error (if a logger is available) and continue to the
  /// next candidate device.
  /// </para>
  /// </remarks>
  /// <seealso cref="IUsbHidDevice"/>
  /// <seealso cref="IMcp2221AInfo"/>
  public static ValueTask<Mcp2221A> CreateAsync(
    IServiceProvider serviceProvider,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
    => CreateAsync(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: (object?)null,
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221A"/> device that matches the specified conditions
  /// from among the USB HID devices available on the system, and creates an instance for it.
  /// </summary>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="usbHidDeviceFilter">
  /// A <see cref="Predicate{T}"/> to filter the USB HID devices.
  /// If <see langword="null"/>, a filter using the default VID (0x04D8) and
  /// PID (0x00DD) is used to find a MCP2221/MCP2221A.
  /// </param>
  /// <param name="mcp2221AFilter">
  /// A <see cref="Predicate{T}"/> to filter the MCP2221/MCP2221A devices based on
  /// their chip information and configurations stored in their flash memory.
  /// If <see langword="null"/>, the method returns the first device
  /// found by <paramref name="usbHidDeviceFilter"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221A"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No device matching the specified filters could be found.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// <para>
  /// This method communicates with each candidate device to retrieve chip
  /// information prior to creating an instance.
  /// </para>
  /// <para>
  /// This method enumerates all devices matching <paramref name="usbHidDeviceFilter"/>,
  /// acquires their chip information, and returns the first device that matches the
  /// <paramref name="mcp2221AFilter"/> if the filter is provided.
  /// </para>
  /// <para>
  /// This method is resilient to certain errors during device discovery. If an exception
  /// occurs while opening an endpoint or acquiring chip information from a candidate device,
  /// the method will log the error (if a logger is available) and continue to the
  /// next candidate device.
  /// </para>
  /// </remarks>
  /// <seealso cref="IUsbHidDevice"/>
  /// <seealso cref="IMcp2221AInfo"/>
  public static Mcp2221A Create(
    IServiceProvider serviceProvider,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
    => Create(
      serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
      serviceKey: (object?)null,
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      cancellationToken: cancellationToken
    );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221A"/> device that matches the specified conditions
  /// from among the USB HID devices available on the system, and creates an instance for
  /// it asynchronously.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the <see cref="IUsbHidService"/> to be obtained from the <paramref name="serviceProvider"/>.
  /// If a keyed service is not found, it attempts to resolve a non-keyed <see cref="IUsbHidService"/>.
  /// </param>
  /// <param name="usbHidDeviceFilter">
  /// A <see cref="Predicate{T}"/> to filter the USB HID devices.
  /// If <see langword="null"/>, a filter using the default VID (0x04D8) and
  /// PID (0x00DD) is used to find a MCP2221/MCP2221A.
  /// </param>
  /// <param name="mcp2221AFilter">
  /// A <see cref="Predicate{T}"/> to filter the MCP2221/MCP2221A devices based on
  /// their chip information and configurations stored in their flash memory.
  /// If <see langword="null"/>, the method returns the first device
  /// found by <paramref name="usbHidDeviceFilter"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="ValueTask{TResult}"/> that represents the asynchronous operation.
  /// The result of the task is a <see cref="Mcp2221A"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No device matching the specified filters could be found.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// <para>
  /// This method communicates with each candidate device to retrieve chip
  /// information prior to creating an instance.
  /// </para>
  /// <para>
  /// This method enumerates all devices matching <paramref name="usbHidDeviceFilter"/>,
  /// acquires their chip information, and returns the first device that matches the
  /// <paramref name="mcp2221AFilter"/> if the filter is provided.
  /// </para>
  /// <para>
  /// This method is resilient to certain errors during device discovery. If an exception
  /// occurs while opening an endpoint or acquiring chip information from a candidate device,
  /// the method will log the error (if a logger is available) and continue to the
  /// next candidate device.
  /// </para>
  /// </remarks>
  /// <seealso cref="IUsbHidDevice"/>
  /// <seealso cref="IMcp2221AInfo"/>
  public static ValueTask<Mcp2221A> CreateAsync<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
    => mcp2221AFilter is null
      ? CreateFromFirstUsbHidDeviceAsyncCore(
          serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
          serviceKey: serviceKey,
          usbHidDeviceFilter: usbHidDeviceFilter,
          cancellationToken: cancellationToken
        )
      : CreateWithDeviceFilterAsyncCore(
          serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
          serviceKey: serviceKey,
          usbHidDeviceFilter: usbHidDeviceFilter,
          mcp2221AFilter: mcp2221AFilter,
          cancellationToken: cancellationToken
        );

  /// <summary>
  /// Finds and opens a <see cref="Mcp2221A"/> device that matches the specified conditions
  /// from among the USB HID devices available on the system, and creates an instance for it.
  /// </summary>
  /// <typeparam name="TServiceKey">
  /// The type of the <paramref name="serviceKey"/>.
  /// </typeparam>
  /// <param name="serviceProvider">
  /// The <see cref="IServiceProvider"/> that provides required <see cref="IUsbHidService"/>
  /// and other optional services like logging.
  /// </param>
  /// <param name="serviceKey">
  /// The key for the <see cref="IUsbHidService"/> to be obtained from the <paramref name="serviceProvider"/>.
  /// If a keyed service is not found, it attempts to resolve a non-keyed <see cref="IUsbHidService"/>.
  /// </param>
  /// <param name="usbHidDeviceFilter">
  /// A <see cref="Predicate{T}"/> to filter the USB HID devices.
  /// If <see langword="null"/>, a filter using the default VID (0x04D8) and
  /// PID (0x00DD) is used to find a MCP2221/MCP2221A.
  /// </param>
  /// <param name="mcp2221AFilter">
  /// A <see cref="Predicate{T}"/> to filter the MCP2221/MCP2221A devices based on
  /// their chip information and configurations stored in their flash memory.
  /// If <see langword="null"/>, the method returns the first device
  /// found by <paramref name="usbHidDeviceFilter"/>.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// </param>
  /// <returns>
  /// A <see cref="Mcp2221A"/> instance for the found device.
  /// </returns>
  /// <exception cref="ArgumentNullException">
  /// <paramref name="serviceProvider"/> is <see langword="null"/>.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// <see cref="IUsbHidService"/> is not registered in <paramref name="serviceProvider"/>.
  /// </exception>
  /// <exception cref="Mcp2221ANotFoundException">
  /// No device matching the specified filters could be found.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// The operation was cancelled.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method requires an <see cref="IUsbHidService"/> to be registered
  /// in the <paramref name="serviceProvider"/>.
  /// </para>
  /// <para>
  /// This method communicates with each candidate device to retrieve chip
  /// information prior to creating an instance.
  /// </para>
  /// <para>
  /// This method enumerates all devices matching <paramref name="usbHidDeviceFilter"/>,
  /// acquires their chip information, and returns the first device that matches the
  /// <paramref name="mcp2221AFilter"/> if the filter is provided.
  /// </para>
  /// <para>
  /// This method is resilient to certain errors during device discovery. If an exception
  /// occurs while opening an endpoint or acquiring chip information from a candidate device,
  /// the method will log the error (if a logger is available) and continue to the
  /// next candidate device.
  /// </para>
  /// </remarks>
  /// <seealso cref="IUsbHidDevice"/>
  /// <seealso cref="IMcp2221AInfo"/>
  public static Mcp2221A Create<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
    => mcp2221AFilter is null
      ? CreateFromFirstUsbHidDeviceCore(
          serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
          serviceKey: serviceKey,
          usbHidDeviceFilter: usbHidDeviceFilter,
          cancellationToken: cancellationToken
        )
      : CreateWithDeviceFilterCore(
          serviceProvider: serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider)),
          serviceKey: serviceKey,
          usbHidDeviceFilter: usbHidDeviceFilter,
          mcp2221AFilter: mcp2221AFilter,
          cancellationToken: cancellationToken
        );

  private static
  (IUsbHidService UsbHidService, IReadOnlyList<IUsbHidDevice> Mcp2221AUsbHidDevices)
  FindAllMcp2221AUsbHidDevices(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    CancellationToken cancellationToken
  )
  {
    cancellationToken.ThrowIfCancellationRequested();

    var usbHidService = GetUsbHidServiceOrThrow(
      serviceProvider: serviceProvider,
      serviceKey: serviceKey
    );

    // If no filter is specified here, filtering is performed using the
    // default VID/PID. If a filter is specified, apply that filter.
    // Considerations for cases where custom VID/PID is set are handled
    // through the use of filters.
    var mcp2221AUsbHidDevices = usbHidService.FindAllDevices(
      vendorId: usbHidDeviceFilter is null ? Mcp2221A.DefaultVendorId : null,
      productId: usbHidDeviceFilter is null ? Mcp2221A.DefaultProductId : null,
      predicate: usbHidDeviceFilter,
      cancellationToken: cancellationToken
    );

    return (usbHidService, mcp2221AUsbHidDevices);
  }

  private static async ValueTask<Mcp2221A> CreateWithDeviceFilterAsyncCore(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo> mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
  {
    var (usbHidService, mcp2221AUsbHidDevices) = FindAllMcp2221AUsbHidDevices(
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      usbHidDeviceFilter: usbHidDeviceFilter,
      cancellationToken: cancellationToken
    );

    IUsbHidDevice? selectedUsbHidDevice = null;

    try {
      cancellationToken.ThrowIfCancellationRequested();

      var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Mcp2221A>();

      foreach (var mcp2221AUsbHidDevice in mcp2221AUsbHidDevices) {
        cancellationToken.ThrowIfCancellationRequested();

        Mcp2221ATransceiver? transceiver = null;

        try {
          // hereafter, the lifecycle of device will be delegated to its endpoint
#pragma warning disable CA2000
          var usbHidEndPoint = await mcp2221AUsbHidDevice.OpenEndPointAsync(
            shouldDisposeDevice: true,
            cancellationToken: cancellationToken
          ).ConfigureAwait(false);

          transceiver = new Mcp2221ATransceiver(
            endPoint: usbHidEndPoint,
            logger: logger
          );
#pragma warning restore CA2000

          var info = await Mcp2221AInfo.ReadFromAsync(
            transceiver,
            cancellationToken
          ).ConfigureAwait(false);

          if (mcp2221AFilter(info)) {
            selectedUsbHidDevice = mcp2221AUsbHidDevice;

            return await CreateFromInfoAndTransceiverAsync(
              transceiver: transceiver,
              info: info,
              logger: logger,
              cancellationToken: cancellationToken
            ).ConfigureAwait(false);
          }

          if (transceiver is not null)
            await transceiver.DisposeAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException) {
          if (transceiver is not null)
            await transceiver.DisposeAsync().ConfigureAwait(false);

          throw;
        }
#pragma warning disable CA1031
        catch (Exception ex) {
          if (transceiver is not null)
            await transceiver.DisposeAsync().ConfigureAwait(false);

          // If chip information cannot be acquire, attempt the next USB HID device.
          logger?.LogWarning(ex, "Unable to access the USB HID device or acquire chip information. ({Device})", mcp2221AUsbHidDevice);

          continue;
        }
#pragma warning restore CA1031
      }

      throw new Mcp2221ANotFoundException(usbHidService, usbHidDeviceFilter, mcp2221AFilter);
    }
    finally {
      foreach (var mcp2221AUsbHidDevice in mcp2221AUsbHidDevices) {
        if (cancellationToken.IsCancellationRequested || mcp2221AUsbHidDevice != selectedUsbHidDevice)
          await mcp2221AUsbHidDevice.DisposeAsync().ConfigureAwait(false);
      }
    }
  }

#pragma warning disable IDE0060
  private static Mcp2221A CreateWithDeviceFilterCore<TServiceKey>(
    IServiceProvider serviceProvider,
    TServiceKey? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo> mcp2221AFilter,
    CancellationToken cancellationToken = default
  )
  {
    var (usbHidService, mcp2221AUsbHidDevices) = FindAllMcp2221AUsbHidDevices(
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      usbHidDeviceFilter: usbHidDeviceFilter,
      cancellationToken: cancellationToken
    );

    IUsbHidDevice? selectedUsbHidDevice = null;

    try {
      cancellationToken.ThrowIfCancellationRequested();

      var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger<Mcp2221A>();

      foreach (var mcp2221AUsbHidDevice in mcp2221AUsbHidDevices) {
        cancellationToken.ThrowIfCancellationRequested();

        Mcp2221ATransceiver? transceiver = null;

        try {
          // hereafter, the lifecycle of device will be delegated to its endpoint
#pragma warning disable CA2000
          var usbHidEndPoint = mcp2221AUsbHidDevice.OpenEndPoint(
            shouldDisposeDevice: true,
            cancellationToken: cancellationToken
          );

          transceiver = new Mcp2221ATransceiver(
            endPoint: usbHidEndPoint,
            logger: logger
          );
#pragma warning restore CA2000

          var info = Mcp2221AInfo.ReadFrom(
            transceiver,
            cancellationToken
          );

          if (mcp2221AFilter(info)) {
            selectedUsbHidDevice = mcp2221AUsbHidDevice;

            return CreateFromInfoAndTransceiver(
              transceiver: transceiver,
              info: info,
              logger: logger,
              cancellationToken: cancellationToken
            );
          }

          transceiver?.Dispose();
        }
        catch (OperationCanceledException) {
          transceiver?.Dispose();

          throw;
        }
#pragma warning disable CA1031
        catch (Exception ex) {
          transceiver?.Dispose();

          // If chip information cannot be acquire, attempt the next USB HID device.
          logger?.LogWarning(ex, "Unable to access the USB HID device or acquire chip information. ({Device})", mcp2221AUsbHidDevice);

          continue;
        }
#pragma warning restore CA1031
      }

      throw new Mcp2221ANotFoundException(usbHidService, usbHidDeviceFilter, mcp2221AFilter);
    }
    finally {
      foreach (var mcp2221AUsbHidDevice in mcp2221AUsbHidDevices) {
        if (cancellationToken.IsCancellationRequested || mcp2221AUsbHidDevice != selectedUsbHidDevice)
          mcp2221AUsbHidDevice.Dispose();
      }
    }
  }
#pragma warning restore IDE0060
}
