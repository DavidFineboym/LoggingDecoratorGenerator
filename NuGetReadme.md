![image](https://github.com/DavidFineboym/LoggingDecoratorGenerator/actions/workflows/dotnet.yml/badge.svg?event=push)
# Logging Decorator Source Generator

Generates logger decorator class for an interface at compile time(*no runtime reflection*). Uses `Microsoft.Extensions.Logging.ILogger` to log.
- Logs method parameters and return value(can omit secrets from log using `[NotLoggedAttribute]`)
- Supports async methods
- Supports log level, event id, and event name override through attribute
- Can catch and log specific exceptions
- Can measure method duration for performance reporting either as metric or log message
- Follows [High-performance logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging) guidance

## Getting started

Use `[DecorateWithLogger]` attribute in `Fineboym.Logging.Attributes` namespace on an interface. In Visual Studio you can see the generated code in Solution Explorer if you expand Dependencies->Analyzers->Fineboym.Logging.Generator.

### Prerequisites

Latest version of Visual Studio 2022.

## Usage

```C#
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace SomeFolder.SomeSubFolder;

// Default log level is Debug, applied to all methods. Can be changed through attribute's constructor.
[DecorateWithLogger(ReportDurationAsMetric = false)]
public interface ISomeService
{
    int SomeMethod(DateTime someDateTime);

    // Override log level and event id. EventName is also supported.
    [LogMethod(Level = LogLevel.Information, EventId = 100, MeasureDuration = true)]
    Task<double?> SomeAsyncMethod(string? s);

    // By default, exceptions are not logged and there is no try-catch block around the method call.
    // If you want to log exceptions, use ExceptionToLog property.
    // Default log level for exceptions is Error and it can be changed through ExceptionLogLevel property.
    [LogMethod(ExceptionToLog = typeof(InvalidOperationException))]
    Task<string> AnotherAsyncMethod(int x);

    // You can omit secrets or PII from logs using [NotLogged] attribute.
    [return: NotLogged]
    string GetMySecretString(string username, [NotLogged] string password);
}
```
This will create a generated class named `SomeServiceLoggingDecorator` in the same namespace as the interface.
You can see the generated code example for above interface on [GitHub README](https://github.com/DavidFineboym/LoggingDecoratorGenerator).

#### Duration as metric
Reporting duration of methods as a metric has an advantage of being separated from logs, so you can enable one without the other.
For example, metrics can be collected ad-hoc by [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-collection#view-metrics-with-dotnet-counters) tool or Prometheus.
Only if `ReportDurationAsMetric` is `true`, then [IMeterFactory](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.imeterfactory) is required in the decorator class constructor.
For the example above, name of the meter will be `decorated.GetType().ToString()` where `ISomeService decorated` is constructor parameter to `SomeServiceLoggingDecorator`.
Name of the instrument is always `"logging_decorator.method.duration"` and type is [Histogram\<double\>](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.histogram-1).
For more info, see [ASP.NET Core metrics](https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/metrics), [.NET observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel).

## Additional documentation

If you use .NET dependency injection, then you can decorate your service interface. You can do it yourself or use [Scrutor](https://github.com/khellang/Scrutor).
Here is an explanation [Adding decorated classes to the ASP.NET Core DI container using Scrutor](https://andrewlock.net/adding-decorated-classes-to-the-asp.net-core-di-container-using-scrutor).
If you're not familiar with Source Generators, read [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview).

## Limitations

Currently it supports non-generic interfaces, only with methods as its members and up to 6 parameters in a method which is what 
[LoggerMessage.Define Method](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define?view=dotnet-plat-ext-7.0) 
supports. To work around 6 parameters limitation, you can encapsulate some
parameters in a class or a struct or omit them from logging using `[NotLogged]` attribute.

## Feedback

Please go to [GitHub repository](https://github.com/DavidFineboym/LoggingDecoratorGenerator) for feedback. Feel free to open issues for questions, bugs, and improvements and I'll try to address them as soon as I can. Thank you.