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
  // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
  private const int LengthOfGpioOutputValues = 4 * NumberOfGpPins;

  // [MCP2221A] 3.1.12 GET GPIO VALUES
  private const int LengthOfGpioValues = 2 * NumberOfGpPins;

  // [MCP2221A] 3.1.12 GET GPIO VALUES
  // [0 + 2n]: GP<n> pin value
  // [1 + 2n]: GP<n> direction value
  private readonly Memory<byte> gpioValueBytes = new byte[LengthOfGpioValues];

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
      const byte ErrorValue = 0xEE; // 0xEE if GP<n> is not set for GPIO operation
      var indexOfErrorResponse = resp.Slice(2, LengthOfGpioOutputValues).IndexOf(ErrorValue);

      if (0 <= indexOfErrorResponse)
        throw new Mcp2221ACommandException($"GP{indexOfErrorResponse / 4} is not set for GPIO operation");

      resp.Slice(2, 4 * NumberOfGpPins).CopyTo(gpioValueBytes.Span);

      return default;
    }
  }

  internal async ValueTask SetGpioOutputValuesAsync(
    ReadOnlyMemory<PinValuePair> values,
    ReadOnlyMemory<PinModePair> modes,
    CancellationToken cancellationToken
  )
  {
    var newGpioOutputArray = ArrayPool<byte>.Shared.Rent(LengthOfGpioOutputValues);

    try {
      var newGpioOutputBytes = newGpioOutputArray.AsMemory(0, LengthOfGpioOutputValues);

      ConstructNewGpioOutputBytes(
        destination: newGpioOutputBytes.Span,
        values: values.Span,
        modes: modes.Span
      );

      _ = await Transceiver.CommandAsync(
        arg: newGpioOutputBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpioOutputValuesCommand.ConstructCommand,
        parseResponse: SetGpioOutputValuesCommand.ParseResponse
      ).ConfigureAwait(false);
    }
    finally {
      ArrayPool<byte>.Shared.Return(newGpioOutputArray);
    }
  }

  internal void SetGpioOutputValues(
    ReadOnlySpan<PinValuePair> values,
    ReadOnlySpan<PinModePair> modes,
    CancellationToken cancellationToken
  )
  {
    var newGpioOutputArray = ArrayPool<byte>.Shared.Rent(LengthOfGpioOutputValues);

    try {
      var newGpioOutputBytes = newGpioOutputArray.AsMemory(0, LengthOfGpioOutputValues);

      ConstructNewGpioOutputBytes(
        destination: newGpioOutputBytes.Span,
        values: values,
        modes: modes
      );

      _ = Transceiver.Command(
        arg: newGpioOutputBytes,
        cancellationToken: cancellationToken,
        constructCommand: SetGpioOutputValuesCommand.ConstructCommand,
        parseResponse: SetGpioOutputValuesCommand.ParseResponse
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
      // [0 + 4n]: Alter GP<n> output: (value other than 0)=enable
      destination[0 + (gp * 4)] = 0xFF;

      // [1 + 4n]: GP<n> output value: 0x00=L, (any other value)=H
      destination[1 + (gp * 4)] = (byte)value;
    }

    foreach (var (gp, mode) in modes) {
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

  internal async ValueTask UpdateCurrentGpioValuesAsync(CancellationToken cancellationToken)
    => _ = await Transceiver.CommandAsync(
      arg: gpioValueBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpioValuesCommand.ConstructCommand,
      parseResponse: GetGpioValuesCommand.ParseResponse
    ).ConfigureAwait(false);

  internal void UpdateCurrentGpioValues(CancellationToken cancellationToken)
    => _ = Transceiver.Command(
      arg: gpioValueBytes,
      cancellationToken: cancellationToken,
      constructCommand: GetGpioValuesCommand.ConstructCommand,
      parseResponse: GetGpioValuesCommand.ParseResponse
    );

  internal PinValue GetCurrentPinValue(int gp)
    // 0 + 2n: GP<n> pin value
    => gpioValueBytes.Span[0 + (gp * 2)] switch {
      0x00 => PinValue.Low,
      0x01 => PinValue.High,
      0xEE => throw new InvalidOperationException($"GP{gp} is not set for GPIO operation"),
      var unknown => throw new NotSupportedException($"unknown GP pin value: {unknown:X2}"),
    };

  internal PinMode GetCurrentDirection(int gp)
    // 1 + 2n: GP<n> direction value
    => gpioValueBytes.Span[1 + (gp * 2)] switch {
      0x00 => PinMode.Output,
      0x01 => PinMode.Input,
      0xEF => throw new InvalidOperationException($"GP{gp} is not set for GPIO operation"),
      var unknown => throw new NotSupportedException($"unknown GP direction value: {unknown:X2}"),
    };
}
