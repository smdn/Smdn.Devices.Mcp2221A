
using System;
using System.Device.I2c;
using System.Text;
using System.Runtime.InteropServices;

public class FourDigitFourteenSegmentDisplay : Iot.Device.Display.Ht16k33 {
  public const int NumberOfDigits = 4;

  private readonly ushort[] buffer = new ushort[NumberOfDigits];

  public FourDigitFourteenSegmentDisplay(I2cDevice i2cDevice)
    : base(i2cDevice)
  {
  }

  public override void Clear() => buffer.AsSpan().Clear();

  public override unsafe void Flush()
  {
    Span<byte> command = stackalloc byte [1 + NumberOfDigits * sizeof(ushort)];

    command[0] = 0b00000000; // display data address pointer

    MemoryMarshal.AsBytes(buffer.AsSpan()).CopyTo(command.Slice(1));

    base._i2cDevice.Write(command);
  }

  public void Write(ReadOnlySpan<char> charSequence)
  {
    var length = Math.Min(Encoding.ASCII.GetByteCount(charSequence), NumberOfDigits);
    Span<byte> byteSequence = stackalloc byte[length];

    Encoding.ASCII.GetBytes(charSequence.Slice(0, length), byteSequence);

    for (var index = 0; index < length; index++) {
      if (displayableCharacterRange.min <= byteSequence[index] && byteSequence[index] <= displayableCharacterRange.max)
        buffer[index] = characters[(int)(byteSequence[index] - displayableCharacterRange.min)];
      else
        throw new ArgumentException("contains undisplayable character", nameof(charSequence));
    }

    AutoFlush();
  }

  public override void Write(ReadOnlySpan<byte> data, int startAddress = 0)
  {
    throw new NotSupportedException();
    //AutoFlush();
  }

  private static readonly (char min, char max) displayableCharacterRange = ((char)0x20, (char)0x7F);

