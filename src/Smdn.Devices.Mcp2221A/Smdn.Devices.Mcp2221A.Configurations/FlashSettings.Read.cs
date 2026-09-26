// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
#if NET8_0_OR_GREATER
#define SYSTEM_TEXT_ENCODING_TRYGETCHARS
#endif

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  /// <summary>Gets a value indicating whether the USB CDC serial number is enumerated.</summary>
  /// <remarks>Reflects <c>CHIPSETTING0</c>; Bit 7 <c>CDCSNEN</c>.</remarks>
  /// <seealso cref="ModifyCdcSerialNumberEnumeration(bool)"/>
  public bool CdcSerialNumberEnumerationEnabled
    => RegisterUtils.ReadUsbCdcSerialNumberEnabled(SettingsForRead.ChipSettings[OffsetOfChipSetting0]);

  /// <summary>Gets the Flash memory write protection level.</summary>
  /// <remarks>Reflects <c>CHIPSETTING0</c>; Bit 1-0 <c>CHIPPROT</c>.</remarks>
  /// <seealso cref="ModifyWriteProtection(DeviceConfigurationProtectionLevel)"/>
  public DeviceConfigurationProtectionLevel WriteProtectionLevel
    => RegisterUtils.ReadFlashWriteProtection(SettingsForRead.ChipSettings[OffsetOfChipSetting0]);

  /// <summary>Gets the duty cycle of the clock output.</summary>
  /// <remarks>Reflects <c>CHIPSETTING1</c>; Bit 4-3 <c>CLKDC</c>.</remarks>
  /// <seealso cref="ModifyClockOutputDutyCycle(ClockOutputDutyCycle)"/>
  public ClockOutputDutyCycle ClockOutputDutyCycle
    => RegisterUtils.ReadClockOutputDutyCycle(SettingsForRead.ChipSettings[OffsetOfChipSetting1]);

  /// <summary>Gets the frequency divider for the clock output.</summary>
  /// <remarks>Reflects <c>CHIPSETTING1</c>; Bit 2-0 <c>CLKDIV</c>.</remarks>
  /// <seealso cref="ModifyClockOutputFrequency(ClockOutputFrequency)"/>
  public ClockOutputFrequency ClockOutputFrequency
    => RegisterUtils.ReadClockOutputFrequency(SettingsForRead.ChipSettings[OffsetOfChipSetting1]);

  /// <summary>Gets the voltage reference source for the DAC.</summary>
  /// <remarks>Reflects <c>CHIPSETTING2</c>; Bit 7-6 <c>DACVRM</c>, Bit 5 <c>DACREF</c>.</remarks>
  /// <seealso cref="ModifyDacVoltageReference(VoltageReferenceSource)"/>
  public VoltageReferenceSource DacVoltageReference
    => RegisterUtils.ReadDacVoltageReference(SettingsForRead.ChipSettings[OffsetOfChipSetting2]);

  /// <summary>Gets the initial raw output value for the DAC.</summary>
  /// <remarks>Reflects <c>CHIPSETTING2</c>; Bit 4-0 <c>DACVAL</c>.</remarks>
  /// <seealso cref="ModifyDacInitialValue(int)"/>
  public int DacInitialValue
    => RegisterUtils.ReadDacInitialValue(SettingsForRead.ChipSettings[OffsetOfChipSetting2]);

  /// <summary>Gets the Interrupt-On-Change (IOC) trigger settings.</summary>
  /// <remarks>Reflects <c>CHIPSETTING3</c>; Bit 6 <c>INTDETFEEN</c>, Bit 5 <c>INTDETREEN</c>.</remarks>
  /// <seealso cref="ModifyInterruptOnChangeTrigger(InterruptOnChangeTrigger)"/>
  public InterruptOnChangeTrigger InterruptOnChangeTrigger
    => RegisterUtils.ReadInterruptOnChangeTrigger(SettingsForRead.ChipSettings[OffsetOfChipSetting3]);

  /// <summary>Gets the voltage reference source for the ADC.</summary>
  /// <remarks>Reflects <c>CHIPSETTING3</c>; Bit 4-3 <c>ADCVRM</c>, Bit 2 <c>ADCREF</c>.</remarks>
  /// <seealso cref="ModifyAdcVoltageReference(VoltageReferenceSource)"/>
  public VoltageReferenceSource AdcVoltageReference
    => RegisterUtils.ReadAdcVoltageReference(SettingsForRead.ChipSettings[OffsetOfChipSetting3]);

  /// <summary>Gets the USB Vendor ID (VID).</summary>
  /// <remarks>Reflects <c>USBVIDL</c>/<c>USBVIDH</c>.</remarks>
  /// <seealso cref="ModifyUsbVendorId(int)"/>
  public int UsbVendorId
    => BinaryPrimitives.ReadUInt16LittleEndian(
      SettingsForRead.ChipSettings.Slice(OffsetOfUsbVendorId, 2)
    );

  /// <summary>Gets the USB Product ID (PID).</summary>
  /// <remarks>Reflects <c>USBPIDL</c>/<c>USBPIDH</c>.</remarks>
  /// <seealso cref="ModifyUsbProductId(int)"/>
  public int UsbProductId
    => BinaryPrimitives.ReadUInt16LittleEndian(
      SettingsForRead.ChipSettings.Slice(OffsetOfUsbProductId, 2)
    );

  /// <summary>Gets the USB power mode (Self-powered or Bus-powered).</summary>
  /// <remarks>Reflects <c>USBPWRATTR</c>; Bit 6 <c>SELFPWR</c>.</remarks>
  /// <seealso cref="ModifyUsbPowerMode(UsbPowerMode)"/>
  public UsbPowerMode UsbPowerMode
    => RegisterUtils.ReadUsbPowerMode(SettingsForRead.ChipSettings[OffsetOfUsbPowerAttributes]);

  /// <summary>Gets a value indicating whether the USB Remote Wake-up capability is enabled.</summary>
  /// <remarks>Reflects <c>USBPWRATTR</c>; Bit 5 <c>REMWKUP</c>.</remarks>
  /// <seealso cref="ModifyUsbRemoteWakeUp(bool)"/>
  public bool UsbRemoteWakeUpEnabled
    => RegisterUtils.ReadUsbRemoteWakeUpEnabled(SettingsForRead.ChipSettings[OffsetOfUsbPowerAttributes]);

  /// <summary>Gets the maximum amount of current requested from the USB bus, in milliamperes (mA).</summary>
  /// <remarks>Reflects <c>USBREQCRT</c>.</remarks>
  /// <seealso cref="ModifyUsbRequestedCurrentAmount(int)"/>
  public int UsbRequestedCurrentAmount
    => RegisterUtils.ReadUsbRequestedCurrentAmount(SettingsForRead.ChipSettings[OffsetOfUsbRequiredCurrent]);

  /// <summary>Gets the configuration for GP0.</summary>
  /// <remarks>
  /// Reflects <c>GPSETTING0</c>.
  /// <para>
  /// Note that <see cref="GpSetting"/> is a <see langword="readonly"/> value struct
  /// representing a snapshot of the configuration at the time of access.
  /// Modifying the returned value does not update the staged settings.
  /// To modify the configuration, use <see cref="ModifyGpSetting"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpSetting"/>
  /// <seealso cref="ModifyGpSetting"/>
  public GpSetting GpPin0 => new(0, SettingsForRead.GpSettings[0]);

  /// <summary>Gets the configuration for GP1.</summary>
  /// <remarks>
  /// Reflects <c>GPSETTING1</c>.
  /// <para>
  /// Note that <see cref="GpSetting"/> is a <see langword="readonly"/> value struct
  /// representing a snapshot of the configuration at the time of access.
  /// Modifying the returned value does not update the staged settings.
  /// To modify the configuration, use <see cref="ModifyGpSetting"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpSetting"/>
  /// <seealso cref="ModifyGpSetting"/>
  public GpSetting GpPin1 => new(1, SettingsForRead.GpSettings[1]);

  /// <summary>Gets the configuration for GP2.</summary>
  /// <remarks>
  /// Reflects <c>GPSETTING2</c>.
  /// <para>
  /// Note that <see cref="GpSetting"/> is a <see langword="readonly"/> value struct
  /// representing a snapshot of the configuration at the time of access.
  /// Modifying the returned value does not update the staged settings.
  /// To modify the configuration, use <see cref="ModifyGpSetting"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpSetting"/>
  /// <seealso cref="ModifyGpSetting"/>
  public GpSetting GpPin2 => new(2, SettingsForRead.GpSettings[2]);

  /// <summary>Gets the configuration for GP3.</summary>
  /// <remarks>
  /// Reflects <c>GPSETTING3</c>.
  /// <para>
  /// Note that <see cref="GpSetting"/> is a <see langword="readonly"/> value struct
  /// representing a snapshot of the configuration at the time of access.
  /// Modifying the returned value does not update the staged settings.
  /// To modify the configuration, use <see cref="ModifyGpSetting"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpSetting"/>
  /// <seealso cref="ModifyGpSetting"/>
  public GpSetting GpPin3 => new(3, SettingsForRead.GpSettings[3]);

  /// <summary>
  /// Gets the read-only list of configurations for all GP pins (GP0 to GP3).
  /// </summary>
  /// <remarks>
  /// Reflects <c>GPSETTING0</c> through <c>GPSETTING3</c>.
  /// <para>
  /// Note that <see cref="GpSetting"/> elements in this list are <see langword="readonly"/>
  /// value structs representing snapshots of the configuration at the time of access.
  /// Modifying the returned values does not update the staged settings.
  /// To modify the configuration, use <see cref="ModifyGpSetting"/>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpSetting"/>
  /// <seealso cref="ModifyGpSetting"/>
  public IReadOnlyList<GpSetting> GpPins => new ReadOnlyGpSettingList(this);

  private readonly struct ReadOnlyGpSettingList(FlashSettings owner) : IReadOnlyList<GpSetting> {
    private readonly FlashSettings owner = owner ?? throw new ArgumentNullException(nameof(owner));

    public int Count => 4; // GpPin[0-3]

    public GpSetting this[int index] => index switch {
      0 => owner.GpPin0,
      1 => owner.GpPin1,
      2 => owner.GpPin2,
      3 => owner.GpPin3,
      _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    public IEnumerator<GpSetting> GetEnumerator()
    {
      // Always evaluate `owner.GpPin<n>` dynamically on each iteration.
      // Do NOT capture `owner.SettingsForRead` or internal buffers into
      // a local variable prior to or during enumeration; doing so would
      // capture a stale snapshot and fail to reflect state changes
      // (e.g., ModifyGpSetting) that occur while iterating.
      yield return owner.GpPin0;
      yield return owner.GpPin1;
      yield return owner.GpPin2;
      yield return owner.GpPin3;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      => GetEnumerator();
  }

  /// <summary>
  /// Attempts to copy the USB manufacturer string descriptor from the Flash
  /// memory buffer to the specified destination.
  /// The string to be copied is the one staged for writing to the Flash memory,
  /// which may have been modified by <see cref="ModifyUsbManufacturerString"/>.
  /// </summary>
  /// <param name="destination">
  /// The span to which the manufacturer string is copied.
  /// </param>
  /// <param name="charsWritten">
  /// When this method returns, contains the number of characters written
  /// to the <paramref name="destination"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the entire string was successfully copied;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Before any modification or after calling a restore operation,
  /// the copied value will be the same as the initial value read from the device.
  /// After calling <see cref="ModifyUsbManufacturerString"/>, this method
  /// will return the updated string staged in the buffer.
  /// </para>
  /// <para>
  /// Providing a <paramref name="destination"/> with a length of at least 30
  /// characters ensures that this method will not return <see langword="false"/>
  /// due to insufficient buffer space.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbManufacturerString"/>
  public bool TryCopyUsbManufacturerStringTo(Span<char> destination, out int charsWritten)
    => TryCopyUsbDescriptorStringTo(
      SettingsForRead.StoredUsbManufacturerDescriptorStringSpan,
      destination,
      out charsWritten
    );

  /// <summary>
  /// Attempts to copy the USB product string descriptor from the Flash
  /// memory buffer to the specified destination.
  /// The string to be copied is the one staged for writing to the Flash memory,
  /// which may have been modified by <see cref="ModifyUsbProductString"/>.
  /// </summary>
  /// <param name="destination">
  /// The span to which the product string is copied.
  /// </param>
  /// <param name="charsWritten">
  /// When this method returns, contains the number of characters written
  /// to the <paramref name="destination"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the entire string was successfully copied;
  /// otherwise, <see langword="false"/>.</returns>
  /// <remarks>
  /// <para>
  /// Before any modification or after calling a restore operation,
  /// the copied value will be the same as the initial value read from the device.
  /// After calling <see cref="ModifyUsbProductString"/>, this method
  /// will return the updated string staged in the buffer.
  /// </para>
  /// <para>
  /// Providing a <paramref name="destination"/> with a length of at least 30
  /// characters ensures that this method will not return <see langword="false"/>
  /// due to insufficient buffer space.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbProductString"/>
  public bool TryCopyUsbProductStringTo(Span<char> destination, out int charsWritten)
    => TryCopyUsbDescriptorStringTo(
      SettingsForRead.StoredUsbProductDescriptorStringSpan,
      destination,
      out charsWritten
    );

  /// <summary>
  /// Attempts to copy the USB serial number string descriptor from the Flash
  /// memory buffer to the specified destination.
  /// The string to be copied is the one staged for writing to the Flash memory,
  /// which may have been modified by <see cref="ModifyUsbSerialNumberString"/>.
  /// </summary>
  /// <param name="destination">
  /// The span to which the serial number string is copied.
  /// </param>
  /// <param name="charsWritten">
  /// When this method returns, contains the number of characters written
  /// to the <paramref name="destination"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the entire string was successfully copied;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Before any modification or after calling a restore operation,
  /// the copied value will be the same as the initial value read from the device.
  /// After calling <see cref="ModifyUsbSerialNumberString"/>, this method
  /// will return the updated string staged in the buffer.
  /// </para>
  /// <para>
  /// Providing a <paramref name="destination"/> with a length of at least 30
  /// characters ensures that this method will not return <see langword="false"/>
  /// due to insufficient buffer space.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbSerialNumberString"/>
  public bool TryCopyUsbSerialNumberStringTo(Span<char> destination, out int charsWritten)
    => TryCopyUsbDescriptorStringTo(
      SettingsForRead.StoredUsbSerialNumberDescriptorStringSpan,
      destination,
      out charsWritten
    );

  private static bool TryCopyUsbDescriptorStringTo(
    ReadOnlySpan<byte> descriptor,
    Span<char> destination,
    out int charsWritten
  )
  {
    charsWritten = default;

#if SYSTEM_TEXT_ENCODING_TRYGETCHARS
    return Encoding.Unicode.TryGetChars(descriptor, destination, out charsWritten);
#elif SYSTEM_TEXT_ENCODING_GETCHARCOUNT_READONLYSPAN_OF_BYTE
    var length = Encoding.Unicode.GetCharCount(descriptor);

    if (destination.Length < length)
      return false;

    charsWritten = Encoding.Unicode.GetChars(descriptor, destination);

    return true;
#else
    var chars = Encoding.Unicode.GetChars(descriptor.ToArray()); // TODO: reduce allocation

    if (destination.Length < chars.Length)
      return false;

    chars.CopyTo(destination);

    charsWritten = chars.Length;

    return true;
#endif
  }

  /// <summary>
  /// Attempts to copy the chip factory serial number from the Flash
  /// memory buffer to the specified destination.
  /// </summary>
  /// <param name="destination">
  /// The span to which the chip factory serial number is copied.
  /// </param>
  /// <param name="charsWritten">
  /// When this method returns, contains the number of characters written
  /// to the <paramref name="destination"/>.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the entire string was successfully copied;
  /// otherwise, <see langword="false"/>.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Providing a <paramref name="destination"/> with a length of at least 60
  /// characters ensures that this method will not return <see langword="false"/>
  /// due to insufficient buffer space.
  /// </para>
  /// </remarks>
  public bool TryCopyChipFactorySerialNumberTo(Span<char> destination, out int charsWritten)
  {
    charsWritten = default;

    var bytes = SettingsForRead.StoredChipFactorySerialNumberSpan;

    if (destination.Length < bytes.Length)
      return false;

    // Since there is no specified character encoding for chip factory
    // serial number, simply convert the raw byte values to char here.
    for (var i = 0; i < bytes.Length; i++) {
      destination[i] = (char)bytes[i];
    }

    charsWritten = bytes.Length;

    return true;
  }
}
