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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(initialGp0Function));
    Assert.That(() => _ = mcp2221A.GpPin0.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
    Assert.That(() => _ = mcp2221A.GpPin0.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(initialGp1Function));
    Assert.That(() => _ = mcp2221A.GpPin1.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
    Assert.That(() => _ = mcp2221A.GpPin1.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(initialGp2Function));
    Assert.That(() => _ = mcp2221A.GpPin2.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
    Assert.That(() => _ = mcp2221A.GpPin2.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(initialGp3Function));
    Assert.That(() => _ = mcp2221A.GpPin3.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
    Assert.That(() => _ = mcp2221A.GpPin3.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
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
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
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
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(initialGp0Function));
    Assert.That(() => _ = mcp2221A.GpPin0.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));
    Assert.That(() => _ = mcp2221A.GpPin0.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP0"));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(initialGp1Function));
    Assert.That(() => _ = mcp2221A.GpPin1.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));
    Assert.That(() => _ = mcp2221A.GpPin1.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP1"));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(initialGp2Function));
    Assert.That(() => _ = mcp2221A.GpPin2.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));
    Assert.That(() => _ = mcp2221A.GpPin2.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP2"));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(initialGp3Function));
    Assert.That(() => _ = mcp2221A.GpPin3.LastUpdatedValue, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
    Assert.That(() => _ = mcp2221A.GpPin3.CurrentMode, Throws.InvalidOperationException.With.Property(nameof(InvalidOperationException.Message)).Contains("GP3"));
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
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
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(gp0Value ?? initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(gp0Mode ?? initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(gp1Value ?? initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(gp1Mode ?? initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(gp2Value ?? initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(gp2Mode ?? initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(gp3Value ?? initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(gp3Mode ?? initialGp3Mode));
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

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    var initialGp0Value = mcp2221A.GpPin0.LastUpdatedValue;
    var initialGp0Mode = mcp2221A.GpPin0.CurrentMode;
    var initialGp1Value = mcp2221A.GpPin1.LastUpdatedValue;
    var initialGp1Mode = mcp2221A.GpPin1.CurrentMode;
    var initialGp2Value = mcp2221A.GpPin2.LastUpdatedValue;
    var initialGp2Mode = mcp2221A.GpPin2.CurrentMode;
    var initialGp3Value = mcp2221A.GpPin3.LastUpdatedValue;
    var initialGp3Mode = mcp2221A.GpPin3.CurrentMode;

    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      // [1] 0x00: Command completed successfully
      // [2-63] Don't care
      "60-00-" + string.Join("-", Enumerable.Repeat("00", 62))
    );
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(gp0Value ?? initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(gp0Mode ?? initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(gp1Value ?? initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(gp1Mode ?? initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(gp2Value ?? initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(gp2Mode ?? initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(gp3Value ?? initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(gp3Mode ?? initialGp3Mode));
  }

  [Test]
  public void ConfigureAllAsGpioOutputAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpioOutputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllAsGpioOutput_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpioOutput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllAsGpioOutputAsync_CancellationRequested()
    => ConfigureAllAsGpioOutputSyncOrAsync_CancellationRequested(
      static async (gps, ct) => await gps.ConfigureAllAsGpioOutputAsync(cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpioOutput_CancellationRequested()
    => ConfigureAllAsGpioOutputSyncOrAsync_CancellationRequested(
      static (gps, ct) => {
        gps.ConfigureAllAsGpioOutput(cancellationToken: ct);
        return default;
      }
    );

  private void ConfigureAllAsGpioOutputSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> configureAllAsGpioOutputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAllAsGpioOutputAsyncFunc(
        mcp2221A.GpPins,
        cts.Token
      ),
      Throws
        .TypeOf<OperationCanceledException>()
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
  public void ConfigureAllAsGpioOutputAsync_Disposed()
    => ConfigureAllAsGpioOutputSyncOrAsync_Disposed(
      static async gps => await gps.ConfigureAllAsGpioOutputAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpioOutput_Disposed()
    => ConfigureAllAsGpioOutputSyncOrAsync_Disposed(
      static gps => {
        gps.ConfigureAllAsGpioOutput();
        return default;
      }
    );

  private void ConfigureAllAsGpioOutputSyncOrAsync_Disposed(
    Func<IGpControllerGroup, ValueTask> configureAllAsGpioOutputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAllAsGpioOutputAsyncFunc(mcp2221A.GpPins),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllAsGpioOutputSyncOrAsync()
  {
    PinValue? nullPinValue = null;

    foreach (var pinValue in new[] { PinValue.Low, PinValue.High }) {
      // GP0
      yield return new object?[] { pinValue, nullPinValue, nullPinValue, nullPinValue };
      // GP1
      yield return new object?[] { nullPinValue, pinValue, nullPinValue, nullPinValue };
      // GP2
      yield return new object?[] { nullPinValue, nullPinValue, pinValue, nullPinValue };
      // GP3
      yield return new object?[] { nullPinValue, nullPinValue, nullPinValue, pinValue };

      // GP0-GP3
      yield return new object?[] { pinValue, pinValue, pinValue, pinValue };
    }

    // leave GP0-GP3 initial value as default
    yield return new object?[] { nullPinValue, nullPinValue, nullPinValue, nullPinValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioOutputSyncOrAsync))]
  public void ConfigureAllAsGpioOutputAsync(
    PinValue? gp0InitialValue,
    PinValue? gp1InitialValue,
    PinValue? gp2InitialValue,
    PinValue? gp3InitialValue
  )
    => ConfigureAllAsGpioOutputSyncOrAsync(
      gp0InitialValue,
      gp1InitialValue,
      gp2InitialValue,
      gp3InitialValue,
      static async (gps, gp0InitialValue, gp1InitialValue, gp2InitialValue, gp3InitialValue)
        => await gps.ConfigureAllAsGpioOutputAsync(gp0InitialValue, gp1InitialValue, gp2InitialValue, gp3InitialValue).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllAsGpioOutputSyncOrAsync))]
  public void ConfigureAllAsGpioOutput(
    PinValue? gp0InitialValue,
    PinValue? gp1InitialValue,
    PinValue? gp2InitialValue,
    PinValue? gp3InitialValue
  )
    => ConfigureAllAsGpioOutputSyncOrAsync(
      gp0InitialValue,
      gp1InitialValue,
      gp2InitialValue,
      gp3InitialValue,
      static (gps, gp0InitialValue, gp1InitialValue, gp2InitialValue, gp3InitialValue) => {
        gps.ConfigureAllAsGpioOutput(gp0InitialValue, gp1InitialValue, gp2InitialValue, gp3InitialValue);
        return default;
      }
    );

  private void ConfigureAllAsGpioOutputSyncOrAsync(
    PinValue? gp0InitialValue,
    PinValue? gp1InitialValue,
    PinValue? gp2InitialValue,
    PinValue? gp3InitialValue,
    Func<IGpControllerGroup, PinValue?, PinValue?, PinValue?, PinValue?, ValueTask> configureAllAsGpioOutputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    var initialGp0Value = PinValue.High;
    var initialGp1Value = PinValue.High;
    var initialGp2Value = PinValue.Low;
    var initialGp3Value = PinValue.Low;

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
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

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = (byte)((bool)(gp0InitialValue ?? initialGp0Value) ? 0b_000_1_0_000 : 0b_000_0_0_000); // [8] GP0 settings
    expectedSentCommand[9] = (byte)((bool)(gp1InitialValue ?? initialGp1Value) ? 0b_000_1_0_000 : 0b_000_0_0_000); // [9] GP1 settings
    expectedSentCommand[10] = (byte)((bool)(gp2InitialValue ?? initialGp2Value) ? 0b_000_1_0_000 : 0b_000_0_0_000); // [10] GP2 settings
    expectedSentCommand[11] = (byte)((bool)(gp3InitialValue ?? initialGp3Value) ? 0b_000_1_0_000 : 0b_000_0_0_000); // [11] GP3 settings

    Assert.That(
      async () => await configureAllAsGpioOutputAsyncFunc(
        mcp2221A.GpPins,
        gp0InitialValue,
        gp1InitialValue,
        gp2InitialValue,
        gp3InitialValue
      ),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(gp0InitialValue ?? initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Output));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(gp1InitialValue ?? initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Output));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(gp2InitialValue ?? initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Output));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(gp3InitialValue ?? initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Output));
  }

  [Test]
  public void ConfigureAllAsGpioInputAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpioInputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllAsGpioInput_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ConfigureAllAsGpioInput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ConfigureAllAsGpioInputAsync_CancellationRequested()
    => ConfigureAllAsGpioInputSyncOrAsync_CancellationRequested(
      static async (gps, ct) => await gps.ConfigureAllAsGpioInputAsync(cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpioInput_CancellationRequested()
    => ConfigureAllAsGpioInputSyncOrAsync_CancellationRequested(
      static (gps, ct) => {
        gps.ConfigureAllAsGpioInput(cancellationToken: ct);
        return default;
      }
    );

  private void ConfigureAllAsGpioInputSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> configureAllAsGpioInputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await configureAllAsGpioInputAsyncFunc(
        mcp2221A.GpPins,
        cts.Token
      ),
      Throws
        .TypeOf<OperationCanceledException>()
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
  public void ConfigureAllAsGpioInputAsync_Disposed()
    => ConfigureAllAsGpioInputSyncOrAsync_Disposed(
      static async gps => await gps.ConfigureAllAsGpioInputAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpioInput_Disposed()
    => ConfigureAllAsGpioInputSyncOrAsync_Disposed(
      static gps => {
        gps.ConfigureAllAsGpioInput();
        return default;
      }
    );

  private void ConfigureAllAsGpioInputSyncOrAsync_Disposed(
    Func<IGpControllerGroup, ValueTask> configureAllAsGpioInputAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAllAsGpioInputAsyncFunc(mcp2221A.GpPins),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  [Test]
  public void ConfigureAllAsGpioInputAsync()
    => ConfigureAllAsGpioInputSyncOrAsync(
      static async gps => await gps.ConfigureAllAsGpioInputAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllAsGpioInput()
    => ConfigureAllAsGpioInputSyncOrAsync(
      static gps => {
        gps.ConfigureAllAsGpioInput();
        return default;
      }
    );

  private void ConfigureAllAsGpioInputSyncOrAsync(
    Func<IGpControllerGroup, ValueTask> configureAllAsGpioInputAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    var initialGp0Value = PinValue.High;
    var initialGp1Value = PinValue.High;
    var initialGp2Value = PinValue.Low;
    var initialGp3Value = PinValue.Low;

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
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

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x60; // [0] SET SRAM SETTINGS
    // [1-6] don't care
    expectedSentCommand[7] = 0b10000000; // [7] Alter GPIO configuration = Alter the GP designation (1)
    expectedSentCommand[8] = (InitialGp0Settings & 0b_000_1_0_000) | 0b_000_0_1_000; // [8] GP0 settings
    expectedSentCommand[9] = (InitialGp1Settings & 0b_000_1_0_000) | 0b_000_0_1_000; // [9] GP1 settings
    expectedSentCommand[10] = (InitialGp2Settings & 0b_000_1_0_000) | 0b_000_0_1_000; // [10] GP2 settings
    expectedSentCommand[11] = (InitialGp3Settings & 0b_000_1_0_000) | 0b_000_0_1_000; // [11] GP3 settings

    Assert.That(
      async () => await configureAllAsGpioInputAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Input));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Input));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Input));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Input));
  }

  [Test]
  public void ReadAsync_WithPinValuePairs_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ReadAsync(default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void Read_WithPinValuePairs_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.Read(default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAsync_WithPinValuePairs_CancellationRequested()
    => ReadSyncOrAsync_WithPinValuePairs_CancellationRequested(
      static async (gpPins, ct) => await gpPins.ReadAsync(default, ct).ConfigureAwait(false)
    );

  [Test]
  public void Read_WithPinValuePairs_CancellationRequested()
    => ReadSyncOrAsync_WithPinValuePairs_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.Read(default, ct);
        return default;
      }
    );

  private void ReadSyncOrAsync_WithPinValuePairs_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> readAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await readAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadSyncOrAsync_WithPinValuePairs()
  {
    // [MCP2221A] 3.1.12 GET GPIO VALUES
    const byte GpL = 0x00; // GP<n> value: LOW
    const byte GpH = 0x01; // GP<n> value: HIGH

    yield return new object[] { GpL, GpL, GpL, GpL, new int[] { 0 }, new PinValue[] { PinValue.Low } };
    yield return new object[] { GpH, GpL, GpL, GpL, new int[] { 0 }, new PinValue[] { PinValue.High } };
    yield return new object[] { GpL, GpL, GpL, GpL, new int[] { 1 }, new PinValue[] { PinValue.Low } };
    yield return new object[] { GpL, GpH, GpL, GpL, new int[] { 1 }, new PinValue[] { PinValue.High } };
    yield return new object[] { GpL, GpL, GpL, GpL, new int[] { 2 }, new PinValue[] { PinValue.Low } };
    yield return new object[] { GpL, GpL, GpH, GpL, new int[] { 2 }, new PinValue[] { PinValue.High } };
    yield return new object[] { GpL, GpL, GpL, GpL, new int[] { 3 }, new PinValue[] { PinValue.Low } };
    yield return new object[] { GpL, GpL, GpL, GpH, new int[] { 3 }, new PinValue[] { PinValue.High } };

    yield return new object[] { GpL, GpH, GpH, GpH, new int[] { 0, 1 }, new PinValue[] { PinValue.Low, PinValue.High } };
    yield return new object[] { GpL, GpL, GpH, GpH, new int[] { 0, 1, 2 }, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.High } };
    yield return new object[] { GpL, GpL, GpL, GpH, new int[] { 0, 1, 2, 3 }, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.Low, PinValue.High } };

    yield return new object[] { GpL, GpH, GpH, GpH, new int[] { 0, 1, 2, 3 }, new PinValue[] { PinValue.Low, PinValue.High, PinValue.High, PinValue.High } };
    yield return new object[] { GpH, GpL, GpH, GpH, new int[] { 0, 1, 2, 3 }, new PinValue[] { PinValue.High, PinValue.Low, PinValue.High, PinValue.High } };
    yield return new object[] { GpH, GpH, GpL, GpH, new int[] { 0, 1, 2, 3 }, new PinValue[] { PinValue.High, PinValue.High, PinValue.Low, PinValue.High } };
    yield return new object[] { GpH, GpH, GpH, GpL, new int[] { 0, 1, 2, 3 }, new PinValue[] { PinValue.High, PinValue.High, PinValue.High, PinValue.Low } };

    yield return new object[] { GpL, GpH, GpH, GpH, new int[] { 3, 2, 1, 0 }, new PinValue[] { PinValue.High, PinValue.High, PinValue.High, PinValue.Low } };
    yield return new object[] { GpH, GpL, GpH, GpH, new int[] { 3, 2, 1, 0 }, new PinValue[] { PinValue.High, PinValue.High, PinValue.Low, PinValue.High } };
    yield return new object[] { GpH, GpH, GpL, GpH, new int[] { 3, 2, 1, 0 }, new PinValue[] { PinValue.High, PinValue.Low, PinValue.High, PinValue.High } };
    yield return new object[] { GpH, GpH, GpH, GpL, new int[] { 3, 2, 1, 0 }, new PinValue[] { PinValue.Low, PinValue.High, PinValue.High, PinValue.High } };

    yield return new object[] { GpH, GpL, GpH, GpL, new int[] { 0, 1, 0 }, new PinValue[] { PinValue.High, PinValue.Low, PinValue.High } };
    yield return new object[] { GpH, GpL, GpH, GpL, new int[] { 2, 2, 2, 2, 3 }, new PinValue[] { PinValue.High, PinValue.High, PinValue.High, PinValue.High, PinValue.Low } };
    yield return new object[] { GpH, GpL, GpH, GpL, new int[] { 1, 1, 0, 0, 1, 1 }, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.High, PinValue.High, PinValue.Low, PinValue.Low } };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_WithPinValuePairs))]
  public void ReadAsync_WithPinValuePairs(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    int[] pinNumbers,
    PinValue[] expectedPinValues
  )
    => ReadSyncOrAsync_WithPinValuePairs(
      gp0PinValue, gp1PinValue, gp2PinValue, gp3PinValue, pinNumbers, expectedPinValues,
      static async (gpPins, pinValuePairs) => await gpPins.ReadAsync(pinValuePairs: pinValuePairs).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_WithPinValuePairs))]
  public void Read_WithPinValuePairs(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    int[] pinNumbers,
    PinValue[] expectedPinValues
  )
    => ReadSyncOrAsync_WithPinValuePairs(
      gp0PinValue, gp1PinValue, gp2PinValue, gp3PinValue, pinNumbers, expectedPinValues,
      static (gpPins, pinValuePairs) => {
        gpPins.Read(pinValuePairs: pinValuePairs.Span);
        return default;
      }
    );

  private void ReadSyncOrAsync_WithPinValuePairs(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    int[] pinNumbers,
    PinValue[] expectedPinValues,
    Func<IGpControllerGroup, Memory<PinValuePair>, ValueTask> readAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      $"{gp0PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp1PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp2PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp3PinValue:X2}-01-", // LOW/HIGH - INPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    var pinValuePairs = pinNumbers.Select(static number => new PinValuePair(number, default)).ToArray();

    Assert.That(
      async () => await readAsyncFunc(mcp2221A.GpPins, pinValuePairs),
      Throws.Nothing
    );
    Assert.That(
      pinValuePairs.Select(static pair => pair.PinValue),
      Is.EqualTo(expectedPinValues).AsCollection
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mcp2221A.GpPin0.LastUpdatedValue,
      Is.EqualTo(gp0PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin1.LastUpdatedValue,
      Is.EqualTo(gp1PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin2.LastUpdatedValue,
      Is.EqualTo(gp2PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin3.LastUpdatedValue,
      Is.EqualTo(gp3PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
  }

  [Test]
  public void ReadAsync_WithPinValuePairs_Empty()
    => ReadSyncOrAsync_WithPinValuePairs_Empty(
      static async gpPins => await gpPins.ReadAsync(pinValuePairs: default).ConfigureAwait(false)
    );

  [Test]
  public void Read_WithPinValuePairs_Empty()
    => ReadSyncOrAsync_WithPinValuePairs_Empty(
      static gpPins => {
        gpPins.Read(pinValuePairs: default);
        return default;
      }
    );

  private void ReadSyncOrAsync_WithPinValuePairs_Empty(
    Func<IGpControllerGroup, ValueTask> readAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      "00-00-", // LOW - OUTPUT
      "00-01-", // LOW - INPUT
      "01-00-", // HIGH - OUTPUT
      "01-01-", // HIGH - INPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Assert.That(
      async () => await readAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(PinValue.High));

    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Input));
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex()
  {
    yield return new object[] { new int[] { -1 }, -1 };
    yield return new object[] { new int[] { 5 }, 5 };
    yield return new object[] { new int[] { int.MaxValue }, int.MaxValue };
    yield return new object[] { new int[] { int.MinValue }, int.MinValue };

    yield return new object[] { new int[] { 0, -1 }, -1 };
    yield return new object[] { new int[] { 0, 5 }, 5 };
    yield return new object[] { new int[] { 0, int.MaxValue }, int.MaxValue };
    yield return new object[] { new int[] { 0, int.MinValue }, int.MinValue };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex))]
  public void ReadAsync_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex
  )
    => ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
      pinNumbers,
      expectedInvalidGpIndex,
      static async (gpPins, pinValuePairs) => await gpPins.ReadAsync(pinValuePairs: pinValuePairs).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex))]
  public void Read_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex
  )
    => ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
      pinNumbers,
      expectedInvalidGpIndex,
      static (gpPins, pinValuePairs) => {
        gpPins.Read(pinValuePairs: pinValuePairs.Span);
        return default;
      }
    );

  private void ReadSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex,
    Func<IGpControllerGroup, Memory<PinValuePair>, ValueTask> readAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - OUTPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_1_1_000; // HIGH - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      "00-01-", // LOW - INPUT
      "01-01-", // HIGH - INPUT
      "00-00-", // LOW - OUTPUT
      "01-00-", // HIGH - OUTPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    var pinValuePairs = pinNumbers.Select(static number => new PinValuePair(number, default)).ToArray();

    Assert.That(
      async () => await readAsyncFunc(mcp2221A.GpPins, pinValuePairs),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"pin index: {expectedInvalidGpIndex}")
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(PinValue.High));

    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Output));
  }

  [Test]
  public void ReadAsync_AsTupleOfPinValue_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ReadAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void Read_AsTupleOfPinValue_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => _ = gpPins!.Read(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAsync_AsTupleOfPinValue_CancellationRequested()
    => ReadSyncOrAsync_AsTupleOfPinValue_CancellationRequested(
      static async (gpPins, ct) => _ = await gpPins.ReadAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void Read_AsTupleOfPinValue_CancellationRequested()
    => ReadSyncOrAsync_AsTupleOfPinValue_CancellationRequested(
      static (gpPins, ct) => {
        _ = gpPins.Read(ct);
        return default;
      }
    );

  private void ReadSyncOrAsync_AsTupleOfPinValue_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> readAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await readAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadSyncOrAsync_AsTupleOfPinValue()
  {
    // [MCP2221A] 3.1.12 GET GPIO VALUES
    const byte GpL = 0x00; // GP<n> value: LOW
    const byte GpH = 0x01; // GP<n> value: HIGH

    yield return new object[] { GpL, GpL, GpL, GpL, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.Low, PinValue.Low } };
    yield return new object[] { GpH, GpL, GpL, GpL, new PinValue[] { PinValue.High, PinValue.Low, PinValue.Low, PinValue.Low } };
    yield return new object[] { GpH, GpH, GpL, GpL, new PinValue[] { PinValue.High, PinValue.High, PinValue.Low, PinValue.Low } };
    yield return new object[] { GpH, GpH, GpH, GpL, new PinValue[] { PinValue.High, PinValue.High, PinValue.High, PinValue.Low } };
    yield return new object[] { GpH, GpH, GpH, GpH, new PinValue[] { PinValue.High, PinValue.High, PinValue.High, PinValue.High } };
    yield return new object[] { GpL, GpH, GpH, GpH, new PinValue[] { PinValue.Low, PinValue.High, PinValue.High, PinValue.High } };
    yield return new object[] { GpL, GpL, GpH, GpH, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.High, PinValue.High } };
    yield return new object[] { GpL, GpL, GpL, GpH, new PinValue[] { PinValue.Low, PinValue.Low, PinValue.Low, PinValue.High } };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_AsTupleOfPinValue))]
  public void ReadAsync_AsTupleOfPinValue(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    PinValue[] expectedPinValues
  )
    => ReadSyncOrAsync_AsTupleOfPinValue(
      gp0PinValue, gp1PinValue, gp2PinValue, gp3PinValue,
      (expectedPinValues[0], expectedPinValues[1], expectedPinValues[2], expectedPinValues[3]),
      static async gpPins => await gpPins.ReadAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadSyncOrAsync_AsTupleOfPinValue))]
  public void Read_AsTupleOfPinValue(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    PinValue[] expectedPinValues
  )
    => ReadSyncOrAsync_AsTupleOfPinValue(
      gp0PinValue, gp1PinValue, gp2PinValue, gp3PinValue,
      (expectedPinValues[0], expectedPinValues[1], expectedPinValues[2], expectedPinValues[3]),
      static gpPins => new(gpPins.Read())
    );

  private void ReadSyncOrAsync_AsTupleOfPinValue(
    byte gp0PinValue,
    byte gp1PinValue,
    byte gp2PinValue,
    byte gp3PinValue,
    (PinValue, PinValue, PinValue, PinValue) expectedPinValues,
    Func<IGpControllerGroup, ValueTask<(PinValue, PinValue, PinValue, PinValue)>> readAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      $"{gp0PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp1PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp2PinValue:X2}-01-", // LOW/HIGH - INPUT
      $"{gp3PinValue:X2}-01-", // LOW/HIGH - INPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    (PinValue, PinValue, PinValue, PinValue) pinValues = default;

    Assert.That(
      async () => pinValues = await readAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );
    Assert.That(
      pinValues,
      Is.EqualTo(expectedPinValues)
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mcp2221A.GpPin0.LastUpdatedValue,
      Is.EqualTo(gp0PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin1.LastUpdatedValue,
      Is.EqualTo(gp1PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin2.LastUpdatedValue,
      Is.EqualTo(gp2PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
    Assert.That(
      mcp2221A.GpPin3.LastUpdatedValue,
      Is.EqualTo(gp3PinValue == 0x00 ? PinValue.Low : PinValue.High)
    );
  }

  [Test]
  public void WriteAsync_WithPinValuePairs_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.WriteAsync(pinValuePairs: default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void Write_WithPinValuePairs_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.Write(pinValuePairs: default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void WriteAsync_WithPinValuePairs_CancellationRequested()
    => WriteSyncOrAsync_WithPinValuePairs_CancellationRequested(
      static async (gpPins, ct) => await gpPins.WriteAsync(default, ct).ConfigureAwait(false)
    );

  [Test]
  public void Write_WithPinValuePairs_CancellationRequested()
    => WriteSyncOrAsync_WithPinValuePairs_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.Write(default, ct);
        return default;
      }
    );

  private void WriteSyncOrAsync_WithPinValuePairs_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> writeAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_WithPinValuePairs()
  {
    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
    // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [2 + 4n]: Alter GP<n> pin direction: (value other than 0)=enable
    // [3 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    yield return new object[] { new PinValuePair[] { new(0, PinValue.Low) }, new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(0, PinValue.High) }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(1, PinValue.Low) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(1, PinValue.High) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(2, PinValue.Low) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(2, PinValue.High) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(3, PinValue.Low) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(3, PinValue.High) }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };

    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
    };
    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High), new(2, PinValue.High) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
    };
    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High), new(2, PinValue.High), new(3, PinValue.High) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 }
    };

    yield return new object[] { new PinValuePair[] { new(3, PinValue.High), new(2, PinValue.High), new(1, PinValue.High), new(0, PinValue.Low) }, new byte[] { 0xFF, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(3, PinValue.High), new(2, PinValue.High), new(1, PinValue.Low), new(0, PinValue.High) }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(3, PinValue.High), new(2, PinValue.Low), new(1, PinValue.High), new(0, PinValue.High) }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };
    yield return new object[] { new PinValuePair[] { new(3, PinValue.Low), new(2, PinValue.High), new(1, PinValue.High), new(0, PinValue.High) }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 } };

    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(0, PinValue.Low) },
      new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
    };
    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High), new(1, PinValue.Low) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
    };
    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High), new(2, PinValue.High), new(2, PinValue.Low) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
    };
    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.High), new(1, PinValue.High), new(2, PinValue.High), new(3, PinValue.High), new(3, PinValue.Low) },
      new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 }
    };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WithPinValuePairs))]
  public void WriteAsync_WithPinValuePairs(
    PinValuePair[] pinValuePairs,
    byte[] gpioOutputsInExpectedSentCommand
  )
    => WriteSyncOrAsync_WithPinValuePairs(
      pinValuePairs,
      gpioOutputsInExpectedSentCommand,
      static async (gpPins, pinValuePairs) => await gpPins.WriteAsync(pinValuePairs: pinValuePairs).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WithPinValuePairs))]
  public void Write_WithPinValuePairs(
    PinValuePair[] pinValuePairs,
    byte[] gpioOutputsInExpectedSentCommand
  )
    => WriteSyncOrAsync_WithPinValuePairs(
      pinValuePairs,
      gpioOutputsInExpectedSentCommand,
      static (gpPins, pinValuePairs) => {
        gpPins.Write(pinValuePairs: pinValuePairs.Span);
        return default;
      }
    );

  private void WriteSyncOrAsync_WithPinValuePairs(
    PinValuePair[] pinValuePairs,
    byte[] gpioOutputsInExpectedSentCommand,
    Func<IGpControllerGroup, ReadOnlyMemory<PinValuePair>, ValueTask> writeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      BitConverter.ToString(gpioOutputsInExpectedSentCommand) + "-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully
    gpioOutputsInExpectedSentCommand.CopyTo(expectedSentCommand.AsSpan(2, 16));

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins, pinValuePairs),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
  }

  [Test]
  public void WriteAsync_WithPinValuePairs_Empty()
    => WriteSyncOrAsync_WithPinValuePairs_Empty(
      static async gpPins => await gpPins.WriteAsync(
        pinValuePairs: default,
        cancellationToken: default
      ).ConfigureAwait(false)
    );

  [Test]
  public void Write_WithPinValuePairs_Empty()
    => WriteSyncOrAsync_WithPinValuePairs_Empty(
      static gpPins => {
        gpPins.Write(
          pinValuePairs: default,
          cancellationToken: default
        );
        return default;
      }
    );

  private void WriteSyncOrAsync_WithPinValuePairs_Empty(
    Func<IGpControllerGroup, ValueTask> writeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(PinValue.Low));

    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Input));
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex()
  {
    yield return new object[] { new int[] { -1 }, -1 };
    yield return new object[] { new int[] { 5 }, 5 };
    yield return new object[] { new int[] { int.MaxValue }, int.MaxValue };
    yield return new object[] { new int[] { int.MinValue }, int.MinValue };

    yield return new object[] { new int[] { 0, -1 }, -1 };
    yield return new object[] { new int[] { 0, 5 }, 5 };
    yield return new object[] { new int[] { 0, int.MaxValue }, int.MaxValue };
    yield return new object[] { new int[] { 0, int.MinValue }, int.MinValue };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex))]
  public void WriteAsync_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex
  )
    => WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
      pinNumbers,
      expectedInvalidGpIndex,
      static async (gpPins, pinValuePairs) => await gpPins.WriteAsync(pinValuePairs: pinValuePairs).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex))]
  public void Write_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex
  )
    => WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
      pinNumbers,
      expectedInvalidGpIndex,
      static (gpPins, pinValuePairs) => {
        gpPins.Write(pinValuePairs: pinValuePairs.Span);
        return default;
      }
    );

  private void WriteSyncOrAsync_WithPinValuePairs_InvalidGpIndex(
    int[] pinNumbers,
    int expectedInvalidGpIndex,
    Func<IGpControllerGroup, ReadOnlyMemory<PinValuePair>, ValueTask> writeAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var pinValuePairs = pinNumbers.Select(static number => new PinValuePair(number, default)).ToArray();

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins, pinValuePairs),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"pin index: {expectedInvalidGpIndex}")
    );
    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void WriteAsync_SeparatePinValues_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.WriteAsync(gp0Value: default, gp1Value: default, gp2Value: default, gp3Value: default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void Write_SeparatePinValues_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.Write(gp0Value: default, gp1Value: default, gp2Value: default, gp3Value: default),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void WriteAsync_SeparatePinValues_CancellationRequested()
    => WriteSyncOrAsync_SeparatePinValues_CancellationRequested(
      static async (gpPins, ct) => await gpPins.WriteAsync(cancellationToken: ct).ConfigureAwait(false)
    );

  [Test]
  public void Write_SeparatePinValues_CancellationRequested()
    => WriteSyncOrAsync_SeparatePinValues_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.Write(cancellationToken: ct);
        return default;
      }
    );

  private void WriteSyncOrAsync_SeparatePinValues_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> writeAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_WriteSyncOrAsync_SeparatePinValues()
  {
    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
    // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [2 + 4n]: Alter GP<n> pin direction: (value other than 0)=enable
    // [3 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    yield return new object[] { new PinValue?[] { PinValue.Low, null, null, null }, new byte[] { 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { PinValue.High, null, null, null }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, PinValue.Low, null, null }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, PinValue.High, null, null }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, null, PinValue.Low, null }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, null, PinValue.High, null }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, null, null, PinValue.Low }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { null, null, null, PinValue.High }, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };

    yield return new object[] { new PinValue?[] { PinValue.High, PinValue.High, null, null }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { PinValue.High, PinValue.High, PinValue.High, null }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new PinValue?[] { PinValue.High, PinValue.High, PinValue.High, PinValue.High }, new byte[] { 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00 } };
  }

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_SeparatePinValues))]
  public void WriteAsync_SeparatePinValues(
    PinValue?[] pinValues,
    byte[] gpioOutputsInExpectedSentCommand
  )
    => WriteSyncOrAsync_SeparatePinValues(
      pinValues,
      gpioOutputsInExpectedSentCommand,
      static async (gpPins, gp0Value, gp1Value, gp2Value, gp3Value) => await gpPins.WriteAsync(gp0Value, gp1Value, gp2Value, gp3Value).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_WriteSyncOrAsync_SeparatePinValues))]
  public void Write_SeparatePinValues(
    PinValue?[] pinValues,
    byte[] gpioOutputsInExpectedSentCommand
  )
    => WriteSyncOrAsync_SeparatePinValues(
      pinValues,
      gpioOutputsInExpectedSentCommand,
      static (gpPins, gp0Value, gp1Value, gp2Value, gp3Value) => {
        gpPins.Write(gp0Value, gp1Value, gp2Value, gp3Value);
        return default;
      }
    );

  private void WriteSyncOrAsync_SeparatePinValues(
    PinValue?[] pinValues,
    byte[] gpioOutputsInExpectedSentCommand,
    Func<IGpControllerGroup, PinValue?, PinValue?, PinValue?, PinValue?, ValueTask> writeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      BitConverter.ToString(gpioOutputsInExpectedSentCommand) + "-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully
    gpioOutputsInExpectedSentCommand.CopyTo(expectedSentCommand.AsSpan(2, 16));

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins, pinValues[0], pinValues[1], pinValues[2], pinValues[3]),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
  }

  [Test]
  public void WriteAsync_SeparatePinValues_AllNull()
    => WriteSyncOrAsync_SeparatePinValues_AllNull(
      static async gpPins => await gpPins.WriteAsync(
        gp0Value: null,
        gp1Value: null,
        gp2Value: null,
        gp3Value: null,
        cancellationToken: default
      ).ConfigureAwait(false)
    );

  [Test]
  public void Write_SeparatePinValues_AllNull()
    => WriteSyncOrAsync_SeparatePinValues_AllNull(
      static gpPins => {
        gpPins.Write(
          gp0Value: null,
          gp1Value: null,
          gp2Value: null,
          gp3Value: null,
          cancellationToken: default
        );
        return default;
      }
    );

  private void WriteSyncOrAsync_SeparatePinValues_AllNull(
    Func<IGpControllerGroup, ValueTask> writeAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

      // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      "00-00-00-00-", // do not modify
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully

    Assert.That(
      async () => await writeAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(PinValue.High));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(PinValue.Low));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(PinValue.Low));

    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Output));
  }

  [Test]
  public void ReadAnalogRawAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ReadAnalogRawAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogRaw_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => _ = gpPins!.ReadAnalogRaw(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogRawAsync_CancellationRequested()
    => ReadAnalogRawSyncOrAsync_CancellationRequested(
      static async (gpPins, ct) => _ = await gpPins.ReadAnalogRawAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void ReadAnalogRaw_CancellationRequested()
    => ReadAnalogRawSyncOrAsync_CancellationRequested(
      static (gpPins, ct) => {
        _ = gpPins.ReadAnalogRaw(ct);
        return default;
      }
    );

  private void ReadAnalogRawSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> readAnalogRawAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await readAnalogRawAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadAnalogRawSyncOrAsync()
  {
    yield return new object[] { "00-00-", "00-00-", "00-00-", 0x_00_00, 0x_00_00, 0x_00_00 };
    yield return new object[] { "FF-00-", "00-00-", "00-00-", 0x_00_FF, 0x_00_00, 0x_00_00 };
    yield return new object[] { "00-03-", "00-00-", "00-00-", 0x_03_00, 0x_00_00, 0x_00_00 };
    yield return new object[] { "00-00-", "FF-00-", "00-00-", 0x_00_00, 0x_00_FF, 0x_00_00 };
    yield return new object[] { "00-00-", "00-03-", "00-00-", 0x_00_00, 0x_03_00, 0x_00_00 };
    yield return new object[] { "00-00-", "00-00-", "FF-00-", 0x_00_00, 0x_00_00, 0x_00_FF };
    yield return new object[] { "00-00-", "00-00-", "00-03-", 0x_00_00, 0x_00_00, 0x_03_00 };
    yield return new object[] { "FF-03-", "FF-03-", "FF-03-", 0x_03_FF, 0x_03_FF, 0x_03_FF };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public void ReadAnalogRawAsync(
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
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static async gpPins => await gpPins.ReadAnalogRawAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogRawSyncOrAsync))]
  public void ReadAnalogRaw(
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
      expectedAdc1RawValue: expectedAdc1RawValue,
      expectedAdc2RawValue: expectedAdc2RawValue,
      expectedAdc3RawValue: expectedAdc3RawValue,
      static gpPins => new(gpPins.ReadAnalogRaw())
    );

  private void ReadAnalogRawSyncOrAsync(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    int expectedAdc1RawValue,
    int expectedAdc2RawValue,
    int expectedAdc3RawValue,
    Func<IGpControllerGroup, ValueTask<(int, int, int)>> readAnalogRawAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp1Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC1)
    const byte InitialGp2Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC2)
    const byte InitialGp3Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: InitialGp0Settings,
        gp1Settings: InitialGp1Settings,
        gp2Settings: InitialGp2Settings,
        gp3Settings: InitialGp3Settings
      ),
      shouldDisposeUsbHidDevice: true
    );

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

    (int, int, int) adcRawValues = default;

    Assert.That(
      async () => adcRawValues = await readAnalogRawAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    var (adc1RawValue, adc2RawValue, adc3RawValue) = adcRawValues;

    Assert.That(adc1RawValue, Is.EqualTo(expectedAdc1RawValue));
    Assert.That(adc2RawValue, Is.EqualTo(expectedAdc2RawValue));
    Assert.That(adc3RawValue, Is.EqualTo(expectedAdc3RawValue));
  }

  [Test]
  public void ReadAnalogVoltageAsync_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ReadAnalogVoltageAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogVoltage_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => _ = gpPins!.ReadAnalogVoltage(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogVoltageAsync_CancellationRequested()
    => ReadAnalogVoltageSyncOrAsync_CancellationRequested(
      static async (gpPins, ct) => _ = await gpPins.ReadAnalogVoltageAsync(ct).ConfigureAwait(false)
    );

  [Test]
  public void ReadAnalogVoltage_CancellationRequested()
    => ReadAnalogVoltageSyncOrAsync_CancellationRequested(
      static (gpPins, ct) => {
        _ = gpPins.ReadAnalogVoltage(ct);
        return default;
      }
    );

  private void ReadAnalogVoltageSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> readAnalogVoltageAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await readAnalogVoltageAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadAnalogVoltageSyncOrAsync()
  {
    const byte InitialChipSettings3_AdcVrm1024 = 0b_0_1_1_01_1_00; // ADC: VRM 1.024V (factory default)
    const byte InitialChipSettings3_AdcVrm2048 = 0b_0_1_1_10_1_00; // ADC: VRM 2.048V
    const byte InitialChipSettings3_AdcVrm4096 = 0b_0_1_1_11_1_00; // ADC: VRM 4.096V
    const byte InitialChipSettings3_AdcVrmOff = 0b_0_1_1_00_1_00; // ADC: VRM Off

    yield return new object[] { "00-00-", "01-00-", "FF-03-", InitialChipSettings3_AdcVrm1024, 0.0d, 0.001d, 1.023d };
    yield return new object[] { "01-00-", "FF-03-", "00-00-", InitialChipSettings3_AdcVrm2048, 0.002d, 2.046d, 0.0d };
    yield return new object[] { "FF-03-", "00-00-", "01-00-", InitialChipSettings3_AdcVrm4096, 4.092d, 0.0d, 0.004d };

    yield return new object[] { "00-00-", "00-00-", "00-00-", InitialChipSettings3_AdcVrmOff, 0.0d, 0.0d, 0.0d };
    yield return new object[] { "01-00-", "01-00-", "01-00-", InitialChipSettings3_AdcVrmOff, 0.0d, 0.0d, 0.0d };
    yield return new object[] { "FF-03-", "FF-03-", "FF-03-", InitialChipSettings3_AdcVrmOff, 0.0d, 0.0d, 0.0d };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogVoltageSyncOrAsync))]
  public void ReadAnalogVoltageAsync(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    byte initialChipSettings3,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
    => ReadAnalogVoltageSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      initialChipSettings3: initialChipSettings3,
      expectedAdc1Voltage: expectedAdc1Voltage,
      expectedAdc2Voltage: expectedAdc2Voltage,
      expectedAdc3Voltage: expectedAdc3Voltage,
      static async gpPins => await gpPins.ReadAnalogVoltageAsync().ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogVoltageSyncOrAsync))]
  public void ReadAnalogVoltage(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    byte initialChipSettings3,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
    => ReadAnalogVoltageSyncOrAsync(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      initialChipSettings3: initialChipSettings3,
      expectedAdc1Voltage: expectedAdc1Voltage,
      expectedAdc2Voltage: expectedAdc2Voltage,
      expectedAdc3Voltage: expectedAdc3Voltage,
      static gpPins => new(gpPins.ReadAnalogVoltage())
    );

  private void ReadAnalogVoltageSyncOrAsync(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    byte initialChipSettings3,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage,
    Func<IGpControllerGroup, ValueTask<(double, double, double)>> readAnalogVoltageAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp1Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC1)
    const byte InitialGp2Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC2)
    const byte InitialGp3Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC3)

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

    (double, double, double) adcVoltages = default;

    Assert.That(
      async () => adcVoltages = await readAnalogVoltageAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    var (adc1Voltage, adc2Voltage, adc3Voltage) = adcVoltages;

    Assert.That(adc1Voltage, Is.EqualTo(expectedAdc1Voltage).Within(1e-9));
    Assert.That(adc2Voltage, Is.EqualTo(expectedAdc2Voltage).Within(1e-9));
    Assert.That(adc3Voltage, Is.EqualTo(expectedAdc3Voltage).Within(1e-9));
  }

  [Test]
  public void ReadAnalogVoltageAsync_WithReferenceVoltage_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => gpPins!.ReadAnalogVoltageAsync(referenceVoltage: 5.0),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogVoltage_WithReferenceVoltage_ArgumentNull()
  {
    IGpControllerGroup? gpPins = null;

    Assert.That(
      () => _ = gpPins!.ReadAnalogVoltage(referenceVoltage: 5.0),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("gpPins")
    );
  }

  [Test]
  public void ReadAnalogVoltageAsync_WithReferenceVoltage_CancellationRequested()
    => ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage_CancellationRequested(
      static async (gpPins, ct) => _ = await gpPins.ReadAnalogVoltageAsync(referenceVoltage: 5.0, ct).ConfigureAwait(false)
    );

  [Test]
  public void ReadAnalogVoltage_WithReferenceVoltage_CancellationRequested()
    => ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage_CancellationRequested(
      static (gpPins, ct) => {
        _ = gpPins.ReadAnalogVoltage(referenceVoltage: 5.0, ct);
        return default;
      }
    );

  private void ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> readAnalogVoltageAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var cts = new CancellationTokenSource();

    cts.Cancel();

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await readAnalogVoltageAsyncFunc(mcp2221A.GpPins, cts.Token),
      Throws
        .TypeOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage()
  {
    const double Vdd_5V0 = 5.0;
    const double Vdd_3V3 = 3.3;
    const double Vdd_Zero = 0.0;

    yield return new object[] { "00-00-", "00-02-", "FF-03-", Vdd_5V0, 0.0d, 2.5d, (1023.0d * 5.0d) / 1024.0 };
    yield return new object[] { "00-00-", "00-01-", "FF-03-", Vdd_3V3, 0.0d, 0.825d, (1023.0d * 3.3d) / 1024.0d };
    yield return new object[] { "00-00-", "00-01-", "FF-03-", Vdd_Zero, 0.0d, 0.0d, 0.0d };
  }

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage))]
  public void ReadAnalogVoltageAsync_WithReferenceVoltage(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    double referenceVoltage,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
    => ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      referenceVoltage: referenceVoltage,
      expectedAdc1Voltage: expectedAdc1Voltage,
      expectedAdc2Voltage: expectedAdc2Voltage,
      expectedAdc3Voltage: expectedAdc3Voltage,
      static async (gpPins, referenceVoltage) => await gpPins.ReadAnalogVoltageAsync(referenceVoltage).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage))]
  public void ReadAnalogVoltage_WithReferenceVoltage(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    double referenceVoltage,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
    => ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage(
      adcChannel0Response: adcChannel0Response,
      adcChannel1Response: adcChannel1Response,
      adcChannel2Response: adcChannel2Response,
      referenceVoltage: referenceVoltage,
      expectedAdc1Voltage: expectedAdc1Voltage,
      expectedAdc2Voltage: expectedAdc2Voltage,
      expectedAdc3Voltage: expectedAdc3Voltage,
      static (gpPins, referenceVoltage) => new(gpPins.ReadAnalogVoltage(referenceVoltage))
    );

  private void ReadAnalogVoltageSyncOrAsync_WithReferenceVoltage(
    string adcChannel0Response,
    string adcChannel1Response,
    string adcChannel2Response,
    double referenceVoltage,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage,
    Func<IGpControllerGroup, double, ValueTask<(double, double, double)>> readAnalogVoltageAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // GPIO operation
    const byte InitialGp1Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC1)
    const byte InitialGp2Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC2)
    const byte InitialGp3Settings = 0b_000_0_0_010; // Alternate Function 0 (ADC3)
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

    (double, double, double) adcVoltages = default;

    Assert.That(
      async () => adcVoltages = await readAnalogVoltageAsyncFunc(mcp2221A.GpPins, referenceVoltage),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    var (adc1Voltage, adc2Voltage, adc3Voltage) = adcVoltages;

    Assert.That(adc1Voltage, Is.EqualTo(expectedAdc1Voltage).Within(1e-9));
    Assert.That(adc2Voltage, Is.EqualTo(expectedAdc2Voltage).Within(1e-9));
    Assert.That(adc3Voltage, Is.EqualTo(expectedAdc3Voltage).Within(1e-9));
  }
}
