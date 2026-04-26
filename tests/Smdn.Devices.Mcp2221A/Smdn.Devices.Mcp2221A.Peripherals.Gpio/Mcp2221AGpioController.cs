// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using SequenceIs = Smdn.Test.NUnit.Constraints.Buffers.Is;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public class Mcp2221AGpioControllerTests {
  [TestCase(int.MinValue)]
  [TestCase(-1)]
  [TestCase(4)]
  [TestCase(int.MaxValue)]
  public void OpenPin_PinNumberOutOfRange(int pinNumber)
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.TypeOf<ArgumentOutOfRangeException>()
    );
  }

  [Test]
  public void OpenPin_CurrentFunctionIsNotGpio(
    [Values(0, 1, 2, 3)] int pinNumber
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_010; // HIGH - INPUT - Alternate Function 0 (LED UART RX)
    const byte InitialGp1Settings = 0b_000_1_0_011; // HIGH - OUTPUT - Alternate Function 1 (LED UART TX)
    const byte InitialGp2Settings = 0b_000_0_1_001; // LOW - INPUT - Dedicated function operation (USBCFG)
    const byte InitialGp3Settings = 0b_000_0_0_001; // LOW - OUTPUT - Dedicated function operation (LED I2C)

    var initialGpValues = new[] {
      PinValue.High,
      PinValue.High,
      PinValue.Low,
      PinValue.Low,
    };
    var initialGpModes = new[] {
      PinMode.Input,
      PinMode.Output,
      PinMode.Input,
      PinMode.Output,
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
    expectedSentCommand[8] = pinNumber == 0 ? (byte)(InitialGp0Settings & 0b_111_1_1_000) : InitialGp0Settings; // [8] GP0 settings
    expectedSentCommand[9] = pinNumber == 1 ? (byte)(InitialGp1Settings & 0b_111_1_1_000) : InitialGp1Settings; // [9] GP1 settings
    expectedSentCommand[10] = pinNumber == 2 ? (byte)(InitialGp2Settings & 0b_111_1_1_000) : InitialGp2Settings; // [10] GP2 settings
    expectedSentCommand[11] = pinNumber == 3 ? (byte)(InitialGp3Settings & 0b_111_1_1_000) : InitialGp3Settings; // [11] GP3 settings

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    GpioPin? pin = null;

    Assert.That(
      () => pin = mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );
#else
    Assert.That(
      () => mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );
#endif

    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(mcp2221A.GpPins[pinNumber].CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPins[pinNumber].LastUpdatedValue, Is.EqualTo(initialGpValues[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].CurrentMode, Is.EqualTo(initialGpModes[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].IsUsedByGpioController, Is.True);

    Assert.That(mcp2221A.GpioController.IsPinOpen(pinNumber), Is.True);

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Assert.That(pin, Is.Not.Null);
    Assert.That(pin.PinNumber, Is.EqualTo(pinNumber));
    Assert.That(pin.Controller, Is.SameAs(mcp2221A.GpioController));
#endif
  }

  [Test]
  public void OpenPin_Disposed(
    [Values(0, 1, 2, 3)] int pinNumber
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

    mcp2221A.Dispose();

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.TypeOf<ObjectDisposedException>()
    );
  }

  [Test]
  public void OpenPin_CurrentFunctionIsGpio(
    [Values(0, 1, 2, 3)] int pinNumber
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

    var initialGpValues = new[] {
      PinValue.High,
      PinValue.High,
      PinValue.Low,
      PinValue.Low,
    };
    var initialGpModes = new[] {
      PinMode.Input,
      PinMode.Output,
      PinMode.Input,
      PinMode.Output,
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    GpioPin? pin = null;

    Assert.That(
      () => pin = mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );
#else
    Assert.That(
      () => mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );
#endif

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPins[pinNumber].CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPins[pinNumber].LastUpdatedValue, Is.EqualTo(initialGpValues[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].CurrentMode, Is.EqualTo(initialGpModes[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].IsUsedByGpioController, Is.True);

    Assert.That(mcp2221A.GpioController.IsPinOpen(pinNumber), Is.True);

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Assert.That(pin, Is.Not.Null);
    Assert.That(pin.PinNumber, Is.EqualTo(pinNumber));
    Assert.That(pin.Controller, Is.SameAs(mcp2221A.GpioController));
#endif
  }

  [Test]
  public void OpenPin_AlreadyOpen(
    [Values(0, 1, 2, 3)] int pinNumber
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW - OUTPUT - GPIO operation (GPIO3)

    var initialGpValues = new[] {
      PinValue.High,
      PinValue.High,
      PinValue.Low,
      PinValue.Low,
    };
    var initialGpModes = new[] {
      PinMode.Input,
      PinMode.Output,
      PinMode.Input,
      PinMode.Output,
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );
    Assert.That(mcp2221A.GpioController.IsPinOpen(pinNumber), Is.True);

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    GpioPin? pin = null;

    Assert.That(
      () => pin = mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing,
      "re-open"
    );
#else
    Assert.That(
      () => mcp2221A.GpioController.OpenPin(pinNumber),
      Throws
        .InvalidOperationException
        .With
        .Property(nameof(InvalidOperationException.Message))
        .EqualTo($"Pin {pinNumber} is already open."),
      "re-open"
    );
#endif

    Assert.That(mcp2221A.GpioController.IsPinOpen(pinNumber), Is.True);

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPins[pinNumber].CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPins[pinNumber].LastUpdatedValue, Is.EqualTo(initialGpValues[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].CurrentMode, Is.EqualTo(initialGpModes[pinNumber]));
    Assert.That(mcp2221A.GpPins[pinNumber].IsUsedByGpioController, Is.True);

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Assert.That(pin, Is.Not.Null);
    Assert.That(pin.PinNumber, Is.EqualTo(pinNumber));
    Assert.That(pin.Controller, Is.SameAs(mcp2221A.GpioController));
#endif
  }

  [Test]
  public void ClosePin(
    [Values(0, 1, 2, 3)] int pinNumber
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    Assert.That(
      () => mcp2221A.GpioController.ClosePin(pinNumber),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(mcp2221A.GpPins[pinNumber].CurrentFunction, Is.EqualTo(GpFunction.Gpio));
    Assert.That(mcp2221A.GpPins[pinNumber].IsUsedByGpioController, Is.False);

    Assert.That(mcp2221A.GpioController.IsPinOpen(pinNumber), Is.False);
  }

  [Test]
  public void SetPinMode_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(PinMode.Input, PinMode.Output)] PinMode mode
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.SetPinMode(pinNumber, mode),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void SetPinMode(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(PinMode.Input, PinMode.Output)] PinMode mode
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      pinNumber == 0 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      pinNumber == 1 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      pinNumber == 2 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      pinNumber == 3 ? $"00-00-FF-{(mode == PinMode.Output ? "00" : "FF")}-" : "00-00-00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

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
      expectedSentCommand[4 + 4 * n] = (byte)(n == pinNumber ? 0xFF : 0x00);
      expectedSentCommand[5 + 4 * n] = (byte)((n == pinNumber) ? (mode == PinMode.Output ? 0x00 : 0xFF) : 0x00);
    }

    Assert.That(
      () => mcp2221A.GpioController.SetPinMode(pinNumber, mode),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
  }

  [Test]
  public void SetPinMode_UnsupportedOrUndefinedMode(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(PinMode.InputPullUp, PinMode.InputPullDown, -1, int.MaxValue)] PinMode unsupportedOrUndefinedPinMode
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.SetPinMode(pinNumber, unsupportedOrUndefinedPinMode),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void GetPinMode_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.GetPinMode(pinNumber),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_GetPinMode()
  {
    for (var pinNumber = 0; pinNumber < 4; pinNumber++) {
      yield return new object[] { pinNumber, "00-01-" /* LOW - INPUT */, PinMode.Input };
      yield return new object[] { pinNumber, "01-01-" /* HIGH - INPUT */, PinMode.Input };
      yield return new object[] { pinNumber, "00-00-" /* LOW - OUTPUT */, PinMode.Output };
      yield return new object[] { pinNumber, "01-00-" /* HIGH - OUTPUT */, PinMode.Output };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_GetPinMode))]
  public void GetPinMode(
    int pinNumber,
    string pinValueAndDirectionInGetGpioValuesResponse,
    PinMode expectedMode
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      pinNumber == 0 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 1 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 2 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 3 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    PinMode mode = default;

    Assert.That(
      () => mode = mcp2221A.GpioController.GetPinMode(pinNumber),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mode,
      Is.EqualTo(expectedMode)
    );
    Assert.That(
      mcp2221A.GpPins[pinNumber].CurrentMode,
      Is.EqualTo(expectedMode)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Write_NotOpen()
  {
    for (var pinNumber = 0; pinNumber < 4; pinNumber++) {
      yield return new object[] { pinNumber, PinValue.High };
      yield return new object[] { pinNumber, PinValue.Low };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Write_NotOpen))]
  public void Write_NotOpen(
    int pinNumber,
    PinValue value
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.Write(pinNumber, value),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Write()
  {
    for (var pinNumber = 0; pinNumber < 4; pinNumber++) {
      yield return new object[] { pinNumber, PinValue.High };
      yield return new object[] { pinNumber, PinValue.Low };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write(
    int pinNumber,
    PinValue value
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      pinNumber == 0 ? $"FF-{(value == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 1 ? $"FF-{(value == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 2 ? $"FF-{(value == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 3 ? $"FF-{(value == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    // [1] Don't care
    // [2 + 4n]: Alter GP<n> output: 0x00=disable, (value other than 0)=enable
    // [3 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [4 + 4n]: Alter GP<n> pin direction: 0x00=disable, (value other than 0)=enable
    // [5 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    for (var n = 0; n < 4; n++) {
      expectedSentCommand[2 + 4 * n] = (byte)(n == pinNumber ? 0xFF : 0x00);
      expectedSentCommand[3 + 4 * n] = (byte)((n == pinNumber) ? (value == PinValue.Low ? 0x00 : 0xFF) : 0x00);
      expectedSentCommand[4 + 4 * n] = 0x00;
      expectedSentCommand[5 + 4 * n] = 0x00;
    }

    Assert.That(
      () => mcp2221A.GpioController.Write(pinNumber, value),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
    Assert.That(
      mcp2221A.GpPins[pinNumber].LastUpdatedValue,
      Is.EqualTo(value)
    );
  }

  [TestCaseSource(nameof(YieldTestCases_Write))]
  public void Write_NotOutputMode(
    int pinNumber,
    PinValue value
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_1_000; // LOW - INPUT - GPIO operation (GPIO3)

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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );

    Assert.That(
      mcp2221A.GpPins[0].LastUpdatedValue,
      Is.EqualTo(initialGp0Value)
    );
    Assert.That(
      mcp2221A.GpPins[1].LastUpdatedValue,
      Is.EqualTo(initialGp1Value)
    );
    Assert.That(
      mcp2221A.GpPins[2].LastUpdatedValue,
      Is.EqualTo(initialGp2Value)
    );
    Assert.That(
      mcp2221A.GpPins[3].LastUpdatedValue,
      Is.EqualTo(initialGp3Value)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Write_WithReadOnlySpan()
  {
    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
    // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [2 + 4n]: Alter GP<n> pin direction: (value other than 0)=enable
    // [3 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    const byte GpLO = 0x00; // GP<n> pin value: LOW
    const byte GpHI = 0xFF; // GP<n> pin value: HIGH
    const byte Gp00 = 0x00; // Alter GP<n> output: disable
    const byte GpFF = 0xFF; // Alter GP<n> output: enable

    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.Low), new(1, PinValue.High), new(2, PinValue.Low), new(3, PinValue.High) },
      new byte[] { GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00 },
      new byte[] { GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00 },
    };
    yield return new object[] {
      new PinValuePair[] { new(3, PinValue.Low), new(2, PinValue.High), new(1, PinValue.Low), new(0, PinValue.High) },
      new byte[] { GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00 },
      new byte[] { GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00 },
    };

    yield return new object[] {
      new PinValuePair[] { new(0, PinValue.Low) },
      new byte[] { GpFF, GpLO, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
      new byte[] { GpFF, GpLO, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
    };
    yield return new object[] {
      new PinValuePair[] { new(1, PinValue.High) },
      new byte[] { Gp00, Gp00, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
      new byte[] { Gp00, Gp00, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
    };
    yield return new object[] {
      new PinValuePair[] { new(2, PinValue.Low) },
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, GpFF, GpLO, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
    };
    yield return new object[] {
      new PinValuePair[] { new(3, PinValue.High) },
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00 },
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, GpFF, GpHI, 0x00, 0x00 },
    };

    yield return new object[] {
      new PinValuePair[] { }, // empty
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
      new byte[] { Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00, Gp00, Gp00, 0x00, 0x00 },
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Write_WithReadOnlySpan))]
  public void Write_WithReadOnlySpan(
    PinValuePair[] pinValuePairs,
    byte[] pinValuesAndDirectionsInSetGpioValuesResponse,
    byte[] gpioOutputsInExpectedSentCommand
  )
  {
    const byte InitialGp0Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_0_000; // HIGH - OUTPUT - GPIO operation (GPIO1)
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

    foreach (var (pinNumber, _) in pinValuePairs) {
      Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
        Throws.Nothing
      );
    }

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      BitConverter.ToString(pinValuesAndDirectionsInSetGpioValuesResponse) + "-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
#else
    // In this version, `GpioController.Write(ReadOnlySpan<PinValuePair>)` is implemented to
    // call `Write(int, PinValue)` for each individual `PinValuePair`, so the response must
    // also be prepared for each one.
    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      Enumerable.Repeat(setGpioOutputValuesResponse, pinValuePairs.Length).ToArray()
    );
#endif
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    expectedSentCommand[1] = 0x00; // Command completed successfully
    gpioOutputsInExpectedSentCommand.CopyTo(expectedSentCommand.AsSpan(2, 16));

    Assert.That(
      () => mcp2221A.GpioController.Write(pinValuePairs),
      Throws.Nothing
    );
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
#else
    // Write(PinValuePair) is called for each individual PinValuePair.
    // Verification of the sent command is skipped here.
#endif

    foreach (var (gp, value) in pinValuePairs) {
      Assert.That(
        mcp2221A.GpPins[gp].LastUpdatedValue,
        Is.EqualTo(value)
      );
    }
  }

  [Test]
  public void Read_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.Read(pinNumber),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Read()
  {
    for (var pinNumber = 0; pinNumber < 4; pinNumber++) {
      yield return new object[] { pinNumber, "00-01-" /* LOW - INPUT */, PinValue.Low };
      yield return new object[] { pinNumber, "01-01-" /* HIGH - INPUT */, PinValue.High };
      yield return new object[] { pinNumber, "00-00-" /* LOW - OUTPUT */, PinValue.Low };
      yield return new object[] { pinNumber, "01-00-" /* HIGH - OUTPUT */, PinValue.High };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Read))]
  public void Read(
    int pinNumber,
    string pinValueAndDirectionInGetGpioValuesResponse,
    PinValue expectedValue
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      pinNumber == 0 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 1 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 2 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      pinNumber == 3 ? pinValueAndDirectionInGetGpioValuesResponse : "00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    PinValue value = default;

    Assert.That(
      () => value = mcp2221A.GpioController.Read(pinNumber),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      value,
      Is.EqualTo(expectedValue)
    );
    Assert.That(
      mcp2221A.GpPins[pinNumber].LastUpdatedValue,
      Is.EqualTo(expectedValue)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Read_WithSpan()
  {
    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { new(0, default), new(1, default), new(2, default), new(3, default) },
      new PinValue[] { PinValue.Low, PinValue.High, PinValue.Low, PinValue.High }
    };
    yield return new object[] {
      "00-01-01-01-01-01-00-01-", // LOW - INPUT - HIGH - INPUT - HIGH - INPUT - LOW - INPUT
      new PinValuePair[] { new(3, default), new(2, default), new(1, default), new(0, default) },
      new PinValue[] { PinValue.Low, PinValue.High, PinValue.High, PinValue.Low }
    };

    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { new(0, default) },
      new PinValue[] { PinValue.Low }
    };
    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { new(1, default) },
      new PinValue[] { PinValue.High }
    };
    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { new(2, default) },
      new PinValue[] { PinValue.Low }
    };
    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { new(3, default) },
      new PinValue[] { PinValue.High }
    };

    yield return new object[] {
      "00-01-01-01-00-01-01-01-", // LOW - INPUT - HIGH - INPUT - LOW - INPUT - HIGH - INPUT
      new PinValuePair[] { }, // empty
      new PinValue[] { }
    };
  }

  [TestCaseSource(nameof(YieldTestCases_Read_WithSpan))]
  public void Read_WithSpan(
    string pinValuesAndDirectionsInGetGpioValuesResponse,
    PinValuePair[] pinValuePairs,
    PinValue[] expectedPinValues
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    foreach (var (pinNumber, _) in pinValuePairs) {
      Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
        Throws.Nothing
      );
    }

    // [MCP2221A] 3.1.12 GET GPIO VALUES
    var getGpioValuesResponse = string.Concat(
      "51-00-",
      pinValuesAndDirectionsInGetGpioValuesResponse,
      string.Join("-", Enumerable.Repeat("00", 64 - 10))
    );

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, getGpioValuesResponse);
#else
    // In this version, `GpioController.Read(Span<PinValuePair>)` is implemented to
    // call `Read(int)` for each individual `PinValuePair`, so the response must
    // also be prepared for each one.
    Mcp2221AControllerTests.AppendPseudoResponse(
      mcp2221A,
      Enumerable.Repeat(getGpioValuesResponse, pinValuePairs.Length).ToArray()
    );
#endif
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64]; // [1-64]: don't care

    expectedSentCommand[0] = 0x51; // GET GPIO VALUES

    Assert.That(
      () => mcp2221A.GpioController.Read(pinValuePairs),
      Throws.Nothing
    );
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );
#else
    // Read(PinValuePair) is called for each individual PinValuePair.
    // Verification of the sent command is skipped here.
