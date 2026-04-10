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
      ReadOnlySpan<byte> userData,
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

      sramSettings.StoreGpSettingsBytes(
        gpSettingBytes: resp.Slice(22, SramSettings.SizeOfGpSettings) // [22-25] GP0-3 Settings
      );

      // store the settings modified and updated from the response
      sramSettings.Store();

      return default;
    }
  }

  private static class SetSramSettingsCommand {
#pragma warning disable IDE0060
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      SramSettings sramSettings
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.13 SET SRAM SETTINGS
      comm[0] = 0x60; // Set SRAM settings
      comm[1] = 0x00; // Don't care

      sramSettings.WriteSetSramSettingsBytes(
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

  internal async ValueTask ConfigureGpDesignationAsync(
    int gp,
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue,
    CancellationToken cancellationToken
  )
  {
    ThrowIfUsedByGpioController(gp);

    try {
      await SetGpSettingsAsync(
        sramSettings: sramSettings.ModifyGpSettings(
          gp: gp,
          designation: gpDesignation,
          direction: gpioDirection,
          outputValue: gpioValue
        ),
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
    GpDesignation gpDesignation,
    PinMode? gpioDirection,
    PinValue? gpioValue,
    CancellationToken cancellationToken
  )
    => ConfigureGpDesignation(
      gp: gp,
      gpDesignation: gpDesignation,
      gpioDirection: gpioDirection,
      gpioValue: gpioValue,
      shouldThrowIfUsedByGpioController: true,
      cancellationToken: cancellationToken
    );

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
      SetGpSettings(
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
      await SetGpSettingsAsync(
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
      SetGpSettings(
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

  private async ValueTask SetGpSettingsAsync(
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

    // save the successfully configured settings as the current state
    sramSettings.Store();

    SyncGpioStates(sramSettings);
  }

  private void SetGpSettings(
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

    // save the successfully configured settings as the current state
    sramSettings.Store();

    SyncGpioStates(sramSettings);
  }
}
