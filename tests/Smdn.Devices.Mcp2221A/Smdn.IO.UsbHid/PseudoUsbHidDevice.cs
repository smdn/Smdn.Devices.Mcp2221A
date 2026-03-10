// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.IO.UsbHid;

class PseudoUsbHidDevice : IUsbHidDevice {
  public bool IsDisposed { get; private set; }

  /// <inheritdoc/>
  public int VendorId { get; }

  /// <inheritdoc/>
  public int ProductId { get; }

  private readonly string? productName;
  private readonly string? manufacturer;
  private readonly string? serialNumber;

  public bool TryGetProductName(
    [NotNullWhen(true)]
    out string? productName
  )
    => (productName = this.productName) is not null;

  public bool TryGetManufacturer(
    [NotNullWhen(true)]
    out string? manufacturer
  )
    => (manufacturer = this.manufacturer) is not null;

  public bool TryGetSerialNumber(
    [NotNullWhen(true)]
    out string? serialNumber
  )
    => (serialNumber = this.serialNumber) is not null;

  public bool TryGetDeviceIdentifier(
    [NotNullWhen(true)]
    out string? deviceIdentifier
  )
    => (deviceIdentifier = "/dev/null") is not null;

  private readonly Func<Stream>? createWriteStream;
  private readonly Func<Stream>? createReadStream;

  private PseudoUsbHidEndPoint? endpoint = null;
  public PseudoUsbHidEndPoint EndPoint
    => IsDisposed
      ? throw new ObjectDisposedException(GetType().FullName)
      : endpoint ?? throw new InvalidOperationException("endpoint is not opened");

  public PseudoUsbHidDevice(
    int vendorId,
    int productId,
    string? productName,
    string? manufacturer,
    string? serialNumber,
    Func<Stream>? createWriteStream,
    Func<Stream>? createReadStream
  )
  {
    VendorId = vendorId;
    ProductId = productId;
    this.productName = productName;
    this.manufacturer = manufacturer;
    this.serialNumber = serialNumber;
    this.createWriteStream = createWriteStream;
    this.createReadStream = createReadStream;
  }

  public void Dispose()
  {
    IsDisposed = true;

    endpoint = null;
  }

  public async ValueTask DisposeAsync()
  {
    IsDisposed = true;

    endpoint = null;
  }

  public IUsbHidEndPoint OpenEndPoint(
    bool openOutEndPoint,
    bool openInEndPoint,
    bool shouldDisposeDevice,
    CancellationToken cancellationToken
  )
  {
    endpoint = new PseudoUsbHidEndPoint(
      this,
      openOutEndPoint ? createWriteStream?.Invoke() : null,
      openInEndPoint ? createReadStream?.Invoke() : null,
      shouldDisposeDevice
    );

    return endpoint;
  }

  public ValueTask<IUsbHidEndPoint> OpenEndPointAsync(
    bool openOutEndPoint,
    bool openInEndPoint,
    bool shouldDisposeDevice,
    CancellationToken cancellationToken
  )
  {
    endpoint = new PseudoUsbHidEndPoint(
      this,
      openOutEndPoint ? createWriteStream?.Invoke() : null,
      openInEndPoint ? createReadStream?.Invoke() : null,
      shouldDisposeDevice
    );

    return new(endpoint);
  }
}
