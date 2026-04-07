// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040
partial class Mcp2221AController : IMcp2221AInfo {
#pragma warning restore IDE0040
  private readonly IMcp2221AInfo info;

  /// <inheritdoc/>
  public string HardwareRevision => info.HardwareRevision;

  /// <inheritdoc/>
  public string FirmwareRevision => info.FirmwareRevision;

  /// <inheritdoc/>
  public string Manufacturer => info.Manufacturer;

  /// <inheritdoc/>
  public string Product => info.Product;

  /// <inheritdoc/>
  public string SerialNumber => info.SerialNumber;

  /// <inheritdoc/>
  /// <remarks>Always returns <c>01234567</c>.</remarks>
  public string ChipFactorySerialNumber => info.ChipFactorySerialNumber;
}
