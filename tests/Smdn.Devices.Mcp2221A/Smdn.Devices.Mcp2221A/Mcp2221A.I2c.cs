// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221ATests {
#pragma warning restore IDE0040
  private static void AppendResponse(PseudoUsbHidEndPoint endPoint, params string[] responseSequences)
  {
    static byte[] ToByteArray(string hexByteSequence)
      => hexByteSequence.Split('-').Select(hex => Convert.ToByte(hex, 16)).ToArray();

    if (!endPoint.CanRead)
      throw new InvalidOperationException("endpoint does not support reading");

    var currentPosition = endPoint.ReadStream!.Position;

    foreach (var sequence in responseSequences) {
      endPoint.ReadStream.WriteByte(ReportInput);

      var sequenceBytes = ToByteArray(sequence);

      endPoint.ReadStream.Write(
#if SYSTEM_IO_STREAM_WRITE_READONLYSPAN_OF_BYTE
        sequenceBytes
#else
        sequenceBytes,
        0,
        sequenceBytes.Length
#endif
      );
    }

    endPoint.ReadStream.Position = currentPosition;
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
    await using var mcp2221A = await Mcp2221A.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    using var i2cDevice = mcp2221A.I2c.CreateDevice(deviceAddress, shouldDisposeMcp2221A);

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
      () => _ = mcp2221A.HidDevice,
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
  public ValueTask Write()
    => WriteSyncAndAsync(
      static (d, address, ct) => {
        d.I2c.Write(address, 100, [0x00, 0x00, 0x00], ct);
        return default;
      }
    );

  [Test]
  public ValueTask WriteAsync()
    => WriteSyncAndAsync(
      static (d, address, ct) => d.I2c.WriteAsync(address, 100, new byte[] { 0x00, 0x00, 0x00 }, ct)
    );

  private async ValueTask WriteSyncAndAsync(Func<Mcp2221A, I2cAddress, CancellationToken, ValueTask> writeAsyncAction)
  {
    await using var mcp2221A = await Mcp2221A.CreateAsync(
      CreatePseudoDevice(),
      shouldDisposeUsbHidDevice: true
    );
    var endPoint = (mcp2221A.HidDevice as PseudoUsbHidDevice)!.EndPoint;
    var address = new I2cAddress(0x20);

    AppendResponse(
      endPoint,
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      // [1] 0x00: Command completed successfully
      // [3] 0x20: The new I2C/SMBus communication speed is now considered
      "10-00-00-20-75-00-00-00-00-03-00-03-00-03-75-00-00-00-10-28-00-60-01-01-00-00-F1-19-F0-00-00-00-30-30-0B-30-10-23-13-71-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F4-02-76-02-00-00-00-00",
      // [MCP2221A] 3.1.5 I2C WRITE DATA
      // [1] 0x00: Command completed successfully
      "90-00-10-20-75-00-00-00-00-03-00-03-00-03-75-00-00-00-10-28-00-60-01-01-00-00-F1-19-F0-00-00-00-30-30-0B-30-10-23-13-71-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F4-02-76-02-00-00-00-00",
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      // [1] 0x00: Command completed successfully
      // [3] No Set I2C/SMBus communication speed was issued
      // [16] Lower byte of the I2C address being used
      $"10-00-00-00-00-00-00-00-00-01-00-01-00-01-75-00-{address}-00-10-28-00-60-01-01-00-00-F1-79-F0-00-00-00-30-30-0B-30-10-23-13-79-05-00-00-26-94-14-41-36-31-32-FB-03-00-00-00-00-F5-02-59-02-00-00-00-00"
    );

    Assert.That(
      async () => await writeAsyncAction(mcp2221A, address, default).ConfigureAwait(false),
      Throws.Nothing
    );

    using var cts = new CancellationTokenSource();

    cts.Cancel();

    Assert.That(
      async () => await writeAsyncAction(mcp2221A, address, cts.Token).ConfigureAwait(false),
      Throws
        .InstanceOf<OperationCanceledException>()
        .With
        .Property(nameof(OperationCanceledException.CancellationToken))
        .EqualTo(cts.Token)
    );
  }
}
