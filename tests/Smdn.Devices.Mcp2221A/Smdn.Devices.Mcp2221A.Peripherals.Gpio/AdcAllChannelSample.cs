// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

[TestFixture]
public class AdcAllChannelSampleTests {
  private static System.Collections.IEnumerable YieldTestCases_Ctor_ArgumentOutOfRange()
  {
    const ushort UInt16_Zero = 0;
    const ushort UInt16_Over10Bit = 1024;

    yield return new object[] { UInt16_Over10Bit, UInt16_Zero, UInt16_Zero, "adc1", UInt16_Over10Bit };
    yield return new object[] { ushort.MaxValue, UInt16_Zero, UInt16_Zero, "adc1", ushort.MaxValue };
    yield return new object[] { UInt16_Zero, UInt16_Over10Bit, UInt16_Zero, "adc2", UInt16_Over10Bit };
    yield return new object[] { UInt16_Zero, ushort.MaxValue, UInt16_Zero, "adc2", ushort.MaxValue };
    yield return new object[] { UInt16_Zero, UInt16_Zero, UInt16_Over10Bit, "adc3", UInt16_Over10Bit };
    yield return new object[] { UInt16_Zero, UInt16_Zero, ushort.MaxValue, "adc3", ushort.MaxValue };
    yield return new object[] { UInt16_Over10Bit, ushort.MaxValue, UInt16_Zero, "adc1", UInt16_Over10Bit };
    yield return new object[] { ushort.MaxValue, UInt16_Over10Bit, UInt16_Zero, "adc1", ushort.MaxValue };
  }

  [TestCaseSource(nameof(YieldTestCases_Ctor_ArgumentOutOfRange))]
  public void Ctor_ArgumentOutOfRange(
    ushort adc1,
    ushort adc2,
    ushort adc3,
    string expectedExceptionParamName,
    ushort expectedExceptionActualValue
  )
  {
    Assert.That(
      () => _ = new AdcAllChannelSample(adc1: adc1, adc2: adc2, adc3: adc3),
      Throws
        .TypeOf<ArgumentOutOfRangeException>()
        .With
        .Property(nameof(ArgumentOutOfRangeException.ParamName))
        .EqualTo(expectedExceptionParamName)
        .And
        .Property(nameof(ArgumentOutOfRangeException.ActualValue))
        .EqualTo(expectedExceptionActualValue)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_AsVoltage_WithAdcVoltageReference_VrmNotOff()
  {
    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, VoltageReferenceSource.Vrm1024, 0.0d, 0.0d, 0.0d };
    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, VoltageReferenceSource.Vrm2048, 0.0d, 0.0d, 0.0d };
    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, VoltageReferenceSource.Vrm4096, 0.0d, 0.0d, 0.0d };

    yield return new object[] { (ushort)1, (ushort)0, (ushort)0, VoltageReferenceSource.Vrm1024, 0.001d, 0.0d, 0.0d };
    yield return new object[] { (ushort)1, (ushort)1, (ushort)1, VoltageReferenceSource.Vrm1024, 0.001d, 0.001d, 0.001d };
    yield return new object[] { (ushort)1023, (ushort)1023, (ushort)1023, VoltageReferenceSource.Vrm1024, 1.023d, 1.023d, 1.023d };

    yield return new object[] { (ushort)0, (ushort)1, (ushort)0, VoltageReferenceSource.Vrm2048, 0.0d, 0.002d, 0.0d };
    yield return new object[] { (ushort)512, (ushort)512, (ushort)1023, VoltageReferenceSource.Vrm2048, 1.024d, 1.024d, 2.046d };

