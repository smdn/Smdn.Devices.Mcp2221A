// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT

using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Smdn.Devices.Mcp2221A;
using Smdn.IO.UsbHid.DependencyInjection;

var services = new ServiceCollection();

services.AddHidSharpUsbHid();

using var serviceProvider = services.BuildServiceProvider();

using var device = Mcp2221AController.Create(serviceProvider);

// Configure GP1 as an Interrupt-on-Change.
device.GpPin1.ConfigureAsInterruptOnChange(
  // By specifying `InterruptOnChangeTrigger`, you can specify
  // which edge (rising, falling, or both) to detect.
  detectionTrigger: InterruptOnChangeTrigger.Both,
  // You can also specify whether to clear the detection status
  // at the same time as the GP1 configuration.
  clearDetectionFlag: true
);

// Configure GP2 and GP2 as GPIO output.
device.GpPin2.ConfigureAsGpioOutput(PinValue.Low);
device.GpPin3.ConfigureAsGpioOutput(PinValue.Low);

using var cts = new CancellationTokenSource();

// Requests cancellation to the CancellationToken when
// Ctrl+C is pressed.
Console.CancelKeyPress += (sender, e) => {
  cts.Cancel();
  e.Cancel = true;
};

using var edgeDetectedEvent = new ManualResetEventSlim(false);

// Start a background task to poll the interrupt status.
var pollingTask = Task.Run(
  cancellationToken: cts.Token,
  function: async () => {
    var cancellationToken = cts.Token;

    Console.WriteLine("Polling task started.");

    while (!cancellationToken.IsCancellationRequested) {
      // Check if an edge has been detected on GP1.
      // All command requests to the MCP2221A are internally thread-safe by
      // utilizing synchronization primitives.
      if (await device.GpPin1.ReadInterruptDetectionAsync(cancellationToken)) {
        Console.WriteLine("Edge detected!");

        // Signal the main loop that an event has occurred.
        edgeDetectedEvent.Set();

        // Clear the interrupt flag in the MCP2221A's SRAM.
        await device.GpPin1.ClearInterruptDetectionAsync(cancellationToken);
      }

      // Release the USB HID communication channel for a short period
      // to allow other operations.
      await Task.Delay(20, cancellationToken);
    }
  }
);

var stopToken = cts.Token;

try {
  var gp2BlinkValue = PinValue.Low;
  var gp3DetectionStateValue = PinValue.High;

  // Main loop: processes main logic and periodically updates pin states.
  while (!stopToken.IsCancellationRequested) {
    // Wait for the edge event with a short timeout to keep the loop responsive.
    if (edgeDetectedEvent.Wait(TimeSpan.FromMilliseconds(50), stopToken)) {
      edgeDetectedEvent.Reset();

      // Toggle the state assigned to GP3 when an edge is detected.
      gp3DetectionStateValue = !gp3DetectionStateValue;
    }

    // Toggle GP2 pin to demonstrate concurrent execution (blinking).
    gp2BlinkValue = !gp2BlinkValue;

    // Update physical pin states.
    // This operation safely waits if the polling task is currently using
    // the USB HID communication channel.
    device.GpPins.Write(
      gp2Value: gp2BlinkValue,
      gp3Value: gp3DetectionStateValue,
      cancellationToken: stopToken
    );
  }

  await pollingTask;
}
catch (OperationCanceledException ex) when (ex.CancellationToken == stopToken) {
  Console.WriteLine("Cancel key pressed.");
}
catch {
  throw; // unexpected exception
}
