// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading.Tasks;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class IMcp2221AInfoExtensionsTests {
  [TestCase('1', '1', false)] // 1.1 (MCP2221)
  [TestCase('1', '2', true)] // 1.2 (MCP2221A)
  public async Task IsMcp2221A(char firmwareRevisionMajor, char firmwareRevisionMinor, bool expected)
  {
    var baseDevice = Mcp2221AControllerTests.CreatePseudoDevice(
      firmwareRevisionMajor: (byte)firmwareRevisionMajor,
      firmwareRevisionMinor: (byte)firmwareRevisionMinor
    );
    using var device = await Mcp2221AController.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: true);

    Assert.That(device.IsMcp2221A, Is.EqualTo(expected));
  }
}