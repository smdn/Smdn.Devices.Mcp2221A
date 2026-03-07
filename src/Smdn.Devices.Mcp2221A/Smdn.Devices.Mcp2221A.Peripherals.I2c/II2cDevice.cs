// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A.Peripherals.I2c;

/// <summary>
/// Provides an API for abstracting I2C devices operated via the MCP2221/MCP2221A.
/// </summary>
/// <remarks>
/// <para>
/// This interface defines only properties related to I2C devices and does not provide
/// an API for device operations.
/// </para>
/// <para>
/// Instead, I2C devices are operated using the <see cref="Mcp2221AI2cDevice"/> class,
/// which implements this interface and extends <see cref="System.Device.I2c.I2cDevice"/>,
/// or using extension methods defined in the <see cref="II2cDeviceExtensions"/> class.
/// </para>
/// </remarks>
/// <seealso cref="II2cDeviceExtensions"/>
/// <seealso cref="Mcp2221AI2cDevice"/>
public interface II2cDevice {
  /// <summary>
  /// Gets the <see cref="II2cController"/> that created this instance.
  /// </summary>
  /// <exception cref="System.ObjectDisposedException">
  /// Thrown if the <see cref="Mcp2221A"/> associated with this instance has
  /// already been disposed.
  /// </exception>
  II2cController Controller { get; }

  /// <summary>
  /// Gets the <see cref="I2cAddress"/> representing I2C address of this instance.
  /// </summary>
  I2cAddress Address { get; }

  /// <summary>
  /// Gets or sets the transmission speed used for reading and writing to the I2C bus in [kbps] units.
  /// </summary>
  /// <exception cref="System.ArgumentOutOfRangeException">
  /// Thrown if the value being set is zero or negative.
  /// </exception>
  int TransmissionSpeedInKbps { get; set; }
}
