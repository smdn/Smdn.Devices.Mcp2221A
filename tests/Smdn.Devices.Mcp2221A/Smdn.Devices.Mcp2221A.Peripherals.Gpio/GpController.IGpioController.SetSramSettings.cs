// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.IO.UsbHid;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );
      Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

      expectedAssignments[gp.Index] = GpFunction.Gpio;

      var expectedOutputValueBits = (bool)initialValue switch {
        true => 0b_000_1_0_000,
        false => 0b_000_0_0_000,
      };
      var expectedDirectionBits = mode switch {
        PinMode.Input => 0b_000_0_1_000,
        PinMode.Output => 0b_000_0_0_000,
        _ => throw new InvalidOperationException(),
      };
      const byte ExpectedDesignationBits = 0b_000_0_0_000; // GPIO operation

      currentGpSettings[gp.Index] = (byte)(expectedOutputValueBits | expectedDirectionBits | ExpectedDesignationBits);

      var expectedSentCommand = new byte[64];

      expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
      // [1-6] don't care
      expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
      expectedSentCommand[8] = currentGpSettings[0]; // [8] GP0 settings
      expectedSentCommand[9] = currentGpSettings[1]; // [9] GP1 settings
      expectedSentCommand[10] = currentGpSettings[2]; // [10] GP2 settings
      expectedSentCommand[11] = currentGpSettings[3]; // [11] GP3 settings

      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, mode, initialValue),
        Throws.Nothing
      );
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A),
        SequenceIs.EqualTo(expectedSentCommand)
      );

      Assert.That(gp.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
      Assert.That(gp.CurrentMode, Is.EqualTo(mode));
      Assert.That(gp.LastUpdatedValue, Is.EqualTo(initialValue));

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(expectedAssignments).AsCollection,
        $"other GP pins must not be configured (except {gp.PinName})"
      );
    }
  }

  [TestCase(PinMode.Input, true)]
  [TestCase(PinMode.Input, false)]
  [TestCase(PinMode.Output, true)]
  [TestCase(PinMode.Output, false)]
  public void ConfigureAsGpioAsync_ThrowsWhenUsedByGpioController(PinMode mode, bool initialValue)
    => ConfigureAsGpioSyncOrAsync_ThrowsWhenUsedByGpioController(
      mode,
      (PinValue)initialValue,
      static async (gp, m, val) => await gp.ConfigureAsGpioAsync(mode: m, initialValue: val).ConfigureAwait(false)
    );

  [TestCase(PinMode.Input, true)]
  [TestCase(PinMode.Input, false)]
  [TestCase(PinMode.Output, true)]
  [TestCase(PinMode.Output, false)]
  public void ConfigureAsGpio_ThrowsWhenUsedByGpioController(PinMode mode, bool initialValue)
    => ConfigureAsGpioSyncOrAsync_ThrowsWhenUsedByGpioController(
      mode,
      (PinValue)initialValue,
      static (gp, m, val) => {
        gp.ConfigureAsGpio(mode: m, initialValue: val);
        return default;
      }
    );

  private void ConfigureAsGpioSyncOrAsync_ThrowsWhenUsedByGpioController(
    PinMode mode,
    PinValue initialValue,
    Func<GpController, PinMode, PinValue, ValueTask> configureAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    for (var gp = 0; gp < 4; gp++) {
      currentGpSettings[gp] = (byte)(currentGpSettings[gp] & 0b_111_1_1_000);

      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );

      Assert.That(
        () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
          _ =
#endif
          mcp2221A.GpioController.OpenPin(gp),
        Throws.Nothing
      );
      Assert.That(mcp2221A.GpPins[gp].IsUsedByGpioController, Is.True);
    }

    foreach (var gp in mcp2221A.GpPins) {
      // command should not be sent
      // Mcp2221AControllerTests.AppendPseudoResponse(...);
      Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

      Assert.That(
        async () => await configureAsGpioAsyncFunc(gp, mode, initialValue),
        Throws
          .InvalidOperationException
          .With
          .Property(nameof(InvalidOperationException.Message))
          .Contains($"GP{gp.Index}")
          .And
          .Property(nameof(InvalidOperationException.Message))
          .Contains(nameof(GpioController))
      );

      Assert.That(
        Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
        Is.Zero,
        "command should not be sent"
      );
    }
  }

  private static IEnumerable<PinMode> YieldTestCases_UnsupportedPinMode()
  {
    yield return PinMode.InputPullUp;
    yield return PinMode.InputPullDown;
    yield return (PinMode)(-1);
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
  public void ConfigureAsGpioAsync_UnsupportedPinMode(PinMode mode)
    => ConfigureAsGpioSyncOrAsync_UnsupportedPinMode(
      mode,
      static async (gp, m) => await gp.ConfigureAsGpioAsync(mode: m).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
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
