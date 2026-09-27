// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Configurations;
using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.Devices.Mcp2221A.Transport;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Provides an interface to access the features of the MCP2221/MCP2221A
/// controllable via USB HID commands.
/// </summary>
/// <remarks>
/// <para>
/// This class serves as the primary entry point for managing the device's
/// I/O functions over the HID interface, excluding UART communication.
/// It organizes the controllable capabilities into the following specialized
/// properties:
/// </para>
/// <list type="bullet">
/// <item>
/// <term><b>GP Pins</b></term>
/// <description>
/// Access to the four General Purpose pins (GP0–GP3) is provided via the
/// <see cref="GpPins"/> collection and individual properties <see cref="GpPin0"/>
/// through <see cref="GpPin3"/>. These properties allow you to configure I/O
/// modes and access pin-specific functions  such as ADC, DAC, and specialized
/// clock or LED outputs.
/// </description>
/// </item>
/// <item>
/// <term><b>I2C Bus</b></term>
/// <description>
/// Access to the I2C master controller is provided through the <see cref="I2cBus"/>
/// property, enabling communication with I2C peripheral devices, bus speed
/// configuration, and data transfer operations.
/// </description>
/// </item>
/// </list>
/// <para>
/// For overall device management, such as resetting the chip or accessing
/// low-level transceiver details, use the methods and properties provided directly
/// by this class.
/// </para>
/// </remarks>
#pragma warning disable IDE0055
public sealed partial class Mcp2221AController :
  IDisposable,
  IAsyncDisposable
{
#pragma warning restore IDE0055
  private Mcp2221ATransceiver? transceiver;

  internal Mcp2221ATransceiver Transceiver
    => transceiver ?? throw new ObjectDisposedException(GetType().Name);

  /// <summary>
  /// Gets an <see cref="IUsbHidDevice"/> object representing the MCP2221/MCP2221A
  /// as a USB HID device for controlling I2C and GPIO pins.
  /// </summary>
  /// <seealso cref="IUsbHidDevice"/>
  public IUsbHidDevice HidDevice
    => transceiver?.EndPoint?.Device ?? throw new ObjectDisposedException(GetType().Name);

  /// <remarks>
  /// Due to inheritance from <see cref="System.Device.Gpio.GpioDriver"/>,
  /// <see cref="Mcp2221AGpioDriver"/> implements <see cref="IDisposable"/>; however,
  /// since the only object that needs to be disposed of is <see cref="Mcp2221ATransceiver"/>,
  /// there is no need to dispose of this object.
  /// </remarks>
#pragma warning disable CA2213
  private readonly Mcp2221AGpioDriver gpioDriver;
#pragma warning restore CA2213

  /// <summary>
  /// Gets a <see cref="Mcp2221AI2cBus"/> that operates the I2C peripherals
  /// using the current <see cref="Mcp2221AController"/> instance as the
  /// underlying <see cref="System.Device.I2c.I2cBus"/>.
  /// </summary>
  /// <remarks>
  /// Since this property can be used as <see cref="System.Device.I2c.I2cBus"/>,
  /// you can use this object to integrate with the I2C device bindings
  /// provided by <see href="https://learn.microsoft.com/dotnet/iot/intro">Iot.Device.Bindings</see>.
  /// </remarks>
  /// <seealso cref="System.Device.I2c.I2cBus"/>
  /// <seealso cref="II2cController"/>
  /// <seealso cref="II2cControllerExtensions"/>
  [CLSCompliant(false)]
  public Mcp2221AI2cBus I2cBus {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  /// <summary>
  /// Gets an <see cref="IGpControllerGroup"/> object that implements the GPIO
  /// functionality of the MCP2221/MCP2221A, providing bulk control of GPIO pins
  /// (<c>GP0</c>-<c>GP3</c>) with a single command.
  /// </summary>
  /// <seealso cref="GpPin0"/>
  /// <seealso cref="GpPin1"/>
  /// <seealso cref="GpPin2"/>
  /// <seealso cref="GpPin3"/>
  /// <seealso cref="IGpControllerGroup"/>
  /// <seealso cref="IGpControllerGroupExtensions"/>
  [CLSCompliant(false)]
  public IGpControllerGroup GpPins {
    get {
      ThrowIfDisposed();
      return gpioDriver;
    }
  }

  /// <summary>
  /// Gets a <see cref="Gp0Controller"/> to configure and control the GPIO functionality
  /// available on the <c>GP0</c> pin of the MCP2221/MCP2221A.
  /// </summary>
  public Gp0Controller GpPin0 => GpPins.Gp0;

  /// <summary>
  /// Gets a <see cref="Gp1Controller"/> to configure and control the GPIO functionality
  /// available on the <c>GP1</c> pin of the MCP2221/MCP2221A.
  /// </summary>
  public Gp1Controller GpPin1 => GpPins.Gp1;

  /// <summary>
  /// Gets a <see cref="Gp2Controller"/> to configure and control the GPIO functionality
  /// available on the <c>GP2</c> pin of the MCP2221/MCP2221A.
  /// </summary>
  public Gp2Controller GpPin2 => GpPins.Gp2;

  /// <summary>
  /// Gets a <see cref="Gp3Controller"/> to configure and control the GPIO functionality
  /// available on the <c>GP3</c> pin of the MCP2221/MCP2221A.
  /// </summary>
  public Gp3Controller GpPin3 => GpPins.Gp3;

  /// <summary>
  /// Gets a <see cref="GpioController"/> that operates the GPIO peripherals using
  /// the current <see cref="Mcp2221AController"/> instance as the underlying <see cref="GpioDriver"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// If the <see cref="GpioController.Dispose()"/> is called on the <see cref="GpioController"/>
  /// returned by this property, the <see cref="GpioController"/> will be disposed of, but
  /// the underlying <see cref="Mcp2221AController"/> will remain available for use.
  /// </para>
  /// <para>
  /// It is recommended that when passing an instance of this property as a <see cref="GpioController"/>
  /// to device binding classes such as <see href="https://learn.microsoft.com/dotnet/iot/intro">Iot.Device.Bindings</see>,
  /// set the <c>shouldDispose</c> parameter to <see langword="false"/> and manage the lifecycle of
  /// the <see cref="Mcp2221AController"/> instance separately.
  /// </para>
  /// </remarks>
  [CLSCompliant(false)]
  public GpioController GpioController {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  /// <summary>
  /// Gets the current voltage reference source configured for the DAC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the DAC module
  /// of the MCP2221/MCP2221A. Changing the reference source on one GP pin
  /// will affect all other GP pins configured as DAC outputs.
  /// </remarks>
  /// <seealso cref="Gp2Controller.ConfigureAsDac"/>
  /// <seealso cref="Gp3Controller.ConfigureAsDac"/>
  /// <seealso cref="IDacController.CurrentDacReferenceSource"/>
  public VoltageReferenceSource CurrentDacReferenceSource {
    get {
      ThrowIfDisposed();
      return gpioDriver.CurrentDacReferenceSource;
    }
  }

  /// <summary>
  /// Gets the 5-bit raw output value (0-31) that was last written to the
  /// DAC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the DAC module
  /// of the MCP2221/MCP2221A. If no write operation has been performed yet,
  /// this property returns the value currently held by the controller (e.g.,
  /// the default value from Flash settings).
  /// </remarks>
  /// <seealso cref="Gp2Controller.ConfigureAsDac"/>
  /// <seealso cref="Gp3Controller.ConfigureAsDac"/>
  /// <seealso cref="IDacController.LastWriteAnalogRawValue"/>
  public int LastWriteAnalogRawValue {
    get {
      ThrowIfDisposed();
      return gpioDriver.LastAppliedDacRawValue;
    }
  }

  /// <summary>
  /// Gets the current voltage reference source configured for the ADC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the ADC module
  /// of the MCP2221/MCP2221A. Changing the reference source on one GP pin
  /// will affect all other GP pins configured as ADC inputs.
  /// </remarks>
  /// <seealso cref="Gp1Controller.ConfigureAsAdc"/>
  /// <seealso cref="Gp2Controller.ConfigureAsAdc"/>
  /// <seealso cref="Gp3Controller.ConfigureAsAdc"/>
  /// <seealso cref="IAdcController.CurrentAdcReferenceSource"/>
  public VoltageReferenceSource CurrentAdcReferenceSource {
    get {
      ThrowIfDisposed();
      return gpioDriver.CurrentAdcReferenceSource;
    }
  }

  /// <summary>
  /// Gets a <see cref="FlashSettings"/> instance that holds both the initial
  /// configuration and staged modifications,/ and provides functionality to
  /// persist changes to the Flash memory of the MCP2221/MCP2221A.
  /// </summary>
  /// <value>
  /// A <see cref="FlashSettings"/> instance used to inspect, stage, and
  /// write Flash memory configurations.
  /// </value>
  /// <remarks>
  /// <para>
  /// Properties and read operations accessed through this instance reflect
  /// the current staged (modified) values rather than the initial state captured
  /// when the <see cref="Mcp2221AController"/> was created.
  /// </para>
  /// <para>
  /// Staged modifications can be inspected via <see cref="FlashSettings.IsDirty"/>
  /// and reverted to the initial state using <see cref="FlashSettings.Restore"/>.
  /// Note that pending password modifications made via <see cref="FlashSettings.ModifyPassword"/>
  /// are maintained even after calling <see cref="FlashSettings.Restore"/>
  /// to prevent accidental password overwrites with indeterminate initial data.
  /// </para>
  /// <para>
  /// <b>Important Cautions for Flash Memory Operations:</b>
  /// <list type="bullet">
  ///   <item>
  ///     <description>
  ///       <b>Power-on/Reset Requirement:</b> Settings written to Flash memory
  ///       do not immediately alter active SRAM settings; they are loaded into SRAM
  ///       as the initial operating configuration upon the next power cycle or
  ///       device reset.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>
  ///       <b>Endurance Limits:</b> Flash memory has a limited number of
  ///       write/erase cycles. Ensure that calls to <see cref="FlashSettings.Write"/>
  ///       and <see cref="FlashSettings.WriteAsync"/> are kept to a minimum to
  ///       prevent premature hardware degradation.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>
  ///       <b>Password Protection Risk:</b> Configuring a password enables write
  ///       protection for Flash settings. However, if the password is lost or forgotten,
  ///       subsequent Flash write operations will become permanently impossible.
  ///     </description>
  ///   </item>
  ///   <item>
  ///     <description>
  ///       <b>Datasheet &amp; Tool Verification:</b> Thoroughly review the official
  ///       datasheet specifications before performing write operations. For critical
  ///       device provisioning, consider using Microchip's official configuration
  ///       utilities.
  ///     </description>
  ///   </item>
  /// </list>
  /// </para>
  /// </remarks>
  /// <seealso cref="FlashSettings"/>
  /// <seealso cref="FlashSettings.Write"/>
  /// <seealso cref="FlashSettings.WriteAsync"/>
  /// <seealso cref="FlashSettings.IsDirty"/>
  /// <seealso cref="FlashSettings.Restore"/>
  /// <seealso cref="FlashSettings.ModifyPassword"/>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 1.4 Device Configuration
  /// </seealso>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 READ FLASH DATA
  /// </seealso>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.3 WRITE FLASH DATA
  /// </seealso>
  [CLSCompliant(false)]
  public FlashSettings Flash {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  private Mcp2221AController(
    Mcp2221ATransceiver transceiver,
    FlashSettings flashSettings,
    ILogger? logger
  )
  {
    this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
    Flash = flashSettings ?? throw new ArgumentNullException(nameof(flashSettings));
    info = flashSettings;

    gpioDriver = new(
      transceiver: transceiver,
      logger: logger
    );
    I2cBus = new(this, logger);
    GpioController = new Mcp2221AGpioController(driver: gpioDriver);
  }

  /// <inheritdoc/>
  /// <seealso cref="Reset(System.Threading.CancellationToken)"/>
  public void Dispose()
  {
    transceiver?.Dispose();
    transceiver = null;

    GC.SuppressFinalize(this);
  }

  /// <inheritdoc/>
  /// <seealso cref="ResetAsync(System.Threading.CancellationToken)"/>
  public async ValueTask DisposeAsync()
  {
    if (transceiver is not null) {
      await transceiver.DisposeAsync().ConfigureAwait(false);
      transceiver = null;
    }

    GC.SuppressFinalize(this);
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(transceiver))]
#endif
  internal void ThrowIfDisposed() => _ = transceiver ?? throw new ObjectDisposedException(GetType().Name);
}
