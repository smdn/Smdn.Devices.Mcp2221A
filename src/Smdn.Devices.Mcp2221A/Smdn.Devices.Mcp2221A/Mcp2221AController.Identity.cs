// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

public partial class Mcp2221AController {
  /// <summary>
  /// Represents the default Vendor ID (VID) for the MCP2221/MCP2221A.
  /// </summary>
  /// <remarks>
  /// Note that a non-default Vendor ID may be configured by rewriting the device's Flash memory.
  /// </remarks>
  public const int DefaultVendorId = 0x04d8;

  /// <summary>
  /// Represents the default Product ID (PID) for the MCP2221/MCP2221A.
  /// </summary>
  /// <remarks>
  /// Note that a non-default Product ID may be configured by rewriting the device's Flash memory.
  /// </remarks>
  public const int DefaultProductId = 0x00dd;

  /// <summary>
  /// The hardware revision number for the MCP2221, represented as a string.
  /// </summary>
  public const string HardwareRevisionMcp2221 = "A.6";

  /// <summary>
  /// The firmware revision number for the MCP2221, represented as a string.
  /// </summary>
  public const string FirmwareRevisionMcp2221 = "1.1"; // MCP2221 (not tested with actual device)

  /// <summary>
  /// The hardware revision number for the MCP2221A, represented as a string.
  /// </summary>
  public const string HardwareRevisionMcp2221A = "A.6";

  /// <summary>
  /// The firmware revision number for the MCP2221A, represented as a string.
  /// </summary>
  public const string FirmwareRevisionMcp2221A = "1.2"; // MCP2221A

  private static void ValidateHardwareRevision(string revision)
  {
    switch (revision) {
      // case HardwareRevisionMcp2221A:
      case HardwareRevisionMcp2221A:
        break;

      default:
        throw new Mcp2221ANotSupportedException($"The hardware revision '{revision}' is not supported.");
    }
  }

  private static void ValidateFirmwareRevision(string revision)
  {
    switch (revision) {
      case FirmwareRevisionMcp2221:
      case FirmwareRevisionMcp2221A:
        break;

      default:
        throw new Mcp2221ANotSupportedException($"The firmware revision '{revision}' is not supported.");
    }
  }
}
