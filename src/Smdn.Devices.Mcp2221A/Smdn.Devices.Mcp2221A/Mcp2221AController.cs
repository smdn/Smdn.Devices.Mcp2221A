// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
using System.Diagnostics.CodeAnalysis;
#endif
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Smdn.Devices.Mcp2221A.Peripherals.Gpio;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;
using Smdn.IO.UsbHid;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0055
public partial class Mcp2221AController :
  IDisposable,
  IAsyncDisposable
{
#pragma warning restore IDE0055
  private Mcp2221ATransceiver? transceiver;
  internal Mcp2221ATransceiver Transceiver => transceiver ?? throw new ObjectDisposedException(GetType().Name);
  public IUsbHidDevice HidDevice => transceiver?.EndPoint?.Device ?? throw new ObjectDisposedException(GetType().Name);

  /// <remarks>
  /// Due to inheritance from <see cref="System.Device.Gpio.GpioDriver"/>,
  /// <see cref="Mcp2221AGpioDriver"/> implements <see cref="IDisposable"/>; however,
  /// since the only object that needs to be disposed of is <see cref="Mcp2221ATransceiver"/>,
  /// there is no need to dispose of this object.
  /// </remarks>
#pragma warning disable CA2213
  private readonly Mcp2221AGpioDriver gpioDriver;
#pragma warning restore CA2213

  [CLSCompliant(false)]
  public Mcp2221AI2cBus I2cBus {
    get {
      ThrowIfDisposed();
      return field;
    }
  }

  [CLSCompliant(false)]
  public IGpControllerGroup GpPins {
    get {
      ThrowIfDisposed();
      return gpioDriver;
    }
  }

  public Gp0Controller GpPin0 => GpPins.Gp0;
  public Gp1Controller GpPin1 => GpPins.Gp1;
  public Gp2Controller GpPin2 => GpPins.Gp2;
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
  /// to device binding classes such as <see href="https://www.nuget.org/packages/Iot.Device.Bindings">Iot.Device.Bindings</see>,
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
  /// of the MCP2221A. Changing the reference source on one GP pin will
  /// affect all other GP pins configured as DAC outputs.
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
  /// of the MCP2221A. If no write operation has been performed yet, this
  /// property returns the value currently held by the controller (e.g.,
  /// the default value from Flash settings).
  /// </remarks>
  /// <seealso cref="Gp2Controller.ConfigureAsDac"/>
  /// <seealso cref="Gp3Controller.ConfigureAsDac"/>
  /// <seealso cref="IDacController.LastWriteAnalogRawValue"/>
  public int LastWriteAnalogRawValue {
    get {
      ThrowIfDisposed();
      return gpioDriver.GetLastAppliedDacRawValue();
    }
  }

  /// <summary>
  /// Gets the current voltage reference source configured for the ADC module.
  /// </summary>
  /// <remarks>
  /// This property represents the global configuration for the ADC module
  /// of the MCP2221A. Changing the reference source on one GP pin will
  /// affect all other GP pins configured as ADC inputs.
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

  private Mcp2221AController(
    Mcp2221ATransceiver transceiver,
    IMcp2221AInfo info,
    ILogger? logger
  )
  {
    this.transceiver = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
    this.info = info ?? throw new ArgumentNullException(nameof(info));

    gpioDriver = new(transceiver: transceiver);
    I2cBus = new(this, logger);
    GpioController = new Mcp2221AGpioController(driver: gpioDriver);
  }

  /// <inheritdoc/>
  /// <seealso cref="Reset(System.Threading.CancellationToken)"/>
  public void Dispose()
  {
    Dispose(disposing: true);

    GC.SuppressFinalize(this);
  }

  /// <inheritdoc/>
  /// <seealso cref="ResetAsync(System.Threading.CancellationToken)"/>
  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);

    Dispose(disposing: false);

    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing) {
      transceiver?.Dispose();
      transceiver = null;
    }
  }

  protected virtual async ValueTask DisposeAsyncCore()
  {
    if (transceiver is not null) {
      await transceiver.DisposeAsync().ConfigureAwait(false);
      transceiver = null;
    }
  }

#if SYSTEM_DIAGNOSTICS_CODEANALYSIS_MEMBERNOTNULLATTRIBUTE
  [MemberNotNull(nameof(transceiver))]
#endif
  internal void ThrowIfDisposed() => _ = transceiver ?? throw new ObjectDisposedException(GetType().Name);
}
