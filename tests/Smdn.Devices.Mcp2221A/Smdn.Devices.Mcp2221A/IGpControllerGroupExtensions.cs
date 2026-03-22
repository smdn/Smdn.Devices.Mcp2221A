// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class IGpControllerGroupExtensionsTests {
  [Test]
  public void ConfigureAllGpFunctionsAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllGpFunctionsAsync(default, default, default, default, default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllGpFunctions_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllGpFunctions(default, default, default, default, default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllGpFunctionsAsync_CancellationRequested()
    => ConfigureAllGpFunctionsSyncOrAsync_CancellationRequested(
      static async (gps, gp0Function, gp1Function, gp2Function, gp3Function, ct)
        => await gps.ConfigureAllGpFunctionsAsync(gp0Function, gp1Function, gp2Function, gp3Function, ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpFunctions_CancellationRequested()
    => ConfigureAllGpFunctionsSyncOrAsync_CancellationRequested(
      static (gps, gp0Function, gp1Function, gp2Function, gp3Function, ct) => {
        gps.ConfigureAllGpFunctions(gp0Function, gp1Function, gp2Function, gp3Function, ct);
        return default;
      }
    );

  private void ConfigureAllGpFunctionsSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, CancellationToken, ValueTask> configureAllGpFunctionsAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221ATests.AppendPseudoResponse(...);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAllGpFunctionsAsyncFunc(
        mcp2221A.GpPins,
        GpFunction.Gpio,
        GpFunction.Gpio,
        GpFunction.Gpio,
        GpFunction.Gpio,
        cts.Token
      ),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );
    Assert.That(
      Mcp2221ATests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(
      mcp2221A.GpPin0.CurrentFunction,
      Is.EqualTo(initialGp0Function)
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(initialGp1Function)
    );
    Assert.That(
      mcp2221A.GpPin2.CurrentFunction,
      Is.EqualTo(initialGp2Function)
    );
    Assert.That(
      mcp2221A.GpPin3.CurrentFunction,
      Is.EqualTo(initialGp3Function)
    );
  }

  [Test]
  public void ConfigureAllGpFunctionsAsync_Disposed()
    => ConfigureAllGpFunctionsSyncOrAsync_Disposed(
      static async (gps, gp0Function, gp1Function, gp2Function, gp3Function)
        => await gps.ConfigureAllGpFunctionsAsync(gp0Function, gp1Function, gp2Function, gp3Function).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpFunctions_Disposed()
    => ConfigureAllGpFunctionsSyncOrAsync_Disposed(
      static (gps, gp0Function, gp1Function, gp2Function, gp3Function) => {
        gps.ConfigureAllGpFunctions(gp0Function, gp1Function, gp2Function, gp3Function);
        return default;
      }
    );

  private void ConfigureAllGpFunctionsSyncOrAsync_Disposed(
    Func<IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, ValueTask> configureAllGpFunctionsAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAllGpFunctionsAsyncFunc(
        mcp2221A.GpPins,
        GpFunction.Gpio,
        GpFunction.Gpio,
        GpFunction.Gpio,
        GpFunction.Gpio
      ),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction()
  {
    // GP0
    yield return new object?[] { GpFunction.Adc, null, null, null };
    yield return new object?[] { GpFunction.Dac, null, null, null };
    yield return new object?[] { GpFunction.ExternalInterrupt, null, null, null };
    yield return new object?[] { GpFunction.ClockOutput, null, null, null };
    yield return new object?[] { GpFunction.UsbConfigureStatus, null, null, null };
    // GP1
    yield return new object?[] { null, GpFunction.Dac, null, null };
    yield return new object?[] { null, GpFunction.UsbSuspendStatus, null, null };
    yield return new object?[] { null, GpFunction.UsbConfigureStatus, null, null };
    // GP2
    yield return new object?[] { null, null, GpFunction.ExternalInterrupt, null };
    yield return new object?[] { null, null, GpFunction.LedOutput, null };
    yield return new object?[] { null, null, GpFunction.ClockOutput, null };
    yield return new object?[] { null, null, GpFunction.UsbSuspendStatus, null };
    // GP3
    yield return new object?[] { null, null, null, GpFunction.ExternalInterrupt };
    yield return new object?[] { null, null, null, GpFunction.ClockOutput };
    yield return new object?[] { null, null, null, GpFunction.UsbSuspendStatus };
    yield return new object?[] { null, null, null, GpFunction.UsbConfigureStatus };
    // GP0-3
    yield return new object?[] { GpFunction.ExternalInterrupt, GpFunction.UsbSuspendStatus, GpFunction.UsbSuspendStatus, GpFunction.UsbSuspendStatus };
    yield return new object?[] { null, GpFunction.UsbSuspendStatus, GpFunction.UsbSuspendStatus, GpFunction.UsbSuspendStatus };
    yield return new object?[] { null, null, GpFunction.UsbSuspendStatus, GpFunction.UsbSuspendStatus };
    yield return new object?[] { null, null, null, GpFunction.UsbSuspendStatus };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction))]
  public void ConfigureAllGpFunctionsAsync_UnsupportedFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function
  )
    => ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      static async (gps, gp0Function, gp1Function, gp2Function, gp3Function)
        => await gps.ConfigureAllGpFunctionsAsync(gp0Function, gp1Function, gp2Function, gp3Function).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction))]
  public void ConfigureAllGpFunctions_UnsupportedFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function
  )
    => ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      static (gps, gp0Function, gp1Function, gp2Function, gp3Function) => {
        gps.ConfigureAllGpFunctions(gp0Function, gp1Function, gp2Function, gp3Function);
        return default;
      }
    );

  private void ConfigureAllGpFunctionsSyncOrAsync_UnsupportedFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function,
    Func<IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, ValueTask> configureAllGpFunctionsAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221ATests.AppendPseudoResponse(...);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAllGpFunctionsAsyncFunc(
        mcp2221A.GpPins,
        gp0Function,
        gp1Function,
        gp2Function,
        gp3Function
      ),
      Throws.TypeOf<NotSupportedException>()
    );
    Assert.That(
      Mcp2221ATests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(
      mcp2221A.GpPin0.CurrentFunction,
      Is.EqualTo(initialGp0Function)
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(initialGp1Function)
    );
    Assert.That(
      mcp2221A.GpPin2.CurrentFunction,
      Is.EqualTo(initialGp2Function)
    );
    Assert.That(
      mcp2221A.GpPin3.CurrentFunction,
      Is.EqualTo(initialGp3Function)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync()
  {
    yield return new object?[] { GpFunction.Gpio, GpFunction.Gpio, GpFunction.Gpio, GpFunction.Gpio };
    yield return new object?[] { null, GpFunction.Gpio, GpFunction.Gpio, GpFunction.Gpio };
    yield return new object?[] { null, null, GpFunction.Gpio, GpFunction.Gpio };
    yield return new object?[] { null, null, null, GpFunction.Gpio };
    yield return new object?[] { GpFunction.Gpio, null, null, null };
    yield return new object?[] { null, GpFunction.Gpio, null, null };
    yield return new object?[] { null, null, GpFunction.Gpio, null };
    yield return new object?[] { null, null, null, GpFunction.Gpio };
    yield return new object?[] { null, GpFunction.Adc, null, null };
    yield return new object?[] { null, null, GpFunction.Adc, null };
    yield return new object?[] { null, null, null, GpFunction.Adc };
    yield return new object?[] { null, null, GpFunction.Dac, null };
    yield return new object?[] { null, null, null, GpFunction.Dac };
    yield return new object?[] { null, GpFunction.ExternalInterrupt, null, null };
    yield return new object?[] { GpFunction.LedOutput, null, null, null };
    yield return new object?[] { null, GpFunction.LedOutput, null, null };
    yield return new object?[] { null, null, null, GpFunction.LedOutput };
    yield return new object?[] { null, GpFunction.ClockOutput, null, null };
    yield return new object?[] { GpFunction.UsbSuspendStatus, null, null, null };
    yield return new object?[] { null, null, GpFunction.UsbConfigureStatus, null };
    yield return new object?[] { GpFunction.UsbSuspendStatus, GpFunction.ClockOutput, GpFunction.UsbConfigureStatus, GpFunction.Adc };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync))]
  public void ConfigureAllGpFunctionsAsync(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function
  )
    => ConfigureAllGpFunctionsSyncOrAsync(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      static async (gps, gp0Function, gp1Function, gp2Function, gp3Function)
        => await gps.ConfigureAllGpFunctionsAsync(gp0Function, gp1Function, gp2Function, gp3Function).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpFunctionsSyncOrAsync))]
  public void ConfigureAllGpFunctions(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function
  )
    => ConfigureAllGpFunctionsSyncOrAsync(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      static (gps, gp0Function, gp1Function, gp2Function, gp3Function) => {
        gps.ConfigureAllGpFunctions(gp0Function, gp1Function, gp2Function, gp3Function);
        return default;
      }
    );

  private void ConfigureAllGpFunctionsSyncOrAsync(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function,
    Func<IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, ValueTask> configureAllGpFunctionsAsyncFunc
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
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    Mcp2221ATests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = InitialGp0Settings & 0b_111_1_1_000; // [8] GP0 settings (except GP0 Designation)
    expectedSentCommand[9] = InitialGp1Settings & 0b_111_1_1_000; // [9] GP1 settings (except GP1 Designation)
    expectedSentCommand[10] = InitialGp2Settings & 0b_111_1_1_000; // [10] GP2 settings (except GP2 Designation)
    expectedSentCommand[11] = InitialGp3Settings & 0b_111_1_1_000; // [11] GP3 settings (except GP3 Designation)

    expectedSentCommand[8] |= (byte)(gp0Function is null ? (InitialGp0Settings & 0b_000_0_0_111) : Mcp2221AGpioDriverTests.GetGpDesignationBitsForFunction(0, gp0Function.Value)); // [8] GP0 settings
    expectedSentCommand[9] |= (byte)(gp1Function is null ? (InitialGp1Settings & 0b_000_0_0_111) : Mcp2221AGpioDriverTests.GetGpDesignationBitsForFunction(1, gp1Function.Value)); // [9] GP1 settings
    expectedSentCommand[10] |= (byte)(gp2Function is null ? (InitialGp2Settings & 0b_000_0_0_111) : Mcp2221AGpioDriverTests.GetGpDesignationBitsForFunction(2, gp2Function.Value)); // [10] GP2 settings
    expectedSentCommand[11] |= (byte)(gp3Function is null ? (InitialGp3Settings & 0b_000_0_0_111) : Mcp2221AGpioDriverTests.GetGpDesignationBitsForFunction(3, gp3Function.Value)); // [11] GP3 settings

    Assert.That(
      async () => await configureAllGpFunctionsAsyncFunc(
        mcp2221A.GpPins,
        gp0Function,
        gp1Function,
        gp2Function,
        gp3Function
      ),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mcp2221A.GpPin0.CurrentFunction,
      Is.EqualTo(gp0Function ?? initialGp0Function)
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(gp1Function ?? initialGp1Function)
    );
    Assert.That(
      mcp2221A.GpPin2.CurrentFunction,
      Is.EqualTo(gp2Function ?? initialGp2Function)
    );
    Assert.That(
      mcp2221A.GpPin3.CurrentFunction,
      Is.EqualTo(gp3Function ?? initialGp3Function)
    );
  }

  [Test]
  public void ConfigureAllGpFunctionsAsync_AllDefault()
    => ConfigureAllGpFunctionsSyncOrAsync_AllDefault(
      static async (gps, gp0Function, gp1Function, gp2Function, gp3Function)
        => await gps.ConfigureAllGpFunctionsAsync(gp0Function, gp1Function, gp2Function, gp3Function).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpFunctions_AllDefault()
    => ConfigureAllGpFunctionsSyncOrAsync_AllDefault(
      static (gps, gp0Function, gp1Function, gp2Function, gp3Function) => {
        gps.ConfigureAllGpFunctions(gp0Function, gp1Function, gp2Function, gp3Function);
        return default;
      }
    );

  private void ConfigureAllGpFunctionsSyncOrAsync_AllDefault(
    Func<IGpControllerGroup, GpFunction?, GpFunction?, GpFunction?, GpFunction?, ValueTask> configureAllGpFunctionsAsyncFunc
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
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221ATests.AppendPseudoResponse(...);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    GpFunction? maintainCurrentGpFunction = null;

    Assert.That(
      async () => await configureAllGpFunctionsAsyncFunc(
        mcp2221A.GpPins,
        maintainCurrentGpFunction,
        maintainCurrentGpFunction,
        maintainCurrentGpFunction,
        maintainCurrentGpFunction
      ),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(
      mcp2221A.GpPin0.CurrentFunction,
      Is.EqualTo(initialGp0Function)
    );
    Assert.That(
      mcp2221A.GpPin1.CurrentFunction,
      Is.EqualTo(initialGp1Function)
    );
    Assert.That(
      mcp2221A.GpPin2.CurrentFunction,
      Is.EqualTo(initialGp2Function)
    );
    Assert.That(
      mcp2221A.GpPin3.CurrentFunction,
      Is.EqualTo(initialGp3Function)
    );
  }

  [Test]
  public void ConfigureAllAsGpioAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpioAsync(default, default, default, default, default, default, default, default, default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllAsGpio_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpio(default, default, default, default, default, default, default, default, default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllAsGpioSyncOrAsync_CancellationRequested()
  {
    PinMode? nullPinMode = null;
    PinValue? nullPinValue = null;

    // all null
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };

    // GP0
    yield return new object?[] { PinMode.Input, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { PinMode.Output, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, PinValue.Low, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, PinValue.High, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };

    // GP1
    yield return new object?[] { nullPinMode, nullPinValue, PinMode.Input, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, PinMode.Output, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, PinValue.Low, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, PinValue.High, nullPinMode, nullPinValue, nullPinMode, nullPinValue };

    // GP2
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, PinMode.Input, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, PinMode.Output, nullPinValue, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinValue.Low, nullPinMode, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinValue.High, nullPinMode, nullPinValue };

    // GP3
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, PinMode.Input, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, PinMode.Output, nullPinValue };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinValue.Low };
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinValue.High };

    // all set
    yield return new object?[] { PinMode.Input, PinValue.Low, PinMode.Input, PinValue.High, PinMode.Output, PinValue.Low, PinMode.Output, PinValue.High };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync_CancellationRequested))]
  public void ConfigureAllAsGpioAsync_CancellationRequested(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_CancellationRequested(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static async (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, ct)
        => await gps.ConfigureAllAsGpioAsync(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, ct).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync_CancellationRequested))]
  public void ConfigureAllAsGpio_CancellationRequested(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_CancellationRequested(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, ct) => {
        gps.ConfigureAllAsGpio(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, ct);
        return default;
      }
    );

  private void ConfigureAllAsGpioSyncOrAsync_CancellationRequested(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value,
    Func<
      IGpControllerGroup,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      CancellationToken,
      ValueTask
    >
    configureAllAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221ATests.AppendPseudoResponse(...);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAllAsGpioAsyncFunc(
        mcp2221A.GpPins,
        gp0Mode,
        gp0Value,
        gp1Mode,
        gp1Value,
        gp2Mode,
        gp2Value,
        gp3Mode,
        gp3Value,
        cts.Token
      ),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );
    Assert.That(
      Mcp2221ATests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(initialGp0Function));
    Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
    Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(initialGp1Function));
    Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
    Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(initialGp2Function));
    Assert.That(() => _ = mcp2221A.GpPin2.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
    Assert.That(() => _ = mcp2221A.GpPin2.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(initialGp3Function));
    Assert.That(() => _ = mcp2221A.GpPin3.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
    Assert.That(() => _ = mcp2221A.GpPin3.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
  }

  [Test]
  public void ConfigureAllAsGpioAsync_Disposed()
    => ConfigureAllAsGpioSyncOrAsync_Disposed(
      static async (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value)
        => await gps.ConfigureAllAsGpioAsync(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpio_Disposed()
    => ConfigureAllAsGpioSyncOrAsync_Disposed(
      static (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value) => {
        gps.ConfigureAllAsGpio(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value);
        return default;
      }
    );

  private void ConfigureAllAsGpioSyncOrAsync_Disposed(
    Func<
      IGpControllerGroup,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      ValueTask
    >
    configureAllAsGpioAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAllAsGpioAsyncFunc(
        mcp2221A.GpPins,
        default,
        default,
        default,
        default,
        default,
        default,
        default,
        default
      ),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode()
  {
    PinMode? nullPinMode = null;
    PinValue? nullPinValue = null;

    foreach (var pinValue in new[] { nullPinValue, PinValue.Low, PinValue.High }) {
      // GP0
      yield return new object?[] { PinMode.InputPullDown, pinValue, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode };
      yield return new object?[] { PinMode.InputPullUp, pinValue, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode };
      // GP1
      yield return new object?[] { nullPinValue, nullPinMode, PinMode.InputPullDown, pinValue, nullPinValue, nullPinMode, nullPinValue, nullPinMode };
      yield return new object?[] { nullPinValue, nullPinMode, PinMode.InputPullUp, pinValue, nullPinValue, nullPinMode, nullPinValue, nullPinMode };
      // GP2
      yield return new object?[] { nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinMode.InputPullDown, pinValue, nullPinValue, nullPinMode };
      yield return new object?[] { nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinMode.InputPullUp, pinValue, nullPinValue, nullPinMode };
      // GP3
      yield return new object?[] { nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinMode.InputPullDown, pinValue };
      yield return new object?[] { nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, PinMode.InputPullUp, pinValue };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode))]
  public void ConfigureAllAsGpioAsync_UnsupportedPinMode(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static async (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value)
        => await gps.ConfigureAllAsGpioAsync(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode))]
  public void ConfigureAllAsGpio_UnsupportedPinMode(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value) => {
        gps.ConfigureAllAsGpio(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default);
        return default;
      }
    );

  private void ConfigureAllAsGpioSyncOrAsync_UnsupportedPinMode(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value,
    Func<
      IGpControllerGroup,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      ValueTask
    >
    configureAllAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221ATests.AppendPseudoResponse(...);
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAllAsGpioAsyncFunc(
        mcp2221A.GpPins,
        gp0Mode,
        gp0Value,
        gp1Mode,
        gp1Value,
        gp2Mode,
        gp2Value,
        gp3Mode,
        gp3Value
      ),
      Throws.TypeOf<NotSupportedException>()
    );
    Assert.That(
      Mcp2221ATests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(initialGp0Function));
    Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
    Assert.That(() => _ = mcp2221A.GpPin0.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(initialGp1Function));
    Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
    Assert.That(() => _ = mcp2221A.GpPin1.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(initialGp2Function));
    Assert.That(() => _ = mcp2221A.GpPin2.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
    Assert.That(() => _ = mcp2221A.GpPin2.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(initialGp3Function));
    Assert.That(() => _ = mcp2221A.GpPin3.LastFetchedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
    Assert.That(() => _ = mcp2221A.GpPin3.LastFetchedMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllAsGpioSyncOrAsync()
  {
    PinMode? nullPinMode = null;
    PinValue? nullPinValue = null;

    foreach (var pinMode in new[] { nullPinMode, PinMode.Output, PinMode.Input }) {
      foreach (var pinValue in new[] { nullPinValue, PinValue.Low, PinValue.High }) {
        // GP0
        yield return new object?[] { pinMode, pinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
        // GP1
        yield return new object?[] { nullPinMode, nullPinValue, pinMode, pinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
        // GP2
        yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, pinMode, pinValue, nullPinMode, nullPinValue };
        // GP3
        yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, pinMode, pinValue };

        // GP0-GP3
        yield return new object?[] { pinMode, pinValue, pinMode, pinValue, pinMode, pinValue, pinMode, pinValue };
      }
    }

    // leave GP0-GP3 as default
    yield return new object?[] { nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue, nullPinMode, nullPinValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync))]
  public void ConfigureAllAsGpioAsync(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static async (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value)
        => await gps.ConfigureAllAsGpioAsync(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync))]
  public void ConfigureAllAsGpio(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value) => {
        gps.ConfigureAllAsGpio(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default);
        return default;
      }
    );

  private void ConfigureAllAsGpioSyncOrAsync(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value,
    Func<
      IGpControllerGroup,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      ValueTask
    >
    configureAllAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    var initialGp0Value = PinValue.High;
    var initialGp0Mode = PinMode.Input;
    var initialGp1Value = PinValue.High;
    var initialGp1Mode = PinMode.Output;
    var initialGp2Value = PinValue.Low;
    var initialGp2Mode = PinMode.Input;
    var initialGp3Value = PinValue.Low;
    var initialGp3Mode = PinMode.Output;

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    Mcp2221ATests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    const byte GpDesignationGpio = 0b_000_0_0_000; // Bit 2-0: 000 (GPIO operation)

    static void WriteGpDirectionBits(ref byte gpSettings, PinMode? mode)
    {
      if (mode is null)
        return;

      gpSettings = (byte)((gpSettings & 0b_111_1_0_111) | (mode == PinMode.Input ? 0b_000_0_1_000 : 0b_000_0_0_000));
    }

    static void WriteGpValueBits(ref byte gpSettings, PinValue? value)
    {
      if (value is null)
        return;

      gpSettings = (byte)((gpSettings & 0b_111_0_1_111) | (value == PinValue.High ? 0b_000_1_0_000 : 0b_000_0_0_000));
    }

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = (InitialGp0Settings & 0b_111_1_1_000) | GpDesignationGpio; // [8] GP0 settings
    expectedSentCommand[9] = (InitialGp1Settings & 0b_111_1_1_000) | GpDesignationGpio; // [9] GP1 settings
    expectedSentCommand[10] = (InitialGp2Settings & 0b_111_1_1_000) | GpDesignationGpio; // [10] GP2 settings
    expectedSentCommand[11] = (InitialGp3Settings & 0b_111_1_1_000) | GpDesignationGpio; // [11] GP3 settings

    WriteGpDirectionBits(ref expectedSentCommand[8], gp0Mode);
    WriteGpDirectionBits(ref expectedSentCommand[9], gp1Mode);
    WriteGpDirectionBits(ref expectedSentCommand[10], gp2Mode);
    WriteGpDirectionBits(ref expectedSentCommand[11], gp3Mode);

    WriteGpValueBits(ref expectedSentCommand[8], gp0Value);
    WriteGpValueBits(ref expectedSentCommand[9], gp1Value);
    WriteGpValueBits(ref expectedSentCommand[10], gp2Value);
    WriteGpValueBits(ref expectedSentCommand[11], gp3Value);

    Assert.That(
      async () => await configureAllAsGpioAsyncFunc(
        mcp2221A.GpPins,
        gp0Mode,
        gp0Value,
        gp1Mode,
        gp1Value,
        gp2Mode,
        gp2Value,
        gp3Mode,
        gp3Value
      ),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastFetchedValue, Is.EqualTo(gp0Value ?? initialGp0Value));
    Assert.That(mcp2221A.GpPin0.LastFetchedMode, Is.EqualTo(gp0Mode ?? initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastFetchedValue, Is.EqualTo(gp1Value ?? initialGp1Value));
    Assert.That(mcp2221A.GpPin1.LastFetchedMode, Is.EqualTo(gp1Mode ?? initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastFetchedValue, Is.EqualTo(gp2Value ?? initialGp2Value));
    Assert.That(mcp2221A.GpPin2.LastFetchedMode, Is.EqualTo(gp2Mode ?? initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastFetchedValue, Is.EqualTo(gp3Value ?? initialGp3Value));
    Assert.That(mcp2221A.GpPin3.LastFetchedMode, Is.EqualTo(gp3Mode ?? initialGp3Mode));
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync))]
  public void ConfigureAllAsGpioAsync_ConfiguredAsGpioAtStartup(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_ConfiguredAsGpioAtStartup(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static async (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value)
        => await gps.ConfigureAllAsGpioAsync(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioSyncOrAsync))]
  public void ConfigureAllAsGpio_ConfiguredAsGpioAtStartup(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value
  )
    => ConfigureAllAsGpioSyncOrAsync_ConfiguredAsGpioAtStartup(
      gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value,
      static (gps, gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value) => {
        gps.ConfigureAllAsGpio(gp0Mode, gp0Value, gp1Mode, gp1Value, gp2Mode, gp2Value, gp3Mode, gp3Value, cancellationToken: default);
        return default;
      }
    );

  private void ConfigureAllAsGpioSyncOrAsync_ConfiguredAsGpioAtStartup(
    PinMode? gp0Mode,
    PinValue? gp0Value,
    PinMode? gp1Mode,
    PinValue? gp1Value,
    PinMode? gp2Mode,
    PinValue? gp2Value,
    PinMode? gp3Mode,
    PinValue? gp3Value,
    Func<
      IGpControllerGroup,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      PinMode?,
      PinValue?,
      ValueTask
    >
    configureAllAsGpioAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221A.Create(
      Mcp2221ATests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var initialGp0Value = mcp2221A.GpPin0.LastFetchedValue;
    var initialGp0Mode = mcp2221A.GpPin0.LastFetchedMode;
    var initialGp1Value = mcp2221A.GpPin1.LastFetchedValue;
    var initialGp1Mode = mcp2221A.GpPin1.LastFetchedMode;
    var initialGp2Value = mcp2221A.GpPin2.LastFetchedValue;
    var initialGp2Mode = mcp2221A.GpPin2.LastFetchedMode;
    var initialGp3Value = mcp2221A.GpPin3.LastFetchedValue;
    var initialGp3Mode = mcp2221A.GpPin3.LastFetchedMode;

    Mcp2221ATests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221ATests.ClearSentCommands(mcp2221A);

    const byte GpDesignationGpio = 0b_000_0_0_000; // Bit 2-0: 000 (GPIO operation)

    static void WriteGpDirectionBits(ref byte gpSettings, PinMode? mode)
    {
      if (mode is null)
        return;

      gpSettings = (byte)((gpSettings & 0b_111_1_0_111) | (mode == PinMode.Input ? 0b_000_0_1_000 : 0b_000_0_0_000));
    }

    static void WriteGpValueBits(ref byte gpSettings, PinValue? value)
    {
      if (value is null)
        return;

      gpSettings = (byte)((gpSettings & 0b_111_0_1_111) | (value == PinValue.High ? 0b_000_1_0_000 : 0b_000_0_0_000));
    }

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = InitialGp0Settings | GpDesignationGpio; // [8] GP0 settings
    expectedSentCommand[9] = InitialGp1Settings | GpDesignationGpio; // [9] GP1 settings
    expectedSentCommand[10] = InitialGp2Settings | GpDesignationGpio; // [10] GP2 settings
    expectedSentCommand[11] = InitialGp3Settings | GpDesignationGpio; // [11] GP3 settings

    WriteGpDirectionBits(ref expectedSentCommand[8], gp0Mode);
    WriteGpDirectionBits(ref expectedSentCommand[9], gp1Mode);
    WriteGpDirectionBits(ref expectedSentCommand[10], gp2Mode);
    WriteGpDirectionBits(ref expectedSentCommand[11], gp3Mode);

    WriteGpValueBits(ref expectedSentCommand[8], gp0Value);
    WriteGpValueBits(ref expectedSentCommand[9], gp1Value);
    WriteGpValueBits(ref expectedSentCommand[10], gp2Value);
    WriteGpValueBits(ref expectedSentCommand[11], gp3Value);

    Assert.That(
      async () => await configureAllAsGpioAsyncFunc(
        mcp2221A.GpPins,
        gp0Mode,
        gp0Value,
        gp1Mode,
        gp1Value,
        gp2Mode,
        gp2Value,
        gp3Mode,
        gp3Value
      ),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221ATests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastFetchedValue, Is.EqualTo(gp0Value ?? initialGp0Value));
    Assert.That(mcp2221A.GpPin0.LastFetchedMode, Is.EqualTo(gp0Mode ?? initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastFetchedValue, Is.EqualTo(gp1Value ?? initialGp1Value));
    Assert.That(mcp2221A.GpPin1.LastFetchedMode, Is.EqualTo(gp1Mode ?? initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastFetchedValue, Is.EqualTo(gp2Value ?? initialGp2Value));
    Assert.That(mcp2221A.GpPin2.LastFetchedMode, Is.EqualTo(gp2Mode ?? initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastFetchedValue, Is.EqualTo(gp3Value ?? initialGp3Value));
    Assert.That(mcp2221A.GpPin3.LastFetchedMode, Is.EqualTo(gp3Mode ?? initialGp3Mode));
  }
}
