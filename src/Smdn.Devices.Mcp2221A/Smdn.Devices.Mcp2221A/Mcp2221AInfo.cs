// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

#pragma warning disable CA1848, CA1873, CA2254

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

/// <summary>
/// Represents the information set on the MCP2221/MCP2221A or
/// stored in its flash memory.
/// </summary>
public sealed class Mcp2221AInfo : IMcp2221AInfo {
  private static class RetrieveRevisionCommand {
#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, None _)
#pragma warning restore IDE0060, SA1313
    {
      // [MCP2221A] 3.1.1 STATUS/SET PARAMETERS
      comm[0] = 0x10; // Status/Set Parameter
    }

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1316:TupleElementNamesShouldUseCorrectCasing", Justification = "Not a publicly-exposed type or member.")]
#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static (
      string firmwareRevision,
      string hardwareRevision
    ) ParseResponse(ReadOnlySpan<byte> resp, None _)
#pragma warning restore IDE0060, SA1313
    {
      static void CreateRevisionString(Span<char> str, (char major, char minor) revision)
      {
        str[0] = revision.major;
        str[1] = '.';
        str[2] = revision.minor;
      }

      return (
#if false // XXX: string.Create does not accept ReadOnlySpan<T>, dotnet/runtime#30175
        string.Create(3, resp, (str, re) => {str[0] = (char)re[46]; str[1] = '.'; str[2] = (char)re[47]; }),
        string.Create(3, resp, (str, re) => {str[0] = (char)re[48]; str[1] = '.'; str[2] = (char)re[49]; })
#endif
        string.Create(3, ((char)resp[46], (char)resp[47]), CreateRevisionString),
        string.Create(3, ((char)resp[48], (char)resp[49]), CreateRevisionString)
      );
    }
  }

  // [MCP2221A] 3.1.2 READ FLASH DATA
  private enum ReadFlashDataSubCode : byte {
    UsbDescriptorStringManufacturer = 0x02,
    UsbDescriptorStringProduct      = 0x03,
    UsbDescriptorStringSerialNumber = 0x04,
    ChipFactorySerialNumber         = 0x05,
  }

  private static class RetrieveFlashStringCommand {
#pragma warning disable IDE0060 // [IDE0060] Remove unused parameter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, ReadFlashDataSubCode subCode)
#pragma warning restore IDE0060
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
#if false // XXX: string.Create does not accept ReadOnlySpan<T>, dotnet/runtime#30175
        return string.Create((int)resp[2], resp, (str, re) => {
          for (var i = 0; i < str.Length; i++) {
            str[i] = (char)re[i];
          }
        }
#endif
        var length = (int)resp[2];
        Span<char> serialNumberChars = stackalloc char[length];

        for (var i = 0; i < length; i++) {
          serialNumberChars[i] = (char)resp[4 + i];
        }

        return new string(serialNumberChars);
      }
      else {
        // 0x02: The number of bytes + 2 in the provided USB Manufacturer/Product/Serial Number Descriptor String.
        var lengthInBytes = resp[2] - 2;
        var length = lengthInBytes / 2;

        Span<char> descriptorStringChars = stackalloc char[length];

        for (var i = 0; i < length; i++) {
          var lower  = resp[4 + (2 * i) + 0];
          var higher = resp[4 + (2 * i) + 1];

          descriptorStringChars[i] = (char)(lower | (higher << 8));
        }

        return new string(descriptorStringChars);
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
