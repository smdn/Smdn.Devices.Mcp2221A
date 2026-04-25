# `InterruptOnChange_Polling`
This example shows how to configure the GP1 pin for Interrupt-on-Change (IOC) and detect changes in the pin's state through polling.

In this example, the task performing the polling is started. When a state change occurs, it sets the `ManualResetEventSlim` to notify the main thread of the change.
