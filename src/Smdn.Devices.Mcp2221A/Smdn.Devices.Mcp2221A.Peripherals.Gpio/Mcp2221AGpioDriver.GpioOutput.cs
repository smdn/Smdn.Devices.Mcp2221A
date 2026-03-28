// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
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
  internal static void ThrowIfInvalidGpIndex(int gp)
  {
    if (gp is < 0 or >= NumberOfGpPins)
      throw new InvalidOperationException($"The index of GP pin must be in range of 0 to {NumberOfGpPins - 1} (Specified pin index: {gp}).");
  }

  // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
  private const int LengthOfGpioOutputValues = 4 * NumberOfGpPins;

  // [MCP2221A] 3.1.12 GET GPIO VALUES
  private const int LengthOfGpioValues = 2 * NumberOfGpPins;

  // [MCP2221A] 3.1.12 GET GPIO VALUES
  // [0 + 2n]: GP<n> pin value
  // [1 + 2n]: GP<n> direction value
  private readonly Memory<byte> gpioStateBytes = new byte[LengthOfGpioValues];

  private const byte GpioValueLow = 0x00;
  private const byte GpioValueHigh = 0x01;
  private const byte GpioValueInvalid = 0xEE; // 0xEE if GP<n> is not set for GPIO operation

  private const byte GpioDirectionOutput = 0x00;
  private const byte GpioDirectionInput = 0x01;
  private const byte GpioDirectionInvalid = 0xEF;

  private static class SetGpioOutputValuesCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      Memory<byte> gpioValueBytes
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
      comm[0] = 0x50; // Set GPIO Output Values
      comm[1] = 0x00; // Don't care

      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      gpioValueBytes.Span.CopyTo(comm.Slice(2, LengthOfGpioOutputValues));
    }

    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      Memory<byte> gpioValueBytes
    )
    {
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
      // [2 + 4n]: Alter GP<n> output (enable/disable) status
      // [3 + 4n]: GP<n> output value status
      // [4 + 4n]: Alter GP<n> pin direction (enable/disable)
      // [5 + 4n]: GP<n> pin direction (input or output)
      resp.Slice(2, LengthOfGpioOutputValues).CopyTo(gpioValueBytes.Span);

      return default;
    }
  }

  // <inheritdoc/>
  public async ValueTask ApplyGpioStatesAsync(
    ReadOnlyMemory<PinValuePair> pinValuePairs,
    ReadOnlyMemory<PinModePair> pinModePairs,
    CancellationToken cancellationToken
  )
  {
    var newGpioOutputArray = ArrayPool<byte>.Shared.Rent(LengthOfGpioOutputValues);

    try {
      var newGpioOutputBytes = newGpioOutputArray.AsMemory(0, LengthOfGpioOutputValues);

      ConstructNewGpioOutputBytes(
        destination: newGpioOutputBytes.Span,
        values: pinValuePairs.Span,
        modes: pinModePairs.Span
      );

      _ = await Transceiver.CommandAsync(
        arg: newGpioOutputBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpioOutputValuesCommand.ConstructCommand,
        parseResponse: SetGpioOutputValuesCommand.ParseResponse
      ).ConfigureAwait(false);

      SyncAndVerifyGpioStates(
        gpioOutputResponseBytes: newGpioOutputBytes.Span,
        values: pinValuePairs.Span,
        modes: pinModePairs.Span
      );
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpioOutputArray);
    }
  }

  // <inheritdoc/>
  public void ApplyGpioStates(
    ReadOnlySpan<PinValuePair> pinValuePairs,
    ReadOnlySpan<PinModePair> pinModePairs,
    CancellationToken cancellationToken
  )
  {
    var newGpioOutputArray = ArrayPool<byte>.Shared.Rent(LengthOfGpioOutputValues);

    try {
      var newGpioOutputBytes = newGpioOutputArray.AsMemory(0, LengthOfGpioOutputValues);

      ConstructNewGpioOutputBytes(
        destination: newGpioOutputBytes.Span,
        values: pinValuePairs,
        modes: pinModePairs
      );

      _ = Transceiver.Command(
        arg: newGpioOutputBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpioOutputValuesCommand.ConstructCommand,
        parseResponse: SetGpioOutputValuesCommand.ParseResponse
      );

      SyncAndVerifyGpioStates(
        gpioOutputResponseBytes: newGpioOutputBytes.Span,
        values: pinValuePairs,
        modes: pinModePairs
      );
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpioOutputArray);
    }
  }

  private static void ConstructNewGpioOutputBytes(
    Span<byte> destination,
    ReadOnlySpan<PinValuePair> values,
    ReadOnlySpan<PinModePair> modes
  )
  {
    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
    // [0 + 4n]: Alter GP<n> output: 0x00=disable
    // [1 + 4n]: GP<n> output value: 0x00=L
    // [2 + 4n]: Alter GP<n> pin direction: 0x00=disable
    // [3 + 4n]: GP<n> pin direction: 0x00=output
    destination.Clear();

    foreach (var (gp, value) in values) {
      ThrowIfInvalidGpIndex(gp);

      // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
      destination[0 + (gp * 4)] = 0xFF;

      // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
      destination[1 + (gp * 4)] = (byte)(value.IsLow ? 0x00 : 0xFF);
    }

    foreach (var (gp, mode) in modes) {
      ThrowIfInvalidGpIndex(gp);

      // [2 + 4n]: Alter GP<n> pin direction: (value other than 0)=enable
      destination[2 + (gp * 4)] = 0xFF;

      // [3 + 4n]: GP<n> pin direction: 0x00=output, (any other value)=input
      destination[3 + (gp * 4)] = mode switch {
        PinMode.Output => 0x00,
        PinMode.Input => 0xFF,
        var unsupportedMode => (byte)GpController.ThrowDirectionNotSupportedException(unsupportedMode),
      };
    }
  }

