using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class LoggingDecoratorTests
{
    [Fact]
    public void SynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
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
        DateTime expectedReturnValue = new(year: 2020, month: 09, day: 06, hour: 00, minute: 00, second: 00, DateTimeKind.Utc);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).Returns(expectedReturnValue);

        // Act
        DateTime actualReturn = decorator.DateTimeReturningMethod(dateTimeParameter);

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        textWriter.Flush();
        string? consoleOutput = textWriter.ToString();
        Assert.Equal(
            "info: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]\r\n      Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45\r\ninfo: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]\r\n      Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00\r\n",
            consoleOutput);
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task AsynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddSimpleConsole());
        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        ISomeService fakeService = A.Fake<ISomeService>();
        ISomeService decorator = new SomeServiceLoggingDecorator(logger, fakeService);
        string inputParameter = "SomeInputParameter";
        string expectedReturnValue = "SomeReturnValue";
        A.CallTo(() => fakeService.StringReturningAsyncMethod(inputParameter)).Returns(expectedReturnValue);

        // Act
        string? actualReturn = await decorator.StringReturningAsyncMethod(inputParameter);

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        textWriter.Flush();
        string? consoleOutput = textWriter.ToString();
        Assert.Equal(
            "info: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]\r\n      Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter\r\ninfo: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]\r\n      Method StringReturningAsyncMethod returned. Result = SomeReturnValue\r\n",
            consoleOutput);
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.StringReturningAsyncMethod(inputParameter)).MustHaveHappenedOnceExactly();
    }
}