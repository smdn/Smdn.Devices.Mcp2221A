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
  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsClockOutputSyncOrAsync()
  {
    const byte InitialChipSettings1_50Percent12MHz = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)
    const byte InitialChipSettings1_00PercentReserved = 0b_000_00_000; // Duty cycle: 0%; Divider: reserved

    foreach (var frequency in new ClockOutputFrequency?[] {
      ClockOutputFrequency.Frequency24MHz,
      ClockOutputFrequency.Frequency12MHz,
      ClockOutputFrequency.Frequency6MHz,
      ClockOutputFrequency.Frequency3MHz,
      ClockOutputFrequency.Frequency1500kHz,
      ClockOutputFrequency.Frequency750kHz,
      ClockOutputFrequency.Frequency375kHz,
      null,
    }) {
      foreach (var dutyCycle in new ClockOutputDutyCycle?[] {
        ClockOutputDutyCycle.Duty75,
        ClockOutputDutyCycle.Duty50,
        ClockOutputDutyCycle.Duty25,
        ClockOutputDutyCycle.Duty0,
        null,
      }) {
        yield return new object?[] { InitialChipSettings1_50Percent12MHz, frequency, dutyCycle };
        yield return new object?[] { InitialChipSettings1_00PercentReserved, frequency, dutyCycle };
      }
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsClockOutputSyncOrAsync))]
  public void ConfigureAsClockOutputAsync(
    byte initialChipSettings1,
    ClockOutputFrequency? frequency,
    ClockOutputDutyCycle? dutyCycle
  )
    => ConfigureAsClockOutputSyncOrAsync(
      initialChipSettings1,
      frequency,
      dutyCycle,
      static async (gp1, f, d) => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(f, d).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsClockOutputSyncOrAsync))]
  public void ConfigureAsClockOutput(
    byte initialChipSettings1,
    ClockOutputFrequency? frequency,
    ClockOutputDutyCycle? dutyCycle
  )
    => ConfigureAsClockOutputSyncOrAsync(
      initialChipSettings1,
      frequency,
      dutyCycle,
      static (gp1, f, d) => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(f, d);
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync(
    byte initialChipSettings1,
    ClockOutputFrequency? frequency,
    ClockOutputDutyCycle? dutyCycle,
    Func<Gp1Controller, ClockOutputFrequency?, ClockOutputDutyCycle?, ValueTask> configureAsClockOutputAsyncFunc
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings1: initialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );

    var expectedAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();

    expectedAssignments[Gp1Index] = GpFunction.ClockOutput;

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedClockDividerBits = frequency switch {
      null => initialChipSettings1 & 0b_000_00_111,
      ClockOutputFrequency.Frequency24MHz => 0b_001,
      ClockOutputFrequency.Frequency12MHz => 0b_010,
      ClockOutputFrequency.Frequency6MHz => 0b_011,
      ClockOutputFrequency.Frequency3MHz => 0b_100,
      ClockOutputFrequency.Frequency1500kHz => 0b_101,
      ClockOutputFrequency.Frequency750kHz => 0b_110,
      ClockOutputFrequency.Frequency375kHz => 0b_111,
      _ => throw new InvalidOperationException(),
    };
    var expectedDutyCycleBits = dutyCycle switch {
      null => (initialChipSettings1 & 0b_000_11_000) >> 3,
      ClockOutputDutyCycle.Duty0 => 0b_00,
      ClockOutputDutyCycle.Duty25 => 0b_01,
      ClockOutputDutyCycle.Duty50 => 0b_10,
      ClockOutputDutyCycle.Duty75 => 0b_11,
      _ => throw new InvalidOperationException(),
    };

    const byte ExpectedDesignationBits = 0b_000_0_0_001; // CLK OUT

    initialGpSettings[Gp1Index] = (byte)((initialGpSettings[Gp1Index] & 0b_1_1111_00_0) | ExpectedDesignationBits);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1] don't care
    // [2] Clock Output Divider Value
    expectedSentCommand[2] = (byte)(
      ((frequency.HasValue || dutyCycle.HasValue) ? 0b_1_00_00_000 : 0b_0_00_00_000) |
      (expectedDutyCycleBits << 3) |
      expectedClockDividerBits
    );
    // [3-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = initialGpSettings[0]; // [8] GP0 settings
    expectedSentCommand[9] = initialGpSettings[1]; // [9] GP1 settings
    expectedSentCommand[10] = initialGpSettings[2]; // [10] GP2 settings
    expectedSentCommand[11] = initialGpSettings[3]; // [11] GP3 settings

    Assert.That(
      async () => await configureAsClockOutputAsyncFunc(mcp2221A.GpPin1, frequency, dutyCycle),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.ClockOutput));
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputFrequency,
      Is.EqualTo(frequency ?? (ClockOutputFrequency)(initialChipSettings1 & 0b_000_00_111)));
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycle,
      Is.EqualTo(dutyCycle ?? (ClockOutputDutyCycle)((initialChipSettings1 & 0b_000_11_000) >> 3))
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(expectedAssignments).AsCollection,
      $"other GP pins must not be configured (except {mcp2221A.GpPin1.PinName})"
    );
  }

  [Test]
  public void ConfigureAsClockOutputAsync_ClockOutputFrequency_Reserved()
    => ConfigureAsClockOutputSyncOrAsync_ClockOutputFrequency_Reserved(
      static async gp1 => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(ClockOutputFrequency.Reserved).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsClockOutput_ClockOutputFrequency_Reserved()
    => ConfigureAsClockOutputSyncOrAsync_ClockOutputFrequency_Reserved(
      static gp1 => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(ClockOutputFrequency.Reserved);
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync_ClockOutputFrequency_Reserved(
    Func<Gp1Controller, ValueTask> configureAsClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings1: InitialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialFrequency = mcp2221A.GpPin1.CurrentClockOutputFrequency;
    var initialDutyCycle = mcp2221A.GpPin1.CurrentClockOutputDutyCycle;

    Assert.That(
      async () => await configureAsClockOutputAsyncFunc(mcp2221A.GpPin1),
      Throws
        .ArgumentException
        .With
        .Property(nameof(ArgumentException.Message))
        .Contains(nameof(ClockOutputFrequency.Reserved)),
      $"unsupported frequency ({nameof(ClockOutputFrequency)}.{nameof(ClockOutputFrequency.Reserved)})"
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(initialAssignments).AsCollection,
      $"must not be configured ({mcp2221A.GpPin1})"
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputFrequency,
      Is.EqualTo(initialFrequency),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentClockOutputFrequency)})"
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycle,
      Is.EqualTo(initialDutyCycle),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentClockOutputDutyCycle)})"
    );
  }

  private static IEnumerable<ClockOutputFrequency> YieldTestCases_UnsupportedClockOutputFrequency()
  {
    yield return (ClockOutputFrequency)int.MinValue;
    yield return (ClockOutputFrequency)(-1);
    yield return (ClockOutputFrequency)0b_1000;
    yield return (ClockOutputFrequency)int.MaxValue;
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedClockOutputFrequency))]
  public void ConfigureAsClockOutputAsync_UnsupportedClockOutputFrequency(ClockOutputFrequency frequency)
    => ConfigureAsClockOutputSyncOrAsync_UnsupportedClockOutputFrequencyOrDutyCycle(
      frequency,
      null,
      static async (gp1, f, d) => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(f, d).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedClockOutputFrequency))]
  public void ConfigureAsClockOutput_UnsupportedClockOutputFrequency(ClockOutputFrequency frequency)
    => ConfigureAsClockOutputSyncOrAsync_UnsupportedClockOutputFrequencyOrDutyCycle(
      frequency,
      null,
      static (gp1, f, d) => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(f, d);
        return default;
      }
    );

  private static IEnumerable<ClockOutputDutyCycle> YieldTestCases_UnsupportedClockOutputDutyCycle()
  {
    yield return (ClockOutputDutyCycle)int.MinValue;
    yield return (ClockOutputDutyCycle)(-1);
    yield return (ClockOutputDutyCycle)0b_100;
    yield return (ClockOutputDutyCycle)int.MaxValue;
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedClockOutputDutyCycle))]
  public void ConfigureAsClockOutputAsync_UnsupportedClockOutputDutyCycle(ClockOutputDutyCycle dutyCycle)
    => ConfigureAsClockOutputSyncOrAsync_UnsupportedClockOutputFrequencyOrDutyCycle(
      null,
      dutyCycle,
      static async (gp1, f, d) => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(f, d).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedClockOutputDutyCycle))]
  public void ConfigureAsClockOutput_UnsupportedClockOutputDutyCycle(ClockOutputDutyCycle dutyCycle)
    => ConfigureAsClockOutputSyncOrAsync_UnsupportedClockOutputFrequencyOrDutyCycle(
      null,
      dutyCycle,
      static (gp1, f, d) => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(f, d);
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync_UnsupportedClockOutputFrequencyOrDutyCycle(
    ClockOutputFrequency? frequency,
    ClockOutputDutyCycle? dutyCycle,
    Func<Gp1Controller, ClockOutputFrequency?, ClockOutputDutyCycle?, ValueTask> configureAsClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings1: InitialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialFrequency = mcp2221A.GpPin1.CurrentClockOutputFrequency;
    var initialDutyCycle = mcp2221A.GpPin1.CurrentClockOutputDutyCycle;

    Assert.That(
      async () => await configureAsClockOutputAsyncFunc(mcp2221A.GpPin1, frequency, dutyCycle),
      Throws
        .ArgumentException
        .With
        .Property(nameof(ArgumentException.Message))
        .Contains(frequency.HasValue ? $"{frequency}" : $"{dutyCycle}"),
      $"unsupported frequency or duty cycle ({mcp2221A.GpPin1.PinName}, {frequency}/{dutyCycle})"
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(initialAssignments).AsCollection,
      $"must not be configured ({mcp2221A.GpPin1})"
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputFrequency,
      Is.EqualTo(initialFrequency),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentClockOutputFrequency)})"
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycle,
      Is.EqualTo(initialDutyCycle),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentClockOutputDutyCycle)})"
    );
  }

  [Test]
  public void ConfigureAsClockOutputAsync_ThrowsWhenUsedByGpioController()
    => ConfigureAsClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp1 => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(
        ClockOutputFrequency.Frequency3MHz,
        ClockOutputDutyCycle.Duty25
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsClockOutput_ThrowsWhenUsedByGpioController()
    => ConfigureAsClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp1 => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(
          ClockOutputFrequency.Frequency3MHz,
          ClockOutputDutyCycle.Duty25
        );
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<Gp1Controller, ValueTask> configureAsClockOutputAsyncFunc
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
      async () => await configureAsClockOutputAsyncFunc(mcp2221A.GpPin1),
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
  public void ConfigureAsClockOutputAsync_CancellationRequested()
    => ConfigureAsClockOutputSyncOrAsync_CancellationRequested(
      static async (gp1, ct) => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(
        ClockOutputFrequency.Frequency3MHz,
        ClockOutputDutyCycle.Duty25,
        cancellationToken: ct
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsClockOutput_CancellationRequested()
    => ConfigureAsClockOutputSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(
          ClockOutputFrequency.Frequency3MHz,
          ClockOutputDutyCycle.Duty25,
          cancellationToken: ct
        );
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync_CancellationRequested(
    Func<Gp1Controller, CancellationToken, ValueTask> configureAsClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings1: InitialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialFrequency = mcp2221A.GpPin1.CurrentClockOutputFrequency;
    var initialDutyCycle = mcp2221A.GpPin1.CurrentClockOutputDutyCycle;

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAsClockOutputAsyncFunc(mcp2221A.GpPin1, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
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
      mcp2221A.GpPin1.CurrentClockOutputFrequency,
      Is.EqualTo(initialFrequency),
      $"must not be configured ({nameof(mcp2221A.GpPin1.CurrentClockOutputFrequency)})"
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycle,
      Is.EqualTo(initialDutyCycle),
      $"must not be changed ({nameof(mcp2221A.GpPin1.CurrentClockOutputDutyCycle)})"
    );
  }

  [Test]
  public void ConfigureAsClockOutputAsync_Disposed()
    => ConfigureAsClockOutputSyncOrAsync_Disposed(
      static async gp1 => await ((IClockOutputController)gp1).ConfigureAsClockOutputAsync(
        ClockOutputFrequency.Frequency3MHz,
        ClockOutputDutyCycle.Duty25
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsClockOutput_Disposed()
    => ConfigureAsClockOutputSyncOrAsync_Disposed(
      static gp1 => {
        ((IClockOutputController)gp1).ConfigureAsClockOutput(
          ClockOutputFrequency.Frequency3MHz,
          ClockOutputDutyCycle.Duty25
        );
        return default;
      }
    );

  private void ConfigureAsClockOutputSyncOrAsync_Disposed(
    Func<Gp1Controller, ValueTask> configureAsClockOutputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAsClockOutputAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>(),
      $"object disposed ({gpPin1.PinName})"
    );
  }

  private static IEnumerable<byte> YieldTestCases_SuspendClockOutputSyncAndAsync_InvalidConfiguration()
  {
    yield return 0b_000_0_0_100; // IOC
    yield return 0b_000_0_0_011; // LED_UTX
    yield return 0b_000_0_0_010; // ADC1
    yield return 0b_000_0_0_000; // GPIO1
  }

  [TestCaseSource(nameof(YieldTestCases_SuspendClockOutputSyncAndAsync_InvalidConfiguration))]
  public void SuspendClockOutputAsync_InvalidConfiguration(byte gp1Settings)
    => SuspendClockOutputSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static async gp1 => await ((IClockOutputController)gp1).SuspendClockOutputAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_SuspendClockOutputSyncAndAsync_InvalidConfiguration))]
  public void SuspendClockOutput_InvalidConfiguration(byte gp1Settings)
    => SuspendClockOutputSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      static gp1 => {
        ((IClockOutputController)gp1).SuspendClockOutput();
        return default;
      }
    );

  private void SuspendClockOutputSyncAndAsync_InvalidConfiguration(
    byte gp1Settings,
    Func<Gp1Controller, ValueTask> suspendClockOutputAsyncFunc
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
    var initialGpFunction = mcp2221A.GpPin1.CurrentFunction;

    Assert.That(
      async () => await suspendClockOutputAsyncFunc(mcp2221A.GpPin1),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(mcp2221A.GpPin1.PinName)
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(initialGpFunction),
      $"must not be reconfigured ({nameof(mcp2221A.GpPin1.CurrentFunction)})"
    );
  }

  [Test]
  public void SuspendClockOutputAsync_Disposed()
    => SuspendClockOutputSyncOrAsync_Disposed(
      static async gp1 => await ((IClockOutputController)gp1).SuspendClockOutputAsync().ConfigureAwait(false)
    );

  [Test]
  public void SuspendClockOutput_Disposed()
    => SuspendClockOutputSyncOrAsync_Disposed(
      static gp1 => {
        ((IClockOutputController)gp1).SuspendClockOutput();
        return default;
      }
    );

  private void SuspendClockOutputSyncOrAsync_Disposed(
    Func<Gp1Controller, ValueTask> suspendClockOutputAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSettings1: InitialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );
    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await suspendClockOutputAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>(),
      $"object disposed ({gpPin1.PinName})"
    );
  }

  [Test]
  public void SuspendClockOutputAsync_CancellationRequested()
    => SuspendClockOutputSyncOrAsync_CancellationRequested(
      static async (gp1, ct) => await ((IClockOutputController)gp1).SuspendClockOutputAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void SuspendClockOutput_CancellationRequested()
    => SuspendClockOutputSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        ((IClockOutputController)gp1).SuspendClockOutput(ct);
        return default;
      }
    );

  private void SuspendClockOutputSyncOrAsync_CancellationRequested(
    Func<Gp1Controller, CancellationToken, ValueTask> suspendClockOutputAsyncFunc
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSettings1: InitialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await suspendClockOutputAsyncFunc(mcp2221A.GpPin1, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token),
      $"cancellation requested ({mcp2221A.GpPin1.PinName})"
    );
  }

  [Test]
  public void SuspendClockOutputAsync_ThrowsWhenUsedByGpioController()
    => SuspendClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp1 => await ((IClockOutputController)gp1).SuspendClockOutputAsync().ConfigureAwait(false)
    );

  [Test]
  public void SuspendClockOutput_ThrowsWhenUsedByGpioController()
    => SuspendClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp1 => {
        ((IClockOutputController)gp1).SuspendClockOutput();
        return default;
      }
    );

  private void SuspendClockOutputSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<Gp1Controller, ValueTask> suspendClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings1 = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings1: InitialChipSettings1
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
      async () => await suspendClockOutputAsyncFunc(mcp2221A.GpPin1),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(mcp2221A.GpPin1.PinName)
        // Since the check for `CurrentFunction` is performed first, the exception resulting
        // from the subsequent check for `IsUsedByGpioController` will not be thrown.
