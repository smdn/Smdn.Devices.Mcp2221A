// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal interface IInterruptController {
  ValueTask ConfigureAsInterruptDetectionAsync(
    CancellationToken cancellationToken = default
  );
  void ConfigureAsInterruptDetection(
    CancellationToken cancellationToken = default
  );
}
