using Fineboym.Logging.Generator;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime someDateTime);

    Task<string?> StringReturningAsyncMethod(string? s);
}