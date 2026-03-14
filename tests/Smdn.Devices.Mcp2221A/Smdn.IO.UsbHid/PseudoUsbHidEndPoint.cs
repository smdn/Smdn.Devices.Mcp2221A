// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Buffers;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.IO.UsbHid;

class PseudoUsbHidEndPoint : IUsbHidEndPoint {
  public IUsbHidDevice Device { get; private set; }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(true, nameof(WriteStream))]
#endif
  public bool CanWrite => WriteStream is not null;

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLWHENATTRIBUTE
  [MemberNotNullWhen(true, nameof(ReadStream))]
#endif
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
#if SYSTEM_IO_STREAM_DISPOSEASYNC
      await WriteStream.DisposeAsync();
#else
      WriteStream.Dispose();
#endif
      WriteStream = null;
    }

    if (ReadStream is not null) {
#if SYSTEM_IO_STREAM_DISPOSEASYNC
      await ReadStream.DisposeAsync();
#else
      ReadStream.Dispose();
#endif
      ReadStream = null;
    }

    if (shouldDisposeDevice && Device is not null)
      await Device.DisposeAsync();

    Device = null!;
  }

  public void Write(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnWritingAction?.Invoke();

    if (!CanWrite)
      throw new InvalidOperationException("not writable");

#if SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
    WriteStream.Write(buffer);
#else
    WriteStream!.Write(buffer.ToArray(), 0, buffer.Length);
#endif
  }

  public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnWritingAction?.Invoke();

    if (!CanWrite)
      throw new InvalidOperationException("not writable");

#if SYSTEM_IO_STREAM_WRITEASYNC_READONLYMEMORY_OF_BYTE
    return WriteStream.WriteAsync(buffer, cancellationToken);
#else
    return WriteAsyncCore(WriteStream!, buffer, cancellationToken);

    static async ValueTask WriteAsyncCore(Stream stream, ReadOnlyMemory<byte> buf, CancellationToken ct)
      => await stream.WriteAsync(buf.ToArray(), 0, buf.Length, ct).ConfigureAwait(false);
#endif
  }

  public int Read(Span<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnReadingAction?.Invoke();

    if (!CanRead)
      throw new InvalidOperationException("not readable");

#if SYSTEM_IO_STREAM_READ_SPAN_OF_BYTE
    return ReadStream.Read(buffer);
#else
    var temp = ArrayPool<byte>.Shared.Rent(buffer.Length);

    try {
      var ret = ReadStream!.Read(temp, 0, buffer.Length);

      temp.AsSpan(0, ret).CopyTo(buffer);

      return ret;
    }
    finally {
      ArrayPool<byte>.Shared.Return(temp);
    }
#endif
  }

  public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
  {
    OnReadingAction?.Invoke();

    if (!CanRead)
      throw new InvalidOperationException("not readable");

#if SYSTEM_IO_STREAM_READASYNC_MEMORY_OF_BYTE
    return ReadStream.ReadAsync(buffer, cancellationToken);
#else
    return ReadAsyncCore(ReadStream!, buffer, cancellationToken);

    static async ValueTask<int> ReadAsyncCore(Stream stream, Memory<byte> buf, CancellationToken ct)
    {
      var temp = ArrayPool<byte>.Shared.Rent(buf.Length);

      try {
        var ret = await stream.ReadAsync(temp, 0, buf.Length, ct).ConfigureAwait(false);

        temp.AsMemory(0, ret).CopyTo(buf);

        return ret;
      }
      finally {
        ArrayPool<byte>.Shared.Return(temp);
      }
    }
#endif
  }
}
