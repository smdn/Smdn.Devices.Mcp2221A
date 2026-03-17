// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal sealed class Mcp2221AGpioDriver : /* System.Device.Gpio.GpioDriver, */ IGpControllerGroup {
  private readonly Mcp2221ATransceiver transceiver;

  public Gp0Controller Gp0 { get; }
  public Gp1Controller Gp1 { get; }
  public Gp2Controller Gp2 { get; }
  public Gp3Controller Gp3 { get; }

  public GpController this[int index]
    => index switch {
      0 => Gp0,
      1 => Gp1,
      2 => Gp2,
      3 => Gp3,
      _ => throw new ArgumentOutOfRangeException(paramName: nameof(index), actualValue: index, message: null),
    };

  public int Count => GpController.NumberOfGpPins;

  internal Mcp2221AGpioDriver(
    Mcp2221ATransceiver transceiver
  )
  {
    this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));

    Gp0 = new(transceiver);
    Gp1 = new(transceiver);
    Gp2 = new(transceiver);
    Gp3 = new(transceiver);
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public IEnumerator<GpController> GetEnumerator()
  {
    yield return Gp0;
    yield return Gp1;
    yield return Gp2;
    yield return Gp3;
  }

  private static uint ToGpSettingsVector(ReadOnlySpan<byte> gpSettings)
    // This simply groups 4 sets of 8 bytes into a 32-byte block,
    // so the byte order doesn't really matter.
    => BinaryPrimitives.ReadUInt32BigEndian(gpSettings);

  private static void ToGpSettings(uint gpSettingsVector, Span<byte> gpSettings)
    // This simply groups 4 sets of 8 bytes into a 32-byte block,
    // so the byte order doesn't really matter.
    => BinaryPrimitives.WriteUInt32BigEndian(gpSettings, gpSettingsVector);

  private static class GetGpSettingsCommand {
#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, None _)
#pragma warning restore IDE0060, SA1313
    {
      // [MCP2221A] 3.1.14 GET SRAM SETTINGS
      comm[0] = 0x61; // Get SRAM Settings
    }

#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static uint ParseResponse(ReadOnlySpan<byte> resp, None _)
#pragma warning restore IDE0060, SA1313
      => ToGpSettingsVector(resp.Slice(22, 4)); // GP0-3 Settings
  }

  internal async ValueTask UpdateCurrentGpDesignationAsync(CancellationToken cancellationToken)
    => UpdateCurrentGpDesignationCore(
      gpSettingsVector: await transceiver.CommandAsync(
        cancellationToken: cancellationToken,
        constructCommand: GetGpSettingsCommand.ConstructCommand,
        parseResponse: GetGpSettingsCommand.ParseResponse
      ).ConfigureAwait(false)
    );

  internal void UpdateCurrentGpDesignation(CancellationToken cancellationToken)
    => UpdateCurrentGpDesignationCore(
      gpSettingsVector: transceiver.Command(
        cancellationToken: cancellationToken,
        constructCommand: GetGpSettingsCommand.ConstructCommand,
        parseResponse: GetGpSettingsCommand.ParseResponse
      )
    );

  private void UpdateCurrentGpDesignationCore(uint gpSettingsVector)
  {
    Span<byte> gpSettings = stackalloc byte[GpController.NumberOfGpPins];

    ToGpSettings(gpSettingsVector, gpSettings);

    Gp0.CurrentGpDesignation = (GpDesignation)gpSettings[0] & GpDesignation.BitMask;
    Gp1.CurrentGpDesignation = (GpDesignation)gpSettings[1] & GpDesignation.BitMask;
    Gp2.CurrentGpDesignation = (GpDesignation)gpSettings[2] & GpDesignation.BitMask;
    Gp3.CurrentGpDesignation = (GpDesignation)gpSettings[3] & GpDesignation.BitMask;
  }
}
