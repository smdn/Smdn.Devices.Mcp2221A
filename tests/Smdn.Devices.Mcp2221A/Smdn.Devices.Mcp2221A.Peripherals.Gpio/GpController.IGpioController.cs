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

  [TestCase(0b_000_1_0_010)] // LED_URX
  [TestCase(0b_000_1_0_001)] // SSPND
  public void SetMode_GPO_InvalidConfiguration(byte gp0Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp0Settings: gp0Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin0.SetMode(default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin0.SetModeAsync(default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_010)] // LED_URX
  [TestCase(0b_000_1_0_001)] // SSPND
  public void Write_GPO_InvalidConfiguration(byte gp0Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp0Settings: gp0Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin0.Write(true, default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin0.WriteAsync(true, default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_100)] // IOC
  [TestCase(0b_000_1_0_011)] // LED_UTX
  [TestCase(0b_000_1_0_010)] // ADC1
  [TestCase(0b_000_1_0_001)] // CLK OUT
  public void SetMode_GP1_InvalidConfiguration(byte gp1Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp1Settings: gp1Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin1.SetMode(default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin1.SetModeAsync(default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_100)] // IOC
  [TestCase(0b_000_1_0_011)] // LED_UTX
  [TestCase(0b_000_1_0_010)] // ADC1
  [TestCase(0b_000_1_0_001)] // CLK OUT
  public void Write_GP1_InvalidConfiguration(byte gp1Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp1Settings: gp1Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin1.Write(true, default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin1.WriteAsync(true, default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_011)] // DAC1
  [TestCase(0b_000_1_0_010)] // ADC2
  [TestCase(0b_000_1_0_001)] // USBCFG
  public void SetMode_GP2_InvalidConfiguration(byte gp2Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp2Settings: gp2Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin2.SetMode(default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin2.SetModeAsync(default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_011)] // DAC1
  [TestCase(0b_000_1_0_010)] // ADC2
  [TestCase(0b_000_1_0_001)] // USBCFG
  public void Write_GP2_InvalidConfiguration(byte gp2Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp2Settings: gp2Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin2.Write(true, default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin2.WriteAsync(true, default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_011)] // DAC2
  [TestCase(0b_000_1_0_010)] // ADC3
  [TestCase(0b_000_1_0_001)] // LED_I2C
  public void SetMode_GP3_InvalidConfiguration(byte gp3Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp3Settings: gp3Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin3.SetMode(default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin3.SetModeAsync(default),
      Throws.InvalidOperationException
    );
  }

  [TestCase(0b_000_1_0_011)] // DAC2
  [TestCase(0b_000_1_0_010)] // ADC3
  [TestCase(0b_000_1_0_001)] // LED_I2C
  public void Write_GP3_InvalidConfiguration(byte gp3Settings)
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(gp3Settings: gp3Settings),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => mcp2221A.GpPin3.Write(true, default),
      Throws.InvalidOperationException
    );
    Assert.That(
      async () => await mcp2221A.GpPin3.WriteAsync(true, default),
      Throws.InvalidOperationException
    );
  }
}
