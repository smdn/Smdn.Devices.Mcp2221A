// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Device.Gpio;
using System.Text;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
  /// <summary>
  /// Modifies the password required for accessing protected
  /// Flash configurations.
  /// </summary>
  /// <param name="password">
  /// A <see cref="ReadOnlySpan{T}"/> containing the password bytes.
  /// The length must be exactly 8 bytes.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Reflects <c>PASS0</c> to <c>PASS7</c>.
  /// </para>
  /// <para>
  /// Calling this method stages the new password and causes <see cref="IsDirty"/>
  /// to return <see langword="true"/>.
  /// Note that staged password modifications are <b>not</b> reverted by
  /// calling <see cref="Restore"/>.
  /// </para>
  /// </remarks>
  /// <exception cref="ArgumentException">
  /// The length of <paramref name="password"/> is not equal to 8.
  /// </exception>
  public FlashSettings ModifyPassword(ReadOnlySpan<byte> password)
  {
    ThrowIfPasswordLengthNotValid(password, nameof(password));

    if (!password.TryCopyTo(SettingsForWrite.Password))
      throw new InvalidOperationException("The destination password buffer is too short to store the password.");

    hasPasswordProvided = true;
    hasPasswordModified = true;
    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the setting that determines whether the USB CDC
  /// serial number is enumerated.
  /// </summary>
  /// <param name="enabled">
  /// <see langword="true"/> to enable CDC serial number enumeration;
  /// otherwise, <see langword="false"/>.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>CHIPSETTING0</c>; Bit 7 <c>CDCSNEN</c>.
  /// </remarks>
  /// <seealso cref="CdcSerialNumberEnumerationEnabled"/>
  public FlashSettings ModifyCdcSerialNumberEnumeration(bool enabled)
  {
    RegisterUtils.WriteUsbCdcSerialNumberEnabled(
      chipSetting0Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting0],
      enabled: enabled
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the Flash memory write protection level.
  /// </summary>
  /// <param name="protectionLevel">
  /// The protection level to be applied to the Flash memory.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="protectionLevel"/> is <see cref="DeviceConfigurationProtectionLevel.Reserved"/>,
  /// or an invalid <see cref="DeviceConfigurationProtectionLevel"/> value.
  /// </exception>
  /// <remarks>
  /// <para>
  /// Reflects <c>CHIPSETTING0</c>; Bit 1-0 <c>CHIPPROT</c>.
  /// </para>
  /// <para>
  /// When setting <paramref name="protectionLevel"/> to
  /// <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>,
  /// <see cref="ModifyPassword"/> must be called to explicitly specify
  /// an 8-byte password. Calling <see cref="Write"/> or <see cref="WriteAsync"/>
  /// while write protection is set to <see cref="DeviceConfigurationProtectionLevel.PasswordProtected"/>
  /// without first setting a password will result in an <see cref="InvalidOperationException"/>
  /// to prevent unintended lockout.
  /// </para>
  /// </remarks>
  /// <seealso cref="WriteProtectionLevel"/>
  /// <seealso cref="ModifyPassword"/>
  /// <seealso cref="Write"/>
  /// <seealso cref="WriteAsync"/>
  public FlashSettings ModifyWriteProtection(
    DeviceConfigurationProtectionLevel protectionLevel
  )
  {
    RegisterUtils.WriteFlashWriteProtection(
      chipSetting0Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting0],
      protectionLevel: protectionLevel switch {
        DeviceConfigurationProtectionLevel.None or
        DeviceConfigurationProtectionLevel.PasswordProtected or
        DeviceConfigurationProtectionLevel.PermanentlyLocked => protectionLevel,

        DeviceConfigurationProtectionLevel.Reserved => throw new ArgumentException(
          message: $"The protection level '{protectionLevel}' is reserved and cannot be set.",
          paramName: nameof(protectionLevel)
        ),

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(protectionLevel),
          invalidValue: (int)protectionLevel,
          enumClass: typeof(DeviceConfigurationProtectionLevel)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the duty cycle of the clock output.
  /// </summary>
  /// <param name="dutyCycle">
  /// The desired duty cycle for the clock output.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="dutyCycle"/> is an invalid <see cref="ClockOutputDutyCycle"/> value.
  /// </exception>
  /// <remarks>
  /// Reflects <c>CHIPSETTING1</c>; Bit 4-3 <c>CLKDC</c>.
  /// </remarks>
  /// <seealso cref="ClockOutputDutyCycle"/>
  public FlashSettings ModifyClockOutputDutyCycle(ClockOutputDutyCycle dutyCycle)
  {
    RegisterUtils.WriteClockOutputDutyCycle(
      chipSetting1Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting1],
      dutyCycle: dutyCycle switch {
        ClockOutputDutyCycle.Duty0 or
        ClockOutputDutyCycle.Duty25 or
        ClockOutputDutyCycle.Duty50 or
        ClockOutputDutyCycle.Duty75 => dutyCycle,

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(dutyCycle),
          invalidValue: (int)dutyCycle,
          enumClass: typeof(ClockOutputDutyCycle)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the frequency divider for the clock output.
  /// </summary>
  /// <param name="frequency">
  /// The desired frequency for the clock output.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="frequency"/> is <see cref="ClockOutputFrequency.Reserved"/>,
  /// or an invalid <see cref="ClockOutputFrequency"/> value.
  /// </exception>
  /// <remarks>
  /// Reflects <c>CHIPSETTING1</c>; Bit 2-0 <c>CLKDIV</c>.
  /// </remarks>
  /// <seealso cref="ClockOutputFrequency"/>
  public FlashSettings ModifyClockOutputFrequency(ClockOutputFrequency frequency)
  {
    RegisterUtils.WriteClockOutputFrequency(
      chipSetting1Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting1],
      frequency: frequency switch {
        ClockOutputFrequency.Frequency24MHz or
        ClockOutputFrequency.Frequency12MHz or
        ClockOutputFrequency.Frequency6MHz or
        ClockOutputFrequency.Frequency3MHz or
        ClockOutputFrequency.Frequency1500kHz or
        ClockOutputFrequency.Frequency750kHz or
        ClockOutputFrequency.Frequency375kHz => frequency,

        ClockOutputFrequency.Reserved => throw new ArgumentException(
          message: $"The frequency divider '{frequency}' is reserved and cannot be set.",
          paramName: nameof(frequency)
        ),

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(frequency),
          invalidValue: (int)frequency,
          enumClass: typeof(ClockOutputFrequency)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the voltage reference source for the DAC.
  /// </summary>
  /// <param name="voltageReference">
  /// The voltage reference source to be used by the DAC.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="voltageReference"/> is an invalid <see cref="VoltageReferenceSource"/> value.
  /// </exception>
  /// <remarks>
  /// Reflects <c>CHIPSETTING2</c>; Bit 7-6 <c>DACVRM</c>, Bit 5 <c>DACREF</c>.
  /// </remarks>
  /// <seealso cref="DacVoltageReference"/>
  public FlashSettings ModifyDacVoltageReference(VoltageReferenceSource voltageReference)
  {
    RegisterUtils.WriteDacVoltageReference(
      chipSetting2Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting2],
      voltageReference: voltageReference switch {
        VoltageReferenceSource.Vdd or
        VoltageReferenceSource.VrmOff or
        VoltageReferenceSource.Vrm1024 or
        VoltageReferenceSource.Vrm2048 or
        VoltageReferenceSource.Vrm4096 => voltageReference,

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(voltageReference),
          invalidValue: (int)voltageReference,
          enumClass: typeof(VoltageReferenceSource)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the initial output value for the DAC.
  /// </summary>
  /// <param name="value">
  /// The 5-bit initial raw output value for the DAC (0 to 31).
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="value"/> is less than 0, or greater than 31.
  /// </exception>
  /// <remarks>
  /// Reflects <c>CHIPSETTING2</c>; Bit 4-0 <c>DACVAL</c>.
  /// </remarks>
  /// <seealso cref="DacInitialValue"/>
  public FlashSettings ModifyDacInitialValue(int value)
  {
    if (value < 0 || 0b1_00000 <= value) {
      throw new ArgumentOutOfRangeException(
        paramName: nameof(value),
        actualValue: value,
        message: "The value must be a 5-bit unsigned integer (0 to 31)."
      );
    }

    RegisterUtils.WriteDacInitialValue(
      chipSetting2Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting2],
      value: value
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the Interrupt-On-Change (IOC) trigger settings.
  /// </summary>
  /// <param name="trigger">
  /// The trigger condition for the interrupt detection.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>CHIPSETTING3</c>; Bit 6 <c>INTDETFEEN</c>, Bit 5 <c>INTDETREEN</c>.
  /// </remarks>
  /// <seealso cref="InterruptOnChangeTrigger"/>
  public FlashSettings ModifyInterruptOnChangeTrigger(InterruptOnChangeTrigger trigger)
  {
    RegisterUtils.WriteInterruptOnChangeTrigger(
      chipSetting3Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting3],
      trigger: trigger switch {
        InterruptOnChangeTrigger.None or
        InterruptOnChangeTrigger.Rising or
        InterruptOnChangeTrigger.Falling or
        InterruptOnChangeTrigger.Both => trigger,

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(trigger),
          invalidValue: (int)trigger,
          enumClass: typeof(InterruptOnChangeTrigger)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the voltage reference source for the ADC.
  /// </summary>
  /// <param name="voltageReference">
  /// The voltage reference source to be used by the ADC.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="voltageReference"/> is an invalid <see cref="VoltageReferenceSource"/> value.
  /// </exception>
  /// <remarks>
  /// Reflects <c>CHIPSETTING3</c>; Bit 4-3 <c>ADCVRM</c>, Bit 2 <c>ADCREF</c>.
  /// </remarks>
  /// <seealso cref="AdcVoltageReference"/>
  public FlashSettings ModifyAdcVoltageReference(VoltageReferenceSource voltageReference)
  {
    RegisterUtils.WriteAdcVoltageReference(
      chipSetting3Register: ref SettingsForWrite.ChipSettings[OffsetOfChipSetting3],
      voltageReference: voltageReference switch {
        VoltageReferenceSource.Vdd or
        VoltageReferenceSource.VrmOff or
        VoltageReferenceSource.Vrm1024 or
        VoltageReferenceSource.Vrm2048 or
        VoltageReferenceSource.Vrm4096 => voltageReference,

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(voltageReference),
          invalidValue: (int)voltageReference,
          enumClass: typeof(VoltageReferenceSource)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB Vendor ID (VID).
  /// </summary>
  /// <param name="vendorId">
  /// The Vendor ID value.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>USBVIDL</c>/<c>USBVIDH</c>.
  /// </remarks>
  /// <seealso cref="UsbVendorId"/>
  public FlashSettings ModifyUsbVendorId(int vendorId)
  {
    BinaryPrimitives.WriteUInt16LittleEndian(
      SettingsForWrite.ChipSettings.Slice(OffsetOfUsbVendorId, 2),
      (ushort)vendorId
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB Product ID (PID).
  /// </summary>
  /// <param name="productId">
  /// The Product ID value.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>USBPIDL</c>/<c>USBPIDH</c>.
  /// </remarks>
  /// <seealso cref="UsbProductId"/>
  public FlashSettings ModifyUsbProductId(int productId)
  {
    BinaryPrimitives.WriteUInt16LittleEndian(
      SettingsForWrite.ChipSettings.Slice(OffsetOfUsbProductId, 2),
      (ushort)productId
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB power mode (Self-powered or Bus-powered).
  /// </summary>
  /// <param name="powerMode">
  /// The USB power mode.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentException">
  /// <paramref name="powerMode"/> is an invalid <see cref="UsbPowerMode"/> value.
  /// </exception>
  /// <remarks>
  /// Reflects <c>USBPWRATTR</c>; Bit 6 <c>SELFPWR</c>.
  /// </remarks>
  /// <seealso cref="UsbPowerMode"/>
  public FlashSettings ModifyUsbPowerMode(UsbPowerMode powerMode)
  {
    RegisterUtils.WriteUsbPowerMode(
      usbPwrAttrRegister: ref SettingsForWrite.ChipSettings[OffsetOfUsbPowerAttributes],
      powerMode: powerMode switch {
        UsbPowerMode.BusPowered or
        UsbPowerMode.SelfPowered => powerMode,

        _ => throw new InvalidEnumArgumentException(
          argumentName: nameof(powerMode),
          invalidValue: (int)powerMode,
          enumClass: typeof(UsbPowerMode)
        ),
      }
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB Remote Wake-up capability.
  /// </summary>
  /// <param name="enabled">
  /// <see langword="true"/> to enable remote wake-up;
  /// otherwise, <see langword="false"/>.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <remarks>
  /// Reflects <c>USBPWRATTR</c>; Bit 5 <c>REMWKUP</c>.
  /// </remarks>
  /// <seealso cref="UsbRemoteWakeUpEnabled"/>
  public FlashSettings ModifyUsbRemoteWakeUp(bool enabled)
  {
    RegisterUtils.WriteUsbRemoteWakeUpEnabled(
      usbPwrAttrRegister: ref SettingsForWrite.ChipSettings[OffsetOfUsbPowerAttributes],
      enabled: enabled
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the USB requested current amount.
  /// </summary>
  /// <param name="currentAmount">
  /// The requested current in mA.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="currentAmount"/> is less than 0, or greater than 500.
  /// </exception>
  /// <remarks>
  /// Reflects <c>USBREQCRT</c>.
  /// The value is stored in the register in units of 2 mA, so specified values
  /// are divided by 2 (rounded down).
  /// </remarks>
  /// <seealso cref="UsbRequestedCurrentAmount"/>
  public FlashSettings ModifyUsbRequestedCurrentAmount(int currentAmount)
  {
    if (currentAmount < 0 || 500 < currentAmount) {
      throw new ArgumentOutOfRangeException(
        paramName: nameof(currentAmount),
        actualValue: currentAmount,
        message: "The requested current amount must be between 0 and 500 mA."
      );
    }

    RegisterUtils.WriteUsbRequestedCurrentAmount(
      usbReqCrtRegister: ref SettingsForWrite.ChipSettings[OffsetOfUsbRequiredCurrent],
      currentAmount: currentAmount
    );

    hasWritten = false;

    return this;
  }

  /// <summary>
  /// Modifies the configuration for a specific GP pin.
  /// </summary>
  /// <param name="gpIndex">
  /// The index of the GP pin (0 to 3).
  /// </param>
  /// <param name="gpFunction">
  /// The function to be assigned to the GP pin.
  /// If <see langword="null"/>, the current function setting is maintained.
  /// </param>
  /// <param name="gpioMode">
  /// The GPIO mode (<see cref="PinMode.Input"/> or <see cref="PinMode.Output"/>).
  /// If <see langword="null"/>, the current mode setting is maintained.
  /// </param>
  /// <param name="gpioOutputValue">
  /// The initial GPIO output value. Applicable when the effective mode is
  /// <see cref="PinMode.Output"/>.
  /// If <see langword="null"/>, the current output value setting is maintained.
  /// </param>
  /// <returns>
  /// The current <see cref="FlashSettings"/> instance.
  /// </returns>
  /// <exception cref="ArgumentOutOfRangeException">
  /// <paramref name="gpIndex"/> is less than 0, or greater than 3.
  /// </exception>
  /// <exception cref="NotSupportedException">
  /// <paramref name="gpFunction"/> is not supported on the specified GP pin, or
  /// <paramref name="gpioMode"/> is <see cref="PinMode.InputPullUp"/> or
  /// <see cref="PinMode.InputPullDown"/>.
  /// </exception>
  /// <exception cref="InvalidEnumArgumentException">
  /// <paramref name="gpioMode"/> is not a valid <see cref="PinMode"/> value.
  /// </exception>
  /// <remarks>
  /// <para>
  /// Specifying <paramref name="gpioMode"/> or <paramref name="gpioOutputValue"/>
  /// is allowed even when <paramref name="gpFunction"/> is <see langword="null"/> or
  /// not <see cref="GpFunction.Gpio"/>.
  /// However, these GPIO settings only take effect when the effective function of
  /// the pin is <see cref="GpFunction.Gpio"/>.
  /// </para>
  /// <para>
  /// Reflects <c>GPSETTING0</c> to <c>GPSETTING3</c>;
  /// Bit 4 <c>GPIOOUTVAL</c>,
  /// Bit 3 <c>GPIODIR</c>,
  /// Bit 2-0 <c>GPDES</c>.
  /// </para>
  /// </remarks>
  /// <seealso cref="GpPin0"/>
  /// <seealso cref="GpPin1"/>
  /// <seealso cref="GpPin2"/>
  /// <seealso cref="GpPin3"/>
  /// <seealso cref="GpPins"/>
  [CLSCompliant(false)]
  public FlashSettings ModifyGpSetting(
    int gpIndex,
    GpFunction? gpFunction,
    PinMode? gpioMode = null,
    PinValue? gpioOutputValue = null
  )
  {
    _ = Mcp2221AGpioDriver.ThrowIfIndexOfGpPinIsOutOfRange(gpIndex, nameof(gpIndex));

    if (gpioMode is { } mode && !(mode == PinMode.Input || mode == PinMode.Output))
      _ = GpController.ThrowDirectionNotSupportedOrInvalidException(mode, nameof(gpioMode));

    ref var register = ref SettingsForWrite.GpSettings[gpIndex];

    if (gpFunction.HasValue) {
      GpSetting.WriteFunction(ref register, gpIndex, gpFunction.Value);

      hasWritten = false;
    }

    if (gpioMode.HasValue) {
      GpSetting.WriteGpioDirection(ref register, gpioMode.Value);

      hasWritten = false;
    }

    if (gpioOutputValue.HasValue) {
      GpSetting.WriteGpioOutputValue(ref register, gpioOutputValue.Value);

      hasWritten = false;
    }

    return this;
  }

  /// <summary>
  /// Attempts to modify the USB manufacturer string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the manufacturer name string.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the manufacturer string descriptor was successfully
  /// modified; otherwise, <see langword="false"/> (for example, if the length of
  /// <paramref name="value"/> exceeds 30 characters).
  /// </returns>
  /// <remarks>
  /// <para>
  /// This method stages the manufacturer string to be written as a USB string
  /// descriptor. The string will be converted to a Unicode byte sequence when
  /// saved to the device.
  /// </para>
  /// <para>
  /// The maximum length of the string is 30 characters (60 bytes in UTF-16).
  /// If <paramref name="value"/> exceeds this limit, the method returns <see langword="false"/>
  /// and the staged settings remain unchanged.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbManufacturerString"/>
  /// <seealso cref="TryCopyUsbManufacturerStringTo"/>
  /// <seealso cref="GetUsbManufacturerString"/>
  public bool TryModifyUsbManufacturerString(ReadOnlySpan<char> value)
  {
    if (
      TryWriteUsbDescriptorStringTo(
        descriptor: value,
        destination: SettingsForWrite.UsbManufacturerDescriptorString,
        out var bytesWritten
      )
    ) {
      SettingsForWrite.UsbManufacturerDescriptorStringLength = bytesWritten;

      hasWritten = false;

      return true;
    }

    return false;
  }

  /// <summary>
  /// Attempts to modify the USB product string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the product name string.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the product string descriptor was successfully
  /// modified; otherwise, <see langword="false"/> (for example, if the length of
  /// <paramref name="value"/> exceeds 30 characters).
  /// </returns>
  /// <remarks>
  /// <para>
  /// While the Product ID (PID) is a fixed numeric value, the product string
  /// is a variable-length Unicode string. This method stages the string as it
  /// should appear when the device is enumerated.
  /// </para>
  /// <para>
  /// The maximum length of the string is 30 characters (60 bytes in UTF-16).
  /// If <paramref name="value"/> exceeds this limit, the method returns <see langword="false"/>
  /// and the staged settings remain unchanged.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbProductString"/>
  /// <seealso cref="TryCopyUsbProductStringTo"/>
  /// <seealso cref="GetUsbProductString"/>
  public bool TryModifyUsbProductString(ReadOnlySpan<char> value)
  {
    if (
      TryWriteUsbDescriptorStringTo(
        descriptor: value,
        destination: SettingsForWrite.UsbProductDescriptorString,
        out var bytesWritten
      )
    ) {
      SettingsForWrite.UsbProductDescriptorStringLength = bytesWritten;

      hasWritten = false;

      return true;
    }

    return false;
  }

  /// <summary>
  /// Attempts to modify the USB serial number string descriptor stored in the
  /// Flash memory buffer.
  /// </summary>
  /// <param name="value">
  /// A <see cref="ReadOnlySpan{Char}"/> containing the serial number string.
  /// </param>
  /// <returns>
  /// <see langword="true"/> if the serial number string descriptor was successfully
  /// modified; otherwise, <see langword="false"/> (for example, if the length of
  /// <paramref name="value"/> exceeds 30 characters).
  /// </returns>
  /// <remarks>
  /// <para>
  /// According to the USB specification, the serial number is a Unicode string
  /// and can contain alphanumeric characters, not just digits. The maximum
  /// length is 30 characters (60 bytes in UTF-16).
  /// </para>
  /// <para>
  /// This serial number string is always used by the HID interface. To also
  /// expose this serial number through the USB CDC (Communication Device Class)
  /// interface, ensure that CDC serial number enumeration is enabled via
  /// <see cref="ModifyCdcSerialNumberEnumeration(bool)"/>.
  /// </para>
  /// <para>
  /// If <paramref name="value"/> exceeds the maximum length, the method returns
  /// <see langword="false"/> and the staged settings remain unchanged.
  /// </para>
  /// </remarks>
  /// <seealso cref="ModifyUsbSerialNumberString"/>
  /// <seealso cref="TryCopyUsbSerialNumberStringTo"/>
  /// <seealso cref="GetUsbSerialNumberString"/>
  public bool TryModifyUsbSerialNumberString(ReadOnlySpan<char> value)
  {
    if (
      TryWriteUsbDescriptorStringTo(
        descriptor: value,
        destination: SettingsForWrite.UsbSerialNumberDescriptorString,
        out var bytesWritten
      )
    ) {
      SettingsForWrite.UsbSerialNumberDescriptorStringLength = bytesWritten;

      hasWritten = false;

      return true;
    }

    return false;
  }

  private static bool TryWriteUsbDescriptorStringTo(
    ReadOnlySpan<char> descriptor,
    Span<byte> destination,
    out int bytesWritten
  )
  {
    bytesWritten = default;

#if SYSTEM_TEXT_ENCODING_TRYGETBYTES
    return Encoding.Unicode.TryGetBytes(descriptor, destination, out bytesWritten);
#elif SYSTEM_TEXT_ENCODING_GETBYTECOUNT_READONLYSPAN_OF_CHAR
    var length = Encoding.Unicode.GetByteCount(descriptor);

    if (destination.Length < length)
      return false;

    bytesWritten = Encoding.Unicode.GetBytes(descriptor, destination);

    return true;
#else
    var bytes = Encoding.Unicode.GetBytes(descriptor.ToArray()); // TODO: reduce allocation

    if (destination.Length < bytes.Length)
      return false;

    bytes.CopyTo(destination);

    bytesWritten = bytes.Length;

    return true;
#endif
  }
}
