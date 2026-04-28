// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class IClockOutputControllerExtensionsTests {
  private static System.Collections.IEnumerable YieldTestCases_CurrentClockOutputFrequencyInHz()
  {
    yield return new object[] { 0b_000_00_001, 24_000_000 };
    yield return new object[] { 0b_000_00_010, 12_000_000 };
    yield return new object[] { 0b_000_01_011, 6_000_000 };
    yield return new object[] { 0b_000_01_100, 3_000_000 };
    yield return new object[] { 0b_000_10_101, 1_500_000 };
    yield return new object[] { 0b_000_10_110, 750_000 };
    yield return new object[] { 0b_000_11_111, 375_000 };
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentClockOutputFrequencyInHz))]
  public void CurrentClockOutputFrequencyInHz(
    byte chipSetting1,
    int expectedClockOutputFrequencyInHz
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting1: chipSetting1
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputFrequencyInHz,
      Is.EqualTo(expectedClockOutputFrequencyInHz)
    );
  }

  [Test]
  public void CurrentClockOutputFrequencyInHz_Reserved()
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)
    const byte InitialChipSetting1_50PercentReserved = 0b_000_10_000; // Duty cycle: 50%; Divider: reserved

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting1: InitialChipSetting1_50PercentReserved
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () => _ = mcp2221A.GpPin1.CurrentClockOutputFrequencyInHz,
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(ClockOutputFrequency.Reserved))
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentClockOutputDutyCycleInPercent()
  {
    yield return new object[] { 0b_000_00_000, 0 };
    yield return new object[] { 0b_000_01_001, 25 };
    yield return new object[] { 0b_000_10_010, 50 };
    yield return new object[] { 0b_000_11_100, 75 };
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentClockOutputDutyCycleInPercent))]
  public void CurrentClockOutputDutyCycleInPercent(
    byte chipSetting1,
    int expectedClockOutputDutyCycleInPercent
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting1: chipSetting1
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycleInPercent,
      Is.EqualTo(expectedClockOutputDutyCycleInPercent)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_CurrentClockOutputDutyRatio()
  {
    yield return new object[] { 0b_000_00_000, 0.0 };
    yield return new object[] { 0b_000_01_001, 0.25 };
    yield return new object[] { 0b_000_10_010, 0.50 };
    yield return new object[] { 0b_000_11_100, 0.75 };
  }

  [TestCaseSource(nameof(YieldTestCases_CurrentClockOutputDutyRatio))]
  public void CurrentClockOutputDutyRatio(
    byte chipSetting1,
    double expectedClockOutputDutyRatio
  )
  {
    const byte InitialGp1Settings = 0b_000_1_0_001; // Dedicated function operation (CLK OUT)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting1: chipSetting1
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyRatio,
      Is.EqualTo(expectedClockOutputDutyRatio)
    );
  }

  [Test]
  public void ResumeClockOutputAsync_ArgumentNull()
  {
    IClockOutputController? gp1 = null;

    Assert.That(
      () => gp1!.ResumeClockOutputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ResumeClockOutput_ArgumentNull()
  {
    IClockOutputController? gp1 = null;

    Assert.That(
      () => gp1!.ResumeClockOutput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ResumeClockOutputAsync_CancellationRequested()
    => ResumeClockOutputSyncOrAsync_CancellationRequested(
      static async (gp1, ct)
        => await gp1.ResumeClockOutputAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void ResumeClockOutput_CancellationRequested()
    => ResumeClockOutputSyncOrAsync_CancellationRequested(
      static (gp1, ct) => {
        gp1.ResumeClockOutput(ct);
        return default;
      }
    );

  private void ResumeClockOutputSyncOrAsync_CancellationRequested(
    Func<IClockOutputController, CancellationToken, ValueTask> resumeClockOutputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await resumeClockOutputAsyncFunc(
        mcp2221A.GpPin1,
        cts.Token
      ),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );
    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(initialGp1Function)
    );
  }

  [Test]
  public void ResumeClockOutputAsync_Disposed()
    => ResumeClockOutputSyncOrAsync_Disposed(
      static async gp1 => await gp1.ResumeClockOutputAsync().ConfigureAwait(false)
    );

  [Test]
  public void ResumeClockOutput_Disposed()
    => ResumeClockOutputSyncOrAsync_Disposed(
      static gp1 => {
        gp1.ResumeClockOutput();
        return default;
      }
    );

  private void ResumeClockOutputSyncOrAsync_Disposed(
    Func<IClockOutputController, ValueTask> resumeClockOutputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    var gpPin1 = mcp2221A.GpPin1;

    mcp2221A.Dispose();

    Assert.That(
      async () => await resumeClockOutputAsyncFunc(gpPin1),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ResumeClockOutputSyncOrAsync()
  {
    yield return new object[] { 0b_000_00_000, ClockOutputFrequency.Reserved, ClockOutputDutyCycle.Duty0 };
    yield return new object[] { 0b_000_00_001, ClockOutputFrequency.Frequency24MHz, ClockOutputDutyCycle.Duty0 };
    yield return new object[] { 0b_000_01_010, ClockOutputFrequency.Frequency12MHz, ClockOutputDutyCycle.Duty25 };
    yield return new object[] { 0b_000_01_011, ClockOutputFrequency.Frequency6MHz, ClockOutputDutyCycle.Duty25 };
    yield return new object[] { 0b_000_10_100, ClockOutputFrequency.Frequency3MHz, ClockOutputDutyCycle.Duty50 };
    yield return new object[] { 0b_000_10_101, ClockOutputFrequency.Frequency1500kHz, ClockOutputDutyCycle.Duty50 };
    yield return new object[] { 0b_000_11_110, ClockOutputFrequency.Frequency750kHz, ClockOutputDutyCycle.Duty75 };
    yield return new object[] { 0b_000_11_111, ClockOutputFrequency.Frequency375kHz, ClockOutputDutyCycle.Duty75 };
  }

  [TestCaseSource(nameof(YieldTestCases_ResumeClockOutputSyncOrAsync))]
  public void ResumeClockOutputAsync(
    byte initialChipSetting1,
    ClockOutputFrequency expectedFrequencyOnResume,
    ClockOutputDutyCycle expectedDutyCycleOnResume
  )
    => ResumeClockOutputSyncOrAsync(
      initialChipSetting1,
      expectedFrequencyOnResume,
      expectedDutyCycleOnResume,
      static async gp1 => await gp1.ResumeClockOutputAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ResumeClockOutputSyncOrAsync))]
  public void ResumeClockOutput(
    byte initialChipSetting1,
    ClockOutputFrequency expectedFrequencyOnResume,
    ClockOutputDutyCycle expectedDutyCycleOnResume
  )
    => ResumeClockOutputSyncOrAsync(
      initialChipSetting1,
      expectedFrequencyOnResume,
      expectedDutyCycleOnResume,
      static gp1 => {
        gp1.ResumeClockOutput();
        return default;
      }
    );

  private void ResumeClockOutputSyncOrAsync(
    byte initialChipSetting1,
    ClockOutputFrequency expectedFrequencyOnResume,
    ClockOutputDutyCycle expectedDutyCycleOnResume,
    Func<Gp1Controller, ValueTask> resumeClockOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_0_0_000; // GPIO Function, GPIO Output=0(LOW), GPIO Direction=0(OUTPUT)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp1Settings: InitialGp1Settings,
        chipSetting1: initialChipSetting1
      ),
      shouldDisposeUsbHidDevice: true
    );

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    const byte ExpectedGp1Settings = 0b_000_0_0_001; // Dedicated function operation (CLK OUT)
    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1] don't care
    // [2] Clock Output Divider Value
    expectedSentCommand[2] = initialChipSetting1;
    // [3-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = InitialGp0Settings; // [8] GP0 settings
    expectedSentCommand[9] = ExpectedGp1Settings; // [9] GP1 settings
    expectedSentCommand[10] = InitialGp2Settings; // [10] GP2 settings
    expectedSentCommand[11] = InitialGp3Settings; // [11] GP3 settings

    Assert.That(
      async () => await resumeClockOutputAsyncFunc(mcp2221A.GpPin1),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.ClockOutput));
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputFrequency,
      Is.EqualTo(expectedFrequencyOnResume)
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentClockOutputDutyCycle,
      Is.EqualTo(expectedDutyCycleOnResume)
    );
  }
}
