# `FlashMemory_Write`
This example demonstrates how to stage configuration changes and persist them to the non-volatile Flash memory of the MCP2221A device.

It covers the complete workflow for updating Flash settings - including write protection management, password authentication, local staging, change detection, hardware persistence, and device resetting.

## Key Features & Workflow

### 1. Local Staging Model & Modification APIs
Flash configuration changes are not committed directly to the physical device when calling modification methods. Instead, they are staged in local memory:

- **Parameter Modifications**: Modify chip settings such as CDC serial number enumeration, clock output frequency/duty cycle, DAC/ADC voltage references, initial DAC output, interrupt triggers, USB power attributes, and GP pin assignments using fluent `ModifyXxx` methods.
- **Change Detection (`IsDirty`)**: Check `Mcp2221AController.Flash.IsDirty` to verify whether unwritten staged changes or pending password modifications exist before issuing a write command.
- **Restoring Staged Buffer (`Restore`)**: Discard staged changes and revert the local buffer back to the initial state captured at instance creation using `Mcp2221AController.Flash.Restore()`.

### 2. Password Protection & Authentication
If the physical device is password-protected, or if you are enabling password protection, two distinct password methods are required:

1. **`SendAccessPasswordAsync()` (Hardware Authentication)**: An immediate command sent to the physical device to temporarily unlock hardware Flash write access.
2. **`ModifyPassword()` (Buffer Staging)**: Prepares the exact 8-byte password payload for the monolithic 64-byte 'Write Chip Settings' command.

> **Why two separate calls are necessary**: The MCP2221A writes protection settings and password bytes simultaneously in a single command, but existing passwords cannot be read back from the device. To prevent accidentally overwriting the Flash password with indeterminate internal buffer data (which would result in a permanent lockout), the library enforces an explicit `ModifyPassword()` call whenever `PasswordProtected` level is active.

### 3. Persisting Changes (`Write`/`WriteAsync`)
Calling `Mcp2221AController.Flash.WriteAsync()` transmits the staged configuration and password buffer to the physical Flash memory.

#### Operational Cautions:
- **Flash Write Endurance Limit**: Physical Flash memory has a maximum write endurance limit. Ensure write operations are minimized and avoid repetitive automated write or rollback routines.
- **USB Vendor ID (VID) and Product ID (PID) Modifications**: Modifying the USB VID/PID carries severe risks, including USB specification non-compliance (USB-IF regulations), OS driver conflicts, and potential device bricking. Keep the default VID (`0x04D8`) / PID (`0x00DD`) for testing, or use Microchip's official PID Sub-licensing Program for custom products.

### 4. Applying Settings via Device Reset
Updating Flash memory settings does not immediately alter the active SRAM operating parameters.

- **Device Reset (`ResetAsync`)**: Issues a reset command so the hardware reloads updated Flash settings into SRAM upon restart.
- **Delayed Password Activation**: Password protection level changes take effect only after a device reset or power cycle.
- **Instance Discard**: Resetting the device causes USB reconnection, rendering the current `Mcp2221AController` instance disposed. Any further operations require instantiating a new controller instance.
