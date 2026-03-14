// Smdn.Devices.MCP2221.dll (Smdn.Devices.MCP2221-0.9.4)
//   Name: Smdn.Devices.MCP2221
//   AssemblyVersion: 0.9.4.0
//   InformationalVersion: 0.9.4+bcf0f60c80f47181419bd376632a2c0be172ac98
//   TargetFramework: .NETCoreApp,Version=v10.0
//   Configuration: Release
//   Metadata: IsTrimmable=True
//   Metadata: RepositoryUrl=https://github.com/smdn/Smdn.Devices.MCP2221
//   Metadata: RepositoryBranch=main
//   Metadata: RepositoryCommit=bcf0f60c80f47181419bd376632a2c0be172ac98
//   Referenced assemblies:
//     HidSharp, Version=2.1.0.0, Culture=neutral
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     System.Collections, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ComponentModel, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Device.Gpio, Version=1.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
//     System.Linq, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Memory, Version=10.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Threading.Thread, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Smdn.Devices.MCP2221;
using Smdn.Devices.UsbHid;

namespace Smdn.Devices.MCP2221 {
  public enum I2CBusSpeed : int {
    Default = 0,
    FastMode = 2,
    LowSpeedMode = 1,
    Speed100kBitsPerSec = 0,
    Speed10kBitsPerSec = 1,
    Speed400kBitsPerSec = 2,
    StandardMode = 0,
  }

  public class CommandException : InvalidOperationException {
    public CommandException(string message) {}
    public CommandException(string message, Exception innerException) {}
  }

  public class DeviceNotFoundException : InvalidOperationException {
    public DeviceNotFoundException() {}
    public DeviceNotFoundException(string message) {}
  }

  public class I2CCommandException : CommandException {
    public I2CCommandException(I2CAddress address, string message) {}
    public I2CCommandException(I2CAddress address, string message, Exception innerException) {}
    public I2CCommandException(string message) {}
    public I2CCommandException(string message, Exception innerException) {}

    public I2CAddress Address { get; }
  }

  public class I2CNAckException : I2CCommandException {
    public I2CNAckException(I2CAddress address) {}
    public I2CNAckException(I2CAddress address, Exception innerException) {}
    public I2CNAckException(string message) {}
    public I2CNAckException(string message, Exception innerException) {}
  }

  public class I2CReadException : I2CCommandException {
    public I2CReadException(I2CAddress address, string message) {}
    public I2CReadException(I2CAddress address, string message, Exception innerException) {}
    public I2CReadException(string message) {}
    public I2CReadException(string message, Exception innerException) {}
  }

