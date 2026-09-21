// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Threading;
using System.Threading.Tasks;

using Smdn.Devices.Mcp2221A.Transport;

namespace Smdn.Devices.Mcp2221A.Configurations;

#pragma warning disable IDE0040
partial class FlashSettings {
#pragma warning restore IDE0040
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
  private enum ReadFlashDataUsbDescriptorStringSubCode : byte {
    Manufacturer = 0x02,
    Product      = 0x03,
    SerialNumber = 0x04,
  }

  private static class RetrieveFlashUsbDescriptorStringCommand {
    public static void ConstructCommand(
      Span<byte> comm,
      (IFlashMemory Memory, ReadFlashDataUsbDescriptorStringSubCode SubCode) arg
    )
    {
      // [MCP2221A] 3.1.2 READ FLASH DATA
      comm[0] = 0xB0; // Read Flash Data

      // Read Flash Data Sub Code
      // 0x02: Read USB Manufacturer Descriptor String
      // 0x03: Read USB Product Descriptor String
      // 0x04: Read USB Serial Number Descriptor String
      comm[1] = (byte)arg.SubCode;
    }

    public static bool ParseResponse(
      ReadOnlySpan<byte> resp,
      (IFlashMemory Memory, ReadFlashDataUsbDescriptorStringSubCode SubCode) arg
    )
    {
      var (memory, subCode) = arg;

      // 0x02: The number of bytes + 2 in the provided USB Manufacturer/Product/Serial Number Descriptor String.
      var lengthInBytes = resp[2] - 2;
      // If lengthInBytes is invalid, an ArgumentException is thrown, so
      // an out-of-bounds reference does not occur.
      var bytes = resp.Slice(4, lengthInBytes);

      switch (subCode) {
        case ReadFlashDataUsbDescriptorStringSubCode.Manufacturer:
          memory.UsbManufacturerDescriptorStringLength = lengthInBytes;
          bytes.CopyTo(memory.UsbManufacturerDescriptorString);
          break;

        case ReadFlashDataUsbDescriptorStringSubCode.Product:
          memory.UsbProductDescriptorStringLength = lengthInBytes;
          bytes.CopyTo(memory.UsbProductDescriptorString);
          break;

        case ReadFlashDataUsbDescriptorStringSubCode.SerialNumber:
          memory.UsbSerialNumberDescriptorStringLength = lengthInBytes;
          bytes.CopyTo(memory.UsbSerialNumberDescriptorString);
          break;

        default:
          throw new InvalidOperationException(); // this should not happen
      }

      // [MCP2221A] 3.1.2 READ FLASH DATA
      // Command responses other than 0x00 are not defined.
      return resp[1] == 0x00;
    }
  }

  private static class RetrieveFlashChipFactorySerialNumberCommand {
#pragma warning disable SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    public static void ConstructCommand(Span<byte> comm, IFlashMemory _)
#pragma warning restore SA1313 // [SA1313] SA1313ParameterNamesMustBeginWithLowerCaseLetter
    {
      // [MCP2221A] 3.1.2 READ FLASH DATA
      comm[0] = 0xB0; // Read Flash Data

      // Read Flash Data Sub Code
      // 0x05: Read Chip Factory Serial Number
      comm[1] = 0x05;
    }

    public static bool ParseResponse(ReadOnlySpan<byte> resp, IFlashMemory memory)
    {
      memory.ChipFactorySerialNumberLength = resp[2];

      // If length is invalid, an ArgumentException is thrown, so
      // an out-of-bounds reference does not occur.
      resp
        .Slice(4, memory.ChipFactorySerialNumberLength)
        .CopyTo(memory.ChipFactorySerialNumber);

      // [MCP2221A] 3.1.2 READ FLASH DATA
      // Command responses other than 0x00 are not defined.
      return resp[1] == 0x00;
    }
  }

  internal static async ValueTask<FlashSettings> ReadFromAsync(
    Mcp2221ATransceiver transceiver,
    IFlashMemoryFactory flashMemoryFactory,
    CancellationToken cancellationToken
  )
  {
    var (hardwareRevision, firmwareRevision) = await transceiver.CommandAsync(
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    ).ConfigureAwait(false);

    var memory = flashMemoryFactory.Create();

    _ = await transceiver.CommandAsync(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.Manufacturer),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    ).ConfigureAwait(false);

    _ = await transceiver.CommandAsync(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.Product),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    ).ConfigureAwait(false);

    _ = await transceiver.CommandAsync(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.SerialNumber),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    ).ConfigureAwait(false);

    _ = await transceiver.CommandAsync(
      arg: memory,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashChipFactorySerialNumberCommand.ConstructCommand,
      parseResponse: RetrieveFlashChipFactorySerialNumberCommand.ParseResponse
    ).ConfigureAwait(false);

    return new(
      initialSettings: memory,
      flashMemoryFactory: flashMemoryFactory,
      transceiver: transceiver,
      hardwareRevision: hardwareRevision,
      firmwareRevision: firmwareRevision
    );
  }

  internal static FlashSettings ReadFrom(
    Mcp2221ATransceiver transceiver,
    IFlashMemoryFactory flashMemoryFactory,
    CancellationToken cancellationToken
  )
  {
    var (hardwareRevision, firmwareRevision) = transceiver.Command(
      cancellationToken: cancellationToken,
      constructCommand: RetrieveRevisionCommand.ConstructCommand,
      parseResponse: RetrieveRevisionCommand.ParseResponse
    );

    var memory = flashMemoryFactory.Create();

    _ = transceiver.Command(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.Manufacturer),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    );

    _ = transceiver.Command(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.Product),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    );

    _ = transceiver.Command(
      arg: (memory, ReadFlashDataUsbDescriptorStringSubCode.SerialNumber),
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashUsbDescriptorStringCommand.ConstructCommand,
      parseResponse: RetrieveFlashUsbDescriptorStringCommand.ParseResponse
    );

    _ = transceiver.Command(
      arg: memory,
      cancellationToken: cancellationToken,
      constructCommand: RetrieveFlashChipFactorySerialNumberCommand.ConstructCommand,
      parseResponse: RetrieveFlashChipFactorySerialNumberCommand.ParseResponse
    );

    return new(
      initialSettings: memory,
      flashMemoryFactory: flashMemoryFactory,
      transceiver: transceiver,
      hardwareRevision: hardwareRevision,
      firmwareRevision: firmwareRevision
    );
  }
}
