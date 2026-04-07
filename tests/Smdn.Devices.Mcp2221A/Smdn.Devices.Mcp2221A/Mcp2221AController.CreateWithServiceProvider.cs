// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public partial class Mcp2221AControllerTests {
  private delegate ValueTask<Mcp2221AController> CreateWithKeyedServiceProviderFunc(
    IServiceProvider serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  );

  private delegate ValueTask<Mcp2221AController> CreateWithServiceProviderFunc(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken
  );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAsync(
    IServiceProvider serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAsync(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      serviceProvider: serviceProvider,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProvider(
    IServiceProvider serviceProvider,
    object? serviceKey,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: cancellationToken
      )
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProvider(
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        serviceProvider: serviceProvider,
        cancellationToken: cancellationToken
      )
    );

  [Test]
  public void CreateAsync_WithServiceProvider_ArgumentNull_ServiceProvider()
    => CreateSyncOrAsync_WithServiceProvider_ArgumentNull_ServiceProvider(
      CreateWithServiceProviderAsync,
      CreateWithServiceProviderAsync
    );

  [Test]
  public void Create_WithServiceProvider_ArgumentNull_ServiceProvider()
    => CreateSyncOrAsync_WithServiceProvider_ArgumentNull_ServiceProvider(
      CreateWithServiceProvider,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_ArgumentNull_ServiceProvider(
    CreateWithKeyedServiceProviderFunc createFuncWithKey,
    CreateWithServiceProviderFunc createFunc
  )
  {
    Assert.That(
      async () => await createFuncWithKey(
        serviceProvider: null!,
        serviceKey: "ServiceKey",
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );

    Assert.That(
      () => createFuncWithKey(
        serviceProvider: null!,
        serviceKey: "ServiceKey",
        cancellationToken: default
      ),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );

    Assert.That(
      async () => await createFunc(
        serviceProvider: null!,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );

    Assert.That(
      () => createFunc(
        serviceProvider: null!,
        cancellationToken: default
      ),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );
  }

  [Test]
  public void CreateAsync_WithServiceProvider()
    => CreateSyncOrAsync_WithServiceProvider(
      CreateWithServiceProviderAsync
    );

  [Test]
  public void Create_WithServiceProvider()
    => CreateSyncOrAsync_WithServiceProvider(
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider(
    CreateWithServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(), // selected
      CreatePseudoDevice(), // not selected
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    Mcp2221AController? mcp2221A = null;

    Assert.That(
      async () => {
        mcp2221A = await createFunc(
          serviceProvider,
          default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );

    Assert.That(mcp2221A, Is.Not.Null);
    Assert.That(devices[0].IsDisposed, Is.False);
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");

    mcp2221A.Dispose();

    Assert.That(devices[0].IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [Test]
  public void CreateAsync_WithServiceProvider_NoUsbHidServiceRegistered()
    => CreateSyncOrAsync_WithServiceProvider_NoUsbHidServiceRegistered(
      CreateWithServiceProviderAsync
    );

  [Test]
  public void Create_WithServiceProvider_NoUsbHidServiceRegistered()
    => CreateSyncOrAsync_WithServiceProvider_NoUsbHidServiceRegistered(
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_NoUsbHidServiceRegistered(
    CreateWithServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();

    // no IUsbHidService registered
    // services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        default
      ).ConfigureAwait(false),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(IUsbHidService))
    );
  }

  [TestCase(IntegerServiceKeyForTestCase)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProvider_NoKeyedUsbHidServiceRegistered(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoKeyedUsbHidServiceRegistered(
      serviceKey,
      CreateWithServiceProviderAsync
    );

  [TestCase(IntegerServiceKeyForTestCase)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProvider_NoKeyedUsbHidServiceRegistered(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoKeyedUsbHidServiceRegistered(
      serviceKey,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_NoKeyedUsbHidServiceRegistered(
    object? serviceKey,
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        serviceKey: "UnregisteredServiceKey",
        default
      ).ConfigureAwait(false),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(IUsbHidService))
    );

    Assert.That(devices[0].IsDisposed, Is.False, "USB HID devices that were not listed must not be disposed.");
  }

  [Test]
  public void CreateAsync_WithServiceProvider_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound()
    => CreateSyncOrAsync_WithServiceProvider_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
      CreateWithServiceProviderAsync
    );

  [Test]
  public void Create_WithServiceProvider_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound()
    => CreateSyncOrAsync_WithServiceProvider_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
    ];

    var services = new ServiceCollection();

    // register IUsbHidService without key
    services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => {
        await using var device = await createFunc(
          serviceProvider,
          serviceKey: "UnregisteredServiceKey",
          default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProvider_NoUsbHidDeviceListed(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoUsbHidDeviceListed(
      serviceKey,
      CreateWithServiceProviderAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProvider_NoUsbHidDeviceListed(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoUsbHidDeviceListed(
      serviceKey,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_NoUsbHidDeviceListed(
    object? serviceKey,
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService([])); // provides no devices

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        serviceKey,
        default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotFoundException>()
    );
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProvider_NoMatchUsbHidDevice(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoMatchUsbHidDevice(
      serviceKey,
      CreateWithServiceProviderAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProvider_NoMatchUsbHidDevice(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_NoMatchUsbHidDevice(
      serviceKey,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_NoMatchUsbHidDevice(
    object? serviceKey,
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(vendorId: Mcp2221AController.DefaultVendorId, productId: 0xFFFF), // not match
      CreatePseudoDevice(vendorId: 0xFFFF, productId: Mcp2221AController.DefaultProductId), // not match
      CreatePseudoDevice(vendorId: 0xFFFF, productId: 0xFFFF), // not match
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        serviceKey,
        default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotFoundException>()
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[2].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProvider_ExceptionWhileConstructingMcp2221AController(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_ExceptionWhileConstructingMcp2221AController(
      serviceKey,
      CreateWithServiceProviderAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProvider_ExceptionWhileConstructingMcp2221AController(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_ExceptionWhileConstructingMcp2221AController(
      serviceKey,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_ExceptionWhileConstructingMcp2221AController(
    object? serviceKey,
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(
        hardwareRevisionMajor: (byte)'?', // unsupported hardware revision
        hardwareRevisionMinor: (byte)'?' // unsupported hardware revision
      ),
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotSupportedException>()
    );

    Assert.That(devices[0].IsDisposed, Is.True);
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProvider_CancellationRequestedWhileConstructingMcp2221AController(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_CancellationRequestedWhileConstructingMcp2221AController(
      serviceKey,
      CreateWithServiceProviderAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProvider_CancellationRequestedWhileConstructingMcp2221AController(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProvider_CancellationRequestedWhileConstructingMcp2221AController(
      serviceKey,
      CreateWithServiceProvider
    );

  private void CreateSyncOrAsync_WithServiceProvider_CancellationRequestedWhileConstructingMcp2221AController(
    object? serviceKey,
    CreateWithKeyedServiceProviderFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
    ];

    using var cts = new CancellationTokenSource();

    devices[0].OnEndPointOpenedAction = () => {
      devices[0].EndPoint.OnReadingAction = () => cts.Cancel();
    };

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
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

    Assert.That(devices[0].IsDisposed, Is.True);
  }
}
