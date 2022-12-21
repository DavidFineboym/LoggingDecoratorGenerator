using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class LoggingDecoratorTests
{
    [Fact]
    public void DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddSimpleConsole());
        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        ISomeService fakeService = A.Fake<ISomeService>();
        ISomeService decorator = new SomeServiceLoggingDecorator(logger, fakeService);
        DateTime dateTimeParameter = new(year: 2022, month: 12, day: 12, hour: 22, minute: 57, second: 45, DateTimeKind.Utc);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).Returns(dateTimeParameter);

        // Act
        DateTime actualReturn = decorator.DateTimeReturningMethod(dateTimeParameter);

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        textWriter.Flush();
        string? consoleOutput = textWriter.ToString();
        Assert.Equal(
            "info: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]\r\n      Entering DateTimeReturningMethod with parameters: dateTime = 12/12/2022 22:57:45\r\n",
            consoleOutput);
        Assert.Equal(expected: dateTimeParameter, actual: actualReturn);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).MustHaveHappenedOnceExactly();
    }
}