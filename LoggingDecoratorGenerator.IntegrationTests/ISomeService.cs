using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime someDateTime);

    [LogMethod(Level = LogLevel.Information, EventId = 0)]
    Task<string?> StringReturningAsyncMethod(string? s);
}