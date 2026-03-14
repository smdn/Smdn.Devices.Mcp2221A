// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.IO.UsbHid;

class PseudoUsbHidEndPoint : IUsbHidEndPoint {
  public IUsbHidDevice Device { get; private set; }

  public bool CanWrite => WriteStream is not null;
  public bool CanRead => ReadStream is not null;

  public Stream? WriteStream { get; private set; }
  public Stream? ReadStream { get; private set; }

  public Action? OnWritingAction { get; set; }
  public Action? OnReadingAction { get; set; }

  private readonly bool shouldDisposeDevice;

  public PseudoUsbHidEndPoint(
    PseudoUsbHidDevice device,
    Stream? writeStream,
    Stream? readStream,
    bool shouldDisposeDevice
  )
  {
    Device = device ?? throw new ArgumentNullException(nameof(device));
    WriteStream = writeStream;
    ReadStream = readStream;
    this.shouldDisposeDevice = shouldDisposeDevice;
  }

  public void Dispose()
  {
    WriteStream?.Dispose();
    WriteStream = null;

    ReadStream?.Dispose();
    ReadStream = null;

    if (shouldDisposeDevice)
      Device?.Dispose();

    Device = null!;
  }

  public async ValueTask DisposeAsync()
  {
    if (WriteStream is not null) {
      await WriteStream.DisposeAsync();
      WriteStream = null;
    }

    if (ReadStream is not null) {
      await ReadStream.DisposeAsync();
      ReadStream = null;
    }

    if (shouldDisposeDevice && Device is not null)
      await Device.DisposeAsync();

    Device = null!;
  }

  public void Write(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnWritingAction?.Invoke();

    (WriteStream ?? throw new InvalidOperationException("not writable")).Write(buffer);
  }

  public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnWritingAction?.Invoke();

    return (WriteStream ?? throw new InvalidOperationException("not writable")).WriteAsync(buffer, cancellationToken);
  }

  public int Read(Span<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnReadingAction?.Invoke();

    return (ReadStream ?? throw new InvalidOperationException("not readable")).Read(buffer);
  }

  public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnReadingAction?.Invoke();

    return (ReadStream ?? throw new InvalidOperationException("not readable")).ReadAsync(buffer, cancellationToken);
  }
}
