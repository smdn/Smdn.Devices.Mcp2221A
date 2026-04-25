// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System.Threading;
using System.Threading.Tasks;

namespace Smdn.Devices.Mcp2221A.Peripherals.Gpio;

#pragma warning disable IDE0040
partial class Mcp2221AGpioDriver {
#pragma warning restore IDE0040
  public InterruptOnChangeTrigger CurrentInterruptOnChangeTrigger {
    get {
      var iocSetup = sramSettings.ReadInterruptDetectionModuleSetupByte();

      return
        ((iocSetup & 0b_0_00_0_1_0_0_0) == 0 ? InterruptOnChangeTrigger.None : InterruptOnChangeTrigger.Rising) |
        ((iocSetup & 0b_0_00_0_0_0_1_0) == 0 ? InterruptOnChangeTrigger.None : InterruptOnChangeTrigger.Falling);
    }
  }

  public bool LastFetchedInterruptDetectionFlag { get; private set; }

  public ClockOutputFrequency CurrentClockOutputFrequency
    => (ClockOutputFrequency)(sramSettings.ReadClockOutputDividerValueByte() & 0b_0_00_00_111);

  public ClockOutputDutyCycle CurrentClockOutputDutyCycle
    => (ClockOutputDutyCycle)((sramSettings.ReadClockOutputDividerValueByte() & 0b_0_00_11_000) >> 3);

  public bool FetchInterruptDetectionFlag(
    CancellationToken cancellationToken
  )
  {
    FetchGpPinInputs(cancellationToken);

    return LastFetchedInterruptDetectionFlag;
  }

  public async ValueTask<bool> FetchInterruptDetectionFlagAsync(
    CancellationToken cancellationToken
  )
  {
    await FetchGpPinInputsAsync(cancellationToken).ConfigureAwait(false);

    return LastFetchedInterruptDetectionFlag;
  }
}
