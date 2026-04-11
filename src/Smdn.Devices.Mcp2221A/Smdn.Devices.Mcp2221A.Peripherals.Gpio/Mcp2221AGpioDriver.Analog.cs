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
  private AdcAllChannelSample lastFetchedAdcSample;

  private static class GetAdcChannelValuesCommand {
#pragma warning disable IDE0060, SA1313
    public static void ConstructCommand(
      Span<byte> comm,
      ReadOnlySpan<byte> userData,
      None _
    )
#pragma warning restore IDE0060, SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      comm[0] = 0x10; // [0] Status/Set Parameters
    }

#pragma warning disable IDE0060, SA1313
    public static AdcAllChannelSample ParseResponse(
      ReadOnlySpan<byte> resp,
      None _
    )
#pragma warning restore IDE0060, SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      // [50-55] ADC Data (16-bit) values; 3x(16-bit) little-endian ADC channel values
      return new(
        adc1: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(50, 2)),
        adc2: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(52, 2)),
        adc3: BinaryPrimitives.ReadUInt16LittleEndian(resp.Slice(54, 2))
      );
    }
  }

  /// <inheritdoc/>
  public VoltageReferenceSource CurrentAdcReferenceSource
    => (VoltageReferenceSource)(sramSettings.ReadAdcSettingsByte() & 0b_0_0000_11_1);

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

  /// <inheritdoc/>
  public AdcAllChannelSample FetchAdcRawValues(
    CancellationToken cancellationToken
  )
  {
    return lastFetchedAdcSample = Transceiver.Command(
      cancellationToken: cancellationToken,
      constructCommand: GetAdcChannelValuesCommand.ConstructCommand,
      parseResponse: GetAdcChannelValuesCommand.ParseResponse
    );
  }

  /// <inheritdoc/>
  public async ValueTask<AdcAllChannelSample> FetchAdcRawValuesAsync(
    CancellationToken cancellationToken
  )
  {
    return lastFetchedAdcSample = await Transceiver.CommandAsync(
      cancellationToken: cancellationToken,
      constructCommand: GetAdcChannelValuesCommand.ConstructCommand,
      parseResponse: GetAdcChannelValuesCommand.ParseResponse
    ).ConfigureAwait(false);
  }
}
