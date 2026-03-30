// Smdn.Devices.Mcp2221A.dll (Smdn.Devices.Mcp2221A-1.0.0-preview2)
//   Name: Smdn.Devices.Mcp2221A
//   AssemblyVersion: 1.0.0.0
//   InformationalVersion: 1.0.0-preview2+9335bdfc1b937075a51ac35af5bd1768fa3a1654
//   TargetFramework: .NETStandard,Version=v2.1
//   Configuration: Release
//   Metadata: RepositoryUrl=https://github.com/smdn/Smdn.Devices.Mcp2221A
//   Metadata: RepositoryBranch=main
//   Metadata: RepositoryCommit=9335bdfc1b937075a51ac35af5bd1768fa3a1654
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=5.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.IO.UsbHid.Abstractions, Version=1.0.0.0, Culture=neutral
//     System.Device.Gpio, Version=1.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
//     netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;
using System.Threading.Tasks;
using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
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

  public static class IGpControllerGroupExtensions {
    extension(IGpControllerGroup gpPins) {
      public void ConfigureAllAsGpio(PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAllAsGpioAsync(PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default) {}
      public void ConfigureAllAsGpioInput(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAllAsGpioInputAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAllAsGpioOutput(PinValue? gp0InitialValue = null, PinValue? gp1InitialValue = null, PinValue? gp2InitialValue = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAllAsGpioOutputAsync(PinValue? gp0InitialValue = null, PinValue? gp1InitialValue = null, PinValue? gp2InitialValue = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default) {}
      public void ConfigureAllGpFunctions(GpFunction? gp0Function = null, GpFunction? gp1Function = null, GpFunction? gp2Function = null, GpFunction? gp3Function = null, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAllGpFunctionsAsync(GpFunction? gp0Function = null, GpFunction? gp1Function = null, GpFunction? gp2Function = null, GpFunction? gp3Function = null, CancellationToken cancellationToken = default) {}
      public (PinValue Gp0Value, PinValue Gp1Value, PinValue Gp2Value, PinValue Gp3Value) Read(CancellationToken cancellationToken = default) {}
      public void Read(Span<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
      public ValueTask ReadAsync(Memory<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
      public ValueTask<(PinValue Gp0Value, PinValue Gp1Value, PinValue Gp2Value, PinValue Gp3Value)> ReadAsync(CancellationToken cancellationToken = default) {}
      public void Write(PinValue? gp0Value = null, PinValue? gp1Value = null, PinValue? gp2Value = null, PinValue? gp3Value = null, CancellationToken cancellationToken = default) {}
      public void Write(ReadOnlySpan<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(PinValue? gp0Value = null, PinValue? gp1Value = null, PinValue? gp2Value = null, PinValue? gp3Value = null, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(ReadOnlyMemory<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
    }
  }

  public static class IGpioControllerExtensions {
    public static void ConfigureAsGpioInput(this IGpioController controller, CancellationToken cancellationToken = default) {}
    public static ValueTask ConfigureAsGpioInputAsync(this IGpioController controller, CancellationToken cancellationToken = default) {}
    public static void ConfigureAsGpioOutput(this IGpioController controller, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
    public static ValueTask ConfigureAsGpioOutputAsync(this IGpioController controller, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
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

    public string ChipFactorySerialNumber { get; }
    public string FirmwareRevision { get; }
    public Gp0Controller GpPin0 { get; }
    public Gp1Controller GpPin1 { get; }
    public Gp2Controller GpPin2 { get; }
    public Gp3Controller GpPin3 { get; }
    public IGpControllerGroup GpPins { get; }
    public GpioController GpioController { get; }
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

  public readonly record struct PinModePair {
    public PinModePair(int PinNumber, PinMode PinMode) {}

    public PinMode PinMode { get; init; }
    public int PinNumber { get; init; }

    [CompilerGenerated]
    public void Deconstruct(out int PinNumber, out PinMode PinMode) {}
    [CompilerGenerated]
    public override string ToString() {}
  }
}

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio {
  public interface IGpControllerGroup : IReadOnlyList<GpController> {
    Gp0Controller Gp0 { get; }
    Gp1Controller Gp1 { get; }
    Gp2Controller Gp2 { get; }
    Gp3Controller Gp3 { get; }

    void ApplyGpioStates(ReadOnlySpan<PinValuePair> pinValuePairs, ReadOnlySpan<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    ValueTask ApplyGpioStatesAsync(ReadOnlyMemory<PinValuePair> pinValuePairs, ReadOnlyMemory<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    void ConfigureAllGpSettings(GpFunction? gp0Function = null, PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, GpFunction? gp1Function = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, GpFunction? gp2Function = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, GpFunction? gp3Function = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default);
    ValueTask ConfigureAllGpSettingsAsync(GpFunction? gp0Function = null, PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, GpFunction? gp1Function = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, GpFunction? gp2Function = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, GpFunction? gp3Function = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default);
    void FetchGpioStates(Span<PinValuePair> pinValuePairs, Span<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    ValueTask FetchGpioStatesAsync(Memory<PinValuePair> pinValuePairs, Memory<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
  }

  public interface IGpioController {
    void ConfigureAsGpio(PinMode mode, PinValue initialValue, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsGpioAsync(PinMode mode, PinValue initialValue, CancellationToken cancellationToken = default);
    PinMode GetMode(CancellationToken cancellationToken = default);
    ValueTask<PinMode> GetModeAsync(CancellationToken cancellationToken = default);
    PinValue Read(CancellationToken cancellationToken = default);
    ValueTask<PinValue> ReadAsync(CancellationToken cancellationToken = default);
    void SetMode(PinMode mode, CancellationToken cancellationToken = default);
    ValueTask SetModeAsync(PinMode mode, CancellationToken cancellationToken = default);
    void Write(PinValue @value, CancellationToken cancellationToken = default);
    ValueTask WriteAsync(PinValue @value, CancellationToken cancellationToken = default);
  }

  public enum GpFunction : int {
    Adc = 1,
    ClockOutput = 5,
    Dac = 2,
    ExternalInterrupt = 3,
    Gpio = 0,
    LedOutput = 4,
    UsbConfigureStatus = 7,
    UsbSuspendStatus = 6,
  }

  public sealed class Gp0Controller : GpController {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public override string PinName { get; }

    public void ConfigureAsUrxLedOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUrxLedOutputAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsUsbSuspendStatus(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUsbSuspendStatusAsync(CancellationToken cancellationToken = default) {}
  }

  public sealed class Gp1Controller : GpController {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public override string PinName { get; }

    public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsClockOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsClockOutputAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsExternalInterrupt(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsExternalInterruptAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsUtxLedOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUtxLedOutputAsync(CancellationToken cancellationToken = default) {}
  }

  public sealed class Gp2Controller : GpController {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public override string PinName { get; }

    public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsDac(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsUsbConfigureStatus(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUsbConfigureStatusAsync(CancellationToken cancellationToken = default) {}
  }

  public sealed class Gp3Controller : GpController {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public override string PinName { get; }

    public void ConfigureAsAdc(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsDac(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsDacAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsI2cLedOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsI2cLedOutputAsync(CancellationToken cancellationToken = default) {}
  }

  public abstract class GpController : IGpioController {
    public abstract string CurrentDesignation { get; }
    public abstract GpFunction CurrentFunction { get; }
    public PinMode CurrentMode { get; }
    public abstract int Index { get; }
    public bool IsUsedByGpioController { get; }
    public PinValue LastUpdatedValue { get; }
    public abstract string PinName { get; }

    public void ConfigureAsGpio(PinMode mode = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsGpioAsync(PinMode mode = PinMode.Output, PinValue initialValue = default, CancellationToken cancellationToken = default) {}
    public PinMode GetMode(CancellationToken cancellationToken = default) {}
    public async ValueTask<PinMode> GetModeAsync(CancellationToken cancellationToken = default) {}
    public bool IsFunctionSupported(GpFunction function) {}
    public PinValue Read(CancellationToken cancellationToken = default) {}
    public async ValueTask<PinValue> ReadAsync(CancellationToken cancellationToken = default) {}
    public void SetMode(PinMode mode, CancellationToken cancellationToken = default) {}
    public async ValueTask SetModeAsync(PinMode mode, CancellationToken cancellationToken = default) {}
    protected void ThrowIfInvalidConfiguration(GpFunction requiredFunction) {}
    public void Write(PinValue @value, CancellationToken cancellationToken = default) {}
    public async ValueTask WriteAsync(PinValue @value, CancellationToken cancellationToken = default) {}
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
    public static (IReadOnlyCollection<I2cAddress> WriteAddressSet, IReadOnlyCollection<I2cAddress> ReadAddressSet) ScanBus(this II2cController controller, I2cAddress addressRangeMin = default, I2cAddress addressRangeMax = default, int i2cBusTransmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
    public static async ValueTask<(IReadOnlyCollection<I2cAddress> WriteAddressSet, IReadOnlyCollection<I2cAddress> ReadAddressSet)> ScanBusAsync(this II2cController controller, I2cAddress addressRangeMin = default, I2cAddress addressRangeMax = default, int i2cBusTransmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
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
    public override I2cDevice CreateDevice(int deviceAddress) {}
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
