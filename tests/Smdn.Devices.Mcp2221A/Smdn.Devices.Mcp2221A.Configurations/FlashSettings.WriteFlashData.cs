// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Configurations;

[TestFixture]
public partial class FlashSettingsTests {
  private static ValueTask Write(FlashSettings flash, CancellationToken cancellationToken)
  {
    flash.Write(cancellationToken);

    return default;
  }

  private static ValueTask WriteAsync(FlashSettings flash, CancellationToken cancellationToken)
    => flash.WriteAsync(cancellationToken);

  [Test]
  public void Write_IsNotDirty()
    => WriteSyncOrAsync_IsNotDirty(Write);

  [Test]
  public void WriteAsync_IsNotDirty()
    => WriteSyncOrAsync_IsNotDirty(WriteAsync);

  private void WriteSyncOrAsync_IsNotDirty(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before write");

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void Write_IsDirty_Modified()
    => WriteSyncOrAsync_IsDirty_Modified(Write);

  [Test]
  public void WriteAsync_IsDirty_Modified()
    => WriteSyncOrAsync_IsDirty_Modified(WriteAsync);

  private void WriteSyncOrAsync_IsDirty_Modified(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    mcp2221A.Flash.ModifyUsbProductId(
      (~mcp2221A.Flash.UsbProductId) & 0xFFFF
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");
  }

  [Test]
  public void Write_IsDirty_Modified_ThenModifyAgain()
    => WriteSyncOrAsync_IsDirty_Modified_ThenModifyAgain(Write);

  [Test]
  public void WriteAsync_IsDirty_Modified_ThenModifyAgain()
    => WriteSyncOrAsync_IsDirty_Modified_ThenModifyAgain(WriteAsync);

  private void WriteSyncOrAsync_IsDirty_Modified_ThenModifyAgain(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // write #1
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // write #2
    );

    var initialUsbProductId = mcp2221A.Flash.UsbProductId;

    // [#1] modify and write, first attempt
    mcp2221A.Flash.ModifyUsbProductId((~initialUsbProductId) & 0xFFFF);

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing,
      "write #1"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after first write");

    // [#2] modify again and write
    mcp2221A.Flash.ModifyUsbProductId((~initialUsbProductId) & 0xFFFF);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after second modification");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing,
      "write #2"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after second write");
  }

  [Test]
  public void Write_IsDirty_PasswordModified()
    => WriteSyncOrAsync_IsDirty_PasswordModified(Write);

  [Test]
  public void WriteAsync_IsDirty_PasswordModified()
    => WriteSyncOrAsync_IsDirty_PasswordModified(WriteAsync);

  private void WriteSyncOrAsync_IsDirty_PasswordModified(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");
  }

  [Test]
  public void Write_IsDirty_BothModified()
    => WriteSyncOrAsync_IsDirty_BothModified(Write);

  [Test]
  public void WriteAsync_IsDirty_BothModified()
    => WriteSyncOrAsync_IsDirty_BothModified(WriteAsync);

  private void WriteSyncOrAsync_IsDirty_BothModified(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    mcp2221A.Flash.ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF);
    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");
  }

  [Test]
  public void Write_AfterRestore()
    => WriteSyncOrAsync_AfterRestore(Write);

  [Test]
  public void WriteAsync_AfterRestore()
    => WriteSyncOrAsync_AfterRestore(WriteAsync);

  private void WriteSyncOrAsync_AfterRestore(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A
      .Flash
      .ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF)
      .Restore();

