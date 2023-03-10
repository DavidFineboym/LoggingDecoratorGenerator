using Fineboym.Logging.Attributes;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime someDateTime);

    Task<string?> StringReturningAsyncMethod(string? s);
}