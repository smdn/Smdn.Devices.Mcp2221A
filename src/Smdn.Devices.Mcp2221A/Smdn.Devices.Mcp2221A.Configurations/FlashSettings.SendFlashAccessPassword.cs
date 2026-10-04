// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  private static class SendFlashAccessPasswordCommand {
#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> password,
      None _
    )
#pragma warning restore SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    {
      // [MCP2221A] 3.1.4 SEND FLASH ACCESS PASSWORD
      comm[0] = 0xB2; // [0] Send Flash Access Password
      // comm[1] = 0x00; // [1] Don't care

      // [2-9] Password byte 1-8
      password
        .Slice(0, FlashMemory.LengthOfPassword)
        .CopyTo(comm.Slice(2, FlashMemory.LengthOfPassword));

      // [10-63] Don't care
    }

#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      Span<byte> _1,
      None _2
    )
#pragma warning restore SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    {
      // [MCP2221A] 3.1.4 SEND FLASH ACCESS PASSWORD
      // [1]: Send Flash Access Password command result
      switch (resp[1]) {
        case 0x00: // 0x00: Command completed successfully.
          return default;

        case 0x03: // 0x03: Command not allowed.
          throw new FlashWriteAccessException();

        default:
          Mcp2221ACommandException.ThrowNoSuccessfulResponse("SEND FLASH ACCESS PASSWORD", resp[1]);
          return default;
      }
    }
  }

  /// <summary>
  /// Sends the Flash access password to unlock Flash memory write operations.
  /// </summary>
  /// <param name="password">
  /// A <see cref="ReadOnlySpan{T}"/> of <see cref="byte"/> containing the
  /// 8-byte password required to unlock Flash memory write protection.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <exception cref="ArgumentException">
  /// Thrown when <paramref name="password"/> length is not exactly 8 bytes.
  /// </exception>
  /// <exception cref="FlashWriteAccessException">
  /// Thrown when the device rejects the command (e.g., returning a <c>0x03 Command not allowed</c>
  /// status when the maximum number of failed Flash updates has been reached).
  /// </exception>
  /// <exception cref="Mcp2221ACommandException">
  /// Thrown when an error occurs during communication with the MCP2221A device.
  /// </exception>
  /// <exception cref="OperationCanceledException">
  /// Thrown when the operation is canceled via <paramref name="cancellationToken"/>.
  /// </exception>
  /// <remarks>
  /// <para>
  /// This method sends the <c>Send Flash Access Password</c> command to
  /// the MCP2221A device. Executing this command with the correct 8-byte
  /// password unlocks write access to Flash memory settings.
  /// </para>
  /// </remarks>
  /// <seealso cref="Write"/>
  /// <seealso cref="SendAccessPasswordAsync"/>
  /// <seealso cref="FlashWriteAccessException"/>
  public void SendAccessPassword(
    ReadOnlySpan<byte> password,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPasswordLengthNotValid(password, nameof(password));

    cancellationToken.ThrowIfCancellationRequested();

    using (transceiver.EnterCommandTransaction(cancellationToken)) {
      _ = transceiver.Command(
        commandInput: password,
        responseOutput: default,
        arg: default(None),
        cancellationToken: cancellationToken,
        constructCommand: SendFlashAccessPasswordCommand.ConstructCommand,
        parseResponse: SendFlashAccessPasswordCommand.ParseResponse
      );
    }
  }

  /// <summary>
  /// Asynchronously sends the Flash access password to unlock Flash memory write operations.
  /// </summary>
  /// <param name="password">
  /// A <see cref="ReadOnlyMemory{T}"/> of <see cref="byte"/> containing the
  /// 8-byte password required to unlock Flash memory write protection.
  /// </param>
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <inheritdoc cref="SendAccessPassword(ReadOnlySpan{byte}, CancellationToken)" path="/exception|/remarks"/>
  /// <returns>
  /// A <see cref="ValueTask"/> representing the asynchronous operation.
  /// </returns>
  /// <seealso cref="WriteAsync"/>
  /// <seealso cref="SendAccessPassword"/>
  /// <seealso cref="FlashWriteAccessException"/>
  public async ValueTask SendAccessPasswordAsync(
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken = default
  )
  {
    ThrowIfPasswordLengthNotValid(password.Span, nameof(password));

    cancellationToken.ThrowIfCancellationRequested();

    using (await transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
      _ = await transceiver.CommandAsync(
        commandInput: password,
        responseOutput: default,
        arg: default(None),
        cancellationToken: cancellationToken,
        constructCommand: SendFlashAccessPasswordCommand.ConstructCommand,
        parseResponse: SendFlashAccessPasswordCommand.ParseResponse
      ).ConfigureAwait(false);
    }
  }
}
