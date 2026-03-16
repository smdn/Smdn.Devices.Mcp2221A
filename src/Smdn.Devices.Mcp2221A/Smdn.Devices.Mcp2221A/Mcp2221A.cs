// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0055, CA1724
public partial class Mcp2221A :
  IDisposable,
  IAsyncDisposable
{
#pragma warning restore IDE0055, CA1724
  private Mcp2221ATransceiver? transceiver;
  internal Mcp2221ATransceiver Transceiver => transceiver ?? throw new ObjectDisposedException(GetType().Name);
  public IUsbHidDevice HidDevice => transceiver?.EndPoint?.Device ?? throw new ObjectDisposedException(GetType().Name);

  [CLSCompliant(false)]
  public Mcp2221AI2cBus I2c {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  public IGpControllerGroup GpPins {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  public Gp0Controller GpPin0 => GpPins.Gp0;
  public Gp1Controller GpPin1 => GpPins.Gp1;
  public Gp2Controller GpPin2 => GpPins.Gp2;
  public Gp3Controller GpPin3 => GpPins.Gp3;

  private Mcp2221A(
    Mcp2221ATransceiver transceiver,
    IMcp2221AInfo info,
    ILogger? logger
  )
  {
    this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
    this.info = info ?? throw new ArgumentNullException(nameof(info));

    GpPins = new Mcp2221AGpioDriver(transceiver: transceiver);
    I2c = new(this, logger);
  }

  public void Dispose()
  {
    Dispose(disposing: true);

    GC.SuppressFinalize(this);
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);

    Dispose(disposing: false);

    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing) {
      transceiver?.Dispose();
      transceiver = null;
    }
  }

  protected virtual async ValueTask DisposeAsyncCore()
  {
    if (transceiver is not null) {
      await transceiver.DisposeAsync().ConfigureAwait(false);
      transceiver = null;
    }
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(transceiver))]
#endif
  internal void ThrowIfDisposed() => _ = transceiver ?? throw new ObjectDisposedException(GetType().Name);
}
