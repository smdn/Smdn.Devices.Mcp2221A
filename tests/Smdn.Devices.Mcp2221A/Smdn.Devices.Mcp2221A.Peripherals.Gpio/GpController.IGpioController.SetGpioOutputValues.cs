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
  [TestCaseSource(nameof(YieldTestCases_GP0_InvalidConfigurationSettings))]
  public void SetMode_GPO_InvalidConfiguration(byte gp0Settings)
    => SetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp0Settings: gp0Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_GP1_InvalidConfigurationSettings))]
  public void SetMode_GP1_InvalidConfiguration(byte gp1Settings)
    => SetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp1Settings: gp1Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_GP2_InvalidConfigurationSettings))]
  public void SetMode_GP2_InvalidConfiguration(byte gp2Settings)
    => SetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp2Settings: gp2Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_GP3_InvalidConfigurationSettings))]
  public void SetMode_GP3_InvalidConfiguration(byte gp3Settings)
    => SetModeSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp3Settings: gp3Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void SetModeSyncAndAsync_InvalidConfiguration(
    Func<IUsbHidDevice> createUsbHidDevice,
    Func<Mcp2221A, GpController> selectGpPin
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      createUsbHidDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var gp = selectGpPin(mcp2221A);

    Assert.That(
      () => gp.SetMode(default),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{gp.Index}")
    );
    Assert.That(
      async () => await gp.SetModeAsync(default),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{gp.Index}")
    );
  }

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
  public void SetMode_GPO_UnsupportedPinMode(PinMode mode)
    => SetModeSyncAndAsync_UnsupportedPinMode(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
  public void SetMode_GP1_UnsupportedPinMode(PinMode mode)
    => SetModeSyncAndAsync_UnsupportedPinMode(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
  public void SetMode_GP2_UnsupportedPinMode(PinMode mode)
    => SetModeSyncAndAsync_UnsupportedPinMode(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_UnsupportedPinMode))]
  public void SetMode_GP3_UnsupportedPinMode(PinMode mode)
    => SetModeSyncAndAsync_UnsupportedPinMode(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void SetModeSyncAndAsync_UnsupportedPinMode(
    PinMode mode,
    Func<Mcp2221A, GpController> selectGpPin
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    var gp = selectGpPin(mcp2221A);

    Assert.That(
      () => gp.SetMode(mode),
      Throws.TypeOf<NotSupportedException>()
    );
    Assert.That(
      async () => await gp.SetModeAsync(mode),
      Throws.TypeOf<NotSupportedException>()
    );
  }

  [Test]
  public void SetModeAsync_Disposed()
    => SetModeSyncOrAsync_Disposed(
      static async gp => await gp.SetModeAsync(PinMode.Output).ConfigureAwait(false)
    );

  [Test]
  public void SetMode_Disposed()
    => SetModeSyncOrAsync_Disposed(
      static gp => {
        gp.SetMode(PinMode.Output);
        return default;
      }
    );

  private void SetModeSyncOrAsync_Disposed(
    Func<GpController, ValueTask> setModeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    var gpPins = mcp2221A.GpPins.ToList();

    mcp2221A.Dispose();

    foreach (var gp in gpPins) {
      Assert.That(
        async () => await setModeAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void SetModeAsync_CancellationRequested()
    => SetModeSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await gp.SetModeAsync(PinMode.Output, ct).ConfigureAwait(false)
    );

  [Test]
  public void SetMode_CancellationRequested()
    => SetModeSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        gp.SetMode(PinMode.Output, ct);
        return default;
      }
    );

  private void SetModeSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> setModeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await setModeAsyncFunc(gp, cts.Token),
        Throws
          .TypeOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }

  private static IEnumerable<PinMode> YieldTestCases_SetMode()
  {
    yield return PinMode.Output;
    yield return PinMode.Input;
  }

  [TestCaseSource(nameof(YieldTestCases_SetMode))]
  public void SetMode_GPO(PinMode mode)
    => SetModeSyncAndAsync(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_SetMode))]
  public void SetMode_GP1(PinMode mode)
    => SetModeSyncAndAsync(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_SetMode))]
  public void SetMode_GP2(PinMode mode)
    => SetModeSyncAndAsync(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_SetMode))]
  public void SetMode_GP3(PinMode mode)
    => SetModeSyncAndAsync(
      mode: mode,
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void SetModeSyncAndAsync(
    PinMode mode,
    Func<Mcp2221A, GpController> selectGpPin
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio(
      initialValues: [
        new(0, PinValue.Low),
        new(1, PinValue.Low),
        new(2, PinValue.Low),
        new(3, PinValue.Low),
      ]
    );
    var gp = selectGpPin(mcp2221A);

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      gp.Index == 0 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      gp.Index == 1 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      gp.Index == 2 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      gp.Index == 3 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    // [1] Don't care
    // [2 + 4n]: Alter GP<n> output: 0x00=disable, (value other than 0)=enable
    // [3 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [4 + 4n]: Alter GP<n> pin direction: 0x00=disable, (value other than 0)=enable
    // [5 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    for (var n = 0; n < 4; n++) {
      expectedSentCommand[2 + 4 * n] = 0x00;
      expectedSentCommand[3 + 4 * n] = 0x00;
      expectedSentCommand[4 + 4 * n] = (byte)(n == gp.Index ? 0xFF : 0x00);
      expectedSentCommand[5 + 4 * n] = (byte)((n == gp.Index) ? (mode == PinMode.Output ? 0x00 : 0xFF) : 0x00);
    }

    Mcp2221ATests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => gp.SetMode(mode, default),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.SetMode)}"
    );

    Mcp2221ATests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await gp.SetModeAsync(mode, default),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.SetModeAsync)}"
    );
  }

  [TestCaseSource(nameof(YieldTestCases_GP0_InvalidConfigurationSettings))]
  public void Write_GPO_InvalidConfiguration(byte gp0Settings)
    => WriteSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp0Settings: gp0Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_GP1_InvalidConfigurationSettings))]
  public void Write_GP1_InvalidConfiguration(byte gp1Settings)
    => WriteSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp1Settings: gp1Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_GP2_InvalidConfigurationSettings))]
  public void Write_GP2_InvalidConfiguration(byte gp2Settings)
    => WriteSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp2Settings: gp2Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_GP3_InvalidConfigurationSettings))]
  public void Write_GP3_InvalidConfiguration(byte gp3Settings)
    => WriteSyncAndAsync_InvalidConfiguration(
      createUsbHidDevice: () => Mcp2221ATests.CreatePseudoDevice(gp3Settings: gp3Settings),
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void WriteSyncAndAsync_InvalidConfiguration(
    Func<IUsbHidDevice> createUsbHidDevice,
    Func<Mcp2221A, GpController> selectGpPin
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      createUsbHidDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var gp = selectGpPin(mcp2221A);

    Assert.That(
      () => gp.Write(true, default),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{gp.Index}")
    );
    Assert.That(
      async () => await gp.WriteAsync(true, default),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{gp.Index}")
    );
  }

  private static IEnumerable<PinValue> YieldTestCases_Write()
  {
    yield return PinValue.High;
    yield return PinValue.Low;
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write_GPO(PinValue value)
    => WriteSyncAndAsync(
      value: value,
      selectGpPin: static mcp2221A => mcp2221A.GpPin0
    );

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write_GP1(PinValue value)
    => WriteSyncAndAsync(
      value: value,
      selectGpPin: static mcp2221A => mcp2221A.GpPin1
    );

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write_GP2(PinValue value)
    => WriteSyncAndAsync(
      value: value,
      selectGpPin: static mcp2221A => mcp2221A.GpPin2
    );

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write_GP3(PinValue value)
    => WriteSyncAndAsync(
      value: value,
      selectGpPin: static mcp2221A => mcp2221A.GpPin3
    );

  private void WriteSyncAndAsync(
    PinValue value,
    Func<Mcp2221A, GpController> selectGpPin
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

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      gp.Index == 0 ? $"FF-{((bool)value ? "FF" : "00")}-00-00-" : "00-00-00-00-",
      gp.Index == 1 ? $"FF-{((bool)value ? "FF" : "00")}-00-00-" : "00-00-00-00-",
      gp.Index == 2 ? $"FF-{((bool)value ? "FF" : "00")}-00-00-" : "00-00-00-00-",
      gp.Index == 3 ? $"FF-{((bool)value ? "FF" : "00")}-00-00-" : "00-00-00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    // [1] Don't care
    // [2 + 4n]: Alter GP<n> output: 0x00=disable, (value other than 0)=enable
    // [3 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [4 + 4n]: Alter GP<n> pin direction: 0x00=disable, (value other than 0)=enable
    // [5 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    for (var n = 0; n < 4; n++) {
      expectedSentCommand[2 + 4 * n] = (byte)(n == gp.Index ? 0xFF : 0x00);
      expectedSentCommand[3 + 4 * n] = (byte)((n == gp.Index) ? ((bool)value ? 0xFF : 0x00) : 0x00);
      expectedSentCommand[4 + 4 * n] = 0x00;
      expectedSentCommand[5 + 4 * n] = 0x00;
    }

    Mcp2221ATests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => gp.Write(value, default),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.Write)}"
    );

    Mcp2221ATests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await gp.WriteAsync(value, default),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand),
      $"sent command from {nameof(gp.WriteAsync)}"
    );
  }

  [Test]
  public void WriteAsync_Disposed()
    => WriteSyncOrAsync_Disposed(
      static async gp => await gp.WriteAsync(PinValue.Low).ConfigureAwait(false)
    );

  [Test]
  public void Write_Disposed()
    => WriteSyncOrAsync_Disposed(
      static gp => {
        gp.Write(PinValue.Low);
        return default;
      }
    );

  private void WriteSyncOrAsync_Disposed(
    Func<GpController, ValueTask> writeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    var gpPins = mcp2221A.GpPins.ToList();

    mcp2221A.Dispose();

    foreach (var gp in gpPins) {
      Assert.That(
        async () => await writeAsyncFunc(gp),
        Throws.TypeOf<ObjectDisposedException>(),
        $"object disposed ({gp.PinName})"
      );
    }
  }

  [Test]
  public void WriteAsync_CancellationRequested()
    => WriteSyncOrAsync_CancellationRequested(
      static async (gp, ct) => await gp.WriteAsync(PinValue.Low, ct).ConfigureAwait(false)
    );

  [Test]
  public void Write_CancellationRequested()
    => WriteSyncOrAsync_CancellationRequested(
      static (gp, ct) => {
        gp.Write(PinValue.Low, ct);
        return default;
      }
    );

  private void WriteSyncOrAsync_CancellationRequested(
    Func<GpController, CancellationToken, ValueTask> writeAsyncFunc
  )
  {
    using var mcp2221A = CreateMcp2221AConfiguredAsGpio();
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    foreach (var gp in mcp2221A.GpPins) {
      Assert.That(
        async () => await writeAsyncFunc(gp, cts.Token),
        Throws
          .TypeOf<OperationCanceledException>()
          .With
          .Property(nameof(OperationCanceledException.CancellationToken))
          .EqualTo(cts.Token),
        $"cancellation requested ({gp.PinName})"
      );
    }
  }
}
