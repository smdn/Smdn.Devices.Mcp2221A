// SPDX-FileCopyrightText: 2026 smdn <smdn@smdn.jp>
// SPDX-License-Identifier: MIT
using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

using NUnit.Framework;

namespace Smdn.Devices.Mcp2221A.DependencyInjection;

[TestFixture]
public class IServiceProviderLoggerExtensionsTests {
  [TestCase("key")]
  [TestCase(null)]
  public void GetKeyedLoggerOrCreate_ReturnsNullIfServiceProviderIsNull(string? serviceKey)
  {
    IServiceProvider? serviceProvider = null;

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(serviceKey),
      Is.Null
    );
  }

  [Test]
  public void GetKeyedLoggerOrCreate_GetsKeyedAndTypedILogger()
  {
    const string ServiceKey0 = nameof(ServiceKey0);
    const string ServiceKey1 = nameof(ServiceKey1);

    var services = new ServiceCollection();
    var loggerForServiceKey0 = new FakeLogger();
    var typedLoggerForServiceKey0 = new FakeLogger<Mcp2221AController>();
    var typedLoggerForServiceKey1 = new FakeLogger<Mcp2221AController>();

    services.AddKeyedSingleton<ILogger>(ServiceKey0, loggerForServiceKey0);
    services.AddKeyedSingleton<ILogger<Mcp2221AController>>(ServiceKey0, typedLoggerForServiceKey0);
    services.AddKeyedSingleton<ILogger<Mcp2221AController>>(ServiceKey1, typedLoggerForServiceKey1);

    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(ServiceKey0),
      Is.SameAs(typedLoggerForServiceKey0)
    );
  }

  [Test]
  public void GetKeyedLoggerOrCreate_GetsKeyedAndTypedILogger_ReturnsNullIfKeyIsNullAndNoILoggerFactoryRegistered()
  {
    const string ServiceKey0 = nameof(ServiceKey0);
    const string ServiceKey1 = nameof(ServiceKey1);

    var services = new ServiceCollection();

    services.AddKeyedSingleton<ILogger>(ServiceKey0, new FakeLogger());
    services.AddKeyedSingleton<ILogger<Mcp2221AController>>(ServiceKey0, new FakeLogger<Mcp2221AController>());
    services.AddKeyedSingleton<ILogger<Mcp2221AController>>(ServiceKey1, new FakeLogger<Mcp2221AController>());

    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(null),
      Is.Null
    );
  }

  [Test]
  public void GetKeyedLoggerOrCreate_GetsKeyedILogger()
  {
    const string ServiceKey0 = nameof(ServiceKey0);
    const string ServiceKey1 = nameof(ServiceKey1);

    var services = new ServiceCollection();
    var loggerForServiceKey0 = new FakeLogger();
    var loggerForServiceKey1 = new FakeLogger();

    services.AddKeyedSingleton<ILogger>(ServiceKey0, loggerForServiceKey0);
    services.AddKeyedSingleton<ILogger>(ServiceKey1, loggerForServiceKey1);

    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(ServiceKey0),
      Is.SameAs(loggerForServiceKey0)
    );
  }

  [Test]
  public void GetKeyedLoggerOrCreate_CreatesFromKeyedILoggerFactory()
  {
    const string ServiceKey0 = nameof(ServiceKey0);
    const string ServiceKey1 = nameof(ServiceKey1);

    var services = new ServiceCollection();

    services.AddKeyedSingleton<ILoggerFactory>(ServiceKey0, new LoggerFactory([new FakeLoggerProvider()]));
    services.AddKeyedSingleton<ILoggerFactory>(ServiceKey1, (provider, key) => throw new NotSupportedException());

    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(ServiceKey0),
      Is.InstanceOf<ILogger<Mcp2221AController>>()
    );
  }

  [TestCase("key")]
  [TestCase(null)]
  public void GetKeyedLoggerOrCreate_CreatesFromILoggerFactory(string? serviceKey)
  {
    var services = new ServiceCollection();

    services.AddSingleton<ILoggerFactory>(new LoggerFactory([new FakeLoggerProvider()]));

    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(serviceKey),
      Is.InstanceOf<ILogger<Mcp2221AController>>()
    );
  }

  [TestCase("key")]
  [TestCase(null)]
  public void GetKeyedLoggerOrCreate_ReturnsNullIfNoILoggerFactoryRegistered(string? serviceKey)
  {
    var services = new ServiceCollection();
    var serviceProvider = services.BuildServiceProvider();

    Assert.That(
      serviceProvider.GetKeyedLoggerOrCreate<Mcp2221AController>(serviceKey),
      Is.Null
    );
  }
}
