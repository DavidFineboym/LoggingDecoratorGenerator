using FakeItEasy;
using Microsoft.Extensions.Logging;
using OtherFolder.OtherSubFolder;
using System.Text.Json;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class LoggingDecoratorTests
{
    [Fact]
    public void SynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddSimpleConsole().SetMinimumLevel(LogLevel.Debug));
        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Debug));
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
        Console.Out.Flush();
        string? consoleOutput = textWriter.ToString();
        Assert.Equal(
            $"dbug: LoggingDecoratorGenerator.IntegrationTests.ISomeService[-1]{Environment.NewLine}      Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45{Environment.NewLine}dbug: LoggingDecoratorGenerator.IntegrationTests.ISomeService[-1]{Environment.NewLine}      Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00{Environment.NewLine}",
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
        Console.Out.Flush();
        string? consoleOutput = textWriter.ToString();
        Assert.Equal(
            $"info: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]{Environment.NewLine}      Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter{Environment.NewLine}info: LoggingDecoratorGenerator.IntegrationTests.ISomeService[0]{Environment.NewLine}      Method StringReturningAsyncMethod returned. Result = SomeReturnValue{Environment.NewLine}",
            consoleOutput);
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.StringReturningAsyncMethod(inputParameter)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task IInformationLevelInterface_MethodWithoutAttribute()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }));
        ILogger<IInformationLevelInterface> logger = loggerFactory.CreateLogger<IInformationLevelInterface>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        IInformationLevelInterface fakeService = A.Fake<IInformationLevelInterface>();
        IInformationLevelInterface decorator = new InformationLevelInterfaceLoggingDecorator(logger, fakeService);
        int x = 42;
        int y = 43;
        float expectedReturnValue = 42.43f;
        A.CallTo(() => fakeService.MethodWithoutAttribute(x, y)).Returns(expectedReturnValue);

        // Act
        float actualReturn = await decorator.MethodWithoutAttribute(x, y);

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        Console.Out.Flush();
        string? consoleOutput = textWriter.ToString();

        string expectedConsoleOutput = """
            {
              "EventId": -1,
              "LogLevel": "Information",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface",
              "Message": "Entering MethodWithoutAttribute with parameters: x = 42, y = 43",
              "State": {
                "Message": "Entering MethodWithoutAttribute with parameters: x = 42, y = 43",
                "x": 42,
                "y": 43,
                "{OriginalFormat}": "Entering MethodWithoutAttribute with parameters: x = {x}, y = {y}"
              }
            }
            {
              "EventId": -1,
              "LogLevel": "Information",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface",
              "Message": "Method MethodWithoutAttribute returned. Result = 42.43",
              "State": {
                "Message": "Method MethodWithoutAttribute returned. Result = 42.43",
                "result": 42.43,
                "{OriginalFormat}": "Method MethodWithoutAttribute returned. Result = {result}"
              }
            }
            
            """.ReplaceLineEndings();
        Assert.Equal(expected: expectedConsoleOutput, actual: consoleOutput);

        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.MethodWithoutAttribute(x, y)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IInformationLevelInterface_MethodWithAttribute()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }).SetMinimumLevel(LogLevel.Debug));

        ILogger<IInformationLevelInterface> logger = loggerFactory.CreateLogger<IInformationLevelInterface>();
        Assert.True(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Trace));
        IInformationLevelInterface fakeService = A.Fake<IInformationLevelInterface>();
        IInformationLevelInterface decorator = new InformationLevelInterfaceLoggingDecorator(logger, fakeService);
        Person firstInput = new("foo", 30);
        int secondInput = 33;
        Person expectedReturnValue = new("bar", 42);
        A.CallTo(() => fakeService.MethodWithAttribute(firstInput, secondInput)).Returns(expectedReturnValue);

        // Act
        Person actualReturn = decorator.MethodWithAttribute(firstInput, secondInput);

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        Console.Out.Flush();
        string? consoleOutput = textWriter.ToString();

        string expectedConsoleOutput = """
            {
              "EventId": 100,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface",
              "Message": "Entering MethodWithAttribute with parameters: person = Person { Name = foo, Age = 30 }, someNumber = 33",
              "State": {
                "Message": "Entering MethodWithAttribute with parameters: person = Person { Name = foo, Age = 30 }, someNumber = 33",
                "person": "Person { Name = foo, Age = 30 }",
                "someNumber": 33,
                "{OriginalFormat}": "Entering MethodWithAttribute with parameters: person = {person}, someNumber = {someNumber}"
              }
            }
            {
              "EventId": 100,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface",
              "Message": "Method MethodWithAttribute returned. Result = Person { Name = bar, Age = 42 }",
              "State": {
                "Message": "Method MethodWithAttribute returned. Result = Person { Name = bar, Age = 42 }",
                "result": "Person { Name = bar, Age = 42 }",
                "{OriginalFormat}": "Method MethodWithAttribute returned. Result = {result}"
              }
            }
            
            """.ReplaceLineEndings();
        Assert.Equal(expected: expectedConsoleOutput, actual: consoleOutput);

        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.MethodWithAttribute(firstInput, secondInput)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public void IInformationLevelInterface_MethodShouldNotBeLoggedBecauseOfLogLevel()
    {
        // Arrange
        using TextWriter textWriter = new StringWriter();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }).SetMinimumLevel(LogLevel.Warning));

        ILogger<IInformationLevelInterface> logger = loggerFactory.CreateLogger<IInformationLevelInterface>();
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.False(logger.IsEnabled(LogLevel.Information));
        IInformationLevelInterface fakeService = A.Fake<IInformationLevelInterface>();
        IInformationLevelInterface decorator = new InformationLevelInterfaceLoggingDecorator(logger, fakeService);
        A.CallTo(() => fakeService.MethodShouldNotBeLoggedBecauseOfLogLevel()).DoesNothing();

        // Act
        decorator.MethodShouldNotBeLoggedBecauseOfLogLevel();

        // Assert
        // Must Dispose to flush the logger
        loggerFactory.Dispose();
        Console.Out.Flush();
        string? consoleOutput = textWriter.ToString();

        Assert.Equal(expected: string.Empty, actual: consoleOutput);

        A.CallTo(() => fakeService.MethodShouldNotBeLoggedBecauseOfLogLevel()).MustHaveHappenedOnceExactly();
    }
}