// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.ComponentModel;
using System.Device.Gpio;
using System.Text;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettingsTests {
#pragma warning restore IDE0040
  [TestCase(0b_0_11111_00, false, 0b_0_11111_00)] // initial: factory default
  [TestCase(0b_0_11111_00, true, 0b_1_11111_00)] // initial: factory default
  [TestCase(0b_1_00000_11, false, 0b_0_00000_11)]
  [TestCase(0b_1_00000_11, true, 0b_1_00000_11)]
  public void ModifyCdcSerialNumberEnumeration(
    byte initialChipSetting0,
    bool newValue,
    byte expectedChipSetting0
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: initialChipSetting0
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyCdcSerialNumberEnumeration(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[0],
      Is.EqualTo(expectedChipSetting0)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting0 == expectedChipSetting0
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.CdcSerialNumberEnumerationEnabled,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING0 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[0] = initialFlashMemory.ChipSettings[0];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0b_0_11111_00, DeviceConfigurationProtectionLevel.None, 0b_0_11111_00)] // initial: factory default
  [TestCase(0b_0_11111_00, DeviceConfigurationProtectionLevel.PasswordProtected, 0b_0_11111_01)] // initial: factory default
  [TestCase(0b_0_11111_00, DeviceConfigurationProtectionLevel.PermanentlyLocked, 0b_0_11111_10)] // initial: factory default
  [TestCase(0b_1_00000_11, DeviceConfigurationProtectionLevel.None, 0b_1_00000_00)]
  [TestCase(0b_1_00000_11, DeviceConfigurationProtectionLevel.PasswordProtected, 0b_1_00000_01)]
  [TestCase(0b_1_00000_11, DeviceConfigurationProtectionLevel.PermanentlyLocked, 0b_1_00000_10)]
  public void ModifyWriteProtection(
    byte initialChipSetting0,
    DeviceConfigurationProtectionLevel newValue,
    byte expectedChipSetting0
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting0: initialChipSetting0
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyWriteProtection(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[0],
      Is.EqualTo(expectedChipSetting0)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting0 == expectedChipSetting0
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.WriteProtectionLevel,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING0 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[0] = initialFlashMemory.ChipSettings[0];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(DeviceConfigurationProtectionLevel.Reserved, typeof(ArgumentException))]
  [TestCase((DeviceConfigurationProtectionLevel)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((DeviceConfigurationProtectionLevel)0b100, typeof(InvalidEnumArgumentException))]
  [TestCase((DeviceConfigurationProtectionLevel)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyWriteProtection_NewValueInvalid(
    DeviceConfigurationProtectionLevel newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyWriteProtection(protectionLevel: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("protectionLevel")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_000_10_010, ClockOutputDutyCycle.Duty0, 0b_000_00_010)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputDutyCycle.Duty25, 0b_000_01_010)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputDutyCycle.Duty50, 0b_000_10_010)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputDutyCycle.Duty75, 0b_000_11_010)] // initial: factory default
  [TestCase(0b_111_01_101, ClockOutputDutyCycle.Duty0, 0b_111_00_101)]
  [TestCase(0b_111_01_101, ClockOutputDutyCycle.Duty25, 0b_111_01_101)]
  [TestCase(0b_111_01_101, ClockOutputDutyCycle.Duty50, 0b_111_10_101)]
  [TestCase(0b_111_01_101, ClockOutputDutyCycle.Duty75, 0b_111_11_101)]
  public void ModifyClockOutputDutyCycle(
    byte initialChipSetting1,
    ClockOutputDutyCycle newValue,
    byte expectedChipSetting1
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting1: initialChipSetting1
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyClockOutputDutyCycle(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[1],
      Is.EqualTo(expectedChipSetting1)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting1 == expectedChipSetting1
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.ClockOutputDutyCycle,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING1 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[1] = initialFlashMemory.ChipSettings[1];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase((ClockOutputDutyCycle)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((ClockOutputDutyCycle)0b100, typeof(InvalidEnumArgumentException))]
  [TestCase((ClockOutputDutyCycle)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyClockOutputDutyCycle_NewValueInvalid(
    ClockOutputDutyCycle newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyClockOutputDutyCycle(dutyCycle: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("dutyCycle")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency24MHz, 0b_000_10_001)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency12MHz, 0b_000_10_010)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency6MHz, 0b_000_10_011)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency3MHz, 0b_000_10_100)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency1500kHz, 0b_000_10_101)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency750kHz, 0b_000_10_110)] // initial: factory default
  [TestCase(0b_000_10_010, ClockOutputFrequency.Frequency375kHz, 0b_000_10_111)] // initial: factory default
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency24MHz, 0b_111_01_001)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency12MHz, 0b_111_01_010)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency6MHz, 0b_111_01_011)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency3MHz, 0b_111_01_100)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency1500kHz, 0b_111_01_101)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency750kHz, 0b_111_01_110)]
  [TestCase(0b_111_01_101, ClockOutputFrequency.Frequency375kHz, 0b_111_01_111)]
  public void ModifyClockOutputFrequency(
    byte initialChipSetting1,
    ClockOutputFrequency newValue,
    byte expectedChipSetting1
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting1: initialChipSetting1
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyClockOutputFrequency(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[1],
      Is.EqualTo(expectedChipSetting1)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting1 == expectedChipSetting1
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.ClockOutputFrequency,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING1 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[1] = initialFlashMemory.ChipSettings[1];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(ClockOutputFrequency.Reserved, typeof(ArgumentException))]
  [TestCase((ClockOutputFrequency)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((ClockOutputFrequency)0b1000, typeof(InvalidEnumArgumentException))]
  [TestCase((ClockOutputFrequency)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyClockOutputFrequency_NewValueInvalid(
    ClockOutputFrequency newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyClockOutputFrequency(frequency: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("frequency")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vdd, 0b_00_0_01000)] // initial: factory default
  [TestCase(0b_10_0_01000, VoltageReferenceSource.VrmOff, 0b_00_1_01000)] // initial: factory default
  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vrm1024, 0b_01_1_01000)] // initial: factory default
  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vrm2048, 0b_10_1_01000)] // initial: factory default
  [TestCase(0b_10_0_01000, VoltageReferenceSource.Vrm4096, 0b_11_1_01000)] // initial: factory default
  [TestCase(0b_01_1_10111, VoltageReferenceSource.Vdd, 0b_00_0_10111)]
  [TestCase(0b_01_1_10111, VoltageReferenceSource.VrmOff, 0b_00_1_10111)]
  [TestCase(0b_01_1_10111, VoltageReferenceSource.Vrm1024, 0b_01_1_10111)]
  [TestCase(0b_01_1_10111, VoltageReferenceSource.Vrm2048, 0b_10_1_10111)]
  [TestCase(0b_01_1_10111, VoltageReferenceSource.Vrm4096, 0b_11_1_10111)]
  public void ModifyDacVoltageReference(
    byte initialChipSetting2,
    VoltageReferenceSource newValue,
    byte expectedChipSetting2
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting2: initialChipSetting2
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyDacVoltageReference(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[2],
      Is.EqualTo(expectedChipSetting2)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting2 == expectedChipSetting2
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.DacVoltageReference,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING2 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[2] = initialFlashMemory.ChipSettings[2];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase((VoltageReferenceSource)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_01_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_10_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_11_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0001_00_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0001_11_1, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyDacVoltageReference_NewValueInvalid(
    VoltageReferenceSource newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyDacVoltageReference(voltageReference: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("voltageReference")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_10_0_01000, 0, 0b_10_0_00000)] // initial: factory default
  [TestCase(0b_10_0_01000, 1, 0b_10_0_00001)] // initial: factory default
  [TestCase(0b_10_0_01000, 16, 0b_10_0_10000)] // initial: factory default
  [TestCase(0b_10_0_01000, 31, 0b_10_0_11111)] // initial: factory default
  [TestCase(0b_01_1_10111, 0, 0b_01_1_00000)]
  [TestCase(0b_01_1_10111, 1, 0b_01_1_00001)]
  [TestCase(0b_01_1_10111, 16, 0b_01_1_10000)]
  [TestCase(0b_01_1_10111, 31, 0b_01_1_11111)]
  public void ModifyDacInitialValue(
    byte initialChipSetting2,
    int newValue,
    byte expectedChipSetting2
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting2: initialChipSetting2
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyDacInitialValue(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[2],
      Is.EqualTo(expectedChipSetting2)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting2 == expectedChipSetting2
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.DacInitialValue,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING2 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[2] = initialFlashMemory.ChipSettings[2];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(-1)]
  [TestCase(0b1_00000)]
  [TestCase(int.MinValue)]
  [TestCase(int.MaxValue)]
  public void ModifyDacInitialValue_NewValueOutOfRange(
    int newValue
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyDacInitialValue(value: newValue),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo("value")
        .With
        .Property(nameof(ArgumentOutOfRangeException.ActualValue))
        .EqualTo(newValue)
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_0_1_1_01_1_00, InterruptOnChangeTrigger.None, 0b_0_0_0_01_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, InterruptOnChangeTrigger.Rising, 0b_0_0_1_01_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, InterruptOnChangeTrigger.Falling, 0b_0_1_0_01_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, InterruptOnChangeTrigger.Both, 0b_0_1_1_01_1_00)] // initial: factory default
  [TestCase(0b_1_0_0_10_0_11, InterruptOnChangeTrigger.None, 0b_1_0_0_10_0_11)]
  [TestCase(0b_1_0_0_10_0_11, InterruptOnChangeTrigger.Rising, 0b_1_0_1_10_0_11)]
  [TestCase(0b_1_0_0_10_0_11, InterruptOnChangeTrigger.Falling, 0b_1_1_0_10_0_11)]
  [TestCase(0b_1_0_0_10_0_11, InterruptOnChangeTrigger.Both, 0b_1_1_1_10_0_11)]
  public void ModifyInterruptOnChangeTrigger(
    byte initialChipSetting3,
    InterruptOnChangeTrigger newValue,
    byte expectedChipSetting3
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting3: initialChipSetting3
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyInterruptOnChangeTrigger(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[3],
      Is.EqualTo(expectedChipSetting3)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting3 == expectedChipSetting3
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.InterruptOnChangeTrigger,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING3 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[3] = initialFlashMemory.ChipSettings[3];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase((InterruptOnChangeTrigger)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((InterruptOnChangeTrigger)0b_100, typeof(InvalidEnumArgumentException))]
  [TestCase((InterruptOnChangeTrigger)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyInterruptOnChangeTrigger_NewValueInvalid(
    InterruptOnChangeTrigger newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyInterruptOnChangeTrigger(trigger: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("trigger")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vdd, 0b_0_1_1_00_0_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.VrmOff, 0b_0_1_1_00_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vrm1024, 0b_0_1_1_01_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vrm2048, 0b_0_1_1_10_1_00)] // initial: factory default
  [TestCase(0b_0_1_1_01_1_00, VoltageReferenceSource.Vrm4096, 0b_0_1_1_11_1_00)] // initial: factory default
  [TestCase(0b_1_0_0_10_0_11, VoltageReferenceSource.Vdd, 0b_1_0_0_00_0_11)]
  [TestCase(0b_1_0_0_10_0_11, VoltageReferenceSource.VrmOff, 0b_1_0_0_00_1_11)]
  [TestCase(0b_1_0_0_10_0_11, VoltageReferenceSource.Vrm1024, 0b_1_0_0_01_1_11)]
  [TestCase(0b_1_0_0_10_0_11, VoltageReferenceSource.Vrm2048, 0b_1_0_0_10_1_11)]
  [TestCase(0b_1_0_0_10_0_11, VoltageReferenceSource.Vrm4096, 0b_1_0_0_11_1_11)]
  public void ModifyAdcVoltageReference(
    byte initialChipSetting3,
    VoltageReferenceSource newValue,
    byte expectedChipSetting3
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        chipSetting3: initialChipSetting3
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyAdcVoltageReference(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[3],
      Is.EqualTo(expectedChipSetting3)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialChipSetting3 == expectedChipSetting3
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.AdcVoltageReference,
      Is.EqualTo(newValue)
    );

    // Restore only the CHIPSETTING3 and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[3] = initialFlashMemory.ChipSettings[3];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase((VoltageReferenceSource)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_01_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_10_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0000_11_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0001_00_0, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0b_0_0001_11_1, typeof(InvalidEnumArgumentException))]
  [TestCase((VoltageReferenceSource)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyAdcVoltageReference_NewValueInvalid(
    VoltageReferenceSource newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyAdcVoltageReference(voltageReference: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("voltageReference")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0x04D8)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void ModifyUsbVendorId(int vendorId)
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();
    var initialProductId = ~vendorId;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbVidHigherByte: (byte)((initialProductId & 0xFF00) >> 8),
        usbVidLowerByte: (byte)(initialProductId & 0xFF)
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbVendorId(vendorId),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[4],
      Is.EqualTo((byte)(vendorId & 0x00FF))
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[5],
      Is.EqualTo((byte)((vendorId & 0xFF00) >> 8))
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after modification");
    Assert.That(
      mcp2221A.Flash.UsbVendorId,
      Is.EqualTo(vendorId)
    );

    // Restore only the USBVIDL/USBVIDH and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[4] = initialFlashMemory.ChipSettings[4];
    stagedFlashMemory.ChipSettings[5] = initialFlashMemory.ChipSettings[5];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0x00DD)] // factory default
  [TestCase(0xCC00)]
  [TestCase(0x0033)]
  public void ModifyUsbProductId(int productId)
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();
    var initialProductId = ~productId;

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPidHigherByte: (byte)((initialProductId & 0xFF00) >> 8),
        usbPidLowerByte: (byte)(initialProductId & 0xFF)
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbProductId(productId),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[6],
      Is.EqualTo((byte)(productId & 0x00FF))
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[7],
      Is.EqualTo((byte)((productId & 0xFF00) >> 8))
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.True, "after modification");
    Assert.That(
      mcp2221A.Flash.UsbProductId,
      Is.EqualTo(productId)
    );

    // Restore only the USBPIDL/USBPIDH and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[6] = initialFlashMemory.ChipSettings[6];
    stagedFlashMemory.ChipSettings[7] = initialFlashMemory.ChipSettings[7];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0b_1_0_0_00000, UsbPowerMode.BusPowered, 0b_1_0_0_00000)] // initial: factory default
  [TestCase(0b_1_0_0_00000, UsbPowerMode.SelfPowered, 0b_1_1_0_00000)] // initial: factory default
  [TestCase(0b_0_1_1_11111, UsbPowerMode.BusPowered, 0b_0_0_1_11111)]
  [TestCase(0b_0_1_1_11111, UsbPowerMode.SelfPowered, 0b_0_1_1_11111)]
  public void ModifyUsbPowerMode(
    byte initialUsbPowerAttributes,
    UsbPowerMode newValue,
    byte expectedUsbPowerAttributes
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPowerAttributes: initialUsbPowerAttributes
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbPowerMode(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[8],
      Is.EqualTo(expectedUsbPowerAttributes)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbPowerAttributes == expectedUsbPowerAttributes
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.UsbPowerMode,
      Is.EqualTo(newValue)
    );

    // Restore only the USBPWRATTR and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[8] = initialFlashMemory.ChipSettings[8];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase((UsbPowerMode)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase((UsbPowerMode)0b_10, typeof(InvalidEnumArgumentException))]
  [TestCase((UsbPowerMode)0xFF, typeof(InvalidEnumArgumentException))]
  public void ModifyUsbPowerMode_NewValueInvalid(
    UsbPowerMode newValue,
    Type expectedTypeOfException
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyUsbPowerMode(powerMode: newValue),
      Throws
        .TypeOf(expectedTypeOfException)
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("powerMode")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0b_1_0_0_00000, false, 0b_1_0_0_00000)] // initial: factory default
  [TestCase(0b_1_0_0_00000, true, 0b_1_0_1_00000)] // initial: factory default
  [TestCase(0b_0_1_1_11111, false, 0b_0_1_0_11111)]
  [TestCase(0b_0_1_1_11111, true, 0b_0_1_1_11111)]
  public void ModifyUsbRemoteWakeUp(
    byte initialUsbPowerAttributes,
    bool newValue,
    byte expectedUsbPowerAttributes
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbPowerAttributes: initialUsbPowerAttributes
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbRemoteWakeUp(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[8],
      Is.EqualTo(expectedUsbPowerAttributes)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbPowerAttributes == expectedUsbPowerAttributes
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.UsbRemoteWakeUpEnabled,
      Is.EqualTo(newValue)
    );

    // Restore only the USBPWRATTR and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[8] = initialFlashMemory.ChipSettings[8];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0b_00110010, 0, 0b_00000000, 0)] // initial: factory default
  [TestCase(0b_00110010, 1, 0b_00000000, 0)] // initial: factory default
  [TestCase(0b_00110010, 2, 0b_00000001, 2)] // initial: factory default
  [TestCase(0b_00110010, 3, 0b_00000001, 2)] // initial: factory default
  [TestCase(0b_00110010, 4, 0b_00000010, 4)] // initial: factory default
  [TestCase(0b_00110010, 100, 0b_00110010, 100)] // initial: factory default, newValue: factory default (50)
  [TestCase(0b_00110010, 101, 0b_00110010, 100)] // initial: factory default, newValue: factory default (50)
  [TestCase(0b_00110010, 498, 0b_11111001, 498)] // initial: factory default
  [TestCase(0b_00110010, 499, 0b_11111001, 498)] // initial: factory default
  [TestCase(0b_00110010, 500, 0b_11111010, 500)] // initial: factory default
  [TestCase(0b_11001101, 0, 0b_00000000, 0)]
  [TestCase(0b_11001101, 1, 0b_00000000, 0)]
  [TestCase(0b_11001101, 2, 0b_00000001, 2)]
  [TestCase(0b_11001101, 3, 0b_00000001, 2)]
  [TestCase(0b_11001101, 4, 0b_00000010, 4)]
  [TestCase(0b_11001101, 498, 0b_11111001, 498)]
  [TestCase(0b_11001101, 499, 0b_11111001, 498)]
  [TestCase(0b_11001101, 500, 0b_11111010, 500)]
  public void ModifyUsbRequestedCurrentAmount(
    byte initialUsbRequiredCurrent,
    int newValue,
    byte expectedUsbRequiredCurrent,
    int expectedValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        usbRequiredCurrent: initialUsbRequiredCurrent
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyUsbRequestedCurrentAmount(newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.ChipSettings[9],
      Is.EqualTo(expectedUsbRequiredCurrent)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbRequiredCurrent == expectedUsbRequiredCurrent
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.UsbRequestedCurrentAmount,
      Is.EqualTo(expectedValue)
    );

    // Restore only the USBREQCRT and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.ChipSettings[9] = initialFlashMemory.ChipSettings[9];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(int.MinValue)]
  [TestCase(-1)]
  [TestCase(501)]
  [TestCase(int.MaxValue)]
  public void ModifyUsbRequestedCurrentAmount_NewValueInvalid(
    int newValue
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyUsbRequestedCurrentAmount(currentAmount: newValue),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("currentAmount")
    );
    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0, 0b_000_1_0_010, GpFunction.LedOutput, 0b_000_1_0_010)] // initial: factory default; newValue: LEDURX (factory default)
  [TestCase(0, 0b_000_1_0_010, GpFunction.UsbSuspendStatus, 0b_000_1_0_001)] // initial: factory default
  [TestCase(0, 0b_000_1_0_010, GpFunction.Gpio, 0b_000_1_0_000)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, GpFunction.InterruptOnChange, 0b_000_1_0_100)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, GpFunction.LedOutput, 0b_000_1_0_011)] // initial: factory default; newValue: LEDUTX (factory default)
  [TestCase(1, 0b_000_1_0_011, GpFunction.Adc, 0b_000_1_0_010)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, GpFunction.ClockOutput, 0b_000_1_0_001)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, GpFunction.Gpio, 0b_000_1_0_000)] // initial: factory default
  [TestCase(2, 0b_000_1_0_001, GpFunction.Dac, 0b_000_1_0_011)] // initial: factory default
  [TestCase(2, 0b_000_1_0_001, GpFunction.Adc, 0b_000_1_0_010)] // initial: factory default
  [TestCase(2, 0b_000_1_0_001, GpFunction.UsbConfigureStatus, 0b_000_1_0_001)] // initial: factory default; newValue: USBCFG (factory default)
  [TestCase(2, 0b_000_1_0_001, GpFunction.Gpio, 0b_000_1_0_000)] // initial: factory default
  [TestCase(3, 0b_000_1_0_001, GpFunction.Dac, 0b_000_1_0_011)] // initial: factory default
  [TestCase(3, 0b_000_1_0_001, GpFunction.Adc, 0b_000_1_0_010)] // initial: factory default
  [TestCase(3, 0b_000_1_0_001, GpFunction.LedOutput, 0b_000_1_0_001)] // initial: factory default; newValue: LEDI2C (factory default)
  [TestCase(3, 0b_000_1_0_001, GpFunction.Gpio, 0b_000_1_0_000)] // initial: factory default
  [TestCase(0, 0b_111_0_1_101, GpFunction.Gpio, 0b_111_0_1_000)]
  [TestCase(1, 0b_111_0_1_100, GpFunction.Gpio, 0b_111_0_1_000)]
  [TestCase(2, 0b_111_0_1_110, GpFunction.Gpio, 0b_111_0_1_000)]
  [TestCase(3, 0b_111_0_1_110, GpFunction.Gpio, 0b_111_0_1_000)]
  public void ModifyGpSetting_Function(
    int gpIndex,
    byte initialGpSetting,
    GpFunction newValue,
    byte expectedGpSetting
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      device: gpIndex switch {
        0 => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: initialGpSetting),
        1 => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: initialGpSetting),
        2 => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: initialGpSetting),
        3 => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: initialGpSetting),
        _ => throw new InvalidOperationException(),
      },
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyGpSetting(gpIndex, gpFunction: newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.GpSettings[gpIndex],
      Is.EqualTo(expectedGpSetting)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialGpSetting == expectedGpSetting
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].Function,
      Is.EqualTo(newValue)
    );

    // Restore only the GPSETTING<n> and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.GpSettings[gpIndex] = initialFlashMemory.GpSettings[gpIndex];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0, 0b_000_1_0_010, PinMode.Output, 0b_000_1_0_010)] // initial: factory default; newValue: Output (factory default)
  [TestCase(1, 0b_000_1_0_011, PinMode.Output, 0b_000_1_0_011)] // initial: factory default; newValue: Output (factory default)
  [TestCase(2, 0b_000_1_0_001, PinMode.Output, 0b_000_1_0_001)] // initial: factory default; newValue: Output (factory default)
  [TestCase(3, 0b_000_1_0_001, PinMode.Output, 0b_000_1_0_001)] // initial: factory default; newValue: Output (factory default)
  [TestCase(0, 0b_000_1_0_010, PinMode.Input, 0b_000_1_1_010)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, PinMode.Input, 0b_000_1_1_011)] // initial: factory default
  [TestCase(2, 0b_000_1_0_001, PinMode.Input, 0b_000_1_1_001)] // initial: factory default
  [TestCase(3, 0b_000_1_0_001, PinMode.Input, 0b_000_1_1_001)] // initial: factory default
  [TestCase(0, 0b_111_0_1_101, PinMode.Output, 0b_111_0_0_101)]
  [TestCase(1, 0b_111_0_1_100, PinMode.Output, 0b_111_0_0_100)]
  [TestCase(2, 0b_111_0_1_110, PinMode.Output, 0b_111_0_0_110)]
  [TestCase(3, 0b_111_0_1_110, PinMode.Output, 0b_111_0_0_110)]
  public void ModifyGpSetting_GpioMode(
    int gpIndex,
    byte initialGpSetting,
    PinMode newValue,
    byte expectedGpSetting
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      device: gpIndex switch {
        0 => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: initialGpSetting),
        1 => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: initialGpSetting),
        2 => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: initialGpSetting),
        3 => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: initialGpSetting),
        _ => throw new InvalidOperationException(),
      },
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyGpSetting(gpIndex, gpFunction: null, gpioMode: newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.GpSettings[gpIndex],
      Is.EqualTo(expectedGpSetting)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialGpSetting == expectedGpSetting
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].GpioMode,
      Is.EqualTo(newValue)
    );

    // Restore only the GPSETTING<n> and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.GpSettings[gpIndex] = initialFlashMemory.GpSettings[gpIndex];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0, 0b_000_1_0_010, true, 0b_000_1_0_010)] // initial: factory default; newValue: High (factory default)
  [TestCase(1, 0b_000_1_0_011, true, 0b_000_1_0_011)] // initial: factory default; newValue: High (factory default)
  [TestCase(2, 0b_000_1_0_001, true, 0b_000_1_0_001)] // initial: factory default; newValue: High (factory default)
  [TestCase(3, 0b_000_1_0_001, true, 0b_000_1_0_001)] // initial: factory default; newValue: High (factory default)
  [TestCase(0, 0b_000_1_0_010, false, 0b_000_0_0_010)] // initial: factory default
  [TestCase(1, 0b_000_1_0_011, false, 0b_000_0_0_011)] // initial: factory default
  [TestCase(2, 0b_000_1_0_001, false, 0b_000_0_0_001)] // initial: factory default
  [TestCase(3, 0b_000_1_0_001, false, 0b_000_0_0_001)] // initial: factory default
  [TestCase(0, 0b_111_0_1_101, true, 0b_111_1_1_101)]
  [TestCase(1, 0b_111_0_1_100, true, 0b_111_1_1_100)]
  [TestCase(2, 0b_111_0_1_110, true, 0b_111_1_1_110)]
  [TestCase(3, 0b_111_0_1_110, true, 0b_111_1_1_110)]
  public void ModifyGpSetting_GpioOutputValue(
    int gpIndex,
    byte initialGpSetting,
    bool newPinValue,
    byte expectedGpSetting
  )
  {
    var newValue = (PinValue)newPinValue;
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      device: gpIndex switch {
        0 => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: initialGpSetting),
        1 => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: initialGpSetting),
        2 => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: initialGpSetting),
        3 => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: initialGpSetting),
        _ => throw new InvalidOperationException(),
      },
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyGpSetting(gpIndex, gpFunction: null, gpioOutputValue: newValue),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.GpSettings[gpIndex],
      Is.EqualTo(expectedGpSetting)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialGpSetting == expectedGpSetting
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].GpioOutputValue,
      Is.EqualTo(newValue)
    );

    // Restore only the GPSETTING<n> and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.GpSettings[gpIndex] = initialFlashMemory.GpSettings[gpIndex];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0, 0b_000_1_0_010, GpFunction.LedOutput, PinMode.Output, true, 0b_000_1_0_010)] // initial: factory default; newValue: factory default
  [TestCase(1, 0b_000_1_0_011, GpFunction.LedOutput, PinMode.Output, true, 0b_000_1_0_011)] // initial: factory default; newValue: factory default
  [TestCase(2, 0b_000_1_0_001, GpFunction.UsbConfigureStatus, PinMode.Output, true, 0b_000_1_0_001)] // initial: factory default; newValue: factory default
  [TestCase(3, 0b_000_1_0_001, GpFunction.LedOutput, PinMode.Output, true, 0b_000_1_0_001)] // initial: factory default; newValue: factory default
  [TestCase(0, 0b_000_1_0_010, null, null, null, 0b_000_1_0_010)] // initial: factory default; newValue: maintain current
  [TestCase(1, 0b_000_1_0_011, null, null, null, 0b_000_1_0_011)] // initial: factory default; newValue: maintain current
  [TestCase(2, 0b_000_1_0_001, null, null, null, 0b_000_1_0_001)] // initial: factory default; newValue: maintain current
  [TestCase(3, 0b_000_1_0_001, null, null, null, 0b_000_1_0_001)] // initial: factory default; newValue: maintain current
  [TestCase(0, 0b_111_1_1_101, GpFunction.Gpio, null, null, 0b_111_1_1_000)]
  [TestCase(1, 0b_111_1_0_100, GpFunction.Gpio, null, null, 0b_111_1_0_000)]
  [TestCase(2, 0b_111_0_1_110, GpFunction.Gpio, null, null, 0b_111_0_1_000)]
  [TestCase(3, 0b_111_0_0_110, GpFunction.Gpio, null, null, 0b_111_0_0_000)]
  [TestCase(0, 0b_100_1_1_101, null, PinMode.Input, null, 0b_100_1_1_101)]
  [TestCase(1, 0b_100_0_1_100, null, PinMode.Output, null, 0b_100_0_0_100)]
  [TestCase(2, 0b_100_1_0_110, null, PinMode.Input, null, 0b_100_1_1_110)]
  [TestCase(3, 0b_100_0_0_110, null, PinMode.Output, null, 0b_100_0_0_110)]
  [TestCase(0, 0b_001_1_1_101, null, null, true, 0b_001_1_1_101)]
  [TestCase(1, 0b_001_0_1_100, null, null, true, 0b_001_1_1_100)]
  [TestCase(2, 0b_001_1_0_110, null, null, false, 0b_001_0_0_110)]
  [TestCase(3, 0b_001_0_0_110, null, null, false, 0b_001_0_0_110)]
  public void ModifyGpSetting(
    int gpIndex,
    byte initialGpSetting,
    GpFunction? newValueForGpFunction,
    PinMode? newValueForGpioMode,
    bool? newPinValue,
    byte expectedGpSetting
  )
  {
    var newValueForGpioOutputValue = (PinValue?)newPinValue;
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      device: gpIndex switch {
        0 => Mcp2221AControllerTests.CreatePseudoDevice(gp0Settings: initialGpSetting),
        1 => Mcp2221AControllerTests.CreatePseudoDevice(gp1Settings: initialGpSetting),
        2 => Mcp2221AControllerTests.CreatePseudoDevice(gp2Settings: initialGpSetting),
        3 => Mcp2221AControllerTests.CreatePseudoDevice(gp3Settings: initialGpSetting),
        _ => throw new InvalidOperationException(),
      },
      initialFlashMemory,
      stagedFlashMemory
    );

    var (
      initialFunction,
      initialGpioMode,
      initialGpioPinValue
    ) = mcp2221A.Flash.GpPins[gpIndex];

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyGpSetting(
        gpIndex,
        gpFunction: newValueForGpFunction,
        gpioMode: newValueForGpioMode,
        gpioOutputValue: newValueForGpioOutputValue
      ),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.GpSettings[gpIndex],
      Is.EqualTo(expectedGpSetting)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialGpSetting == expectedGpSetting
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].Function,
      Is.EqualTo(newValueForGpFunction ?? initialFunction)
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].GpioMode,
      Is.EqualTo(newValueForGpioMode ?? initialGpioMode)
    );
    Assert.That(
      mcp2221A.Flash.GpPins[gpIndex].GpioOutputValue,
      Is.EqualTo(newValueForGpioOutputValue ?? initialGpioPinValue)
    );

    // Restore only the GPSETTING<n> and verify that the other
    // register areas have not been modified.
    stagedFlashMemory.GpSettings[gpIndex] = initialFlashMemory.GpSettings[gpIndex];

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCase(0, PinMode.InputPullDown, typeof(NotSupportedException))]
  [TestCase(0, PinMode.InputPullUp, typeof(NotSupportedException))]
  [TestCase(1, PinMode.InputPullDown, typeof(NotSupportedException))]
  [TestCase(1, PinMode.InputPullUp, typeof(NotSupportedException))]
  [TestCase(2, PinMode.InputPullDown, typeof(NotSupportedException))]
  [TestCase(2, PinMode.InputPullUp, typeof(NotSupportedException))]
  [TestCase(3, PinMode.InputPullDown, typeof(NotSupportedException))]
  [TestCase(3, PinMode.InputPullUp, typeof(NotSupportedException))]
  [TestCase(0, (PinMode)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase(1, (PinMode)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase(2, (PinMode)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase(3, (PinMode)(-1), typeof(InvalidEnumArgumentException))]
  [TestCase(0, (PinMode)int.MinValue, typeof(InvalidEnumArgumentException))]
  [TestCase(0, (PinMode)int.MaxValue, typeof(InvalidEnumArgumentException))]
  public void ModifyGpSetting_GpioModeInvalid(
    int gpIndex,
    PinMode gpioMode,
    Type typeOfExpectedException
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyGpSetting(gpIndex: gpIndex, gpFunction: null, gpioMode: gpioMode),
      Throws.TypeOf(typeOfExpectedException)
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(0, GpFunction.Adc)]
  [TestCase(0, GpFunction.Dac)]
  [TestCase(0, GpFunction.InterruptOnChange)]
  [TestCase(0, GpFunction.ClockOutput)]
  [TestCase(0, GpFunction.UsbConfigureStatus)]
  [TestCase(1, GpFunction.Dac)]
  [TestCase(1, GpFunction.UsbSuspendStatus)]
  [TestCase(1, GpFunction.UsbConfigureStatus)]
  [TestCase(2, GpFunction.InterruptOnChange)]
  [TestCase(2, GpFunction.LedOutput)]
  [TestCase(2, GpFunction.ClockOutput)]
  [TestCase(2, GpFunction.UsbSuspendStatus)]
  [TestCase(3, GpFunction.InterruptOnChange)]
  [TestCase(3, GpFunction.ClockOutput)]
  [TestCase(3, GpFunction.UsbSuspendStatus)]
  [TestCase(3, GpFunction.UsbConfigureStatus)]
  [TestCase(0, (GpFunction)(-1))]
  [TestCase(1, (GpFunction)(-1))]
  [TestCase(2, (GpFunction)(-1))]
  [TestCase(3, (GpFunction)(-1))]
  [TestCase(0, (GpFunction)int.MaxValue)]
  [TestCase(1, (GpFunction)int.MaxValue)]
  [TestCase(2, (GpFunction)int.MaxValue)]
  [TestCase(3, (GpFunction)int.MaxValue)]
  public void ModifyGpSetting_UnsupportedFunction(
    int gpIndex,
    GpFunction newFunction
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyGpSetting(gpIndex: gpIndex, gpFunction: newFunction),
      Throws.TypeOf<NotSupportedException>()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  [TestCase(int.MinValue)]
  [TestCase(-1)]
  [TestCase(5)]
  [TestCase(int.MaxValue)]
  public void ModifyGpSetting_GpIndexOutOfRange(
    int gpIndex
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => mcp2221A.Flash.ModifyGpSetting(gpIndex: gpIndex, gpFunction: GpFunction.Gpio),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo("gpIndex")
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification (should not be modified)");
  }

  private static System.Collections.IEnumerable YieldTestCases_ModifyPassword()
  {
    yield return new object[] { new byte[8] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };
    yield return new object[] { new byte[8] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF } };
    yield return new object[] { new byte[8] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 } };
    yield return new object[] { "password"u8.ToArray() };
  }

  [TestCaseSource(nameof(YieldTestCases_ModifyPassword))]
  public void ModifyPassword(
    byte[] newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.ModifyPassword(newValue.AsSpan()),
      Is.SameAs(mcp2221A.Flash)
    );
    Assert.That(
      stagedFlashMemory.Password.SequenceEqual(newValue.AsSpan()),
      Is.True
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      "after modification"
    );

    // Once a password has been changed using ModifyPassword(), it is treated as
    // being in a 'dirty' state until a write operation is performed using Write(),
    // and it remains in the 'dirty' state even if Restore() is called.
    Assert.That(
      () => mcp2221A.Flash.Restore(),
      Throws.Nothing
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      Is.True,
      "after restoration"
    );

    // Verify that the other register areas have not been modified.
    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_ModifyPassword_InvalidLength()
  {
    yield return new object[] { new byte[0] };
    yield return new object[] { new byte[] { 0x00 } };
    yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 } };
    yield return new object[] { new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 } };
  }

  [TestCaseSource(nameof(YieldTestCases_ModifyPassword_InvalidLength))]
  public void ModifyPassword_InvalidLength(
    byte[] newValue
  )
  {
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(),
      new FlashMemory(),
      stagedFlashMemory
    );

    var initialPassword = "password"u8.ToArray();

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyPassword(password: initialPassword),
      Throws.Nothing,
      "set valid password"
    );

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyPassword(password: newValue.AsSpan()),
      Throws
        .ArgumentException
        .ParamName.EqualTo("password"),
      "set invalid password"
    );

    Assert.That(
      stagedFlashMemory.Password.SequenceEqual(initialPassword),
      Is.True
    );
  }

  [TestCaseSource(nameof(YieldTestCases_ModifyPassword_InvalidLength))]
  public void ModifyPassword_InvalidLength_IsDirtyMustNotBeChanged(
    byte[] newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice()
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      () => _ = mcp2221A.Flash.ModifyPassword(password: newValue.AsSpan()),
      Throws
        .ArgumentException
        .ParamName.EqualTo("password"),
      "set invalid password"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }

  private static System.Collections.IEnumerable YieldTestCases_TryModifyUsbDescriptorString_InvalidLength()
  {
    yield return new object[] { "", "012345678901234567890123456789" /* 30 */ + "0" };
    yield return new object[] { "Initial Descriptor String", "012345678901234567890123456789" /* 30 */ + "0" };
    yield return new object[] { "Initial Descriptor String", "012345678901234567890123456789" /* 30 */ + "01" };
  }

  private static System.Collections.IEnumerable YieldTestCases_TryModifyUsbManufacturerString()
  {
    yield return new object[] { "Microchip Technology Inc.", "Microchip Technology Inc." }; // initial: factory default
    yield return new object[] { "Microchip Technology Inc.", "" };
    yield return new object[] { "Microchip Technology Inc.", "Vendor" };
    yield return new object[] { "Microchip Technology Inc.", "🔌" };
    yield return new object[] { "Microchip Technology Inc.", "012345678901234567890123456789" }; // initial: factory default, newValue: max length
    yield return new object[] { "Initial Vendor", "New Vendor" };
    yield return new object[] { "", "" };
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbManufacturerString))]
  public void TryModifyUsbManufacturerString(
    string initialUsbManufacturerString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialUsbManufacturerString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbManufacturerString(newValue),
      Is.True
    );
    Assert.That(
      stagedFlashMemory.UsbManufacturerDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(stagedFlashMemory.StoredUsbManufacturerDescriptorStringSpan),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbManufacturerString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbManufacturerString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB manufacturer descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbManufacturerDescriptorString.CopyTo(stagedFlashMemory.UsbManufacturerDescriptorString);

    stagedFlashMemory.UsbManufacturerDescriptorStringLength = initialFlashMemory.UsbManufacturerDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void TryModifyUsbManufacturerString_InvalidLength(
    string initialUsbManufacturerString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        manufacturer: initialUsbManufacturerString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbManufacturerString(newValue),
      Is.False
    );
    Assert.That(
      mcp2221A.Flash.GetUsbManufacturerString(),
      Is.EqualTo(initialUsbManufacturerString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }

  private static System.Collections.IEnumerable YieldTestCases_TryModifyUsbProductString()
  {
    yield return new object[] { "MCP2221 USB-I2C/UART Combo", "MCP2221 USB-I2C/UART Combo" }; // initial: factory default
    yield return new object[] { "MCP2221 USB-I2C/UART Combo", "" };
    yield return new object[] { "MCP2221 USB-I2C/UART Combo", "Product" };
    yield return new object[] { "MCP2221 USB-I2C/UART Combo", "🔌" };
    yield return new object[] { "MCP2221 USB-I2C/UART Combo", "012345678901234567890123456789" }; // initial: factory default, newValue: max length
    yield return new object[] { "Initial Product", "New Product" };
    yield return new object[] { "", "" };
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbProductString))]
  public void TryModifyUsbProductString(
    string initialUsbProductString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: initialUsbProductString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbProductString(newValue),
      Is.True
    );
    Assert.That(
      stagedFlashMemory.UsbProductDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(stagedFlashMemory.StoredUsbProductDescriptorStringSpan),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbProductString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbProductString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB product descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbProductDescriptorString.CopyTo(stagedFlashMemory.UsbProductDescriptorString);

    stagedFlashMemory.UsbProductDescriptorStringLength = initialFlashMemory.UsbProductDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void TryModifyUsbProductString_InvalidLength(
    string initialUsbProductString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        product: initialUsbProductString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbProductString(newValue),
      Is.False
    );
    Assert.That(
      mcp2221A.Flash.GetUsbProductString(),
      Is.EqualTo(initialUsbProductString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }

  private static System.Collections.IEnumerable YieldTestCases_TryModifyUsbSerialNumberString()
  {
    yield return new object[] { "", "" }; // initial: factory default
    yield return new object[] { "", "Serial Number" };
    yield return new object[] { "", "🔌" };
    yield return new object[] { "", "012345678901234567890123456789" }; // initial: factory default, newValue: max length
    yield return new object[] { "Initial Serial Number", "New Serial Number" };
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbSerialNumberString))]
  public void TryModifyUsbSerialNumberString(
    string initialUsbSerialNumberString,
    string newValue
  )
  {
    var initialFlashMemory = new FlashMemory();
    var stagedFlashMemory = new FlashMemory();

    using var mcp2221A = CreateWithAllocatedFlashMemory(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: initialUsbSerialNumberString
      ),
      initialFlashMemory,
      stagedFlashMemory
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbSerialNumberString(newValue),
      Is.True
    );
    Assert.That(
      stagedFlashMemory.UsbSerialNumberDescriptorStringLength,
      Is.EqualTo(Encoding.Unicode.GetByteCount(newValue))
    );
    Assert.That(
      Encoding.Unicode.GetString(stagedFlashMemory.StoredUsbSerialNumberDescriptorStringSpan),
      Is.EqualTo(newValue)
    );
    Assert.That(
      mcp2221A.Flash.IsDirty,
      initialUsbSerialNumberString.Equals(newValue, StringComparison.Ordinal)
        ? Is.False
        : Is.True,
      "after modification"
    );
    Assert.That(
      mcp2221A.Flash.GetUsbSerialNumberString(),
      Is.EqualTo(newValue)
    );

    // Restore only the USB serial number descriptor register area and
    // verify that the other register areas have not been modified.
    initialFlashMemory.UsbSerialNumberDescriptorString.CopyTo(stagedFlashMemory.UsbSerialNumberDescriptorString);

    stagedFlashMemory.UsbSerialNumberDescriptorStringLength = initialFlashMemory.UsbSerialNumberDescriptorStringLength;

    Assert.That(
      stagedFlashMemory.DiffersFrom(initialFlashMemory),
      Is.False
    );
  }

  [TestCaseSource(nameof(YieldTestCases_TryModifyUsbDescriptorString_InvalidLength))]
  public void TryModifyUsbSerialNumberString_InvalidLength(
    string initialUsbSerialNumberString,
    string newValue
  )
  {
    using var mcp2221A = Mcp2221AController.Create(
      Mcp2221AControllerTests.CreatePseudoDevice(
        serialNumber: initialUsbSerialNumberString
      )
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "before modification");

    Assert.That(
      mcp2221A.Flash.TryModifyUsbSerialNumberString(newValue),
      Is.False
    );
    Assert.That(
      mcp2221A.Flash.GetUsbSerialNumberString(),
      Is.EqualTo(initialUsbSerialNumberString),
      "initial value must be kept"
    );

    Assert.That(mcp2221A.Flash.IsDirty, Is.False, "after modification");
  }
}
