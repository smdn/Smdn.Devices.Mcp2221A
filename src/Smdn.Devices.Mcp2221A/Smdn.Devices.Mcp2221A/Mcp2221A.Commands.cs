// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

#pragma warning disable CA1848, CA1873, CA2254

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A;

#pragma warning disable IDE0040, CA1724
partial class Mcp2221A {
#pragma warning restore IDE0040, CA1724
#if __FUTURE_VERSION
  public ValueTask ResetAsync() => ValueTask.FromException(new NotImplementedException());
#endif

  private static class RetrieveRevisionCommand {
#pragma warning disable IDE0060, SA1313 // [IDE0060] Remove unused parameter [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(Span<byte> comm, ReadOnlySpan<byte> userData, int _)
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
    ) ParseResponse(ReadOnlySpan<byte> resp, int _)
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
      // 0x04: Read USB Manufacturer Descriptor String
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

  private async ValueTask RetrieveChipInformationAsync(
    Action<string> validateHardwareRevision,
    Action<string> validateFirmwareRevision,
    CancellationToken cancellationToken
  )
  {
    (HardwareRevision, FirmwareRevision) = await CommandAsync(
      userData: default,
      arg: 0,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    ).ConfigureAwait(false);

    validateHardwareRevision?.Invoke(HardwareRevision);
    validateFirmwareRevision?.Invoke(FirmwareRevision);

    ManufacturerDescriptor = await CommandAsync(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringManufacturer,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    ProductDescriptor = await CommandAsync(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringProduct,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    SerialNumberDescriptor = await CommandAsync(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringSerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);

    ChipFactorySerialNumber = await CommandAsync(
      userData: default,
      arg: ReadFlashDataSubCode.ChipFactorySerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    ).ConfigureAwait(false);
  }

  private void RetrieveChipInformation(
    Action<string> validateHardwareRevision,
    Action<string> validateFirmwareRevision,
    CancellationToken cancellationToken
  )
  {
    (HardwareRevision, FirmwareRevision) = Command(
      userData: default,
      arg: 0,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    );

    validateHardwareRevision?.Invoke(HardwareRevision);
    validateFirmwareRevision?.Invoke(FirmwareRevision);

    ManufacturerDescriptor = Command(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringManufacturer,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    ProductDescriptor = Command(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringProduct,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    SerialNumberDescriptor = Command(
      userData: default,
      arg: ReadFlashDataSubCode.UsbDescriptorStringSerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );

    ChipFactorySerialNumber = Command(
      userData: default,
      arg: ReadFlashDataSubCode.ChipFactorySerialNumber,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashStringCommand.ParseResponse
    );
  }
}