#endif

    for (var i = 0; i < pinValuePairs.Length; i++) {
      Assert.That(
        pinValuePairs[i].PinValue,
        Is.EqualTo(expectedPinValues[i])
      );
      Assert.That(
        mcp2221A.GpPins[pinValuePairs[i].PinNumber].LastUpdatedValue,
        Is.EqualTo(expectedPinValues[i])
      );
    }
  }

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
  [Test]
  public void Toggle_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.Toggle(pinNumber),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_Toggle()
  {
    for (var pinNumber = 0; pinNumber < 4; pinNumber++) {
      yield return new object[] { pinNumber, PinValue.Low, PinValue.High };
      yield return new object[] { pinNumber, PinValue.High, PinValue.Low };
    }
  }

  [TestCaseSource(nameof(YieldTestCases_Toggle))]
  public void Toggle_ThrowsNotImplementedException(
    int pinNumber,
    PinValue initialValue,
    PinValue expectedValue
  )
  {
    const byte InitialGp0Settings = 0b_000_0_0_000; // LOW/HIGH - OUTPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_0_0_000; // LOW/HIGH - OUTPUT - GPIO operation (GPIO1)
    const byte InitialGp2Settings = 0b_000_0_0_000; // LOW/HIGH - OUTPUT - GPIO operation (GPIO2)
    const byte InitialGp3Settings = 0b_000_0_0_000; // LOW/HIGH - OUTPUT - GPIO operation (GPIO3)

    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        gp0Settings: (byte)(InitialGp0Settings | (pinNumber == 0 ? (initialValue == PinValue.Low ? 0b_000_0_0_000 : 0b_000_1_0_000) : 0b_000_0_0_000)),
        gp1Settings: (byte)(InitialGp1Settings | (pinNumber == 1 ? (initialValue == PinValue.Low ? 0b_000_0_0_000 : 0b_000_1_0_000) : 0b_000_0_0_000)),
        gp2Settings: (byte)(InitialGp2Settings | (pinNumber == 2 ? (initialValue == PinValue.Low ? 0b_000_0_0_000 : 0b_000_1_0_000) : 0b_000_0_0_000)),
        gp3Settings: (byte)(InitialGp3Settings | (pinNumber == 3 ? (initialValue == PinValue.Low ? 0b_000_0_0_000 : 0b_000_1_0_000) : 0b_000_0_0_000))
      ),
      shouldDisposeUsbHidDevice: true
    );

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    var valueToggledFromInitial = initialValue == PinValue.Low ? PinValue.High : PinValue.Low;
    var setGpioOutputValuesResponse = string.Concat(
      "50-00-",
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      pinNumber == 0 ? $"FF-{(expectedValue == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 1 ? $"FF-{(expectedValue == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 2 ? $"FF-{(expectedValue == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      pinNumber == 3 ? $"FF-{(expectedValue == PinValue.Low ? "00" : "FF")}-00-00-" : "00-00-00-00-",
      string.Join("-", Enumerable.Repeat("00", 64 - 18))
    );

    Mcp2221AControllerTests.AppendPseudoResponse(mcp2221A, setGpioOutputValuesResponse);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    var expectedSentCommand = new byte[64];

    expectedSentCommand[0] = 0x50; // SET GPIO OUTPUT VALUES
    // [1] Don't care
    // [2 + 4n]: Alter GP<n> output: 0x00=disable, (value other than 0)=enable
    // [3 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
    // [4 + 4n]: Alter GP<n> pin direction: 0x00=disable, (value other than 0)=enable
    // [5 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
    for (var n = 0; n < 4; n++) {
      expectedSentCommand[2 + 4 * n] = (byte)(n == pinNumber ? 0xFF : 0x00);
      expectedSentCommand[3 + 4 * n] = (byte)((n == pinNumber) ? (valueToggledFromInitial == PinValue.Low ? 0x00 : 0xFF) : 0x00);
      expectedSentCommand[4 + 4 * n] = 0x00;
      expectedSentCommand[5 + 4 * n] = 0x00;
    }

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    Assert.That(
      () => mcp2221A.GpioController.Toggle(pinNumber),
      Throws.Nothing
    );
    Assert.That(
      Mcp2221AControllerTests.GetSentCommand(mcp2221A),
      SequenceIs.EqualTo(expectedSentCommand)
    );

    Assert.That(
      mcp2221A.GpPins[pinNumber].LastUpdatedValue,
      Is.EqualTo(expectedValue)
    );
  }
#endif

  [Test]
  public void WaitForEvent_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    Assert.That(
      () => mcp2221A.GpioController.WaitForEvent(pinNumber, pinEventTypes, cts.Token),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void WaitForEvent_ThrowsNotImplementedException(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    Assert.That(
      () => mcp2221A.GpioController.WaitForEvent(pinNumber, pinEventTypes, cts.Token),
      Throws.TypeOf<NotImplementedException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void WaitForEventAsync_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    Assert.That(
      async () => await mcp2221A.GpioController.WaitForEventAsync(pinNumber, pinEventTypes, cts.Token).ConfigureAwait(false),
      Throws.TypeOf<InvalidOperationException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void WaitForEventAsync_ThrowsNotImplementedException(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    // command should not be sent
    // Mcp2221AControllerTests.AppendPseudoResponse(...);
    Mcp2221AControllerTests.ClearSentCommands(mcp2221A);

    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));

    Assert.That(
      async () => await mcp2221A.GpioController.WaitForEventAsync(pinNumber, pinEventTypes, cts.Token).ConfigureAwait(false),
      Throws.TypeOf<NotImplementedException>()
    );

    Assert.That(
      Mcp2221AControllerTests.GetEndPointWriteStream(mcp2221A).Length,
      Is.Zero,
      "command should not be sent"
    );
  }

  [Test]
  public void RegisterCallbackForPinValueChangedEvent_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () => mcp2221A.GpioController.RegisterCallbackForPinValueChangedEvent(
        pinNumber,
        pinEventTypes,
        static (sender, e) => throw new InvalidOperationException("event should not be raised")
      ),
      Throws.TypeOf<InvalidOperationException>()
    );
  }

  [Test]
  public void RegisterCallbackForPinValueChangedEvent_ThrowsNotImplementedException(
    [Values(0, 1, 2, 3)] int pinNumber,
    [Values(
      PinEventTypes.None,
      PinEventTypes.Rising,
      PinEventTypes.Falling,
      PinEventTypes.Rising | PinEventTypes.Falling
    )]
    PinEventTypes pinEventTypes
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    Assert.That(
      () => mcp2221A.GpioController.RegisterCallbackForPinValueChangedEvent(
        pinNumber,
        pinEventTypes,
        static (sender, e) => throw new InvalidOperationException("event should not be raised")
      ),
      Throws.TypeOf<NotImplementedException>()
    );
  }

  [Test]
  public void UnregisterCallbackForPinValueChangedEvent_NotOpen(
    [Values(0, 1, 2, 3)] int pinNumber
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () => mcp2221A.GpioController.UnregisterCallbackForPinValueChangedEvent(
        pinNumber,
        static (sender, e) => throw new InvalidOperationException("event should not be raised")
      ),
      Throws.TypeOf<InvalidOperationException>()
    );
  }

  [Test]
  public void UnregisterCallbackForPinValueChangedEvent_ThrowsNotImplementedException(
    [Values(0, 1, 2, 3)] int pinNumber
  )
  {
    const byte InitialGp0Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO0)
    const byte InitialGp1Settings = 0b_000_1_1_000; // HIGH - INPUT - GPIO operation (GPIO1)
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

    Assert.That(
      () =>
#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
        _ =
#endif
        mcp2221A.GpioController.OpenPin(pinNumber),
      Throws.Nothing
    );

    Assert.That(
      () => mcp2221A.GpioController.UnregisterCallbackForPinValueChangedEvent(
        pinNumber,
        static (sender, e) => throw new InvalidOperationException("event should not be raised")
      ),
      Throws.TypeOf<NotImplementedException>()
    );
  }
}
