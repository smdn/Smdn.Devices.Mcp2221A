// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Transport;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  private static class WriteFlashDataCommand {
    public static void ConstructWriteChipSettingsCommand(
      Span<byte> comm,
      IFlashMemory memory
    )
    {
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      comm[0] = 0xB1; // [0] Write Flash Data

      // [1]: Write Flash Data Sub Code (0x00: Write Chip Settings)
      comm[1] = 0x00;

      // [ 2]: CHIPSETTING0
      // [ 3]: CHIPSETTING1
      // [ 4]: CHIPSETTING2
      // [ 5]: CHIPSETTING3
      // [ 6]: USBVIDL
      // [ 7]: USBVIDH
      // [ 8]: USBPIDL
      // [ 9]: USBPIDH
      // [10]: USBPWRATTR
      // [11]: USBREQCRT
      memory.ChipSettings.CopyTo(comm.Slice(2, 10));

      // [12-19]: PASS0-PASS7 (8-byte password)
      memory.Password.CopyTo(comm.Slice(12, 8));

      // [20-63]: Don't care
    }

    public static void ConstructWriteGpSettingsCommand(
      Span<byte> comm,
      IFlashMemory memory
    )
    {
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      comm[0] = 0xB1; // [0] Write Flash Data

      // [1]: Write Flash Data Sub Code (0x01: Write GP Settings)
      comm[1] = 0x01;

      // [2]: GP0 Power-up Settings
      // [3]: GP1 Power-up Settings
      // [4]: GP2 Power-up Settings
      // [5]: GP3 Power-up Settings
      memory.GpSettings.CopyTo(comm.Slice(2, 4));
    }

    public static void ConstructWriteUsbManufacturerDescriptorStringCommand(
      Span<byte> comm,
      IFlashMemory memory
    )
      => ConstructWriteUsbDescriptorStringCommand(
        comm: comm,
        // [MCP2221A] 3.1.3 WRITE FLASH DATA
        // 0x02: Write USB Manufacturer Descriptor String
        subCode: 0x02,
        descriptorString: memory.StoredUsbManufacturerDescriptorStringSpan
      );

    public static void ConstructWriteUsbProductDescriptorStringCommand(
      Span<byte> comm,
      IFlashMemory memory
    )
      => ConstructWriteUsbDescriptorStringCommand(
        comm: comm,
        // [MCP2221A] 3.1.3 WRITE FLASH DATA
        // 0x03: Write USB Product Descriptor String
        subCode: 0x03,
        descriptorString: memory.StoredUsbProductDescriptorStringSpan
      );

    public static void ConstructWriteUsbSerialNumberDescriptorStringCommand(
      Span<byte> comm,
      IFlashMemory memory
    )
      => ConstructWriteUsbDescriptorStringCommand(
        comm: comm,
        // [MCP2221A] 3.1.3 WRITE FLASH DATA
        // 0x04: Write USB Serial Number Descriptor String
        subCode: 0x04,
        descriptorString: memory.StoredUsbSerialNumberDescriptorStringSpan
      );

    private static void ConstructWriteUsbDescriptorStringCommand(
      Span<byte> comm,
      byte subCode,
      ReadOnlySpan<byte> descriptorString
    )
    {
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      comm[0] = 0xB1; // [0] Write Flash Data

      // [1]: Write Flash Data Sub Code
      comm[1] = subCode;

      // [2]: Number of bytes + 2 in the provided USB Descriptor String.
      // The value at Byte Index 2 must be 2 + 2 x (number of Unicode characters in the string).
      comm[2] = (byte)(2 + descriptorString.Length);

      // [3]: The value at this index must always be 0x03.
      comm[3] = 0x03;

      // [4 + 2 x Unicode_char_number + 0]: Lower byte of the 16-bit Unicode character.
      // [4 + 2 x Unicode_char_number + 1]: Higher byte of the 16-bit Unicode character.
      descriptorString.CopyTo(comm.Slice(4, 60));
    }

