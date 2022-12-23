using Fineboym.Logging.Generator;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger]
public interface ISomeService
{
    DateTime DateTimeReturningMethod(DateTime dateTime);

    Task<string?> StringReturningAsyncMethod(string? s);
}