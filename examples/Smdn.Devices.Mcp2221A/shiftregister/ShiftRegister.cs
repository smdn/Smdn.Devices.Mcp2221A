// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Device.Gpio;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

enum BitOrder {
  LSBFirst,
  HSBFirst,
}

enum Endianness {
  LittleEndian,
  BigEndian,
}

class ShiftRegister {
  private readonly IGpioController gpioLatch;
  private readonly IGpioController gpioClock;
  private readonly IGpioController gpioData;

  /// <param name="gpioLatch">storage register clock pin (RCLK/ST_CP)</param>
  /// <param name="gpioClock">shift register clock pin (SRCLK/SH_CP)</param>
  /// <param name="gpioData">serial output pin (SER)</param>
  public ShiftRegister(
    IGpioController gpioLatch,
    IGpioController gpioClock,
    IGpioController gpioData
  )
  {
    this.gpioLatch = gpioLatch ?? throw new ArgumentNullException(nameof(gpioLatch));
    this.gpioClock = gpioClock ?? throw new ArgumentNullException(nameof(gpioClock));
    this.gpioData = gpioData ?? throw new ArgumentNullException(nameof(gpioData));
  }

  public async ValueTask WriteAsync(
    ReadOnlyMemory<byte> sequence,
    BitOrder bitOrder = default,
    CancellationToken cancellationToken = default
  )
  {
    var (firstBitMask, shiftAmount) = bitOrder switch {
      BitOrder.LSBFirst => (0b_00000001u, +1),
      BitOrder.HSBFirst => (0b_10000000u, -1),
      _ => throw new ArgumentException($"undefined bit order ({bitOrder})", nameof(bitOrder)),
    };

    for (var byt = 0; byt < sequence.Length; byt++) {
      for (uint bit = 0u, bitMask = firstBitMask; bit < 8u; bit++) {
        await gpioData.WriteAsync(0L != (sequence.Span[byt] & bitMask), cancellationToken).ConfigureAwait(false);

        await gpioClock.WriteAsync(PinValue.High, cancellationToken).ConfigureAwait(false);
        await gpioClock.WriteAsync(PinValue.Low, cancellationToken).ConfigureAwait(false);

        bitMask = BitOperations.RotateLeft(bitMask, shiftAmount);
      }
    }

    await gpioLatch.WriteAsync(PinValue.Low, cancellationToken).ConfigureAwait(false);
    await gpioLatch.WriteAsync(PinValue.High, cancellationToken).ConfigureAwait(false);
  }

  public void Write(
    ReadOnlySpan<byte> sequence,
    BitOrder bitOrder = default,
    CancellationToken cancellationToken = default
  )
  {
    var (firstBitMask, shiftAmount) = bitOrder switch {
      BitOrder.LSBFirst => (0b_00000001u, +1),
      BitOrder.HSBFirst => (0b_10000000u, -1),
      _ => throw new ArgumentException($"undefined bit order ({bitOrder})", nameof(bitOrder)),
    };

    for (var byt = 0; byt < sequence.Length; byt++) {
      for (uint bit = 0u, bitMask = firstBitMask; bit < 8u; bit++) {
        gpioData.Write(0L != (sequence[byt] & bitMask), cancellationToken);

        gpioClock.Write(PinValue.High, cancellationToken);
        gpioClock.Write(PinValue.Low, cancellationToken);

        bitMask = BitOperations.RotateLeft(bitMask, shiftAmount);
      }
    }

    gpioLatch.Write(PinValue.Low, cancellationToken);
    gpioLatch.Write(PinValue.High, cancellationToken);
  }

  public async ValueTask WriteAsync(
    byte value,
    BitOrder bitOrder = default
  )
  {
    var sequence = ArrayPool<byte>.Shared.Rent(1);

    try {
      sequence[0] = value;

      await WriteAsync(sequence.AsMemory(0, 1), bitOrder).ConfigureAwait(false);
    }
    finally {
      ArrayPool<byte>.Shared.Return(sequence);
    }
  }

  public void Write(
    byte value,
    BitOrder bitOrder = default
  )
    => Write([value], bitOrder);

  public async ValueTask WriteAsync(
    uint value,
    Endianness endianness = Endianness.LittleEndian,
    BitOrder bitOrder = default
  )
  {
    var sequence = ArrayPool<byte>.Shared.Rent(4);

    try {
      if (endianness == Endianness.LittleEndian)
        BinaryPrimitives.WriteUInt32LittleEndian(sequence.AsSpan(0, 4), value);
      else if (endianness == Endianness.BigEndian)
        BinaryPrimitives.WriteUInt32BigEndian(sequence.AsSpan(0, 4), value);
      else
        throw new ArgumentException($"undefined endianness ({endianness})", nameof(endianness));

      await WriteAsync(sequence.AsMemory(0, 4), bitOrder).ConfigureAwait(false);
    }
    finally {
      ArrayPool<byte>.Shared.Return(sequence);
    }
  }

  public void Write(
    uint value,
    Endianness endianness = Endianness.LittleEndian,
    BitOrder bitOrder = default
  )
  {
    Span<byte> sequence = stackalloc byte[4];

    if (endianness == Endianness.LittleEndian)
      BinaryPrimitives.WriteUInt32LittleEndian(sequence, value);
    else if (endianness == Endianness.BigEndian)
      BinaryPrimitives.WriteUInt32BigEndian(sequence, value);
    else
      throw new ArgumentException($"undefined endianness ({endianness})", nameof(endianness));

    Write(sequence, bitOrder);
  }
}
