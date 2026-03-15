#!/usr/bin/env dotnet-script
// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
//
// requirements:
//   .NET Core or .NET Runtime
//
// usage:
//   1. install dotnet-script
//     dotnet tool install -g dotnet-script
//   2. run this script
//     tshark -r  *.pcap -Tfields ... | ./format-usbdata.csx
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

private class UserDataParser {
  public static void Parse(bool isCommand, ReadOnlySpan<byte> userData)
  {
    Console.Write(isCommand ? "COMMAND: " : "RESPONSE: ");

    switch (userData[0]) {
      case 0x10:
        Console.Write("[STATUS/SET PARAMETER] ");

        if (!isCommand) {
          var engineState = new {
            BusStatus                       = userData[2] switch {
              0x00 => "NoSpecialOperation",
              0x10 => "MarkedForCancellation",
              0x11 => "AlreadyInIdleMode",
              _ => "Unkwnown"
            },
            StateMachineStateValue          = "0x" + userData[8].ToString("X2"), // Internal I2C state machine state value
            RequestedTransferLength         = (int)(userData[ 9] | (userData[10] << 8)), // Lower/Higher byte of the requested I2C transfer length
            AlreadyTransferredLength        = (int)(userData[11] | (userData[12] << 8)), // Lower/Higher byte of the already transferred number of bytes
            DataBufferCounter               = userData[13], // Internal I2C data buffer counter
            CommunicationSpeedDividerValue  = "0x" + userData[14].ToString("X2"), // Current I2C communication speed divider value
            TimeoutValue                    = userData[15], // Current I2C time-out value
            Address                         = "0x" + ((int)((userData[16] | (userData[17] << 8)) >> 1)).ToString("X2"), // Lower/Higher byte of the I2C address being used
            ReadWrite                       = ((int)(userData[16] | (userData[17] << 8)) & 0b1) == 0b1 ? "READ" : "WRITE", // Lower/Higher byte of the I2C address being used
            LineValueSCL                    = userData[22] == 0 ? "LOW" : "HIGH", // SCL line value as read from the pin
            LineValueSDA                    = userData[23] == 0 ? "LOW" : "HIGH", // SDA line value as read from the pin
            ReadPendingValue                = userData[25], // I2C Read pending value
          };

          Console.Write(engineState);
        }

        break;

      case 0x40:
        Console.Write("[I2C READ DATA - GET I2C DATA] ");
        break;

      case 0x90:
        Console.Write("[I2C WRITE DATA] ");
        if (isCommand)
          ParseI2CAddress(userData[3]);
        if (isCommand)
          Console.Write($"RequestedLength={userData[1] | userData[2] << 8} ");
        break;

      case 0x91:
        Console.Write("[I2C READ DATA] ");
        if (isCommand)
          ParseI2CAddress(userData[3]);
        if (isCommand)
          Console.Write($"RequestedLength={userData[1] | userData[2] << 8} ");
        break;
    }

    Console.WriteLine();

    static void ParseI2CAddress(byte addr)
    {
      Console.Write($"Address: 0x{addr >> 1:X2}(");
      Console.Write((addr & 0b1) == 0b1 ? "READ" : "WRITE");
      Console.Write(") ");
    }
  }
}

static ReadOnlyMemory<byte> ParseByteSequence(string str)
{
  var bytes = new byte[str.Length / 2];

  for (var i = 0; i < str.Length; i += 2)
    bytes[i / 2] = (byte)Convert.ToByte(str.Substring(i, 2), 16);

  return bytes;
}

static string FormatByteSequence(ReadOnlySpan<byte> sequence)
{
  return BitConverter.ToString(sequence.ToArray());
}

for (;;) {
  var line = Console.ReadLine();

  if (line == null)
    break;
  var fields = line.Split('\t', StringSplitOptions.TrimEntries);

  if (!int.TryParse(fields[0], out var frameNumber))
    continue; // header line

  var isCommand = string.Equals(fields[1], "host", StringComparison.OrdinalIgnoreCase);
  var userDataSequence = ParseByteSequence(fields[3]);

  UserDataParser.Parse(isCommand, userDataSequence.Span);

  fields[3] = FormatByteSequence(userDataSequence.Span);

  Console.WriteLine(string.Join("\t", fields));
  Console.WriteLine();
}