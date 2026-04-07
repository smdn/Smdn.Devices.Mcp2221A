// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221ATests {
#pragma warning restore IDE0040
  [Test]
  public void ResetAsync()
    => ResetSyncOrAsync(
      resetAsyncFunc: static async mcp2221A => await mcp2221A.ResetAsync().ConfigureAwait(false)
    );

  [Test]
  public void Reset()
    => ResetSyncOrAsync(
      resetAsyncFunc: static mcp2221A => {
        mcp2221A.Reset();
        return default;
      }
    );

  private void ResetSyncOrAsync(
    Func<Mcp2221A, ValueTask> resetAsyncFunc
  )
  {
    var loggerProvider = new FakeLoggerProvider();
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true,
      serviceProvider: serviceProvider
    );

    loggerProvider.Collector.Clear();

    Assert.That(
      async () => await resetAsyncFunc(mcp2221A),
      Throws.Nothing
    );

    Assert.That(
      loggerProvider.Collector.Count,
      Is.EqualTo(1),
      "The sent command should be logged."
    );
    Assert.That(
      loggerProvider.Collector.LatestRecord.Message,
      Does.Contain("70-AB-CD-EF-"),
      "The sent 'RESET CHIP' command should be logged."
    );

    Assert.That(() => _ = mcp2221A.HidDevice, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = mcp2221A.GpPins, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = mcp2221A.I2cBus, Throws.TypeOf<ObjectDisposedException>());
    Assert.That(() => _ = mcp2221A.GpioController, Throws.TypeOf<ObjectDisposedException>());

    Assert.That(() => mcp2221A.Reset(), Throws.TypeOf<ObjectDisposedException>());
    Assert.That(async () => await mcp2221A.ResetAsync(), Throws.TypeOf<ObjectDisposedException>());

    Assert.That(() => mcp2221A.Dispose(), Throws.Nothing);
  }

  [Test]
  public void ResetAsync_CancellationRequestedBeforeSendCommand()
    => ResetSyncOrAsync_CancellationRequestedBeforeSendCommand(
      resetAsyncFunc: static async (mcp2221A, cancelledToken) => await mcp2221A.ResetAsync(cancelledToken).ConfigureAwait(false)
    );

  [Test]
  public void Reset_CancellationRequestedBeforeSendCommand()
    => ResetSyncOrAsync_CancellationRequestedBeforeSendCommand(
      resetAsyncFunc: static (mcp2221A, cancelledToken) => {
        mcp2221A.Reset(cancelledToken);
        return default;
      }
    );

  private void ResetSyncOrAsync_CancellationRequestedBeforeSendCommand(
    Func<Mcp2221A, CancellationToken, ValueTask> resetAsyncFunc
  )
  {
    var loggerProvider = new FakeLoggerProvider();
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([loggerProvider]));

    using var serviceProvider = services.BuildServiceProvider();
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true,
      serviceProvider: serviceProvider
    );

    loggerProvider.Collector.Clear();

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await resetAsyncFunc(mcp2221A, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      loggerProvider.Collector.Count,
      Is.Zero,
      "The sent command should not be logged."
    );

    Assert.That(() => _ = mcp2221A.HidDevice, Throws.Nothing);
    Assert.That(() => _ = mcp2221A.GpPins, Throws.Nothing);
    Assert.That(() => _ = mcp2221A.I2cBus, Throws.Nothing);
    Assert.That(() => _ = mcp2221A.GpioController, Throws.Nothing);

    Assert.That(() => mcp2221A.Dispose(), Throws.Nothing);
  }
}
