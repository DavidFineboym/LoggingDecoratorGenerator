![image](https://github.com/DavidFineboym/LoggingDecoratorGenerator/actions/workflows/dotnet.yml/badge.svg?event=push)
# Logging Decorator Generator

Generates logger decorator class for an interface. Uses `Microsoft.Extensions.Logging.ILogger<{Your interface}>` to log and requires it in decorator class constructor.
Logs method parameters and return value. Supports async methods. Supports log level, event id, and event name override through attribute.
Can catch and log specific exceptions.
Can measure method duration for performance reporting.
Follows high-performance logging guidance by .NET team-> https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging

## Getting started

NuGet: https://www.nuget.org/packages/Fineboym.Logging.Generator/

Use `[DecorateWithLogger]` attribute in `Fineboym.Logging.Attributes` namespace on an interface. In Visual Studio you can see the generated code in Solution Explorer if you expand Dependencies->Analyzers->Fineboym.Logging.Generator.

### Prerequisites

Latest version of Visual Studio 2022.

## Usage

```C#
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace SomeFolder.SomeSubFolder;

// Default log level is Debug, applied to all methods. Can be changed through attribute's constructor.
[DecorateWithLogger]
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
}
```
This will create a generated class named `SomeServiceLoggingDecorator` in the same namespace as the interface.

## Additional documentation

If you use .NET dependency injection, then you can decorate your service interface using, for example, Scrutor-> https://github.com/khellang/Scrutor
Go here for explanation-> https://andrewlock.net/adding-decorated-classes-to-the-asp.net-core-di-container-using-scrutor/

## Limitations

Currently it supports simple interfaces, only with methods as its members and up to 6 parameters in a method which is what `LoggerMessage.Define` supports (https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage.define?view=dotnet-plat-ext-7.0).

## Feedback

Please go to https://github.com/DavidFineboym/LoggingDecoratorGenerator for feedback. Feel free to open issues for questions, bugs, and improvements and I'll try to address them as soon as I can. Thank you.