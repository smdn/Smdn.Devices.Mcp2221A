# MCP2221A + TM1637
This example demonstrates how to control a TM1637 LED driver connected to an MCP2221A, using `Iot.Device.Tm16xx.Tm1637`.

In this example, light up the 6-digit 7-segment LED connected to the TM1637 to display the current time.

The current time is displayed in the `HH.MM.SS.` format, with the trailing period blinking every 0.5 seconds.

Additionally, to improve data transmission efficiency, when updating the display every 0.5 seconds, only the data for the parts that have changed is transmitted.
