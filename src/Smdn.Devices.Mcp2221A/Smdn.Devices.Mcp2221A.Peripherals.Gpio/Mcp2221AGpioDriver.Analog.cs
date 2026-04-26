// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class Mcp2221AGpioDriver {
#pragma warning restore IDE0040
  internal static int? ThrowIfDacOutputValueOutOfRange(int? value, string paramName)
    => value.HasValue ? ThrowIfDacOutputValueOutOfRange(value.Value, paramName) : null;

  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="value"/> is negative, or greater than 31 (the maximum
  /// value for a 5-bit DAC).
  /// </exception>
  internal static int ThrowIfDacOutputValueOutOfRange(int value, string paramName)
  {
    const int DacOutputValueMax = 0b11111;

    if (value is < 0 or > DacOutputValueMax) {
      throw new ArgumentOutOfRangeException(
        message: $"The DAC output value must be in range of 0 to {DacOutputValueMax} (5-bit).",
        actualValue: value,
        paramName: paramName
      );
    }

    return value;
  }

  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="value"/> is greater than 1023 (the maximum value for a 10-bit ADC).
  /// </exception>
  internal static ushort ThrowIfAdcRawValueOutOfRange(ushort value, string paramName)
  {
    const ushort AdcRawMaxValue = 0b_0000_0011_1111_1111;

    if (AdcRawMaxValue < value) {
      throw new ArgumentOutOfRangeException(
        message: $"The ADC raw value must be in range of 0 to {AdcRawMaxValue} (10-bit).",
        actualValue: value,
        paramName: paramName
      );
    }

    return value;
  }

  private int? lastAppliedDacRawValue;
  private AdcAllChannelSample lastFetchedAdcSample;

  /// <inheritdoc/>
  public VoltageReferenceSource CurrentDacReferenceSource
    => ParseVoltageReferenceSource(sramSettings.ReadDacVoltageReferenceByte() & 0b_0_0000_11_1);

  /// <inheritdoc/>
  public VoltageReferenceSource CurrentAdcReferenceSource
    => ParseVoltageReferenceSource(sramSettings.ReadAdcVoltageReferenceByte() & 0b_0_0000_11_1);

  // If the least significant bit is 0, return it as VDD regardless of the VRM voltage bits.
  // Otherwise, return it as representing the VRM voltage.
  // This ensures that the VRM reference voltage selection is maintained even when VDD is selected.
  private static VoltageReferenceSource ParseVoltageReferenceSource(int voltageReferenceBits)
    => (voltageReferenceBits & 0b_001) == 0
      ? VoltageReferenceSource.Vdd
      : (VoltageReferenceSource)voltageReferenceBits;

  private static class FetchGpPinInputsCommand {
#pragma warning disable SA1313
    public static void ConstructCommand(
      Span<byte> comm,
      None _
    )
#pragma warning restore SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      comm[0] = 0x10; // [0] Status/Set Parameters
    }

#pragma warning disable SA1313
    public static
    (bool InterruptDetectionFlag, AdcAllChannelSample AdcSample)
    ParseResponse(
      ReadOnlySpan<byte> resp,
      None _
    )
#pragma warning restore SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      return (
        // [24] Interrupt edge detector state
        InterruptDetectionFlag: resp[24] != 0,
        // [50-55] ADC Data (16-bit) values; 3x(16-bit) little-endian ADC channel values
        AdcSample: new(
          adc1: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(50, 2)),
          adc2: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(52, 2)),
          adc3: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(54, 2))
        )
      );
    }
  }

  public int GetLastAppliedDacRawValue()
  {
    // If a value has been set most recently, return that value;
    // otherwise, return the SRAM power-up DAC value.
    if (lastAppliedDacRawValue.HasValue)
      return lastAppliedDacRawValue.Value;
    else
      return sramSettings.ReadDacOutputValueByte() & 0b_0_00_11111;
  }

  /// <inheritdoc/>
  public void ApplyDacRawValue(
    int value,
    CancellationToken cancellationToken = default
  )
  {
    SetSramSettings(
      argSramSettings: ThrowIfDacOutputValueOutOfRange(value, nameof(value)),
      modifySramSettings: static (sramSettings, val) => sramSettings.ModifyDacSettings(
        voltageReferenceSource: null,
        outputValue: val
      ),
      cancellationToken: cancellationToken
    );

    lastAppliedDacRawValue = value;
  }

  /// <inheritdoc/>
  public async ValueTask ApplyDacRawValueAsync(
    int value,
    CancellationToken cancellationToken = default
  )
  {
    await SetSramSettingsAsync(
      argSramSettings: ThrowIfDacOutputValueOutOfRange(value, nameof(value)),
      modifySramSettings: static (sramSettings, val) => sramSettings.ModifyDacSettings(
        voltageReferenceSource: null,
        outputValue: val
      ),
      cancellationToken: cancellationToken
    ).ConfigureAwait(false);

    lastAppliedDacRawValue = value;
  }

  /// <summary>
  /// Returns the cached 10-bit raw ADC input value (0-1023) for the specified GP pin
  /// retrieved during the last call to <see cref="FetchAdcRawValues"/> or
  /// <see cref="FetchAdcRawValuesAsync"/>.
  /// </summary>
  /// <param name="gp">
  /// The GP pin number (typically 1, 2, or 3) whose cached ADC value is to be retrieved.
  /// </param>
  /// <returns>The 10-bit raw ADC value (0-1023).</returns>
  /// <remarks>
  /// <param>
  /// This method does not communicate with the device; it returns the value from the
  /// local cache.
  /// </param>
  /// <param>
  /// If <see cref="FetchAdcRawValues"/> or <see cref="FetchAdcRawValuesAsync"/> has not
  /// been called yet, this method always returns 0.
  /// </param>
  /// </remarks>
  public int GetLastFetchedAdcRawValue(int gp)
    => gp switch {
      1 => lastFetchedAdcSample.Adc1,
      2 => lastFetchedAdcSample.Adc2,
      3 => lastFetchedAdcSample.Adc3,
      _ => throw new ArgumentOutOfRangeException(
        paramName: nameof(gp),
        actualValue: gp,
        message: $"The GP pin index {gp} is out of range for the Analog-to-Digital Converter (ADC) channels."
      ),
    };

  private void FetchGpPinInputs(
    CancellationToken cancellationToken
  )
  {
    using (Transceiver.EnterCommandTransaction(cancellationToken)) {
      (
        LastFetchedInterruptDetectionFlag,
        lastFetchedAdcSample
      )
        = Transceiver.Command(
          cancellationToken: cancellationToken,
          constructCommand: FetchGpPinInputsCommand.ConstructCommand,
          parseResponse: FetchGpPinInputsCommand.ParseResponse
        );
    }
  }

  private async ValueTask FetchGpPinInputsAsync(
    CancellationToken cancellationToken
  )
  {
    using (await Transceiver.EnterCommandTransactionAsync(cancellationToken).ConfigureAwait(false)) {
      (
        LastFetchedInterruptDetectionFlag,
        lastFetchedAdcSample
      )
        = await Transceiver.CommandAsync(
          cancellationToken: cancellationToken,
          constructCommand: FetchGpPinInputsCommand.ConstructCommand,
          parseResponse: FetchGpPinInputsCommand.ParseResponse
        ).ConfigureAwait(false);
    }
  }

  /// <inheritdoc/>
  public AdcAllChannelSample FetchAdcRawValues(
    CancellationToken cancellationToken
  )
  {
    FetchGpPinInputs(cancellationToken);

    return lastFetchedAdcSample;
  }

  /// <inheritdoc/>
  public async ValueTask<AdcAllChannelSample> FetchAdcRawValuesAsync(
    CancellationToken cancellationToken
  )
  {
    await FetchGpPinInputsAsync(cancellationToken).ConfigureAwait(false);

    return lastFetchedAdcSample;
  }
}
