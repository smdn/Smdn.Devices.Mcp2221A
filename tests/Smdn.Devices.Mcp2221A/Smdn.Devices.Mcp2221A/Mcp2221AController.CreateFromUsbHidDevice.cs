// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public partial class Mcp2221AControllerTests {
  private delegate ValueTask<Mcp2221AController> CreateFromUsbHidDeviceWithKeyedServiceProviderFunc(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  );

  private delegate ValueTask<Mcp2221AController> CreateFromUsbHidDeviceWithServiceProviderFunc(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    CancellationToken cancellationToken
  );

  private ValueTask<Mcp2221AController> CreateFromUsbHidDeviceAsync(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      usbHidDevice: usbHidDevice,
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateFromUsbHidDeviceAsync(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      usbHidDevice: usbHidDevice,
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateFromUsbHidDevice(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        usbHidDevice: usbHidDevice,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: cancellationToken
      )
    );

  private ValueTask<Mcp2221AController> CreateFromUsbHidDevice(
    IUsbHidDevice usbHidDevice,
    bool shouldDisposeUsbHidDevice,
    IServiceProvider? serviceProvider,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        usbHidDevice: usbHidDevice,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        cancellationToken: cancellationToken
      )
    );

  [Test]
  public void CreateAsync_FromUsbHidDevice_ArgumentNull_UsbHidDevice(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => CreateSyncOrAsync_FromUsbHidDevice_ArgumentNull_UsbHidDevice(
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync,
      CreateFromUsbHidDeviceAsync
    );

  [Test]
  public void Create_FromUsbHidDevice_ArgumentNull_UsbHidDevice(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => CreateSyncOrAsync_FromUsbHidDevice_ArgumentNull_UsbHidDevice(
      shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_ArgumentNull_UsbHidDevice(
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFuncWithKey,
    CreateFromUsbHidDeviceWithServiceProviderFunc createFunc
  )
  {
    Assert.That(
      async () => await createFuncWithKey(
        usbHidDevice: null!,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: null,
        serviceKey: "ServiceKey",
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("usbHidDevice")
    );

    Assert.That(
      () => createFuncWithKey(
        usbHidDevice: null!,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: null,
        serviceKey: "ServiceKey",
        cancellationToken: default
      ),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("usbHidDevice")
    );

    Assert.That(
      async () => await createFunc(
        usbHidDevice: null!,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: null,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("usbHidDevice")
    );

    Assert.That(
      () => createFunc(
        usbHidDevice: null!,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: null,
        cancellationToken: default
      ),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("usbHidDevice")
    );
  }

  [Test]
  public void CreateAsync_FromUsbHidDevice_ShouldDisposeUsbHidDevice(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => CreateSyncOrAsync_FromUsbHidDevice_ShouldDisposeUsbHidDevice(
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync
    );

  [Test]
  public void Create_FromUsbHidDevice_ShouldDisposeUsbHidDevice(
    [Values] bool shouldDisposeUsbHidDevice
  )
    => CreateSyncOrAsync_FromUsbHidDevice_ShouldDisposeUsbHidDevice(
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_ShouldDisposeUsbHidDevice(
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithServiceProviderFunc createFunc
  )
  {
    using var usbHidDevice = CreatePseudoDevice();
    Mcp2221AController? mcp2221A = null;

    Assert.That(
      async () => {
        mcp2221A = await createFunc(
          usbHidDevice: usbHidDevice,
          shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
          serviceProvider: null,
          cancellationToken: default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );

    Assert.That(mcp2221A, Is.Not.Null);
    Assert.That(usbHidDevice.IsDisposed, Is.False);

    mcp2221A.Dispose();

    Assert.That(usbHidDevice.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice));
  }

  [Test]
  public void CreateAsync_FromUsbHidDevice_NoServiceProviderProvided()
    => CreateSyncOrAsync_FromUsbHidDevice_NoServiceProviderProvided(
      CreateFromUsbHidDeviceAsync,
      CreateFromUsbHidDeviceAsync
    );

  [Test]
  public void Create_FromUsbHidDevice_NoServiceProviderProvided()
    => CreateSyncOrAsync_FromUsbHidDevice_NoServiceProviderProvided(
      CreateFromUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_NoServiceProviderProvided(
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFuncWithKey,
    CreateFromUsbHidDeviceWithServiceProviderFunc createFunc
  )
  {
    IServiceProvider? nullServiceProvider = null;

    Assert.That(
      async () => {
        using var usbHidDevice = CreatePseudoDevice();
        await using var mcp2221A = await createFuncWithKey(
          usbHidDevice: usbHidDevice,
          shouldDisposeUsbHidDevice: true,
          serviceProvider: nullServiceProvider,
          serviceKey: "ServiceKey",
          cancellationToken: default
        );
      },
      Throws.Nothing
    );

    Assert.That(
      async () => {
        using var usbHidDevice = CreatePseudoDevice();
        await using var mcp2221A = await createFunc(
          usbHidDevice: usbHidDevice,
          shouldDisposeUsbHidDevice: true,
          serviceProvider: nullServiceProvider,
          cancellationToken: default
        );
      },
      Throws.Nothing
    );
  }

  [Test]
  public void CreateAsync_FromUsbHidDevice_WithLogging()
    => CreateSyncOrAsync_FromUsbHidDevice_WithLogging(
      CreateFromUsbHidDeviceAsync,
      CreateFromUsbHidDeviceAsync
    );

  [Test]
  public void Create_FromUsbHidDevice_WithLogging()
    => CreateSyncOrAsync_FromUsbHidDevice_WithLogging(
      CreateFromUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_WithLogging(
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFuncWithKey,
    CreateFromUsbHidDeviceWithServiceProviderFunc createFunc
  )
  {
    var loggerProvider = new FakeLoggerProvider();
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => {
        using var usbHidDevice = CreatePseudoDevice();
        await using var mcp2221A = await createFuncWithKey(
          usbHidDevice: usbHidDevice,
          shouldDisposeUsbHidDevice: true,
          serviceProvider: serviceProvider,
          serviceKey: "ServiceKey",
          cancellationToken: default
        );
      },
      Throws.Nothing
    );

    Assert.That(loggerProvider.Collector.Count, Is.Not.Zero);

    loggerProvider.Collector.Clear();

    Assert.That(
      async () => {
        using var usbHidDevice = CreatePseudoDevice();
        await using var mcp2221A = await createFunc(
          usbHidDevice: usbHidDevice,
          shouldDisposeUsbHidDevice: true,
          serviceProvider: serviceProvider,
          cancellationToken: default
        );
      },
      Throws.Nothing
    );

    Assert.That(loggerProvider.Collector.Count, Is.Not.Zero);
  }

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void CreateAsync_FromUsbHidDevice_ExceptionWhileOpeningEndPoint(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileOpeningEndPoint(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync
    );

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void Create_FromUsbHidDevice_ExceptionWhileOpeningEndPoint(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileOpeningEndPoint(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileOpeningEndPoint(
    object? serviceKey,
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();
    using var serviceProvider = services.BuildServiceProvider();
    using var device = CreatePseudoDevice();

    device.OnEndPointOpeningAction = static () => throw new NotSupportedException();

    Assert.That(
      async () => await createFunc(
        usbHidDevice: device,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .TypeOf<Mcp2221AUnavailableException>()
        .With
        .Property(nameof(Mcp2221AUnavailableException.InnerException))
        .TypeOf<NotSupportedException>()
    );

    Assert.That(device.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice));
  }

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void CreateAsync_FromUsbHidDevice_CancellationRequestedWhileOpeningEndPoint(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileOpeningEndPoint(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync
    );

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void Create_FromUsbHidDevice_CancellationRequestedWhileOpeningEndPoint(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileOpeningEndPoint(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileOpeningEndPoint(
    object? serviceKey,
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();
    using var serviceProvider = services.BuildServiceProvider();
    using var device = CreatePseudoDevice();
    using var cts = new CancellationTokenSource();

    device.OnEndPointOpeningAction = () => cts.Cancel();

    Assert.That(
      async () => await createFunc(
        usbHidDevice: device,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: cts.Token
      ).ConfigureAwait(false),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(device.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice));
  }

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void CreateAsync_FromUsbHidDevice_ExceptionWhileConstructingMcp2221AController(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileConstructingMcp2221AController(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync
    );

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void Create_FromUsbHidDevice_ExceptionWhileConstructingMcp2221AController(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileConstructingMcp2221AController(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_ExceptionWhileConstructingMcp2221AController(
    object? serviceKey,
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();
    using var serviceProvider = services.BuildServiceProvider();
    using var device = CreatePseudoDevice(
      hardwareRevisionMajor: (byte)'?', // unsupported hardware revision
      hardwareRevisionMinor: (byte)'?' // unsupported hardware revision
    );

    Assert.That(
      async () => await createFunc(
        usbHidDevice: device,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotSupportedException>()
    );

    Assert.That(device.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice));
  }

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void CreateAsync_FromUsbHidDevice_CancellationRequestedWhileConstructingMcp2221AController(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileConstructingMcp2221AController(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDeviceAsync
    );

  [TestCase(null, true)]
  [TestCase(null, false)]
  [TestCase(StringServiceKeyForTestCase, true)]
  public void Create_FromUsbHidDevice_CancellationRequestedWhileConstructingMcp2221AController(object? serviceKey, bool shouldDisposeUsbHidDevice)
    => CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileConstructingMcp2221AController(
      serviceKey,
      shouldDisposeUsbHidDevice,
      CreateFromUsbHidDevice
    );

  private void CreateSyncOrAsync_FromUsbHidDevice_CancellationRequestedWhileConstructingMcp2221AController(
    object? serviceKey,
    bool shouldDisposeUsbHidDevice,
    CreateFromUsbHidDeviceWithKeyedServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();
    using var serviceProvider = services.BuildServiceProvider();
    using var device = CreatePseudoDevice();
    using var cts = new CancellationTokenSource();

    device.OnEndPointOpenedAction = () => {
      device.EndPoint.OnReadingAction = () => cts.Cancel();
    };

    Assert.That(
      async () => await createFunc(
        usbHidDevice: device,
        shouldDisposeUsbHidDevice: shouldDisposeUsbHidDevice,
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: cts.Token
      ).ConfigureAwait(false),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(device.IsDisposed, Is.EqualTo(shouldDisposeUsbHidDevice));
  }
}
