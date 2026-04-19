// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class Mcp2221AGpioDriver {
#pragma warning restore IDE0040
  internal GpDesignation GetCurrentGpDesignation(int gp)
    => (GpDesignation)sramSettings.ReadGpSettingsByte(gp) & GpDesignation.BitMask;

  private static class GetSramSettingsCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(
      Span<byte> comm,
      SramSettings sramSettings
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.14 GET SRAM SETTINGS
      comm[0] = 0x61; // Get SRAM Settings
    }

    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      SramSettings sramSettings
    )
    {
      _ = resp[1] switch {
        0x00 => true, // Command completed successfully
        _ => throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})"),
      };

      // TODO: update other SRAM settings

      // [6] Bit 7-6: DAC Reference voltage option
      // [6] Bit 5: DAC reference option
      // Note: The DAC option bits in the SRAM settings are stored as-is.
      // In other words, even if the least significant bit specifies VDD as
      // the reference voltage, the VRM voltage bits are maintained as-is.
      sramSettings.StoreDacVoltageReferenceByte(
        (byte)((resp[6] & 0b_11_1_00000) >> 5)
      );

      // [6] Bit 4-0: Power-up DAC value
      sramSettings.StoreDacOutputValueByte(
        (byte)(resp[6] & 0b_00_0_11111)
      );

      // [7] Bit 4-3: ADC Reference Voltage
      // [7] Bit 2: ADC Reference Option
      // Note: The DAC option bits in the SRAM settings are stored as-is.
      // In other words, even if the least significant bit specifies VDD as
      // the reference voltage, the VRM voltage bits are maintained as-is.
      sramSettings.StoreAdcVoltageReferenceByte(
        (byte)((resp[7] & 0b_0_0_0_11_1_0_0) >> 2)
      );

      sramSettings.StoreGpSettingsBytes(
        gpSettingBytes: resp.Slice(22, SramSettings.SizeOfGpSettings) // [22-25] GP0-3 Settings
      );

      return default;
    }
  }

  private static class SetSramSettingsCommand {
    public static void ConstructCommand(
      Span<byte> comm,
      SramSettings sramSettings
    )
    {
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      comm[0] = 0x60; // Set SRAM settings
      comm[1] = 0x00; // Don't care

      sramSettings.WriteAsSetSramSettingsCommand(
        comm.Slice(2, SramSettings.SizeOfSelf) // [2-11] SRAM settings
      );
    }

#pragma warning disable IDE0060, SA1313
    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      SramSettings sramSettings
    )
