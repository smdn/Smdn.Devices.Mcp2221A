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
  private readonly record struct GpSettings(
    GpDesignation? Designation = default,
    PinMode? Direction = default,
    PinValue? OutputValue = default
  ) {
    public bool IsNull => Designation == null && Direction == null && OutputValue == null;
  }

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

  internal async ValueTask FetchGpSettingsAsync(CancellationToken cancellationToken)
  {
    _ = await Transceiver.CommandAsync(
      arg: gpSettingsBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpSettingsCommand.ConstructCommand,
      parseResponse: GetGpSettingsCommand.ParseResponse
    ).ConfigureAwait(false);

    SyncGpioValues(gpSettingsBytes.Span);
  }

  internal void FetchGpSettings(CancellationToken cancellationToken)
  {
    _ = Transceiver.Command(
      arg: gpSettingsBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpSettingsCommand.ConstructCommand,
      parseResponse: GetGpSettingsCommand.ParseResponse
    );

    SyncGpioValues(gpSettingsBytes.Span);
  }

  internal ValueTask ConfigureGpDesignationAsync(
    int gp,
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue,
    CancellationToken cancellationToken
  )
    => SetGpSettingsAsync(
      allGpSettings: (
        Gp0Settings: gp == 0 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp1Settings: gp == 1 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp2Settings: gp == 2 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp3Settings: gp == 3 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default
      ),
      cancellationToken: cancellationToken
    );

  internal void ConfigureGpDesignation(
    int gp,
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue,
    CancellationToken cancellationToken
  )
    => SetGpSettings(
      allGpSettings: (
        Gp0Settings: gp == 0 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp1Settings: gp == 1 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp2Settings: gp == 2 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default,
        Gp3Settings: gp == 3 ? ConstructGpSettings(gpDesignation, gpioDirection, gpioValue) : default
      ),
      cancellationToken: cancellationToken
    );

  private static GpSettings ConstructGpSettings(
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue
  )
    => new(
      Designation: gpDesignation,
      // applies only when GP<n> is set to GPIO
      Direction: gpDesignation == GpDesignation.GpioOperation ? gpioDirection : null,
      // applies only when GP<n> is set to GPIO
      OutputValue: gpDesignation == GpDesignation.GpioOperation ? gpioValue : null
    );

  /// <inheritdoc/>
  public ValueTask ConfigureAllGpSettingsAsync(
    GpFunction? gp0Function = default,
    PinMode? gp0Mode = default,
    PinValue? gp0InitialValue = default,
    GpFunction? gp1Function = default,
    PinMode? gp1Mode = default,
    PinValue? gp1InitialValue = default,
    GpFunction? gp2Function = default,
    PinMode? gp2Mode = default,
    PinValue? gp2InitialValue = default,
    GpFunction? gp3Function = default,
    PinMode? gp3Mode = default,
    PinValue? gp3InitialValue = default,
    CancellationToken cancellationToken = default
  )
    => SetGpSettingsAsync(
        allGpSettings: ConstructGpSettings(
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
        cancellationToken: cancellationToken
      );

  /// <inheritdoc/>
  public void ConfigureAllGpSettings(
    GpFunction? gp0Function = default,
    PinMode? gp0Mode = default,
    PinValue? gp0InitialValue = default,
    GpFunction? gp1Function = default,
    PinMode? gp1Mode = default,
    PinValue? gp1InitialValue = default,
    GpFunction? gp2Function = default,
    PinMode? gp2Mode = default,
    PinValue? gp2InitialValue = default,
    GpFunction? gp3Function = default,
    PinMode? gp3Mode = default,
    PinValue? gp3InitialValue = default,
    CancellationToken cancellationToken = default
  )
    => SetGpSettings(
        allGpSettings: ConstructGpSettings(
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
        cancellationToken: cancellationToken
      );

  private
  (
    GpSettings Gp0Settings,
    GpSettings Gp1Settings,
    GpSettings Gp2Settings,
    GpSettings Gp3Settings
  )
  ConstructGpSettings(
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
    PinValue? gp3InitialValue
  )
  {
    static GpSettings ConstructGpSettings(
      GpController gp,
      GpFunction? gpFunction,
      PinMode? gpMode,
      PinValue? gpInitialValue
    )
      => gpFunction is { } gpFunc
        ? new(
            Designation: gp.GetDesignationForFunctionOrThrow(gpFunc),
            // applies only when GP<n> is set to GPIO
            Direction: gpFunc == GpFunction.Gpio ? gpMode : null,
            // applies only when GP<n> is set to GPIO
            OutputValue: gpFunc == GpFunction.Gpio ? gpInitialValue : null
          )
        : default; // maintains current designation/direction/value

    return (
      Gp0Settings: ConstructGpSettings(Gp0, gp0Function, gp0Mode, gp0InitialValue),
      Gp1Settings: ConstructGpSettings(Gp1, gp1Function, gp1Mode, gp1InitialValue),
      Gp2Settings: ConstructGpSettings(Gp2, gp2Function, gp2Mode, gp2InitialValue),
      Gp3Settings: ConstructGpSettings(Gp3, gp3Function, gp3Mode, gp3InitialValue)
    );
  }

  private async ValueTask SetGpSettingsAsync(
    (
      GpSettings Gp0Settings,
      GpSettings Gp1Settings,
      GpSettings Gp2Settings,
      GpSettings Gp3Settings
    ) allGpSettings,
    CancellationToken cancellationToken
  )
  {
    var (gp0Settings, gp1Settings, gp2Settings, gp3Settings) = allGpSettings;

    if (gp0Settings.IsNull && gp1Settings.IsNull && gp2Settings.IsNull && gp3Settings.IsNull)
      return; // nothing to configure, do nothing and just return

    cancellationToken.ThrowIfCancellationRequested();

    var newGpSettingsArray = ArrayPool<byte>.Shared.Rent(NumberOfGpPins);

    try {
      var newGpSettingsBytes = newGpSettingsArray.AsMemory(0, NumberOfGpPins);

      ConstructNewGpSettingsBytes(
        destination: newGpSettingsBytes.Span,
        gp0Settings: gp0Settings,
        gp1Settings: gp1Settings,
        gp2Settings: gp2Settings,
        gp3Settings: gp3Settings
      );

      // attempt to set new GP0-GP3 settings
      _ = await Transceiver.CommandAsync<ReadOnlyMemory<byte>, bool>(
        arg: newGpSettingsBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      // save the successfully configured settings as the current state
      newGpSettingsBytes.CopyTo(gpSettingsBytes);

      SyncGpioValues(gpSettingsBytes.Span);
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpSettingsArray);
    }
  }

  private void SetGpSettings(
    (
      GpSettings Gp0Settings,
      GpSettings Gp1Settings,
      GpSettings Gp2Settings,
      GpSettings Gp3Settings
    ) allGpSettings,
    CancellationToken cancellationToken
  )
  {
    var (gp0Settings, gp1Settings, gp2Settings, gp3Settings) = allGpSettings;

    if (gp0Settings.IsNull && gp1Settings.IsNull && gp2Settings.IsNull && gp3Settings.IsNull)
      return; // nothing to configure, do nothing and just return

    cancellationToken.ThrowIfCancellationRequested();

    var newGpSettingsArray = ArrayPool<byte>.Shared.Rent(NumberOfGpPins);

    try {
      var newGpSettingsBytes = newGpSettingsArray.AsMemory(0, NumberOfGpPins);

      ConstructNewGpSettingsBytes(
        destination: newGpSettingsBytes.Span,
        gp0Settings: gp0Settings,
        gp1Settings: gp1Settings,
        gp2Settings: gp2Settings,
        gp3Settings: gp3Settings
      );

      // attempt to set new GP0-GP3 settings
      _ = Transceiver.Command<ReadOnlyMemory<byte>, bool>(
        arg: newGpSettingsBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpSettingsCommand.ConstructCommand,
        parseResponse: SetGpSettingsCommand.ParseResponse
      );

      // save the successfully configured settings as the current state
      newGpSettingsBytes.CopyTo(gpSettingsBytes);

      SyncGpioValues(gpSettingsBytes.Span);
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpSettingsArray);
    }
  }

  private void ConstructNewGpSettingsBytes(
    Span<byte> destination,
    GpSettings gp0Settings,
    GpSettings gp1Settings,
    GpSettings gp2Settings,
    GpSettings gp3Settings
  )
  {
    // copy current GP0-GP3 settings
    gpSettingsBytes.Span.CopyTo(destination);

    ReadOnlySpan<GpSettings> allGpSettings = [
      gp0Settings,
      gp1Settings,
      gp2Settings,
      gp3Settings
    ];

    // construct new GP0-GP3 settings
    for (var i = 0; i < NumberOfGpPins; i++) {
      // construct new GP<n> settings
      byte gpSettingsBits = 0b_000_0_0_000;

      // Bit 2-0: GP<n> Designation
      gpSettingsBits |= allGpSettings[i].Designation switch {
        null => (byte)(destination[i] & (byte)GpDesignation.BitMask), // maintain the current settings
        GpDesignation designation => (byte)(designation & GpDesignation.BitMask),
      };

      // Bit 3: GPIO Direction
      gpSettingsBits |= allGpSettings[i].Direction switch {
        null => (byte)(destination[i] & 0b_000_0_1_000), // maintain the current settings
        PinMode.Input => 0b_000_0_1_000,
        PinMode.Output => 0b_000_0_0_000,
        PinMode unsupportedMode => (byte)GpController.ThrowDirectionNotSupportedException(unsupportedMode),
      };

      // Bit 4: GPIO Output value
      gpSettingsBits |= allGpSettings[i].OutputValue switch {
        null => (byte)(destination[i] & 0b_000_1_0_000), // maintain the current settings
        PinValue val => (byte)(val.IsHigh ? 0b_000_1_0_000 : 0b_000_0_0_000),
      };

      // Bit 7-5: Don't care
      // gpSettings |= 0b_000_0_0_000;

      // overwrite GP<n> settings
      destination[i] = gpSettingsBits;
    }
  }
}
