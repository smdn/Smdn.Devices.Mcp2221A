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
        device.GP0.ConfigureAsGPIO(PinMode.Output)
        device.GP1.ConfigureAsGPIO(PinMode.Output)
        device.GP2.ConfigureAsGPIO(PinMode.Output)
        device.GP3.ConfigureAsGPIO(PinMode.Output, PinValue.Low) ' initial value also can be specified

        ' set GPIO pin values
        Console.WriteLine("set all GPs HIGH")

        device.GPs(0).SetValue(1) ' set GP0 to HIGH with integer value (0 = LOW, any other value = HIGH)

        device.GPs(1).SetValue(True) ' set GP1 to HIGH with boolean value

        device.GP2.SetValue(CByte(1)) ' set GP2 to HIGH with byte value

        Dim gp3Value As PinValue = 1

        device.GP3.SetValue(gp3Value) ' set GP3 to HIGH with struct PinValue

        Thread.Sleep(1000)

        Console.WriteLine("set all GPs LOW")

        ' GP0-GP3 also can be accessed via `GPs` read-only collection property
        For Each gp In device.GPs
          gp.SetValue(PinValue.Low)
        Next

        Thread.Sleep(1000)

        ' blink GP0-GP3
        For Each gp In device.GPs
          Console.WriteLine($"blink {gp.PinDesignation}")

          For n = 0 To 9
            gp.SetValue(False)
            Thread.Sleep(100)

            gp.SetValue(True)
            Thread.Sleep(100)
          Next
        Next
      End Using
    End Using
  End Sub
End Class
