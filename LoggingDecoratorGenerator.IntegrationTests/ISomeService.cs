using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime someDateTime);

    [LogMethod(Level = LogLevel.Information, EventId = 0)]
    Task<string?> StringReturningAsyncMethod(string? s);

    [LogMethod(EventId = 222, EventName = "Parameterless")]
    void TwoMethodsWithSameName();

    [LogMethod(EventId = 333, EventName = "WithIntegerParam")]
    void TwoMethodsWithSameName(int i);
}