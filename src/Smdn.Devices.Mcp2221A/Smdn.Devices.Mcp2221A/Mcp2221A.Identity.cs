// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

public partial class Mcp2221A {
  public const int DeviceVendorId = 0x04d8;
  public const int DeviceProductId = 0x00dd;

  // MCP2221 (not tested)
  public const string HardwareRevisionMcp2221 = "A.6";
  public const string FirmwareRevisionMcp2221 = "1.1";

  // MCP2221A
  public const string HardwareRevisionMcp2221A = "A.6";
  public const string FirmwareRevisionMcp2221A = "1.2";

  private static void ValidateHardwareRevision(string revision)
  {
    switch (revision) {
      // case HardwareRevisionMcp2221A:
      case HardwareRevisionMcp2221A:
        break;

      default:
        throw new Mcp2221ANotSupportedException($"hardware revision '{revision}' is not supported");
    }
  }

  private static void ValidateFirmwareRevision(string revision)
  {
    switch (revision) {
      case FirmwareRevisionMcp2221:
      case FirmwareRevisionMcp2221A:
        break;

      default:
        throw new Mcp2221ANotSupportedException($"firmware revision '{revision}' is not supported");
    }
  }
}
