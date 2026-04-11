// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers;

namespace System.IO;

internal static class StreamExtensions {
#if !NET7_0_OR_GREATER
  public static void ReadExactly(this Stream stream, Span<byte> destination)
  {
    if (stream is null)
      throw new ArgumentNullException(nameof(stream));

    const int BufferSize = 1024;

    var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

    try {
      for (;;) {
        if (destination.IsEmpty)
          return;

        int read = stream.Read(buffer, 0, Math.Min(destination.Length, BufferSize));

        if (read == 0)
          throw new EndOfStreamException();

        buffer.AsMemory(0, read).Span.CopyTo(destination);

        destination = destination.Slice(read);
      }
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }
#endif
}
