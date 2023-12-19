![image](https://github.com/DavidFineboym/LoggingDecoratorGenerator/actions/workflows/dotnet.yml/badge.svg?event=push)
# Logging Decorator Source Generator

Generates logger decorator class for an interface at compile time(*no runtime reflection*). Uses `Microsoft.Extensions.Logging.ILogger` to log and requires it in decorator class constructor.
- Logs method parameters and return value(can omit secrets from log using `[NotLoggedAttribute]`)
- Supports async methods
- Supports log level, event id, and event name override through attribute
- Can catch and log specific exceptions
- Can measure method duration for performance reporting either as metric or log message
- Follows [High-performance logging in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging) guidance

## Getting started

Install the package from [NuGet](https://www.nuget.org/packages/Fineboym.Logging.Generator)

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
<details><summary>Click to see the generated code</summary>

```C#
#nullable enable

namespace SomeFolder.SomeSubFolder
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Fineboym.Logging.Generator", "1.10.0.0")]
    public sealed class SomeServiceLoggingDecorator : ISomeService
    {
        private readonly global::Microsoft.Extensions.Logging.ILogger<ISomeService> _logger;
        private readonly ISomeService _decorated;

        public SomeServiceLoggingDecorator(
            global::Microsoft.Extensions.Logging.ILogger<ISomeService> logger,
            ISomeService decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.DateTime, global::System.Exception?> s_beforeSomeMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.DateTime>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(15022964, nameof(SomeMethod)),
                "Entering SomeMethod with parameters: someDateTime = {someDateTime}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, global::System.Exception?> s_afterSomeMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(15022964, nameof(SomeMethod)),
                "Method SomeMethod returned. Result = {result}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public int SomeMethod(global::System.DateTime someDateTime)
        {
            var __logEnabled = _logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug);

            if (__logEnabled)
            {
                s_beforeSomeMethod(_logger, someDateTime, null);
            }

            var __result = _decorated.SomeMethod(someDateTime);

            if (__logEnabled)
            {
                s_afterSomeMethod(_logger, __result, null);
            }

            return __result;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, string?, global::System.Exception?> s_beforeSomeAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<string?>(
                global::Microsoft.Extensions.Logging.LogLevel.Information,
                new global::Microsoft.Extensions.Logging.EventId(100, nameof(SomeAsyncMethod)),
                "Entering SomeAsyncMethod with parameters: s = {s}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, double?, double?, global::System.Exception?> s_afterSomeAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<double?, double?>(
                global::Microsoft.Extensions.Logging.LogLevel.Information,
                new global::Microsoft.Extensions.Logging.EventId(100, nameof(SomeAsyncMethod)),
                "Method SomeAsyncMethod returned. Result = {result}. DurationInMilliseconds = {durationInMilliseconds}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.Task<double?> SomeAsyncMethod(string? s)
        {
            var __logEnabled = _logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Information);
            global::System.Int64 __startTimestamp = 0;

            if (__logEnabled)
            {
                s_beforeSomeAsyncMethod(_logger, s, null);
                __startTimestamp = global::System.Diagnostics.Stopwatch.GetTimestamp();
            }

            var __result = await _decorated.SomeAsyncMethod(s).ConfigureAwait(false);

            if (__logEnabled)
            {
                var __elapsedTime = global::System.Diagnostics.Stopwatch.GetElapsedTime(__startTimestamp);
                s_afterSomeAsyncMethod(_logger, __result, __elapsedTime.TotalMilliseconds, null);
            }

            return __result;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, global::System.Exception?> s_beforeAnotherAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(2017861863, nameof(AnotherAsyncMethod)),
                "Entering AnotherAsyncMethod with parameters: x = {x}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, string, global::System.Exception?> s_afterAnotherAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<string>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(2017861863, nameof(AnotherAsyncMethod)),
                "Method AnotherAsyncMethod returned. Result = {result}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.Task<string> AnotherAsyncMethod(int x)
        {
            var __logEnabled = _logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug);

            if (__logEnabled)
            {
                s_beforeAnotherAsyncMethod(_logger, x, null);
            }

            string __result;
            try
            {
                __result = await _decorated.AnotherAsyncMethod(x).ConfigureAwait(false);
            }
            catch (global::System.InvalidOperationException __e)
            {
                global::Microsoft.Extensions.Logging.LoggerExtensions.Log(
                    _logger,
                    global::Microsoft.Extensions.Logging.LogLevel.Error,
                    new global::Microsoft.Extensions.Logging.EventId(2017861863, nameof(AnotherAsyncMethod)),
                    __e,
                    "AnotherAsyncMethod failed");

                throw;
            }

            if (__logEnabled)
            {
                s_afterAnotherAsyncMethod(_logger, __result, null);
            }

            return __result;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, string, global::System.Exception?> s_beforeGetMySecretString
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<string>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(1921103492, nameof(GetMySecretString)),
                "Entering GetMySecretString with parameters: username = {username}, password = [REDACTED]",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterGetMySecretString
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(1921103492, nameof(GetMySecretString)),
                "Method GetMySecretString returned. Result = [REDACTED]",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public string GetMySecretString(string username, string password)
        {
            var __logEnabled = _logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug);

            if (__logEnabled)
            {
                s_beforeGetMySecretString(_logger, username, null);
            }

            var __result = _decorated.GetMySecretString(username, password);

            if (__logEnabled)
            {
                s_afterGetMySecretString(_logger, null);
            }

            return __result;
        }
    }
}

```

</details>

#### Duration as metric
Reporting duration of methods as a metric has an advantage of being separated from logs, so you can enable one without the other.
For example, metrics can be collected ad-hoc by [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-collection#view-metrics-with-dotnet-counters) tool or Prometheus.
It also has built-in statistical aggregation like percentiles.<br>
Only if `ReportDurationAsMetric` is `true`, then [IMeterFactory](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.imeterfactory) is required in the decorator class constructor.
For the example above, name of the meter will be `typeof(ISomeService).ToString()`.
Name of the instrument is always `"logging_decorator.method.duration"` and type is [Histogram](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.histogram-1).<br>
For more info, see [ASP.NET Core metrics](https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/metrics), [.NET observability with OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel).

## Additional documentation

If you use .NET dependency injection, then you can decorate your service interface. You can do it yourself or use [Scrutor](https://github.com/khellang/Scrutor).
Here is an explanation [Adding decorated classes to the ASP.NET Core DI container using Scrutor](https://andrewlock.net/adding-decorated-classes-to-the-asp.net-core-di-container-using-scrutor).<br>
If you're not familiar with Source Generators, read [Source Generators](https://learn.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview).

## Limitations

Currently it supports non-generic interfaces, only with methods as its members and up to 6 parameters in a method which is what 
[LoggerMessage.Define Method](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define?view=dotnet-plat-ext-7.0) 
supports. To work around 6 parameters limitation, you can encapsulate some
parameters in a class or a struct or omit them from logging using `[NotLogged]` attribute.

## Feedback

Feel free to open issues here for questions, bugs, and improvements and I'll try to address them as soon as I can. Thank you.