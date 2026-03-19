// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections;
using System.Collections.Generic;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal sealed partial class Mcp2221AGpioDriver : /* System.Device.Gpio.GpioDriver, */ IGpControllerGroup {
  internal const int NumberOfGpPins = 4;

  internal Mcp2221ATransceiver Transceiver { get; }

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

  public int Count => NumberOfGpPins;

  internal Mcp2221AGpioDriver(
    Mcp2221ATransceiver transceiver
  )
  {
    Transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));

    Gp0 = new(this);
    Gp1 = new(this);
    Gp2 = new(this);
    Gp3 = new(this);
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public IEnumerator<GpController> GetEnumerator()
  {
    yield return Gp0;
    yield return Gp1;
    yield return Gp2;
    yield return Gp3;
  }
}
