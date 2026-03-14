// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040, CA1724
partial class Mcp2221A {
#pragma warning restore IDE0040, CA1724
#if __FUTURE_VERSION
  public static ValueTask<Mcp2221A> CreateAsync(
    CancellationToken cancellationToken = default
  )
    // future: create with implementation using linux kernel module
    => throw new NotImplementedException();

  // future: create with implementation using linux kernel module
  public static Mcp2221A Create(
    CancellationToken cancellationToken = default
  )
    // future: create with implementation using linux kernel module
    => throw new NotImplementedException();
#endif
}
