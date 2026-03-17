// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal interface IClockController {
  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <seealso cref="GpFunction.ClockOutput"/>
  ValueTask ConfigureAsClockOutputAsync(
    CancellationToken cancellationToken = default
  );

  /// <param name="cancellationToken">
  /// The <see cref="CancellationToken"/> to monitor for cancellation requests.
  /// The default value is <see cref="CancellationToken.None"/>.
  /// </param>
  /// <seealso cref="GpFunction.ClockOutput"/>
  void ConfigureAsClockOutput(
    CancellationToken cancellationToken = default
  );

#if __FUTURE_VERSION
  int ClockFrequency { get; set; }
#endif
}