  public class MCP2221 :
    IAsyncDisposable,
    IDisposable
  {
    public sealed class GP0Functionality : GPFunctionality {
      public void ConfigureAsLEDURX(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLEDURXAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsSSPND(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsSSPNDAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP1Functionality : GPFunctionality {
      public void ConfigureAsADC(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsADCAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsClockOutput(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsInterruptDetection(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsInterruptDetectionAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsLEDUTX(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLEDUTXAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP2Functionality : GPFunctionality {
      public void ConfigureAsADC(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsADCAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsDAC(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsDACAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsUSBCFG(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsUSBCFGAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP3Functionality : GPFunctionality {
      public void ConfigureAsADC(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsADCAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsDAC(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsDACAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsLEDI2C(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLEDI2CAsync(CancellationToken cancellationToken = default) {}
    }

    public abstract class GPFunctionality {
      public string PinDesignation { get; }
      public string PinName { get; }

      public void ConfigureAsGPIO(PinMode initialDirection = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsGPIOAsync(PinMode initialDirection = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
      public PinMode GetDirection(CancellationToken cancellationToken = default) {}
      public ValueTask<PinMode> GetDirectionAsync(CancellationToken cancellationToken = default) {}
      public PinValue GetValue(CancellationToken cancellationToken = default) {}
      public ValueTask<PinValue> GetValueAsync(CancellationToken cancellationToken = default) {}
      public void SetDirection(PinMode newDirection, CancellationToken cancellationToken = default) {}
      public ValueTask SetDirectionAsync(PinMode newDirection, CancellationToken cancellationToken = default) {}
      public void SetValue(PinValue newValue, CancellationToken cancellationToken = default) {}
      public ValueTask SetValueAsync(PinValue newValue, CancellationToken cancellationToken = default) {}
    }

    public sealed class I2CFunctionality {
      public const int MaxBlockLength = 65535;

      public I2CBusSpeed BusSpeed { get; set; }

      public int Read(I2CAddress address, Span<byte> buffer, CancellationToken cancellationToken = default) {}
      public int Read(I2CAddress address, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public ValueTask<int> ReadAsync(I2CAddress address, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public async ValueTask<int> ReadAsync(I2CAddress address, Memory<byte> buffer, CancellationToken cancellationToken = default) {}
      public int ReadByte(I2CAddress address, CancellationToken cancellationToken = default) {}
      public async ValueTask<int> ReadByteAsync(I2CAddress address, CancellationToken cancellationToken = default) {}
      public (IReadOnlySet<I2CAddress> WriteAddressSet, IReadOnlySet<I2CAddress> ReadAddressSet) ScanBus(I2CAddress addressRangeMin = default, I2CAddress addressRangeMax = default, IProgress<I2CScanBusProgress> progress = null, CancellationToken cancellationToken = default) {}
      public async ValueTask<(IReadOnlySet<I2CAddress> WriteAddressSet, IReadOnlySet<I2CAddress> ReadAddressSet)> ScanBusAsync(I2CAddress addressRangeMin = default, I2CAddress addressRangeMax = default, IProgress<I2CScanBusProgress> progress = null, CancellationToken cancellationToken = default) {}
      public void Write(I2CAddress address, ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default) {}
      public void Write(I2CAddress address, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(I2CAddress address, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public async ValueTask WriteAsync(I2CAddress address, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {}
      public void WriteByte(I2CAddress address, byte @value, CancellationToken cancellationToken = default) {}
      public async ValueTask WriteByteAsync(I2CAddress address, byte @value, CancellationToken cancellationToken = default) {}
    }

    public const int DeviceProductID = 221;
    public const int DeviceVendorID = 1240;
    public const string FirmwareRevisionMCP2221 = "1.1";
    public const string FirmwareRevisionMCP2221A = "1.2";
    public const string HardwareRevisionMCP2221 = "A.6";
    public const string HardwareRevisionMCP2221A = "A.6";

    public static MCP2221 Open(Func<IUsbHidDevice> createHidDevice, IServiceProvider serviceProvider = null) {}
    public static MCP2221 Open(IServiceProvider serviceProvider = null) {}
    public static MCP2221 Open(Predicate<IUsbHidDevice> findDevicePredicate, IServiceProvider serviceProvider = null) {}
    public static ValueTask<MCP2221> OpenAsync(IServiceProvider serviceProvider = null) {}
    public static ValueTask<MCP2221> OpenAsync(Predicate<IUsbHidDevice> findDevicePredicate, IServiceProvider serviceProvider = null) {}
    public static async ValueTask<MCP2221> OpenAsync(Func<IUsbHidDevice> createHidDevice, IServiceProvider serviceProvider = null) {}

    public string ChipFactorySerialNumber { get; }
    public string FirmwareRevision { get; }
    public MCP2221.GP0Functionality GP0 { get; }
    public MCP2221.GP1Functionality GP1 { get; }
    public MCP2221.GP2Functionality GP2 { get; }
    public MCP2221.GP3Functionality GP3 { get; }
    public IReadOnlyList<MCP2221.GPFunctionality> GPs { get; }
    public string HardwareRevision { get; }
    public IUsbHidDevice HidDevice { get; }
    public MCP2221.I2CFunctionality I2C { get; }
    public string ManufacturerDescriptor { get; }
    public string ProductDescriptor { get; }
    public string SerialNumberDescriptor { get; }

    public void Dispose() {}
    public async ValueTask DisposeAsync() {}
  }

  public readonly struct I2CAddress :
    IComparable<I2CAddress>,
    IEquatable<I2CAddress>,
    IEquatable<byte>,
    IEquatable<int>
  {
    public static readonly I2CAddress DeviceMaxValue; // = "77"
    public static readonly I2CAddress DeviceMinValue; // = "08"
    public static readonly I2CAddress Zero; // = "00"

    public static bool operator == (I2CAddress x, I2CAddress y) {}
    public static explicit operator byte(I2CAddress address) {}
    public static explicit operator int(I2CAddress address) {}
    public static bool operator > (I2CAddress left, I2CAddress right) {}
    public static bool operator >= (I2CAddress left, I2CAddress right) {}
    public static implicit operator I2CAddress(byte address) {}
    public static bool operator != (I2CAddress x, I2CAddress y) {}
    public static bool operator < (I2CAddress left, I2CAddress right) {}
    public static bool operator <= (I2CAddress left, I2CAddress right) {}

    public I2CAddress(int address) {}
    public I2CAddress(int deviceAddressBits, int hardwareAddressBits) {}

    public int CompareTo(I2CAddress other) {}
    public bool Equals(I2CAddress other) {}
    public bool Equals(byte other) {}
    public bool Equals(int other) {}
    public override bool Equals(object obj) {}
    public override int GetHashCode() {}
    public override string ToString() {}
  }

  public readonly struct I2CScanBusProgress {
    public I2CAddress AddressRangeMax { get; }
    public I2CAddress AddressRangeMin { get; }
    public int ProgressInPercent { get; }
    public I2CAddress ScanningAddress { get; }
  }
}

namespace Smdn.Devices.UsbHid {
  public interface IUsbHidDevice :
    IAsyncDisposable,
    IDisposable
  {
    string DevicePath { get; }
    string FileSystemName { get; }
    string Manufacturer { get; }
    int ProductID { get; }
    string ProductName { get; }
    Version ReleaseNumber { get; }
    string SerialNumber { get; }
    int VendorID { get; }

    IUsbHidStream OpenStream();
    ValueTask<IUsbHidStream> OpenStreamAsync();
  }

  public interface IUsbHidStream :
    IAsyncDisposable,
    IDisposable
  {
    bool RequiresPacketOnly { get; }

    int Read(Span<byte> buffer);
    ValueTask<int> ReadAsync(Memory<byte> buffer);
    void Write(ReadOnlySpan<byte> buffer);
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer);
  }

  public static class Log {
    public static LogLevel NativeLibraryLogLevel { get; set; }
  }

  public class UsbHidException : InvalidOperationException {
    public UsbHidException() {}
    public UsbHidException(string message) {}
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.8.1.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.6.1.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
