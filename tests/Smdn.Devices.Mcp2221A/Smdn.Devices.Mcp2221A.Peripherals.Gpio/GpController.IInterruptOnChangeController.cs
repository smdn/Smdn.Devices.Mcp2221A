// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpControllerTests {
#pragma warning restore IDE0040
  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsInterruptOnChangeSyncOrAsync()
  {
    const byte InitialChipSetting3_DetectBothEdge = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)
    const byte InitialChipSetting3_DetectFallingEdge = 0b_0_1_0_11_0_00; // INTDETFEEN: 1, INTDETREEN: 0, ADCVRM: 11(4.096V), ADCREF: 0(VDD)
    const byte InitialChipSetting3_DetectRisingEdge = 0b_0_0_1_10_0_00; // INTDETFEEN: 0, INTDETREEN: 1, ADCVRM: 10(2.048V), ADCREF: 0(VDD)
    const byte InitialChipSetting3_DetectNone = 0b_0_0_0_00_1_00; // INTDETFEEN: 0, INTDETREEN: 0, ADCVRM: 00(Off), ADCREF: 1(VRM)

    const bool InterruptEdgeDetectorState_Detected = true;
    const bool InterruptEdgeDetectorState_NotDetected = false;

    foreach (var detectionTrigger in new InterruptOnChangeTrigger?[] {
      InterruptOnChangeTrigger.Both,
      InterruptOnChangeTrigger.Falling,
      InterruptOnChangeTrigger.Rising,
      InterruptOnChangeTrigger.None,
      null,
    }) {
      foreach (var clearDetectionFlag in new[] { true, false }) {
        yield return new object?[] { InitialChipSetting3_DetectBothEdge, InterruptEdgeDetectorState_Detected, detectionTrigger, clearDetectionFlag };
        yield return new object?[] { InitialChipSetting3_DetectFallingEdge, InterruptEdgeDetectorState_Detected, detectionTrigger, clearDetectionFlag };
        yield return new object?[] { InitialChipSetting3_DetectRisingEdge, InterruptEdgeDetectorState_NotDetected, detectionTrigger, clearDetectionFlag };
        yield return new object?[] { InitialChipSetting3_DetectNone, InterruptEdgeDetectorState_NotDetected, detectionTrigger, clearDetectionFlag };
      }
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsInterruptOnChangeSyncOrAsync))]
  public void ConfigureAsInterruptOnChangeAsync(
    byte initialChipSetting3,
    bool initialInterruptEdgeDetectorState,
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag
  )
    => ConfigureAsInterruptOnChangeSyncOrAsync(
      initialChipSetting3,
      initialInterruptEdgeDetectorState,
      detectionTrigger,
      clearDetectionFlag,
      static async (gp1, trigger, clear)
        => await ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChangeAsync(trigger, clear).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsInterruptOnChangeSyncOrAsync))]
  public void ConfigureAsInterruptOnChange(
    byte initialChipSetting3,
    bool initialInterruptEdgeDetectorState,
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag
  )
    => ConfigureAsInterruptOnChangeSyncOrAsync(
      initialChipSetting3,
      initialInterruptEdgeDetectorState,
      detectionTrigger,
      clearDetectionFlag,
      static (gp1, trigger, clear) => {
        ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChange(trigger, clear);
        return default;
      }
    );

  private void ConfigureAsInterruptOnChangeSyncOrAsync(
    byte initialChipSetting3,
    bool initialInterruptEdgeDetectorState,
    InterruptOnChangeTrigger? detectionTrigger,
    bool clearDetectionFlag,
    Func<Gp1Controller, InterruptOnChangeTrigger?, bool, ValueTask> configureAsInterruptOnChangeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const int Gp1Index = 1;

    var initialGpSettings = new byte[4] {
      InitialGp0Settings,
      InitialGp1Settings,
      InitialGp2Settings,
      InitialGp3Settings
    };
    var initialAdcVoltageReferenceBits = (initialChipSetting3 & 0b_0_0_0_11_1_00) >> 2;
    var initialDetectionTrigger =
      ((initialChipSetting3 & 0b_0_0_1_00_0_00) == 0 ? InterruptOnChangeTrigger.None : InterruptOnChangeTrigger.Rising) | // positive edge
      ((initialChipSetting3 & 0b_0_1_0_00_0_00) == 0 ? InterruptOnChangeTrigger.None : InterruptOnChangeTrigger.Falling); // negative edge

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: initialChipSetting3,
        interruptEdgeDetectorState: initialInterruptEdgeDetectorState
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger,
      Is.EqualTo(initialDetectionTrigger)
    );

    var expectedAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();

    expectedAssignments[Gp1Index] = GpFunction.InterruptOnChange;

    var shouldReenableVrm = (initialAdcVoltageReferenceBits & 0b1) != 0;

    if (shouldReenableVrm) {
      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62)),
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS (response to the command to re-enable VRM)
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );
    }
    else {
      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );
    }

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedPositiveEdgeDetectorBit = (detectionTrigger ?? initialDetectionTrigger) switch {
      InterruptOnChangeTrigger.Both => 0b_0_00_0_1_0_0_0,
      InterruptOnChangeTrigger.Falling => 0b_0_00_0_0_0_0_0,
      InterruptOnChangeTrigger.Rising => 0b_0_00_0_1_0_0_0,
      InterruptOnChangeTrigger.None => 0b_0_00_0_0_0_0_0,
      _ => throw new InvalidOperationException(),
    };
    var expectedNegativeEdgeDetectorBit = (detectionTrigger ?? initialDetectionTrigger) switch {
      InterruptOnChangeTrigger.Both => 0b_0_00_0_0_0_1_0,
      InterruptOnChangeTrigger.Falling => 0b_0_00_0_0_0_1_0,
      InterruptOnChangeTrigger.Rising => 0b_0_00_0_0_0_0_0,
      InterruptOnChangeTrigger.None => 0b_0_00_0_0_0_0_0,
      _ => throw new InvalidOperationException(),
    };

    const byte ExpectedDesignationBits = 0b_000_0_0_100; // IOC

    initialGpSettings[Gp1Index] = (byte)((initialGpSettings[Gp1Index] & 0b_111_1_1_000) | ExpectedDesignationBits);

    var expectedSentSramSettingsCommand = new byte[64];

    expectedSentSramSettingsCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-4] don't care
    expectedSentSramSettingsCommand[5] = (byte)initialAdcVoltageReferenceBits; // [5] ADC Voltage Reference
    // [6] Set Up the Interrupt Detection Mechanism and Clear the Detection Flag
    expectedSentSramSettingsCommand[6] = (byte)(
      ((detectionTrigger.HasValue || clearDetectionFlag) ? 0b_1_00_0_0_0_0_0 : 0b_0_00_0_0_0_0_0) |
      (detectionTrigger.HasValue ? 0b_0_00_1_0_0_0_0 : 0b_0_00_0_0_0_0_0) |
      expectedPositiveEdgeDetectorBit |
      (detectionTrigger.HasValue ? 0b_0_00_0_0_1_0_0 : 0b_0_00_0_0_0_0_0) |
      expectedNegativeEdgeDetectorBit |
      (clearDetectionFlag ? 0b_0_00_0_0_0_0_1 : 0b_0_00_0_0_0_0_0)
    );
    expectedSentSramSettingsCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentSramSettingsCommand[8] = initialGpSettings[0]; // [8] GP0 settings
    expectedSentSramSettingsCommand[9] = initialGpSettings[1]; // [9] GP1 settings
    expectedSentSramSettingsCommand[10] = initialGpSettings[2]; // [10] GP2 settings
    expectedSentSramSettingsCommand[11] = initialGpSettings[3]; // [11] GP3 settings

    var expectedSentReenableVrmCommand = new byte[64];

    expectedSentReenableVrmCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-4] don't care
    expectedSentReenableVrmCommand[5] = (byte)(0b10000000 | (byte)initialAdcVoltageReferenceBits); // [5] ADC Voltage Reference (re-enable)
    // [6] Set Up the Interrupt Detection Mechanism and Clear the Detection Flag
    expectedSentReenableVrmCommand[6] = expectedSentSramSettingsCommand[6];
    expectedSentReenableVrmCommand[7] = 0b00000000; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    expectedSentReenableVrmCommand[8] = initialGpSettings[0]; // [8] GP0 settings
    expectedSentReenableVrmCommand[9] = initialGpSettings[1]; // [9] GP1 settings
    expectedSentReenableVrmCommand[10] = initialGpSettings[2]; // [10] GP2 settings
    expectedSentReenableVrmCommand[11] = initialGpSettings[3]; // [11] GP3 settings

    Assert.That(
      async () => await configureAsInterruptOnChangeAsyncFunc(mcp2221A.GpPin1, detectionTrigger, clearDetectionFlag),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, 0),
      SequenceIs.EqualTo(expectedSentSramSettingsCommand)
    );
    if (shouldReenableVrm) {
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A, 1),
        SequenceIs.EqualTo(expectedSentReenableVrmCommand)
      );
    }

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.InterruptOnChange));
    Assert.That(
      mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger,
      Is.EqualTo(detectionTrigger ?? initialDetectionTrigger)
    );
    Assert.That(
      mcp2221A.GpPin1.LastReadInterruptDetectionFlag,
      Is.False
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(expectedAssignments).AsCollection,
      $"other GP pins must not be configured (except {mcp2221A.GpPin1.PinName})"
    );
  }

  private static IEnumerable<InterruptOnChangeTrigger> YieldTestCases_UndefinedInterruptOnChangeTrigger()
  {
    yield return (InterruptOnChangeTrigger)int.MinValue;
    yield return (InterruptOnChangeTrigger)(-1);
    yield return (InterruptOnChangeTrigger)0b_100;
    yield return (InterruptOnChangeTrigger)int.MaxValue;
  }

  [TestCaseSource(nameof(YieldTestCases_UndefinedInterruptOnChangeTrigger))]
  public void ConfigureAsInterruptOnChangeAsync_UndefinedInterruptOnChangeTrigger(
    InterruptOnChangeTrigger detectionTrigger
  )
    => ConfigureAsInterruptOnChangeSyncOrAsync_UndefinedInterruptOnChangeTrigger(
      detectionTrigger,
      static async (gp1, t) => await ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChangeAsync(detectionTrigger: t, clearDetectionFlag: default).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UndefinedInterruptOnChangeTrigger))]
  public void ConfigureAsInterruptOnChange_UndefinedInterruptOnChangeTrigger(
    InterruptOnChangeTrigger detectionTrigger
  )
    => ConfigureAsInterruptOnChangeSyncOrAsync_UndefinedInterruptOnChangeTrigger(
      detectionTrigger,
      static (gp1, t) => {
        ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChange(detectionTrigger: t, clearDetectionFlag: default);
        return default;
      }
    );

  private void ConfigureAsInterruptOnChangeSyncOrAsync_UndefinedInterruptOnChangeTrigger(
    InterruptOnChangeTrigger detectionTrigger,
    Func<Gp1Controller, InterruptOnChangeTrigger, ValueTask> configureAsInterruptOnChangeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialDetectionTrigger = mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger;

    Assert.That(
      async () => await configureAsInterruptOnChangeAsyncFunc(mcp2221A.GpPin1, detectionTrigger),
      Throws
        .InstanceOf<ArgumentException>()
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo(nameof(detectionTrigger))
        .And
        .Property(nameof(ArgumentException.Message))
        .Contains($"{detectionTrigger}"),
      $"undefined trigger ({mcp2221A.GpPin1.PinName}, {detectionTrigger})"
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(initialAssignments).AsCollection,
      $"must not be configured ({mcp2221A.GpPin1})"
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger,
      Is.EqualTo(initialDetectionTrigger),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger)})"
    );
  }

  [Test]
  public void ConfigureAsInterruptOnChangeAsync_ThrowsWhenUsedByGpioController()
    => ConfigureAsInterruptOnChangeSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp1 => await ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChangeAsync(
        detectionTrigger: InterruptOnChangeTrigger.Both,
        clearDetectionFlag: default
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsInterruptOnChange_ThrowsWhenUsedByGpioController()
    => ConfigureAsInterruptOnChangeSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChange(
          detectionTrigger: InterruptOnChangeTrigger.Both,
          clearDetectionFlag: default
        );
        return default;
      }
    );

  private void ConfigureAsInterruptOnChangeSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<Gp1Controller, ValueTask> configureAsInterruptOnChangeAsyncFunc
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

    for (var gp = 0; gp < 4; gp++) {
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAsInterruptOnChangeAsyncFunc(mcp2221A.GpPin1),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(mcp2221A.GpPin1.PinName)
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

  [Test]
  public void ConfigureAsInterruptOnChangeAsync_CancellationRequested()
    => ConfigureAsInterruptOnChangeSyncOrAsync_CancellationRequested(
      static async (gp1, ct) => await ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChangeAsync(
        detectionTrigger: InterruptOnChangeTrigger.Rising,
        clearDetectionFlag: default,
        cancellationToken: ct
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsInterruptOnChange_CancellationRequested()
    => ConfigureAsInterruptOnChangeSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChange(
          detectionTrigger: InterruptOnChangeTrigger.Falling,
          clearDetectionFlag: default,
          cancellationToken: ct
        );
        return default;
      }
    );

  private void ConfigureAsInterruptOnChangeSyncOrAsync_CancellationRequested(
    Func<Gp1Controller, CancellationToken, ValueTask> configureAsInterruptOnChangeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialDetectionTrigger = mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger;

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAsInterruptOnChangeAsyncFunc(mcp2221A.GpPin1, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token),
      $"cancellation requested ({mcp2221A.GpPin1.PinName})"
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(initialAssignments).AsCollection,
      $"must not be configured ({mcp2221A.GpPin1.PinName})"
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger,
      Is.EqualTo(initialDetectionTrigger),
      $"must not be configured ({nameof(mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger)})"
    );
  }

  [Test]
  public void ConfigureAsInterruptOnChangeAsync_Disposed()
    => ConfigureAsInterruptOnChangeSyncOrAsync_Disposed(
      static async gp1 => await ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChangeAsync(
        detectionTrigger: InterruptOnChangeTrigger.None,
        clearDetectionFlag: default
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsInterruptOnChange_Disposed()
    => ConfigureAsInterruptOnChangeSyncOrAsync_Disposed(
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ConfigureAsInterruptOnChange(
          detectionTrigger: InterruptOnChangeTrigger.Both,
          clearDetectionFlag: default
        );
        return default;
      }
    );

  private void ConfigureAsInterruptOnChangeSyncOrAsync_Disposed(
    Func<Gp1Controller, ValueTask> configureAsInterruptOnChangeAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAsInterruptOnChangeAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>(),
      $"object disposed ({gpPin1.PinName})"
    );
  }

  private static IEnumerable<byte> YieldTestCases_ReadInterruptDetectionSyncAndAsync_InvalidConfiguration()
  {
    yield return 0b_000_1_0_011; // LED_UTX
    yield return 0b_000_0_0_010; // ADC1
    yield return 0b_000_1_0_001; // CLK OUT
    yield return 0b_000_0_0_000; // GPIO1
  }

  [TestCaseSource(nameof(YieldTestCases_ReadInterruptDetectionSyncAndAsync_InvalidConfiguration))]
  public void ReadInterruptDetectionAsync_InvalidConfiguration(byte gp1Settings)
    => ReadInterruptDetectionSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static async mcp2221a => _ = await mcp2221a.GpPin1.ReadInterruptDetectionAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadInterruptDetectionSyncAndAsync_InvalidConfiguration))]
  public void ReadInterruptDetection_InvalidConfiguration(byte gp1Settings)
    => ReadInterruptDetectionSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static mcp2221a => {
        _ = mcp2221a.GpPin1.ReadInterruptDetection();
        return default;
      }
    );

  private void ReadInterruptDetectionSyncAndAsync_InvalidConfiguration(
    byte gp1Settings,
    Func<Mcp2221AController, ValueTask> readInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    // const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: gp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      async () => await readInterruptDetectionAsyncFunc(mcp2221A),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(mcp2221A.GpPin1.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.InterruptOnChange)
    );
  }

  [Test]
  public void ReadInterruptDetectionAsync_Disposed()
    => ReadInterruptDetectionSyncOrAsync_Disposed(
      static async gp1 => { _ = await ((IInterruptOnChangeController)gp1).ReadInterruptDetectionAsync().ConfigureAwait(false); }
    );

  [Test]
  public void ReadInterruptDetection_Disposed()
    => ReadInterruptDetectionSyncOrAsync_Disposed(
      static gp1 => {
        _ = ((IInterruptOnChangeController)gp1).ReadInterruptDetection();
        return default;
      }
    );

  private void ReadInterruptDetectionSyncOrAsync_Disposed(
    Func<Gp1Controller, ValueTask> readInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await readInterruptDetectionAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>(),
      $"object disposed ({gpPin1.PinName})"
    );
  }

  [Test]
  public void ReadInterruptDetectionAsync_CancellationRequested()
    => ReadInterruptDetectionSyncOrAsync_CancellationRequested(
      static async (gp1, ct) => { _ = await ((IInterruptOnChangeController)gp1).ReadInterruptDetectionAsync(ct).ConfigureAwait(false); }
    );

  [Test]
  public void ReadInterruptDetection_CancellationRequested()
    => ReadInterruptDetectionSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        _ = ((IInterruptOnChangeController)gp1).ReadInterruptDetection(ct);
        return default;
      }
    );

  private void ReadInterruptDetectionSyncOrAsync_CancellationRequested(
    Func<Gp1Controller, CancellationToken, ValueTask> readInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await readInterruptDetectionAsyncFunc(mcp2221A.GpPin1, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token),
      $"cancellation requested ({mcp2221A.GpPin1.PinName})"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadInterruptDetectionSyncOrAsync()
  {
    yield return new object[] { 0x00, false, "01-00-00-00-00-00-", 0x_00_01, 0x_00_00, 0x_00_00 };
    yield return new object[] { 0x01, true, "00-00-01-00-00-00-", 0x_00_00, 0x_00_01, 0x_00_00 };
    yield return new object[] { 0x80, true, "00-00-00-00-01-00-", 0x_00_00, 0x_00_00, 0x_00_01 };
    yield return new object[] { 0xFF, true, "FF-03-FF-03-FF-03-", 0x_03_FF, 0x_03_FF, 0x_03_FF };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadInterruptDetectionSyncOrAsync))]
  public ValueTask ReadInterruptDetectionAsync(
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag,
    string adcDataValuesInResponse,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadInterruptDetectionSyncOrAsync(
      interruptEdgeDetectorStateInResponse: interruptEdgeDetectorStateInResponse,
      expectedInterruptDetectionFlag: expectedInterruptDetectionFlag,
      adcDataValuesInResponse: adcDataValuesInResponse,
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static async mcp2221a => await mcp2221a.GpPin1.ReadInterruptDetectionAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadInterruptDetectionSyncOrAsync))]
  public ValueTask ReadInterruptDetection(
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag,
    string adcDataValuesInResponse,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadInterruptDetectionSyncOrAsync(
      interruptEdgeDetectorStateInResponse: interruptEdgeDetectorStateInResponse,
      expectedInterruptDetectionFlag: expectedInterruptDetectionFlag,
      adcDataValuesInResponse: adcDataValuesInResponse,
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static mcp2221a => new(mcp2221a.GpPin1.ReadInterruptDetection())
    );

  private async ValueTask ReadInterruptDetectionSyncOrAsync(
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag,
    string adcDataValuesInResponse,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue,
    Func<Mcp2221AController, ValueTask<bool>> readInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
    var statusSetParametersResponse = string.Concat(
      "10-00-",
      string.Join("-", Enumerable.Repeat("00", 24 - 2)), "-",
      $"{interruptEdgeDetectorStateInResponse:X2}-", // [24] Interrupt edge detector state
      string.Join("-", Enumerable.Repeat("00", 50 - 25)), "-",
      adcDataValuesInResponse, // [50-55] ADC Data (16-bit) values
      string.Join("-", Enumerable.Repeat("00", 64 - 56))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, statusSetParametersResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x10; // STATUS/SET PARAMETERS

    Assert.That(
      await readInterruptDetectionAsyncFunc(mcp2221A),
      Is.EqualTo(expectedInterruptDetectionFlag)
    );
    Assert.That(
      mcp2221A.GpPin1.LastReadInterruptDetectionFlag,
      Is.EqualTo(expectedInterruptDetectionFlag)
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(IInterruptOnChangeController.ReadInterruptDetection)}"
    );

    Assert.That(
      mcp2221A.GpPin1.LastReadAnalogRawValue,
      Is.EqualTo(expectedAdc1RawValue),
      $"STATUS/SET PARAMETERS also updates ADC values of {mcp2221A.GpPin1.PinName}"
    );
    Assert.That(
      mcp2221A.GpPin2.LastReadAnalogRawValue,
      Is.EqualTo(expectedAdc2RawValue),
      $"STATUS/SET PARAMETERS also updates ADC values of {mcp2221A.GpPin2.PinName}"
    );
    Assert.That(
      mcp2221A.GpPin3.LastReadAnalogRawValue,
      Is.EqualTo(expectedAdc3RawValue),
      $"STATUS/SET PARAMETERS also updates ADC values of {mcp2221A.GpPin3.PinName}"
    );
  }

  private static IEnumerable<byte> YieldTestCases_ClearInterruptDetectionSyncAndAsync_InvalidConfiguration()
  {
    yield return 0b_000_1_0_011; // LED_UTX
    yield return 0b_000_0_0_010; // ADC1
    yield return 0b_000_1_0_001; // CLK OUT
    yield return 0b_000_0_0_000; // GPIO1
  }

  [TestCaseSource(nameof(YieldTestCases_ClearInterruptDetectionSyncAndAsync_InvalidConfiguration))]
  public void ClearInterruptDetectionAsync_InvalidConfiguration(byte gp1Settings)
    => ClearInterruptDetectionSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static async gp1 => await ((IInterruptOnChangeController)gp1).ClearInterruptDetectionAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ClearInterruptDetectionSyncAndAsync_InvalidConfiguration))]
  public void ClearInterruptDetection_InvalidConfiguration(byte gp1Settings)
    => ClearInterruptDetectionSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ClearInterruptDetection();
        return default;
      }
    );

  private void ClearInterruptDetectionSyncAndAsync_InvalidConfiguration(
    byte gp1Settings,
    Func<Gp1Controller, ValueTask> clearInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    // const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: gp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await clearInterruptDetectionAsyncFunc(mcp2221A.GpPin1),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(mcp2221A.GpPin1.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.InterruptOnChange)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void ClearInterruptDetectionAsync_Disposed()
    => ClearInterruptDetectionSyncOrAsync_Disposed(
      static async gp1 => await ((IInterruptOnChangeController)gp1).ClearInterruptDetectionAsync().ConfigureAwait(false)
    );

  [Test]
  public void ClearInterruptDetection_Disposed()
    => ClearInterruptDetectionSyncOrAsync_Disposed(
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ClearInterruptDetection();
        return default;
      }
    );

  private void ClearInterruptDetectionSyncOrAsync_Disposed(
    Func<Gp1Controller, ValueTask> clearInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await clearInterruptDetectionAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>(),
      $"object disposed ({gpPin1.PinName})"
    );
  }

  [Test]
  public void ClearInterruptDetectionAsync_CancellationRequested()
    => ClearInterruptDetectionSyncOrAsync_CancellationRequested(
      static async (gp1, ct) => await ((IInterruptOnChangeController)gp1).ClearInterruptDetectionAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void ClearInterruptDetection_CancellationRequested()
    => ClearInterruptDetectionSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        ((IInterruptOnChangeController)gp1).ClearInterruptDetection(ct);
        return default;
      }
    );

  private void ClearInterruptDetectionSyncOrAsync_CancellationRequested(
    Func<Gp1Controller, CancellationToken, ValueTask> clearInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialChipSetting3 = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await clearInterruptDetectionAsyncFunc(mcp2221A.GpPin1, cts.Token),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token),
      $"cancellation requested ({mcp2221A.GpPin1.PinName})"
    );
  }

  [Test]
  public void ClearInterruptDetectionAsync_ThrowsWhenUsedByGpioController()
    => ClearInterruptDetectionSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp1 => await ((IInterruptOnChangeController)gp1).ClearInterruptDetectionAsync().ConfigureAwait(false)
    );

  [Test]
  public void ClearInterruptDetection_ThrowsWhenUsedByGpioController()
    => ClearInterruptDetectionSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ClearInterruptDetection();
        return default;
      }
    );

  private void ClearInterruptDetectionSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<Gp1Controller, ValueTask> clearInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSetting3 = 0b_0_1_1_00_0_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 00(Off), ADCREF: 0(VDD)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: InitialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );

    for (var gp = 0; gp < 4; gp++) {
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

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await clearInterruptDetectionAsyncFunc(mcp2221A.GpPin1),
      Throws
#if false
        // Since the check for `CurrentFunction` is performed first, the exception resulting
        // from the subsequent check for `IsUsedByGpioController` will not be thrown.
        .TypeOf<InvalidOperationException>()
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(GpioController))
#endif
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(mcp2221A.GpPin1.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.InterruptOnChange)
        .And
        .Property(nameof(Mcp2221AConfigurationException.CurrentFunction))
        .EqualTo(GpFunction.Gpio)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
  }

  private static System.Collections.IEnumerable YieldTestCases_ClearInterruptDetectionSyncOrAsync()
  {
    // INTDETFEEN: 0/1, INTDETREEN: 0/1, ADCVRM: 00(Off), ADCREF: 0(Vdd)
    const byte InitialChipSetting3_DetectBothEdge = 0b_0_1_1_00_0_00; // INTDETFEEN: 1, INTDETREEN: 1
    const byte InitialChipSetting3_DetectFallingEdge = 0b_0_1_0_00_0_00; // INTDETFEEN: 1, INTDETREEN: 0
    const byte InitialChipSetting3_DetectRisingEdge = 0b_0_0_1_00_0_00; // INTDETFEEN: 0, INTDETREEN: 1
    const byte InitialChipSetting3_DetectNone = 0b_0_0_0_00_0_00; // INTDETFEEN: 0, INTDETREEN: 0

    foreach ((byte state, bool expected) in new (byte, bool)[] {
      (0x00, false),
      (0x01, true),
      (0x80, true),
      (0xFF, true),
    }) {
      yield return new object[] { InitialChipSetting3_DetectBothEdge, state, expected };
      yield return new object[] { InitialChipSetting3_DetectFallingEdge, state, expected };
      yield return new object[] { InitialChipSetting3_DetectRisingEdge, state, expected };
      yield return new object[] { InitialChipSetting3_DetectNone, state, expected };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ClearInterruptDetectionSyncOrAsync))]
  public void ClearInterruptDetectionAsync(
    byte initialChipSetting3,
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag
  )
    => ClearInterruptDetectionSyncOrAsync(
      initialChipSetting3: initialChipSetting3,
      interruptEdgeDetectorStateInResponse: interruptEdgeDetectorStateInResponse,
      expectedInterruptDetectionFlag: expectedInterruptDetectionFlag,
      static async gp1 => await ((IInterruptOnChangeController)gp1).ClearInterruptDetectionAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ClearInterruptDetectionSyncOrAsync))]
  public void ClearInterruptDetection(
    byte initialChipSetting3,
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag
  )
    => ClearInterruptDetectionSyncOrAsync(
      initialChipSetting3: initialChipSetting3,
      interruptEdgeDetectorStateInResponse: interruptEdgeDetectorStateInResponse,
      expectedInterruptDetectionFlag: expectedInterruptDetectionFlag,
      static gp1 => {
        ((IInterruptOnChangeController)gp1).ClearInterruptDetection();
        return default;
      }
    );

  private void ClearInterruptDetectionSyncOrAsync(
    byte initialChipSetting3,
    byte interruptEdgeDetectorStateInResponse,
    bool expectedInterruptDetectionFlag,
    Func<Gp1Controller, ValueTask> clearInterruptDetectionAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_100; // Alternate function 2 (interrupt detector)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSetting3: initialChipSetting3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialDetectionTrigger = mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger;

    /*
     * read current detection state
     */
    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      string.Concat(
        "10-00-",
        string.Join("-", Enumerable.Repeat("00", 24 - 2)), "-",
        $"{interruptEdgeDetectorStateInResponse:X2}-", // [24] Interrupt edge detector state
        string.Join("-", Enumerable.Repeat("00", 64 - 25))
      )
    );

    Assert.That(
      mcp2221A.GpPin1.ReadInterruptDetection(),
      Is.EqualTo(expectedInterruptDetectionFlag)
    );
    Assert.That(
      mcp2221A.GpPin1.LastReadInterruptDetectionFlag,
      Is.EqualTo(expectedInterruptDetectionFlag)
    );

    /*
     * then clear current detection state
     */
    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedPositiveEdgeDetectorBit = mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger switch {
      InterruptOnChangeTrigger.Both => 0b_0_00_0_1_0_0_0,
      InterruptOnChangeTrigger.Falling => 0b_0_00_0_0_0_0_0,
      InterruptOnChangeTrigger.Rising => 0b_0_00_0_1_0_0_0,
      InterruptOnChangeTrigger.None => 0b_0_00_0_0_0_0_0,
      _ => throw new InvalidOperationException(),
    };
    var expectedNegativeEdgeDetectorBit = mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger switch {
      InterruptOnChangeTrigger.Both => 0b_0_00_0_0_0_1_0,
      InterruptOnChangeTrigger.Falling => 0b_0_00_0_0_0_1_0,
      InterruptOnChangeTrigger.Rising => 0b_0_00_0_0_0_0_0,
      InterruptOnChangeTrigger.None => 0b_0_00_0_0_0_0_0,
      _ => throw new InvalidOperationException(),
    };
    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-5] don't care
    // [6] Set Up the Interrupt Detection Mechanism and Clear the Detection Flag
    expectedSentCommand[6] = (byte)(
      0b_1_00_0_0_0_0_0 | // Bit 7: Enable the modification of the interrupt detection conditions
      expectedPositiveEdgeDetectorBit |
      expectedNegativeEdgeDetectorBit |
      0b_0_00_0_0_0_0_1 // Bit 0: Clear the interrupt detection flag
    );
    expectedSentCommand[7] = 0; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    expectedSentCommand[8] = InitialGp0Settings; // [8] GP0 settings
    expectedSentCommand[9] = InitialGp1Settings; // [9] GP1 settings
    expectedSentCommand[10] = InitialGp2Settings; // [10] GP2 settings
    expectedSentCommand[11] = InitialGp3Settings; // [11] GP3 settings

    Assert.That(
      async () => await clearInterruptDetectionAsyncFunc(mcp2221A.GpPin1),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger,
      Is.EqualTo(initialDetectionTrigger),
      $"{nameof(mcp2221A.GpPin1.CurrentInterruptOnChangeTrigger)} must not be changed"
    );
    Assert.That(
      mcp2221A.GpPin1.LastReadInterruptDetectionFlag,
      Is.False,
      $"{mcp2221A.GpPin1.LastReadInterruptDetectionFlag} should also be cleared"
    );
  }
}
