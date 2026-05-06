// Smdn.Devices.Mcp2221A.dll (Smdn.Devices.Mcp2221A-1.0.0)
//   Name: Smdn.Devices.Mcp2221A
//   AssemblyVersion: 1.0.0.0
//   InformationalVersion: 1.0.0+0bf9c954cff7b4ca33c418cbc5e863d9862124f3
//   TargetFramework: .NETCoreApp,Version=v8.0
//   Configuration: Release
//   Metadata: IsTrimmable=True
//   Metadata: RepositoryUrl=https://github.com/smdn/Smdn.Devices.Mcp2221A
//   Metadata: RepositoryBranch=main
//   Metadata: RepositoryCommit=0bf9c954cff7b4ca33c418cbc5e863d9862124f3
//   Referenced assemblies:
//     Microsoft.Extensions.DependencyInjection.Abstractions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Microsoft.Extensions.Logging.Abstractions, Version=6.0.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//     Smdn.IO.UsbHid.Abstractions, Version=1.0.0.0, Culture=neutral
//     System.Collections, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ComponentModel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.ComponentModel.Primitives, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Device.Gpio, Version=4.1.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35
//     System.Linq, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Memory, Version=8.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51
//     System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//     System.Threading, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
#nullable enable annotations

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Numerics;
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

  public enum ClockOutputDutyCycle : int {
    Duty0 = 0,
    Duty25 = 1,
    Duty50 = 2,
    Duty75 = 3,
  }

  public enum ClockOutputFrequency : int {
    Frequency12MHz = 2,
    Frequency1500kHz = 5,
    Frequency24MHz = 1,
    Frequency375kHz = 7,
    Frequency3MHz = 4,
    Frequency6MHz = 3,
    Frequency750kHz = 6,
    Reserved = 0,
  }

  public enum DeviceConfigurationProtectionLevel : int {
    None = 0,
    PasswordProtected = 1,
    PermanentlyLocked = 2,
    Reserved = 3,
  }

  public enum GpFunction : int {
    Adc = 1,
    ClockOutput = 5,
    Dac = 2,
    Gpio = 0,
    InterruptOnChange = 3,
    LedOutput = 4,
    UsbConfigureStatus = 7,
    UsbSuspendStatus = 6,
  }

  public enum InterruptOnChangeTrigger : int {
    Both = 3,
    Falling = 2,
    None = 0,
    Rising = 1,
  }

  public enum UsbPowerMode : int {
    BusPowered = 0,
    SelfPowered = 1,
  }

  public enum VoltageReferenceSource : int {
    Vdd = 0,
    Vrm1024 = 3,
    Vrm2048 = 5,
    Vrm4096 = 7,
    VrmOff = 1,
  }

  public static class IClockOutputControllerExtensions {
    extension(IClockOutputController controller) {
      public int CurrentClockOutputDutyCycleInPercent { get; }
      public double CurrentClockOutputDutyRatio { get; }
      public int CurrentClockOutputFrequencyInHz { get; }

      public void ResumeClockOutput(CancellationToken cancellationToken = default) {}
      public ValueTask ResumeClockOutputAsync(CancellationToken cancellationToken = default) {}
    }
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
      public (int Gp1Value, int Gp2Value, int Gp3Value) ReadAnalogRaw(CancellationToken cancellationToken = default) {}
      public ValueTask<(int Gp1Value, int Gp2Value, int Gp3Value)> ReadAnalogRawAsync(CancellationToken cancellationToken = default) {}
      public (double Gp1Voltage, double Gp2Voltage, double Gp3Voltage) ReadAnalogVoltage(CancellationToken cancellationToken = default) {}
      public (double Gp1Voltage, double Gp2Voltage, double Gp3Voltage) ReadAnalogVoltage(double referenceVoltage, CancellationToken cancellationToken = default) {}
      public ValueTask<(double Gp1Voltage, double Gp2Voltage, double Gp3Voltage)> ReadAnalogVoltageAsync(CancellationToken cancellationToken = default) {}
      public ValueTask<(double Gp1Voltage, double Gp2Voltage, double Gp3Voltage)> ReadAnalogVoltageAsync(double referenceVoltage, CancellationToken cancellationToken = default) {}
      public ValueTask ReadAsync(Memory<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
      public ValueTask<(PinValue Gp0Value, PinValue Gp1Value, PinValue Gp2Value, PinValue Gp3Value)> ReadAsync(CancellationToken cancellationToken = default) {}
      public void Write(PinValue? gp0Value = null, PinValue? gp1Value = null, PinValue? gp2Value = null, PinValue? gp3Value = null, CancellationToken cancellationToken = default) {}
      public void Write(ReadOnlySpan<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
      public void WriteAnalogVoltage(double voltage, CancellationToken cancellationToken = default) {}
      public void WriteAnalogVoltage(double voltage, double referenceVoltage, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAnalogVoltageAsync(double voltage, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAnalogVoltageAsync(double voltage, double referenceVoltage, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(PinValue? gp0Value = null, PinValue? gp1Value = null, PinValue? gp2Value = null, PinValue? gp3Value = null, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(ReadOnlyMemory<PinValuePair> pinValuePairs, CancellationToken cancellationToken = default) {}
    }
  }

  public static class IGpioControllerExtensions {
    extension(IGpioController controller) {
      public void ConfigureAsGpioInput(CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsGpioInputAsync(CancellationToken cancellationToken = default) {}
      public void ConfigureAsGpioOutput(PinValue? initialValue = null, CancellationToken cancellationToken = default) {}
      public ValueTask ConfigureAsGpioOutputAsync(PinValue? initialValue = null, CancellationToken cancellationToken = default) {}
    }
  }

  public static class II2cControllerExtensions {
    extension(II2cController controller) {
      public int Read(I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public ValueTask<int> ReadAsync(I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public int ReadByte(I2cAddress address, int transmissionSpeedInKbps, CancellationToken cancellationToken = default) {}
      public ValueTask<int> ReadByteAsync(I2cAddress address, int transmissionSpeedInKbps, CancellationToken cancellationToken = default) {}
      public void Write(I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(I2cAddress address, int transmissionSpeedInKbps, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default) {}
      public void WriteByte(I2cAddress address, int transmissionSpeedInKbps, byte @value, CancellationToken cancellationToken = default) {}
      public ValueTask WriteByteAsync(I2cAddress address, int transmissionSpeedInKbps, byte @value, CancellationToken cancellationToken = default) {}
    }
  }

  public static class II2cDeviceExtensions {
    extension(II2cDevice device) {
      public void Read(Span<byte> buffer, CancellationToken cancellationToken = default) {}
      public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) {}
      public int ReadByte(CancellationToken cancellationToken = default) {}
      public ValueTask<int> ReadByteAsync(CancellationToken cancellationToken = default) {}
      public void Write(ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default) {}
      public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {}
      public void WriteByte(byte @value, CancellationToken cancellationToken = default) {}
      public ValueTask WriteByteAsync(byte @value, CancellationToken cancellationToken = default) {}
    }
  }

  public static class IMcp2221AInfoExtensions {
    extension(IMcp2221AInfo info) {
      public bool IsMcp2221A { get; }
    }
  }

  public class Mcp2221ACommandException : InvalidOperationException {
    public Mcp2221ACommandException() {}
    public Mcp2221ACommandException(string? message) {}
    public Mcp2221ACommandException(string? message, Exception? innerException) {}
  }

  public class Mcp2221AConfigurationException : InvalidOperationException {
    public Mcp2221AConfigurationException() {}
    public Mcp2221AConfigurationException(string? message) {}
    public Mcp2221AConfigurationException(string? message, Exception? innerException) {}

    public GpFunction? CurrentFunction { get; }
    public int? GpIndex { get; }
    public GpFunction? RequiredFunction { get; }
  }

  public sealed class Mcp2221AController :
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

    public static Mcp2221AController Create(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) {}
    public static Mcp2221AController Create(IServiceProvider serviceProvider, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static Mcp2221AController Create(IUsbHidDevice usbHidDevice, bool shouldDisposeUsbHidDevice = false, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static Mcp2221AController Create<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, CancellationToken cancellationToken = default) {}
    public static Mcp2221AController Create<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static Mcp2221AController Create<TServiceKey>(IUsbHidDevice usbHidDevice, IServiceProvider? serviceProvider, TServiceKey serviceKey, bool shouldDisposeUsbHidDevice = false, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync(IServiceProvider serviceProvider, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync(IUsbHidDevice usbHidDevice, bool shouldDisposeUsbHidDevice = false, IServiceProvider? serviceProvider = null, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync<TServiceKey>(IServiceProvider serviceProvider, TServiceKey serviceKey, Predicate<IUsbHidDevice>? usbHidDeviceFilter, Predicate<IMcp2221AInfo>? mcp2221AFilter, CancellationToken cancellationToken = default) {}
    public static ValueTask<Mcp2221AController> CreateAsync<TServiceKey>(IUsbHidDevice usbHidDevice, IServiceProvider? serviceProvider, TServiceKey serviceKey, bool shouldDisposeUsbHidDevice = false, CancellationToken cancellationToken = default) {}

    public string ChipFactorySerialNumber { get; }
    public VoltageReferenceSource CurrentAdcReferenceSource { get; }
    public VoltageReferenceSource CurrentDacReferenceSource { get; }
    public string FirmwareRevision { get; }
    public DeviceConfigurationProtectionLevel FlashWriteProtection { get; }
    public Gp0Controller GpPin0 { get; }
    public Gp1Controller GpPin1 { get; }
    public Gp2Controller GpPin2 { get; }
    public Gp3Controller GpPin3 { get; }
    public IGpControllerGroup GpPins { get; }
    public GpioController GpioController { get; }
    public string HardwareRevision { get; }
    public IUsbHidDevice HidDevice { get; }
    public Mcp2221AI2cBus I2cBus { get; }
    public int LastWriteAnalogRawValue { get; }
    public string Manufacturer { get; }
    public string Product { get; }
    public string SerialNumber { get; }
    public bool UsbCdcSerialNumberEnabled { get; }
    public UsbPowerMode UsbPowerMode { get; }
    public int UsbProductId { get; }
    public bool UsbRemoteWakeUpEnabled { get; }
    public int UsbRequestedCurrentAmount { get; }
    public int UsbVendorId { get; }

    public void Dispose() {}
    public async ValueTask DisposeAsync() {}
    public void Reset(CancellationToken cancellationToken = default) {}
    public async ValueTask ResetAsync(CancellationToken cancellationToken = default) {}
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
    IComparisonOperators<I2cAddress, I2cAddress, bool>,
    IEquatable<I2cAddress>,
    IEquatable<byte>,
    IEquatable<int>
  {
    public static I2cAddress DeviceMaxValue { get; } // = "0x77"
    public static I2cAddress DeviceMinValue { get; } // = "0x08"
    public static I2cAddress Zero { get; } // = "0x00"

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
  public interface IAdcController {
    VoltageReferenceSource CurrentAdcReferenceSource { get; }
    int LastReadAnalogRawValue { get; }

    void ConfigureAsAdc(VoltageReferenceSource? voltageReferenceSource, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsAdcAsync(VoltageReferenceSource? voltageReferenceSource, CancellationToken cancellationToken = default);
    int ReadAnalogRaw(CancellationToken cancellationToken = default);
    ValueTask<int> ReadAnalogRawAsync(CancellationToken cancellationToken = default);
  }

  public interface IClockOutputController {
    ClockOutputDutyCycle CurrentClockOutputDutyCycle { get; }
    ClockOutputFrequency CurrentClockOutputFrequency { get; }

    void ConfigureAsClockOutput(ClockOutputFrequency? frequency = null, ClockOutputDutyCycle? dutyCycle = null, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsClockOutputAsync(ClockOutputFrequency? frequency = null, ClockOutputDutyCycle? dutyCycle = null, CancellationToken cancellationToken = default);
    void SuspendClockOutput(CancellationToken cancellationToken = default);
    ValueTask SuspendClockOutputAsync(CancellationToken cancellationToken = default);
  }

  public interface IDacController {
    VoltageReferenceSource CurrentDacReferenceSource { get; }
    int LastWriteAnalogRawValue { get; }

    void ConfigureAsDac(VoltageReferenceSource? voltageReferenceSource, int? initialOutputValue = null, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsDacAsync(VoltageReferenceSource? voltageReferenceSource, int? initialOutputValue = null, CancellationToken cancellationToken = default);
    void WriteAnalogRaw(int @value, CancellationToken cancellationToken = default);
    ValueTask WriteAnalogRawAsync(int @value, CancellationToken cancellationToken = default);
  }

  public interface IGpControllerGroup : IReadOnlyList<GpController> {
    VoltageReferenceSource CurrentAdcReferenceSource { get; }
    VoltageReferenceSource CurrentDacReferenceSource { get; }
    Gp0Controller Gp0 { get; }
    Gp1Controller Gp1 { get; }
    Gp2Controller Gp2 { get; }
    Gp3Controller Gp3 { get; }

    void ApplyDacRawValue(int @value, CancellationToken cancellationToken = default);
    ValueTask ApplyDacRawValueAsync(int @value, CancellationToken cancellationToken = default);
    void ApplyGpioStates(ReadOnlySpan<PinValuePair> pinValuePairs, ReadOnlySpan<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    ValueTask ApplyGpioStatesAsync(ReadOnlyMemory<PinValuePair> pinValuePairs, ReadOnlyMemory<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    void ConfigureAllGpSettings(GpFunction? gp0Function = null, PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, GpFunction? gp1Function = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, GpFunction? gp2Function = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, GpFunction? gp3Function = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default);
    ValueTask ConfigureAllGpSettingsAsync(GpFunction? gp0Function = null, PinMode? gp0Mode = null, PinValue? gp0InitialValue = null, GpFunction? gp1Function = null, PinMode? gp1Mode = null, PinValue? gp1InitialValue = null, GpFunction? gp2Function = null, PinMode? gp2Mode = null, PinValue? gp2InitialValue = null, GpFunction? gp3Function = null, PinMode? gp3Mode = null, PinValue? gp3InitialValue = null, CancellationToken cancellationToken = default);
    AdcAllChannelSample FetchAdcRawValues(CancellationToken cancellationToken = default);
    ValueTask<AdcAllChannelSample> FetchAdcRawValuesAsync(CancellationToken cancellationToken = default);
    void FetchGpioStates(Span<PinValuePair> pinValuePairs, Span<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
    ValueTask FetchGpioStatesAsync(Memory<PinValuePair> pinValuePairs, Memory<PinModePair> pinModePairs, CancellationToken cancellationToken = default);
  }

  public interface IGpioController {
    PinMode CurrentMode { get; }
    PinValue LastUpdatedValue { get; }

    void ConfigureAsGpio(PinMode? mode, PinValue? initialValue, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsGpioAsync(PinMode? mode, PinValue? initialValue, CancellationToken cancellationToken = default);
    PinMode GetMode(CancellationToken cancellationToken = default);
    ValueTask<PinMode> GetModeAsync(CancellationToken cancellationToken = default);
    PinValue Read(CancellationToken cancellationToken = default);
    ValueTask<PinValue> ReadAsync(CancellationToken cancellationToken = default);
    void SetMode(PinMode mode, CancellationToken cancellationToken = default);
    ValueTask SetModeAsync(PinMode mode, CancellationToken cancellationToken = default);
    void Write(PinValue @value, CancellationToken cancellationToken = default);
    ValueTask WriteAsync(PinValue @value, CancellationToken cancellationToken = default);
  }

  public interface IInterruptOnChangeController {
    InterruptOnChangeTrigger CurrentInterruptOnChangeTrigger { get; }
    bool LastReadInterruptDetectionFlag { get; }

    void ClearInterruptDetection(CancellationToken cancellationToken = default);
    ValueTask ClearInterruptDetectionAsync(CancellationToken cancellationToken = default);
    void ConfigureAsInterruptOnChange(InterruptOnChangeTrigger? detectionTrigger, bool clearDetectionFlag, CancellationToken cancellationToken = default);
    ValueTask ConfigureAsInterruptOnChangeAsync(InterruptOnChangeTrigger? detectionTrigger, bool clearDetectionFlag, CancellationToken cancellationToken = default);
    bool ReadInterruptDetection(CancellationToken cancellationToken = default);
    ValueTask<bool> ReadInterruptDetectionAsync(CancellationToken cancellationToken = default);
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

  public sealed class Gp1Controller :
    GpController,
    IAdcController,
    IClockOutputController,
    IInterruptOnChangeController
  {
    public ClockOutputDutyCycle CurrentClockOutputDutyCycle { get; }
    public ClockOutputFrequency CurrentClockOutputFrequency { get; }
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public InterruptOnChangeTrigger CurrentInterruptOnChangeTrigger { get; }
    public override int Index { get; }
    public int LastReadAnalogRawValue { get; }
    public bool LastReadInterruptDetectionFlag { get; }
    public override string PinName { get; }
    VoltageReferenceSource IAdcController.CurrentAdcReferenceSource { get; }

    public void ClearInterruptDetection(CancellationToken cancellationToken = default) {}
    public ValueTask ClearInterruptDetectionAsync(CancellationToken cancellationToken = default) {}
    public void ConfigureAsAdc(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public void ConfigureAsClockOutput(ClockOutputFrequency? frequency = null, ClockOutputDutyCycle? dutyCycle = null, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsClockOutputAsync(ClockOutputFrequency? frequency = null, ClockOutputDutyCycle? dutyCycle = null, CancellationToken cancellationToken = default) {}
    public void ConfigureAsInterruptOnChange(InterruptOnChangeTrigger? detectionTrigger = null, bool clearDetectionFlag = true, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsInterruptOnChangeAsync(InterruptOnChangeTrigger? detectionTrigger = null, bool clearDetectionFlag = true, CancellationToken cancellationToken = default) {}
    public void ConfigureAsUtxLedOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUtxLedOutputAsync(CancellationToken cancellationToken = default) {}
    public int ReadAnalogRaw(CancellationToken cancellationToken = default) {}
    public async ValueTask<int> ReadAnalogRawAsync(CancellationToken cancellationToken = default) {}
    public bool ReadInterruptDetection(CancellationToken cancellationToken = default) {}
    public async ValueTask<bool> ReadInterruptDetectionAsync(CancellationToken cancellationToken = default) {}
    public void SuspendClockOutput(CancellationToken cancellationToken = default) {}
    public ValueTask SuspendClockOutputAsync(CancellationToken cancellationToken = default) {}
  }

  public sealed class Gp2Controller :
    GpController,
    IAdcController,
    IDacController
  {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public int LastReadAnalogRawValue { get; }
    public override string PinName { get; }
    VoltageReferenceSource IAdcController.CurrentAdcReferenceSource { get; }
    VoltageReferenceSource IDacController.CurrentDacReferenceSource { get; }
    int IDacController.LastWriteAnalogRawValue { get; }

    public void ConfigureAsAdc(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public void ConfigureAsDac(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, int? initialOutputValue = null, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsDacAsync(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, int? initialOutputValue = null, CancellationToken cancellationToken = default) {}
    public void ConfigureAsUsbConfigureStatus(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsUsbConfigureStatusAsync(CancellationToken cancellationToken = default) {}
    public int ReadAnalogRaw(CancellationToken cancellationToken = default) {}
    public async ValueTask<int> ReadAnalogRawAsync(CancellationToken cancellationToken = default) {}
    public void WriteAnalogRaw(int @value, CancellationToken cancellationToken = default) {}
    public ValueTask WriteAnalogRawAsync(int @value, CancellationToken cancellationToken = default) {}
  }

  public sealed class Gp3Controller :
    GpController,
    IAdcController,
    IDacController
  {
    public override string CurrentDesignation { get; }
    public override GpFunction CurrentFunction { get; }
    public override int Index { get; }
    public int LastReadAnalogRawValue { get; }
    public override string PinName { get; }
    VoltageReferenceSource IAdcController.CurrentAdcReferenceSource { get; }
    VoltageReferenceSource IDacController.CurrentDacReferenceSource { get; }
    int IDacController.LastWriteAnalogRawValue { get; }

    public void ConfigureAsAdc(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsAdcAsync(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, CancellationToken cancellationToken = default) {}
    public void ConfigureAsDac(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, int? initialOutputValue = null, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsDacAsync(VoltageReferenceSource? voltageReferenceSource = (VoltageReferenceSource)0, int? initialOutputValue = null, CancellationToken cancellationToken = default) {}
    public void ConfigureAsI2cLedOutput(CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsI2cLedOutputAsync(CancellationToken cancellationToken = default) {}
    public int ReadAnalogRaw(CancellationToken cancellationToken = default) {}
    public async ValueTask<int> ReadAnalogRawAsync(CancellationToken cancellationToken = default) {}
    public void WriteAnalogRaw(int @value, CancellationToken cancellationToken = default) {}
    public ValueTask WriteAnalogRawAsync(int @value, CancellationToken cancellationToken = default) {}
  }

  public abstract class GpController : IGpioController {
    public PinMode ConfiguredMode { get; }
    public PinValue ConfiguredOutputValue { get; }
    public abstract string CurrentDesignation { get; }
    public abstract GpFunction CurrentFunction { get; }
    public PinMode CurrentMode { get; }
    public abstract int Index { get; }
    public bool IsUsedByGpioController { get; }
    public PinValue LastUpdatedValue { get; }
    public abstract string PinName { get; }

    public void ConfigureAsGpio(PinMode? mode = (PinMode)1, PinValue? initialValue = null, CancellationToken cancellationToken = default) {}
    public ValueTask ConfigureAsGpioAsync(PinMode? mode = (PinMode)1, PinValue? initialValue = null, CancellationToken cancellationToken = default) {}
    public PinMode GetMode(CancellationToken cancellationToken = default) {}
    public async ValueTask<PinMode> GetModeAsync(CancellationToken cancellationToken = default) {}
    public bool IsFunctionSupported(GpFunction function) {}
    public PinValue Read(CancellationToken cancellationToken = default) {}
    public async ValueTask<PinValue> ReadAsync(CancellationToken cancellationToken = default) {}
    public void SetMode(PinMode mode, CancellationToken cancellationToken = default) {}
    public async ValueTask SetModeAsync(PinMode mode, CancellationToken cancellationToken = default) {}
    public void Write(PinValue @value, CancellationToken cancellationToken = default) {}
    public async ValueTask WriteAsync(PinValue @value, CancellationToken cancellationToken = default) {}
  }

  public readonly record struct AdcAllChannelSample {
    public AdcAllChannelSample(ushort adc1, ushort adc2, ushort adc3) {}

    public ushort Adc1 { get; init; }
    public ushort Adc2 { get; init; }
    public ushort Adc3 { get; init; }

    public (int Adc1, int Adc2, int Adc3) AsInt32() {}
    public (double Adc1, double Adc2, double Adc3) AsVoltage(VoltageReferenceSource adcVoltageReference) {}
    public (double Adc1, double Adc2, double Adc3) AsVoltage(double referenceVoltage) {}
    [CompilerGenerated]
    public override string ToString() {}
  }
}

namespace Smdn.Devices.Mcp2221A.Peripherals.I2c {
  public interface II2cController {
    void CancelTransfer(I2cAddress address);
    ValueTask CancelTransferAsync(I2cAddress address);
    int Read(I2cAddress address, int transmissionSpeedInKbps, Span<byte> buffer, CancellationToken cancellationToken = default);
    ValueTask<int> ReadAsync(I2cAddress address, int transmissionSpeedInKbps, Memory<byte> buffer, CancellationToken cancellationToken = default);
    void Write(I2cAddress address, int transmissionSpeedInKbps, ReadOnlySpan<byte> buffer, CancellationToken cancellationToken = default);
    ValueTask WriteAsync(I2cAddress address, int transmissionSpeedInKbps, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
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
    public I2cNackException(I2cAddress address, string? message = null, Exception? innerException = null) {}
    public I2cNackException(string? message) {}
    public I2cNackException(string? message, Exception? innerException) {}
  }

  public class I2cReadException : I2cCommandException {
    public I2cReadException() {}
    public I2cReadException(I2cAddress address, string? message = null, Exception? innerException = null) {}
    public I2cReadException(string? message) {}
    public I2cReadException(string? message, Exception? innerException) {}
  }

  public static class II2cControllerBusScanningExtensions {
    extension(II2cController controller) {
      public (IReadOnlySet<I2cAddress> WriteAddressSet, IReadOnlySet<I2cAddress> ReadAddressSet) ScanBus(I2cAddress fromAddress = default, I2cAddress toAddress = default, int transmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
      public ValueTask<(IReadOnlySet<I2cAddress> WriteAddressSet, IReadOnlySet<I2cAddress> ReadAddressSet)> ScanBusAsync(I2cAddress fromAddress = default, I2cAddress toAddress = default, int transmissionSpeedInKbps = 100, IProgress<I2cScanBusProgress>? progress = null, CancellationToken cancellationToken = default) {}
    }
  }

  public sealed class Mcp2221AI2cBus :
    I2cBus,
    II2cController
  {
    public const int MaxBlockLength = 65535;

    public Mcp2221AI2cDevice CreateDevice(I2cAddress deviceAddress, bool shouldDisposeMcp2221AController = false) {}
    public Mcp2221AI2cDevice CreateDevice(I2cAddress deviceAddress, int transmissionSpeedInKbps, bool shouldDisposeMcp2221AController = false) {}
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
    public I2cAddress CurrentAddress { get; }
    public I2cAddress FromAddress { get; }
    public int ProgressInPercent { get; }
    public I2cAddress ToAddress { get; }
  }
}
// API list generated by Smdn.Reflection.ReverseGenerating.ListApi.MSBuild.Tasks v1.8.2.0.
// Smdn.Reflection.ReverseGenerating.ListApi.Core v1.6.2.0 (https://github.com/smdn/Smdn.Reflection.ReverseGenerating)
