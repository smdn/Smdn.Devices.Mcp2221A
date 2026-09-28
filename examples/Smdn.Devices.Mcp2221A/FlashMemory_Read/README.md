# `FlashMemory_Read`
This example shows how to read settings stored in the Flash memory of the MCP2221/MCP2221A.

When an `Mcp2221AController` instance is created, it reads the settings (the register values of `Chip settings` and `GP settings`) written to flash memory and stores them in the local machine's memory as default settings.
These settings can be accessed via the `Mcp2221AController.Flash` property.
