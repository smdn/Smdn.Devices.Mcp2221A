// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040

partial class Mcp2221AGpioDriver {
#pragma warning restore IDE0040
  private readonly Memory<byte> gpSettingsBytes = new byte[NumberOfGpPins];

  internal GpDesignation GetCurrentGpDesignation(int gp)
    => (GpDesignation)gpSettingsBytes.Span[gp] & GpDesignation.BitMask;

  private static class GetGpSettingsCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      Memory<byte> gpSettingsBytes
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.14 GET SRAM SETTINGS
      comm[0] = 0x61; // Get SRAM Settings
    }

    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      Memory<byte> gpSettingsBytes
    )
    {
      resp.Slice(22, 4).CopyTo(gpSettingsBytes.Span); // GP0-3 Settings

      return default;
    }
  }

  private static class SetGpSettingsCommand {
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      ReadOnlyMemory<byte> gpSettingsBytes
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

      // GP0-GP3 settings
      gpSettingsBytes.Span.CopyTo(comm.Slice(FirstIndexOfGPSettings, Mcp2221AGpioDriver.NumberOfGpPins));
    }

#pragma warning disable IDE0060, SA1313
    public static bool ParseResponse(
      ReadOnlySpan<byte> resp,
      ReadOnlyMemory<byte> _
    )
#pragma warning restore IDE0060, SA1313
    {
      return resp[1] switch {
        0x00 => true, // Command completed successfully
        _ => throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})"),
      };
    }
  }

  internal async ValueTask UpdateCurrentGpDesignationAsync(CancellationToken cancellationToken)
    => _ = await Transceiver.CommandAsync(
      arg: gpSettingsBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpSettingsCommand.ConstructCommand,
      parseResponse: GetGpSettingsCommand.ParseResponse
    ).ConfigureAwait(false);

  internal void UpdateCurrentGpDesignation(CancellationToken cancellationToken)
    => _ = Transceiver.Command(
      arg: gpSettingsBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpSettingsCommand.ConstructCommand,
      parseResponse: GetGpSettingsCommand.ParseResponse
    );

  internal async ValueTask ConfigureGpDesignationAsync(
    int gp,
    GpDesignation gpDesignation,
    PinMode gpioDirection,
    PinValue gpioValue,
    CancellationToken cancellationToken
  )
  {
    var newGpSettingsArray = ArrayPool<byte>.Shared.Rent(NumberOfGpPins);
    var newGpSettingsBytes = newGpSettingsArray.AsMemory(0, NumberOfGpPins);

    ConstructNewGpSettingsBytes(
      destination: newGpSettingsBytes.Span,
      gp: gp,
      gpDesignation: gpDesignation,
      gpioDirection: gpioDirection,
      gpioValue: gpioValue
    );

    try {
      // attempt to set new GP0-GP3 settings
      _ = await Transceiver.CommandAsync<ReadOnlyMemory<byte>, bool>(
        arg: newGpSettingsBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      // save the successfully configured settings as the current state
      newGpSettingsBytes.CopyTo(gpSettingsBytes);
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpSettingsArray);
    }
  }

  internal void ConfigureGpDesignation(
    int gp,
    GpDesignation gpDesignation,
    PinMode gpioDirection,
    PinValue gpioValue,
    CancellationToken cancellationToken
  )
  {
    var newGpSettingsArray = ArrayPool<byte>.Shared.Rent(NumberOfGpPins);
    var newGpSettingsBytes = newGpSettingsArray.AsMemory(0, NumberOfGpPins);

    ConstructNewGpSettingsBytes(
      destination: newGpSettingsBytes.Span,
      gp: gp,
      gpDesignation: gpDesignation,
      gpioDirection: gpioDirection,
      gpioValue: gpioValue
    );

    try {
      // attempt to set new GP0-GP3 settings
      _ = Transceiver.Command<ReadOnlyMemory<byte>, bool>(
        arg: newGpSettingsBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      );

      // save the successfully configured settings as the current state
      newGpSettingsBytes.CopyTo(gpSettingsBytes);
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpSettingsArray);
    }
  }

  private void ConstructNewGpSettingsBytes(
    Span<byte> destination,
    int gp,
    GpDesignation gpDesignation,
    PinMode gpioDirection,
    PinValue gpioValue
  )
  {
    // copy current GP0-GP3 settings
    gpSettingsBytes.Span.CopyTo(destination);

    // construct new GP<n> settings
    var bitsGpioOutputValue = (bool)gpioValue
      ? 0b_000_1_0_000
      : 0b_000_0_0_000;
    var bitsGpioDirection = gpioDirection switch {
      PinMode.Input => 0b_000_0_1_000,
      PinMode.Output => 0b_000_0_0_000,

      _ => throw new NotSupportedException(
        message: $"The GPIO direction cannot be set to either {nameof(PinMode.InputPullUp)} or {nameof(PinMode.InputPullDown)}"
      ),
    };
    var bitsGpDesignation = (byte)(gpDesignation & GpDesignation.BitMask);

    // overwrite GP<n> settings and set GP0-GP3 settings
    destination[gp] = (byte)(
      // 0b_000_0_0_000 | // Bit 7-5: Don't care
      bitsGpioOutputValue | // Bit 4: GPIO Output value
      bitsGpioDirection | // Bit 3: GPIO Direction
      bitsGpDesignation // Bit 2-0: GP<n> Designation
    );
  }
}