#if false
        .And
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(GpioController))
#endif
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
  }

  private static System.Collections.IEnumerable YieldTestCases_SuspendClockOutputSyncOrAsync()
  {
    const byte InitialChipSettings1_50Percent12MHz = 0b_000_10_010; // Duty cycle: 50%; Divider: 12MHz (factory default)
    const byte InitialChipSettings1_00PercentReserved = 0b_000_00_000; // Duty cycle: 0%; Divider: reserved
    const byte InitialChipSettings1_75Percent3MHz = 0b_000_11_100; // Duty cycle: 75%; Divider: 3MHz
    const byte InitialChipSettings1_25Percent375kHz = 0b_000_01_111; // Duty cycle: 25%; Divider: 375kHz

    yield return new object[] { InitialChipSettings1_50Percent12MHz };
    yield return new object[] { InitialChipSettings1_00PercentReserved };
    yield return new object[] { InitialChipSettings1_75Percent3MHz };
    yield return new object[] { InitialChipSettings1_25Percent375kHz };
  }

  [TestCaseSource(nameof(YieldTestCases_SuspendClockOutputSyncOrAsync))]
  public void SuspendClockOutputAsync(byte initialChipSettings1)
    => SuspendClockOutputSyncOrAsync(
      initialChipSettings1: initialChipSettings1,
      static async gp1 => await ((IClockOutputController)gp1).SuspendClockOutputAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_SuspendClockOutputSyncOrAsync))]
  public void SuspendClockOutput(byte initialChipSettings1)
    => SuspendClockOutputSyncOrAsync(
      initialChipSettings1: initialChipSettings1,
      static gp1 => {
        ((IClockOutputController)gp1).SuspendClockOutput();
        return default;
      }
    );

  private void SuspendClockOutputSyncOrAsync(
    byte initialChipSettings1,
    Func<Gp1Controller, ValueTask> suspendClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSettings1: initialChipSettings1
      ),
      shouldDisposeUsbHidDevice: true
    );

    var initialFrequency = mcp2221A.GpPin1.CurrentClockOutputFrequency;
    var initialDutyCycle = mcp2221A.GpPin1.CurrentClockOutputDutyCycle;

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1] don't care
    // [2] Clock Output Divider Value
    expectedSentCommand[2] = initialChipSettings1;
    // [3-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = InitialGp0Settings; // [8] GP0 settings
    expectedSentCommand[9] = 0b_000_0_0_000; // [9] GP1 settings: GPIO Output=0(LOW), GPIO Direction=0(OUTPUT), Designation=000(GPIO)
    expectedSentCommand[10] = InitialGp2Settings; // [10] GP2 settings
    expectedSentCommand[11] = InitialGp3Settings; // [11] GP3 settings

    Assert.That(
      async () => await suspendClockOutputAsyncFunc(mcp2221A.GpPin1),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(PinValue.Low));

    Assert.That(mcp2221A.GpPin1.CurrentClockOutputFrequency, Is.EqualTo(initialFrequency));
    Assert.That(mcp2221A.GpPin1.CurrentClockOutputDutyCycle, Is.EqualTo(initialDutyCycle));
  }
}
