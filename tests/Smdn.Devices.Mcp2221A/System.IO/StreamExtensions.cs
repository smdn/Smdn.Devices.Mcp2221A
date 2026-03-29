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
      int readTotal = 0;

      while (readTotal < destination.Length) {
        int n = stream.Read(buffer, 0, BufferSize);

        if (n == 0)
          throw new EndOfStreamException();

        buffer.AsMemory(0, n).Span.CopyTo(destination.Slice(readTotal));

        readTotal += n;
      }
    }
    finally {
      ArrayPool<byte>.Shared.Return(buffer);
    }
  }
#endif
}
