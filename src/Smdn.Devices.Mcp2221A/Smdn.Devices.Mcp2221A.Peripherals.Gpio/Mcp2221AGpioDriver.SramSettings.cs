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
  private readonly record struct GpSettings(
    GpDesignation? Designation,
    PinMode? Direction,
    PinValue? OutputValue
  ) {
    public bool HasValue => Designation.HasValue || Direction.HasValue || OutputValue.HasValue;
  }

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
      if (resp[1] != 0x00) // Command completed successfully
        Mcp2221ACommandException.ThrowNoSuccessfulResponse("GET SRAM SETTINGS", resp[1]);

      // TODO: update other SRAM settings

      // [5] Bit 7-5: Don't care
      // [5] Bit 4-0: Clock Output divider value
      //   Bits[4:3]: duty cycle
      //   Bits[2:0]: clock divider
      // Note: The clock output bits in the SRAM settings are stored as-is.
      // In other words, even if a bit value marked as "Reserved" is specified
      // in the clock divider value, it will remain unchanged.
      sramSettings.StoreClockOutputDividerValueByte(
        (byte)(resp[5] & 0b_000_11_111)
      );

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

      // [7] Bit 6: If set, the interrupt detection flag will be set when a negative edge occurs
      // [7] Bit 5: If set, the interrupt detection flag will be set when a positive edge occurs
      sramSettings.StoreInterruptDetectionModuleSetupByte(
        (byte)(
          (((resp[7] & 0b_0_1_0_00_0_0_0) == 0) ? 0b_0_00_0_0_0_0_0 : 0b_0_00_0_0_0_1_0 /* trigger on negative edge */) |
          (((resp[7] & 0b_0_0_1_00_0_0_0) == 0) ? 0b_0_00_0_0_0_0_0 : 0b_0_00_0_1_0_0_0 /* trigger on positive edge */)
        )
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
      if (resp[1] != 0x00) // Command completed successfully
        Mcp2221ACommandException.ThrowNoSuccessfulResponse("SET SRAM SETTINGS", resp[1]);

      return default;
    }
  }

  internal async ValueTask FetchSramSettingsAsync(CancellationToken cancellationToken)
  {
    using (await Transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
      _ = await Transceiver.CommandAsync(
        arg: sramSettings,
        cancellationToken: cancellationToken,
        constructCommand: GetSramSettingsCommand.ConstructCommand,
        parseResponse: GetSramSettingsCommand.ParseResponse
      ).ConfigureAwait(false);

      SyncGpioStates(sramSettings);
    }
  }

  internal void FetchSramSettings(CancellationToken cancellationToken)
  {
    using (Transceiver.EnterCommandTransaction(cancellationToken)) {
      _ = Transceiver.Command(
        arg: sramSettings,
        cancellationToken: cancellationToken,
        constructCommand: GetSramSettingsCommand.ConstructCommand,
        parseResponse: GetSramSettingsCommand.ParseResponse
      );

      SyncGpioStates(sramSettings);
    }
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

    SetSramSettings(
      argSramSettings: (
        GpIndex: gp,
        GpSettings: new GpSettings(gpDesignation, gpioDirection, gpioValue)
      ),
      modifySramSettings: static (sramSettings, arg) => sramSettings.ModifyGpSettings(
        gp: arg.GpIndex,
        designation: arg.GpSettings.Designation!.Value, // [NotNullIfNotNull]
        direction: arg.GpSettings.Direction,
        outputValue: arg.GpSettings.OutputValue
      ),
      cancellationToken: cancellationToken
    );
  }

  internal ValueTask ConfigureGpPinSettingsAsync<TArg>(
    int gpIndex,
    TArg arg,
    Action<SramSettings, int, TArg> modifyGpPinSettings,
    CancellationToken cancellationToken
  )
  {
    ThrowIfUsedByGpioController(gpIndex);

    return SetSramSettingsAsync(
      argSramSettings: (
        ModifyGpPinSettings: modifyGpPinSettings,
        GpIndex: gpIndex,
        Argument: arg
      ),
      modifySramSettings: static (sramSettings, arg)
        => arg.ModifyGpPinSettings(sramSettings, arg.GpIndex, arg.Argument),
      cancellationToken: cancellationToken
    );
  }

  internal void ConfigureGpPinSettings<TArg>(
    int gpIndex,
    TArg arg,
    Action<SramSettings, int, TArg> modifyGpPinSettings,
    CancellationToken cancellationToken
  )
  {
    ThrowIfUsedByGpioController(gpIndex);

    SetSramSettings(
      argSramSettings: (
        ModifyGpPinSettings: modifyGpPinSettings,
        GpIndex: gpIndex,
        Argument: arg
      ),
      modifySramSettings: static (sramSettings, arg)
        => arg.ModifyGpPinSettings(sramSettings, arg.GpIndex, arg.Argument),
      cancellationToken: cancellationToken
    );
  }

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
  {
    GpSettings gp0Settings = new(
      gp0Function.HasValue ? Gp0.GetDesignationForFunctionOrThrow(gp0Function.Value) : null,
      gp0Mode,
      gp0InitialValue
    );
    GpSettings gp1Settings = new(
      gp1Function.HasValue ? Gp1.GetDesignationForFunctionOrThrow(gp1Function.Value) : null,
      gp1Mode,
      gp1InitialValue
    );
    GpSettings gp2Settings = new(
      gp2Function.HasValue ? Gp2.GetDesignationForFunctionOrThrow(gp2Function.Value) : null,
      gp2Mode,
      gp2InitialValue
    );
    GpSettings gp3Settings = new(
      gp3Function.HasValue ? Gp3.GetDesignationForFunctionOrThrow(gp3Function.Value) : null,
      gp3Mode,
      gp3InitialValue
    );

    if (gp0Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 0);
    if (gp1Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 1);
    if (gp2Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 2);
    if (gp3Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 3);

    return SetSramSettingsAsync(
      argSramSettings: (gp0Settings, gp1Settings, gp2Settings, gp3Settings),
      modifySramSettings: ModifyAllGpSettings,
      cancellationToken: cancellationToken
    );
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
    GpSettings gp0Settings = new(
      gp0Function.HasValue ? Gp0.GetDesignationForFunctionOrThrow(gp0Function.Value) : null,
      gp0Mode,
      gp0InitialValue
    );
    GpSettings gp1Settings = new(
      gp1Function.HasValue ? Gp1.GetDesignationForFunctionOrThrow(gp1Function.Value) : null,
      gp1Mode,
      gp1InitialValue
    );
    GpSettings gp2Settings = new(
      gp2Function.HasValue ? Gp2.GetDesignationForFunctionOrThrow(gp2Function.Value) : null,
      gp2Mode,
      gp2InitialValue
    );
    GpSettings gp3Settings = new(
      gp3Function.HasValue ? Gp3.GetDesignationForFunctionOrThrow(gp3Function.Value) : null,
      gp3Mode,
      gp3InitialValue
    );

    if (gp0Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 0);
    if (gp1Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 1);
    if (gp2Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 2);
    if (gp3Settings.HasValue)
      ThrowIfUsedByGpioController(gp: 3);

    SetSramSettings(
      argSramSettings: (gp0Settings, gp1Settings, gp2Settings, gp3Settings),
      modifySramSettings: ModifyAllGpSettings,
      cancellationToken: cancellationToken
    );
  }

  private static void ModifyAllGpSettings(
    SramSettings sramSettings,
    (
      GpSettings Gp0Settings,
      GpSettings Gp1Settings,
      GpSettings Gp2Settings,
      GpSettings Gp3Settings
    ) arg
  )
  {
    ReadOnlySpan<GpSettings> allGpSettings = [
      arg.Gp0Settings,
      arg.Gp1Settings,
      arg.Gp2Settings,
      arg.Gp3Settings,
    ];

    for (var gpIndex = 0; gpIndex < allGpSettings.Length; gpIndex++) {
      var (designation, direction, outputValue) = allGpSettings[gpIndex];

      if (!designation.HasValue)
        continue;

      sramSettings.ModifyGpSettings(
        gp: gpIndex,
        designation: designation.Value,
        direction: direction,
        outputValue: outputValue
      );
    }
  }

  private async ValueTask SetSramSettingsAsync<TArg>(
    TArg argSramSettings,
    Action<SramSettings, TArg> modifySramSettings,
    CancellationToken cancellationToken
  )
  {
    using (await Transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
      modifySramSettings(sramSettings, argSramSettings);

      if (!sramSettings.IsDirty)
        return; // nothing to configure, do nothing and just return

      try {
        // attempt to set new SRAM settings
        _ = await Transceiver.CommandAsync(
          arg: sramSettings,
          cancellationToken: cancellationToken,
          constructCommand: SetSramSettingsCommand.ConstructCommand,
          parseResponse: SetSramSettingsCommand.ParseResponse
        ).ConfigureAwait(false);

        if (sramSettings.ShouldReenableVrm()) {
          _ = await Transceiver.CommandAsync(
            arg: sramSettings,
            cancellationToken: cancellationToken,
            constructCommand: SetSramSettingsCommand.ConstructCommand,
            parseResponse: SetSramSettingsCommand.ParseResponse
          ).ConfigureAwait(false);
        }
      }
      catch {
        sramSettings.Restore();
        throw;
      }

      if (sramSettings.ShouldResetInterruptDetectionFlag())
        LastFetchedInterruptDetectionFlag = default;

      // save the successfully configured settings as the current state
      sramSettings.Store();

      SyncGpioStates(sramSettings);
    } // end of using
  }

  private void SetSramSettings<TArg>(
    TArg argSramSettings,
    Action<SramSettings, TArg> modifySramSettings,
    CancellationToken cancellationToken
  )
  {
    using (Transceiver.EnterCommandTransaction(cancellationToken)) {
      modifySramSettings(sramSettings, argSramSettings);

      if (!sramSettings.IsDirty)
        return; // nothing to configure, do nothing and just return

      try {
        // attempt to set new SRAM settings
        _ = Transceiver.Command(
          arg: sramSettings,
          cancellationToken: cancellationToken,
          constructCommand: SetSramSettingsCommand.ConstructCommand,
          parseResponse: SetSramSettingsCommand.ParseResponse
        );

        if (sramSettings.ShouldReenableVrm()) {
          _ = Transceiver.Command(
            arg: sramSettings,
            cancellationToken: cancellationToken,
            constructCommand: SetSramSettingsCommand.ConstructCommand,
            parseResponse: SetSramSettingsCommand.ParseResponse
          );
        }
      }
      catch {
        sramSettings.Restore();
        throw;
      }

      if (sramSettings.ShouldResetInterruptDetectionFlag())
        LastFetchedInterruptDetectionFlag = default;

      // save the successfully configured settings as the current state
      sramSettings.Store();

      SyncGpioStates(sramSettings);
    } // end of using
  }
}
