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
  private const int IntegerServiceKeyForTestCase = int.MaxValue;
  private const string StringServiceKeyForTestCase = nameof(StringServiceKeyForTestCase);

  private delegate ValueTask<Mcp2221AController> CreateWithKeyedServiceProviderAndDeviceFilterFunc(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  );

  private delegate ValueTask<Mcp2221AController> CreateWithServiceProviderAndDeviceFilterFunc(
    IServiceProvider serviceProvider,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAndDeviceFilterAsync(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      serviceProvider: serviceProvider,
      serviceKey: serviceKey,
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAndDeviceFilterAsync(
    IServiceProvider serviceProvider,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  )
    => Mcp2221AController.CreateAsync(
      serviceProvider: serviceProvider,
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      cancellationToken: cancellationToken
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAndDeviceFilter(
    IServiceProvider serviceProvider,
    object? serviceKey,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        serviceProvider: serviceProvider,
        serviceKey: serviceKey,
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
        cancellationToken: cancellationToken
      )
    );

  private ValueTask<Mcp2221AController> CreateWithServiceProviderAndDeviceFilter(
    IServiceProvider serviceProvider,
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CancellationToken cancellationToken
  )
    => new(
      Mcp2221AController.Create(
        serviceProvider: serviceProvider,
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
        cancellationToken: cancellationToken
      )
    );

  private static System.Collections.IEnumerable YieldTestCases_CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider()
  {
    Predicate<IUsbHidDevice>? nonNullUsbHidDeviceFilter = static _ => true;
    Predicate<IMcp2221AInfo>? nonNullMcp2221AFilter = static _ => true;

    yield return new object?[] { null, null };
    yield return new object?[] { null, nonNullMcp2221AFilter };
    yield return new object?[] { nonNullUsbHidDeviceFilter, null };
    yield return new object?[] { nonNullUsbHidDeviceFilter, nonNullMcp2221AFilter };
  }

  [TestCaseSource(nameof(YieldTestCases_CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider))]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider(
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider(
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      CreateWithServiceProviderAndDeviceFilterAsync,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCaseSource(nameof(YieldTestCases_CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider))]
  public void Create_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider(
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider(
      usbHidDeviceFilter: usbHidDeviceFilter,
      mcp2221AFilter: mcp2221AFilter,
      CreateWithServiceProviderAndDeviceFilter,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ArgumentNull_ServiceProvider(
    Predicate<IUsbHidDevice>? usbHidDeviceFilter,
    Predicate<IMcp2221AInfo>? mcp2221AFilter,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFuncWithKey,
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    Assert.That(
      () => createFuncWithKey(
        serviceProvider: null!,
        serviceKey: "ServiceKey",
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
        cancellationToken: default
      ),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );

    Assert.That(
      async () => await createFuncWithKey(
        serviceProvider: null!,
        serviceKey: "ServiceKey",
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
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
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
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
        usbHidDeviceFilter: usbHidDeviceFilter,
        mcp2221AFilter: mcp2221AFilter,
        cancellationToken: default
      ).ConfigureAwait(false),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("serviceProvider")
    );
  }

  [Test]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_WithLogging()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_WithLogging(
      CreateWithServiceProviderAndDeviceFilterAsync,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_WithLogging()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_WithLogging(
      CreateWithServiceProviderAndDeviceFilter,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_WithLogging(
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFuncWithKey,
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    var services = new ServiceCollection();

    services.AddPseudoUsbHid(StringServiceKeyForTestCase, new PseudoUsbHidService([CreatePseudoDevice()]));
    services.AddPseudoUsbHid(new PseudoUsbHidService([CreatePseudoDevice()]));

    var loggerProvider = new FakeLoggerProvider();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => {
        await using var mcp2221A = await createFuncWithKey(
          serviceProvider: serviceProvider,
          serviceKey: StringServiceKeyForTestCase,
          usbHidDeviceFilter: static _ => true, // select all
          mcp2221AFilter: static _ => true, // select all
          cancellationToken: default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );

    Assert.That(loggerProvider.Collector.Count, Is.Not.Zero);

    loggerProvider.Collector.Clear();

    Assert.That(
      async () => {
        await using var mcp2221A = await createFunc(
          serviceProvider: serviceProvider,
          usbHidDeviceFilter: static _ => true, // select all
          mcp2221AFilter: static _ => true, // select all
          cancellationToken: default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );

    Assert.That(loggerProvider.Collector.Count, Is.Not.Zero);
  }

  [Test]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_NoUsbHidServiceRegistered()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoUsbHidServiceRegistered(
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_NoUsbHidServiceRegistered()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoUsbHidServiceRegistered(
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoUsbHidServiceRegistered(
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    var services = new ServiceCollection();

    // no IUsbHidService registered
    // services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        usbHidDeviceFilter: static _ => true,
        mcp2221AFilter: static _ => true,
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
  public void CreateAsync_WithServiceProviderAndDeviceFilter_NoKeyedUsbHidServiceRegistered(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoKeyedUsbHidServiceRegistered(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(IntegerServiceKeyForTestCase)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProviderAndDeviceFilter_NoKeyedUsbHidServiceRegistered(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoKeyedUsbHidServiceRegistered(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NoKeyedUsbHidServiceRegistered(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
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
        usbHidDeviceFilter: static _ => true,
        mcp2221AFilter: static _ => true,
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
  public void CreateAsync_WithServiceProviderAndDeviceFilter_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound()
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_NonKeyedUsbHidServiceSelectedIfNoKeyedServiceFound(
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
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
          usbHidDeviceFilter: static _ => true,
          mcp2221AFilter: static _ => true,
          default
        ).ConfigureAwait(false);
      },
      Throws.Nothing
    );
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask CreateAsync_WithServiceProviderAndDeviceFilter_OnlyUsbHidDeviceFilterProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_OnlyUsbHidDeviceFilterProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask Create_WithServiceProviderAndDeviceFilter_OnlyUsbHidDeviceFilterProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_OnlyUsbHidDeviceFilterProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private async ValueTask CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_OnlyUsbHidDeviceFilterProvided(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    const int VendorIdToBeSelected = 0xFFF1;
    const int VendorIdNotToBeSelected = 0xFFF2;

    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(vendorId: VendorIdNotToBeSelected), // not selected
      CreatePseudoDevice(vendorId: VendorIdToBeSelected), // selected
      CreatePseudoDevice(vendorId: VendorIdToBeSelected), // not tested
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    await using var mcp2221A = await createFunc(
      serviceProvider,
      serviceKey,
      usbHidDeviceFilter: device => device.VendorId == VendorIdToBeSelected,
      mcp2221AFilter: null, // not provided
      default
    ).ConfigureAwait(false);

    Assert.That(mcp2221A.HidDevice.VendorId, Is.EqualTo(VendorIdToBeSelected));

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.False);
    Assert.That(devices[2].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");

    mcp2221A.Dispose();

    Assert.That(devices[1].IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask CreateAsync_WithServiceProviderAndDeviceFilter_BothUsbHidDeviceFilterAndMcp2221AFilterNotProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_BothUsbHidDeviceFilterAndMcp2221AFilterNotProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask Create_WithServiceProviderAndDeviceFilter_BothUsbHidDeviceFilterAndMcp2221AFilterNotProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_BothUsbHidDeviceFilterAndMcp2221AFilterNotProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private async ValueTask CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_BothUsbHidDeviceFilterAndMcp2221AFilterNotProvided(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    const int UnrelatedDeviceVendorId = 0xCAFE;

    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(vendorId: UnrelatedDeviceVendorId), // not selected
      CreatePseudoDevice(vendorId: Mcp2221AController.DefaultVendorId, productId: 0xFFFF), // not selected
      CreatePseudoDevice(vendorId: Mcp2221AController.DefaultVendorId, productId: Mcp2221AController.DefaultProductId), // selected
      CreatePseudoDevice(vendorId: Mcp2221AController.DefaultVendorId, productId: Mcp2221AController.DefaultProductId), // not tested
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    await using var mcp2221A = await createFunc(
      serviceProvider,
      serviceKey,
      usbHidDeviceFilter: null, // not provided
      mcp2221AFilter: null, // not provided
      default
    ).ConfigureAwait(false);

    Assert.That(mcp2221A.HidDevice.VendorId, Is.EqualTo(Mcp2221AController.DefaultVendorId));
    Assert.That(mcp2221A.HidDevice.ProductId, Is.EqualTo(Mcp2221AController.DefaultProductId));

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[2].IsDisposed, Is.False);
    Assert.That(devices[3].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");

    mcp2221A.Dispose();

    Assert.That(devices[2].IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterNotProvided_NoMatchUsbHidDevice(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterNotProvided_NoMatchUsbHidDevice(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProviderAndDeviceFilter_Mcp2221AFilterNotProvided_NoMatchUsbHidDevice(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterNotProvided_NoMatchUsbHidDevice(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterNotProvided_NoMatchUsbHidDevice(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(), // not selected
      CreatePseudoDevice(), // not selected
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
        usbHidDeviceFilter: static _ => false, // all USB HID devices do not match
        mcp2221AFilter: null, // not provided
        default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotFoundException>()
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask CreateAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask Create_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private async ValueTask CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    const int VendorIdToBeSelected = 0xFFF1;
    const int VendorIdNotToBeSelected = 0xFFF2;
    const string SerialNumberToBeSelected = nameof(SerialNumberToBeSelected);
    const string SerialNumberNotToBeSelected = nameof(SerialNumberNotToBeSelected);

    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(vendorId: VendorIdNotToBeSelected, serialNumber: SerialNumberNotToBeSelected), // not selected by usbHidDeviceFilter
      CreatePseudoDevice(vendorId: VendorIdNotToBeSelected, serialNumber: SerialNumberToBeSelected), // not selected by usbHidDeviceFilter
      CreatePseudoDevice(vendorId: VendorIdToBeSelected, serialNumber: SerialNumberNotToBeSelected), // selected by usbHidDeviceFilter, but not by mcp2221AFilter
      CreatePseudoDevice(vendorId: VendorIdToBeSelected, serialNumber: SerialNumberToBeSelected), // selected
      CreatePseudoDevice(vendorId: VendorIdToBeSelected, serialNumber: SerialNumberToBeSelected), // not tested
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    await using var mcp2221A = await createFunc(
      serviceProvider,
      serviceKey,
      usbHidDeviceFilter: device => device.VendorId == VendorIdToBeSelected,
      mcp2221AFilter: info => info.SerialNumber == SerialNumberToBeSelected,
      default
    ).ConfigureAwait(false);

    Assert.That(mcp2221A.HidDevice.VendorId, Is.EqualTo(VendorIdToBeSelected));
    Assert.That(mcp2221A.SerialNumber, Is.EqualTo(SerialNumberToBeSelected));

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[2].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[3].IsDisposed, Is.False);
    Assert.That(devices[4].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");

    mcp2221A.Dispose();

    Assert.That(devices[3].IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask CreateAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_UsbHidDeviceFilterNotProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_UsbHidDeviceFilterNotProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public ValueTask Create_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_UsbHidDeviceFilterNotProvided(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_UsbHidDeviceFilterNotProvided(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private async ValueTask CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_UsbHidDeviceFilterNotProvided(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    const int UnrelatedDeviceVendorId = 0xCAFE;
    const string SerialNumberToBeSelected = nameof(SerialNumberToBeSelected);
    const string SerialNumberNotToBeSelected = nameof(SerialNumberNotToBeSelected);

    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(vendorId: UnrelatedDeviceVendorId, serialNumber: SerialNumberToBeSelected), // not selected
      CreatePseudoDevice(serialNumber: SerialNumberNotToBeSelected), // not selected
      CreatePseudoDevice(serialNumber: SerialNumberToBeSelected), // selected
      CreatePseudoDevice(serialNumber: SerialNumberToBeSelected), // not selected
    ];

    var services = new ServiceCollection();

    // IUsbHidService registered with other key must not be used
    services.AddKeyedSingleton<IUsbHidService>(nameof(ThrowingUsbHidService), new ThrowingUsbHidService());

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    await using var mcp2221A = await createFunc(
      serviceProvider,
      serviceKey,
      usbHidDeviceFilter: null, // not provided
      mcp2221AFilter: info => info.SerialNumber == SerialNumberToBeSelected,
      default
    ).ConfigureAwait(false);

    Assert.That(mcp2221A.SerialNumber, Is.EqualTo(SerialNumberToBeSelected));

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[2].IsDisposed, Is.False);
    Assert.That(devices[3].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");

    mcp2221A.Dispose();

    Assert.That(devices[2].IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_NoMatchMcp2221A(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_NoMatchMcp2221A(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_NoMatchMcp2221A(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_NoMatchMcp2221A(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_Mcp2221AFilterProvided_NoMatchMcp2221A(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(), // not selected
      CreatePseudoDevice(), // not selected
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
        usbHidDeviceFilter: static _ => true, // all USB HID devices match
        mcp2221AFilter: static _ => false, // all MCP2221As do not match
        default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotFoundException>()
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but not selected must be disposed.");
  }

  [Test]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterProvided(
    [Values] bool selectAnyUsbHidDevice,
    [Values] bool selectAnyMcp2221A
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterProvided(
      selectAnyUsbHidDevice: selectAnyUsbHidDevice,
      selectAnyMcp2221A: selectAnyMcp2221A,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterProvided(
    [Values] bool selectAnyUsbHidDevice,
    [Values] bool selectAnyMcp2221A
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterProvided(
      selectAnyUsbHidDevice: selectAnyUsbHidDevice,
      selectAnyMcp2221A: selectAnyMcp2221A,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterProvided(
    bool selectAnyUsbHidDevice,
    bool selectAnyMcp2221A,
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
      CreatePseudoDevice(),
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    using var cts = new CancellationTokenSource();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        usbHidDeviceFilter: _ => {
          cts.Cancel(); // request cancellation
          return selectAnyUsbHidDevice;
        },
        mcp2221AFilter: _ => selectAnyMcp2221A,
        cancellationToken: cts.Token
      ).ConfigureAwait(false),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
  }

  [Test]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterNotProvided(
    [Values] bool selectAnyUsbHidDevice
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterNotProvided(
      selectAnyUsbHidDevice,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterNotProvided(
    [Values] bool selectAnyUsbHidDevice
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterNotProvided(
      selectAnyUsbHidDevice,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingUsbHidDeviceFilter_Mcp2221AFilterNotProvided(
    bool selectAnyUsbHidDevice,
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
      CreatePseudoDevice(),
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    using var cts = new CancellationTokenSource();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        usbHidDeviceFilter: _ => {
          cts.Cancel(); // request cancellation
          return selectAnyUsbHidDevice;
        },
        mcp2221AFilter: null, // not provided
        cancellationToken: cts.Token
      ).ConfigureAwait(false),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
  }

  [Test]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingMcp2221AFilter(
    [Values] bool selectAnyMcp2221A
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingMcp2221AFilter(
      selectAnyMcp2221A: selectAnyMcp2221A,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [Test]
  public void Create_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingMcp2221AFilter(
    [Values] bool selectAnyMcp2221A
  )
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingMcp2221AFilter(
      selectAnyMcp2221A: selectAnyMcp2221A,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_CancellationRequested_WhileEvaluatingMcp2221AFilter(
    bool selectAnyMcp2221A,
    CreateWithServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
      CreatePseudoDevice(),
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();
    using var cts = new CancellationTokenSource();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        usbHidDeviceFilter: static _ => true, // all USB HID devices match (to evaluate with mcp2221AFilter)
        mcp2221AFilter: _ => {
          cts.Cancel(); // request cancellation
          return selectAnyMcp2221A;
        },
        cancellationToken: cts.Token
      ).ConfigureAwait(false),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(devices[0].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "USB HID devices that were listed but cancellation requested after that must be disposed.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_ContinuesOnExceptionWhileAcquireChipInformation(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ContinuesOnExceptionWhileAcquireChipInformation(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProviderAndDeviceFilter_ContinuesOnExceptionWhileAcquireChipInformation(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ContinuesOnExceptionWhileAcquireChipInformation(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ContinuesOnExceptionWhileAcquireChipInformation(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    const string DeviceSerialNumberToFail = nameof(DeviceSerialNumberToFail);
    const string DeviceSerialNumberToSucceed = nameof(DeviceSerialNumberToSucceed);

    var deviceToFail = CreatePseudoDevice(serialNumber: DeviceSerialNumberToFail);

    deviceToFail.OnEndPointOpeningAction = static () => throw new NotSupportedException();

    var deviceToSucceed = CreatePseudoDevice(serialNumber: DeviceSerialNumberToSucceed);

    PseudoUsbHidDevice[] devices = [
      deviceToFail,
      deviceToSucceed,
    ];

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Mcp2221AController? mcp2221A = null;

    Assert.That(
      async () => mcp2221A = await createFunc(
        serviceProvider,
        serviceKey,
        usbHidDeviceFilter: static _ => true,
        mcp2221AFilter: static _ => true,
        default
      ).ConfigureAwait(false),
      Throws.Nothing
    );

    Assert.That(mcp2221A, Is.Not.Null);
    Assert.That(mcp2221A.SerialNumber, Is.EqualTo(DeviceSerialNumberToSucceed));
    Assert.That(deviceToSucceed.IsDisposed, Is.False);

    Assert.That(deviceToFail.IsDisposed, Is.True, "The device that failed while acquiring the chip information must be disposed.");

    mcp2221A.Dispose();

    Assert.That(deviceToSucceed.IsDisposed, Is.True, "The underlying device must be disposed of along with the owning instance.");
  }

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void CreateAsync_WithServiceProviderAndDeviceFilter_ThrowsWhenAllDevicesFailWhileAcquireChipInformation(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ThrowsWhenAllDevicesFailWhileAcquireChipInformation(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilterAsync
    );

  [TestCase(null)]
  [TestCase(StringServiceKeyForTestCase)]
  public void Create_WithServiceProviderAndDeviceFilter_ThrowsWhenAllDevicesFailWhileAcquireChipInformation(object? serviceKey)
    => CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ThrowsWhenAllDevicesFailWhileAcquireChipInformation(
      serviceKey,
      CreateWithServiceProviderAndDeviceFilter
    );

  private void CreateSyncOrAsync_WithServiceProviderAndDeviceFilter_ThrowsWhenAllDevicesFailWhileAcquireChipInformation(
    object? serviceKey,
    CreateWithKeyedServiceProviderAndDeviceFilterFunc createFunc
  )
  {
    PseudoUsbHidDevice[] devices = [
      CreatePseudoDevice(),
      CreatePseudoDevice()
    ];

    devices[0].OnEndPointOpeningAction = static () => throw new NotSupportedException();
    devices[1].OnEndPointOpeningAction = static () => throw new NotSupportedException();

    var services = new ServiceCollection();

    services.AddPseudoUsbHid(serviceKey, new PseudoUsbHidService(devices));

    using var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      async () => await createFunc(
        serviceProvider,
        serviceKey,
        usbHidDeviceFilter: static _ => true,
        mcp2221AFilter: static _ => true,
        default
      ).ConfigureAwait(false),
      Throws.TypeOf<Mcp2221ANotFoundException>()
    );

    Assert.That(devices[0].IsDisposed, Is.True, "The device that failed while acquiring the chip information must be disposed.");
    Assert.That(devices[1].IsDisposed, Is.True, "The device that failed while acquiring the chip information must be disposed.");
  }
}
