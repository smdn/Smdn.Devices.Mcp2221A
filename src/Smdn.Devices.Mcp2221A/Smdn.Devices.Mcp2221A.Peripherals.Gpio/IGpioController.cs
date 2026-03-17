// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

internal interface IGpioController {
  ValueTask ConfigureAsGpioAsync(
    PinMode initialDirection = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  );

  void ConfigureAsGpio(
    PinMode initialDirection = PinMode.Output,
    PinValue initialValue = default,
    CancellationToken cancellationToken = default
  );

  ValueTask<PinMode> GetDirectionAsync(
    CancellationToken cancellationToken = default
  );

  PinMode GetDirection(
    CancellationToken cancellationToken = default
  );

  ValueTask SetDirectionAsync(
    PinMode newDirection,
    CancellationToken cancellationToken = default
  );

  void SetDirection(
    PinMode newDirection,
    CancellationToken cancellationToken = default
  );

  ValueTask<PinValue> GetValueAsync(
    CancellationToken cancellationToken = default
  );

  PinValue GetValue(
    CancellationToken cancellationToken = default
  );

  ValueTask SetValueAsync(
    PinValue newValue,
    CancellationToken cancellationToken = default
  );

  void SetValue(
    PinValue newValue,
    CancellationToken cancellationToken = default
  );
}
