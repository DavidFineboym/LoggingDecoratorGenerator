using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;
using OtherFolder.OtherSubFolder;

namespace LoggingDecoratorGenerator.IntegrationTests
{
    [DecorateWithLogger(LogLevel.Information)]
    public interface IInformationLevelInterface
    {
        ValueTask<float> MethodWithoutAttribute(int x, int y);

        [LogMethod(Level = LogLevel.Debug, EventName = "SomePersonEventName", EventId = 100)]
        Person MethodWithAttribute(Person person, int someNumber);

        void MethodShouldNotBeLoggedBecauseOfLogLevel();

        [LogMethod(MeasureDuration = true, ExceptionToLog = typeof(Exception), ExceptionLogLevel = LogLevel.Critical)]
        Task<Person> MethodWithMeasuredDurationAsync(DateOnly someDate);

        [LogMethod(ExceptionToLog = typeof(Exception), EventId = 777, MeasureDuration = true)]
        Task MethodThrowsAndLogsExceptionAsync();
    }
}

namespace OtherFolder.OtherSubFolder
{
    public record Person(string Name, int Age);
}
