// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Linq;

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

internal readonly struct I2cEngineState {
  public bool IsInitialState => StateMachineStateValue == 0 && Address == 0 && CommunicationSpeedDividerValue == 0;

  public I2cEngineTransferStatus BusStatus { get; init; }
  public byte StateMachineStateValue { get; init; }
  public int RequestedTransferLength { get; init; }
  public int AlreadyTransferredLength { get; init; }
  public int DataBufferCounter { get; init; }

  public ushort Address { get; init; }
  public int ReadPendingValue { get; init; }
  public byte CommunicationSpeedDividerValue { get; init; }
  public byte TimeoutValue { get; init; }
  public PinValue LineValueScl { get; init; }
  public PinValue LineValueSda { get; init; }

  public static I2cEngineState Parse(ReadOnlySpan<byte> resp)
    => new() {
#pragma warning disable IDE0055 // Fix formatting
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      BusStatus                       = (I2cEngineTransferStatus)resp[2],
      StateMachineStateValue          = resp[8], // Internal I2C state machine state value
      RequestedTransferLength         = resp[9] | (resp[10] << 8), // Lower/Higher byte of the requested I2C transfer length
      AlreadyTransferredLength        = resp[11] | (resp[12] << 8), // Lower/Higher byte of the already transferred number of bytes
      DataBufferCounter               = resp[13], // Internal I2C data buffer counter
      CommunicationSpeedDividerValue  = resp[14], // Current I2C communication speed divider value
      TimeoutValue                    = resp[15], // Current I2C time-out value
      Address                         = resp[16] /* | (resp[17] << 8) */, // Lower/Higher byte of the I2C address being used
      LineValueScl                    = resp[22], // SCL line value as read from the pin
      LineValueSda                    = resp[23], // SDA line value as read from the pin
      ReadPendingValue                = resp[25], // I2C Read pending value
#pragma warning restore IDE0055 // Fix formatting
    };

  public override string ToString()
    => string.Concat(
      "{",
      nameof(I2cEngineState),
      ": ",
      string.Join(
        ", ",
        new (string Name, object Value)[] {
          (nameof(StateMachineStateValue), $"0x{StateMachineStateValue:X2}"),
          (nameof(Address), $"0x{Address >> 1:X2}"),
          ("R/W", (Address & 0b1) == 0b1 ? "READ" : "WRITE"),
          (nameof(BusStatus), BusStatus),
          (nameof(RequestedTransferLength), RequestedTransferLength),
          (nameof(AlreadyTransferredLength), AlreadyTransferredLength),
          (nameof(ReadPendingValue), ReadPendingValue),
          (nameof(DataBufferCounter), DataBufferCounter),
          (nameof(CommunicationSpeedDividerValue), $"0x{CommunicationSpeedDividerValue:X2}"),
          (nameof(TimeoutValue), TimeoutValue),
          (nameof(LineValueScl), LineValueScl),
          (nameof(LineValueSda), LineValueSda),
        }.Select(pair => string.Concat(pair.Name, "=", pair.Value))
      ),
      "}"
    );
}
