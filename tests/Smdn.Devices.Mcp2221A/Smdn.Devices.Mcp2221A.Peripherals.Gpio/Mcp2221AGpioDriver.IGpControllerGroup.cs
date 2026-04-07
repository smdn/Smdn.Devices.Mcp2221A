// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

partial class Mcp2221AGpioDriverTests {
  [Test]
  public void ConfigureAllGpSettingsAsync_Disposed()
    => ConfigureAllGpSettingsSyncOrAsync_Disposed(
      static async gpPins => await gpPins.ConfigureAllGpSettingsAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpSettings_Disposed()
    => ConfigureAllGpSettingsSyncOrAsync_Disposed(
      static gpPins => {
        gpPins.ConfigureAllGpSettings();
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_Disposed(
    Func<IGpControllerGroup, ValueTask> configureAllGpSettingsAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await configureAllGpSettingsAsyncFunc(mcp2221A.GpPins),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  [Test]
  public void ConfigureAllGpSettingsAsync_CancellationRequested()
    => ConfigureAllGpSettingsSyncOrAsync_CancellationRequested(
      static async (gpPins, ct) => await gpPins.ConfigureAllGpSettingsAsync(
        gp0Function: GpFunction.Gpio,
        gp1Function: GpFunction.Gpio,
        gp2Function: GpFunction.Gpio,
        gp3Function: GpFunction.Gpio,
        cancellationToken: ct
      ).ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpSettings_CancellationRequested()
    => ConfigureAllGpSettingsSyncOrAsync_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.ConfigureAllGpSettings(
          gp0Function: GpFunction.Gpio,
          gp1Function: GpFunction.Gpio,
          gp2Function: GpFunction.Gpio,
          gp3Function: GpFunction.Gpio,
          cancellationToken: ct
        );
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> configureAllGpSettingsAsyncFunc
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
      async () => await configureAllGpSettingsAsyncFunc(mcp2221A.GpPins, cts.Token),
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

  [Test]
  public void ConfigureAllGpSettingsAsync_AllDefault(
    [Values] bool openByGpioController
  )
    => ConfigureAllGpSettingsSyncOrAsync_AllDefault(
      openByGpioController,
      static async gpPins => await gpPins.ConfigureAllGpSettingsAsync().ConfigureAwait(false)
    );

  [Test]
  public void ConfigureAllGpSettings_AllDefault(
    [Values] bool openByGpioController
  )
    => ConfigureAllGpSettingsSyncOrAsync_AllDefault(
      openByGpioController,
      static gpPins => {
        gpPins.ConfigureAllGpSettings();
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_AllDefault(
    bool openByGpioController,
    Func<IGpControllerGroup, ValueTask> configureAllGpSettingsAsyncFunc
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

    if (openByGpioController) {
      for (var gp = 0; gp < 4; gp++) {
        Assert.That(
          () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
          _ =
#endif
            mcp2221A.GpioController.OpenPin(gp),
          Throws.Nothing
        );
      }
    }

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAllGpSettingsAsyncFunc(mcp2221A.GpPins),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction()
  {
    yield return new object?[] { GpFunction.UsbSuspendStatus, null, null, null, (byte?)0b_000_0_0_001, null, null, null, };
    yield return new object?[] { GpFunction.LedOutput, null, null, null, (byte?)0b_000_0_0_010, null, null, null, };

    yield return new object?[] { null, GpFunction.ClockOutput, null, null, null, (byte?)0b_000_0_0_001, null, null };
    yield return new object?[] { null, GpFunction.Adc, null, null, null, (byte?)0b_000_0_0_010, null, null };
    yield return new object?[] { null, GpFunction.LedOutput, null, null, null, (byte?)0b_000_0_0_011, null, null };
    yield return new object?[] { null, GpFunction.ExternalInterrupt, null, null, null, (byte?)0b_000_0_0_100, null, null };

    yield return new object?[] { null, null, GpFunction.UsbConfigureStatus, null, null, null, (byte?)0b_000_0_0_001, null };
    yield return new object?[] { null, null, GpFunction.Adc, null, null, null, (byte?)0b_000_0_0_010, null };
    yield return new object?[] { null, null, GpFunction.Dac, null, null, null, (byte?)0b_000_0_0_011, null };

    yield return new object?[] { null, null, null, GpFunction.LedOutput, null, null, null, (byte?)0b_000_0_0_001 };
    yield return new object?[] { null, null, null, GpFunction.Adc, null, null, null, (byte?)0b_000_0_0_010 };
    yield return new object?[] { null, null, null, GpFunction.Dac, null, null, null, (byte?)0b_000_0_0_011 };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction))]
  public void ConfigureAllGpSettingsAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function,
    byte? expectedGp0Settings,
    byte? expectedGp1Settings,
    byte? expectedGp2Settings,
    byte? expectedGp3Settings
  )
    => ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      expectedGp0Settings,
      expectedGp1Settings,
      expectedGp2Settings,
      expectedGp3Settings,
      static async (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
      )
        => await gpPins.ConfigureAllGpSettingsAsync(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction))]
  public void ConfigureAllGpSettings_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function,
    byte? expectedGp0Settings,
    byte? expectedGp1Settings,
    byte? expectedGp2Settings,
    byte? expectedGp3Settings
  )
    => ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction(
      gp0Function,
      gp1Function,
      gp2Function,
      gp3Function,
      expectedGp0Settings,
      expectedGp1Settings,
      expectedGp2Settings,
      expectedGp3Settings,
      static (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
       ) => {
        gpPins.ConfigureAllGpSettings(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        );
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfSetToNonGpioFunction(
    GpFunction? gp0Function,
    GpFunction? gp1Function,
    GpFunction? gp2Function,
    GpFunction? gp3Function,
    byte? expectedGp0Settings,
    byte? expectedGp1Settings,
    byte? expectedGp2Settings,
    byte? expectedGp3Settings,
    Func<
      IGpControllerGroup,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      ValueTask
    > configureAllGpSettingsAsyncFunc
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

    foreach (var mode in new[] { PinMode.Input, PinMode.Output }) {
      foreach (var initialValue in new[] { PinValue.High, PinValue.Low }) {
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
        expectedSentCommand[8] = expectedGp0Settings ?? InitialGp0Settings; // [8] GP0 settings
        expectedSentCommand[9] = expectedGp1Settings ?? InitialGp1Settings; // [9] GP1 settings
        expectedSentCommand[10] = expectedGp2Settings ?? InitialGp2Settings; // [10] GP2 settings
        expectedSentCommand[11] = expectedGp3Settings ?? InitialGp3Settings; // [11] GP3 settings

        Assert.That(
          async () => await configureAllGpSettingsAsyncFunc(
            mcp2221A.GpPins,
            gp0Function,
            gp0Function is null ? null : mode,
            gp0Function is null ? null : initialValue,
            gp1Function,
            gp1Function is null ? null : mode,
            gp1Function is null ? null : initialValue,
            gp2Function,
            gp2Function is null ? null : mode,
            gp2Function is null ? null : initialValue,
            gp3Function,
            gp3Function is null ? null : mode,
            gp3Function is null ? null : initialValue
          ),
          Throws.Nothing
        );

        Assert.That(
          Mcp2221AControllerTests.GetSentCommand(mcp2221A),
          SequenceIs.EqualTo(expectedSentCommand)
        );
      }
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull()
  {
    foreach (var mode in new PinMode?[] { null, PinMode.Input, PinMode.Output }) {
      foreach (var initialValue in new PinValue?[] { null, PinValue.High, PinValue.Low }) {
        yield return new object?[] { mode, initialValue };
      }
    }
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull))]
  public void ConfigureAllGpSettingsAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull(
    PinMode? mode,
    PinValue? initialValue
  )
    => ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull(
      mode,
      initialValue,
      static async (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
      )
        => await gpPins.ConfigureAllGpSettingsAsync(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull))]
  public void ConfigureAllGpSettings_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull(
    PinMode? mode,
    PinValue? initialValue
  )
    => ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull(
      mode,
      initialValue,
      static (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
       ) => {
        gpPins.ConfigureAllGpSettings(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        );
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_ModesAndInitialValuesMustBeIgnoredIfFunctionIsNull(
    PinMode? mode,
    PinValue? initialValue,
    Func<
      IGpControllerGroup,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      ValueTask
    > configureAllGpSettingsAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    GpFunction? gp0NullFunction = null;
    GpFunction? gp1NullFunction = null;
    GpFunction? gp2NullFunction = null;
    GpFunction? gp3NullFunction = null;

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAllGpSettingsAsyncFunc(
        mcp2221A.GpPins,
        gp0NullFunction,
        mode,
        initialValue,
        gp1NullFunction,
        mode,
        initialValue,
        gp2NullFunction,
        mode,
        initialValue,
        gp3NullFunction,
        mode,
        initialValue
      ),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController()
  {
    GpFunction? nullFunction = null;
    PinValue? nullValue = null;
    PinMode? nullMode = null;

    // GP0-GP3 GPIO
    yield return new object?[] { new[] { 0 }, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0 }, GpFunction.Gpio, PinMode.Output, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0 }, GpFunction.Gpio, nullMode, PinValue.High, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };

    yield return new object?[] { new[] { 1 }, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 1 };
    yield return new object?[] { new[] { 1 }, nullFunction, nullMode, nullValue, GpFunction.Gpio, PinMode.Output, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 1 };
    yield return new object?[] { new[] { 1 }, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, PinValue.High, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 1 };

    yield return new object?[] { new[] { 2 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, 2 };
    yield return new object?[] { new[] { 2 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, PinMode.Output, nullValue, nullFunction, nullMode, nullValue, 2 };
    yield return new object?[] { new[] { 2 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, PinValue.High, nullFunction, nullMode, nullValue, 2 };

    yield return new object?[] { new[] { 3 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 3 };
    yield return new object?[] { new[] { 3 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, PinMode.Output, nullValue, 3 };
    yield return new object?[] { new[] { 3 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, PinValue.High, 3 };

    // GP0
    foreach (var function in new[] {
      GpFunction.UsbSuspendStatus,
      GpFunction.LedOutput,
    }) {
      yield return new object?[] {
        new[] { 0, 1, 2, 3 },
        function, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        0
      };
    }

    // GP1
    foreach (var function in new[] {
      GpFunction.ClockOutput,
      GpFunction.Adc,
      GpFunction.LedOutput,
      GpFunction.ExternalInterrupt,
    }) {
      yield return new object?[] {
        new[] { 0, 1, 2, 3 },
        nullFunction, nullMode, nullValue,
        function, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        1
      };
    }

    // GP2
    foreach (var function in new[] {
      GpFunction.UsbConfigureStatus,
      GpFunction.Adc,
      GpFunction.Dac,
    }) {
      yield return new object?[] {
        new[] { 0, 1, 2, 3 },
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        function, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        2
      };
    }

    // GP3
    foreach (var function in new[] {
      GpFunction.LedOutput,
      GpFunction.Adc,
      GpFunction.Dac,
    }) {
      yield return new object?[] {
        new[] { 0, 1, 2, 3 },
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        nullFunction, nullMode, nullValue,
        function, nullMode, nullValue,
        3
      };
    }

    yield return new object?[] { new[] { 0, 1, 2, 3 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0, 1, 2, 3 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0, 1, 2, 3 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0, 1, 2, 3 }, GpFunction.Gpio, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, 0 };

    yield return new object?[] { new[] { 0, 1, 2, 3 }, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 1 };
    yield return new object?[] { new[] { 0, 1, 2, 3 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 2 };
    yield return new object?[] { new[] { 0, 1, 2, 3 }, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, nullFunction, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 3 };

    yield return new object?[] { new[] { 0, 1, 2 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0, 1 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 0 };
    yield return new object?[] { new[] { 0 }, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, GpFunction.Gpio, nullMode, nullValue, 0 };
  }

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController))]
  public void ConfigureAllGpSettingsAsync_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    GpFunction? gp0Function,
    PinMode? gp0Mode,
    PinValue? gp0InitialValue,
    GpFunction? gp1Function,
    PinMode? gp1Mode,
    PinValue? gp1InitialValue,
    GpFunction? gp2Function,
    PinMode? gp2Mode,
    PinValue? gp2InitialValue,
    GpFunction? gp3Function,
    PinMode? gp3Mode,
    PinValue? gp3InitialValue,
    int expectedGpIndexInThrownException
  )
    => ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController(
      pinNumbersToBeOpened,
      gp0Function,
      gp0Mode,
      gp0InitialValue,
      gp1Function,
      gp1Mode,
      gp1InitialValue,
      gp2Function,
      gp2Mode,
      gp2InitialValue,
      gp3Function,
      gp3Mode,
      gp3InitialValue,
      expectedGpIndexInThrownException,
      static async (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
      )
        => await gpPins.ConfigureAllGpSettingsAsync(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        ).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController))]
  public void ConfigureAllGpSettings_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    GpFunction? gp0Function,
    PinMode? gp0Mode,
    PinValue? gp0InitialValue,
    GpFunction? gp1Function,
    PinMode? gp1Mode,
    PinValue? gp1InitialValue,
    GpFunction? gp2Function,
    PinMode? gp2Mode,
    PinValue? gp2InitialValue,
    GpFunction? gp3Function,
    PinMode? gp3Mode,
    PinValue? gp3InitialValue,
    int expectedGpIndexInThrownException
  )
    => ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController(
      pinNumbersToBeOpened,
      gp0Function,
      gp0Mode,
      gp0InitialValue,
      gp1Function,
      gp1Mode,
      gp1InitialValue,
      gp2Function,
      gp2Mode,
      gp2InitialValue,
      gp3Function,
      gp3Mode,
      gp3InitialValue,
      expectedGpIndexInThrownException,
      static (
        gpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
       ) => {
        gpPins.ConfigureAllGpSettings(
          gp0Function,
          gp0Mode,
          gp0InitialValue,
          gp1Function,
          gp1Mode,
          gp1InitialValue,
          gp2Function,
          gp2Mode,
          gp2InitialValue,
          gp3Function,
          gp3Mode,
          gp3InitialValue
        );
        return default;
      }
    );

  private void ConfigureAllGpSettingsSyncOrAsync_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    GpFunction? gp0Function,
    PinMode? gp0Mode,
    PinValue? gp0InitialValue,
    GpFunction? gp1Function,
    PinMode? gp1Mode,
    PinValue? gp1InitialValue,
    GpFunction? gp2Function,
    PinMode? gp2Mode,
    PinValue? gp2InitialValue,
    GpFunction? gp3Function,
    PinMode? gp3Mode,
    PinValue? gp3InitialValue,
    int expectedGpIndexInThrownException,
    Func<
      IGpControllerGroup,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      GpFunction?,
      PinMode?,
      PinValue?,
      ValueTask
    > configureAllGpSettingsAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

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

    var initialGp0Function = mcp2221A.GpPin0.CurrentFunction;
    var initialGp1Function = mcp2221A.GpPin1.CurrentFunction;
    var initialGp2Function = mcp2221A.GpPin2.CurrentFunction;
    var initialGp3Function = mcp2221A.GpPin3.CurrentFunction;

    for (var i = 0; i < pinNumbersToBeOpened.Length; i++) {
      Assert.That(
        () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
          _ =
#endif
          mcp2221A.GpioController.OpenPin(pinNumbersToBeOpened[i]),
        Throws.Nothing
      );
    }

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await configureAllGpSettingsAsyncFunc(
        mcp2221A.GpPins,
        gp0Function,
        gp0Mode,
        gp0InitialValue,
        gp1Function,
        gp1Mode,
        gp1InitialValue,
        gp2Function,
        gp2Mode,
        gp2InitialValue,
        gp3Function,
        gp3Mode,
        gp3InitialValue
      ),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{expectedGpIndexInThrownException}")
        .And
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(GpioController))
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(initialGp0Function));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(initialGp1Function));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(initialGp2Function));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(initialGp3Function));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(initialGp3Mode));
  }

  [Test]
  public void FetchGpioStatesAsync_Disposed()
    => FetchGpioStatesSyncOrAsync_Disposed(
      static async gpPins => await gpPins.FetchGpioStatesAsync(default, default, default).ConfigureAwait(false)
    );

  [Test]
  public void FetchGpioStates_Disposed()
    => FetchGpioStatesSyncOrAsync_Disposed(
      static gpPins => {
        gpPins.FetchGpioStates(default, default, default);
        return default;
      }
    );

  private void FetchGpioStatesSyncOrAsync_Disposed(
    Func<IGpControllerGroup, ValueTask> fetchGpioStatesAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await fetchGpioStatesAsyncFunc(mcp2221A.GpPins),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  [Test]
  public void FetchGpioStatesAsync_CancellationRequested()
    => FetchGpioStatesSyncOrAsync_CancellationRequested(
      static async (gpPins, ct) => await gpPins.FetchGpioStatesAsync(default, default, ct).ConfigureAwait(false)
    );

  [Test]
  public void FetchGpioStates_CancellationRequested()
    => FetchGpioStatesSyncOrAsync_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.FetchGpioStates(default, default, ct);
        return default;
      }
    );

  private void FetchGpioStatesSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> fetchGpioStatesAsyncFunc
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
      async () => await fetchGpioStatesAsyncFunc(mcp2221A.GpPins, cts.Token),
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

  [Test]
  public void FetchGpioStatesAsync_Empty()
    => FetchGpioStatesSyncOrAsync_Empty(
      static async gpPins => await gpPins.FetchGpioStatesAsync(default, default).ConfigureAwait(false)
    );

  [Test]
  public void FetchGpioStates_Empty()
    => FetchGpioStatesSyncOrAsync_Empty(
      static gpPins => {
        gpPins.FetchGpioStates(default, default);
        return default;
      }
    );

  private void FetchGpioStatesSyncOrAsync_Empty(
    Func<IGpControllerGroup, ValueTask> fetchGpioStatesAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // LOW - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // HIGH - INPUT - GPIO operation (GPIO2)
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
      "01-00-", // HIGH - OUTPUT
      "01-01-", // HIGH - INPUT
      "00-00-", // LOW - OUTPUT
      "00-01-", // LOW - INPUT
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Assert.That(
      async () => await fetchGpioStatesAsyncFunc(mcp2221A.GpPins),
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

    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(PinMode.Input));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(PinMode.Output));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(PinMode.Input));
  }

  public enum ExpectedValue {
    Initial,
    High,
    Low,
    Exception,
  }

  public enum ExpectedMode {
    Initial,
    Output,
    Input,
    Exception,
  }

  private static System.Collections.IEnumerable YieldTestCases_FetchGpioStatesSyncOrAsync_NotSetForGpioOperation()
  {
    // [MCP2221A] 3.1.12 GET GPIO VALUES
    const byte GpLO = 0x00; // GP<n> pin value: LOW
    const byte GpHI = 0x01; // GP<n> pin value: HIGH
    const byte GpEE = 0xEE; // GP<n> pin value: GP<n> is not set for GPIO operation
    const byte GpOP = 0x00; // GP<n> direction value: OUTPUT
    const byte GpIP = 0x01; // GP<n> direction value: INPUT
    const byte GpEF = 0xEF; // GP<n> direction value: GP<n> is not set for GPIO operation

#pragma warning disable CA1825
    // InvalidOperationException will be thrown
    yield return new object?[] {
      new byte[] { GpEE, GpIP, GpLO, GpIP, GpLO, GpIP, GpLO, GpIP },
      new PinValuePair[] { new(0, default) },
      new PinModePair[] { },
      0,
      new[] { ExpectedValue.Exception, ExpectedValue.Low, ExpectedValue.Low, ExpectedValue.Low },
      new[] { ExpectedMode.Input, ExpectedMode.Input, ExpectedMode.Input, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpEF, GpLO, GpIP, GpLO, GpIP, GpLO, GpIP },
      new PinValuePair[] { },
      new PinModePair[] { new(0, default) },
      0,
      new[] { ExpectedValue.High, ExpectedValue.Low, ExpectedValue.Low, ExpectedValue.Low },
      new[] { ExpectedMode.Exception, ExpectedMode.Input, ExpectedMode.Input, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpEE, GpIP, GpLO, GpIP, GpLO, GpIP },
      new PinValuePair[] { new(1, default) },
      new PinModePair[] { },
      1,
      new[] { ExpectedValue.High, ExpectedValue.Exception, ExpectedValue.Low, ExpectedValue.Low },
      new[] { ExpectedMode.Output, ExpectedMode.Input, ExpectedMode.Input, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpHI, GpEF, GpLO, GpIP, GpLO, GpIP },
      new PinValuePair[] { },
      new PinModePair[] { new(1, default) },
      1,
      new[] { ExpectedValue.High, ExpectedValue.High, ExpectedValue.Low, ExpectedValue.Low },
      new[] { ExpectedMode.Output, ExpectedMode.Exception, ExpectedMode.Input, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpHI, GpOP, GpEE, GpIP, GpLO, GpIP },
      new PinValuePair[] { new(2, default) },
      new PinModePair[] { },
      2,
      new[] { ExpectedValue.High, ExpectedValue.High, ExpectedValue.Exception, ExpectedValue.Low },
      new[] { ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Input, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpHI, GpOP, GpHI, GpEF, GpLO, GpIP },
      new PinValuePair[] { },
      new PinModePair[] { new(2, default) },
      2,
      new[] { ExpectedValue.High, ExpectedValue.High, ExpectedValue.High, ExpectedValue.Low },
      new[] { ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Exception, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpHI, GpOP, GpHI, GpOP, GpEE, GpIP },
      new PinValuePair[] { new(3, default) },
      new PinModePair[] { },
      3,
      new[] { ExpectedValue.High, ExpectedValue.High, ExpectedValue.High, ExpectedValue.Exception },
      new[] { ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Input },
    };
    yield return new object?[] {
      new byte[] { GpHI, GpOP, GpHI, GpOP, GpHI, GpOP, GpHI, GpEF },
      new PinValuePair[] { },
      new PinModePair[] { new(3, default) },
      3,
      new[] { ExpectedValue.High, ExpectedValue.High, ExpectedValue.High, ExpectedValue.High },
      new[] { ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Output, ExpectedMode.Exception },
    };

    // no exception will be thrown
    yield return new object?[] {
      new byte[] { GpHI, GpEF, GpLO, GpEF, GpHI, GpEF, GpLO, GpEF },
      new PinValuePair[] { new(0, default), new(1, default), new(2, default), new(3, default) },
      new PinModePair[] { },
      null,
      new[] { ExpectedValue.High, ExpectedValue.Low, ExpectedValue.High, ExpectedValue.Low },
      new[] { ExpectedMode.Exception, ExpectedMode.Exception, ExpectedMode.Exception, ExpectedMode.Exception },
    };
    yield return new object?[] {
      new byte[] { GpEE, GpIP, GpEE, GpOP, GpEE, GpIP, GpEE, GpOP },
      new PinValuePair[] { },
      new PinModePair[] { new(0, default), new(1, default), new(2, default), new(3, default) },
      null,
      new[] { ExpectedValue.Exception, ExpectedValue.Exception, ExpectedValue.Exception, ExpectedValue.Exception },
      new[] { ExpectedMode.Input, ExpectedMode.Output, ExpectedMode.Input, ExpectedMode.Output },
    };
#pragma warning restore CA1825
  }

  [TestCaseSource(nameof(YieldTestCases_FetchGpioStatesSyncOrAsync_NotSetForGpioOperation))]
  public void FetchGpioStatesAsync_NotSetForGpioOperation(
    byte[] pinAndDirectionValuesOfGetGpioValuesResponse,
    PinValuePair[] pinValuePairsToFetch,
    PinModePair[] pinModePairsToFetch,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes
  )
    => FetchGpioStatesSyncOrAsync_NotSetForGpioOperation(
      pinAndDirectionValuesOfGetGpioValuesResponse,
      pinValuePairsToFetch,
      pinModePairsToFetch,
      expectedGpIndexInThrownException,
      expectedLastUpdatedValues,
      expectedCurrentModes,
      static async (gpPins, pinValuePairsToFetch, pinModePairsToFetch)
        => await gpPins.FetchGpioStatesAsync(pinValuePairsToFetch, pinModePairsToFetch).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_FetchGpioStatesSyncOrAsync_NotSetForGpioOperation))]
  public void FetchGpioStates_NotSetForGpioOperation(
    byte[] pinAndDirectionValuesOfGetGpioValuesResponse,
    PinValuePair[] pinValuePairsToFetch,
    PinModePair[] pinModePairsToFetch,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes
  )
    => FetchGpioStatesSyncOrAsync_NotSetForGpioOperation(
      pinAndDirectionValuesOfGetGpioValuesResponse,
      pinValuePairsToFetch,
      pinModePairsToFetch,
      expectedGpIndexInThrownException,
      expectedLastUpdatedValues,
      expectedCurrentModes,
      static (gpPins, pinValuePairsToFetch, pinModePairsToFetch) => {
        gpPins.FetchGpioStates(pinValuePairsToFetch.Span, pinModePairsToFetch.Span);
        return default;
      }
    );

  private void FetchGpioStatesSyncOrAsync_NotSetForGpioOperation(
    byte[] pinAndDirectionValuesOfGetGpioValuesResponse,
    PinValuePair[] pinValuePairsToFetch,
    PinModePair[] pinModePairsToFetch,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes,
    Func<IGpControllerGroup, Memory<PinValuePair>, Memory<PinModePair>, ValueTask> fetchGpioStatesAsyncFunc
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
      BitConverter.ToString(pinAndDirectionValuesOfGetGpioValuesResponse),
      "-",
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Assert.That(
      async () => await fetchGpioStatesAsyncFunc(mcp2221A.GpPins, pinValuePairsToFetch, pinModePairsToFetch),
      expectedGpIndexInThrownException.HasValue
        ? Throws
          .InvalidOperationException
          .With
          .Property(nameof(InvalidOperationException.Message))
          .Contains($"GP{expectedGpIndexInThrownException.Value}")
        : Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    for (var i = 0; i < 4; i++) {
      if (expectedLastUpdatedValues[i] == ExpectedValue.Exception) {
        Assert.That(
          () => _ = mcp2221A.GpPins[i].LastUpdatedValue,
          Throws
            .InvalidOperationException
            .With
            .Property(nameof(InvalidOperationException.Message))
            .Contains($"GP{i}"),
          $"{nameof(GpController.LastUpdatedValue)} GP{i}"
        );
      }
      else {
        Assert.That(
          mcp2221A.GpPins[i].LastUpdatedValue,
          expectedLastUpdatedValues[i] == ExpectedValue.High
            ? Is.EqualTo(PinValue.High)
            : Is.EqualTo(PinValue.Low),
          $"{nameof(GpController.LastUpdatedValue)} GP{i}"
        );
      }

      if (expectedCurrentModes[i] == ExpectedMode.Exception) {
        Assert.That(
          () => _ = mcp2221A.GpPins[i].CurrentMode,
          Throws
            .InvalidOperationException
            .With
            .Property(nameof(InvalidOperationException.Message))
            .Contains($"GP{i}"),
          $"{nameof(GpController.CurrentMode)} GP{i}"
        );
      }
      else {
        Assert.That(
          mcp2221A.GpPins[i].CurrentMode,
          expectedCurrentModes[i] == ExpectedMode.Output
            ? Is.EqualTo(PinMode.Output)
            : Is.EqualTo(PinMode.Input),
          $"{nameof(GpController.CurrentMode)} GP{i}"
        );
      }
    }
  }

  [Test]
  public void ApplyGpioStatesAsync_Disposed()
    => ApplyGpioStatesSyncOrAsync_Disposed(
      static async gpPins => await gpPins.ApplyGpioStatesAsync(default, default, default).ConfigureAwait(false)
    );

  [Test]
  public void ApplyGpioStates_Disposed()
    => ApplyGpioStatesSyncOrAsync_Disposed(
      static gpPins => {
        gpPins.ApplyGpioStates(default, default, default);
        return default;
      }
    );

  private void ApplyGpioStatesSyncOrAsync_Disposed(
    Func<IGpControllerGroup, ValueTask> applyGpioStatesAsyncFunc
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    mcp2221A.Dispose();

    Assert.That(
      async () => await applyGpioStatesAsyncFunc(mcp2221A.GpPins),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  [Test]
  public void ApplyGpioStatesAsync_CancellationRequested()
    => ApplyGpioStatesSyncOrAsync_CancellationRequested(
      static async (gpPins, ct) => await gpPins.ApplyGpioStatesAsync(default, default, ct).ConfigureAwait(false)
    );

  [Test]
  public void ApplyGpioStates_CancellationRequested()
    => ApplyGpioStatesSyncOrAsync_CancellationRequested(
      static (gpPins, ct) => {
        gpPins.ApplyGpioStates(default, default, ct);
        return default;
      }
    );

  private void ApplyGpioStatesSyncOrAsync_CancellationRequested(
    Func<IGpControllerGroup, CancellationToken, ValueTask> applyGpioStatesAsyncFunc
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
      async () => await applyGpioStatesAsyncFunc(mcp2221A.GpPins, cts.Token),
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

  private static System.Collections.IEnumerable YieldTestCases_ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation()
  {
    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
    // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [2 + 4n]: Alter GP<n> pin direction: (value other than 0)=enable
    // [3 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input

    const byte GpLO = 0x00; // GP<n> pin value: LOW
    const byte GpHI = 0x01; // GP<n> pin value: HIGH
    const byte GpOP = 0x00; // GP<n> direction value: OUTPUT
    const byte GpIP = 0x01; // GP<n> direction value: INPUT
    const byte GpEE = 0xEE; // GP<n> pin/direction value: GP<n> is not set for GPIO operation
    const byte Gp00 = 0x00; // Alter GP<n> output/pin direction: disable
    const byte GpFF = 0xFF; // Alter GP<n> output/pin direction: enable

    static byte[] Gp0(byte alterOutput, byte output, byte alterDirection, byte direction) => [alterOutput, output, alterDirection, direction];
    static byte[] Gp1(byte alterOutput, byte output, byte alterDirection, byte direction) => [alterOutput, output, alterDirection, direction];
    static byte[] Gp2(byte alterOutput, byte output, byte alterDirection, byte direction) => [alterOutput, output, alterDirection, direction];
    static byte[] Gp3(byte alterOutput, byte output, byte alterDirection, byte direction) => [alterOutput, output, alterDirection, direction];
    static byte[] GpioOutputValues(ReadOnlySpan<byte> gp0, ReadOnlySpan<byte> gp1, ReadOnlySpan<byte> gp2, ReadOnlySpan<byte> gp3)
      => [.. gp0, .. gp1, .. gp2, .. gp3];
    static byte[] GpioOutputResponse(ReadOnlySpan<byte> gp0, ReadOnlySpan<byte> gp1, ReadOnlySpan<byte> gp2, ReadOnlySpan<byte> gp3)
      => [.. gp0, .. gp1, .. gp2, .. gp3];

#pragma warning disable CA1825
    // InvalidOperationException will be thrown
    yield return new object?[] {
      new PinValuePair[] { new(0, PinValue.High) },
      new PinModePair[] { },
      GpioOutputResponse(Gp0(GpFF, GpEE, Gp00, GpOP), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0xFF, 0xFF, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      0,
      new[] { ExpectedValue.Exception, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Output, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { },
      new PinModePair[] { new(0, PinMode.Input) },
      GpioOutputResponse(Gp0(Gp00, GpHI, GpFF, GpEE), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0xFF, 0xFF), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      0,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Exception, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { new(1, PinValue.High) },
      new PinModePair[] { },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(GpFF, GpEE, Gp00, GpOP), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0xFF, 0xFF, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      1,
      new[] { ExpectedValue.Initial, ExpectedValue.Exception, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { },
      new PinModePair[] { new(1, PinMode.Input) },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(Gp00, GpHI, GpFF, GpEE), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0xFF, 0xFF), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      1,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Exception, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { new(2, PinValue.High) },
      new PinModePair[] { },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(GpFF, GpEE, Gp00, GpOP), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0xFF, 0xFF, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      2,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Exception, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { },
      new PinModePair[] { new(2, PinMode.Input) },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(Gp00, GpHI, GpFF, GpEE), Gp3(Gp00, GpHI, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0xFF, 0xFF), Gp3(0x00, 0x00, 0x00, 0x00)),
      2,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Exception, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { new(3, PinValue.High) },
      new PinModePair[] { },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(GpFF, GpEE, Gp00, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0xFF, 0xFF, 0x00, 0x00)),
      3,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Exception },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { },
      new PinModePair[] { new(3, PinMode.Input) },
      GpioOutputResponse(Gp0(Gp00, GpHI, Gp00, GpOP), Gp1(Gp00, GpHI, Gp00, GpOP), Gp2(Gp00, GpHI, Gp00, GpOP), Gp3(Gp00, GpHI, GpFF, GpEE)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0xFF, 0xFF)),
      3,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Exception },
    };

    // no exception will be thrown
    yield return new object?[] {
      new PinValuePair[] { },
      new PinModePair[] { },
      GpioOutputResponse(Gp0(Gp00, GpEE, Gp00, GpEE), Gp1(Gp00, GpEE, Gp00, GpEE), Gp2(Gp00, GpEE, Gp00, GpEE), Gp3(Gp00, GpEE, Gp00, GpEE)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0x00, 0x00, 0x00, 0x00)),
      null,
      new[] { ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial, ExpectedValue.Initial },
      new[] { ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial, ExpectedMode.Initial },
    };
    yield return new object?[] {
      new PinValuePair[] { new(1, PinValue.High), new(2, PinValue.Low), new(3, PinValue.High) },
      new PinModePair[] { new(1, PinMode.Output), new(2, PinMode.Input), new(3, PinMode.Output) },
      GpioOutputResponse(Gp0(Gp00, GpEE, Gp00, GpEE), Gp1(GpFF, GpHI, GpFF, GpOP), Gp2(GpFF, GpLO, GpFF, GpIP), Gp3(GpFF, GpHI, GpFF, GpOP)),
      GpioOutputValues(Gp0(0x00, 0x00, 0x00, 0x00), Gp1(0xFF, 0xFF, 0xFF, 0x00), Gp2(0xFF, 0x00, 0xFF, 0xFF), Gp3(0xFF, 0xFF, 0xFF, 0x00)),
      null,
      new[] { ExpectedValue.Initial, ExpectedValue.High, ExpectedValue.Low, ExpectedValue.High },
      new[] { ExpectedMode.Initial, ExpectedMode.Output, ExpectedMode.Input, ExpectedMode.Output },
    };
    yield return new object?[] {
      new PinValuePair[] { new(0, PinValue.Low), new(2, PinValue.Low), new(3, PinValue.High) },
      new PinModePair[] { new(0, PinMode.Input), new(2, PinMode.Input), new(3, PinMode.Output) },
      GpioOutputResponse(Gp0(GpFF, GpLO, GpFF, GpIP), Gp1(Gp00, GpEE, Gp00, GpEE), Gp2(GpFF, GpLO, GpFF, GpIP), Gp3(GpFF, GpHI, GpFF, GpOP)),
      GpioOutputValues(Gp0(0xFF, 0x00, 0xFF, 0xFF), Gp1(0x00, 0x00, 0x00, 0x00), Gp2(0xFF, 0x00, 0xFF, 0xFF), Gp3(0xFF, 0xFF, 0xFF, 0x00)),
      null,
      new[] { ExpectedValue.Low, ExpectedValue.Initial, ExpectedValue.Low, ExpectedValue.High },
      new[] { ExpectedMode.Input, ExpectedMode.Initial, ExpectedMode.Input, ExpectedMode.Output },
    };
    yield return new object?[] {
      new PinValuePair[] { new(0, PinValue.Low), new(1, PinValue.High), new(3, PinValue.High) },
      new PinModePair[] { new(0, PinMode.Input), new(1, PinMode.Output), new(3, PinMode.Output) },
      GpioOutputResponse(Gp0(GpFF, GpLO, GpFF, GpIP), Gp1(GpFF, GpHI, GpFF, GpOP), Gp2(Gp00, GpEE, Gp00, GpEE), Gp3(GpEE, GpHI, GpFF, GpOP)),
      GpioOutputValues(Gp0(0xFF, 0x00, 0xFF, 0xFF), Gp1(0xFF, 0xFF, 0xFF, 0x00), Gp2(0x00, 0x00, 0x00, 0x00), Gp3(0xFF, 0xFF, 0xFF, 0x00)),
      null,
      new[] { ExpectedValue.Low, ExpectedValue.High, ExpectedValue.Initial, ExpectedValue.High },
      new[] { ExpectedMode.Input, ExpectedMode.Output, ExpectedMode.Initial, ExpectedMode.Output },
    };
    yield return new object?[] {
      new PinValuePair[] { new(0, PinValue.Low), new(1, PinValue.High), new(2, PinValue.Low) },
      new PinModePair[] { new(0, PinMode.Input), new(1, PinMode.Output), new(2, PinMode.Input) },
      GpioOutputResponse(Gp0(GpFF, GpLO, GpFF, GpIP), Gp1(GpFF, GpHI, GpFF, GpOP), Gp2(GpFF, GpLO, GpFF, GpIP), Gp3(Gp00, GpEE, Gp00, GpEE)),
      GpioOutputValues(Gp0(0xFF, 0x00, 0xFF, 0xFF), Gp1(0xFF, 0xFF, 0xFF, 0x00), Gp2(0xFF, 0x00, 0xFF, 0xFF), Gp3(0x00, 0x00, 0x00, 0x00)),
      null,
      new[] { ExpectedValue.Low, ExpectedValue.High, ExpectedValue.Low, ExpectedValue.Initial },
      new[] { ExpectedMode.Input, ExpectedMode.Output, ExpectedMode.Input, ExpectedMode.Initial },
    };
#pragma warning restore CA1825
  }

  [TestCaseSource(nameof(YieldTestCases_ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation))]
  public void ApplyGpioStatesAsync_NotSetForGpioOperation(
    PinValuePair[] pinValuePairsToApply,
    PinModePair[] pinModePairsToApply,
    byte[] pinAndDirectionValuesOfSetGpioOutputValuesResponse,
    byte[] gpioOutputsInExpectedSentCommand,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes
  )
    => ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation(
      pinValuePairsToApply,
      pinModePairsToApply,
      pinAndDirectionValuesOfSetGpioOutputValuesResponse,
      gpioOutputsInExpectedSentCommand,
      expectedGpIndexInThrownException,
      expectedLastUpdatedValues,
      expectedCurrentModes,
      static async (gpPins, pinValuePairsToApply, pinModePairsToApply)
        => await gpPins.ApplyGpioStatesAsync(pinValuePairsToApply, pinModePairsToApply).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation))]
  public void ApplyGpioStates_NotSetForGpioOperation(
    PinValuePair[] pinValuePairsToApply,
    PinModePair[] pinModePairsToApply,
    byte[] pinAndDirectionValuesOfSetGpioOutputValuesResponse,
    byte[] gpioOutputsInExpectedSentCommand,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes
  )
    => ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation(
      pinValuePairsToApply,
      pinModePairsToApply,
      pinAndDirectionValuesOfSetGpioOutputValuesResponse,
      gpioOutputsInExpectedSentCommand,
      expectedGpIndexInThrownException,
      expectedLastUpdatedValues,
      expectedCurrentModes,
      static (gpPins, pinValuePairsToApply, pinModePairsToApply) => {
        gpPins.ApplyGpioStates(pinValuePairsToApply.Span, pinModePairsToApply.Span);
        return default;
      }
    );

  private void ApplyGpioStatesSyncOrAsync_NotSetForGpioOperation(
    PinValuePair[] pinValuePairsToApply,
    PinModePair[] pinModePairsToApply,
    byte[] pinAndDirectionValuesOfSetGpioOutputValuesResponse,
    byte[] gpioOutputsInExpectedSentCommand,
    int? expectedGpIndexInThrownException,
    ExpectedValue[] expectedLastUpdatedValues,
    ExpectedMode[] expectedCurrentModes,
    Func<IGpControllerGroup, Memory<PinValuePair>, Memory<PinModePair>, ValueTask> applyGpioStatesAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

    var initialValues = new[] {
      PinValue.High,
      PinValue.Low,
      PinValue.High,
      PinValue.Low,
    };
    var initialModes = new[] {
      PinMode.Output,
      PinMode.Output,
      PinMode.Input,
      PinMode.Input,
    };

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
      BitConverter.ToString(pinAndDirectionValuesOfSetGpioOutputValuesResponse),
      "-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully
    gpioOutputsInExpectedSentCommand.CopyTo(expectedSentCommand.AsSpan(2, 16));

    Assert.That(
      async () => await applyGpioStatesAsyncFunc(mcp2221A.GpPins, pinValuePairsToApply, pinModePairsToApply),
      expectedGpIndexInThrownException.HasValue
        ? Throws
          .TypeOf<InvalidOperationException>()
          .With
          .Property(nameof(InvalidOperationException.Message))
          .Contains($"GP{expectedGpIndexInThrownException.Value}")
        : Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    for (var i = 0; i < 4; i++) {
      if (expectedLastUpdatedValues[i] == ExpectedValue.Exception) {
        Assert.That(
          () => _ = mcp2221A.GpPins[i].LastUpdatedValue,
          Throws
            .InvalidOperationException
            .With
            .Property(nameof(InvalidOperationException.Message))
            .Contains($"GP{i}"),
          $"{nameof(GpController.LastUpdatedValue)} GP{i}"
        );
      }
      else {
        Assert.That(
          mcp2221A.GpPins[i].LastUpdatedValue,
          expectedLastUpdatedValues[i] switch {
            ExpectedValue.High => Is.EqualTo(PinValue.High),
            ExpectedValue.Low => Is.EqualTo(PinValue.Low),
            _ => Is.EqualTo(initialValues[i]),
          },
          $"{nameof(GpController.LastUpdatedValue)} GP{i}"
        );
      }

      if (expectedCurrentModes[i] == ExpectedMode.Exception) {
        Assert.That(
          () => _ = mcp2221A.GpPins[i].CurrentMode,
          Throws
            .InvalidOperationException
            .With
            .Property(nameof(InvalidOperationException.Message))
            .Contains($"GP{i}"),
          $"{nameof(GpController.CurrentMode)} GP{i}"
        );
      }
      else {
        Assert.That(
          mcp2221A.GpPins[i].CurrentMode,
          expectedCurrentModes[i] switch {
            ExpectedMode.Output => Is.EqualTo(PinMode.Output),
            ExpectedMode.Input => Is.EqualTo(PinMode.Input),
            _ => Is.EqualTo(initialModes[i]),
          },
          $"{nameof(GpController.CurrentMode)} GP{i}"
        );
      }
    }
  }

  private static System.Collections.IEnumerable YieldTestCases_ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController()
  {
    yield return new object[] { new[] { 0 }, new PinValuePair[] { new(0, default) }, new PinModePair[] { }, 0 };
    yield return new object[] { new[] { 0 }, new PinValuePair[] { }, new PinModePair[] { new(0, default) }, 0 };
    yield return new object[] { new[] { 1 }, new PinValuePair[] { new(1, default) }, new PinModePair[] { }, 1 };
    yield return new object[] { new[] { 1 }, new PinValuePair[] { }, new PinModePair[] { new(1, default) }, 1 };
    yield return new object[] { new[] { 2 }, new PinValuePair[] { new(2, default) }, new PinModePair[] { }, 2 };
    yield return new object[] { new[] { 2 }, new PinValuePair[] { }, new PinModePair[] { new(2, default) }, 2 };
    yield return new object[] { new[] { 3 }, new PinValuePair[] { new(3, default) }, new PinModePair[] { }, 3 };
    yield return new object[] { new[] { 3 }, new PinValuePair[] { }, new PinModePair[] { new(3, default) }, 3 };

    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, new PinModePair[] { }, 3 };
    yield return new object[] { new[] { 0, 1, 2 }, new PinValuePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, new PinModePair[] { }, 2 };
    yield return new object[] { new[] { 0, 1 }, new PinValuePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, new PinModePair[] { }, 1 };
    yield return new object[] { new[] { 0 }, new PinValuePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, new PinModePair[] { }, 0 };

    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { new(0, default), new(1, default), new(2, default), new(3, default) }, new PinModePair[] { }, 0 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { new(1, default), new(2, default), new(3, default) }, new PinModePair[] { }, 1 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { new(2, default), new(3, default) }, new PinModePair[] { }, 2 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { new(3, default) }, new PinModePair[] { }, 3 };

    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { }, new PinModePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, 3 };
    yield return new object[] { new[] { 0, 1, 2 }, new PinValuePair[] { }, new PinModePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, 2 };
    yield return new object[] { new[] { 0, 1 }, new PinValuePair[] { }, new PinModePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, 1 };
    yield return new object[] { new[] { 0 }, new PinValuePair[] { }, new PinModePair[] { new(3, default), new(2, default), new(1, default), new(0, default) }, 0 };

    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { }, new PinModePair[] { new(0, default), new(1, default), new(2, default), new(3, default) }, 0 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { }, new PinModePair[] { new(1, default), new(2, default), new(3, default) }, 1 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { }, new PinModePair[] { new(2, default), new(3, default) }, 2 };
    yield return new object[] { new[] { 0, 1, 2, 3 }, new PinValuePair[] { }, new PinModePair[] { new(3, default) }, 3 };
  }

  [TestCaseSource(nameof(YieldTestCases_ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController))]
  public void ApplyGpioStatesAsync_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    PinValuePair[] pinValuePairs,
    PinModePair[] pinModePairs,
    int expectedGpIndexInThrownException
  )
    => ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController(
      pinNumbersToBeOpened,
      pinValuePairs,
      pinModePairs,
      expectedGpIndexInThrownException,
      static async (gpPins, pinValuePairs, pinModePairs)
        => await gpPins.ApplyGpioStatesAsync(pinValuePairs, pinModePairs).ConfigureAwait(false)
    );

  [TestCaseSource(nameof(YieldTestCases_ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController))]
  public void ApplyGpioStates_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    PinValuePair[] pinValuePairs,
    PinModePair[] pinModePairs,
    int expectedGpIndexInThrownException
  )
    => ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController(
      pinNumbersToBeOpened,
      pinValuePairs,
      pinModePairs,
      expectedGpIndexInThrownException,
      static (gpPins, pinValuePairs, pinModePairs) => {
        gpPins.ApplyGpioStates(pinValuePairs.Span, pinModePairs.Span);
        return default;
      }
    );

  private void ApplyGpioStatesSyncOrAsync_ThrowsWhenUsedByGpioController(
    int[] pinNumbersToBeOpened,
    PinValuePair[] pinValuePairs,
    PinModePair[] pinModePairs,
    int expectedGpIndexInThrownException,
    Func<IGpControllerGroup, ReadOnlyMemory<PinValuePair>, ReadOnlyMemory<PinModePair>, ValueTask> applyGpioStatesAsyncFunc
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

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

    for (var i = 0; i < pinNumbersToBeOpened.Length; i++) {
      Assert.That(
        () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
          _ =
#endif
          mcp2221A.GpioController.OpenPin(pinNumbersToBeOpened[i]),
        Throws.Nothing
      );
    }

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      async () => await applyGpioStatesAsyncFunc(mcp2221A.GpPins, pinValuePairs, pinModePairs),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .Contains($"GP{expectedGpIndexInThrownException}")
        .And
        .Property(nameof(InvalidOperationException.Message))
        .Contains(nameof(GpioController))
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPin0.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin0.LastUpdatedValue, Is.EqualTo(initialGp0Value));
    Assert.That(mcp2221A.GpPin0.CurrentMode, Is.EqualTo(initialGp0Mode));

    Assert.That(mcp2221A.GpPin1.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin1.LastUpdatedValue, Is.EqualTo(initialGp1Value));
    Assert.That(mcp2221A.GpPin1.CurrentMode, Is.EqualTo(initialGp1Mode));

    Assert.That(mcp2221A.GpPin2.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin2.LastUpdatedValue, Is.EqualTo(initialGp2Value));
    Assert.That(mcp2221A.GpPin2.CurrentMode, Is.EqualTo(initialGp2Mode));

    Assert.That(mcp2221A.GpPin3.CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPin3.LastUpdatedValue, Is.EqualTo(initialGp3Value));
    Assert.That(mcp2221A.GpPin3.CurrentMode, Is.EqualTo(initialGp3Mode));
  }
}
