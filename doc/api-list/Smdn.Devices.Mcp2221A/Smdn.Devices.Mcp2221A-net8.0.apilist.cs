// Smdn.Devices.Mcp2221A.dll (Smdn.Devices.Mcp2221A-1.0.0-preview1)
//   Name: Smdn.Devices.Mcp2221A
//   AssemblyVersion: 1.0.0.0
//   InformationalVersion: 1.0.0-preview1+077854654720e368ee674194833ee52d976ac129
//   TargetFramework: .NETCoreApp,Version=v8.0
//   Configuration: Release
//   Metadata: IsTrimmable=True
//   Metadata: RepositoryUrl=https://github.com/smdn/Smdn.Devices.Mcp2221A
//   Metadata: RepositoryBranch=main
//   Metadata: RepositoryCommit=077854654720e368ee674194833ee52d976ac129
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.IO.UsbHid.Abstractions, Version=1.0.0.0, Culture=neutral
//     System.Collections, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ComponentModel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Device.Gpio, Version=1.4.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
//     System.Linq, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Memory, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A {
  public interface IMcp2221AInfo {
    string ChipFactorySerialNumber { get; }
    string FirmwareRevision { get; }
    string HardwareRevision { get; }
    string Manufacturer { get; }
    string Product { get; }
    string SerialNumber { get; }
  }

  public static class IMcp2221AInfoExtensions {
    extension(IMcp2221AInfo info) {
      public bool IsMcp2221A { get; }
    }
  }

  public class Mcp2221A :
    IAsyncDisposable,
    IDisposable,
    IMcp2221AInfo
  {
    public sealed class GP0Functionality : GPFunctionality {
      public void ConfigureAsLedUrx(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLedUrxAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsSspnd(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsSspndAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP1Functionality : GPFunctionality {
      public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsClockOutput(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsInterruptDetection(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsInterruptDetectionAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsLedUtx(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLedUtxAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP2Functionality : GPFunctionality {
      public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsDac(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsUsbCfg(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsUsbCfgAsync(CancellationToken cancellationToken = default) {}
    }

    public sealed class GP3Functionality : GPFunctionality {
      public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsDac(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsLedI2c(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsLedI2cAsync(CancellationToken cancellationToken = default) {}
    }

    public abstract class GPFunctionality {
      public string? PinDesignation { get; }
      public string PinName { get; }

      public void ConfigureAsGpio(PinMode initialDirection = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsGpioAsync(PinMode initialDirection = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
      public PinMode GetDirection(CancellationToken cancellationToken = default) {}
      public ValueTask<PinMode> GetDirectionAsync(CancellationToken cancellationToken = default) {}
      public PinValue GetValue(CancellationToken cancellationToken = default) {}
      public ValueTask<PinValue> GetValueAsync(CancellationToken cancellationToken = default) {}
      public void SetDirection(PinMode newDirection, CancellationToken cancellationToken = default) {}
      public ValueTask SetDirectionAsync(PinMode newDirection, CancellationToken cancellationToken = default) {}
      public void SetValue(PinValue newValue, CancellationToken cancellationToken = default) {}
      public ValueTask SetValueAsync(PinValue newValue, CancellationToken cancellationToken = default) {}
    }

    public const int DefaultProductId = 221;
    public const int DefaultVendorId = 1240;
    public const string FirmwareRevisionMcp2221 = "1.1";
    public const string FirmwareRevisionMcp2221A = "1.2";
    public const string HardwareRevisionMcp2221 = "A.6";
    public const string HardwareRevisionMcp2221A = "A.6";

    public static Mcp2221A Create(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) {}
    public static Mcp2221A Create(IServiceProvider serviceProvider, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static Mcp2221A Create(IUsbHidDevice usbHidDevice, bool shouldDisposeUsbHidDevice = false, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static Mcp2221A Create<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, CancellationToken cancellationToken = default) {}
    public static Mcp2221A Create<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static Mcp2221A Create<TServiceKey>(IUsbHidDevice usbHidDevice, IServiceProvider? serviceProvider, TServiceKey serviceKey, bool shouldDisposeUsbHidDevice = false, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync(IServiceProvider serviceProvider, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync(IUsbHidDevice usbHidDevice, bool shouldDisposeUsbHidDevice = false, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221A> CreateAsync<TServiceKey>(IUsbHidDevice usbHidDevice, IServiceProvider? serviceProvider, TServiceKey serviceKey, bool shouldDisposeUsbHidDevice = false, CancellationToken cancellationToken = default) {}
    [Obsolete("Use Create with IUsbHidDevice instead.", true)]
    public static Mcp2221A Open(Func<IUsbHidDevice?> createHidDevice, IServiceProvider? serviceProvider = null) {}
    [Obsolete("Use CreateAsync with IUsbHidDevice instead.", true)]
    public static async ValueTask<Mcp2221A> OpenAsync(Func<IUsbHidDevice?> createHidDevice, IServiceProvider? serviceProvider = null) {}
    public static bool TryCalculateMcp2221AI2cSpeedDivider(int i2cBusSpeedInKbps, out byte i2cSpeedDivider) {}
    public static bool TryCalculateMcp2221I2cSpeedDivider(int i2cBusSpeedInKbps, out byte i2cSpeedDivider) {}

    public string ChipFactorySerialNumber { get; }
    public string FirmwareRevision { get; }
    public Mcp2221A.GP0Functionality GP0 { get; }
    public Mcp2221A.GP1Functionality GP1 { get; }
    public Mcp2221A.GP2Functionality GP2 { get; }
    public Mcp2221A.GP3Functionality GP3 { get; }
    public IReadOnlyList<Mcp2221A.GPFunctionality> GPs { get; }
    public string HardwareRevision { get; }
    public IUsbHidDevice HidDevice { get; }
    public Mcp2221AI2cBus I2c { get; }
    public string Manufacturer { get; }
    public string Product { get; }
    public string SerialNumber { get; }

    protected virtual void Dispose(bool disposing) {}
    public void Dispose() {}
    public async ValueTask DisposeAsync() {}
    protected virtual async ValueTask DisposeAsyncCore() {}
    private TResponse Smdn.Devices.Mcp2221A.IMcp2221ATransceiver.Command<TArg, TResponse>(ReadOnlySpan<byte> userData, TArg arg, Mcp2221AConstructCommandAction<TArg> constructCommand, Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse, CancellationToken cancellationToken) {}
    private ValueTask<TResponse> Smdn.Devices.Mcp2221A.IMcp2221ATransceiver.CommandAsync<TArg, TResponse>(ReadOnlyMemory<byte> userData, TArg arg, Mcp2221AConstructCommandAction<TArg> constructCommand, Mcp2221AParseResponseFunc<TArg, TResponse> parseResponse, CancellationToken cancellationToken) {}
  }

  public class Mcp2221ACommandException : InvalidOperationException {
    public Mcp2221ACommandException() {}
    public Mcp2221ACommandException(string? message) {}
    public Mcp2221ACommandException(string? message, Exception? innerException) {}
  }

  public sealed class Mcp2221AInfo : IMcp2221AInfo {
    public string ChipFactorySerialNumber { get; init; }
    public string FirmwareRevision { get; init; }
    public string HardwareRevision { get; init; }
    public string Manufacturer { get; init; }
    public string Product { get; init; }
    public string SerialNumber { get; init; }
  }

  public class Mcp2221ANotFoundException : InvalidOperationException {
    public Mcp2221ANotFoundException() {}
    public Mcp2221ANotFoundException(string? message) {}
    public Mcp2221ANotFoundException(string? message, Exception? innerException) {}
  }

  public class Mcp2221ANotSupportedException : NotSupportedException {
    public Mcp2221ANotSupportedException() {}
    public Mcp2221ANotSupportedException(string? message) {}
    public Mcp2221ANotSupportedException(string? message, Exception? innerException) {}
  }

  public class Mcp2221AUnavailableException : UnauthorizedAccessException {
    public Mcp2221AUnavailableException() {}
    public Mcp2221AUnavailableException(Exception innerException, IUsbHidDevice? device = null) {}
    public Mcp2221AUnavailableException(string? message) {}
    public Mcp2221AUnavailableException(string? message, Exception? innerException) {}
  }

  public readonly struct I2cAddress :
    IComparable<I2cAddress>,
    IEquatable<I2cAddress>,
    IEquatable<byte>,
    IEquatable<int>
  {
    public static readonly I2cAddress DeviceMaxValue; // = "77"
    public static readonly I2cAddress DeviceMinValue; // = "08"
    public static readonly I2cAddress Zero; // = "00"

    public static I2cAddress FromByte(byte address) {}
    public static bool operator == (I2cAddress x, I2cAddress y) {}
    public static explicit operator byte(I2cAddress address) {}
    public static explicit operator int(I2cAddress address) {}
    public static bool operator > (I2cAddress left, I2cAddress right) {}
    public static bool operator >= (I2cAddress left, I2cAddress right) {}
    public static implicit operator I2cAddress(byte address) {}
    public static bool operator != (I2cAddress x, I2cAddress y) {}
    public static bool operator < (I2cAddress left, I2cAddress right) {}
    public static bool operator <= (I2cAddress left, I2cAddress right) {}

    public I2cAddress(int address) {}
    public I2cAddress(int deviceAddressBits, int hardwareAddressBits) {}

    public int CompareTo(I2cAddress other) {}
    public bool Equals(I2cAddress other) {}
    public bool Equals(byte other) {}
    public bool Equals(int other) {}
    public override bool Equals(object? obj) {}
    public override int GetHashCode() {}
    public byte ToByte() {}
    public int ToInt32() {}
    public override string ToString() {}
  }
}

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c {
  public interface II2cController {
    void CancelTransfer(I2cAddress address);
    ValueTask CancelTransferAsync(I2cAddress address);
    int Read(I2cAddress address, int transmissionSpeedInKbps, Span<byte> buffer, CancellationToken cancellationToken);
    ValueTask<int> ReadAsync(I2cAddress address, int transmissionSpeedInKbps, Memory<byte> buffer, CancellationToken cancellationToken);
    void Write(I2cAddress address, int transmissionSpeedInKbps, ReadOnlySpan<byte> buffer, CancellationToken cancellationToken);
    ValueTask WriteAsync(I2cAddress address, int transmissionSpeedInKbps, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken);
  }

  public interface II2cDevice {
    I2cAddress Address { get; }
    II2cController Controller { get; }
    int TransmissionSpeedInKbps { get; set; }
  }

  public class I2cCommandException : Mcp2221ACommandException {
    public I2cCommandException() {}
    public I2cCommandException(I2cAddress address, string? message) {}
    public I2cCommandException(I2cAddress address, string? message, Exception? innerException) {}
    public I2cCommandException(string? message) {}
    public I2cCommandException(string? message, Exception? innerException) {}

    public I2cAddress Address { get; }
  }

  public class I2cNackException : I2cCommandException {
    public I2cNackException() {}
    public I2cNackException(I2cAddress address) {}
    public I2cNackException(I2cAddress address, Exception? innerException) {}
    public I2cNackException(string? message) {}
    public I2cNackException(string? message, Exception? innerException) {}
  }

  public class I2cReadException : I2cCommandException {
    public I2cReadException() {}
    public I2cReadException(I2cAddress address, string? message) {}
    public I2cReadException(I2cAddress address, string? message, Exception? innerException) {}
    public I2cReadException(string? message) {}
    public I2cReadException(string? message, Exception? innerException) {}
  }

  public static class II2cControllerExtensions {
    public static int Read(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
    public static ValueTask<int> ReadAsync(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
    public static int ReadByte(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, CancellationToken cancellationToken = default) {}
    public static async ValueTask<int> ReadByteAsync(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, CancellationToken cancellationToken = default) {}
    public static (IReadOnlySet<I2cAddress> WriteAddressSet, IReadOnlySet<I2cAddress> ReadAddressSet) ScanBus(this II2cController controller, I2cAddress addressRangeMin = default, I2cAddress addressRangeMax = default, int i2cBusTransmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
    public static async ValueTask<(IReadOnlySet<I2cAddress> WriteAddressSet, IReadOnlySet<I2cAddress> ReadAddressSet)> ScanBusAsync(this II2cController controller, I2cAddress addressRangeMin = default, I2cAddress addressRangeMax = default, int i2cBusTransmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
    public static void Write(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
    public static ValueTask WriteAsync(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
    public static void WriteByte(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte @value, CancellationToken cancellationToken = default) {}
    public static async ValueTask WriteByteAsync(this II2cController controller, I2cAddress address, int transmissionSpeedInKbps, byte @value, CancellationToken cancellationToken = default) {}
  }

  public static class II2cDeviceExtensions {
    public static void Read(this II2cDevice device, Span<byte> buffer, CancellationToken cancellationToken = default) {}
    public static ValueTask<int> ReadAsync(this II2cDevice device, Memory<byte> buffer, CancellationToken cancellationToken = default) {}
    public static int ReadByte(this II2cDevice device, CancellationToken cancellationToken = default) {}
    public static ValueTask<int> ReadByteAsync(this II2cDevice device, CancellationToken cancellationToken = default) {}
    public static void Write(this II2cDevice device, ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default) {}
    public static ValueTask WriteAsync(this II2cDevice device, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {}
    public static void WriteByte(this II2cDevice device, byte @value, CancellationToken cancellationToken = default) {}
    public static ValueTask WriteByteAsync(this II2cDevice device, byte @value, CancellationToken cancellationToken = default) {}
  }

  public sealed class Mcp2221AI2cBus :
    I2cBus,
    II2cController
  {
    public const int MaxBlockLength = 65535;

    public Mcp2221AI2cDevice CreateDevice(I2cAddress deviceAddress, bool shouldDisposeMcp2221A = false) {}
    public Mcp2221AI2cDevice CreateDevice(I2cAddress deviceAddress, int transmissionSpeedInKbps, bool shouldDisposeMcp2221A = false) {}
    [PreserveBaseOverrides]
    public virtual Mcp2221AI2cDevice CreateDevice(int deviceAddress) {}
    public int Read(I2cAddress address, int transmissionSpeedInKbps, Span<byte> buffer, CancellationToken cancellationToken = default) {}
    public async ValueTask<int> ReadAsync(I2cAddress address, int transmissionSpeedInKbps, Memory<byte> buffer, CancellationToken cancellationToken = default) {}
    public override void RemoveDevice(int deviceAddress) {}
    public void RemoveDevice(I2cAddress deviceAddress) {}
    void II2cController.CancelTransfer(I2cAddress address) {}
    ValueTask II2cController.CancelTransferAsync(I2cAddress address) {}
    public void Write(I2cAddress address, int transmissionSpeedInKbps, ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default) {}
    public async ValueTask WriteAsync(I2cAddress address, int transmissionSpeedInKbps, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {}
  }

  public sealed class Mcp2221AI2cDevice :
    I2cDevice,
    II2cDevice
  {
    public override I2cConnectionSettings ConnectionSettings { get; }
    I2cAddress II2cDevice.Address { get; }
    II2cController II2cDevice.Controller { get; }
    public int TransmissionSpeedInKbps { get; set; }

    protected override void Dispose(bool disposing) {}
    public override void Read(Span<byte> buffer) {}
    public override byte ReadByte() {}
    public Mcp2221AI2cDevice WithFastMode() {}
    public Mcp2221AI2cDevice WithStandardMode() {}
    public override void Write(ReadOnlySpan<byte> buffer) {}
    public override void WriteByte(byte @value) {}
    public override void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer) {}
  }

  public readonly struct I2cScanBusProgress {
    public I2cAddress AddressRangeMax { get; }
    public I2cAddress AddressRangeMin { get; }
    public int ProgressInPercent { get; }
    public I2cAddress ScanningAddress { get; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.8.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.6.2.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
