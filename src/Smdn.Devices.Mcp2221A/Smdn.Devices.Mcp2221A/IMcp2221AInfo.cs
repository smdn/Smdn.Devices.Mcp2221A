// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Defines an interface for accessing pre-fetched chip information set on
/// the MCP2221/MCP2221A or stored in its flash memory.
/// </summary>
/// <seealso cref="Mcp2221AController.Create{TServiceKey}(System.IServiceProvider, TServiceKey, System.Predicate{Smdn.IO.UsbHid.IUsbHidDevice}?, System.Predicate{Smdn.Devices.Mcp2221A.IMcp2221AInfo}?, System.Threading.CancellationToken)"/>
/// <seealso cref="Mcp2221AController.CreateAsync{TServiceKey}(System.IServiceProvider, TServiceKey, System.Predicate{Smdn.IO.UsbHid.IUsbHidDevice}?, System.Predicate{Smdn.Devices.Mcp2221A.IMcp2221AInfo}?, System.Threading.CancellationToken)"/>
public interface IMcp2221AInfo {
  /// <summary>
  /// Gets the hardware revision represented as a string in the format <c>major.minor</c>.
  /// </summary>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
  /// </seealso>
  string HardwareRevision { get; }

  /// <summary>
  /// Gets the firmware revision represented as a string in the format <c>major.minor</c>.
  /// </summary>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
  /// </seealso>
  string FirmwareRevision { get; }

  /// <summary>
  /// Gets the value of the USB Manufacturer String Descriptor stored on the flash memory as a string.
  /// </summary>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 READ FLASH DATA
  /// </seealso>
  string Manufacturer { get; }

  /// <summary>
  /// Gets the value of the USB Product String Descriptor stored on the flash memory as a string.
  /// </summary>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 READ FLASH DATA
  /// </seealso>
  string Product { get; }

  /// <summary>
  /// Gets the value of the USB Serial Number String Descriptor stored on the flash memory as a string.
  /// </summary>
  /// <remarks>
  /// This serial number can be changed by the user through a command.
  /// </remarks>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 READ FLASH DATA
  /// </seealso>
  string SerialNumber { get; }

  /// <summary>
  /// Gets the value of the factory set serial number as a string.
  /// </summary>
  /// <remarks>
  /// This serial number cannot be changed by the user.
  /// </remarks>
  /// <seealso href="https://www.microchip.com/en-us/product/mcp2221a">
  /// [MCP2221A] 3.1.2 READ FLASH DATA
  /// </seealso>
  string ChipFactorySerialNumber { get; }
}