#pragma warning restore IDE0040
  private static class GetGpioValuesCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      Memory<byte> gpioValueBytes
    )
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.12 GET GPIO VALUES
      comm[0] = 0x51; // Get GPIO Values
    }

    public static None ParseResponse(
      ReadOnlySpan<byte> resp,
      Memory<byte> gpioValueBytes
    )
    {
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      // 2 + 2n: GP<n> pin value
      // 3 + 2n: GP<n> direction value
      resp.Slice(2, LengthOfGpioValues).CopyTo(gpioValueBytes.Span);

      return default;
    }
  }

  /// <inheritdoc/>
  public async ValueTask FetchGpioStatesAsync(
    Memory<PinValuePair> pinValuePairs = default,
    Memory<PinModePair> pinModePairs = default,
    CancellationToken cancellationToken = default
  )
  {
    _ = await Transceiver.CommandAsync(
      arg: gpioStateBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpioValuesCommand.ConstructCommand,
      parseResponse: GetGpioValuesCommand.ParseResponse
    ).ConfigureAwait(false);

    if (!pinValuePairs.IsEmpty)
      GetLastUpdatedValuesOrThrow(pinValuePairs.Span);

    if (!pinModePairs.IsEmpty)
      GetLastUpdatedModesOrThrow(pinModePairs.Span);
  }

  /// <inheritdoc/>
  public void FetchGpioStates(
    Span<PinValuePair> pinValuePairs = default,
    Span<PinModePair> pinModePairs = default,
    CancellationToken cancellationToken = default
  )
  {
    _ = Transceiver.Command(
      arg: gpioStateBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpioValuesCommand.ConstructCommand,
      parseResponse: GetGpioValuesCommand.ParseResponse
    );

    if (!pinValuePairs.IsEmpty)
      GetLastUpdatedValuesOrThrow(pinValuePairs);

    if (!pinModePairs.IsEmpty)
      GetLastUpdatedModesOrThrow(pinModePairs);
  }

  private void GetLastUpdatedValuesOrThrow(Span<PinValuePair> pinValuePairs)
  {
    for (var i = 0; i < pinValuePairs.Length; i++) {
      ref var p = ref pinValuePairs[i];

      ThrowIfInvalidGpIndex(p.PinNumber);

      p = new(
        p.PinNumber,
        GetLastUpdatedValueOrThrow(p.PinNumber)
      );
    }
  }

  private void GetLastUpdatedModesOrThrow(Span<PinModePair> pinModePairs)
  {
    for (var i = 0; i < pinModePairs.Length; i++) {
      ref var p = ref pinModePairs[i];

      ThrowIfInvalidGpIndex(p.PinNumber);

      p = new(
        p.PinNumber,
        GetLastUpdatedDirectionOrThrow(p.PinNumber)
      );
    }
  }

  internal PinValue GetLastUpdatedValueOrThrow(int gp)
    // 0 + 2n: GP<n> pin value
    => gpioStateBytes.Span[0 + (gp * 2)] switch {
      GpioValueLow => PinValue.Low,
      GpioValueHigh => PinValue.High,
      GpioValueInvalid => throw new InvalidOperationException($"GP{gp} is not set for GPIO operation"),
      var unknown => throw new NotSupportedException($"unknown GP pin value: {unknown:X2}"),
    };

  internal PinMode GetLastUpdatedDirectionOrThrow(int gp)
    // 1 + 2n: GP<n> direction value
    => gpioStateBytes.Span[1 + (gp * 2)] switch {
      GpioDirectionOutput => PinMode.Output,
      GpioDirectionInput => PinMode.Input,
      GpioDirectionInvalid => throw new InvalidOperationException($"GP{gp} is not set for GPIO operation"),
      var unknown => throw new NotSupportedException($"unknown GP direction value: {unknown:X2}"),
    };

  /// <summary>
  /// Synchronize and verify the GPIO states cache (<see cref="gpioStateBytes"/>)
  /// based on the response of 'SET GPIO OUTPUT VALUES' command.
  /// </summary>
  private void SyncAndVerifyGpioStates(
    ReadOnlySpan<byte> gpioOutputResponseBytes,
    ReadOnlySpan<PinValuePair> values,
    ReadOnlySpan<PinModePair> modes
  )
  {
    const byte GpIsNotSetForGpioOperation = 0xEE;

    // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES (response)
    // [0 + 4n]: Alter GP<n> output (enable/disable) status
    // [1 + 4n]: GP<n> output value status
    // [2 + 4n]: Alter GP<n> pin direction (enable/disable)
    // [3 + 4n]: GP<n> pin direction (input or output)
    int? firstGpIndexOfNotSetForGpioOperation = null;

    for (var gp = 0; gp < NumberOfGpPins; gp++) {
      if (
        gpioOutputResponseBytes[0 + (4 * gp)] == GpIsNotSetForGpioOperation ||
        gpioOutputResponseBytes[1 + (4 * gp)] == GpIsNotSetForGpioOperation
      ) {
        firstGpIndexOfNotSetForGpioOperation ??= gp;
      }

      // 0 + 2n: GP<n> pin value
      gpioStateBytes.Span[0 + (2 * gp)] = gpioOutputResponseBytes[1 + (4 * gp)] switch {
        GpIsNotSetForGpioOperation => GpioValueInvalid,
        0x00 => GpioValueLow,
        _ => GpioValueHigh,
      };

      if (
        gpioOutputResponseBytes[2 + (4 * gp)] == GpIsNotSetForGpioOperation ||
        gpioOutputResponseBytes[3 + (4 * gp)] == GpIsNotSetForGpioOperation
      ) {
        firstGpIndexOfNotSetForGpioOperation ??= gp;
      }

      // 1 + 2n: GP<n> direction value
      gpioStateBytes.Span[1 + (2 * gp)] = gpioOutputResponseBytes[3 + (4 * gp)] switch {
        GpIsNotSetForGpioOperation => GpioDirectionInvalid,
        0x00 => GpioDirectionOutput,
        _ => GpioDirectionInput,
      };
    }

    if (firstGpIndexOfNotSetForGpioOperation.HasValue) {
      // whether or not a command has been issued to alter the GPIO values of GP<n>
      foreach (var (gp, _) in values) {
        if (gp == firstGpIndexOfNotSetForGpioOperation.Value)
          throw new InvalidOperationException($"GP{gp} is not set for GPIO operation");
      }

      // whether or not a command has been issued to alter the GPIO direction of GP<n>
      foreach (var (gp, _) in modes) {
        if (gp == firstGpIndexOfNotSetForGpioOperation.Value)
          throw new InvalidOperationException($"GP{gp} is not set for GPIO operation");
      }
    }
  }

  /// <summary>
  /// Synchronize the GPIO states cache (<see cref="gpioStateBytes"/>) based on
  /// the updated SRAM settings (<see cref="gpSettingsBytes"/>).
  /// </summary>
  /// <remarks>
  /// When using the 'GET SRAM SETTINGS' or 'SET SRAM SETTINGS' commands, setting
  /// or getting SRAM settings updates not only the designation of GP0-GP3 but
  /// also the GPIO direction and value.
  /// Therefore, this method updates the <see cref="gpioStateBytes"/> based on
  /// the SRAM settings.
  /// </remarks>
  private void SyncGpioStates(ReadOnlySpan<byte> gpSettings)
  {
    const byte GpSettingsGpioOutputValueMask = 0b_000_1_0_000;
    const byte GpSettingsGpioDirectionMask = 0b_000_0_1_000;

    for (int gp = 0, i = 0; gp < NumberOfGpPins; gp++) {
      var isGpio = (GpDesignation)(gpSettings[gp] & (byte)GpDesignation.BitMask) == GpDesignation.GpioOperation;

      // 0 + 2n: GP<n> pin value
      gpioStateBytes.Span[i++] = isGpio
        ? ((gpSettings[gp] & GpSettingsGpioOutputValueMask) == 0) ? GpioValueLow : GpioValueHigh
        : GpioValueInvalid;

      // 1 + 2n: GP<n> direction value
      gpioStateBytes.Span[i++] = isGpio
        ? ((gpSettings[gp] & GpSettingsGpioDirectionMask) == 0) ? GpioDirectionOutput : GpioDirectionInput
        : GpioDirectionInvalid;
    }
  }
}
