// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal interface IAdcController {
  ValueTask ConfigureAsAdcAsync(
    CancellationToken cancellationToken = default
  );
  void ConfigureAsAdc(
    CancellationToken cancellationToken = default
  );
#if __FUTURE_VERSION
  int AdcValue { get; }
#endif
}