#pragma warning restore IDE0060, SA1313
    {
      _ = resp[1] switch {
        0x00 => true, // Command completed successfully
        _ => throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})"),
      };

      return default;
    }
  }

  internal async ValueTask FetchSramSettingsAsync(CancellationToken cancellationToken)
  {
    _ = await Transceiver.CommandAsync<SramSettings, None>(
      arg: sramSettings,
      cancellationToken: cancellationToken,
      constructCommand: GetSramSettingsCommand.ConstructCommand,
      parseResponse: GetSramSettingsCommand.ParseResponse
    ).ConfigureAwait(false);

    SyncGpioStates(sramSettings);
  }

  internal void FetchSramSettings(CancellationToken cancellationToken)
  {
    _ = Transceiver.Command<SramSettings, None>(
      arg: sramSettings,
      cancellationToken: cancellationToken,
      constructCommand: GetSramSettingsCommand.ConstructCommand,
      parseResponse: GetSramSettingsCommand.ParseResponse
    );

    SyncGpioStates(sramSettings);
  }

  private void ConfigureGpDesignation(
    int gp,
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue,
    bool shouldThrowIfUsedByGpioController,
    CancellationToken cancellationToken
  )
  {
    if (shouldThrowIfUsedByGpioController)
      ThrowIfUsedByGpioController(gp);

    try {
      SetSramSettings(
        sramSettings: sramSettings.ModifyGpSettings(
          gp: gp,
          designation: gpDesignation,
          direction: gpioDirection,
          outputValue: gpioValue
        ),
        cancellationToken: cancellationToken
      );
    }
    catch {
      sramSettings.Restore();
      throw;
    }
  }

  internal async ValueTask ConfigureGpDesignationAsync(
    int gp,
    Func<SramSettings, SramSettings> configureSramSettings,
    CancellationToken cancellationToken
  )
  {
    ThrowIfUsedByGpioController(gp);

    try {
      await SetSramSettingsAsync(
        sramSettings: configureSramSettings(sramSettings),
        cancellationToken: cancellationToken
      ).ConfigureAwait(false);
    }
    catch {
      sramSettings.Restore();
      throw;
    }
  }

  internal void ConfigureGpDesignation(
    int gp,
    Func<SramSettings, SramSettings> configureSramSettings,
    CancellationToken cancellationToken
  )
  {
    ThrowIfUsedByGpioController(gp);

    try {
      SetSramSettings(
        sramSettings: configureSramSettings(sramSettings),
        cancellationToken: cancellationToken
      );
    }
    catch {
      sramSettings.Restore();
      throw;
    }
  }

  /// <inheritdoc/>
  public async ValueTask ConfigureAllGpSettingsAsync(
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
  {
    try {
      await SetSramSettingsAsync(
        sramSettings: ModifyAllGpSettings(
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
      ).ConfigureAwait(false);
    }
    catch {
      sramSettings.Restore();
      throw;
    }
  }

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
  {
    try {
      SetSramSettings(
        sramSettings: ModifyAllGpSettings(
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
    }
    catch {
      sramSettings.Restore();
      throw;
    }
  }

  private SramSettings ModifyAllGpSettings(
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
    ReadOnlySpan<(GpController, GpFunction?, PinMode?, PinValue?)> allGpAndSettings = [
      (Gp0, gp0Function, gp0Mode, gp0InitialValue),
      (Gp1, gp1Function, gp1Mode, gp1InitialValue),
      (Gp2, gp2Function, gp2Mode, gp2InitialValue),
      (Gp3, gp3Function, gp3Mode, gp3InitialValue),
    ];

    for (var gp = 0; gp < allGpAndSettings.Length; gp++) {
      var (gpController, gpFunction, gpMode, gpInitialValue) = allGpAndSettings[gp];

      if (gpFunction.HasValue || gpMode.HasValue || gpInitialValue.HasValue)
        ThrowIfUsedByGpioController(gp);

      if (!gpFunction.HasValue)
        continue;

      sramSettings.ModifyGpSettings(
        gp: gp,
        designation: gpController.GetDesignationForFunctionOrThrow(gpFunction.Value),
        direction: gpMode,
        outputValue: gpInitialValue
      );
    }

    return sramSettings;
  }

  private async ValueTask SetSramSettingsAsync(
    SramSettings sramSettings,
    CancellationToken cancellationToken
  )
  {
    if (!sramSettings.IsDirty)
      return; // nothing to configure, do nothing and just return

    cancellationToken.ThrowIfCancellationRequested();

    // attempt to set new SRAM settings
    _ = await Transceiver.CommandAsync<SramSettings, None>(
      arg: sramSettings,
      cancellationToken: cancellationToken,
      constructCommand: SetSramSettingsCommand.ConstructCommand,
      parseResponse: SetSramSettingsCommand.ParseResponse
    ).ConfigureAwait(false);

    if (sramSettings.ShouldReenableVrm()) {
      _ = await Transceiver.CommandAsync<SramSettings, None>(
        arg: sramSettings,
        cancellationToken: cancellationToken,
        constructCommand: SetSramSettingsCommand.ConstructCommand,
        parseResponse: SetSramSettingsCommand.ParseResponse
      ).ConfigureAwait(false);
    }

    // save the successfully configured settings as the current state
    sramSettings.Store();

    SyncGpioStates(sramSettings);
  }

  private void SetSramSettings(
    SramSettings sramSettings,
    CancellationToken cancellationToken
  )
  {
    if (!sramSettings.IsDirty)
      return; // nothing to configure, do nothing and just return

    cancellationToken.ThrowIfCancellationRequested();

    // attempt to set new GP0-GP3 settings
    _ = Transceiver.Command<SramSettings, None>(
      arg: sramSettings,
      cancellationToken: cancellationToken,
      constructCommand: SetSramSettingsCommand.ConstructCommand,
      parseResponse: SetSramSettingsCommand.ParseResponse
    );

    if (sramSettings.ShouldReenableVrm()) {
      _ = Transceiver.Command<SramSettings, None>(
        arg: sramSettings,
        cancellationToken: cancellationToken,
        constructCommand: SetSramSettingsCommand.ConstructCommand,
        parseResponse: SetSramSettingsCommand.ParseResponse
      );
    }

    // save the successfully configured settings as the current state
    sramSettings.Store();

    SyncGpioStates(sramSettings);
  }
}
