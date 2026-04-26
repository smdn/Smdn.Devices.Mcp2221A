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
  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsDacSyncOrAsync()
  {
    const byte InitialChipSettings2_DacVrm = 0b_01_1_00111; // DAC: VRM 1.024V; Output = 7
    const byte InitialChipSettings2_DacVdd = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    for (var gpIndex = 2; gpIndex <= 3; gpIndex++) {
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vdd };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.VrmOff };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vrm1024 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vrm2048 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vrm4096 };

      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vdd };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.VrmOff };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vrm1024 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vrm2048 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vrm4096 };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync))]
  public void ConfigureAsDacAsync(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource
  )
    => ConfigureAsDacSyncOrAsync(
      gpIndex,
      initialChipSettings2,
      voltageReferenceSource,
      static async (gp, vr) => await ((IDacController)gp).ConfigureAsDacAsync(voltageReferenceSource: vr).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync))]
  public void ConfigureAsDac(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource
  )
    => ConfigureAsDacSyncOrAsync(
      gpIndex,
      initialChipSettings2,
      voltageReferenceSource,
      static (gp, vr) => {
        ((IDacController)gp).ConfigureAsDac(voltageReferenceSource: vr);
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource,
    Func<GpController, VoltageReferenceSource, ValueTask> configureAsDacAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)

    var initialDacRawValue = initialChipSettings2 & 0b_0_00_11111;

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings2: initialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );

    var expectedAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    if (voltageReferenceSource == VoltageReferenceSource.Vdd) {
      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
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
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62)),
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS (response to the command to re-enable VRM)
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );
    }

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    expectedAssignments[gpIndex] = GpFunction.Dac;

    var expectedVoltageReferenceBits = voltageReferenceSource switch {
      VoltageReferenceSource.Vdd => 0b_0_0000_00_0,
      VoltageReferenceSource.VrmOff => 0b_0_0000_00_1,
      VoltageReferenceSource.Vrm1024 => 0b_0_0000_01_1,
      VoltageReferenceSource.Vrm2048 => 0b_0_0000_10_1,
      VoltageReferenceSource.Vrm4096 => 0b_0_0000_11_1,
      _ => throw new InvalidOperationException(),
    };
    const byte ExpectedDesignationBits = 0b_000_0_0_011; // DAC1/DAC2

    currentGpSettings[gpIndex] = (byte)((currentGpSettings[gpIndex] & 0b_1_1111_00_0) | ExpectedDesignationBits);

    var expectedSentSramSettingsCommand = new byte[64];

    expectedSentSramSettingsCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-2] don't care
    expectedSentSramSettingsCommand[3] = (byte)(0b10000000 | expectedVoltageReferenceBits); // [3] DAC Voltage Reference
    expectedSentSramSettingsCommand[4] = (byte)(0b_0_00_00000 | initialDacRawValue); // [4] Set DAC Output Value
    // [5-6] don't care
    expectedSentSramSettingsCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentSramSettingsCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentSramSettingsCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentSramSettingsCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentSramSettingsCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    var expectedSentReenableVrmCommand = new byte[64];

    expectedSentReenableVrmCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-2] don't care
    expectedSentReenableVrmCommand[3] = (byte)(0b10000000 | expectedVoltageReferenceBits); // [3] DAC Voltage Reference
    expectedSentReenableVrmCommand[4] = (byte)(0b_0_00_00000 | initialDacRawValue); // [4] Set DAC Output Value
    // [5-6] don't care
    expectedSentReenableVrmCommand[7] = 0b00000000; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    expectedSentReenableVrmCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentReenableVrmCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentReenableVrmCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentReenableVrmCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    Assert.That(
      async () => await configureAsDacAsyncFunc(mcp2221A.GpPins[gpIndex], voltageReferenceSource),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, 0),
      SequenceIs.EqualTo(expectedSentSramSettingsCommand)
    );

    if (voltageReferenceSource != VoltageReferenceSource.Vdd) {
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A, 1),
        SequenceIs.EqualTo(expectedSentReenableVrmCommand)
      );
    }

    Assert.That(mcp2221A.CurrentDacReferenceSource, Is.EqualTo(voltageReferenceSource));
    Assert.That(mcp2221A.LastWriteAnalogRawValue, Is.EqualTo(initialDacRawValue));

    Assert.That(mcp2221A.GpPins[gpIndex].CurrentFunction, Is.EqualTo(GpFunction.Dac));
    Assert.That(((IDacController)mcp2221A.GpPins[gpIndex]).CurrentDacReferenceSource, Is.EqualTo(voltageReferenceSource));
    Assert.That(((IDacController)mcp2221A.GpPins[gpIndex]).LastWriteAnalogRawValue, Is.EqualTo(initialDacRawValue));

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(expectedAssignments).AsCollection,
      $"other GP pins must not be configured (except {mcp2221A.GpPins[gpIndex].PinName})"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue()
  {
    const byte InitialChipSettings2_DacVrm = 0b_01_1_00111; // DAC: VRM 1.024V; Output = 7
    const byte InitialChipSettings2_DacVdd = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    for (var gpIndex = 2; gpIndex <= 3; gpIndex++) {
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vdd, 31 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.VrmOff, 1 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVrm, VoltageReferenceSource.Vrm1024, 0 };

      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vdd, 0 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.VrmOff, 31 };
      yield return new object[] { gpIndex, InitialChipSettings2_DacVdd, VoltageReferenceSource.Vrm4096, 1 };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue))]
  public void ConfigureAsDacAsync_WithInitialOutputValue(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue
  )
    => ConfigureAsDacSyncOrAsync_WithInitialOutputValue(
      gpIndex,
      initialChipSettings2,
      voltageReferenceSource,
      initialOutputValue,
      static async (gp, vr, val) => await ((IDacController)gp).ConfigureAsDacAsync(
        voltageReferenceSource: vr,
        initialOutputValue: val
      ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue))]
  public void ConfigureAsDac_WithInitialOutputValue(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue
  )
    => ConfigureAsDacSyncOrAsync_WithInitialOutputValue(
      gpIndex,
      initialChipSettings2,
      voltageReferenceSource,
      initialOutputValue,
      static (gp, vr, val) => {
        ((IDacController)gp).ConfigureAsDac(
          voltageReferenceSource: vr,
          initialOutputValue: val
        );
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_WithInitialOutputValue(
    int gpIndex,
    byte initialChipSettings2,
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue,
    Func<GpController, VoltageReferenceSource, int, ValueTask> configureAsDacAsyncFunc
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
        gp3Settings: InitialGp3Settings,
        chipSettings2: initialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );

    var expectedAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var currentGpSettings = new byte[4] { InitialGp0Settings, InitialGp1Settings, InitialGp2Settings, InitialGp3Settings };

    if (voltageReferenceSource == VoltageReferenceSource.Vdd) {
      Mcp2221AControllerTests.AppendPseudoResponse(
        mcp2221A,
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS
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
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62)),
        // [MCP2221A] 3.1.13 SET SRAM SETTINGS (response to the command to re-enable VRM)
        // [1] 0x00: Command completed successfully
        // [2-63] Don't care
        "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
      );
    }

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    expectedAssignments[gpIndex] = GpFunction.Dac;

    var expectedVoltageReferenceBits = voltageReferenceSource switch {
      VoltageReferenceSource.Vdd => 0b_0_0000_00_0,
      VoltageReferenceSource.VrmOff => 0b_0_0000_00_1,
      VoltageReferenceSource.Vrm1024 => 0b_0_0000_01_1,
      VoltageReferenceSource.Vrm2048 => 0b_0_0000_10_1,
      VoltageReferenceSource.Vrm4096 => 0b_0_0000_11_1,
      _ => throw new InvalidOperationException(),
    };
    const byte ExpectedDesignationBits = 0b_000_0_0_011; // DAC1/DAC2

    currentGpSettings[gpIndex] = (byte)((currentGpSettings[gpIndex] & 0b_1_1111_00_0) | ExpectedDesignationBits);

    var expectedSentSramSettingsCommand = new byte[64];

    expectedSentSramSettingsCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-2] don't care
    expectedSentSramSettingsCommand[3] = (byte)(0b10000000 | expectedVoltageReferenceBits); // [3] DAC Voltage Reference
    expectedSentSramSettingsCommand[4] = (byte)(0b_1_00_00000 | initialOutputValue); // [4] Set DAC Output Value
    // [5-6] don't care
    expectedSentSramSettingsCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentSramSettingsCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentSramSettingsCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentSramSettingsCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentSramSettingsCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    var expectedSentReenableVrmCommand = new byte[64];

    expectedSentReenableVrmCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-2] don't care
    expectedSentReenableVrmCommand[3] = (byte)(0b10000000 | expectedVoltageReferenceBits); // [3] DAC Voltage Reference
    expectedSentReenableVrmCommand[4] = (byte)(0b_1_00_00000 | initialOutputValue); // [4] Set DAC Output Value
    // [5-6] don't care
    expectedSentReenableVrmCommand[7] = 0b00000000; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    expectedSentReenableVrmCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentReenableVrmCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentReenableVrmCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentReenableVrmCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    Assert.That(
      async () => await configureAsDacAsyncFunc(mcp2221A.GpPins[gpIndex], voltageReferenceSource, initialOutputValue),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, 0),
      SequenceIs.EqualTo(expectedSentSramSettingsCommand)
    );

    if (voltageReferenceSource != VoltageReferenceSource.Vdd) {
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A, 1),
        SequenceIs.EqualTo(expectedSentReenableVrmCommand)
      );
    }

    Assert.That(mcp2221A.CurrentDacReferenceSource, Is.EqualTo(voltageReferenceSource));
    Assert.That(mcp2221A.LastWriteAnalogRawValue, Is.EqualTo(initialOutputValue));

    Assert.That(mcp2221A.GpPins[gpIndex].CurrentFunction, Is.EqualTo(GpFunction.Dac));
    Assert.That(((IDacController)mcp2221A.GpPins[gpIndex]).CurrentDacReferenceSource, Is.EqualTo(voltageReferenceSource));
    Assert.That(((IDacController)mcp2221A.GpPins[gpIndex]).LastWriteAnalogRawValue, Is.EqualTo(initialOutputValue));

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(expectedAssignments).AsCollection,
      $"other GP pins must not be configured (except {mcp2221A.GpPins[gpIndex].PinName})"
    );
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedVoltageReferenceSource))]
  public void ConfigureAsDacAsync_UnsupportedVoltageReferenceSource(VoltageReferenceSource voltageReferenceSource)
    => ConfigureAsDacSyncOrAsync_UnsupportedVoltageReferenceSource(
      voltageReferenceSource,
      static async (gp, vr) => await ((IDacController)gp).ConfigureAsDacAsync(voltageReferenceSource: vr).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedVoltageReferenceSource))]
  public void ConfigureAsDac_UnsupportedVoltageReferenceSource(VoltageReferenceSource voltageReferenceSource)
    => ConfigureAsDacSyncOrAsync_UnsupportedVoltageReferenceSource(
      voltageReferenceSource,
      static (gp, vr) => {
        ((IDacController)gp).ConfigureAsDac(voltageReferenceSource: vr);
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_UnsupportedVoltageReferenceSource(
    VoltageReferenceSource voltageReferenceSource,
    Func<GpController, VoltageReferenceSource, ValueTask> configureAsDacAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings2 = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings2: InitialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialDacReferenceSource = mcp2221A.CurrentDacReferenceSource;
    var initialDacRawValue = mcp2221A.LastWriteAnalogRawValue;

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await configureAsDacAsyncFunc(gp, voltageReferenceSource),
        Throws.TypeOf<NotSupportedException>(),
        $"unsupported voltage reference source ({gp.PinName}, {voltageReferenceSource})"
      );

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(initialAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
      );

      Assert.That(
        mcp2221A.CurrentDacReferenceSource,
        Is.EqualTo(initialDacReferenceSource),
        $"must not be configured ({nameof(mcp2221A.CurrentDacReferenceSource)})"
      );
      Assert.That(
        mcp2221A.LastWriteAnalogRawValue,
        Is.EqualTo(initialDacRawValue),
        $"must not be changed ({nameof(mcp2221A.LastWriteAnalogRawValue)})"
      );
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange()
  {
    yield return new object[] { VoltageReferenceSource.Vdd, -1 };
    yield return new object[] { VoltageReferenceSource.VrmOff, int.MinValue };
    yield return new object[] { VoltageReferenceSource.Vrm1024, 32 };
    yield return new object[] { VoltageReferenceSource.Vrm2048, int.MaxValue };
    yield return new object[] { VoltageReferenceSource.Vrm4096, int.MinValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange))]
  public void ConfigureAsDacAsync_WithInitialOutputValue_OutOfRange(
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue
  )
    => ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange(
      voltageReferenceSource,
      initialOutputValue,
      static async (gp, vr, val) => await ((IDacController)gp).ConfigureAsDacAsync(
        voltageReferenceSource: vr,
        initialOutputValue: val
      ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange))]
  public void ConfigureAsDac_WithInitialOutputValue_OutOfRange(
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue
  )
    => ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange(
      voltageReferenceSource,
      initialOutputValue,
      static (gp, vr, val) => {
        ((IDacController)gp).ConfigureAsDac(
          voltageReferenceSource: vr,
          initialOutputValue: val
        );
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_WithInitialOutputValue_OutOfRange(
    VoltageReferenceSource voltageReferenceSource,
    int initialOutputValue,
    Func<GpController, VoltageReferenceSource, int, ValueTask> configureAsDacAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings2 = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings2: InitialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialDacReferenceSource = mcp2221A.CurrentDacReferenceSource;
    var initialDacRawValue = mcp2221A.LastWriteAnalogRawValue;

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await configureAsDacAsyncFunc(gp, voltageReferenceSource, initialOutputValue),
        Throws
          .TypeOf<ArgumentOutOfRangeException>()
          .With
          .Property(nameof(ArgumentOutOfRangeException.ParamName))
          .EqualTo("initialOutputValue")
          .And
          .Property(nameof(ArgumentOutOfRangeException.ActualValue))
          .EqualTo(initialOutputValue),
        $"DAC output value out of range ({gp.PinName}, {initialOutputValue})"
      );

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(initialAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
      );

      Assert.That(
        mcp2221A.CurrentDacReferenceSource,
        Is.EqualTo(initialDacReferenceSource),
        $"must not be configured ({nameof(mcp2221A.CurrentDacReferenceSource)})"
      );
      Assert.That(
        mcp2221A.LastWriteAnalogRawValue,
        Is.EqualTo(initialDacRawValue),
        $"must not be changed ({nameof(mcp2221A.LastWriteAnalogRawValue)})"
      );
    }
  }

  [Test]
  public void ConfigureAsDacAsync_ThrowsWhenUsedByGpioController()
    => ConfigureAsDacSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp => await ((IDacController)gp).ConfigureAsDacAsync(VoltageReferenceSource.Vdd, initialOutputValue: 8).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsDac_ThrowsWhenUsedByGpioController()
    => ConfigureAsDacSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp => {
        ((IDacController)gp).ConfigureAsDac(VoltageReferenceSource.Vdd, initialOutputValue: 8);
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<GpController, ValueTask> configureAsDacAsyncFunc
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

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      // command should not be sent
      // Mcp2221AControllerTests.AppendPseudoResponse(...);
      Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

      Assert.That(
        async () => await configureAsDacAsyncFunc(gp),
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

  [Test]
  public void ConfigureAsDacAsync_CancellationRequested()
    => ConfigureAsDacSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await ((IDacController)gp).ConfigureAsDacAsync(VoltageReferenceSource.Vdd, cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsDac_CancellationRequested()
    => ConfigureAsDacSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        ((IDacController)gp).ConfigureAsDac(VoltageReferenceSource.Vdd, cancellationToken: ct);
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> configureAsDacAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings2 = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings,
        chipSettings2: InitialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialDacReferenceSource = mcp2221A.CurrentDacReferenceSource;
    var initialDacRawValue = mcp2221A.LastWriteAnalogRawValue;

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await configureAsDacAsyncFunc(gp, cts.Token),
        Throws
          .InstanceOf<OperationCanceledException>()
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

      Assert.That(
        mcp2221A.CurrentDacReferenceSource,
        Is.EqualTo(initialDacReferenceSource),
        $"must not be configured ({nameof(mcp2221A.CurrentDacReferenceSource)})"
      );
      Assert.That(
        mcp2221A.LastWriteAnalogRawValue,
        Is.EqualTo(initialDacRawValue),
        $"must not be changed ({nameof(mcp2221A.LastWriteAnalogRawValue)})"
      );
    }
  }

  [Test]
  public void ConfigureAsDacAsync_Disposed()
    => ConfigureAsDacSyncOrAsync_Disposed(
      static async gp => await ((IDacController)gp).ConfigureAsDacAsync(VoltageReferenceSource.Vdd).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsDac_Disposed()
    => ConfigureAsDacSyncOrAsync_Disposed(
      static gp => {
        ((IDacController)gp).ConfigureAsDac(VoltageReferenceSource.Vdd);
        return default;
      }
    );

  private void ConfigureAsDacSyncOrAsync_Disposed(
    Func<GpController, ValueTask> configureAsDacAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var dacPins = new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 };

    mcp2221A.Dispose();

    foreach (var gp in dacPins) {
      Assert.That(
        async () => await configureAsDacAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  private static IEnumerable<byte> YieldTestCases_WriteAnalogRawSyncAndAsync_GP2_InvalidConfiguration()
  {
    yield return 0b_000_1_0_000; // GPIO2
    yield return 0b_000_1_0_010; // ADC2
    yield return 0b_000_1_0_001; // USBCFG
  }

  private static IEnumerable<byte> YieldTestCases_WriteAnalogRawSyncAndAsync_GP3_InvalidConfiguration()
  {
    yield return 0b_000_1_0_000; // GPIO3
    yield return 0b_000_1_0_010; // ADC3
    yield return 0b_000_1_0_001; // LED_I2C
  }

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncAndAsync_GP2_InvalidConfiguration))]
  public void WriteAnalogRawAsync_GP2_InvalidConfiguration(byte gp2Settings)
    => WriteAnalogRawSyncAndAsync_InvalidConfiguration(
      gp2Settings: gp2Settings,
      gp3Settings: null,
      expectedDacPinNumberInExceptionMessage: 2,
      static async mcp2221a => await mcp2221a.GpPin2.WriteAnalogRawAsync(0).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncAndAsync_GP2_InvalidConfiguration))]
  public void WriteAnalogRaw_GP2_InvalidConfiguration(byte gp2Settings)
    => WriteAnalogRawSyncAndAsync_InvalidConfiguration(
      gp2Settings: gp2Settings,
      gp3Settings: null,
      expectedDacPinNumberInExceptionMessage: 2,
      static mcp2221a => {
        mcp2221a.GpPin2.WriteAnalogRaw(0);
        return default;
      }
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncAndAsync_GP3_InvalidConfiguration))]
  public void WriteAnalogRawAsync_GP3_InvalidConfiguration(byte gp3Settings)
    => WriteAnalogRawSyncAndAsync_InvalidConfiguration(
      gp2Settings: null,
      gp3Settings: gp3Settings,
      expectedDacPinNumberInExceptionMessage: 3,
      static async mcp2221a => await mcp2221a.GpPin3.WriteAnalogRawAsync(0).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncAndAsync_GP3_InvalidConfiguration))]
  public void WriteAnalogRaw_GP3_InvalidConfiguration(byte gp3Settings)
    => WriteAnalogRawSyncAndAsync_InvalidConfiguration(
      gp2Settings: null,
      gp3Settings: gp3Settings,
      expectedDacPinNumberInExceptionMessage: 3,
      static mcp2221a => {
        mcp2221a.GpPin3.WriteAnalogRaw(0);
        return default;
      }
    );

  private void WriteAnalogRawSyncAndAsync_InvalidConfiguration(
    byte? gp2Settings,
    byte? gp3Settings,
    int expectedDacPinNumberInExceptionMessage,
    Func<Mcp2221AController, ValueTask> writeAnalogRawAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings2 = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: gp2Settings ?? InitialGp2Settings,
        gp3Settings: gp3Settings ?? InitialGp3Settings,
        chipSettings2: InitialChipSettings2
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialDacRawValue = mcp2221A.LastWriteAnalogRawValue;

    Assert.That(
      async () => await writeAnalogRawAsyncFunc(mcp2221A),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{expectedDacPinNumberInExceptionMessage}")
    );

    Assert.That(
      mcp2221A.LastWriteAnalogRawValue,
      Is.EqualTo(initialDacRawValue),
      $"must not be changed ({nameof(mcp2221A.LastWriteAnalogRawValue)})"
    );
  }

  [Test]
  public void WriteAnalogRawAsync_Disposed()
    => WriteAnalogRawSyncOrAsync_Disposed(
      static async gp => await ((IDacController)gp).WriteAnalogRawAsync(0).ConfigureAwait(false)
    );

  [Test]
  public void WriteAnalogRaw_Disposed()
    => WriteAnalogRawSyncOrAsync_Disposed(
      static gp => {
        ((IDacController)gp).WriteAnalogRaw(0);
        return default;
      }
    );

  private void WriteAnalogRawSyncOrAsync_Disposed(
    Func<GpController, ValueTask> writeAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsDac();
    var dacPins = new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 };

    mcp2221A.Dispose();

    foreach (var gp in dacPins) {
      Assert.That(
        async () => await writeAnalogRawAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void WriteAnalogRawAsync_CancellationRequested()
    => WriteAnalogRawSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await ((IDacController)gp).WriteAnalogRawAsync(0, ct).ConfigureAwait(false)
    );

  [Test]
  public void WriteAnalogRaw_CancellationRequested()
    => WriteAnalogRawSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        ((IDacController)gp).WriteAnalogRaw(0, ct);
        return default;
      }
    );

  private void WriteAnalogRawSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> writeAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsDac();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await writeAnalogRawAsyncFunc(gp, cts.Token),
        Throws
          .InstanceOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteAnalogRawSyncOrAsync_ValueOutOfRange()
  {
    yield return new object[] { int.MinValue };
    yield return new object[] { -1 };
    yield return new object[] { 32 };
    yield return new object[] { int.MaxValue };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync_ValueOutOfRange))]
  public void WriteAnalogRawAsync_ValueOutOfRange(int value)
    => WriteAnalogRawSyncOrAsync_ValueOutOfRange(
      value,
      static async (gp, val) => await ((IDacController)gp).WriteAnalogRawAsync(val).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync_ValueOutOfRange))]
  public void WriteAnalogRaw_ValueOutOfRange(int value)
    => WriteAnalogRawSyncOrAsync_ValueOutOfRange(
      value,
      static (gp, val) => {
        ((IDacController)gp).WriteAnalogRaw(val);
        return default;
      }
    );

  private void WriteAnalogRawSyncOrAsync_ValueOutOfRange(
    int value,
    Func<GpController, int, ValueTask> writeAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsDac();
    var initialDacRawValue = mcp2221A.LastWriteAnalogRawValue;

    foreach (var gp in new GpController[] { mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await writeAnalogRawAsyncFunc(gp, value),
        Throws
          .TypeOf<ArgumentOutOfRangeException>()
          .With
          .Property(nameof(ArgumentOutOfRangeException.ParamName))
          .EqualTo("value")
          .And
          .Property(nameof(ArgumentOutOfRangeException.ActualValue))
          .EqualTo(value),
        $"DAC output value out of range ({gp.PinName}, {value})"
      );

      Assert.That(
        mcp2221A.LastWriteAnalogRawValue,
        Is.EqualTo(initialDacRawValue),
        $"must not be changed ({nameof(mcp2221A.LastWriteAnalogRawValue)})"
      );
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteAnalogRawSyncOrAsync()
  {
    const byte InitialChipSettings2_DacVrm = 0b_01_1_00111; // DAC: VRM 1.024V; Output = 7
    const byte InitialChipSettings2_DacVdd = 0b_10_0_01000; // DAC: VDD(VRM 2.048V); Output = 8 (factory default)

    yield return new object[] { InitialChipSettings2_DacVrm, 0 };
    yield return new object[] { InitialChipSettings2_DacVdd, 0 };

    yield return new object[] { InitialChipSettings2_DacVrm, 1 };
    yield return new object[] { InitialChipSettings2_DacVdd, 30 };

    yield return new object[] { InitialChipSettings2_DacVrm, 31 };
    yield return new object[] { InitialChipSettings2_DacVdd, 31 };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync))]
  public ValueTask WriteAnalogRawAsync_GP2(
    byte chipSettings2,
    int value
  )
    => WriteAnalogRawSyncOrAsync(
      chipSettings2: chipSettings2,
      value: value,
      static async (mcp2221a, value) => await mcp2221a.GpPin2.WriteAnalogRawAsync(value).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync))]
  public ValueTask WriteAnalogRaw_GP2(
    byte chipSettings2,
    int value
  )
    => WriteAnalogRawSyncOrAsync(
      chipSettings2: chipSettings2,
      value: value,
      static (mcp2221a, value) => {
        mcp2221a.GpPin2.WriteAnalogRaw(value);
        return default;
      }
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync))]
  public ValueTask WriteAnalogRawAsync_GP3(
    byte chipSettings2,
    int value
  )
    => WriteAnalogRawSyncOrAsync(
      chipSettings2: chipSettings2,
      value: value,
      static async (mcp2221a, value) => await mcp2221a.GpPin3.WriteAnalogRawAsync(value).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteAnalogRawSyncOrAsync))]
  public ValueTask WriteAnalogRaw_GP3(
    byte chipSettings2,
    int value
  )
    => WriteAnalogRawSyncOrAsync(
      chipSettings2: chipSettings2,
      value: value,
      static (mcp2221a, value) => {
        mcp2221a.GpPin2.WriteAnalogRaw(value);
        return default;
      }
    );

  private async ValueTask WriteAnalogRawSyncOrAsync(
    byte chipSettings2,
    int value,
    Func<Mcp2221AController, int, ValueTask> writeAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsDac(chipSettings2);

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );

    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[8];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-2] don't care
    expectedSentCommand[3] = (byte)(chipSettings2 >> 5); // [3] DAC Voltage Reference
    expectedSentCommand[4] = (byte)(0b_1_00_00000 | value); // [4] Set DAC Output Value
    // [5-6] don't care
    expectedSentCommand[7] = 0; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    // [8-11] GP0-GP3 settings: No assertions are to be made in this test case.

    Assert.That(
      async () => await writeAnalogRawAsyncFunc(mcp2221A, value),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, 0).Slice(0, expectedSentCommand.Length),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.LastWriteAnalogRawValue, Is.EqualTo(value));

    Assert.That(((IDacController)mcp2221A.GpPin2).LastWriteAnalogRawValue, Is.EqualTo(value));
    Assert.That(((IDacController)mcp2221A.GpPin3).LastWriteAnalogRawValue, Is.EqualTo(value));
  }
}