  // character definition imported from Smdn::LibLEDDisplay::Characters::FourteenSegmentCharacters
  private static readonly ushort[] characters = {
    // 0x00 - 0x1F (exclude control chars)
    // 0x20 - 0x2F
    //         D NMLKJHGGFEDCBA
    //         P       21
    (ushort)0b_0_00000000000000u, // SPACE
    (ushort)0b_1_00000000000110u, // '!'
    (ushort)0b_0_00001000000010u, // '"'
    (ushort)0b_0_01001011001110u, // '#'
    (ushort)0b_0_01001011101101u, // '$'
    (ushort)0b_0_10110111100100u, // '%'
    (ushort)0b_0_10010101011101u, // '&'
    (ushort)0b_0_00001000000000u, // '''
    (ushort)0b_0_00000000111001u, // '('
    (ushort)0b_0_00000000001111u, // ')'
    (ushort)0b_0_11111100000000u, // '*'
    (ushort)0b_0_01001011000000u, // '+'
    (ushort)0b_0_00100000000000u, // ','
    (ushort)0b_0_00000011000000u, // '-'
    (ushort)0b_1_00000000000000u, // '.'
    (ushort)0b_0_00110000000000u, // '/'
    // 0x30 - 0x3F
    (ushort)0b_0_00110000111111u, // '0'
    (ushort)0b_0_00000000000110u, // '1'
    (ushort)0b_0_00000011011011u, // '2'
    (ushort)0b_0_00000011001111u, // '3'
    (ushort)0b_0_00000011100110u, // '4'
    (ushort)0b_0_00000011101101u, // '5'
    (ushort)0b_0_00000011111101u, // '6'
    (ushort)0b_0_00000000000111u, // '7'
    (ushort)0b_0_00000011111111u, // '8'
    (ushort)0b_0_00000011101111u, // '9'
    (ushort)0b_0_00000001000001u, // ':'
    (ushort)0b_0_00100001000001u, // ';'
    (ushort)0b_0_10010000000000u, // '<'
    (ushort)0b_0_00000011001000u, // '='
    (ushort)0b_0_00100100000000u, // '>'
    (ushort)0b_1_01000010000011u, // '?'
    // 0x40 - 0x5F
    (ushort)0b_0_00001010111011u, // '@'
    (ushort)0b_0_00000011110111u, // 'A'
    (ushort)0b_0_01001010001111u, // 'B'
    (ushort)0b_0_00000000111001u, // 'C'
    (ushort)0b_0_01001000001111u, // 'D'
    (ushort)0b_0_00000011111001u, // 'E'
    (ushort)0b_0_00000011110001u, // 'F'
    (ushort)0b_0_00000010111101u, // 'G'
    (ushort)0b_0_00000011110110u, // 'H'
    (ushort)0b_0_01001000001001u, // 'I'
    (ushort)0b_0_00000000011110u, // 'J'
    (ushort)0b_0_10010001110000u, // 'K'
    (ushort)0b_0_00000000111000u, // 'L'
    (ushort)0b_0_00010100110110u, // 'M'
    (ushort)0b_0_10000100110110u, // 'N'
    (ushort)0b_0_00000000111111u, // 'O'
    (ushort)0b_0_00000011110011u, // 'P'
    (ushort)0b_0_10000000111111u, // 'Q'
    (ushort)0b_0_10000011110011u, // 'R'
    (ushort)0b_0_00000110001101u, // 'S'
    (ushort)0b_0_01001000000001u, // 'T'
    (ushort)0b_0_00000000111110u, // 'U'
    (ushort)0b_0_00110000110000u, // 'V'
    (ushort)0b_0_10101000110110u, // 'W'
    (ushort)0b_0_10110100000000u, // 'X'
    (ushort)0b_0_01010100000000u, // 'Y'
    (ushort)0b_0_00110000001001u, // 'Z'
    (ushort)0b_0_00000000111001u, // '['
    (ushort)0b_0_10000100000000u, // '\'
    (ushort)0b_0_00000000001111u, // ']'
    (ushort)0b_0_10100000000000u, // '^'
    (ushort)0b_0_00000000001000u, // '_'
    // 0x60 - 0x7F
    (ushort)0b_0_00000100000000u, // '`'
    (ushort)0b_0_01000001011000u, // 'a'
    (ushort)0b_0_00000011111100u, // 'b'
    (ushort)0b_0_00000011011000u, // 'c'
    (ushort)0b_0_00000011011110u, // 'd'
    (ushort)0b_0_00100001011000u, // 'e'
    (ushort)0b_0_00000001110001u, // 'f'
    (ushort)0b_0_00000110001111u, // 'g'
    (ushort)0b_0_00000011110100u, // 'h'
    (ushort)0b_0_01000000000000u, // 'i'
    (ushort)0b_0_00101000000000u, // 'j'
    (ushort)0b_0_11011000000000u, // 'k'
    (ushort)0b_0_00000000111000u, // 'l'
    (ushort)0b_0_01000011010100u, // 'm'
    (ushort)0b_0_01000001010000u, // 'n'
    (ushort)0b_0_00000011011100u, // 'o'
    (ushort)0b_0_00000011110011u, // 'p'
    (ushort)0b_0_00000011100111u, // 'q'
    (ushort)0b_0_00100010010000u, // 'r'
    (ushort)0b_0_10000010001000u, // 's'
    (ushort)0b_0_00000001111000u, // 't'
    (ushort)0b_0_00000000011100u, // 'u'
    (ushort)0b_0_00100000010000u, // 'v'
    (ushort)0b_0_10100000010100u, // 'w'
    (ushort)0b_0_10110100000000u, // 'x'
    (ushort)0b_0_00110100000000u, // 'y'
    (ushort)0b_0_00100001001000u, // 'z'
    (ushort)0b_0_01001001000000u, // '{'
    (ushort)0b_0_01001000000000u, // '|'
    (ushort)0b_0_01001010000000u, // '}'
    (ushort)0b_0_00000000000001u, // '~'
  };
}