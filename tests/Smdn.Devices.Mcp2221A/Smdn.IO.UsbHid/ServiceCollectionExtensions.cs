// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;

namespace Smdn.IO.UsbHid;

public static class ServiceCollectionExtensions {
  public static IServiceCollection AddPseudoUsbHid(
    this IServiceCollection services,
    PseudoUsbHidService pseudoUsbHidService
  )
    => AddPseudoUsbHid(
      services,
      (object?)null,
      pseudoUsbHidService
    );

  public static IServiceCollection AddPseudoUsbHid<TServiceKey>(
    this IServiceCollection services,
    TServiceKey serviceKey,
    PseudoUsbHidService pseudoUsbHidService
  )
  {
    if (services is null)
      throw new ArgumentNullException(nameof(services));

    if (serviceKey is null) {
      services.Add(
        ServiceDescriptor.Singleton<IUsbHidService>(
          (serviceProvider) => pseudoUsbHidService
        )
      );
    }
    else {
      services.Add(
        ServiceDescriptor.KeyedSingleton<IUsbHidService>(
          serviceKey: serviceKey,
          (serviceProvider, serviceKey) => pseudoUsbHidService
        )
      );
    }

    return services;
  }
}