    // command should not be sent after restore
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent after restore"
    );
  }

  [Test]
  public void Write_CancellationRequested()
    => WriteSyncOrAsync_CancellationRequested(Write);

  [Test]
  public void WriteAsync_CancellationRequested()
    => WriteSyncOrAsync_CancellationRequested(WriteAsync);

  private void WriteSyncOrAsync_CancellationRequested(Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void Write_ExceptionThrown_MaintainsDirtyState()
    => WriteSyncOrAsync_ExceptionThrown_MaintainsDirtyState(Write);

  [Test]
  public void WriteAsync_ExceptionThrown_MaintainsDirtyState()
    => WriteSyncOrAsync_ExceptionThrown_MaintainsDirtyState(WriteAsync);

  private void WriteSyncOrAsync_ExceptionThrown_MaintainsDirtyState(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x02: Command not supported
      // [2-63] Don't care
      "B1-02-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    mcp2221A.Flash.ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws
        .TypeOf<Mcp2221ACommandException>()
        .With
        .Message.Contain("Code: 0x02") // 0x02: Command not supported
    );

    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      $"{nameof(mcp2221A.Flash.IsDirty)} must remain true if the write operation fails"
    );
  }

  [Test]
  public void Write_ControllerDisposed()
    => WriteSyncOrAsync_ControllerDisposed(Write);

  [Test]
  public void WriteAsync_ControllerDisposed()
    => WriteSyncOrAsync_ControllerDisposed(WriteAsync);

  private void WriteSyncOrAsync_ControllerDisposed(Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyPassword("password"u8);

    var flash = mcp2221A.Flash;

    Assert.That(flash.IsDirty, Is.True, "before write");

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    mcp2221A.Dispose();

    Assert.That(
      async () => await writeSyncOrAsync(flash, default),
      Throws.TypeOf<ObjectDisposedException>()
    );

    Assert.That(
      flash.IsDirty,
      Is.True,
      $"{nameof(flash.IsDirty)} must remain true if the write operation fails"
    );

    // This throws ObjectDisposedException : Cannot access a disposed object.
    // Assert.That(
    //   Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
    //   Is.Zero,
    //   "command should not be sent"
    // );
  }

  [Test]
  public void Write_WriteFlashDataCommand_CommandNotAllowedResponse()
    => WriteSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(Write);

  [Test]
  public void WriteAsync_WriteFlashDataCommand_CommandNotAllowedResponse()
    => WriteSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    // set IsDirty to true
    mcp2221A.Flash.ModifyUsbVendorId(
      (~mcp2221A.Flash.UsbVendorId) & 0xFFFF
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x03: Command not allowed
      // [2-63] Don't care
      "B1-03-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.TypeOf<FlashWriteAccessException>()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. stagedFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  [Test]
  public void Write_WriteFlashDataCommand_WriteChipSettings()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings(Write);

  [Test]
  public void WriteAsync_WriteFlashDataCommand_WriteChipSettings()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings(WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      initialFlashMemory,
      stagedFlashMemory
    );

    initialFlashMemory.ChipSettings.Clear();

    mcp2221A.Flash.ModifyUsbVendorId(0); // set IsDirty to true

    new byte[] {
      0xC0, 0xC1, 0xC2, 0xC3,
      0xC4, 0xC5, 0xC6, 0xC7,
      0xC8, 0xC9,
    }.CopyTo(stagedFlashMemory.ChipSettings);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. stagedFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  [Test]
  public void Write_WriteFlashDataCommand_WriteChipSettings_Password()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_Password(Write);

  [Test]
  public void WriteAsync_WriteFlashDataCommand_WriteChipSettings_Password()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_Password(WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_Password(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    mcp2221A.Flash.ModifyPassword("password"u8);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. stagedFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  [Test]
  public void Write_WriteFlashDataCommand_WriteChipSettings_StoredPasswordShouldBeRewritten()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_StoredPasswordShouldBeRewritten(Write);

  [Test]
  public void WriteAsync_WriteFlashDataCommand_WriteChipSettings_StoredPasswordShouldBeRewritten()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_StoredPasswordShouldBeRewritten(WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteChipSettings_StoredPasswordShouldBeRewritten(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        vendorId: 0xFFFF
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    /*
     * write #1
     */
    mcp2221A.Flash.ModifyUsbVendorId(0); // set IsDirty to true
    mcp2221A.Flash.ModifyPassword(password);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #1");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #1");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );

    /*
     * write #2
     */
    mcp2221A.Flash.ModifyUsbVendorId(1); // set IsDirty to true

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #2");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #2");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  [Test]
  public void Write_WriteFlashDataCommand_WriteGpSettings()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteGpSettings(Write);

  [Test]
  public void WriteAsync_WriteFlashDataCommand_WriteGpSettings()
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteGpSettings(WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteGpSettings(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      initialFlashMemory,
      stagedFlashMemory
    );

    initialFlashMemory.ChipSettings.Clear();

    mcp2221A.Flash.ModifyGpSetting(0, GpFunction.Gpio); // set IsDirty to true

    new byte[] {
      0xC0, 0xC1, 0xC2, 0xC3,
    }.CopyTo(stagedFlashMemory.GpSettings);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x01, // [1] Write GP Settings
          .. stagedFlashMemory.GpSettings, // [2-5] GP0-GP3 Power-Up Settings
          .. Enumerable.Repeat<byte>(0x00, 64 - 6) // [6-63]: don't care
        ]
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString()
  {
    const string InitialUsbManufacturerString = "Microchip Technology Inc.";

    yield return new object[] { InitialUsbManufacturerString, "012345678901234567890123456789" }; // max length
    yield return new object[] { InitialUsbManufacturerString, "Vendor" };
    yield return new object[] { InitialUsbManufacturerString, "🔌" };
    yield return new object[] { InitialUsbManufacturerString, "" };
    yield return new object[] { "", "Vendor" };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString))]
  public void Write_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString(string initialManufacturer, string manufacturer)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString(initialManufacturer, manufacturer, Write);

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString))]
  public void WriteAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString(string initialManufacturer, string manufacturer)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString(initialManufacturer, manufacturer, WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbManufacturerDescriptorString(
    string initialManufacturer,
    string manufacturer,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialManufacturer
      )
    );

    // fill the entire register area with dummy data
    mcp2221A.Flash.ModifyUsbManufacturerString(
      new string('\uEEEE', 30) // 30 chars of U+EEEE (Private Use Area)
    );
    // then set the desired string
    mcp2221A.Flash.ModifyUsbManufacturerString(manufacturer);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    var numberOfBytes = Encoding.Unicode.GetByteCount(manufacturer);

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x02, // [1] Write USB Manufacturer Descriptor String
          (byte)(numberOfBytes + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. Encoding.Unicode.GetBytes(manufacturer), // [4-] 16-bit Unicode chars
          // Verify that zeros are written to ranges outside the actual
          // character string, rather than dummy data.
          .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfBytes)
        ]
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString()
  {
    const string InitialProductDescriptorString = "MCP2221 USB-I2C/UART Combo";

    yield return new object[] { InitialProductDescriptorString, "012345678901234567890123456789" }; // max length
    yield return new object[] { InitialProductDescriptorString, "Product" };
    yield return new object[] { InitialProductDescriptorString, "🔌" };
    yield return new object[] { InitialProductDescriptorString, "" };
    yield return new object[] { "", "Product" };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString))]
  public void Write_WriteFlashDataCommand_WriteUsbProductDescriptorString(string initialProduct, string product)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString(initialProduct, product, Write);

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString))]
  public void WriteAsyncWriteFlashDataCommand_WriteUsbProductDescriptorString(string initialProduct, string product)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString(initialProduct, product, WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbProductDescriptorString(
    string initialProduct,
    string product,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: initialProduct
      )
    );

    // fill the entire register area with dummy data
    mcp2221A.Flash.ModifyUsbProductString(
      new string('\uEEEE', 30) // 30 chars of U+EEEE (Private Use Area)
    );
    // then set the desired string
    mcp2221A.Flash.ModifyUsbProductString(product);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    var numberOfBytes = Encoding.Unicode.GetByteCount(product);

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x03, // [1] Write USB Product Descriptor String
          (byte)(numberOfBytes + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. Encoding.Unicode.GetBytes(product), // [4-] 16-bit Unicode chars
          // Verify that zeros are written to ranges outside the actual
          // character string, rather than dummy data.
          .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfBytes)
        ]
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString()
  {
    const string InitialSerialNumberDescriptorString = "XXXXXXXXXX";

    yield return new object[] { InitialSerialNumberDescriptorString, "012345678901234567890123456789" }; // max length
    yield return new object[] { InitialSerialNumberDescriptorString, "Serial Number" };
    yield return new object[] { InitialSerialNumberDescriptorString, "🔌" };
    yield return new object[] { InitialSerialNumberDescriptorString, "" };
    yield return new object[] { "", "Serial Number" };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString))]
  public void Write_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString(string initialSerialNumber, string serialNumber)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString(initialSerialNumber, serialNumber, Write);

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString))]
  public void WriteAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString(string initialSerialNumber, string serialNumber)
    => WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString(initialSerialNumber, serialNumber, WriteAsync);

  private void WriteSyncOrAsync_WriteFlashDataCommand_WriteUsbSerialNumberDescriptorString(
    string initialSerialNumber,
    string serialNumber,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: initialSerialNumber
      )
    );

    // fill the entire register area with dummy data
    mcp2221A.Flash.ModifyUsbSerialNumberString(
      new string('\uEEEE', 30) // 30 chars of U+EEEE (Private Use Area)
    );
    // then set the desired string
    mcp2221A.Flash.ModifyUsbSerialNumberString(serialNumber);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    var numberOfBytes = Encoding.Unicode.GetByteCount(serialNumber);

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x04, // [1] Write USB Serial Number Descriptor String
          (byte)(numberOfBytes + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. Encoding.Unicode.GetBytes(serialNumber), // [4-] 16-bit Unicode chars
          // Verify that zeros are written to ranges outside the actual
          // character string, rather than dummy data.
          .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfBytes)
        ]
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings()
  {
    const string InitialUsbManufacturerString = "Microchip Technology Inc.";
    const string InitialProductDescriptorString = "MCP2221 USB-I2C/UART Combo";
    const string InitialSerialNumberDescriptorString = "XXXXXXXXXX";

    yield return new object?[] {
      InitialUsbManufacturerString, "Vendor",
      InitialProductDescriptorString, "Product",
      InitialSerialNumberDescriptorString, null
    };

    yield return new object?[] {
      InitialUsbManufacturerString, null,
      InitialProductDescriptorString, "Product",
      InitialSerialNumberDescriptorString, "Serial Number"
    };

    yield return new object?[] {
      InitialUsbManufacturerString, "Vendor",
      InitialProductDescriptorString, "Product",
      InitialSerialNumberDescriptorString, "Serial Number"
    };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings))]
  public void Write_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings(
    string initialManufacturer,
    string? manufacturer,
    string initialProduct,
    string? product,
    string initialSerialNumber,
    string? serialNumber
  )
    => WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings(
      initialManufacturer,
      manufacturer,
      initialProduct,
      product,
      initialSerialNumber,
      serialNumber,
      Write
    );

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings))]
  public void WriteAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings(
    string initialManufacturer,
    string? manufacturer,
    string initialProduct,
    string? product,
    string initialSerialNumber,
    string? serialNumber
  )
    => WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings(
      initialManufacturer,
      manufacturer,
      initialProduct,
      product,
      initialSerialNumber,
      serialNumber,
      WriteAsync
    );

  private void WriteSyncOrAsync_MultipleWriteFlashDataCommands_WriteUsbDescriptorStrings(
    string initialManufacturer,
    string? manufacturer,
    string initialProduct,
    string? product,
    string initialSerialNumber,
    string? serialNumber,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialManufacturer,
        product: initialProduct,
        serialNumber: initialSerialNumber
      )
    );

    // fill the entire register area with dummy data first,
    // then set the desired string
    if (manufacturer is not null) {
      mcp2221A
        .Flash
        .ModifyUsbManufacturerString(new string('\uEEEE', 30) /* 30 chars of U+EEEE (Private Use Area) */)
        .ModifyUsbManufacturerString(manufacturer);
    }

    if (product is not null) {
      mcp2221A
        .Flash
        .ModifyUsbProductString(new string('\uEEEE', 30) /* 30 chars of U+EEEE (Private Use Area) */)
        .ModifyUsbProductString(product);
    }

    if (serialNumber is not null) {
      mcp2221A
        .Flash
        .ModifyUsbSerialNumberString(new string('\uEEEE', 30) /* 30 chars of U+EEEE (Private Use Area) */)
        .ModifyUsbSerialNumberString(serialNumber);
    }

    // [MCP2221A] 3.1.3 WRITE FLASH DATA
    // [1] 0x00: Command completed successfully
    // [2-63] Don't care
    string[] responses = [];

    if (manufacturer is not null)
      responses = [.. responses, "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))];
    if (product is not null)
      responses = [.. responses, "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))];
    if (serialNumber is not null)
      responses = [.. responses, "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))];

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      responses
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    var expectedSentCommands = new List<byte[]>();

    if (manufacturer is not null) {
      var numberOfManufacturerBytes = Encoding.Unicode.GetByteCount(manufacturer);

      expectedSentCommands.Add([
        0xB1, // [0] WRITE FLASH DATA
        0x02, // [1] Write USB Manufacturer Descriptor String
        (byte)(numberOfManufacturerBytes + 2), // [2] Number of bytes + 2
        0x03, // [3] must always be 0x03
        .. Encoding.Unicode.GetBytes(manufacturer), // [4-] 16-bit Unicode chars
        // Verify that zeros are written to ranges outside the actual
        // character string, rather than dummy data.
        .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfManufacturerBytes)
      ]);
    }

    if (product is not null) {
      var numberOfProductBytes = Encoding.Unicode.GetByteCount(product);

      expectedSentCommands.Add([
        0xB1, // [0] WRITE FLASH DATA
        0x03, // [1] Write USB Product Descriptor String
        (byte)(numberOfProductBytes + 2), // [2] Number of bytes + 2
        0x03, // [3] must always be 0x03
        .. Encoding.Unicode.GetBytes(product), // [4-] 16-bit Unicode chars
        // Verify that zeros are written to ranges outside the actual
        // character string, rather than dummy data.
        .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfProductBytes)
      ]);
    }

    if (serialNumber is not null) {
      var numberOfSerialNumberBytes = Encoding.Unicode.GetByteCount(serialNumber);

      expectedSentCommands.Add([
        0xB1, // [0] WRITE FLASH DATA
        0x04, // [1] Write USB Serial Number Descriptor String
        (byte)(numberOfSerialNumberBytes + 2), // [2] Number of bytes + 2
        0x03, // [3] must always be 0x03
        .. Encoding.Unicode.GetBytes(serialNumber), // [4-] 16-bit Unicode chars
        // Verify that zeros are written to ranges outside the actual
        // character string, rather than dummy data.
        .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfSerialNumberBytes)
      ]);
    }

    for (var i = 0; i < expectedSentCommands.Count; i++) {
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: i),
        SequenceIs.EqualTo(expectedSentCommands[i])
      );
    }
  }

  [Test]
  public void Write_MultipleWriteFlashDataCommands_CancelAfterFirstCommand()
    => WriteSyncOrAsync_MultipleWriteFlashDataCommands_CancelAfterFirstCommand(Write);

  [Test]
  public void WriteAsync_MultipleWriteFlashDataCommands_CancelAfterFirstCommand()
    => WriteSyncOrAsync_MultipleWriteFlashDataCommands_CancelAfterFirstCommand(WriteAsync);

  private void WriteSyncOrAsync_MultipleWriteFlashDataCommands_CancelAfterFirstCommand(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    const string InitialUsbManufacturerString = "Microchip Technology Inc.";
    const string InitialProductDescriptorString = "MCP2221 USB-I2C/UART Combo";
    const string NewUsbManufacturerString = "Vendor";
    const string NewProductDescriptorString = "Product";

    using var device = Mcp2221AControllerTests.CreatePseudoDevice(
      manufacturer: InitialUsbManufacturerString,
      product: InitialProductDescriptorString
    );
    using var cts = new CancellationTokenSource();
    var isWriteSyncOrAsyncRunning = false;
    var numberOfIssuedWriteFlashDataCommands = 0;

    device.OnEndPointOpenedAction = () => {
      device.EndPoint.OnReadingAction = () => {
        if (isWriteSyncOrAsyncRunning) {
          if (++numberOfIssuedWriteFlashDataCommands == 1)
            cts.Cancel(); // Cancel when the first command response is returned
        }
      };
    };

    using var mcp2221A = Mcp2221AController.Create(device);

    mcp2221A
      .Flash
      .ModifyUsbManufacturerString(NewUsbManufacturerString)
      .ModifyUsbProductString(NewProductDescriptorString);

    // [MCP2221A] 3.1.3 WRITE FLASH DATA
    // [1] 0x00: Command completed successfully
    // [2-63] Don't care
    string[] responses = [
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Manufacturer Descriptor
      // "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // Write USB Vendor Descriptor
    ];

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      responses
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    isWriteSyncOrAsyncRunning = true;

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");

    var numberOfManufacturerBytes = Encoding.Unicode.GetByteCount(NewUsbManufacturerString);

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>([
        0xB1, // [0] WRITE FLASH DATA
        0x02, // [1] Write USB Manufacturer Descriptor String
        (byte)(numberOfManufacturerBytes + 2), // [2] Number of bytes + 2
        0x03, // [3] must always be 0x03
        .. Encoding.Unicode.GetBytes(NewUsbManufacturerString), // [4-] 16-bit Unicode chars
        // Verify that zeros are written to ranges outside the actual
        // character string, rather than dummy data.
        .. Enumerable.Repeat<byte>(0x00, 64 - 4 - numberOfManufacturerBytes)
      ])
    );
  }

  // 初期状態で書き込み保護が PasswordProtected に設定されているデバイスに対し、
  // ModifyPassword を一度も呼び出さずに Write を実行した場合、
  // InvalidOperationException がスローされることを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.Write"/> throws an <see cref="InvalidOperationException"/>
  /// when the initial device write protection level is <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// and no password has been provided.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>The device's initial configuration has <see cref="FlashSettings.WriteProtectionLevel"/> set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> has not been called.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Calling <see cref="FlashSettings.Write"/> throws an <see cref="InvalidOperationException"/>.</description></item>
  ///   <item><description>No write commands are issued to the device to prevent overwriting the password with indeterminate data.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException()
    => WriteSyncOrAsync_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException(Write);

  /// <inheritdoc cref="Write_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException"/>
  [Test]
  public void WriteAsync_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException()
    => WriteSyncOrAsync_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException(WriteAsync);

  private void WriteSyncOrAsync_InitialStatePasswordProtected_PasswordNotProvided_ThrowsException(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_01 // CHIPPROT(1-0): 01(Password-protected)
      )
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // mcp2221A.Flash.SendAccessPassword(...);

    mcp2221A
      .Flash
      // .ModifyPassword("password"u8) // password not provided
      .ModifyUsbVendorId((~mcp2221A.Flash.UsbVendorId) & 0xFFFF); // set IsDirty to true

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws
        .InvalidOperationException
        .With
        .Message.Contains("no password has been provided")
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");
    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  // ModifyWriteProtection で PasswordProtected に変更したが、
  // ModifyPassword を一度も呼び出さずに Write を実行した場合、
  // InvalidOperationException がスローされることを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.Write"/> throws an <see cref="InvalidOperationException"/>
  /// when <see cref="FlashSettings.ModifyWriteProtection"/> sets the protection level to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// without calling <see cref="FlashSettings.ModifyPassword"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description><see cref="FlashSettings.ModifyWriteProtection"/> is called with <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> has not been called.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Calling <see cref="FlashSettings.Write"/> throws an <see cref="InvalidOperationException"/>.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException()
    => WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException(Write);

  /// <inheritdoc cref="Write_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException"/>
  [Test]
  public void WriteAsync_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException()
    => WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException(WriteAsync);

  private void WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordNotProvided_ThrowsException(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      )
    );

    mcp2221A
      .Flash
      // .ModifyPassword("password"u8) // password not provided
      .ModifyWriteProtection(
        // set to PasswordProtected and set IsDirty to true
        DeviceConfigurationProtectionLevel.PasswordProtected
      );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws
        .InvalidOperationException
        .With
        .Message.Contains("no password has been provided")
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");
    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  // 初期状態で書き込み保護が PasswordProtected に設定されているデバイスに対し、
  // ModifyPassword でパスワードを指定した上で Write を実行した場合、
  // InvalidOperationException がスローされないことを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.Write"/> executes without throwing an <see cref="InvalidOperationException"/>
  /// when the initial protection level is <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// and a password has been explicitly provided via <see cref="FlashSettings.ModifyPassword"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>The device's initial configuration has <see cref="FlashSettings.WriteProtectionLevel"/> set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> is called to specify a valid 8-byte password.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Calling <see cref="FlashSettings.Write"/> does not throw an <see cref="InvalidOperationException"/>.</description></item>
  ///   <item><description>The write command is successfully constructed and sent to the device.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException()
    => WriteSyncOrAsync_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException(Write);

  /// <inheritdoc cref="Write_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException"/>
  [Test]
  public void WriteAsync_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException()
    => WriteSyncOrAsync_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException(WriteAsync);

  private void WriteSyncOrAsync_InitialStatePasswordProtected_PasswordProvided_DoesNotThrowException(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_01 // CHIPPROT(1-0): 01(Password-protected)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // It is also assumed that the response to the Write Flash Data command
    // reflects an unlocked state.
    // mcp2221A.Flash.SendAccessPassword(...);

    mcp2221A
      .Flash
      .ModifyPassword(password); // provide password and set IsDirty to true

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // ModifyWriteProtection で PasswordProtected に変更し、
  // かつ ModifyPassword でパスワードを指定した上で Write を実行した場合、
  // InvalidOperationException がスローされないことを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.Write"/> executes without throwing an <see cref="InvalidOperationException"/>
  /// when protection is modified to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// and <see cref="FlashSettings.ModifyPassword"/> is also called.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description><see cref="FlashSettings.ModifyWriteProtection"/> is called with <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> is called with a valid 8-byte password buffer.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Calling <see cref="FlashSettings.Write"/> does not throw an <see cref="InvalidOperationException"/>.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException()
    => WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException(Write);

  /// <inheritdoc cref="Write_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException"/>
  [Test]
  public void WriteAsync_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException()
    => WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException(WriteAsync);

  private void WriteSyncOrAsync_ModifyWriteProtectionToPasswordProtected_PasswordProvided_DoesNotThrowException(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    mcp2221A
      .Flash
      .ModifyWriteProtection(
        // set to PasswordProtected and set IsDirty to true
        DeviceConfigurationProtectionLevel.PasswordProtected
      )
      .ModifyPassword(password); // provide password

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // 書き込み保護が PasswordProtected 以外（None または PermanentlyLocked）に設定されている場合、
  // ModifyPassword が呼び出されていなくても Write 実行時に InvalidOperationException がスローされないことを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.Write"/> does not throw an <see cref="InvalidOperationException"/>
  /// when the write protection level is set to a level other than <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>,
  /// even if <see cref="FlashSettings.ModifyPassword"/> has not been called.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description><see cref="FlashSettings.WriteProtectionLevel"/> is set to <see cref="DeviceConfigurationProtectionLevel.None"/> or <see cref="DeviceConfigurationProtectionLevel.PermanentlyLocked"/>.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> has not been called.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Calling <see cref="FlashSettings.Write"/> does not throw an <see cref="InvalidOperationException"/>.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [TestCase(DeviceConfigurationProtectionLevel.None)]
  [TestCase(DeviceConfigurationProtectionLevel.PermanentlyLocked)]
  public void Write_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException(DeviceConfigurationProtectionLevel protectionLevel)
    => WriteSyncOrAsync_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException(protectionLevel, Write);

  /// <inheritdoc cref="Write_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException"/>
  [TestCase(DeviceConfigurationProtectionLevel.None)]
  [TestCase(DeviceConfigurationProtectionLevel.PermanentlyLocked)]
  public void WriteAsync_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException(DeviceConfigurationProtectionLevel protectionLevel)
    => WriteSyncOrAsync_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException(protectionLevel, WriteAsync);

  private void WriteSyncOrAsync_ProtectionLevelNotPasswordProtected_PasswordNotProvided_DoesNotThrowException(
    DeviceConfigurationProtectionLevel protectionLevel,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_01 // CHIPPROT(1-0): 01(Password-protected)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // It is also assumed that the response to the Write Flash Data command
    // reflects an unlocked state.
    // mcp2221A.Flash.SendAccessPassword(...);

    mcp2221A
      .Flash
      // .ModifyPassword("password"u8) // password not provided
      .ModifyWriteProtection(
        // set to non-PasswordProtected value and set IsDirty to true
        protectionLevel
      );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.Not.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. stagedFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // 1回目の Write で PasswordProtected とパスワードを設定して書き込み完了後、
  // ModifyPassword を再呼び出しせずに他の設定（CHIPSETTING1〜3等）のみ変更して 2回目の Write を呼び出した際、
  // 例外スローが発生せずに正常に書き込みが完了し、かつ 2回目の Write Chip Settings コマンドパケット内で
  // 最初の指定値（CHIPSETTING0 および PASS0-PASS7）がそのまま維持されて再送信されることを検証する。
  /// <summary>
  /// Tests that executing a second <see cref="FlashSettings.Write"/> call after modifying non-password settings
  /// does not throw an exception and preserves the previously configured protection level (CHIPSETTING0)
  /// and password bytes (PASS0–PASS7) in the generated command packet.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>A first call to <see cref="FlashSettings.Write"/> completes successfully with <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/> and an 8-byte password.</description></item>
  ///   <item><description>Subsequent modifications are made to other settings (e.g., registers corresponding to CHIPSETTING1–3) without re-calling <see cref="FlashSettings.ModifyPassword"/>.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>The second call to <see cref="FlashSettings.Write"/> completes successfully without throwing an <see cref="InvalidOperationException"/>.</description></item>
  ///   <item><description>The generated <c>Write Chip Settings</c> command packet retains the CHIPSETTING0 protection level and PASS0–PASS7 password bytes configured during the first write operation.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds()
    => WriteSyncOrAsync_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds(Write);

  /// <inheritdoc cref="Write_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds"/>
  [Test]
  public void WriteAsync_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds()
    => WriteSyncOrAsync_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds(WriteAsync);

  private void WriteSyncOrAsync_SecondWriteWithOtherSettingsModified_PreservesPasswordAndSucceeds(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    /*
     * write #1
     */
    mcp2221A
      .Flash
      .ModifyWriteProtection(
        // set to PasswordProtected and set IsDirty to true
        DeviceConfigurationProtectionLevel.PasswordProtected
      )
      .ModifyPassword(password); // provide password

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #1");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #1");
    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected),
      $"{nameof(mcp2221A.Flash.WriteProtectionLevel)} after write #1"
    );

    /*
     * write #2
     */
    mcp2221A
      .Flash
      // .ModifyPassword("PASSWD#2") // does not re-provide password
      .ModifyUsbProductId(
        // modify USBPIDL/USBPIDH of Chip Settings area and set IsDirty to true
        (~mcp2221A.Flash.UsbProductId) & 0xFFFF
      );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #2");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #2");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // 1回目の Write で PasswordProtected とパスワードを設定した後に Restore() を呼び出し、
  // その後で他の設定（CHIPSETTING1〜3等）を変更して 2回目の Write を呼び出した場合でも、
  // 保持されていたパスワード情報が送信パケットに正しく反映され、かつ例外なく書き込みが成功することを検証する。
  /// <summary>
  /// Tests that invoking <see cref="FlashSettings.Restore"/> after a successful write operation
  /// does not discard the configured password buffer when executing a subsequent write with updated settings.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>A first write operation completes with <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/> and a configured password.</description></item>
  ///   <item><description><see cref="FlashSettings.Restore"/> is called, followed by modifications to other settings.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>The password buffer remains preserved in internal state despite calling <see cref="FlashSettings.Restore"/>.</description></item>
  ///   <item><description>The second <see cref="FlashSettings.Write"/> call succeeds without throwing an exception, sending the correct password bytes in the command packet.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket()
    => WriteSyncOrAsync_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket(Write);

  /// <inheritdoc cref="Write_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket"/>
  [Test]
  public void WriteAsync_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket()
    => WriteSyncOrAsync_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket(WriteAsync);

  private void WriteSyncOrAsync_SecondWriteAfterRestoreAndOtherSettingsModified_PreservesPasswordInCommandPacket(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    /*
     * write #1
     */
    mcp2221A
      .Flash
      .ModifyWriteProtection(
        // set to PasswordProtected and set IsDirty to true
        DeviceConfigurationProtectionLevel.PasswordProtected
      )
      .ModifyPassword(password); // provide password

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #1");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #1");
    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected),
      $"{nameof(mcp2221A.Flash.WriteProtectionLevel)} after write #1"
    );

    /*
     * write #2
     */
    mcp2221A
      .Flash
      // restore to initial settings
      .Restore()
      // .ModifyPassword("PASSWD#2") // does not re-provide password
      .ModifyUsbProductId(
        // modify USBVIDL/USBVIDH of Chip Settings area and set IsDirty to true
        (~mcp2221A.Flash.UsbVendorId) & 0xFFFF
      );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #2");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #2");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // PasswordProtected 状態のデバイスに対し、1回目の Write 完了後に他の設定を変更して 2回目の Write を行う際、
  // 送信パケット内の CHIPSETTING0 / PASS0-PASS7 に1回目の設定値が正しく維持されているものの、
  // デバイス側の書き込み保護ロック（実機対向時におけるSendAccessPassword 未発行）によって拒否された場合に、
  // 正しく FlashWriteAccessException がスローされ、かつステージング状態（IsDirty）が維持されることを検証する。
  /// <summary>
  /// Tests that executing a second write operation without unlocking the device via <see cref="FlashSettings.SendAccessPassword"/>
  /// results in a <see cref="FlashWriteAccessException"/> and retains staged changes, even when the command packet contains valid password bytes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>The device is in a write-protected state (<see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>).</description></item>
  ///   <item><description>A second write attempt is made without calling <see cref="FlashSettings.SendAccessPassword"/> to unlock Flash write access.</description></item>
  ///   <item><description>The device responds with a <c>0x03 Command not allowed</c> status code.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description><see cref="FlashSettings.Write"/> throws a <see cref="FlashWriteAccessException"/>.</description></item>
  ///   <item><description>Staged changes are retained and <see cref="FlashSettings.IsDirty"/> remains <see langword="true"/>.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState()
    => WriteSyncOrAsync_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState(Write);

  /// <inheritdoc cref="Write_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState"/>
  [Test]
  public void WriteAsync_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState()
    => WriteSyncOrAsync_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState(WriteAsync);

  private void WriteSyncOrAsync_SecondWriteWhenDeviceIsLocked_ThrowsExceptionAndPreservesDirtyState(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var password = "password"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_01 // CHIPPROT(1-0): 01(Password-protected)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // It is also assumed that the response to the Write Flash Data command
    // reflects an unlocked state.
    // mcp2221A.Flash.SendAccessPassword(...);

    /*
     * write #1
     */
    mcp2221A
      .Flash
      .ModifyPassword(password) // provide password
      .ModifyUsbVendorId((~mcp2221A.Flash.UsbVendorId) & 0xFFFF); // set IsDirty to true

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #1");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #1");

    /*
     * write #2
     */
    mcp2221A
      .Flash
      // .ModifyPassword("PASSWD#2") // does not re-provide password
      .ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF); // set IsDirty to true

    // attempt to write without unlocking Flash write access
    // mcp2221A.Flash.SendAccessPassword(...);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x03: Command not allowed
      // [2-63] Don't care
      "B1-03-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #2");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.TypeOf<FlashWriteAccessException>()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write #2");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // 1回目の Write 完了後に ModifyPassword で新しいパスワードを指定し、他の設定とともに 2回目の Write を呼び出した際、
  // 送信コマンドパケット内の PASS0-PASS7 が新しく指定されたパスワード値へ正しく更新されて再送信されることを検証する。
  /// <summary>
  /// Tests that calling <see cref="FlashSettings.ModifyPassword"/> with a new password prior to a second write operation
  /// correctly updates the password bytes (PASS0–PASS7) in the generated command packet.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>A first write operation completes with an initial password buffer.</description></item>
  ///   <item><description><see cref="FlashSettings.ModifyPassword"/> is called with a new 8-byte password buffer before initiating a second write operation.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>The second <see cref="FlashSettings.Write"/> call succeeds.</description></item>
  ///   <item><description>The generated <c>Write Chip Settings</c> command packet contains the updated password bytes instead of the previous password.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void Write_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket()
    => WriteSyncOrAsync_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket(Write);

  /// <inheritdoc cref="Write_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket"/>
  [Test]
  public void WriteAsync_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket()
    => WriteSyncOrAsync_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket(WriteAsync);

  private void WriteSyncOrAsync_SecondWriteWithUpdatedPassword_SendsNewPasswordInCommandPacket(
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync
  )
  {
    var passwordFirst = "PASSWD#1"u8;
    var passwordSecond = "PASSWD#2"u8;
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      new FlashMemory(),
      stagedFlashMemory
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // It is also assumed that the response to the Write Flash Data command
    // reflects an unlocked state.
    // mcp2221A.Flash.SendAccessPassword(...);

    /*
     * write #1
     */
    mcp2221A
      .Flash
      .ModifyWriteProtection(
        // set to PasswordProtected and set IsDirty to true
        DeviceConfigurationProtectionLevel.PasswordProtected
      )
      .ModifyPassword(passwordFirst); // provide password #1

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #1");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #1");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. passwordFirst, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );

    /*
     * write #2
     */
    mcp2221A
      .Flash
      .ModifyPassword(passwordSecond); // provide new password #2

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write #2");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write #2");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. stagedFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. passwordSecond, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  private static ValueTask WriteInitialSettings(FlashSettings flash, CancellationToken cancellationToken)
  {
    flash.WriteInitialSettings(cancellationToken);

    return default;
  }

  private static ValueTask WriteInitialSettingsAsync(FlashSettings flash, CancellationToken cancellationToken)
    => flash.WriteInitialSettingsAsync(cancellationToken);

  // 設定変更を行っていない初期状態（IsDirty が false）であっても、
  // IsDirty チェックをバイパスして物理デバイスへ書き込みコマンドが必ず送信されることを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.WriteInitialSettings"/> bypasses the <see cref="FlashSettings.IsDirty"/> check
  /// and forcibly transmits write commands to the device even when no settings have been modified.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>No modifications have been made to <see cref="FlashSettings"/> (<see cref="FlashSettings.IsDirty"/> is <see langword="false"/>).</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>The method completes successfully without throwing an exception.</description></item>
  ///   <item><description>Flash write commands are actually transmitted to the underlying transceiver stream.</description></item>
  ///   <item><description><see cref="FlashSettings.IsDirty"/> remains <see langword="false"/> after execution.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void WriteInitialSettings_IsNotDirty()
    => WriteInitialSettingsSyncOrAsync_IsNotDirty(WriteInitialSettings);

  /// <inheritdoc cref="WriteInitialSettings_IsNotDirty">
  [Test]
  public void WriteInitialSettingsAsync_IsNotDirty()
    => WriteInitialSettingsSyncOrAsync_IsNotDirty(WriteInitialSettingsAsync);

  private void WriteInitialSettingsSyncOrAsync_IsNotDirty(
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write Chip Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write GP Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Manufacturer Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Product Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // Write USB Serial Number Descriptor String
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before write");

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 0),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. initialFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. initialFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 1),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x01, // [1] Write GP Settings
          .. initialFlashMemory.GpSettings, // [2-5] GP0-GP3 Power-Up Settings
          .. Enumerable.Repeat<byte>(0x00, 64 - 6) // [6-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 2),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x02, // [1] Write USB Manufacturer Descriptor String
          (byte)(initialFlashMemory.UsbManufacturerDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbManufacturerDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 3),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x03, // [1] Write USB Product Descriptor String
          (byte)(initialFlashMemory.UsbProductDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbProductDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 4),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x04, // [1] Write USB Serial Number Descriptor String
          (byte)(initialFlashMemory.UsbSerialNumberDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbSerialNumberDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
  }

  // 設定に変更を加えた状態（IsDirty が true）で WriteInitialSettings を呼び出した際、
  // ステージングされていた変更が破棄されてセッション開始時の初期設定値が書き込まれ、実行後に IsDirty が false になることを検証する。
  /// <summary>
  /// Tests that calling <see cref="FlashSettings.WriteInitialSettings"/> when staged changes exist
  /// discards those staged changes, forcibly writes the initial settings to the device, and sets <see cref="FlashSettings.IsDirty"/> to <see langword="false"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>One or more properties (e.g., USB Product ID) have been modified in <see cref="FlashSettings"/> (<see cref="FlashSettings.IsDirty"/> is <see langword="true"/>).</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>Staged modifications are discarded, and property values revert to their initial values.</description></item>
  ///   <item><description>The command payload sent to the device contains the initial settings rather than the staged modifications.</description></item>
  ///   <item><description><see cref="FlashSettings.IsDirty"/> becomes <see langword="false"/> after completion.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void WriteInitialSettings_IsDirty([Values] bool enablePasswordProtected)
    => WriteInitialSettingsSyncOrAsync_IsDirty(enablePasswordProtected, WriteInitialSettings);

  /// <inheritdoc cref="WriteInitialSettings_IsDirty"/>
  [Test]
  public void WriteInitialSettingsAsync_IsDirty([Values] bool enablePasswordProtected)
    => WriteInitialSettingsSyncOrAsync_IsDirty(enablePasswordProtected, WriteInitialSettingsAsync);

  private void WriteInitialSettingsSyncOrAsync_IsDirty(
    bool enablePasswordProtected,
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();
    var hasPasswordProvided = enablePasswordProtected;
    var password = "password"u8;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    // set IsDirty to true
    mcp2221A
      .Flash
      .ModifyUsbVendorId((~mcp2221A.Flash.UsbVendorId) & 0xFFFF) // Chip Settings area
      .ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF) // Chip Settings area
      .ModifyGpSetting(0, GpFunction.Gpio, PinMode.Output, !mcp2221A.Flash.GpPin0.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(1, GpFunction.Gpio, PinMode.Output, !mcp2221A.Flash.GpPin1.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(2, GpFunction.Gpio, PinMode.Input, !mcp2221A.Flash.GpPin2.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(3, GpFunction.Gpio, PinMode.Input, !mcp2221A.Flash.GpPin3.GpioOutputValue) // GP Settings area
      .ModifyUsbManufacturerString("Vendor") // USB Manufacturer Descriptor String area
      .ModifyUsbProductString("Product") // USB Product Descriptor String area
      .ModifyUsbSerialNumberString("Serial Number"); // USB Serial Number Descriptor String area

    if (enablePasswordProtected) {
      mcp2221A
        .Flash
        .ModifyWriteProtection(DeviceConfigurationProtectionLevel.PasswordProtected) // Chip Settings area
        .ModifyPassword(password); // Chip Settings area
    }

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write Chip Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write GP Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Manufacturer Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Product Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // Write USB Serial Number Descriptor String
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 0),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. initialFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. hasPasswordProvided // [12-19] PASS0-PASS8
            ? password
            : initialFlashMemory.Password,
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 1),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x01, // [1] Write GP Settings
          .. initialFlashMemory.GpSettings, // [2-5] GP0-GP3 Power-Up Settings
          .. Enumerable.Repeat<byte>(0x00, 64 - 6) // [6-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 2),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x02, // [1] Write USB Manufacturer Descriptor String
          (byte)(initialFlashMemory.UsbManufacturerDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbManufacturerDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 3),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x03, // [1] Write USB Product Descriptor String
          (byte)(initialFlashMemory.UsbProductDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbProductDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 4),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x04, // [1] Write USB Serial Number Descriptor String
          (byte)(initialFlashMemory.UsbSerialNumberDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbSerialNumberDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
  }

  // キャンセル要求済みの CancellationToken を指定して WriteInitialSettings を呼び出した場合、
  // OperationCanceledException がスローされコマンド送信が行われないことを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.WriteInitialSettings"/> throws an <see cref="OperationCanceledException"/>
  /// when passed an already-canceled <see cref="CancellationToken"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>A <see cref="CancellationToken"/> in a canceled state is passed to <see cref="FlashSettings.WriteInitialSettings"/>.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>An <see cref="OperationCanceledException"/> (or <see cref="TaskCanceledException"/>) is thrown.</description></item>
  ///   <item><description>No write commands are transmitted to the transceiver.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void WriteInitialSettings_CancellationRequested()
    => WriteInitialSettingsSyncOrAsync_CancellationRequested(WriteInitialSettings);

  /// <inheritdoc cref="WriteInitialSettings_CancellationRequested"/>
  [Test]
  public void WriteInitialSettingsAsync_CancellationRequested()
    => WriteInitialSettingsSyncOrAsync_CancellationRequested(WriteInitialSettingsAsync);

  private void WriteInitialSettingsSyncOrAsync_CancellationRequested(
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    mcp2221A.Flash.ModifyPassword("password"u8);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after write");

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void WriteInitialSettings_ControllerDisposed()
    => WriteInitialSettingsSyncOrAsync_ControllerDisposed(WriteInitialSettings);

  [Test]
  public void WriteInitialSettingsAsync_ControllerDisposed()
    => WriteInitialSettingsSyncOrAsync_ControllerDisposed(WriteInitialSettings);

  private void WriteInitialSettingsSyncOrAsync_ControllerDisposed(
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    mcp2221A.Flash.ModifyPassword("password"u8);

    var flash = mcp2221A.Flash;

    Assert.That(flash.IsDirty, Is.True, "before write");

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    mcp2221A.Dispose();

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(flash, default),
      Throws.TypeOf<ObjectDisposedException>()
    );

    Assert.That(
      flash.IsDirty,
      Is.True,
      $"{nameof(flash.IsDirty)} must remain true if the write operation fails"
    );

    // This throws ObjectDisposedException : Cannot access a disposed object.
    // Assert.That(
    //   Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
    //   Is.Zero,
    //   "command should not be sent"
    // );
  }

  [Test]
  public void WriteInitialSettings_WriteFlashDataCommand_CommandNotAllowedResponse()
    => WriteInitialSettingsSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(WriteInitialSettings);

  [Test]
  public void WriteInitialSettingsAsync_WriteFlashDataCommand_CommandNotAllowedResponse()
    => WriteInitialSettingsSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(WriteInitialSettingsAsync);

  private void WriteInitialSettingsSyncOrAsync_WriteFlashDataCommand_CommandNotAllowedResponse(
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      initialFlashMemory,
      stagedFlashMemory
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x03: Command not allowed
      // [2-63] Don't care
      "B1-03-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, default),
      Throws.TypeOf<FlashWriteAccessException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. initialFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. stagedFlashMemory.Password, // [12-19] PASS0-PASS8
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
  }

  // 1回目の Write で一度物理 Flash メモリへ変更を更新した後に再変更を行った際、
  // WriteInitialSettings を実行すると 1 回目の更新内容ではなくインスタンス生成時のセッション初期値が書き戻されることを検証する。
  /// <summary>
  /// Tests that calling <see cref="FlashSettings.WriteInitialSettings"/> after a prior <see cref="FlashSettings.Write"/>
  /// correctly restores and writes the initial settings captured at instance creation, rather than retaining the written state.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>A first call to <see cref="FlashSettings.Write"/> successfully persisted modified settings to the physical Flash.</description></item>
  ///   <item><description>Subsequent modifications are made to <see cref="FlashSettings"/>.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>The generated command packet contains the initial settings from controller creation time.</description></item>
  ///   <item><description>Local property values revert to the initial settings.</description></item>
  ///   <item><description><see cref="FlashSettings.IsDirty"/> becomes <see langword="false"/>.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void WriteInitialSettings_AfterPreviousWrite_WritesInitialSettings(
    [Values] bool enablePasswordProtected
  )
    => WriteInitialSettingsSyncOrAsync_AfterPreviousWrite_WritesInitialSettings(
      enablePasswordProtected,
      Write,
      WriteInitialSettings
    );

  /// <inheritdoc cref="WriteInitialSettings_AfterPreviousWrite_WritesInitialSettings">
  [Test]
  public void WriteInitialSettingsAsync_AfterPreviousWrite_WritesInitialSettings(
    [Values] bool enablePasswordProtected
  )
    => WriteInitialSettingsSyncOrAsync_AfterPreviousWrite_WritesInitialSettings(
      enablePasswordProtected,
      WriteAsync,
      WriteInitialSettingsAsync
    );

  private void WriteInitialSettingsSyncOrAsync_AfterPreviousWrite_WritesInitialSettings(
    bool enablePasswordProtected,
    Func<FlashSettings, CancellationToken, ValueTask> writeSyncOrAsync,
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();
    var hasPasswordProvided = enablePasswordProtected;
    var password = "password"u8;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_00 // CHIPPROT(1-0): 00(Unsecured)
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    /*
     * write #1
     */

    // set IsDirty to true
    mcp2221A
      .Flash
      .ModifyUsbVendorId((~mcp2221A.Flash.UsbVendorId) & 0xFFFF) // Chip Settings area
      .ModifyUsbProductId((~mcp2221A.Flash.UsbProductId) & 0xFFFF) // Chip Settings area
      .ModifyGpSetting(0, GpFunction.Gpio, PinMode.Output, !mcp2221A.Flash.GpPin0.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(1, GpFunction.Gpio, PinMode.Output, !mcp2221A.Flash.GpPin1.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(2, GpFunction.Gpio, PinMode.Input, !mcp2221A.Flash.GpPin2.GpioOutputValue) // GP Settings area
      .ModifyGpSetting(3, GpFunction.Gpio, PinMode.Input, !mcp2221A.Flash.GpPin3.GpioOutputValue) // GP Settings area
      .ModifyUsbManufacturerString("Vendor #1") // USB Manufacturer Descriptor String area
      .ModifyUsbProductString("Product #1") // USB Product Descriptor String area
      .ModifyUsbSerialNumberString("Serial Number #1"); // USB Serial Number Descriptor String area

    if (enablePasswordProtected) {
      mcp2221A
        .Flash
        .ModifyWriteProtection(DeviceConfigurationProtectionLevel.PasswordProtected) // Chip Settings area
        .ModifyPassword(password); // Chip Settings area
    }

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write Chip Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write GP Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Manufacturer Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Product Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // Write USB Serial Number Descriptor String
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    /*
     * write #2 (initial settings)
     */
    mcp2221A
      .Flash
      .ModifyUsbVendorId(0xFFFF) // Chip Settings area
      .ModifyUsbProductId(0xFFFF) // Chip Settings area
      .ModifyGpSetting(0, GpFunction.LedOutput, PinMode.Input) // GP Settings area
      .ModifyGpSetting(1, GpFunction.LedOutput, PinMode.Input) // GP Settings area
      .ModifyGpSetting(2, GpFunction.Dac, PinMode.Output) // GP Settings area
      .ModifyGpSetting(3, GpFunction.Dac, PinMode.Output) // GP Settings area
      .ModifyUsbManufacturerString("Vendor #2") // USB Manufacturer Descriptor String area
      .ModifyUsbProductString("Product #2") // USB Product Descriptor String area
      .ModifyUsbSerialNumberString("Serial Number #2"); // USB Serial Number Descriptor String area

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write Chip Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write GP Settings
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Manufacturer Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)), // Write USB Product Descriptor String
      "B1-00-" + string.Join("-", Enumerable.Repeat("00", 62)) // Write USB Serial Number Descriptor String
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "before write");

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, default),
      Throws.Nothing
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 0),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x00, // [1] Write Chip Settings
          .. initialFlashMemory.ChipSettings, // [2-11] CHIPSETTING0-USBREQCRT
          .. hasPasswordProvided // [12-19] PASS0-PASS8
            ? password
            : initialFlashMemory.Password,
          .. Enumerable.Repeat<byte>(0x00, 64 - 20) // [20-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 1),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x01, // [1] Write GP Settings
          .. initialFlashMemory.GpSettings, // [2-5] GP0-GP3 Power-Up Settings
          .. Enumerable.Repeat<byte>(0x00, 64 - 6) // [6-63]: don't care
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 2),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x02, // [1] Write USB Manufacturer Descriptor String
          (byte)(initialFlashMemory.UsbManufacturerDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbManufacturerDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 3),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x03, // [1] Write USB Product Descriptor String
          (byte)(initialFlashMemory.UsbProductDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbProductDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, commandNumber: 4),
      SequenceIs.EqualTo<byte>(
        [
          0xB1, // [0] WRITE FLASH DATA
          0x04, // [1] Write USB Serial Number Descriptor String
          (byte)(initialFlashMemory.UsbSerialNumberDescriptorStringLength + 2), // [2] Number of bytes + 2
          0x03, // [3] must always be 0x03
          .. initialFlashMemory.UsbSerialNumberDescriptorString, // [4-63] 16-bit Unicode chars
        ]
      )
    );
  }

  // 書き込み保護が PasswordProtected に設定されており、かつパスワードが未設定の状態で
  // WriteInitialSettings を呼び出した場合、InvalidOperationException がスローされコマンド送信が防止されることを検証する。
  /// <summary>
  /// Tests that <see cref="FlashSettings.WriteInitialSettings"/> throws an <see cref="InvalidOperationException"/>
  /// when write protection is set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/> but no password has been provided.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <b>Preconditions:</b>
  /// <list type="bullet">
  ///   <item><description>The initial device protection level is <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.</description></item>
  ///   <item><description>No password has been set via <see cref="FlashSettings.ModifyPassword"/>.</description></item>
  /// </list>
  /// </para>
  /// <para>
  /// <b>Expected Results:</b>
  /// <list type="bullet">
  ///   <item><description>An <see cref="InvalidOperationException"/> is thrown.</description></item>
  ///   <item><description>No write commands are transmitted to the transceiver.</description></item>
  /// </list>
  /// </para>
  /// </remarks>
  [Test]
  public void WriteInitialSettings_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException()
    => WriteInitialSettingsSyncOrAsync_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException(WriteInitialSettings);

  /// <inheritdoc cref="WriteInitialSettings_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException">
  [Test]
  public void WriteInitialSettingsAsync_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException()
    => WriteInitialSettingsSyncOrAsync_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException(WriteInitialSettingsAsync);

  private void WriteInitialSettingsSyncOrAsync_PasswordProtectionEnabledAndPasswordNotProvided_ThrowsException(
    Func<FlashSettings, CancellationToken, ValueTask> writeInitialSettingsSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: 0b_0_11111_01 // CHIPPROT(1-0): 01(Password-protected)
      )
    );

    // On an actual device, SendAccessPassword must be called here to unlock
    // password protection before performing a write operation. However, this
    // test case assumes that write protection has already been unlocked.
    // mcp2221A.Flash.SendAccessPassword(...);

    // password not provided
    // mcp2221A.Flash.ModifyPassword("password"u8);

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(DeviceConfigurationProtectionLevel.PasswordProtected)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before write");

    Assert.That(
      async () => await writeInitialSettingsSyncOrAsync(mcp2221A.Flash, default),
      Throws
        .InvalidOperationException
        .With
        .Message.Contains("no password has been provided")
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after write");
    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }
}
