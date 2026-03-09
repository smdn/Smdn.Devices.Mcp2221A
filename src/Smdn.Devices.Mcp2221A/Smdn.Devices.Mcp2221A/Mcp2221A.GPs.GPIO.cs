// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221A {
#pragma warning restore IDE0040
  internal interface IGpioFunctionality {
    ValueTask ConfigureAsGpioAsync(
      PinMode initialDirection = PinMode.Output,
      PinValue initialValue = default,
      CancellationToken cancellationToken = default
    );
    void ConfigureAsGpio(
      PinMode initialDirection = PinMode.Output,
      PinValue initialValue = default,
      CancellationToken cancellationToken = default
    );

    ValueTask<PinMode> GetDirectionAsync(
      CancellationToken cancellationToken = default
    );
    PinMode GetDirection(
      CancellationToken cancellationToken = default
    );

    ValueTask SetDirectionAsync(
      PinMode newDirection,
      CancellationToken cancellationToken = default
    );
    void SetDirection(
      PinMode newDirection,
      CancellationToken cancellationToken = default
    );

    ValueTask<PinValue> GetValueAsync(
      CancellationToken cancellationToken = default
    );
    PinValue GetValue(
      CancellationToken cancellationToken = default
    );

    ValueTask SetValueAsync(
      PinValue newValue,
      CancellationToken cancellationToken = default
    );
    void SetValue(
      PinValue newValue,
      CancellationToken cancellationToken = default
    );
  }

#pragma warning disable IDE0040
  partial class GPFunctionality : IGpioFunctionality {
#pragma warning restore IDE0040
    [CLSCompliant(false)]
    public ValueTask ConfigureAsGpioAsync(
      PinMode initialDirection = PinMode.Output,
      PinValue initialValue = default,
      CancellationToken cancellationToken = default
    )
      => ConfigureGPDesignationAsync(
        pinDesignation: $"GPIO{GPIndex}",
        gpDesignation: GPDesignation.GpioOperation,
        gpioInitialDirection: initialDirection,
        gpioInitialValue: initialValue,
        cancellationToken: cancellationToken
      );

    [CLSCompliant(false)]
    public void ConfigureAsGpio(
      PinMode initialDirection = PinMode.Output,
      PinValue initialValue = default,
      CancellationToken cancellationToken = default
    )
      => ConfigureGPDesignation(
        pinDesignation: $"GPIO{GPIndex}",
        gpDesignation: GPDesignation.GpioOperation,
        gpioInitialDirection: initialDirection,
        gpioInitialValue: initialValue,
        cancellationToken: cancellationToken
      );

    private static class GetDirectionCommand {
      public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, GPFunctionality gp)
        => throw new NotImplementedException();

#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
      public static PinMode ParseResponse(ReadOnlySpan<byte> resp, GPFunctionality gp)
#pragma warning restore IDE0060
        => throw new NotImplementedException();
    }

    [CLSCompliant(false)]
    public ValueTask<PinMode> GetDirectionAsync(
      CancellationToken cancellationToken = default
    )
      => device.CommandAsync(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: GetDirectionCommand.ConstructCommand,
        parseResponse: GetDirectionCommand.ParseResponse
      );

    [CLSCompliant(false)]
    public PinMode GetDirection(
      CancellationToken cancellationToken = default
    )
      => device.Command(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: GetDirectionCommand.ConstructCommand,
        parseResponse: GetDirectionCommand.ParseResponse
      );

    private static class SetDirectionCommand {
      [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
      public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, (GPFunctionality gp, PinMode newDirection) args)
        => throw new NotImplementedException();

      [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
      public static bool ParseResponse(ReadOnlySpan<byte> resp, (GPFunctionality gp, PinMode newDirection) args)
#pragma warning restore IDE0060

        => throw new NotImplementedException();
    }

    [CLSCompliant(false)]
    public ValueTask SetDirectionAsync(
      PinMode newDirection,
      CancellationToken cancellationToken = default
    )
      => device.CommandAsync(
        arg: (this, newDirection),
        cancellationToken: cancellationToken,
        constructCommand: SetDirectionCommand.ConstructCommand,
        parseResponse: SetDirectionCommand.ParseResponse
      ).AsValueTask();

    [CLSCompliant(false)]
    public void SetDirection(
      PinMode newDirection,
      CancellationToken cancellationToken = default
    )
      => device.Command(
        arg: (this, newDirection),
        cancellationToken: cancellationToken,
        constructCommand: SetDirectionCommand.ConstructCommand,
        parseResponse: SetDirectionCommand.ParseResponse
      );

    private static class GetValueCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
      public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, GPFunctionality gp)
