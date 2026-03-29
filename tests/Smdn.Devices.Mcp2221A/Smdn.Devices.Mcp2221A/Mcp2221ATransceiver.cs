// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NUnit.Framework;

using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class Mcp2221ATransceiverTests {
  private const int LengthOfReportId = 1;

  [Test]
  public void CommandAsync_UnexpectedCommandEcho()
    => CommandSyncOrAsync_UnexpectedCommandEcho(
      static async mcp2221A => await mcp2221A.GpPin0.ReadAsync().ConfigureAwait(false)
    );

  [Test]
  public void Command_UnexpectedCommandEcho()
    => CommandSyncOrAsync_UnexpectedCommandEcho(
      static mcp2221A => new(mcp2221A.GpPin0.Read())
    );

  private void CommandSyncOrAsync_UnexpectedCommandEcho(
    Func<Mcp2221A, ValueTask<PinValue>> getGp0ReadAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation (GPIO0)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // Assume a scenario where 0xFF is returned instead of the
    // expected 0x51 for the 'Get GPIO Values' command code
    const byte ResponseCommandCode = 0xFF;

    Mcp2221ATests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.12 GET GPIO VALUES
      // [0] 0x51: Get GPIO Values command code
      // [1] 0x00: Command completed successfully
      // [2 + 2n] 0xEE: GP<n> is not set for GPIO operation
      // [3 + 2n] 0xEF: GP<n> is not set for GPIO operation
      // [10-63] Don't care
      $"{ResponseCommandCode:X2}-00-EE-FF-EE-FF-EE-FF-EE-FF-" + string.Join("-", Enumerable.Repeat((byte)0x00, 64 - 10))
    );

    Assert.That(
      async () => _ = await getGp0ReadAsyncFunc(mcp2221A),
      Throws
        .TypeOf<Mcp2221ACommandException>()
        .With
        .Property(nameof(Mcp2221ACommandException.Message))
        .Contains($"{ResponseCommandCode:X2}")
    );
  }

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(63)]
  public void CommandAsync_ResponseReportTooShort(int actualResponseLength)
    => CommandSyncOrAsync_ResponseReportTooShort(
      actualResponseLength: actualResponseLength,
      static async mcp2221A => await mcp2221A.GpPin0.ReadAsync().ConfigureAwait(false)
    );

  [TestCase(0)]
  [TestCase(1)]
  [TestCase(63)]
  public void Command_ResponseReportTooShort(int actualResponseLength)
    => CommandSyncOrAsync_ResponseReportTooShort(
      actualResponseLength: actualResponseLength,
      static mcp2221A => new(mcp2221A.GpPin0.Read())
    );

  private void CommandSyncOrAsync_ResponseReportTooShort(
    int actualResponseLength,
    Func<Mcp2221A, ValueTask<PinValue>> getGp0ReadAsyncFunc
  )
  {
    var loggerProvider = new FakeLoggerProvider();
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();

    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation (GPIO0)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings
      ),
      shouldDisposeUsbHidDevice: true,
      serviceProvider: serviceProvider
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    // [0] 0x51: Get GPIO Values command code
    // [1] 0x00: Command completed successfully
    // [2 + 2n] 0xEE: GP<n> is not set for GPIO operation
    // [3 + 2n] 0xEF: GP<n> is not set for GPIO operation
    // [10-63] Don't care
    var getGpioValuesResponseBytes =
      new byte[] { 0x51, 0x00, 0xEE, 0xEF, 0xEE, 0xEF, 0xEE, 0xEF, 0xEE, 0xEF }
      .Concat(Enumerable.Repeat((byte)0x00, 64 - 10))
      .ToArray();

    // Assume a scenario where a response of 64 bytes is expected,
    // but less than 64 bytes is returned.
    Mcp2221ATests.AppendPseudoResponse(
      mcp2221A,
      verifyCommandLength: false,
      BitConverter.ToString(getGpioValuesResponseBytes.Take(actualResponseLength).ToArray())
    );

    loggerProvider.Collector.Clear();

    var actualReportLength = actualResponseLength + LengthOfReportId;

    Assert.That(
      async () => _ = await getGp0ReadAsyncFunc(mcp2221A),
      Throws
        .TypeOf<Mcp2221ACommandException>()
        .With
        .Property(nameof(Mcp2221ACommandException.Message))
        .Contains($"{actualReportLength} bytes")
    );

    Assert.That(
      loggerProvider.Collector.Count,
      Is.EqualTo(2),
      "The sent command and received response should be logged."
    );
  }

  [Test]
  public void CommandAsync_ResponseReportTooShort_NoResponse()
    => CommandSyncOrAsync_ResponseReportTooShort_NoResponse(
      static async mcp2221A => await mcp2221A.GpPin0.ReadAsync().ConfigureAwait(false)
    );

  [Test]
  public void Command_ResponseReportTooShort_NoResponse()
    => CommandSyncOrAsync_ResponseReportTooShort_NoResponse(
      static mcp2221A => new(mcp2221A.GpPin0.Read())
    );

  private void CommandSyncOrAsync_ResponseReportTooShort_NoResponse(
    Func<Mcp2221A, ValueTask<PinValue>> getGp0ReadAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation (GPIO0)

    var loggerProvider = new FakeLoggerProvider();
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings
      ),
      shouldDisposeUsbHidDevice: true,
      serviceProvider: serviceProvider
    );

    // Assume a scenario where the report is not returned and
    // the read result from the endpoint is empty.
    // Mcp2221ATests.AppendResponse(...);
    const int ActualReportLength = 0;

    loggerProvider.Collector.Clear();

    Assert.That(
      async () => _ = await getGp0ReadAsyncFunc(mcp2221A),
      Throws
        .TypeOf<Mcp2221ACommandException>()
        .With
        .Property(nameof(Mcp2221ACommandException.Message))
        .Contains($"{ActualReportLength} bytes")
    );

    Assert.That(
      loggerProvider.Collector.Count,
      Is.EqualTo(2),
      "The sent command and received response should be logged."
    );
  }
}
