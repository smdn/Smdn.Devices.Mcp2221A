// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class Mcp2221ATransceiverTests {
  [Test]
  public void CommandAsync_UnexpectedCommandEcho()
    => CommandSyncOrAsync_UnexpectedCommandEcho(
      static async mcp2221A => await mcp2221A.GP0.GetValueAsync().ConfigureAwait(false)
    );

  [Test]
  public void Command_UnexpectedCommandEcho()
    => CommandSyncOrAsync_UnexpectedCommandEcho(
      static mcp2221A => new(mcp2221A.GP0.GetValue())
    );

  private void CommandSyncOrAsync_UnexpectedCommandEcho(
    Func<Mcp2221A, ValueTask<PinValue>> getGp0PinValueAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var endPoint = (mcp2221A.HidDevice as PseudoUsbHidDevice)!.EndPoint;

    // Assume a scenario where 0xFF is returned instead of the
    // expected 0x51 for the 'Get GPIO Values' command code
    const byte ResponseCommandCode = 0xFF;

    Mcp2221ATests.AppendResponse(
      endPoint,
      // [MCP2221A] 3.1.12 GET GPIO VALUES
      // [0] 0x51: Get GPIO Values command code
      // [1] 0x00: Command completed successfully
      // [2 + 2n] 0xEE: GP<n> is not set for GPIO operation
      // [3 + 2n] 0xEF: GP<n> is not set for GPIO operation
      // [10-63] Don't care
      $"{ResponseCommandCode:X2}-00-EE-FF-EE-FF-EE-FF-EE-FF-" + string.Join("-", Enumerable.Repeat((byte)0x00, 64 - 10))
    );

    Assert.That(
      async () => _ = await getGp0PinValueAsyncFunc(mcp2221A),
      Throws
        .TypeOf<Mcp2221ACommandException>()
        .With
        .Property(nameof(Mcp2221ACommandException.Message))
        .Contains($"{ResponseCommandCode:X2}")
    );
  }
}
