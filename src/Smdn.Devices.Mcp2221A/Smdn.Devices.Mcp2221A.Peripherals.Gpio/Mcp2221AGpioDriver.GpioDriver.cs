// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;
using System.Device.Gpio;
using System.Threading;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class Mcp2221AGpioDriver : GpioDriver {
#pragma warning restore IDE0040
  /// <inheritdoc/>
  protected override int PinCount => NumberOfGpPins;

  private readonly bool[] isUsedByGpioController = new bool[NumberOfGpPins];

  internal bool IsUsedByGpioController(int gp)
    => isUsedByGpioController[ThrowIfIndexOfGpPinIsOutOfRange(gp, nameof(gp))];

  internal void ThrowIfUsedByGpioController(int gp)
  {
    if (IsUsedByGpioController(gp)) {
      throw new InvalidOperationException(
        message: $"The GP{gp} is currently being used by a {nameof(Mcp2221AController.GpioController)}. Close the pin with the {nameof(Mcp2221AController.GpioController)} before performing this operation."
      );
    }
  }

  /// <inheritdoc/>
  /// <remarks>
  /// Although <see cref="GpioDriver"/> requires an implementation of <see cref="IDisposable"/>,
  /// this class does not hold any objects whose lifecycle needs to be maintained,
  /// and since the lifecycle of this class itself is maintained by the <see cref="Mcp2221AController"/>,
  /// this method does nothing when called.
  /// </remarks>
  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
  }

  /// <inheritdoc/>
  protected override void OpenPin(int pinNumber)
  {
    Transceiver.ThrowIfDisposed();

    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    if (
      !isUsedByGpioController[pinNumber] &&
      GetCurrentGpDesignation(pinNumber) != GpDesignation.GpioOperation
    ) {
      SetGpSettings(
        allGpSettings: (
          pinNumber == 0 ? new(Designation: GpDesignation.GpioOperation) : default,
          pinNumber == 1 ? new(Designation: GpDesignation.GpioOperation) : default,
          pinNumber == 2 ? new(Designation: GpDesignation.GpioOperation) : default,
          pinNumber == 3 ? new(Designation: GpDesignation.GpioOperation) : default
        ),
        shouldThrowIfUsedByGpioController: false,
        cancellationToken: default
      );
    }

    isUsedByGpioController[pinNumber] = true;
  }

  /// <inheritdoc/>
  protected override void ClosePin(int pinNumber)
  {
    Transceiver.ThrowIfDisposed();

    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    isUsedByGpioController[pinNumber] = false;
  }

  /// <inheritdoc/>
  protected override bool IsPinModeSupported(
    int pinNumber,
    PinMode mode
  )
  {
    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    return mode switch {
      PinMode.Input => true,
      PinMode.Output => true,
      _ => false,
    };
  }

  /// <inheritdoc/>
  protected override PinMode GetPinMode(
    int pinNumber
  )
  {
    Span<PinModePair> pinModePairs = [
      new(
        ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber)),
        default
      )
    ];

    FetchGpioStates(
      pinValuePairs: default,
      pinModePairs: pinModePairs,
      cancellationToken: default
    );

    return pinModePairs[0].PinMode;
  }

  /// <inheritdoc/>
  protected override void SetPinMode(
    int pinNumber,
    PinMode mode
  )
    => ApplyGpioStates(
      pinValuePairs: default,
      pinModePairs: [
        new(
          ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber)),
          mode
        )
      ],
      shouldThrowIfUsedByGpioController: false,
      cancellationToken: default
    );

  /// <inheritdoc/>
  /// <remarks>
  /// This method is exposed in System.Device.Gpio version 1.5.0 or later.
  /// </remarks>
  protected override void SetPinMode(
    int pinNumber,
    PinMode mode,
    PinValue initialValue
  )
  {
    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    ApplyGpioStates(
      pinValuePairs: [new(pinNumber, initialValue)],
      pinModePairs: [new(pinNumber, mode)],
      shouldThrowIfUsedByGpioController: false,
      cancellationToken: default
    );
  }

  /// <inheritdoc/>
  protected override PinValue Read(
    int pinNumber
  )
  {
    Span<PinValuePair> pinValuePairs = [
      new(
        ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber)),
        default
      )
    ];

    FetchGpioStates(
      pinValuePairs: pinValuePairs,
      pinModePairs: default,
      cancellationToken: default
    );

    return pinValuePairs[0].PinValue;
  }

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
  /// <inheritdoc/>
  protected override void Read(
    Span<PinValuePair> pinValuePairs
  )
    => FetchGpioStates(
      pinValuePairs: pinValuePairs,
      pinModePairs: default,
      cancellationToken: default
    );
#endif

  internal void WriteWithoutModeCheck(
    int pinNumber,
    PinValue value
  )
    => Write(
      pinNumber: pinNumber,
      value: value
    );

  // <inheritdoc/>
  protected override void Write(
    int pinNumber,
    PinValue value
  )
    => ApplyGpioStates(
      pinValuePairs: [
        new(
          ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber)),
          value
        )
      ],
      pinModePairs: default,
      shouldThrowIfUsedByGpioController: false,
      cancellationToken: default
    );

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
  /// <inheritdoc/>
  /// <remarks>
  /// This method is exposed in System.Device.Gpio version 4.1.0 or later.
  /// </remarks>
  protected override void Write(
    ReadOnlySpan<PinValuePair> pinValuePairs
  )
    => ApplyGpioStates(
      pinValuePairs: pinValuePairs,
      pinModePairs: default,
      shouldThrowIfUsedByGpioController: false,
      cancellationToken: default
    );
#endif

#if SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
  /// <inheritdoc/>
  /// <remarks>
  /// This method is exposed in System.Device.Gpio version 3.0.0 or later.
  /// </remarks>
  protected override void Toggle(int pinNumber)
    => ApplyGpioStates(
      pinValuePairs: [
        new(
          ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber)),
          GetLastUpdatedValueOrThrow(pinNumber) == PinValue.Low
            ? PinValue.High
            : PinValue.Low
        )
      ],
      pinModePairs: default,
      shouldThrowIfUsedByGpioController: false,
      cancellationToken: default
    );
#endif

  /// <inheritdoc/>
  protected override WaitForEventResult WaitForEvent(
    int pinNumber,
    PinEventTypes eventTypes,
    CancellationToken cancellationToken
  )
  {
    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    // TODO
    throw new NotImplementedException();
  }

  /// <inheritdoc/>
  protected override void AddCallbackForPinValueChangedEvent(
    int pinNumber,
    PinEventTypes eventTypes,
    PinChangeEventHandler callback
  )
  {
    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    throw new NotImplementedException();
  }

  /// <inheritdoc/>
  protected override void RemoveCallbackForPinValueChangedEvent(
    int pinNumber,
    PinChangeEventHandler callback
  )
  {
    _ = ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));

    throw new NotImplementedException();
  }

#if !SYSTEM_DEVICE_GPIO_4_1_0_OR_GREATER
  /// <inheritdoc/>
  /// <remarks>
  /// <para>
  /// The pin numbers refer to index of GP pins, not physical GP pin numbers.
  /// Specifically, pin numbers <c>0</c>-<c>3</c> correspond to <c>GP0</c>-<c>GP3</c>.
  /// </para>
  /// <para>
  /// This method was deprecated and removed starting with System.Device.Gpio v4.1.0.
  /// </para>
  /// </remarks>
  /// <seealso cref="PinNumberingScheme.Logical"/>
  protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
    => ThrowIfIndexOfGpPinIsOutOfRange(pinNumber, nameof(pinNumber));
#endif
}
