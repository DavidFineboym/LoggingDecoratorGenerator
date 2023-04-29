using FakeItEasy;
using Microsoft.Extensions.Logging;
using OtherFolder.OtherSubFolder;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LoggingDecoratorGenerator.IntegrationTests;

public partial class LoggingDecoratorTests
{
    [Fact]
    public void SynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        StringWriter textWriter = new();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }).SetMinimumLevel(LogLevel.Debug));

        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Trace));
        ISomeService fakeService = A.Fake<ISomeService>();
        ISomeService decorator = new SomeServiceLoggingDecorator(logger, fakeService);
        DateTime dateTimeParameter = new(year: 2022, month: 12, day: 12, hour: 22, minute: 57, second: 45, DateTimeKind.Utc);
        DateTime expectedReturnValue = new(year: 2020, month: 09, day: 06, hour: 00, minute: 00, second: 00, DateTimeKind.Utc);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).Returns(expectedReturnValue);

        // Act
        DateTime actualReturn = decorator.DateTimeReturningMethod(dateTimeParameter);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.DateTimeReturningMethod(dateTimeParameter)).MustHaveHappenedOnceExactly();

        string expectedConsoleOutput = """
            {
              "EventId": -1,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45",
              "State": {
                "Message": "Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45",
                "someDateTime": "12/12/2022 22:57:45",
                "{OriginalFormat}": "Entering DateTimeReturningMethod with parameters: someDateTime = {someDateTime}"
              }
            }
            {
              "EventId": -1,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00",
              "State": {
                "Message": "Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00",
                "result": "09/06/2020 00:00:00",
                "{OriginalFormat}": "Method DateTimeReturningMethod returned. Result = {result}"
              }
            }

            """.ReplaceLineEndings();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expectedConsoleOutput, consoleOutput);
    }

    [Fact]
    public async Task AsynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        StringWriter textWriter = new();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }));

        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        ISomeService fakeService = A.Fake<ISomeService>();
        ISomeService decorator = new SomeServiceLoggingDecorator(logger, fakeService);
        string inputParameter = "SomeInputParameter";
        string expectedReturnValue = "SomeReturnValue";
        A.CallTo(() => fakeService.StringReturningAsyncMethod(inputParameter)).Returns(expectedReturnValue);

        // Act
        string? actualReturn = await decorator.StringReturningAsyncMethod(inputParameter);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.StringReturningAsyncMethod(inputParameter)).MustHaveHappenedOnceExactly();

        string expectedConsoleOutput = """
            {
              "EventId": 0,
              "LogLevel": "Information",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter",
              "State": {
                "Message": "Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter",
                "s": "SomeInputParameter",
                "{OriginalFormat}": "Entering StringReturningAsyncMethod with parameters: s = {s}"
              }
            }
            {
              "EventId": 0,
              "LogLevel": "Information",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Method StringReturningAsyncMethod returned. Result = SomeReturnValue",
              "State": {
                "Message": "Method StringReturningAsyncMethod returned. Result = SomeReturnValue",
                "result": "SomeReturnValue",
                "{OriginalFormat}": "Method StringReturningAsyncMethod returned. Result = {result}"
              }
            }

            """.ReplaceLineEndings();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expectedConsoleOutput, consoleOutput);
    }

    [Fact]
    public void ISomeService_TwoMethodsWithSameName_WithIntegerParameter()
    {
        // Arrange
        StringWriter textWriter = new();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = true
            };
        }).SetMinimumLevel(LogLevel.Debug));

        ILogger<ISomeService> logger = loggerFactory.CreateLogger<ISomeService>();
        Assert.True(logger.IsEnabled(LogLevel.Debug));
        Assert.False(logger.IsEnabled(LogLevel.Trace));
        ISomeService fakeService = A.Fake<ISomeService>();
        ISomeService decorator = new SomeServiceLoggingDecorator(logger, fakeService);
        int inputParameter = 42;
        A.CallTo(() => fakeService.TwoMethodsWithSameName(inputParameter)).DoesNothing();

        // Act
        decorator.TwoMethodsWithSameName(inputParameter);

        // Assert
        A.CallTo(() => fakeService.TwoMethodsWithSameName(inputParameter)).MustHaveHappenedOnceExactly();

        string expectedConsoleOutput = """
            {
              "EventId": 333,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Entering TwoMethodsWithSameName with parameters: i = 42",
              "State": {
                "Message": "Entering TwoMethodsWithSameName with parameters: i = 42",
                "i": 42,
                "{OriginalFormat}": "Entering TwoMethodsWithSameName with parameters: i = {i}"
              }
            }
            {
              "EventId": 333,
              "LogLevel": "Debug",
              "Category": "LoggingDecoratorGenerator.IntegrationTests.ISomeService",
              "Message": "Method TwoMethodsWithSameName returned",
              "State": {
                "Message": "Method TwoMethodsWithSameName returned",
                "{OriginalFormat}": "Method TwoMethodsWithSameName returned"
              }
            }

            """.ReplaceLineEndings();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expectedConsoleOutput, consoleOutput);
    }

    [Fact]
    public async Task IInformationLevelInterface_MethodWithoutAttribute()
    {
        // Arrange
        StringWriter textWriter = new();
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
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.MethodWithoutAttribute(x, y)).MustHaveHappenedOnceExactly();

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

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expected: expectedConsoleOutput, actual: consoleOutput);
    }

    [Fact]
    public void IInformationLevelInterface_MethodWithAttribute()
    {
        // Arrange
        StringWriter textWriter = new();
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
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.MethodWithAttribute(firstInput, secondInput)).MustHaveHappenedOnceExactly();

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

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expected: expectedConsoleOutput, actual: consoleOutput);
    }

    [Fact]
    public void IInformationLevelInterface_MethodShouldNotBeLoggedBecauseOfLogLevel()
    {
        // Arrange
        StringWriter textWriter = new();
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
        A.CallTo(() => fakeService.MethodShouldNotBeLoggedBecauseOfLogLevel()).MustHaveHappenedOnceExactly();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.Equal(expected: string.Empty, actual: consoleOutput);
    }

    [Fact]
    public async Task IInformationLevelInterface_MethodWithMeasuredDurationAsync()
    {
        // Arrange
        StringWriter textWriter = new();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = false
            };
        }).SetMinimumLevel(LogLevel.Information));

        ILogger<IInformationLevelInterface> logger = loggerFactory.CreateLogger<IInformationLevelInterface>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        IInformationLevelInterface fakeService = A.Fake<IInformationLevelInterface>();
        IInformationLevelInterface decorator = new InformationLevelInterfaceLoggingDecorator(logger, fakeService);
        DateOnly inputParam = DateOnly.FromDayNumber(1_000);
        Person expectedReturnValue = new("bar", 42);
        A.CallTo(() => fakeService.MethodWithMeasuredDurationAsync(inputParam)).Returns(expectedReturnValue);

        // Act
        Person actualReturn = await decorator.MethodWithMeasuredDurationAsync(inputParam);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => fakeService.MethodWithMeasuredDurationAsync(inputParam)).MustHaveHappenedOnceExactly();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.NotNull(consoleOutput);
        string[] twoLines = consoleOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(expected: 2, actual: twoLines.Length);
        Assert.Equal(
            expected: /*lang=json,strict*/ """{"EventId":-1,"LogLevel":"Information","Category":"LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface","Message":"Entering MethodWithMeasuredDurationAsync with parameters: someDate = 09/28/0003","State":{"Message":"Entering MethodWithMeasuredDurationAsync with parameters: someDate = 09/28/0003","someDate":"09/28/0003","{OriginalFormat}":"Entering MethodWithMeasuredDurationAsync with parameters: someDate = {someDate}"}}""",
            actual: twoLines[0]);
        Assert.Matches(expectedRegex: DurationRegex(), actualString: twoLines[1]);
    }

    [GeneratedRegex(
        @"\{""EventId"":-1,""LogLevel"":""Information"",""Category"":""LoggingDecoratorGenerator\.IntegrationTests\.IInformationLevelInterface"",""Message"":""Method MethodWithMeasuredDurationAsync returned\. Result = Person \{ Name = bar, Age = 42 \}\. DurationInMilliseconds = \d+(\.\d+)?"",""State"":\{""Message"":""Method MethodWithMeasuredDurationAsync returned\. Result = Person \{ Name = bar, Age = 42 \}\. DurationInMilliseconds = \d+(\.\d+)?"",""result"":""Person \{ Name = bar, Age = 42 \}"",""durationInMilliseconds"":\d+(\.\d+)?,""{OriginalFormat}"":""Method MethodWithMeasuredDurationAsync returned\. Result = {result}\. DurationInMilliseconds = {durationInMilliseconds}""\}\}",
        RegexOptions.ExplicitCapture)]
    private static partial Regex DurationRegex();

    [Fact]
    public async Task IInformationLevelInterface_MethodThrowsAndLogsExceptionAsync()
    {
        // Arrange
        StringWriter textWriter = new();
        Console.SetOut(textWriter);
        ILoggerFactory loggerFactory = LoggerFactory.Create(static builder => builder.AddJsonConsole(options =>
        {
            options.JsonWriterOptions = new JsonWriterOptions
            {
                Indented = false
            };
        }).SetMinimumLevel(LogLevel.Information));

        ILogger<IInformationLevelInterface> logger = loggerFactory.CreateLogger<IInformationLevelInterface>();
        Assert.True(logger.IsEnabled(LogLevel.Information));
        Assert.False(logger.IsEnabled(LogLevel.Debug));
        IInformationLevelInterface fakeService = A.Fake<IInformationLevelInterface>();
        IInformationLevelInterface decorator = new InformationLevelInterfaceLoggingDecorator(logger, fakeService);
        InvalidOperationException expectedException = new("someMessage");
        A.CallTo(() => fakeService.MethodThrowsAndLogsExceptionAsync()).ThrowsAsync(expectedException);

        // Act and Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => decorator.MethodThrowsAndLogsExceptionAsync());
        Assert.Equal(expectedException, actualException);
        A.CallTo(() => fakeService.MethodThrowsAndLogsExceptionAsync()).MustHaveHappenedOnceExactly();

        loggerFactory.Dispose();

        string? consoleOutput = textWriter.ToString();
        Assert.NotNull(consoleOutput);
        string[] twoLines = consoleOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(expected: 2, actual: twoLines.Length);
        Assert.Equal(
            expected: /*lang=json,strict*/ """{"EventId":777,"LogLevel":"Information","Category":"LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface","Message":"Entering MethodThrowsAndLogsExceptionAsync","State":{"Message":"Entering MethodThrowsAndLogsExceptionAsync","{OriginalFormat}":"Entering MethodThrowsAndLogsExceptionAsync"}}""",
            actual: twoLines[0]);

        Assert.StartsWith(
            """{"EventId":777,"LogLevel":"Error","Category":"LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface","Message":"MethodThrowsAndLogsExceptionAsync failed","Exception":"System.InvalidOperationException: someMessage    at LoggingDecoratorGenerator.IntegrationTests.InformationLevelInterfaceLoggingDecorator.MethodThrowsAndLogsExceptionAsync() in """,
            twoLines[1]);

        Assert.EndsWith(
            """InformationLevelInterfaceLoggingDecorator.g.cs:line 166","State":{"Message":"MethodThrowsAndLogsExceptionAsync failed","{OriginalFormat}":"MethodThrowsAndLogsExceptionAsync failed"}}""",
            twoLines[1]);
    }
}