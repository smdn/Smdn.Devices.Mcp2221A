// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using NUnit.Framework;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;

namespace Smdn.Devices.Mcp2221A;

[TestFixture]
public class IGpioControllerExtensionsTests {
  [Test]
  public void ConfigureAsGpioOutputAsync_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioOutputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioOutput_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioOutput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioInputAsync_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioInputAsync(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }

  [Test]
  public void ConfigureAsGpioInput_ArgumentNull()
  {
    IGpioController? gpioController = null;

    Assert.That(
      () => gpioController!.ConfigureAsGpioInput(),
      Throws
        .ArgumentNullException
        .With
        .Property(nameof(ArgumentNullException.ParamName))
        .EqualTo("controller")
    );
  }
}
