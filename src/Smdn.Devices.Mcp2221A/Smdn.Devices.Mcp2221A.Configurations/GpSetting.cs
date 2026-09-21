// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER
#define SYSTEM_HASHCODE_COMBINE
#endif

using System;
using System.Device.Gpio;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A.Configurations;

/// <summary>
/// Represents the configuration of a single GP pin stored in Flash memory.
/// </summary>
[CLSCompliant(false)]
public readonly struct GpSetting : IEquatable<GpSetting> {
  private readonly byte index;
  private readonly byte registerValue;

  internal GpSetting(int index, byte registerValue)
  {
    this.index = (byte)(index & 0x03); // must be 0-3
    this.registerValue = registerValue;
  }

  /// <summary>
  /// Gets the index of the target GP pin (0 to 3).
  /// </summary>
  public int Index => index;

  /// <summary>
  /// Gets the 3-bit designated function value (<c>0b000</c> to <c>0b111</c>)
  /// stored in the register (<c>GPDES</c>).
  /// </summary>
  /// <remarks>
  /// This property returns the raw numerical value written in the specific
  /// register bits (Bits 2-0) as-is. Even if the register value is an
  /// undefined (Reserved) value and the <see cref="Function"/> property
  /// falls back to the factory default function, this property allows
  /// inspecting the raw value actually written.
  /// </remarks>
  /// <seealso cref="Function"/>
  public byte Designation => (byte)(registerValue & 0b_000_0_0_111);

  /// <summary>
  /// Gets the function assigned to the GP pin.
  /// </summary>
  /// <remarks>
  /// If the designated register value (<see cref="Designation"/>) is
  /// an undefined value (Reserved), it automatically falls back to the
  /// factory default function for the target pin for safety.
  /// </remarks>
  /// <seealso cref="Designation"/>
  public GpFunction Function {
    get {
      const bool FallBackToFactoryDefault = false;

      var designation = (GpDesignation)Designation;

      return index switch {
        0 => Gp0Controller.TranslateDesignation(designation, throwIfUnsupported: FallBackToFactoryDefault),
        1 => Gp1Controller.TranslateDesignation(designation, throwIfUnsupported: FallBackToFactoryDefault),
        2 => Gp2Controller.TranslateDesignation(designation, throwIfUnsupported: FallBackToFactoryDefault),
        3 => Gp3Controller.TranslateDesignation(designation, throwIfUnsupported: FallBackToFactoryDefault),
        _ => throw new InvalidOperationException($"GP pin index {index} is not supported. It must be between 0 and 3."),
      };
    }
  }

  /// <summary>
  /// Gets a value indicating whether the GP pin is configured for
  /// the GPIO function.
  /// </summary>
  /// <value>
  /// <see langword="true"/> if <see cref="Function"/> is <see cref="GpFunction.Gpio"/>;
  /// otherwise, <see langword="false"/>.
  /// </value>
  /// <remarks>
  /// The <see cref="GpioMode"/> and <see cref="GpioOutputValue"/> properties are
  /// meaningful only when this property is <see langword="true"/>.
  /// </remarks>
  /// <seealso cref="GpioMode"/>
  /// <seealso cref="GpioOutputValue"/>
  public bool IsGpio => Function == GpFunction.Gpio;

  /// <summary>
  /// Gets the initial GPIO mode (input or output).
  /// </summary>
  /// <value>
  /// A <see cref="PinMode"/> indicating the pin direction.
  /// </value>
  /// <remarks>
  /// This property is meaningful only when <see cref="IsGpio"/> is <see langword="true"/>
  /// (that is, <see cref="Function"/> is <see cref="GpFunction.Gpio"/>).
  /// Even if <see cref="IsGpio"/> is <see langword="false"/>, no exception is thrown,
  /// and the initial value held in the register (<c>GPIODIR</c>) is returned as-is.
  /// </remarks>
  /// <seealso cref="IsGpio"/>
  /// <seealso cref="Function"/>
  /// <seealso cref="GpioOutputValue"/>
  public PinMode GpioMode
    => (registerValue & 0b_000_0_1_000) != 0 ? PinMode.Input : PinMode.Output;

  /// <summary>
  /// Gets the initial GPIO output level (HIGH or LOW).
  /// </summary>
  /// <value>
  /// A <see cref="PinValue"/> indicating the pin output level.
  /// </value>
  /// <remarks>
  /// This property is meaningful only when <see cref="IsGpio"/> is <see langword="true"/>
  /// (that is, <see cref="Function"/> is <see cref="GpFunction.Gpio"/>) and
  /// <see cref="GpioMode"/> is <see cref="PinMode.Output"/>.
  /// Even if these conditions are not met, no exception is thrown, and the initial
  /// value held in the register (<c>GPIOOUTVAL</c>) is returned as-is.
  /// </remarks>
  /// <seealso cref="IsGpio"/>
  /// <seealso cref="Function"/>
  /// <seealso cref="GpioMode"/>
  public PinValue GpioOutputValue
    => (registerValue & 0b_000_1_0_000) != 0 ? PinValue.High : PinValue.Low;

  /// <summary>
  /// Deconstructs the GP pin settings (excluding the pin index).
  /// </summary>
  /// <param name="function">
  /// The assigned function (<see cref="Function"/>).
  /// </param>
  /// <param name="mode">
  /// The GPIO input/output mode (<see cref="GpioMode"/>).
  /// </param>
  /// <param name="value">
  /// The initial GPIO output value (<see cref="GpioOutputValue"/>).
  /// </param>
  public void Deconstruct(
    out GpFunction function,
    out PinMode mode,
    out PinValue value
  )
  {
    function = Function;
    mode = GpioMode;
    value = GpioOutputValue;
  }

  /// <summary>
  /// Deconstructs the GP pin settings (including the pin index).
  /// </summary>
  /// <param name="index">
  /// The index of the target GP pin (<see cref="Index"/>).
  /// </param>
  /// <param name="function">
  /// The assigned function (<see cref="Function"/>).
  /// </param>
  /// <param name="mode">
  /// The GPIO input/output mode (<see cref="GpioMode"/>).
  /// </param>
  /// <param name="value">
  /// The initial GPIO output value (<see cref="GpioOutputValue"/>).
  /// </param>
  public void Deconstruct(
    out int index,
    out GpFunction function,
    out PinMode mode,
    out PinValue value
  )
  {
    index = Index;
    function = Function;
    mode = GpioMode;
    value = GpioOutputValue;
  }

  /// <summary>
  /// Deconstructs the GP pin settings (including the pin index and designated value).
  /// </summary>
  /// <param name="index">
  /// The index of the target GP pin (<see cref="Index"/>).
  /// </param>
  /// <param name="designation">
  /// The 3-bit designated function value (<see cref="Designation"/>).
  /// </param>
  /// <param name="function">
  /// The assigned function (<see cref="Function"/>).
  /// </param>
  /// <param name="mode">
  /// The GPIO input/output mode (<see cref="GpioMode"/>).
  /// </param>
  /// <param name="value">
  /// The initial GPIO output value (<see cref="GpioOutputValue"/>).
  /// </param>
  public void Deconstruct(
    out int index,
    out byte designation,
    out GpFunction function,
    out PinMode mode,
    out PinValue value
  )
  {
    index = Index;
    designation = Designation;
    function = Function;
    mode = GpioMode;
    value = GpioOutputValue;
  }

  /// <summary>
  /// Indicates whether the current instance is equal to another
  /// <see cref="GpSetting"/> instance.
  /// </summary>
  /// <param name="other">
  /// A <see cref="GpSetting"/> to compare with this instance.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the <see cref="Index"/>, <see cref="Function"/>,
  /// <see cref="GpioMode"/>, and <see cref="GpioOutputValue"/> properties are equal;
  /// otherwise, <see langword="false"/>.
  /// The <see cref="Designation"/> property does not affect the evaluation result.
  /// </returns>
  public bool Equals(GpSetting other)
    =>
      Index == other.Index &&
      Function == other.Function &&
      GpioMode == other.GpioMode &&
      GpioOutputValue == other.GpioOutputValue;

  /// <inheritdoc/>
  public override bool Equals(object? obj)
    => obj is GpSetting other && Equals(other);

  /// <inheritdoc/>
  public override int GetHashCode()
#if SYSTEM_HASHCODE_COMBINE
    => HashCode.Combine(Index, Function, GpioMode, GpioOutputValue);
#else
  {
    unchecked {
      var hash = 17;

      hash = (hash * 31) + Index.GetHashCode();
      hash = (hash * 31) + Function.GetHashCode();
      hash = (hash * 31) + GpioMode.GetHashCode();
      hash = (hash * 31) + GpioOutputValue.GetHashCode();

      return hash;
    }
  }
#endif

  /// <summary>
  /// Determines whether two <see cref="GpSetting"/> instances are equal.
  /// </summary>
  /// <param name="left">The first <see cref="GpSetting"/> to compare.</param>
  /// <param name="right">The second <see cref="GpSetting"/> to compare.</param>
  /// <returns>
  /// <see langword="true"/> if the <see cref="Index"/>, <see cref="Function"/>,
  /// <see cref="GpioMode"/>, and <see cref="GpioOutputValue"/> properties are equal;
  /// otherwise, <see langword="false"/>.
  /// The <see cref="Designation"/> property does not affect the evaluation result.
  /// </returns>
  public static bool operator ==(GpSetting left, GpSetting right)
    => left.Equals(right);

  /// <summary>
  /// Determines whether two <see cref="GpSetting"/> instances are different.
  /// </summary>
  /// <param name="left">The first <see cref="GpSetting"/> to compare.</param>
  /// <param name="right">The second <see cref="GpSetting"/> to compare.</param>
  /// <returns>
  /// <see langword="true"/> if the instances are different;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  public static bool operator !=(GpSetting left, GpSetting right)
    => !left.Equals(right);

  /// <summary>
  /// Returns a string that represents the current instance.
  /// </summary>
  /// <returns>
  /// A string containing <see cref="Index"/>, <see cref="Function"/>,
  /// <see cref="GpioMode"/>, and <see cref="GpioOutputValue"/>.
  /// </returns>
  public override string ToString()
    => $"GP{Index}: Function={Function}, GpioMode={GpioMode}, GpioOutputValue={GpioOutputValue}";
}
