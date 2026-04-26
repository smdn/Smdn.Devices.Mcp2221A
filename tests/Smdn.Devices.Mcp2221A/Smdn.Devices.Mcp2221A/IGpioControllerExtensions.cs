// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class IGpioControllerExtensionsTests {
  [Test]
  public void ConfigureAsGpioOutputAsync_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioOutputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioOutput_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioOutput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioInputAsync_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioInputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioInput_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioInput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsGpioOutputSyncOrAsync()
  {
    for (var gpIndex = 0; gpIndex < 4; gpIndex++) {
      foreach (var initialValue in new PinValue?[] { PinValue.High, PinValue.Low, null }) {
        yield return new object?[] { gpIndex, initialValue };
      }
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsGpioOutputSyncOrAsync))]
  public void ConfigureAsGpioOutputAsync(int gpIndex, PinValue? initialValue)
    => ConfigureAsGpioOutputSyncOrAsync(
      gpIndex,
      initialValue,
      static async (gpioController, val) => await gpioController.ConfigureAsGpioOutputAsync(
        initialValue: val
      ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsGpioOutputSyncOrAsync))]
  public void ConfigureAsGpioOutput(int gpIndex, PinValue? initialValue)
    => ConfigureAsGpioOutputSyncOrAsync(
      gpIndex,
      initialValue,
      static (gpioController, val) => {
        gpioController.ConfigureAsGpioOutput(initialValue: val);
        return default;
      }
    );

  private void ConfigureAsGpioOutputSyncOrAsync(
    int gpIndex,
    PinValue? initialValue,
    Func<IGpioController, PinValue?, ValueTask> configureAsGpioOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_0_1_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_1_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings3 = 0b_0_1_1_00_0_00; // ADC: VDD

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings3: InitialChipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    var gp = mcp2221A.GpPins[gpIndex];
    var initialOutputValue = ((currentGpSettings[gp.Index] & 0b_000_1_0_000) == 0)
      ? PinValue.Low
      : PinValue.High;

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Assert.That(
      async () => await configureAsGpioOutputAsyncFunc(gp, initialValue),
      Throws.Nothing
    );

    Assert.That(gp.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(gp.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(gp.LastUpdatedValue, Is.EqualTo(initialValue ?? initialOutputValue));
  }

  [Test]
  public void ConfigureAsGpioInputAsync(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioInputSyncOrAsync(
      gpIndex,
      static async gpioController => await gpioController.ConfigureAsGpioInputAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsGpioInput(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioInputSyncOrAsync(
      gpIndex,
      static gpioController => {
        gpioController.ConfigureAsGpioInput();
        return default;
      }
    );

  private void ConfigureAsGpioInputSyncOrAsync(
    int gpIndex,
    Func<IGpioController, ValueTask> configureAsGpioInputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_0_1_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_1_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings3 = 0b_0_1_1_00_0_00; // ADC: VDD

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings3: InitialChipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    var gp = mcp2221A.GpPins[gpIndex];
    var initialOutputValue = ((currentGpSettings[gp.Index] & 0b_000_1_0_000) == 0)
      ? PinValue.Low
      : PinValue.High;

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Assert.That(
      async () => await configureAsGpioInputAsyncFunc(gp),
      Throws.Nothing
    );

    Assert.That(gp.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(gp.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(gp.LastUpdatedValue, Is.EqualTo(initialOutputValue), "GPIO output value must not be configured");
  }

  [Test]
  public void ConfigureAsGpioOutputAsync_CancellationRequested(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioOutputOrInputSyncOrAsync_CancellationRequested(
      gpIndex,
      static async (gpioController, ct) => await gpioController.ConfigureAsGpioOutputAsync(
        initialValue: PinValue.High,
        cancellationToken: ct
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsGpioOutput_CancellationRequested(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioOutputOrInputSyncOrAsync_CancellationRequested(
      gpIndex,
      static (gpioController, ct) => {
        gpioController.ConfigureAsGpioOutput(
          initialValue: PinValue.High,
          cancellationToken: ct
        );
        return default;
      }
    );

  [Test]
  public void ConfigureAsGpioInputAsync_CancellationRequested(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioOutputOrInputSyncOrAsync_CancellationRequested(
      gpIndex,
      static async (gpioController, ct) => await gpioController.ConfigureAsGpioInputAsync(
        cancellationToken: ct
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsGpioInput_CancellationRequested(
    [Values(0, 1, 2, 3)] int gpIndex
  )
    => ConfigureAsGpioOutputOrInputSyncOrAsync_CancellationRequested(
      gpIndex,
      static (gpioController, ct) => {
        gpioController.ConfigureAsGpioInput(
          cancellationToken: ct
        );
        return default;
      }
    );

  private void ConfigureAsGpioOutputOrInputSyncOrAsync_CancellationRequested(
    int gpIndex,
    Func<IGpioController, CancellationToken, ValueTask> configureAsGpioOutputOrInputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_0_1_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_1_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var gp = mcp2221A.GpPins[gpIndex];

    var initialFunction = gp.CurrentFunction;

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAsGpioOutputOrInputAsyncFunc(gp, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token),
      $"cancellation requested ({gp.PinName})"
    );

    Assert.That(gp.CurrentFunction, Is.EqualTo(initialFunction));
  }
}
