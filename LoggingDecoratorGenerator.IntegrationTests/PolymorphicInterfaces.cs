using Fineboym.Logging.Attributes;

namespace LoggingDecoratorGenerator.IntegrationTests
{
    public interface IBaseInterfaceWithoutAttribute
    {
        Task<int> PassThroughMethodAsync(int x, int y);

        [LogMethod(Level = Microsoft.Extensions.Logging.LogLevel.Trace, EventId = 7, ExceptionToLog = typeof(InvalidOperationException))]
        Task<string> MethodWithAttributeAsync(float num, [NotLogged] string secret);
    }

    [DecorateWithLogger()]
    public interface IBaseInterfaceWithAttribute : IBaseInterfaceWithoutAttribute
    {
        TimeOnly MethodWithoutAttribute(DateOnly someDate);
    }

    [DecorateWithLogger(level: Microsoft.Extensions.Logging.LogLevel.Information)]
    public interface IDerivedInterface : IBaseInterfaceWithAttribute
    {
        ValueTask<double> DerivedMethodAsync(int x);
    }
}