#pragma warning restore IDE0060
      {
        // [MCP2221A] 3.1.12 GET GPIO VALUES
        comm[0] = 0x51; // Get GPIO Values
      }

      public static PinValue ParseResponse(ReadOnlySpan<byte> resp, GPFunctionality gp)
      {
        if (resp[1] != 0x00) // Command completed successfully
          throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

        var gpPinValue        = resp[2 + (2 * gp.GPIndex)];
        var gpDirectionValue  = resp[3 + (2 * gp.GPIndex)];

        if (gpPinValue == 0xEF || gpDirectionValue == 0xEF)
          throw new Mcp2221ACommandException($"{gp.PinName} is not set for GPIO operation");

        return gpPinValue;
      }
    }

    [CLSCompliant(false)]
    public ValueTask<PinValue> GetValueAsync(
      CancellationToken cancellationToken = default
    )
      => device.CommandAsync(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: GetValueCommand.ConstructCommand,
        parseResponse: GetValueCommand.ParseResponse
      );

    [CLSCompliant(false)]
    public PinValue GetValue(
      CancellationToken cancellationToken = default
    )
      => device.Command(
        arg: this,
        cancellationToken: cancellationToken,
        constructCommand: GetValueCommand.ConstructCommand,
        parseResponse: GetValueCommand.ParseResponse
      );

    private static class SetValueCommand {
      [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
      public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, (GPFunctionality gp, PinValue newValue) args)
#pragma warning restore IDE0060
      {
        // [MCP2221A] 3.1.11 SET GPIO OUTPUT VALUES
        comm[0] = 0x50; // Set GPIO Output Values
        comm[1] = 0x00; // Don't care

        // GP<n>
        comm[2 + (4 * args.gp.GPIndex)] = 0xFF; // Alter GP<n> Output = alter
        comm[3 + (4 * args.gp.GPIndex)] = (byte)args.newValue; // GP<n> output value
      }

      [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
      public static bool ParseResponse(ReadOnlySpan<byte> resp, (GPFunctionality gp, PinValue newValue) args)
      {
        if (resp[1] != 0x00) // Command completed successfully
          throw new Mcp2221ACommandException($"unexpected command response ({resp[1]:X2})");

        if (
          resp[2 + (4 * args.gp.GPIndex)] == 0xEE ||
          resp[3 + (4 * args.gp.GPIndex)] == 0xEE ||
          resp[4 + (4 * args.gp.GPIndex)] == 0xEE ||
          resp[5 + (4 * args.gp.GPIndex)] == 0xEE
        ) {
          throw new Mcp2221ACommandException($"{args.gp.PinName} is not set for GPIO operation");
        }

        return true;
      }
    }

    [CLSCompliant(false)]
    public ValueTask SetValueAsync(
      PinValue newValue,
      CancellationToken cancellationToken = default
    )
      => device.CommandAsync(
        arg: (this, newValue),
        cancellationToken: cancellationToken,
        constructCommand: SetValueCommand.ConstructCommand,
        parseResponse: SetValueCommand.ParseResponse
      ).AsValueTask();

    [CLSCompliant(false)]
    public void SetValue(
      PinValue newValue,
      CancellationToken cancellationToken = default
    )
      => device.Command(
        arg: (this, newValue),
        cancellationToken: cancellationToken,
        constructCommand: SetValueCommand.ConstructCommand,
        parseResponse: SetValueCommand.ParseResponse
      );
  }
}
