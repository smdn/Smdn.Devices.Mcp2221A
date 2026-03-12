// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.IO.UsbHid;

public sealed class PseudoUsbHidService(IReadOnlyList<IUsbHidDevice> devices) : IUsbHidService {
  private bool disposed = false;

  public IReadOnlyList<IUsbHidDevice> GetDevices(
    CancellationToken cancellationToken = default
  )
  {
    if (disposed)
      throw new ObjectDisposedException(GetType().FullName);

    cancellationToken.ThrowIfCancellationRequested();

    return devices;
  }

  public void Dispose()
  {
    // nothing to do
    disposed = true;
  }

  public ValueTask DisposeAsync()
  {
    // nothing to do
    disposed = true;

    return default;
  }
}
