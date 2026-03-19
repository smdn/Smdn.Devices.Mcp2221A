// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpControllerTests {
#pragma warning restore IDE0040
  [TestCase(PinMode.Input, true)]
  [TestCase(PinMode.Input, false)]
  [TestCase(PinMode.Output, true)]
  [TestCase(PinMode.Output, false)]
  public void ConfigureAsGpioAsync(PinMode mode, bool initialValue)
    => ConfigureAsGpioSyncOrAsync(
      mode,
      (PinValue)initialValue,
      static async (gp, m, val) => await gp.ConfigureAsGpioAsync(mode: m, initialValue: val).ConfigureAwait(false)
    );

  [TestCase(PinMode.Input, true)]
  [TestCase(PinMode.Input, false)]
  [TestCase(PinMode.Output, true)]
  [TestCase(PinMode.Output, false)]
  public void ConfigureAsGpio(PinMode mode, bool initialValue)
    => ConfigureAsGpioSyncOrAsync(
      mode,
      (PinValue)initialValue,
      static (gp, m, val) => {
        gp.ConfigureAsGpio(mode: m, initialValue: val);
        return default;
      }
    );

  private void ConfigureAsGpioSyncOrAsync(
    PinMode mode,
    PinValue initialValue,
    Func<GpController, PinMode, PinValue, ValueTask> configureAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );
    var expectedAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    foreach (var gp in mcp2221A.GpPins) {
      var expectedOutputValueBits = (bool)initialValue switch {
        true => (mode == PinMode.Output) ? 0b_000_1_0_000 : 0b_000_0_0_000,
        false => 0b_000_0_0_000,
      };
      var expectedDirectionBits = mode switch {
        PinMode.Input => 0b_000_0_1_000,
        PinMode.Output => 0b_000_0_0_000,
        _ => throw new InvalidOperationException(),
      };
      const byte ExpectedDesignationBits = 0b_000_0_0_000; // GPIO operation

      currentGpSettings[gp.Index] = (byte)(expectedOutputValueBits | expectedDirectionBits | ExpectedDesignationBits);

      Mcp2221ATests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
        // [1] 0x00: Command completed successfully
        // [22] GP0 Settings
        // [23] GP1 Settings
        // [24] GP2 Settings
        // [25] GP3 Settings
        "60-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-" +
        $"{currentGpSettings[0]:X2}-{currentGpSettings[1]:X2}-{currentGpSettings[2]:X2}-{currentGpSettings[3]:X2}-" +
        "00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00"
      );

      expectedAssignments[gp.Index] = GpFunction.Gpio;

      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, mode, initialValue),
        Throws.Nothing
      );

      Assert.That(gp.CurrentFunction, Is.EqualTo(GpFunction.Gpio));

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(expectedAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
      );
    }
  }

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
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: 0b_000_1_0_010, // Alternate Function 0 (LED UART RX)
        gp1Settings: 0b_000_1_0_011, // Alternate Function 1 (LED UART TX)
        gp2Settings: 0b_000_1_0_001, // Dedicated function operation (USBCFG)
        gp3Settings: 0b_000_1_0_001 // Dedicated function operation (LED I2C)
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, mode),
        Throws.TypeOf<NotSupportedException>(),
        $"unsupported pin mode ({gp.PinName}, {mode})"
      );

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(initialAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
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
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: 0b_000_1_0_010, // Alternate Function 0 (LED UART RX)
        gp1Settings: 0b_000_1_0_011, // Alternate Function 1 (LED UART TX)
        gp2Settings: 0b_000_1_0_001, // Dedicated function operation (USBCFG)
        gp3Settings: 0b_000_1_0_001 // Dedicated function operation (LED I2C)
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      var initialFunction = gp.CurrentFunction;

      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, cts.Token),
        Throws
          .TypeOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(initialAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
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
