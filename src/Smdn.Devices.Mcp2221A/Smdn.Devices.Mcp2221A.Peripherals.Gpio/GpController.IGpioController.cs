// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class GpController : IGpioController {
#pragma warning restore IDE0040
  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask ConfigureAsGpioAsync(
    PinMode mode = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignationAsync(
      pinDesignation: $"GPIO{GpPinNumber}",
      gpDesignation: GpDesignation.GpioOperation,
      gpioInitialDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void ConfigureAsGpio(
    PinMode mode = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  )
    => ConfigureGpDesignation(
      pinDesignation: $"GPIO{GpPinNumber}",
      gpDesignation: GpDesignation.GpioOperation,
      gpioInitialDirection: mode,
      gpioInitialValue: initialValue,
      cancellationToken: cancellationToken
    );

  private static class GetDirectionCommand {
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, GpController gp)
      => throw new NotImplementedException();

#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static PinMode ParseResponse(ReadOnlySpan<byte> resp, GpController gp)
#pragma warning restore IDE0060
      => throw new NotImplementedException();
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask<PinMode> GetModeAsync(
    CancellationToken cancellationToken = default
  )
    => transceiver.CommandAsync(
      arg: this,
      cancellationToken: cancellationToken,
      constructCommand: GetDirectionCommand.ConstructCommand,
      parseResponse: GetDirectionCommand.ParseResponse
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public PinMode GetMode(
    CancellationToken cancellationToken = default
  )
    => transceiver.Command(
      arg: this,
      cancellationToken: cancellationToken,
      constructCommand: GetDirectionCommand.ConstructCommand,
      parseResponse: GetDirectionCommand.ParseResponse
    );

  private static class SetDirectionCommand {
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, (GpController gp, PinMode newDirection) args)
      => throw new NotImplementedException();

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static bool ParseResponse(ReadOnlySpan<byte> resp, (GpController gp, PinMode newDirection) args)
#pragma warning restore IDE0060

      => throw new NotImplementedException();
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask SetModeAsync(
    PinMode mode,
    CancellationToken cancellationToken = default
  )
  {
    transceiver.ThrowIfDisposed();

    return transceiver.CommandAsync(
      arg: (this, mode),
      cancellationToken: cancellationToken,
      constructCommand: SetDirectionCommand.ConstructCommand,
      parseResponse: SetDirectionCommand.ParseResponse
    ).AsValueTask();
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void SetMode(
    PinMode mode,
    CancellationToken cancellationToken = default
  )
  {
    transceiver.ThrowIfDisposed();

    transceiver.Command(
      arg: (this, mode),
      cancellationToken: cancellationToken,
      constructCommand: SetDirectionCommand.ConstructCommand,
      parseResponse: SetDirectionCommand.ParseResponse
    );
  }

  private static class GetValueCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, GpController gp)
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.12 GET GPIO VALUES
      comm[0] = 0x51; // Get GPIO Values
    }

    public static PinValue ParseResponse(ReadOnlySpan<byte> resp, GpController gp)
    {
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      var gpPinValue        = resp[2 + (2 * gp.GpPinNumber)];
      var gpDirectionValue  = resp[3 + (2 * gp.GpPinNumber)];

      if (gpPinValue == 0xEF || gpDirectionValue == 0xEF)
        throw new Mcp2221ACommandException($"{gp.PinName} is not set for GPIO operation");

      return gpPinValue;
    }
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask<PinValue> ReadAsync(
    CancellationToken cancellationToken = default
  )
    => transceiver.CommandAsync(
      arg: this,
      cancellationToken: cancellationToken,
      constructCommand: GetValueCommand.ConstructCommand,
      parseResponse: GetValueCommand.ParseResponse
    );

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public PinValue Read(
    CancellationToken cancellationToken = default
  )
    => transceiver.Command(
      arg: this,
      cancellationToken: cancellationToken,
      constructCommand: GetValueCommand.ConstructCommand,
      parseResponse: GetValueCommand.ParseResponse
    );

  private static class SetValueCommand {
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, (GpController gp, PinValue newValue) args)
#pragma warning restore IDE0060
    {
      // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
      comm[0] = 0x50; // Set GPIO Output Values
      comm[1] = 0x00; // Don't care

      // GP<n>
      comm[2 + (4 * args.gp.GpPinNumber)] = 0xFF; // Alter GP<n> Output = alter
      comm[3 + (4 * args.gp.GpPinNumber)] = (byte)args.newValue; // GP<n> output value
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
    public static bool ParseResponse(ReadOnlySpan<byte> resp, (GpController gp, PinValue newValue) args)
    {
      if (resp[1] != 0x00) // Command completed successfully
        throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

      if (
        resp[2 + (4 * args.gp.GpPinNumber)] == 0xEE ||
        resp[3 + (4 * args.gp.GpPinNumber)] == 0xEE ||
        resp[4 + (4 * args.gp.GpPinNumber)] == 0xEE ||
        resp[5 + (4 * args.gp.GpPinNumber)] == 0xEE
      ) {
        throw new Mcp2221ACommandException($"{args.gp.PinName} is not set for GPIO operation");
      }

      return true;
    }
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public ValueTask WriteAsync(
    PinValue value,
    CancellationToken cancellationToken = default
  )
  {
    transceiver.ThrowIfDisposed();

    return transceiver.CommandAsync(
      arg: (this, value),
      cancellationToken: cancellationToken,
      constructCommand: SetValueCommand.ConstructCommand,
      parseResponse: SetValueCommand.ParseResponse
    ).AsValueTask();
  }

  /// <inheritdoc/>
  [CLSCompliant(false)]
  public void Write(
    PinValue value,
    CancellationToken cancellationToken = default
  )
  {
    transceiver.ThrowIfDisposed();

    transceiver.Command(
      arg: (this, value),
      cancellationToken: cancellationToken,
      constructCommand: SetValueCommand.ConstructCommand,
      parseResponse: SetValueCommand.ParseResponse
    );
  }
}
