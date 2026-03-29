' SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
' SPDX-License-Identifier: MIT

Option Infer On

Imports System
Imports System.Device.Gpio
Imports System.Threading

Imports Microsoft.Extensions.DependencyInjection

Imports Smdn.Devices.Mcp2221A
Imports Smdn.IO.UsbHid.DependencyInjection

Class Blink
  Shared Sub Main()
    Dim services As New ServiceCollection()

    services.AddHidSharpUsbHid()

    Using serviceProvider = services.BuildServiceProvider()
      Using device = Mcp2221A.Create(serviceProvider)
        Console.WriteLine("[MCP2221 Device information]")

        Dim serialNumber As String = Nothing

        If device.HidDevice.TryGetSerialNumber(serialNumber) Then
          Console.WriteLine($"Serial number: {serialNumber}")
        End If

        Console.WriteLine($"USB Manufacturer descriptor: {device.Manufacturer}")
        Console.WriteLine($"USB Product descriptor: {device.Product}")
        Console.WriteLine($"USB Serial number descriptor: {device.SerialNumber}")
        Console.WriteLine($"Hardware revision: {device.HardwareRevision}")
        Console.WriteLine($"Firmware revision: {device.FirmwareRevision}")
        Console.WriteLine()

        ' configure GP0-GP3 as GPIO output
        device.GpPin0.ConfigureAsGpioOutput()
        device.GpPin1.ConfigureAsGpioOutput()
        device.GpPin2.ConfigureAsGpioOutput()
        device.GpPin3.ConfigureAsGpioOutput(PinValue.Low) ' initial value also can be specified

        ' set GPIO pin values
        Console.WriteLine("set all GPs HIGH")

        device.GpPins(0).Write(1) ' set GP0 to HIGH with integer value (0 = LOW, any other value = HIGH)

        device.GpPins(1).Write(True) ' set GP1 to HIGH with boolean value

        device.GpPin2.Write(CByte(1)) ' set GP2 to HIGH with byte value

        Dim gp3Value As PinValue = 1

        device.GpPin3.Write(gp3Value) ' set GP3 to HIGH with struct PinValue

        Thread.Sleep(1000)

        Console.WriteLine("set all GPs LOW")

        ' GP0-GP3 also can be accessed via `GpPins` read-only collection property
        For Each gp In device.GpPins
          gp.Write(PinValue.Low)
        Next

        Thread.Sleep(1000)

        Console.WriteLine("set all GPs")

        device.GpPins.Write(PinValue.High, PinValue.High, PinValue.High, PinValue.High)

        Thread.Sleep(1000)

        device.GpPins.Write(PinValue.Low, PinValue.Low, PinValue.Low, PinValue.Low)

        ' blink GP0-GP3
        For Each gp In device.GpPins
          Console.WriteLine($"blink {gp.PinName}")

          For n = 0 To 9
            gp.Write(False)
            Thread.Sleep(100)

            gp.Write(True)
            Thread.Sleep(100)
          Next
        Next
      End Using
    End Using
  End Sub
End Class
