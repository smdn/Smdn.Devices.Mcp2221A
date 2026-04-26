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
  [Test]
  public void GetModeAsync_Disposed()
    => GetModeSyncOrAsync_Disposed(
      static async gp => { _ = await gp.GetModeAsync().ConfigureAwait(false); }
    );

  [Test]
  public void GetMode_Disposed()
    => GetModeSyncOrAsync_Disposed(
      static gp => {
        _ = gp.GetMode();
        return default;
      }
    );

  private void GetModeSyncOrAsync_Disposed(
    Func<GpController, ValueTask> getModeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    var gpPins = mcp2221A.GpPins.ToList();

    mcp2221A.Dispose();

    foreach (var gp in gpPins) {
      Assert.That(
        async () => await getModeAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void GetModeAsync_CancellationRequested()
    => GetModeSyncOrAsync_CancellationRequested(
      static async (gp, ct) => { _ = await gp.GetModeAsync(ct).ConfigureAwait(false); }
    );

  [Test]
  public void GetMode_CancellationRequested()
    => GetModeSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        _ = gp.GetMode(ct);
        return default;
      }
    );

  private void GetModeSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> getModeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await getModeAsyncFunc(gp, cts.Token),
        Throws
          .InstanceOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  [TestCaseSource(nameof(YieldTestCases_GP0_InvalidConfigurationSettings))]
  public void GetMode_GPO_InvalidConfiguration(byte gp0Settings)
    => GetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: gp0Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_GP1_InvalidConfigurationSettings))]
  public void GetMode_GP1_InvalidConfiguration(byte gp1Settings)
    => GetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: gp1Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_GP2_InvalidConfigurationSettings))]
  public void GetMode_GP2_InvalidConfiguration(byte gp2Settings)
    => GetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: gp2Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_GP3_InvalidConfigurationSettings))]
  public void GetMode_GP3_InvalidConfiguration(byte gp3Settings)
    => GetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: gp3Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void GetModeSyncAndAsync_InvalidConfiguration(
    Func<IUsbHidDevice> createUsbHidDevice,
    Func<Mcp2221AController, GpController> selectGpPin
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      createUsbHidDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var gp = selectGpPin(mcp2221A);

    Assert.That(
      () => _ = gp.GetMode(default),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(gp.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.Gpio)
    );
    Assert.That(
      async () => _ = await gp.GetModeAsync(default),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(gp.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.Gpio)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_GetMode_Read()
  {
    foreach (var (gpPinValueAndDirectionResponse, expectedValue, expectedMode) in new[] {
      ("00-00-", PinValue.Low, PinMode.Output),
      ("00-01-", PinValue.Low, PinMode.Input),
      ("01-00-", PinValue.High, PinMode.Output),
      ("01-01-", PinValue.High, PinMode.Input),
    }) {
      yield return new object[] { gpPinValueAndDirectionResponse, expectedValue, expectedMode, (SelectGpControllerFunc)SelectGp0Controller };
      yield return new object[] { gpPinValueAndDirectionResponse, expectedValue, expectedMode, (SelectGpControllerFunc)SelectGp1Controller };
      yield return new object[] { gpPinValueAndDirectionResponse, expectedValue, expectedMode, (SelectGpControllerFunc)SelectGp2Controller };
      yield return new object[] { gpPinValueAndDirectionResponse, expectedValue, expectedMode, (SelectGpControllerFunc)SelectGp3Controller };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_GetMode_Read))]
  public ValueTask GetMode(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin
  )
    => GetModeSyncOrAsync(
      gpPinValueAndDirectionResponse: gpPinValueAndDirectionResponse,
      expectedValue: expectedValue,
      expectedMode: expectedMode,
      selectGpPin: selectGpPin,
      getModeAsyncFunc: static gp => new(gp.GetMode(default))
    );

  [TestCaseSource(nameof(YieldTestCases_GetMode_Read))]
  public ValueTask GetModeAsync(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin
  )
    => GetModeSyncOrAsync(
      gpPinValueAndDirectionResponse: gpPinValueAndDirectionResponse,
      expectedValue: expectedValue,
      expectedMode: expectedMode,
      selectGpPin: selectGpPin,
      getModeAsyncFunc: static gp => gp.GetModeAsync(default)
    );

  private async ValueTask GetModeSyncOrAsync(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin,
    Func<GpController, ValueTask<PinMode>> getModeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio(
      initialModes: [
        new(0, PinMode.Output),
        new(1, PinMode.Output),
        new(2, PinMode.Output),
        new(3, PinMode.Output),
      ]
    );

    var gp = selectGpPin(mcp2221A);

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      gp.Index == 0 ? gpPinValueAndDirectionResponse : "00-00-", // LOW - OUTPUT
      gp.Index == 1 ? gpPinValueAndDirectionResponse : "00-00-", // LOW - OUTPUT
      gp.Index == 2 ? gpPinValueAndDirectionResponse : "00-00-", // LOW - OUTPUT
      gp.Index == 3 ? gpPinValueAndDirectionResponse : "00-00-", // LOW - OUTPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Assert.That(
      gp.CurrentMode,
      Is.EqualTo(PinMode.Output),
      "initial mode"
    );

    Assert.That(
      await getModeAsyncFunc(gp),
      Is.EqualTo(expectedMode)
    );
    Assert.That(
      gp.CurrentMode,
      Is.EqualTo(expectedMode)
    );
    Assert.That(
      gp.LastUpdatedValue,
      Is.EqualTo(expectedValue)
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.GetMode)}"
    );
  }

  [TestCaseSource(nameof(YieldTestCases_GP0_InvalidConfigurationSettings))]
  public void Read_GPO_InvalidConfiguration(byte gp0Settings)
    => ReadSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: gp0Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_GP1_InvalidConfigurationSettings))]
  public void Read_GP1_InvalidConfiguration(byte gp1Settings)
    => ReadSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: gp1Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_GP2_InvalidConfigurationSettings))]
  public void Read_GP2_InvalidConfiguration(byte gp2Settings)
    => ReadSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: gp2Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_GP3_InvalidConfigurationSettings))]
  public void Read_GP3_InvalidConfiguration(byte gp3Settings)
    => ReadSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: gp3Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void ReadSyncAndAsync_InvalidConfiguration(
    Func<IUsbHidDevice> createUsbHidDevice,
    Func<Mcp2221AController, GpController> selectGpPin
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      createUsbHidDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var gp = selectGpPin(mcp2221A);

    Assert.That(
      () => _ = gp.Read(default),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(gp.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.Gpio)
    );
    Assert.That(
      async () => _ = await gp.ReadAsync(default),
      Throws
        .TypeOf<Mcp2221AConfigurationException>()
        .With
        .Property(nameof(Mcp2221AConfigurationException.GpIndex))
        .EqualTo(gp.Index)
        .And
        .Property(nameof(Mcp2221AConfigurationException.RequiredFunction))
        .EqualTo(GpFunction.Gpio)
    );
  }

  [Test]
  public void ReadAsync_Disposed()
    => ReadSyncOrAsync_Disposed(
      static async gp => { _ = await gp.ReadAsync().ConfigureAwait(false); }
    );

  [Test]
  public void Read_Disposed()
    => ReadSyncOrAsync_Disposed(
      static gp => {
        _ = gp.Read();
        return default;
      }
    );

  private void ReadSyncOrAsync_Disposed(
    Func<GpController, ValueTask> readAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    var gpPins = mcp2221A.GpPins.ToList();

    mcp2221A.Dispose();

    foreach (var gp in gpPins) {
      Assert.That(
        async () => await readAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void ReadAsync_CancellationRequested()
    => ReadSyncOrAsync_CancellationRequested(
      static async (gp, ct) => { _ = await gp.ReadAsync(ct).ConfigureAwait(false); }
    );

  [Test]
  public void Read_CancellationRequested()
    => ReadSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        _ = gp.Read(ct);
        return default;
      }
    );

  private void ReadSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> readAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await readAsyncFunc(gp, cts.Token),
        Throws
          .InstanceOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  [TestCaseSource(nameof(YieldTestCases_GetMode_Read))]
  public ValueTask Read(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin
  )
    => ReadSyncOrAsync(
      gpPinValueAndDirectionResponse: gpPinValueAndDirectionResponse,
      expectedValue: expectedValue,
      expectedMode: expectedMode,
      selectGpPin: selectGpPin,
      readAsyncFunc: static gp => new(gp.Read(default))
    );

  [TestCaseSource(nameof(YieldTestCases_GetMode_Read))]
  public ValueTask ReadAsync(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin
  )
    => ReadSyncOrAsync(
      gpPinValueAndDirectionResponse: gpPinValueAndDirectionResponse,
      expectedValue: expectedValue,
      expectedMode: expectedMode,
      selectGpPin: selectGpPin,
      readAsyncFunc: static gp => gp.ReadAsync(default)
    );

  private async ValueTask ReadSyncOrAsync(
    string gpPinValueAndDirectionResponse,
    PinValue expectedValue,
    PinMode expectedMode,
    SelectGpControllerFunc selectGpPin,
    Func<GpController, ValueTask<PinValue>> readAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio(
      initialModes: [
        new(0, PinMode.Input),
        new(1, PinMode.Input),
        new(2, PinMode.Input),
        new(3, PinMode.Input),
      ],
      initialValues: [
        new(0, PinValue.Low),
        new(1, PinValue.Low),
        new(2, PinValue.Low),
        new(3, PinValue.Low),
      ]
    );

    var gp = selectGpPin(mcp2221A);

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      gp.Index == 0 ? gpPinValueAndDirectionResponse : "00-01-", // LOW - INPUT
      gp.Index == 1 ? gpPinValueAndDirectionResponse : "00-01-", // LOW - INPUT
      gp.Index == 2 ? gpPinValueAndDirectionResponse : "00-01-", // LOW - INPUT
      gp.Index == 3 ? gpPinValueAndDirectionResponse : "00-01-", // LOW - INPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      gp.LastUpdatedValue,
      Is.EqualTo(PinValue.Low),
      "initial value"
    );

    Assert.That(
      await readAsyncFunc(gp),
      Is.EqualTo(expectedValue)
    );
    Assert.That(
      gp.LastUpdatedValue,
      Is.EqualTo(expectedValue)
    );
    Assert.That(
      gp.CurrentMode,
      Is.EqualTo(expectedMode)
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.Read)}"
    );
  }
}
