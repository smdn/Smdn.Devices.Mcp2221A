// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Collections.Generic;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

public interface IGpControllerGroup : IReadOnlyList<GpController> {
  Gp0Controller Gp0 { get; }
  Gp1Controller Gp1 { get; }
  Gp2Controller Gp2 { get; }
  Gp3Controller Gp3 { get; }
}
