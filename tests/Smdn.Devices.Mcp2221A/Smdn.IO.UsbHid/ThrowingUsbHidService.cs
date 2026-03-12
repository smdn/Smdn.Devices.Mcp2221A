// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.IO.UsbHid;

public sealed class ThrowingUsbHidService : IUsbHidService {
  public IReadOnlyList<IUsbHidDevice> GetDevices(
    CancellationToken cancellationToken = default
  )
    => throw new NotSupportedException();

  public void Dispose()
  {
  }

  public ValueTask DisposeAsync() => default;
}
