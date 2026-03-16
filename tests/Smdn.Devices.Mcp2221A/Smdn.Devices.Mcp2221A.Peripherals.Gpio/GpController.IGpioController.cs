// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpControllerTests {
#pragma warning restore IDE0040
  [TestCase(PinMode.InputPullUp)]
  [TestCase(PinMode.InputPullDown)]
  [TestCase(-1)]
  public void ConfigureAsGpioAsync_UnsupportedPinMode(PinMode mode)
    => ConfigureAsGpioSyncOrAsync_UnsupportedPinMode(
      mode,
      static async (gp, m) => await gp.ConfigureAsGpioAsync(mode: m).ConfigureAwait(false)
    );

  [TestCase(PinMode.InputPullUp)]
  [TestCase(PinMode.InputPullDown)]
  [TestCase(-1)]
  public void ConfigureAsGpio_UnsupportedPinMode(PinMode mode)
    => ConfigureAsGpioSyncOrAsync_UnsupportedPinMode(
      mode,
      static (gp, m) => {
        gp.ConfigureAsGpio(mode: m);
        return default;
      }
    );

  private void ConfigureAsGpioSyncOrAsync_UnsupportedPinMode(
    PinMode mode,
    Func<GpController, PinMode, ValueTask> configureAsGpioAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, mode),
        Throws.TypeOf<NotSupportedException>(),
        $"unsupported pin mode ({gp.PinName}, {mode})"
      );
    }
  }

  [Test]
  public void ConfigureAsGpioAsync_CancellationRequested()
    => ConfigureAsGpioSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await gp.ConfigureAsGpioAsync(cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsGpio_CancellationRequested()
    => ConfigureAsGpioSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        gp.ConfigureAsGpio(cancellationToken: ct);
        return default;
      }
    );

  private void ConfigureAsGpioSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> configureAsGpioAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, cts.Token),
        Throws
          .TypeOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  [Test]
  public void ConfigureAsGpioAsync_Disposed()
    => ConfigureAsGpioSyncOrAsync_Disposed(
      static async gp => await gp.ConfigureAsGpioAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsGpio_Disposed()
    => ConfigureAsGpioSyncOrAsync_Disposed(
      static gp => {
        gp.ConfigureAsGpio();
        return default;
      }
    );

  private void ConfigureAsGpioSyncOrAsync_Disposed(
    Func<GpController, ValueTask> configureAsGpioAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var gpPins = mcp2221A.GpPins;

    mcp2221A.Dispose();

    foreach (var gp in gpPins) {
      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }
}