    yield return new object[] { (ushort)0, (ushort)0, (ushort)1, VoltageReferenceSource.Vrm4096, 0.0d, 0.0d, 0.004d };
    yield return new object[] { (ushort)512, (ushort)1023, (ushort)1023, VoltageReferenceSource.Vrm4096, 2.048d, 4.092d, 4.092d };
  }

  [TestCaseSource(nameof(YieldTestCases_AsVoltage_WithAdcVoltageReference_VrmNotOff))]
  public void AsVoltage_WithAdcVoltageReference_VrmNotOff(
    ushort adc1,
    ushort adc2,
    ushort adc3,
    VoltageReferenceSource adcVoltageReference,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
  {
    var expectedVoltages = (expectedAdc1Voltage, expectedAdc2Voltage, expectedAdc3Voltage);

    Assert.That(
      new AdcAllChannelSample(adc1, adc2, adc3).AsVoltage(adcVoltageReference),
      Is.EqualTo(expectedVoltages).Within(1e-9)
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_AsVoltage_WithAdcVoltageReference_VrmOff()
  {
    yield return new object[] { (ushort)0, (ushort)0, (ushort)0 };
    yield return new object[] { (ushort)1, (ushort)0, (ushort)0 };
    yield return new object[] { (ushort)0, (ushort)2, (ushort)0 };
    yield return new object[] { (ushort)0, (ushort)0, (ushort)3 };
    yield return new object[] { (ushort)1023, (ushort)1023, (ushort)1023 };
  }

  [TestCaseSource(nameof(YieldTestCases_AsVoltage_WithAdcVoltageReference_VrmOff))]
  public void AsVoltage_WithAdcVoltageReference_VrmOff(
    ushort adc1,
    ushort adc2,
    ushort adc3
  )
  {
    Assert.That(
      new AdcAllChannelSample(adc1, adc2, adc3).AsVoltage(VoltageReferenceSource.VrmOff),
      Is.Default
    );
  }

  [Test]
  public void AsVoltage_WithAdcVoltageReference_Vdd()
  {
    Assert.That(
      () => _ = new AdcAllChannelSample().AsVoltage(VoltageReferenceSource.Vdd),
      Throws.InvalidOperationException
    );
  }

  [TestCase((VoltageReferenceSource)(-1))]
  [TestCase((VoltageReferenceSource)2)]
  [TestCase((VoltageReferenceSource)4)]
  [TestCase((VoltageReferenceSource)6)]
  [TestCase((VoltageReferenceSource)8)]
  [TestCase((VoltageReferenceSource)int.MaxValue)]
  public void AsVoltage_WithAdcVoltageReference_InvalidVoltageReferenceSource(
    VoltageReferenceSource voltageReferenceSource
  )
  {
    Assert.That(
      () => _ = new AdcAllChannelSample().AsVoltage(voltageReferenceSource),
      Throws.ArgumentException
    );
  }

  private static System.Collections.IEnumerable YieldTestCases_AsVoltage_WithReferenceVoltage()
  {
    const double Vdd_5V0 = 5.0;
    const double Vdd_3V3 = 3.3;
    const double Vdd_Zero = 0.0;

    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, Vdd_5V0, 0.0d, 0.0d, 0.0d };
    yield return new object[] { (ushort)512, (ushort)512, (ushort)512, Vdd_5V0, 2.5d, 2.5d, 2.5d };
    yield return new object[] { (ushort)1023, (ushort)0, (ushort)0, Vdd_5V0, (1023.0d * 5.0d) / 1024.0, 0.0d, 0.0d };

    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, Vdd_3V3, 0.0d, 0.0d, 0.0d };
    yield return new object[] { (ushort)256, (ushort)256, (ushort)256, Vdd_3V3, 0.825d, 0.825d, 0.825d };
    yield return new object[] { (ushort)0, (ushort)1023, (ushort)0, Vdd_3V3, 0.0d, (1023.0d * 3.3d) / 1024.0d, 0.0d };

    yield return new object[] { (ushort)0, (ushort)0, (ushort)0, Vdd_Zero, 0.0d, 0.0d, 0.0d };
    yield return new object[] { (ushort)1023, (ushort)1023, (ushort)1023, Vdd_Zero, 0.0d, 0.0d, 0.0d };

    yield return new object[] { (ushort)100, (ushort)500, (ushort)1000, Vdd_3V3, (100.0d * 3.3d) / 1024.0d, (500.0d * 3.3d) / 1024.0d, (1000.0d * 3.3d) / 1024.0d };
  }

  [TestCaseSource(nameof(YieldTestCases_AsVoltage_WithReferenceVoltage))]
  public void AsVoltage_WithReferenceVoltage(
    ushort adc1,
    ushort adc2,
    ushort adc3,
    double referenceVoltage,
    double expectedAdc1Voltage,
    double expectedAdc2Voltage,
    double expectedAdc3Voltage
  )
  {
    var expectedVoltages = (expectedAdc1Voltage, expectedAdc2Voltage, expectedAdc3Voltage);

    Assert.That(
      new AdcAllChannelSample(adc1, adc2, adc3).AsVoltage(referenceVoltage),
      Is.EqualTo(expectedVoltages).Within(1e-9)
    );
  }

  [TestCase(-1.0)]
  [TestCase(double.MinValue)]
  // [TestCase(double.NegativeZero)]
  [TestCase(double.NegativeInfinity)]
  [TestCase(double.PositiveInfinity)]
  [TestCase(double.NaN)]
  public void AsVoltage_WithReferenceVoltage_InvalidReferenceVoltage(
    double referenceVoltage
  )
  {
    Assert.That(
      () => new AdcAllChannelSample().AsVoltage(referenceVoltage: referenceVoltage),
      Throws
        .InstanceOf<ArgumentException>()
        .With
        .Property(nameof(ArgumentException.ParamName))
        .EqualTo(nameof(referenceVoltage))
    );
  }
}
