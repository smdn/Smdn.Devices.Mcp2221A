// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221ATests {
#pragma warning restore IDE0040
  [TestFixture]
  public class I2cFunctionality {
    private readonly I2cAddress address = new(0x20);

    private static byte[] ToByteArray(string hexByteSequence)
      => hexByteSequence.Split('-').Select(hex => Convert.ToByte(hex, 16)).ToArray();

    private static void AppendResponse(PseudoUsbHidEndPoint endpoint, params string[] responseSequences)
    {
      if (!endpoint.CanRead)
        throw new InvalidOperationException("endpoint does not support reading");

      var currentPosition = endpoint.ReadStream!.Position;

      foreach (var sequence in responseSequences) {
        endpoint.ReadStream.WriteByte(ReportInput);
        endpoint.ReadStream.Write(ToByteArray(sequence));
      }

      endpoint.ReadStream.Position = currentPosition;
    }

    private static async Task<(Mcp2221A, PseudoUsbHidEndPoint)> CreatePseudoDeviceWithConfiguredI2C()
    {
      var baseDevice = CreatePseudoDevice();
      var device = await Mcp2221A.CreateAsync(baseDevice, shouldDisposeUsbHidDevice: true);

      AppendResponse(
        baseDevice.EndPoint,
        "10-00-00-20-75-00-00-00-00-03-00-03-00-03-75-00-00-00-10-28-00-60-01-01-00-00-F1-19-F0-00-00-00-30-30-0B-30-10-23-13-71-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F4-02-76-02-00-00-00-00"
      );

      return (device, baseDevice.EndPoint);
    }

    private static System.Collections.IEnumerable YieldTestCases_CreateI2cDeviceAdapter()
    {
      const bool ShouldDisposeMcp2221A = true;
      const bool ShouldNotDisposeMcp2221A = false;

      yield return new object[] { I2cAddress.DeviceMinValue, ShouldDisposeMcp2221A };
      yield return new object[] { I2cAddress.DeviceMaxValue, ShouldDisposeMcp2221A };
      yield return new object[] { I2cAddress.DeviceMinValue, ShouldNotDisposeMcp2221A };
    }

    [TestCaseSource(nameof(YieldTestCases_CreateI2cDeviceAdapter))]
    public async Task CreateI2cDeviceAdapter(
      I2cAddress deviceAddress,
      bool shouldDisposeMcp2221A
    )
    {
      var (device, _) = await CreatePseudoDeviceWithConfiguredI2C();

      using var i2cDevice = device.I2c.CreateDevice(deviceAddress, shouldDisposeMcp2221A);

      Assert.That(i2cDevice, Is.Not.Null);
      Assert.That(i2cDevice.ConnectionSettings, Is.Not.Null);
      Assert.That(i2cDevice.ConnectionSettings.DeviceAddress, Is.EqualTo(deviceAddress));

      i2cDevice.Dispose();

      Assert.That(
        () => i2cDevice.WriteByte(0x00),
        Throws.TypeOf<ObjectDisposedException>()
      );
      Assert.That(
        i2cDevice.ReadByte,
        Throws.TypeOf<ObjectDisposedException>()
      );

      Assert.That(
        () => _ = device.HidDevice,
        shouldDisposeMcp2221A
          ? Throws.TypeOf<ObjectDisposedException>()
          : Throws.Nothing
      );

      Assert.That(
        i2cDevice.Dispose,
        Throws.Nothing,
        "dispose again"
      );
    }

    [Test]
    public async Task Write()
      => await TestWrite(d => { d.I2c.Write(address, 100, [0x00, 0x00, 0x00]); return Task.CompletedTask; });

    [Test]
    public async Task WriteAsync()
      => await TestWrite(async d => { await d.I2c.WriteAsync(address, 100, new byte[] { 0x00, 0x00, 0x00 }); });

    private async Task TestWrite(Func<Mcp2221A, Task> writeAction)
    {
      var (device, stream) = await CreatePseudoDeviceWithConfiguredI2C();

      AppendResponse(
        stream,
        "10-00-00-20-75-00-00-00-00-03-00-03-00-03-75-00-00-00-10-28-00-60-01-01-00-00-F1-19-F0-00-00-00-30-30-0B-30-10-23-13-71-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F4-02-76-02-00-00-00-00",
        "90-00-10-20-75-00-00-00-00-03-00-03-00-03-75-00-00-00-10-28-00-60-01-01-00-00-F1-19-F0-00-00-00-30-30-0B-30-10-23-13-71-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F4-02-76-02-00-00-00-00",
        $"10-00-00-00-00-00-00-00-00-01-00-01-00-01-75-00-{address}-00-10-28-00-60-01-01-00-00-F1-79-F0-00-00-00-30-30-0B-30-10-23-13-79-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F5-02-59-02-00-00-00-00"
      );

      Assert.That(async () => await writeAction(device), Throws.Nothing);
    }
  }
}
