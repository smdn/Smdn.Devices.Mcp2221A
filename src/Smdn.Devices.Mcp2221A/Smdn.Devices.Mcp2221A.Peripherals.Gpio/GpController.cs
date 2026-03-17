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
  private const int NumberOfGPs = 4;

  private readonly Mcp2221A device;
  private protected abstract int GPIndex { get; }
  public string PinName => $"GP{GPIndex}";
  public string? PinDesignation { get; private set; }

  private protected GpController(Mcp2221A device)
  {
    this.device = device;
  }

  private static class GetGPSettingsCommand {
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

  private static class SetGPSettingsCommand {
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      (
        ReadOnlyMemory<byte> gpSettings,
        int gpIndex,
        GPDesignation gpDesignation,
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
      args.gpSettings.Span.CopyTo(comm.Slice(FirstIndexOfGPSettings, NumberOfGPs));

      // construct new GP<n> settings
      var bitsGpioOutputValue = (bool)args.gpioValue
        ? 0b_000_1_0_000
        : 0b_000_0_0_000;
      var bitsGpioDirection = args.gpioDirection switch {
        PinMode.Input => 0b_000_0_1_000,
        PinMode.Output => 0b_000_0_0_000,

        _ => throw new ArgumentOutOfRangeException(
          paramName: nameof(args),
          actualValue: args.gpioDirection,
          message: $"must be {nameof(PinMode.Input)} or {nameof(PinMode.Output)}"
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
        GPDesignation,
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

  private protected async ValueTask ConfigureGPDesignationAsync(
    string pinDesignation,
    GPDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
  {
    var gpSettings = ArrayPool<byte>.Shared.Rent(NumberOfGPs);

    try {
      // retrieve current GP0-GP3 settings
      _ = await device.CommandAsync(
        arg: gpSettings.AsMemory(0, 4),
        cancellationToken: cancellationToken,
        constructCommand: GetGPSettingsCommand.ConstructCommand,
        parseResponse: GetGPSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      // overwrite GPn settings and set GP0-GP3 settings
      _ = await device.CommandAsync(
        arg: ((ReadOnlyMemory<byte>)gpSettings.AsMemory(0, 4), GPIndex, gpDesignation, gpioInitialDirection, gpioInitialValue),
        cancellationToken: cancellationToken,
        constructCommand: SetGPSettingsCommand.ConstructCommand,
        parseResponse: SetGPSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      PinDesignation = pinDesignation;
    }
    finally {
      ArrayPool<byte>.Shared.Return(gpSettings);
    }
  }

  private protected void ConfigureGPDesignation(
    string pinDesignation,
    GPDesignation gpDesignation,
    PinMode gpioInitialDirection = default,
    PinValue gpioInitialValue = default,
    CancellationToken cancellationToken = default
  )
  {
    var gpSettings = ArrayPool<byte>.Shared.Rent(4);

    try {
      // retrieve current GP0-GP3 settings
      device.Command(
        arg: gpSettings.AsMemory(0, 4),
        cancellationToken: cancellationToken,
        constructCommand: GetGPSettingsCommand.ConstructCommand,
        parseResponse: GetGPSettingsCommand.ParseResponse
      );

      // overwrite GPn settings and set GP0-GP3 settings
      device.Command(
        arg: ((ReadOnlyMemory<byte>)gpSettings.AsMemory(0, 4), GPIndex, gpDesignation, gpioInitialDirection, gpioInitialValue),
        cancellationToken: cancellationToken,
        constructCommand: SetGPSettingsCommand.ConstructCommand,
        parseResponse: SetGPSettingsCommand.ParseResponse
      );

      PinDesignation = pinDesignation;
    }
    finally {
      ArrayPool<byte>.Shared.Return(gpSettings);
    }
  }
}
