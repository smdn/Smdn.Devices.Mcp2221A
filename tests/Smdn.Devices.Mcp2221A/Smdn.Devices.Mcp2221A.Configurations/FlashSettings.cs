// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A.Configurations;

[TestFixture]
public partial class FlashSettingsTests {
  private sealed class AllocatedFlashMemoryFactory(
    IFlashMemory initial,
    IFlashMemory staging
  ) : IFlashMemoryFactory {
    private bool hasInitialCreated;

    public IFlashMemory Create()
    {
      if (hasInitialCreated)
        return staging;

      hasInitialCreated = true;

      return initial;
    }
  }

  private Mcp2221AController CreateWithAllocatedFlashMemory(
    IUsbHidDevice device,
    IFlashMemory initial,
    IFlashMemory staging
  )
  {
    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(initial, staging)
    );

    return Mcp2221AController.Create(
      device,
      serviceProvider: services.BuildServiceProvider(),
      shouldDisposeUsbHidDevice: true
    );
  }

  private ValueTask<Mcp2221AController> CreateWithAllocatedFlashMemoryAsync(
    IUsbHidDevice device,
    IFlashMemory initial,
    IFlashMemory staging
  )
  {
    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(initial, staging)
    );

    return Mcp2221AController.CreateAsync(
      device,
      serviceProvider: services.BuildServiceProvider(),
      shouldDisposeUsbHidDevice: true
    );
  }

  [Test]
  public void CreateFlashMemory_Mcp2221AController_CreateFromUsbHidDevice()
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(
      () => _ = mcp2221A.Flash,
      Throws.Nothing
    );
    Assert.That(
      () => mcp2221A.Flash.ModifyPassword("password"u8),
      Throws.Nothing
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual("password"u8),
      Is.True,
      "Must be written to the `IFlashMemory` created by `IFlashMemoryFactory`"
    );
  }

  [Test]
  public async ValueTask CreateFlashMemory_Mcp2221AController_CreateFromUsbHidDeviceAsync()
  {
    var stagedFlashMemory = new FlashMemory();

    await using var mcp2221A = await CreateWithAllocatedFlashMemoryAsync(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(
      () => _ = mcp2221A.Flash,
      Throws.Nothing
    );
    Assert.That(
      () => mcp2221A.Flash.ModifyPassword("password"u8),
      Throws.Nothing
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual("password"u8),
      Is.True,
      "Must be written to the `IFlashMemory` created by `IFlashMemoryFactory`"
    );
  }

  [Test]
  public void CreateFlashMemory_Mcp2221AController_CreateWithDeviceFilter()
  {
    var stagedFlashMemory = new FlashMemory();

    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(new FlashMemory(), stagedFlashMemory)
    );
    services.AddPseudoUsbHid(
      new PseudoUsbHidService([Mcp2221AControllerTests.CreatePseudoDevice()])
    );

    using var mcp2221A = Mcp2221AController.Create(
      serviceProvider: services.BuildServiceProvider(),
      usbHidDeviceFilter: static _ => true, // select all
      mcp2221AFilter: static _ => true // select all
    );

    Assert.That(
      () => _ = mcp2221A.Flash,
      Throws.Nothing
    );
    Assert.That(
      () => mcp2221A.Flash.ModifyPassword("password"u8),
      Throws.Nothing
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual("password"u8),
      Is.True,
      "Must be written to the `IFlashMemory` created by `IFlashMemoryFactory`"
    );
  }

  [Test]
  public async ValueTask CreateFlashMemory_Mcp2221AController_CreateWithDeviceFilterAsync()
  {
    var stagedFlashMemory = new FlashMemory();

    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(new FlashMemory(), stagedFlashMemory)
    );
    services.AddPseudoUsbHid(
      new PseudoUsbHidService([Mcp2221AControllerTests.CreatePseudoDevice()])
    );

    await using var mcp2221A = await Mcp2221AController.CreateAsync(
      serviceProvider: services.BuildServiceProvider(),
      usbHidDeviceFilter: static _ => true, // select all
      mcp2221AFilter: static _ => true // select all
    );

    Assert.That(
      () => _ = mcp2221A.Flash,
      Throws.Nothing
    );
    Assert.That(
      () => mcp2221A.Flash.ModifyPassword("password"u8),
      Throws.Nothing
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual("password"u8),
      Is.True,
      "Must be written to the `IFlashMemory` created by `IFlashMemoryFactory`"
    );
  }

  [Test]
  public void Restore()
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(initialFlashMemory, stagedFlashMemory)
    );

    using var mcp2221A = Mcp2221AController.Create(
      serviceProvider: services.BuildServiceProvider(),
      usbHidDevice: Mcp2221AControllerTests.CreatePseudoDevice()
    );

    var initialUsbProductId = mcp2221A.Flash.UsbProductId;
    var modifiedUsbProductId = (ushort)((~initialUsbProductId) & 0xFFFF);

    mcp2221A.Flash.ModifyUsbProductId(modifiedUsbProductId);

    Assert.That(
      mcp2221A.Flash.UsbProductId,
      Is.EqualTo(modifiedUsbProductId),
      "before restore"
    );

    mcp2221A.Flash.Restore();

    Assert.That(
      mcp2221A.Flash.UsbProductId,
      Is.EqualTo(initialUsbProductId),
      "after restore"
    );
    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [Test]
  public void Restore_PasswordModificationMustBeKept()
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(initialFlashMemory, stagedFlashMemory)
    );

    using var mcp2221A = Mcp2221AController.Create(
      serviceProvider: services.BuildServiceProvider(),
      usbHidDevice: Mcp2221AControllerTests.CreatePseudoDevice()
    );

    var password = "password"u8;
    var initialUsbProductId = mcp2221A.Flash.UsbProductId;

    mcp2221A.Flash.ModifyUsbProductId((ushort)((~initialUsbProductId) & 0xFFFF));
    mcp2221A.Flash.ModifyPassword(password);

    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      "before restore"
    );

    mcp2221A.Flash.Restore();

    Assert.That(
      mcp2221A.Flash.UsbProductId,
      Is.EqualTo(initialUsbProductId),
      "after restore"
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      $"{nameof(mcp2221A.Flash.IsDirty)} must remain true due to pending password modification"
    );
    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual(password),
      Is.True
    );
  }

  [Test]
  public void Restore_ReturnsSelf()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(
      mcp2221A.Flash.Restore(),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      mcp2221A.Flash.Restore().Restore(),
      Is.SameAs(mcp2221A.Flash)
    );
  }

  [Test]
  public void Restore_AfterWrite_RollsBackToInitialState()
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    var services = new ServiceCollection();

    services.AddSingleton<IFlashMemoryFactory>(
      new AllocatedFlashMemoryFactory(initialFlashMemory, stagedFlashMemory)
    );

    using var mcp2221A = Mcp2221AController.Create(
      serviceProvider: services.BuildServiceProvider(),
      usbHidDevice: Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    var initialUsbProductId = mcp2221A.Flash.UsbProductId;

    // [#1] first modify and write
    mcp2221A.Flash.ModifyUsbProductId((ushort)((~initialUsbProductId) & 0xFFFF));

    Assert.That(
      () => mcp2221A.Flash.Write(),
      Throws.Nothing
    );

    // [#2] second modify and restore
    mcp2221A
      .Flash
      .ModifyUsbProductId(0x1234)
      .Restore();

    Assert.That(mcp2221A.Flash.UsbProductId, Is.EqualTo(initialUsbProductId));
    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [Test]
  public void IsDirty_InitialState()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False);
  }

  [Test]
  public void IsDirty_Modified()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    mcp2221A.Flash.ModifyUsbProductId(
      (~mcp2221A.Flash.UsbProductId) & 0xFFFF
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after modified");
  }

  [Test]
  public void IsDirty_ModifiedButSameAsInitial()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    var initialUsbProductId = mcp2221A.Flash.UsbProductId;

    mcp2221A.Flash.ModifyUsbProductId(
      (~initialUsbProductId) & 0xFFFF
    );
    mcp2221A.Flash.ModifyUsbProductId(
      initialUsbProductId
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modified");
  }

  [Test]
  public void IsDirty_PasswordModified()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after password modified");
  }

  [Test]
  public void IsDirty_AfterRestore()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyUsbProductId(
      (~mcp2221A.Flash.UsbProductId) & 0xFFFF
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before restore");

    mcp2221A.Flash.Restore();

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after restore");
  }

  [Test]
  public void IsDirty_PasswordModified_AfterRestore()
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before restore");

    mcp2221A.Flash.Restore();

    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      "password modification must be maintained even after restore"
    );
  }
}
