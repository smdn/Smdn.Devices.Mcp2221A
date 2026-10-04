// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  private static ValueTask SendAccessPassword(
    FlashSettings flash,
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken
  )
  {
    flash.SendAccessPassword(password.Span, cancellationToken);

    return default;
  }

  private static ValueTask SendAccessPasswordAsync(
    FlashSettings flash,
    ReadOnlyMemory<byte> password,
    CancellationToken cancellationToken
  )
    => flash.SendAccessPasswordAsync(password, cancellationToken);

  [TestCase("password")]
  [TestCase("01234567")]
  [TestCase("        ")]
  public void SendAccessPassword(string password)
    => SendAccessPasswordSyncOrAsync(password, SendAccessPassword);

  [TestCase("password")]
  [TestCase("01234567")]
  [TestCase("        ")]
  public void SendAccessPasswordAsync(string password)
    => SendAccessPasswordSyncOrAsync(password, SendAccessPasswordAsync);

  private void SendAccessPasswordSyncOrAsync(
    string password,
    Func<FlashSettings, ReadOnlyMemory<byte>, CancellationToken, ValueTask> sendAccessPasswordSyncOrAsync
  )
  {
    var passwordBytes = Encoding.UTF8.GetBytes(password);

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.4 SEND FLASH ACCESS PASSWORD
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "B2-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await sendAccessPasswordSyncOrAsync(
        mcp2221A.Flash,
        passwordBytes,
        CancellationToken.None
      ),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB2, // [0] SEND FLASH ACCESS PASSWORD
          0x00, // [1] Don't care
          .. passwordBytes, // [2-9] Password byte 1-8
          .. Enumerable.Repeat<byte>(0x00, 64 - 10) // [10-63] Don't care
        ]
      )
    );
  }

  [Test]
  public void SendAccessPassword_CommandNotAllowed()
    => SendAccessPasswordSyncOrAsync_CommandNotAllowed(SendAccessPassword);

  [Test]
  public void SendAccessPasswordAsync_CommandNotAllowed()
    => SendAccessPasswordSyncOrAsync_CommandNotAllowed(SendAccessPasswordAsync);

  private void SendAccessPasswordSyncOrAsync_CommandNotAllowed(
    Func<FlashSettings, ReadOnlyMemory<byte>, CancellationToken, ValueTask> sendAccessPasswordSyncOrAsync
  )
  {
    var password = "password"u8.ToArray();

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.4 SEND FLASH ACCESS PASSWORD
      // [1] 0x03: Command not allowed
      // [2-63] Don't care
      "B2-03-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await sendAccessPasswordSyncOrAsync(
        mcp2221A.Flash,
        password,
        CancellationToken.None
      ),
      Throws.TypeOf<FlashWriteAccessException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo<byte>(
        [
          0xB2, // [0] SEND FLASH ACCESS PASSWORD
          0x00, // [1] Don't care
          .. password, // [2-9] Password byte 1-8
          .. Enumerable.Repeat<byte>(0x00, 64 - 10) // [10-63] Don't care
        ]
      )
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_SendAccessPasswordSyncOrAsync_PasswordLengthNotValid()
  {
    yield return "";
    yield return "0123456";
    yield return "012345678";
  }

  [TestCaseSource(nameof(YieldTestCases_SendAccessPasswordSyncOrAsync_PasswordLengthNotValid))]
  public void SendAccessPassword_PasswordLengthNotValid(string password)
    => SendAccessPasswordSyncOrAsync_PasswordLengthNotValid(password, SendAccessPassword);

  [TestCaseSource(nameof(YieldTestCases_SendAccessPasswordSyncOrAsync_PasswordLengthNotValid))]
  public void SendAccessPasswordAsync_PasswordLengthNotValid(string password)
    => SendAccessPasswordSyncOrAsync_PasswordLengthNotValid(password, SendAccessPasswordAsync);

  private void SendAccessPasswordSyncOrAsync_PasswordLengthNotValid(
    string password,
    Func<FlashSettings, ReadOnlyMemory<byte>, CancellationToken, ValueTask> sendAccessPasswordSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    // command should not be sent
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await sendAccessPasswordSyncOrAsync(
        mcp2221A.Flash,
        Encoding.ASCII.GetBytes(password),
        CancellationToken.None
      ),
      Throws
        .ArgumentException
        .ParamName.EqualTo("password")
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void SendAccessPassword_CancellationRequested()
    => SendAccessPasswordSyncOrAsync_CancellationRequested(SendAccessPassword);

  [Test]
  public void SendAccessPasswordAsync_CancellationRequested()
    => SendAccessPasswordSyncOrAsync_CancellationRequested(SendAccessPasswordAsync);

  private void SendAccessPasswordSyncOrAsync_CancellationRequested(
    Func<FlashSettings, ReadOnlyMemory<byte>, CancellationToken, ValueTask> sendAccessPasswordSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await sendAccessPasswordSyncOrAsync(
        mcp2221A.Flash,
        "password"u8.ToArray(),
        cts.Token
      ),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void SendAccessPassword_ControllerDisposed()
    => SendAccessPasswordSyncOrAsync_ControllerDisposed(SendAccessPassword);

  [Test]
  public void SendAccessPasswordAsync_ControllerDisposed()
    => SendAccessPasswordSyncOrAsync_ControllerDisposed(SendAccessPasswordAsync);

  private void SendAccessPasswordSyncOrAsync_ControllerDisposed(
    Func<FlashSettings, ReadOnlyMemory<byte>, CancellationToken, ValueTask> sendAccessPasswordSyncOrAsync
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    var flash = mcp2221A.Flash;

    mcp2221A.Dispose();

    Assert.That(
      async () => await sendAccessPasswordSyncOrAsync(
        flash,
        "password"u8.ToArray(),
        CancellationToken.None
      ),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }
}
