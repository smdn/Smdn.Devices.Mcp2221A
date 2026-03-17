// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Device.Gpio;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
public abstract partial class GpController {
#pragma warning restore IDE0040
  internal const int NumberOfGpPins = 4;

  private readonly Mcp2221ATransceiver transceiver;

  private protected abstract int GpPinNumber { get; }
  internal GpDesignation CurrentGpDesignation { get; set; }

  /// <summary>
  /// Gets the GP pin name represented by the current instance.
  /// </summary>
  public abstract string PinName { get; }

  /// <summary>
  /// Gets the current function assigned to the GP pin.
  /// </summary>
  /// <value>
  /// A <see cref="GpFunction"/> value indicating the current function of the pin.
  /// </value>
  /// <remarks>
  /// This value changes when a configuration method, such as
  /// <see cref="ConfigureAsGpio(PinMode, PinValue, CancellationToken)"/>,
  /// is successfully called.
  /// </remarks>
  /// <seealso cref="CurrentDesignation"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 1.7.1 CONFIGURABLE PIN FUNCTIONS
  /// [MCP2221A] TABLE 1-5: GP DESIGNATION TABLE
  /// </seealso>
  public abstract GpFunction CurrentFunction { get; }

  /// <summary>
  /// Gets the specific hardware designation label currently assigned to the GP pin.
  /// </summary>
  /// <value>
  /// A <see cref="string"/> representing the hardware-specific function name
  /// (e.g., <c>GPIO</c>, <c>ADC1</c>, <c>SSPND</c>, <c>LED_I2C</c>).
  /// </value>
  /// <remarks>
  /// This property returns the label corresponding to the current <see cref="CurrentFunction"/>,
  /// taking into account the specific GP pin index. For example, if <see cref="CurrentFunction"/>
  /// is <see cref="GpFunction.LedOutput"/>, this property may return <c>LED_URX</c>,
  /// <c>LED_UTX</c>, or <c>LED_I2C</c> depending on the pin.
  /// </remarks>
  /// <seealso cref="CurrentFunction"/>
  public abstract string CurrentDesignation { get; }

  private protected GpController(Mcp2221ATransceiver transceiver)
  {
    this.transceiver = transceiver;
  }

  private static class GetGpSettingsCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, Memory<byte> gpSettings)
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.14 GET SRAM SETTINGS
      comm[0] = 0x61; // Get SRAM Settings
    }

    public static bool ParseResponse(ReadOnlySpan<byte> resp, Memory<byte> gpSettings)
    {
      resp.Slice(22, 4).CopyTo(gpSettings.Span); // GP0-3 Settings

      return true;
    }
  }

  private static class SetGpSettingsCommand {
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      (
        ReadOnlyMemory<byte> gpSettings,
        int gpIndex,
        GpDesignation gpDesignation,
        PinMode gpioDirection,
        PinValue gpioValue
      ) args
    )
    {
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      comm[0] = 0x60; // Set SRAM settings
#if false
      comm[1] = 0x00; // Don't care
      comm[2] = 0b00000000; // Clock Output Driver Value = remain unaltered (0b0_______)
      comm[3] = 0b00000000; // DAC Voltage Reference = remain unaltered (0b0_______)
      comm[4] = 0b00000000; // Set DAC Output Value = remain unaltered (0b0_______)
      comm[5] = 0b00000000; // ADC Voltage Reference = remain unaltered (0b0_______)
      comm[6] = 0b00000000; // Setup the interrupt detection mechanism and clear the detection flag = remain unaltered (0b0_______)
#endif
      comm[7] = 0b10000000; // Alter GPIO configuration = Alter the GP designation (1)

      const int FirstIndexOfGPSettings = 8; // GP0 Settings

      // copy current GP0-GP3 settings
      args.gpSettings.Span.CopyTo(comm.Slice(FirstIndexOfGPSettings, NumberOfGpPins));

      // construct new GP<n> settings
      var bitsGpioOutputValue = (bool)args.gpioValue
        ? 0b_000_1_0_000
        : 0b_000_0_0_000;
      var bitsGpioDirection = args.gpioDirection switch {
        PinMode.Input => 0b_000_0_1_000,
        PinMode.Output => 0b_000_0_0_000,

        _ => throw new NotSupportedException(
          message: $"The GPIO direction cannot be set to either {nameof(PinMode.InputPullUp)} or {nameof(PinMode.InputPullDown)}"
        ),
      };
      var bitsGPnDesignation = (byte)args.gpDesignation & 0b_000_0_0_111;

      comm[FirstIndexOfGPSettings + args.gpIndex] = (byte)(
        // Byte Index 8-11 GP0-3 Settings
        0b_000_0_0_000 | // Bit 7-5: Don't care
        bitsGpioOutputValue | // Bit 4: GPIO Output value
        bitsGpioDirection | // Bit 3: GPIO Direction
        bitsGPnDesignation // Bit 2-0: GP<n> Designation
      );
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
    [SuppressMessage("StyleCop.CSharp.MaintainAbilityRules", "SA1414:TupleTypesInSignaturesShouldHaveElementNames", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static bool ParseResponse(
      ReadOnlySpan<byte> resp,
      (
        ReadOnlyMemory<byte>,
        int,
        GpDesignation,
        PinMode,
        PinValue
      ) _
    )
#pragma warning restore IDE0060, SA1313
    {
      return resp[1] switch {
        0x00 => true, // Command completed successfully
        _ => throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})"),
      };
    }
  }

  private protected async ValueTask ConfigureGpDesignationAsync(
    GpDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
  {
    var gpSettings = ArrayPool<byte>.Shared.Rent(NumberOfGpPins);

    try {
      // retrieve current GP0-GP3 settings
      _ = await transceiver.CommandAsync(
        arg: gpSettings.AsMemory(0, 4),
        cancellationToken: cancellationToken,
        constructCommand: GetGpSettingsCommand.ConstructCommand,
        parseResponse: GetGpSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      // overwrite GPn settings and set GP0-GP3 settings
      _ = await transceiver.CommandAsync(
        arg: ((ReadOnlyMemory<byte>)gpSettings.AsMemory(0, 4), GpPinNumber, gpDesignation, gpioInitialDirection, gpioInitialValue),
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      CurrentGpDesignation = gpDesignation;
    }
    finally {
      ArrayPool<byte>.Shared.Return(gpSettings);
    }
  }

  private protected void ConfigureGpDesignation(
    GpDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
  {
    var gpSettings = ArrayPool<byte>.Shared.Rent(4);

    try {
      // retrieve current GP0-GP3 settings
      _ = transceiver.Command(
        arg: gpSettings.AsMemory(0, 4),
        cancellationToken: cancellationToken,
        constructCommand: GetGpSettingsCommand.ConstructCommand,
        parseResponse: GetGpSettingsCommand.ParseResponse
      );

      // overwrite GPn settings and set GP0-GP3 settings
      _ = transceiver.Command(
        arg: ((ReadOnlyMemory<byte>)gpSettings.AsMemory(0, 4), GpPinNumber, gpDesignation, gpioInitialDirection, gpioInitialValue),
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      );

      CurrentGpDesignation = gpDesignation;
    }
    finally {
      ArrayPool<byte>.Shared.Return(gpSettings);
    }
  }
}
