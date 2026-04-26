// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.IO.UsbHid;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpControllerTests {
#pragma warning restore IDE0040
  private static System.Collections.IEnumerable YieldTestCases_ConfigureAsAdcSyncOrAsync()
  {
    const byte InitialChipSettings3_AdcVrm = 0b_0_1_1_01_1_00; // INTDETFEEN: 1, INTDETREEN: 1, ADCVRM: 01(1.024V), ADCREF: 1(VRM) (factory default)
    const byte InitialChipSettings3_AdcVdd = 0b_0_0_0_00_0_00; // INTDETFEEN: 0, INTDETREEN: 0, ADCVRM: 00(Off), ADCREF: 0(Vdd)

    for (var gpIndex = 1; gpIndex <= 3; gpIndex++) {
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, VoltageReferenceSource.Vdd };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, VoltageReferenceSource.VrmOff };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, VoltageReferenceSource.Vrm1024 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, VoltageReferenceSource.Vrm2048 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, VoltageReferenceSource.Vrm4096 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVrm, null };

      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, VoltageReferenceSource.Vdd };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, VoltageReferenceSource.VrmOff };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, VoltageReferenceSource.Vrm1024 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, VoltageReferenceSource.Vrm2048 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, VoltageReferenceSource.Vrm4096 };
      yield return new object?[] { gpIndex, InitialChipSettings3_AdcVdd, null };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsAdcSyncOrAsync))]
  public void ConfigureAsAdcAsync(
    int gpIndex,
    byte initialChipSettings3,
    VoltageReferenceSource? voltageReferenceSource
  )
    => ConfigureAsAdcSyncOrAsync(
      gpIndex,
      initialChipSettings3,
      voltageReferenceSource,
      static async (gp, vr) => await ((IAdcController)gp).ConfigureAsAdcAsync(voltageReferenceSource: vr).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAsAdcSyncOrAsync))]
  public void ConfigureAsAdc(
    int gpIndex,
    byte initialChipSettings3,
    VoltageReferenceSource? voltageReferenceSource
  )
    => ConfigureAsAdcSyncOrAsync(
      gpIndex,
      initialChipSettings3,
      voltageReferenceSource,
      static (gp, vr) => {
        ((IAdcController)gp).ConfigureAsAdc(voltageReferenceSource: vr);
        return default;
      }
    );

  private void ConfigureAsAdcSyncOrAsync(
    int gpIndex,
    byte initialChipSettings3,
    VoltageReferenceSource? voltageReferenceSource,
    Func<GpController, VoltageReferenceSource?, ValueTask> configureAsAdcAsyncFunc
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
        chipSettings3: initialChipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );
    var initialAdcReferenceSource = mcp2221A.CurrentAdcReferenceSource;
    var expectedInterruptOnChangeBits = (byte)(
      ((initialChipSettings3 & 0b_0_0_1_00_0_00) == 0 ? 0 : 0b_0_00_0_1_0_0_0) | // positive edge
      ((initialChipSettings3 & 0b_0_1_0_00_0_00) == 0 ? 0 : 0b_0_00_0_0_0_1_0) // negative edge
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

    expectedAssignments[gpIndex] = GpFunction.Adc;

    var expectedVoltageReferenceBits = voltageReferenceSource switch {
      VoltageReferenceSource.Vdd => 0b_0_0000_00_0,
      VoltageReferenceSource.VrmOff => 0b_0_0000_00_1,
      VoltageReferenceSource.Vrm1024 => 0b_0_0000_01_1,
      VoltageReferenceSource.Vrm2048 => 0b_0_0000_10_1,
      VoltageReferenceSource.Vrm4096 => 0b_0_0000_11_1,
      null => (initialChipSettings3 & 0b_0_0_0_11_1_00) >> 2,
      _ => throw new InvalidOperationException(),
    };
    const byte ExpectedDesignationBits = 0b_000_0_0_010; // ADC1/ADC2/ADC3

    currentGpSettings[gpIndex] = (byte)((currentGpSettings[gpIndex] & 0b_1_1111_00_0) | ExpectedDesignationBits);

    var expectedSentSramSettingsCommand = new byte[64];

    expectedSentSramSettingsCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-4] don't care
    // [5] ADC Voltage Reference
    expectedSentSramSettingsCommand[5] = (byte)(
      (voltageReferenceSource.HasValue ? 0b10000000 : 0b00000000) |
      expectedVoltageReferenceBits
    );
    expectedSentSramSettingsCommand[6] = expectedInterruptOnChangeBits; // [6] Set Up the Interrupt Detection Mechanism and Clear the Detection Flag
    expectedSentSramSettingsCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentSramSettingsCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentSramSettingsCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentSramSettingsCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentSramSettingsCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    var expectedSentReenableVrmCommand = new byte[64];

    expectedSentReenableVrmCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-4] don't care
    expectedSentReenableVrmCommand[5] = (byte)(0b10000000 | expectedVoltageReferenceBits); // [5] ADC Voltage Reference
    expectedSentReenableVrmCommand[6] = expectedInterruptOnChangeBits; // [6] Set Up the Interrupt Detection Mechanism and Clear the Detection Flag
    expectedSentReenableVrmCommand[7] = 0b00000000; // [7] Alter GPIO configuration = Do not alter the current GP designation (0)
    expectedSentReenableVrmCommand[8] = currentGpSettings[0]; // [8] GP0 settings
    expectedSentReenableVrmCommand[9] = currentGpSettings[1]; // [9] GP1 settings
    expectedSentReenableVrmCommand[10] = currentGpSettings[2]; // [10] GP2 settings
    expectedSentReenableVrmCommand[11] = currentGpSettings[3]; // [11] GP3 settings

    Assert.That(
      async () => await configureAsAdcAsyncFunc(mcp2221A.GpPins[gpIndex], voltageReferenceSource),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A, 0),
      SequenceIs.EqualTo(expectedSentSramSettingsCommand)
    );

    if ((voltageReferenceSource ?? initialAdcReferenceSource) != VoltageReferenceSource.Vdd) {
      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A, 1),
        SequenceIs.EqualTo(expectedSentReenableVrmCommand)
      );
    }

    Assert.That(
      mcp2221A.CurrentAdcReferenceSource,
      Is.EqualTo(voltageReferenceSource ?? initialAdcReferenceSource)
    );

    Assert.That(mcp2221A.GpPins[gpIndex].CurrentFunction, Is.EqualTo(GpFunction.Adc));
    Assert.That(
      ((IAdcController)mcp2221A.GpPins[gpIndex]).CurrentAdcReferenceSource,
      Is.EqualTo(voltageReferenceSource ?? initialAdcReferenceSource)
    );

    Assert.That(
      mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
      Is.EqualTo(expectedAssignments).AsCollection,
      $"other GP pins must not be configured (except {mcp2221A.GpPins[gpIndex].PinName})"
    );
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedVoltageReferenceSource))]
  public void ConfigureAsAdcAsync_UnsupportedVoltageReferenceSource(VoltageReferenceSource voltageReferenceSource)
    => ConfigureAsAdcSyncOrAsync_UnsupportedVoltageReferenceSource(
      voltageReferenceSource,
      static async (gp, vr) => await ((IAdcController)gp).ConfigureAsAdcAsync(voltageReferenceSource: vr).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedVoltageReferenceSource))]
  public void ConfigureAsAdc_UnsupportedVoltageReferenceSource(VoltageReferenceSource voltageReferenceSource)
    => ConfigureAsAdcSyncOrAsync_UnsupportedVoltageReferenceSource(
      voltageReferenceSource,
      static (gp, vr) => {
        ((IAdcController)gp).ConfigureAsAdc(voltageReferenceSource: vr);
        return default;
      }
    );

  private void ConfigureAsAdcSyncOrAsync_UnsupportedVoltageReferenceSource(
    VoltageReferenceSource voltageReferenceSource,
    Func<GpController, VoltageReferenceSource, ValueTask> configureAsAdcAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings3 = 0b_0_1_1_01_1_00; // ADC: VRM 1.024V (factory default)

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
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialAdcReferenceSource = mcp2221A.CurrentAdcReferenceSource;

    foreach (var gp in new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await configureAsAdcAsyncFunc(gp, voltageReferenceSource),
        Throws.TypeOf<NotSupportedException>(),
        $"unsupported voltage reference source ({gp.PinName}, {voltageReferenceSource})"
      );

      Assert.That(
        mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList(),
        Is.EqualTo(initialAssignments).AsCollection,
        $"must not be configured ({gp.PinName})"
      );

      Assert.That(
        mcp2221A.CurrentAdcReferenceSource,
        Is.EqualTo(initialAdcReferenceSource),
        $"must not be configured ({nameof(mcp2221A.CurrentAdcReferenceSource)})"
      );
    }
  }


  [Test]
  public void ConfigureAsAdcAsync_ThrowsWhenUsedByGpioController()
    => ConfigureAsAdcSyncOrAsync_ThrowsWhenUsedByGpioController(
      static async gp => await ((IAdcController)gp).ConfigureAsAdcAsync(VoltageReferenceSource.Vdd).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsAdc_ThrowsWhenUsedByGpioController()
    => ConfigureAsAdcSyncOrAsync_ThrowsWhenUsedByGpioController(
      static gp => {
        ((IAdcController)gp).ConfigureAsAdc(VoltageReferenceSource.Vdd);
        return default;
      }
    );

  private void ConfigureAsAdcSyncOrAsync_ThrowsWhenUsedByGpioController(
    Func<GpController, ValueTask> configureAsAdcAsyncFunc
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

    foreach (var gp in new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
       // command should not be sent
      // Mcp2221AControllerTests.AppendPseudoResponse(...);
      Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

      Assert.That(
        async () => await configureAsAdcAsyncFunc(gp),
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
  public void ConfigureAsAdcAsync_CancellationRequested()
    => ConfigureAsAdcSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await ((IAdcController)gp).ConfigureAsAdcAsync(VoltageReferenceSource.Vdd, cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsAdc_CancellationRequested()
    => ConfigureAsAdcSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        ((IAdcController)gp).ConfigureAsAdc(VoltageReferenceSource.Vdd, cancellationToken: ct);
        return default;
      }
    );

  private void ConfigureAsAdcSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> configureAsAdcAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings3 = 0b_0_1_1_01_1_00; // ADC: VRM 1.024V (factory default)

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
    var initialAssignments = mcp2221A.GpPins.Select(static gp => gp.CurrentFunction).ToList();
    var initialAdcReferenceSource = mcp2221A.CurrentAdcReferenceSource;

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await configureAsAdcAsyncFunc(gp, cts.Token),
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
        mcp2221A.CurrentAdcReferenceSource,
        Is.EqualTo(initialAdcReferenceSource),
        $"must not be configured ({nameof(mcp2221A.CurrentAdcReferenceSource)})"
      );
    }
  }

  [Test]
  public void ConfigureAsAdcAsync_Disposed()
    => ConfigureAsAdcSyncOrAsync_Disposed(
      static async gp => await ((IAdcController)gp).ConfigureAsAdcAsync(VoltageReferenceSource.Vdd).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAsAdc_Disposed()
    => ConfigureAsAdcSyncOrAsync_Disposed(
      static gp => {
        ((IAdcController)gp).ConfigureAsAdc(VoltageReferenceSource.Vdd);
        return default;
      }
    );

  private void ConfigureAsAdcSyncOrAsync_Disposed(
    Func<GpController, ValueTask> configureAsAdcAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var adcPins = new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 };

    mcp2221A.Dispose();

    foreach (var gp in adcPins) {
      Assert.That(
        async () => await configureAsAdcAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  private static IEnumerable<byte> YieldTestCases_ReadAnalogRawSyncAndAsync_GP1_InvalidConfiguration()
  {
    yield return 0b_000_1_0_000; // GPIO1
    yield return 0b_000_1_0_100; // IOC
    yield return 0b_000_1_0_011; // LED_UTX
    yield return 0b_000_1_0_001; // CLK OUT
  }

  private static IEnumerable<byte> YieldTestCases_ReadAnalogRawSyncAndAsync_GP2_InvalidConfiguration()
  {
    yield return 0b_000_1_0_000; // GPIO2
    yield return 0b_000_1_0_011; // DAC1
    yield return 0b_000_1_0_001; // USBCFG
  }

  private static IEnumerable<byte> YieldTestCases_ReadAnalogRawSyncAndAsync_GP3_InvalidConfiguration()
  {
    yield return 0b_000_1_0_000; // GPIO3
    yield return 0b_000_1_0_011; // DAC2
    yield return 0b_000_1_0_001; // LED_I2C
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP1_InvalidConfiguration))]
  public void ReadAnalogRawAsync_GP1_InvalidConfiguration(byte gp1Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      gp2Settings: null,
      gp3Settings: null,
      expectedAdcPinNumberInExceptionMessage: 1,
      static async mcp2221a => _ = await mcp2221a.GpPin1.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP1_InvalidConfiguration))]
  public void ReadAnalogRaw_GP1_InvalidConfiguration(byte gp1Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: gp1Settings,
      gp2Settings: null,
      gp3Settings: null,
      expectedAdcPinNumberInExceptionMessage: 1,
      static mcp2221a => {
        _ = mcp2221a.GpPin1.ReadAnalogRaw();
        return default;
      }
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP2_InvalidConfiguration))]
  public void ReadAnalogRawAsync_GP2_InvalidConfiguration(byte gp2Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: null,
      gp2Settings: gp2Settings,
      gp3Settings: null,
      expectedAdcPinNumberInExceptionMessage: 2,
      static async mcp2221a => _ = await mcp2221a.GpPin2.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP2_InvalidConfiguration))]
  public void ReadAnalogRaw_GP2_InvalidConfiguration(byte gp2Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: null,
      gp2Settings: gp2Settings,
      gp3Settings: null,
      expectedAdcPinNumberInExceptionMessage: 2,
      static mcp2221a => {
        _ = mcp2221a.GpPin2.ReadAnalogRaw();
        return default;
      }
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP3_InvalidConfiguration))]
  public void ReadAnalogRawAsync_GP3_InvalidConfiguration(byte gp3Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: null,
      gp2Settings: null,
      gp3Settings: gp3Settings,
      expectedAdcPinNumberInExceptionMessage: 3,
      static async mcp2221a => _ = await mcp2221a.GpPin3.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncAndAsync_GP3_InvalidConfiguration))]
  public void ReadAnalogRaw_GP3_InvalidConfiguration(byte gp3Settings)
    => ReadAnalogRawSyncAndAsync_InvalidConfiguration(
      gp1Settings: null,
      gp2Settings: null,
      gp3Settings: gp3Settings,
      expectedAdcPinNumberInExceptionMessage: 3,
      static mcp2221a => {
        _ = mcp2221a.GpPin3.ReadAnalogRaw();
        return default;
      }
    );

  private void ReadAnalogRawSyncAndAsync_InvalidConfiguration(
    byte? gp1Settings,
    byte? gp2Settings,
    byte? gp3Settings,
    int expectedAdcPinNumberInExceptionMessage,
    Func<Mcp2221AController, ValueTask> readAnalogRawAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_010; // Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_1_0_001; // Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_1_0_001; // Dedicated function operation (LED I2C)
    const byte InitialChipSettings3 = 0b_0_1_1_01_1_00; // ADC: VRM 1.024V (factory default)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: gp1Settings ?? InitialGp1Settings,
        gp2Settings: gp2Settings ?? InitialGp2Settings,
        gp3Settings: gp3Settings ?? InitialGp3Settings,
        chipSettings3: InitialChipSettings3
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      async () => await readAnalogRawAsyncFunc(mcp2221A),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{expectedAdcPinNumberInExceptionMessage}")
    );
  }

  [Test]
  public void ReadAnalogRawAsync_Disposed()
    => ReadAnalogRawSyncOrAsync_Disposed(
      static async gp => { _ = await ((IAdcController)gp).ReadAnalogRawAsync().ConfigureAwait(false); }
    );

  [Test]
  public void ReadAnalogRaw_Disposed()
    => ReadAnalogRawSyncOrAsync_Disposed(
      static gp => {
        _ = ((IAdcController)gp).ReadAnalogRaw();
        return default;
      }
    );

  private void ReadAnalogRawSyncOrAsync_Disposed(
    Func<GpController, ValueTask> readAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsAdc();
    var adcPins = new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 };

    mcp2221A.Dispose();

    foreach (var gp in adcPins) {
      Assert.That(
        async () => await readAnalogRawAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void ReadAnalogRawAsync_CancellationRequested()
    => ReadAnalogRawSyncOrAsync_CancellationRequested(
      static async (gp, ct) => { _ = await ((IAdcController)gp).ReadAnalogRawAsync(ct).ConfigureAwait(false); }
    );

  [Test]
  public void ReadAnalogRaw_CancellationRequested()
    => ReadAnalogRawSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        _ = ((IAdcController)gp).ReadAnalogRaw(ct);
        return default;
      }
    );

  private void ReadAnalogRawSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> readAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsAdc();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in new GpController[] { mcp2221A.GpPin1, mcp2221A.GpPin2, mcp2221A.GpPin3 }) {
      Assert.That(
        async () => await readAnalogRawAsyncFunc(gp, cts.Token),
        Throws
          .InstanceOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadAnalogRawSyncOrAsync()
  {
    yield return new object[] { "00-00-", "00-00-", "00-00-", 0x_00_00, 0x_00_00, 0x_00_00 };
    yield return new object[] { "FF-03-", "FF-03-", "FF-03-", 0x_03_FF, 0x_03_FF, 0x_03_FF };
    yield return new object[] { "23-01-", "46-02-", "69-03-", 0x_01_23, 0x_02_46, 0x_03_69 };

    yield return new object[] { "00-03-", "00-00-", "00-00-", 0x_03_00, 0x_00_00, 0x_00_00 };
    yield return new object[] { "00-00-", "00-03-", "00-00-", 0x_00_00, 0x_03_00, 0x_00_00 };
    yield return new object[] { "00-00-", "00-00-", "00-03-", 0x_00_00, 0x_00_00, 0x_03_00 };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRawAsync_GP1(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc1RawValue,
      static async mcp2221a => await mcp2221a.GpPin1.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRaw_GP1(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc1RawValue,
      static mcp2221a => new(mcp2221a.GpPin1.ReadAnalogRaw())
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRawAsync_GP2(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc2RawValue,
      static async mcp2221a => await mcp2221a.GpPin2.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRaw_GP2(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc2RawValue,
      static mcp2221a => new(mcp2221a.GpPin2.ReadAnalogRaw())
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRawAsync_GP3(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc3RawValue,
      static async mcp2221a => await mcp2221a.GpPin3.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public ValueTask ReadAnalogRaw_GP3(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => ReadAnalogRawSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdcRawValue: expectedAdc3RawValue,
      static mcp2221a => new(mcp2221a.GpPin3.ReadAnalogRaw())
    );

  private async ValueTask ReadAnalogRawSyncOrAsync(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdcRawValue,
    Func<Mcp2221AController, ValueTask<int>> readAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsAdc();

    // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
    var statusSetParametersResponse = string.Concat(
      "10-00-",
      string.Join("-", Enumerable.Repeat("00", 50 - 2)), "-",
      // [50-55] ADC Data (16-bit) values
      adcChannel0Response, // [50] LSB [51] MSB of ADC CH0
      adcChannel1Response, // [52] LSB [53] MSB of ADC CH1
      adcChannel2Response, // [54] LSB [55] MSB of ADC CH2
      string.Join("-", Enumerable.Repeat("00", 64 - 56))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, statusSetParametersResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x10; // STATUS/SET PARAMETERS

    Assert.That(
      await readAnalogRawAsyncFunc(mcp2221A),
      Is.EqualTo(expectedAdcRawValue)
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(IAdcController.ReadAnalogRaw)}"
    );
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public void LastReadAnalogRawValue_AfterReadAnalogRawAsync(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => LastReadAnalogRawValue(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static async gp => { _ = await ((IAdcController)gp).ReadAnalogRawAsync().ConfigureAwait(false); }
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public void LastReadAnalogRawValue_AfterReadAnalogRaw(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue
  )
    => LastReadAnalogRawValue(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static gp => {
        _ = ((IAdcController)gp).ReadAnalogRaw();
        return default;
      }
    );

  private void LastReadAnalogRawValue(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue,
    Func<GpController, ValueTask> readAnalogRawAsyncFunc
  )
  {
    for (var gpIndex = 1; gpIndex <= 3; gpIndex++) {
      using var mcp2221A = CreateMcp2221AConfiguredAsAdc();
      var gp = mcp2221A.GpPins[gpIndex];

      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      var statusSetParametersResponse = string.Concat(
        "10-00-",
        string.Join("-", Enumerable.Repeat("00", 50 - 2)), "-",
        // [50-55] ADC Data (16-bit) values
        adcChannel0Response, // [50] LSB [51] MSB of ADC CH0
        adcChannel1Response, // [52] LSB [53] MSB of ADC CH1
        adcChannel2Response, // [54] LSB [55] MSB of ADC CH2
        string.Join("-", Enumerable.Repeat("00", 64 - 56))
      );

      Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, statusSetParametersResponse);
      Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

      var expectedSentCommand = new byte[64]; // [1-64]: don't care

      expectedSentCommand[0] = 0x10; // STATUS/SET PARAMETERS

      Assert.That(
        async () => await readAnalogRawAsyncFunc(gp),
        Throws.Nothing,
        $"ReadAnalogRaw {gp.PinName}"
      );

      Assert.That(
        Mcp2221AControllerTests.GetSentCommand(mcp2221A),
        SequenceIs.EqualTo(expectedSentCommand),
        $"sent command from {nameof(IAdcController.ReadAnalogRaw)} ({gp.PinName})"
      );

      Assert.That(
        mcp2221A.GpPin1.LastReadAnalogRawValue,
        Is.EqualTo(expectedAdc1RawValue)
      );
      Assert.That(
        mcp2221A.GpPin2.LastReadAnalogRawValue,
        Is.EqualTo(expectedAdc2RawValue)
      );
      Assert.That(
        mcp2221A.GpPin3.LastReadAnalogRawValue,
        Is.EqualTo(expectedAdc3RawValue)
      );
    }
  }
}
