// SPDX-FileCopyrightText: 2021 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Threading.Tasks;

using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.FilteringMode;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.Devices.Mcp2221A.Peripherals.I2c;

using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

await using var device = await Mcp2221A.CreateAsync(serviceProvider);

await device.GP3.ConfigureAsLedI2cAsync();

var i2cDevice = device.I2c
  .CreateDevice(Bme280.DefaultI2cAddress /* or Bme280.SecondaryI2cAddress */)
  .WithStandardMode();

var bme280 = new Bme280(
  i2cDevice: i2cDevice
) {
  TemperatureSampling = Sampling.LowPower,
  PressureSampling = Sampling.LowPower,
  HumiditySampling = Sampling.LowPower,
};

var initialCursorPosition = (left: Console.CursorLeft, top: Console.CursorTop);

while (true) {
  Console.SetCursorPosition(initialCursorPosition.left, initialCursorPosition.top);

  var measuredValue = await bme280.ReadAsync();

  Console.WriteLine($"{"Temperature",20}: {measuredValue.Temperature?.DegreesCelsius:N1} [℃]");
  Console.WriteLine($"{"Humidity",20}: {measuredValue.Humidity?.Percent:N1} [%]");
  Console.WriteLine($"{"Atmospheric pressure",20}: {measuredValue.Pressure?.Hectopascals:N0} [hPa]");

  if (bme280.TryReadAltitude(WeatherHelper.MeanSeaLevel, out var altitude))
    Console.WriteLine($"{"Altitude",20}: {altitude.Meters:N1} [m]");

  await Task.Delay(TimeSpan.FromSeconds(1.0));
}