#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static None ParseResponse(ReadOnlySpan<byte> resp, IFlashMemory _)
#pragma warning restore SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    {
      // [MCP2221A] 3.1.3 WRITE FLASH DATA
      // [1]: Write Flash Data command result
      switch (resp[1]) {
        case 0x00: // 0x00: Command completed successfully.
          return default;

        case 0x02: // 0x02: Command not supported.
          // This is the response returned when a command is issued with an
          // undefined subcommand. Therefore, this situation will not occur
          // unless there is an implementation bug.
          Mcp2221ACommandException.ThrowNoSuccessfulResponse("WRITE FLASH DATA", resp[1]);
          return default;

        case 0x03: // 0x03: Command not allowed.
          throw new FlashWriteAccessException();

        default:
          Mcp2221ACommandException.ThrowNoSuccessfulResponse("WRITE FLASH DATA", resp[1]);
          return default;
      }
    }
  }

  /// <summary>
  /// Writes the current staged settings and any pending password modifications
  /// to the Flash memory of the MCP2221A device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="FlashWriteAccessException">
  /// Thrown when the device rejects the write operation because Flash write
  /// protection remains active (e.g., when <see cref="SendAccessPassword"/> was
  /// omitted or supplied with an incorrect password) or write access is not
  /// permitted.
  /// </exception>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="WriteProtectionLevel"/> is set to
  /// <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// but <see cref="ModifyPassword"/> has not been called to specify a password.
  /// Call <see cref="ModifyPassword"/> to specify an 8-byte password before
  /// writing settings.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method issues the command to persist all staged configuration changes
  /// to the physical non-volatile Flash memory.
  /// If there are no staged configuration changes or pending password
  /// modifications (i.e., <see cref="IsDirty"/> is <see langword="false"/>),
  /// this method returns immediately without issuing any write commands to
  /// the device.
  /// </para>
  /// <para>
  /// If password-protected write protection is active on the device,
  /// <see cref="SendAccessPassword"/> or <see cref="SendAccessPasswordAsync"/>
  /// must be called prior to invoking this method to unlock write access.
  /// The actual write-protection status currently applied to the Flash memory
  /// can be checked via <see cref="Mcp2221AController.FlashWriteProtection"/>.
  /// Note that <see cref="WriteProtectionLevel"/> represents the staged setting
  /// to be written and may not reflect the actual active protection state of the
  /// Flash memory; therefore, it should not be used to determine whether
  /// <see cref="SendAccessPassword"/> needs to be called.
  /// Note also that <see cref="SendAccessPassword"/> returns successfully even if
  /// an incorrect password is provided; however, write protection remains active
  /// and will cause this method to fail.
  /// </para>
  /// <para>
  /// Attempting to write to Flash memory while write protection remains active
  /// will cause the device to reject the command (returning a
  /// <c>0x03 Command not allowed</c> status), resulting in a <see cref="FlashWriteAccessException"/>.
  /// </para>
  /// <para>
  /// Before issuing write commands, this method validates whether a password
  /// has been explicitly provided if <see cref="WriteProtectionLevel"/> is
  /// set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// (either as the initial state or via a staged change).
  /// Because existing passwords cannot be read back from the MCP2221A device,
  /// the initial internal password buffer contains indeterminate data.
  /// Writing settings while write protection is set to
  /// <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/> without
  /// explicitly setting a password via <see cref="ModifyPassword"/> would
  /// overwrite the device password with indeterminate data, risking an
  /// unintentional lockout. To prevent this, an <see cref="InvalidOperationException"/>
  /// is thrown if <see cref="ModifyPassword"/> has not been called.
  /// </para>
  /// <para>
  /// Depending on which registers correspond to the modified settings, writing
  /// staged changes may involve issuing one or more commands to the device.
  /// If the write operation fails before all commands are successfully executed -
  /// such as due to an error response from the device, an exception during
  /// communication, or cancellation via <paramref name="cancellationToken"/> -
  /// the current staged changes are retained and <see cref="IsDirty"/> remains
  /// <see langword="true"/>.
  /// </para>
  /// <para>
  /// Upon completion, the staged changes are marked as written, causing
  /// <see cref="IsDirty"/> to return <see langword="false"/> until subsequent
  /// modifications are made.
  /// Note that writing to Flash memory does not immediately alter the current
  /// SRAM operating parameters or active write-protection state; a device reset
  /// or power cycle is required to load the updated Flash settings into SRAM
  /// and enforce any newly written protection levels.
  /// </para>
  /// </remarks>
  /// <seealso cref="Mcp2221AController.FlashWriteProtection"/>
  /// <seealso cref="WriteProtectionLevel"/>
  /// <seealso cref="ModifyPassword"/>
  /// <seealso cref="WriteAsync"/>
  /// <seealso cref="SendAccessPassword"/>
  /// <seealso cref="FlashWriteAccessException"/>
  /// <seealso cref="Restore"/>
  /// <seealso cref="IsDirty"/>
  public void Write(CancellationToken cancellationToken = default)
  {
    if (!ShouldWriteFlashData(out var settingsToWrite))
      return;

    EnsureWritePreconditions(cancellationToken);

    WriteCore(
      settingsToWrite:
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
        settingsToWrite,
#else
        settingsToWrite!,
#endif
      writeForcibly: false,
      cancellationToken: cancellationToken
    );
  }

  private void WriteCore(
    IFlashMemory settingsToWrite,
    bool writeForcibly,
    CancellationToken cancellationToken
  )
  {
    foreach (var constructCommand in IterateWriteFlashDataCommands(settingsToWrite, writeForcibly: writeForcibly)) {
      using (transceiver.EnterCommandTransaction(cancellationToken)) {
        _ = transceiver.Command(
          arg: settingsToWrite,
          cancellationToken: cancellationToken,
          constructCommand: constructCommand,
          parseResponse: WriteFlashDataCommand.ParseResponse
        );
      }
    }

    hasWritten = true;
    hasPasswordModified = false;
  }

  /// <summary>
  /// Asynchronously writes the current staged settings and any pending password
  /// modifications to the Flash memory of the MCP2221A device.
  /// </summary>
  /// <inheritdoc cref="Write(CancellationToken)" path="/param|/exception|/remarks|/seealso"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous write operation.
  /// </returns>
  public ValueTask WriteAsync(CancellationToken cancellationToken = default)
  {
    if (!ShouldWriteFlashData(out var settingsToWrite))
      return default;

    EnsureWritePreconditions(cancellationToken);

    return WriteAsyncCore(
      settingsToWrite:
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
        settingsToWrite,
#else
        settingsToWrite!,
#endif
      writeForcibly: false,
      cancellationToken: cancellationToken
    );
  }

  private async ValueTask WriteAsyncCore(
    IFlashMemory settingsToWrite,
    bool writeForcibly,
    CancellationToken cancellationToken
  )
  {
    foreach (var constructCommand in IterateWriteFlashDataCommands(settingsToWrite, writeForcibly: writeForcibly)) {
      using (await transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
        _ = await transceiver.CommandAsync(
          arg: settingsToWrite,
          cancellationToken: cancellationToken,
          constructCommand: constructCommand,
          parseResponse: WriteFlashDataCommand.ParseResponse
        ).ConfigureAwait(false);
      }
    }

    hasWritten = true;
    hasPasswordModified = false;
  }

  private bool ShouldWriteFlashData(
#if NULL_STATE_STATIC_ANALYSIS_ATTRIBUTES
    [NotNullWhen(true)]
#endif
    out IFlashMemory? settingsToWrite
  )
  {
    settingsToWrite = null;

    if (!IsDirty)
      return false; // nothing to write
    if (stagedSettings is null)
      return false; // nothing to write; check to prevent null references

    settingsToWrite = stagedSettings;

    return true;
  }

  private void EnsureWritePreconditions(
    CancellationToken cancellationToken
  )
  {
    transceiver.ThrowIfDisposed();

    cancellationToken.ThrowIfCancellationRequested();

    ThrowIfPasswordProtectionIsEnabledButPasswordNotProvided();
  }

  /// <summary>
  /// Reverts all staged changes to the initial state and forcibly writes
  /// those initial settings to the Flash memory of the MCP2221A device.
  /// </summary>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="FlashWriteAccessException">
  /// Thrown when the device rejects the write operation because Flash write
  /// protection is active or write access is not permitted.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method performs an operation equivalent to calling <see cref="Restore"/>
  /// followed by a write operation. It discards any currently staged modifications,
  /// reverts the internal buffer to the initial state captured when the
  /// <see cref="Mcp2221AController"/> instance was created, and writes those
  /// initial settings to the physical Flash memory.
  /// </para>
  /// <para>
  /// Unlike <see cref="Write"/>, this method bypasses the <see cref="IsDirty"/>
  /// check and always issues write commands to the device, even if
  /// <see cref="IsDirty"/> is <see langword="false"/>.
  /// </para>
  /// <para>
  /// Upon completion, all staged changes are reset to the initial state, causing
  /// <see cref="IsDirty"/> to return <see langword="false"/> until subsequent
  /// modifications are made.
  /// Note that writing to Flash memory does not immediately alter the current
  /// SRAM operating parameters or active write-protection state; a device reset
  /// or power cycle is required to load the updated Flash settings into SRAM
  /// and enforce any newly written protection levels.
  /// </para>
  /// </remarks>
  /// <seealso cref="Write"/>
  /// <seealso cref="WriteAsync"/>
  /// <seealso cref="Restore"/>
  /// <seealso cref="IsDirty"/>
  public void WriteInitialSettings(
    CancellationToken cancellationToken = default
  )
  {
    EnsureWritePreconditions(cancellationToken);

    // By ensuring that `stagedSettings` has been created, guarantees that
    // the contents of `initialSettings` are copied to `stagedSettings` by
    // the `Restore` method, and that if a password is provided by
    // the `ModifyPassword` method, that password is stored in `stagedSettings`.
    var settingsToWrite = EnsureStagedSettingsCreated();

    Restore();

    WriteCore(
      settingsToWrite: settingsToWrite,
      writeForcibly: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  /// Asynchronously reverts all staged changes to the initial state and
  /// forcibly writes those initial settings to the Flash memory of the
  /// MCP2221A device.
  /// </summary>
  /// <inheritdoc cref="WriteInitialSettings(CancellationToken)" path="/param|/exception|/remarks|/seealso"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous write operation.
  /// </returns>
  public ValueTask WriteInitialSettingsAsync(CancellationToken cancellationToken = default)
  {
    EnsureWritePreconditions(cancellationToken);

    // By ensuring that `stagedSettings` has been created, guarantees that
    // the contents of `initialSettings` are copied to `stagedSettings` by
    // the `Restore` method, and that if a password is provided by
    // the `ModifyPassword` method, that password is stored in `stagedSettings`.
    var settingsToWrite = EnsureStagedSettingsCreated();

    Restore();

    return WriteAsyncCore(
      settingsToWrite: settingsToWrite,
      writeForcibly: true,
      cancellationToken: cancellationToken
    );
  }

  /// <summary>
  /// Validates that a password has been explicitly provided if Flash write
  /// protection is set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Since the existing password cannot be read back from the MCP2221A device,
  /// the initial password buffer (<see cref="IFlashMemory.Password"/>) contains
  /// indeterminate data (1.4.2 CHIP SETTINGS MAP and 3.1.2 Read Flash Data).
  /// </para>
  /// <para>
  /// Writing settings while protection is set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// without calling <see cref="ModifyPassword"/> would overwrite the device
  /// password with this indeterminate data, potentially locking the user out of
  /// Flash write access permanently (3.1.3 Write Flash Data).
  /// </para>
  /// </remarks>
  /// <exception cref="InvalidOperationException">
  /// Thrown when <see cref="WriteProtectionLevel"/> is <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// and <see cref="ModifyPassword"/> has not been called.
  /// </exception>
  private void ThrowIfPasswordProtectionIsEnabledButPasswordNotProvided()
  {
    // Check the currently staged (or initial if unmodified) CHIPPROT value.
    // If it is set to PasswordProtected (0b01), check whether the
    // password has been provided.
    // Throw an exception if the password has not been set.
    if (
      WriteProtectionLevel == DeviceConfigurationProtectionLevel.PasswordProtected &&
      !hasPasswordProvided
    ) {
      const string ExceptionMessage =
        $"Flash memory write protection is set to {nameof(DeviceConfigurationProtectionLevel.PasswordProtected)}, " +
        "but no password has been provided. " +
        $"Call {nameof(ModifyPassword)} to specify the 8-byte password before writing settings.";

      throw new InvalidOperationException(message: ExceptionMessage);
    }
  }

  private
  IEnumerable<Mcp2221AConstructCommandAction<IFlashMemory>>
  IterateWriteFlashDataCommands(
    IFlashMemory settingsToWrite,
    bool writeForcibly
  )
  {
    if (stagedSettings is null)
      throw new InvalidOperationException("no staged settings; nothing to write");

    // Write Flash Data - 0x00 Write Chip Settings
    if (
      writeForcibly ||
      hasPasswordModified ||
      !settingsToWrite.ChipSettings.SequenceEqual(initialSettings.ChipSettings)
    ) {
      yield return WriteFlashDataCommand.ConstructWriteChipSettingsCommand;
    }

    // Write Flash Data - 0x01 Write GP Settings
    if (
      writeForcibly ||
      !settingsToWrite.GpSettings.SequenceEqual(initialSettings.GpSettings)
    ) {
      yield return WriteFlashDataCommand.ConstructWriteGpSettingsCommand;
    }

    // Write Flash Data - 0x02 Write USB Manufacturer Descriptor String
    if (
      writeForcibly ||
      !settingsToWrite.StoredUsbManufacturerDescriptorStringSpan.SequenceEqual(
        initialSettings.StoredUsbManufacturerDescriptorStringSpan
      )
    ) {
      yield return WriteFlashDataCommand.ConstructWriteUsbManufacturerDescriptorStringCommand;
    }

    // Write Flash Data - 0x03 Write USB Product Descriptor String
    if (
      writeForcibly ||
      !settingsToWrite.StoredUsbProductDescriptorStringSpan.SequenceEqual(
        initialSettings.StoredUsbProductDescriptorStringSpan
      )
    ) {
      yield return WriteFlashDataCommand.ConstructWriteUsbProductDescriptorStringCommand;
    }

    // Write Flash Data - 0x04 Write USB Serial Number Descriptor String
    if (
      writeForcibly ||
      !settingsToWrite.StoredUsbSerialNumberDescriptorStringSpan.SequenceEqual(
        initialSettings.StoredUsbSerialNumberDescriptorStringSpan
      )
    ) {
      yield return WriteFlashDataCommand.ConstructWriteUsbSerialNumberDescriptorStringCommand;
    }
  }
}
