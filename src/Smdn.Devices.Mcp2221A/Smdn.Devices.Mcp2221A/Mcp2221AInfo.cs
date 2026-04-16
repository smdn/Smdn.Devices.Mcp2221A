// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

#pragma warning disable CA1848, CA1873, CA2254

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Represents the information set on the MCP2221/MCP2221A or
/// stored in its flash memory.
/// </summary>
public sealed class Mcp2221AInfo : IMcp2221AInfo {
  private static class RetrieveRevisionCommand {
#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(Span<byte> comm, None _)
#pragma warning restore SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      comm[0] = 0x10; // Status/Set Parameter
    }

    public static (
      string FirmwareRevision,
      string HardwareRevision
    )
#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    ParseResponse(ReadOnlySpan<byte> resp, None _)
#pragma warning restore SA1313
      => (
        new string([(char)resp[46], '.', (char)resp[47]]),
        new string([(char)resp[48], '.', (char)resp[49]])
      );
  }

  // [MCP2221A] 3.1.2 READ FLASH DATA
  private enum ReadFlashDataSubCode : byte {
    UsbDescriptorStringManufacturer = 0x02,
    UsbDescriptorStringProduct      = 0x03,
    UsbDescriptorStringSerialNumber = 0x04,
    ChipFactorySerialNumber         = 0x05,
  }

  private static class RetrieveFlashStringCommand {
    public static void ConstructCommand(Span<byte> comm, ReadFlashDataSubCode subCode)
    {
      // [MCP2221A] 3.1.2 READ FLASH DATA
      comm[0] = 0xB0; // Read Flash Data

      // Read Flash Data Sub Code
      // 0x02: Read USB Manufacturer Descriptor String
      // 0x03: Read USB Product Descriptor String
      // 0x04: Read USB Serial Number Descriptor String
      // 0x05: Read Chip Factory Serial Number
      comm[1] = (byte)subCode;
    }

    public static string ParseResponse(ReadOnlySpan<byte> resp, ReadFlashDataSubCode subCode)
    {
      if (subCode == ReadFlashDataSubCode.ChipFactorySerialNumber) {
        var lengthInBytes = (int)resp[2];
        // If lengthInBytes is invalid, an ArgumentException is thrown, so
        // an out-of-bounds reference does not occur.
        var bytes = resp.Slice(4, lengthInBytes);

#if SYSTEM_STRING_CREATE_OF_TSTATE_ALLOWS_REF_STRUCT
        return string.Create(
          bytes.Length,
          bytes,
          static (s, by) => {
            for (var i = 0; i < s.Length; i++) {
              s[i] = (char)by[i];
            }
          }
        );
#else
        Span<char> serialNumberChars = stackalloc char[bytes.Length];

        for (var i = 0; i < bytes.Length; i++) {
          serialNumberChars[i] = (char)bytes[i];
        }

#pragma warning disable SA1114
        return new string(
#if SYSTEM_STRING_CTOR_READONLYSPAN_OF_CHAR
          serialNumberChars
#else
          serialNumberChars.ToArray(),
          0,
          serialNumberChars.Length
#endif
        );
#pragma warning restore SA1114
#endif
      }
      else {
        // 0x02: The number of bytes + 2 in the provided USB Manufacturer/Product/Serial Number Descriptor String.
        var lengthInBytes = resp[2] - 2;
        // If lengthInBytes is invalid, an ArgumentException is thrown, so
        // an out-of-bounds reference does not occur.
        var bytes = resp.Slice(4, lengthInBytes);

#pragma warning disable SA1114
        return Encoding.Unicode.GetString(
#if SYSTEM_TEXT_ENCODING_GETSTRING_READONLYSPAN_OF_BYTE
          bytes
#else
          bytes.ToArray(),
          0,
          bytes.Length
#endif
        );
#pragma warning restore SA1114
      }
    }
  }

  internal static async ValueTask<Mcp2221AInfo> ReadFromAsync(
    Mcp2221ATransceiver transceiver,
    CancellationToken cancellationToken
  )
  {
    var (hardwareRevision, firmwareRevision) = await transceiver.CommandAsync(
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    ).ConfigureAwait(false);

    var manufacturerDescriptor = await transceiver.CommandAsync(
      arg: ReadFlashDataSubCode.UsbDescriptorStringManufacturer,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    var productDescriptor = await transceiver.CommandAsync(
      arg: ReadFlashDataSubCode.UsbDescriptorStringProduct,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    var serialNumberDescriptor = await transceiver.CommandAsync(
      arg: ReadFlashDataSubCode.UsbDescriptorStringSerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    var chipFactorySerialNumber = await transceiver.CommandAsync(
      arg: ReadFlashDataSubCode.ChipFactorySerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    return new(
      hardwareRevision: hardwareRevision,
      firmwareRevision: firmwareRevision,
      manufacturer: manufacturerDescriptor,
      product: productDescriptor,
      serialNumber: serialNumberDescriptor,
      chipFactorySerialNumber: chipFactorySerialNumber
    );
  }

  internal static Mcp2221AInfo ReadFrom(
    Mcp2221ATransceiver transceiver,
    CancellationToken cancellationToken
  )
  {
    var (hardwareRevision, firmwareRevision) = transceiver.Command(
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    );

    var manufacturerDescriptor = transceiver.Command(
      arg: ReadFlashDataSubCode.UsbDescriptorStringManufacturer,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    var productDescriptor = transceiver.Command(
      arg: ReadFlashDataSubCode.UsbDescriptorStringProduct,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    var serialNumberDescriptor = transceiver.Command(
      arg: ReadFlashDataSubCode.UsbDescriptorStringSerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    var chipFactorySerialNumber = transceiver.Command(
      arg: ReadFlashDataSubCode.ChipFactorySerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    return new(
      hardwareRevision: hardwareRevision,
      firmwareRevision: firmwareRevision,
      manufacturer: manufacturerDescriptor,
      product: productDescriptor,
      serialNumber: serialNumberDescriptor,
      chipFactorySerialNumber: chipFactorySerialNumber
    );
  }

  /*
   * instance members
   */

  /// <inheritdoc/>
  public string HardwareRevision { get; init; }

  /// <inheritdoc/>
  public string FirmwareRevision { get; init; }

  /// <inheritdoc/>
  public string Manufacturer { get; init; }

  /// <inheritdoc/>
  public string Product { get; init; }

  /// <inheritdoc/>
  public string SerialNumber { get; init; }

  /// <inheritdoc/>
  public string ChipFactorySerialNumber { get; init; }

  private Mcp2221AInfo(
    string hardwareRevision,
    string firmwareRevision,
    string manufacturer,
    string product,
    string serialNumber,
    string chipFactorySerialNumber
  )
  {
    HardwareRevision = hardwareRevision;
    FirmwareRevision = firmwareRevision;
    Manufacturer = manufacturer;
    Product = product;
    SerialNumber = serialNumber;
    ChipFactorySerialNumber = chipFactorySerialNumber;
  }
}
