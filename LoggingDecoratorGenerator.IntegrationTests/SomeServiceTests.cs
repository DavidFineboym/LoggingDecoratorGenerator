using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class SomeServiceTests
{
    private readonly TestSink _testSink;
    private readonly ISomeService _fakeService;
    private readonly SomeServiceLoggingDecorator _decorator;

    public SomeServiceTests()
    {
        _testSink = new();
        TestLoggerFactory testLoggerFactory = new(_testSink, enabled: true);
        ILogger<ISomeService> logger = new Logger<ISomeService>(testLoggerFactory);
        _fakeService = A.Fake<ISomeService>();
        _decorator = new SomeServiceLoggingDecorator(logger, _fakeService);
    }

    [Fact]
    public void PassThroughMethodDoesNotCallLogger()
    {
        // Arrange
        ILogger<ISomeService> fakeLogger = A.Fake<ILogger<ISomeService>>();
        var decorator = new SomeServiceLoggingDecorator(fakeLogger, _fakeService);
        A.CallTo(() => _fakeService.Dispose()).DoesNothing();

        // Act
        decorator.Dispose();

        // Assert
        A.CallTo(() => _fakeService.Dispose()).MustHaveHappenedOnceExactly();
        A.CallTo(fakeLogger).MustNotHaveHappened();
    }

    [Fact]
    public void SynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        DateTime dateTimeParameter = new(year: 2022, month: 12, day: 12, hour: 22, minute: 57, second: 45, DateTimeKind.Utc);
        DateTime expectedReturnValue = new(year: 2020, month: 09, day: 06, hour: 00, minute: 00, second: 00, DateTimeKind.Utc);
        A.CallTo(() => _fakeService.DateTimeReturningMethod(dateTimeParameter)).Returns(expectedReturnValue);

        // Act
        DateTime actualReturn = _decorator.DateTimeReturningMethod(dateTimeParameter);

        // Assert
        Assert.Equal(expectedReturnValue, actualReturn);
        A.CallTo(() => _fakeService.DateTimeReturningMethod(dateTimeParameter)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(-1, firstWrite.EventId.Id);
        Assert.Equal("DateTimeReturningMethod", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.LoggerName);
        Assert.Equal("Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("someDateTime", dateTimeParameter),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering DateTimeReturningMethod with parameters: someDateTime = {someDateTime}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(-1, lastWrite.EventId.Id);
        Assert.Equal("DateTimeReturningMethod", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.LoggerName);
        Assert.Equal("Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method DateTimeReturningMethod returned. Result = {result}"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }

    [Fact]
    public void ParameterAndReturnValuesNotLogged()
    {
        // Arrange
        string username = "foo";
        string password = "bar";
        string expectedReturnValue = "returnValue";
        int x = 42;
        A.CallTo(() => _fakeService.GetMySecretString(username, password, x)).Returns(expectedReturnValue);

        // Act
        string actualReturn = _decorator.GetMySecretString(username, password, x);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.GetMySecretString(username, password, x)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(-1, firstWrite.EventId.Id);
        Assert.Equal("GetMySecretString", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.LoggerName);
        Assert.Equal("Entering GetMySecretString with parameters: username = foo, password = [REDACTED], x = 42", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("username", username),
            new KeyValuePair<string, object>("x", x),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering GetMySecretString with parameters: username = {username}, password = [REDACTED], x = {x}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(-1, lastWrite.EventId.Id);
        Assert.Equal("GetMySecretString", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.LoggerName);
        Assert.Equal("Method GetMySecretString returned. Result = [REDACTED]", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Method GetMySecretString returned. Result = [REDACTED]")
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }

    [Fact]
    public async Task AsynchronousMethod_DecoratorLogsAndCallsDecoratedInstance()
    {
        // Arrange
        string inputParameter = "SomeInputParameter";
        string expectedReturnValue = "SomeReturnValue";
        A.CallTo(() => _fakeService.StringReturningAsyncMethod(inputParameter)).Returns(expectedReturnValue);

        // Act
        string? actualReturn = await _decorator.StringReturningAsyncMethod(inputParameter);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.StringReturningAsyncMethod(inputParameter)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(0, firstWrite.EventId.Id);
        Assert.Equal("StringReturningAsyncMethod", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.LoggerName);
        Assert.Equal("Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("s", inputParameter),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering StringReturningAsyncMethod with parameters: s = {s}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(0, lastWrite.EventId.Id);
        Assert.Equal("StringReturningAsyncMethod", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.LoggerName);
        Assert.Equal("Method StringReturningAsyncMethod returned. Result = SomeReturnValue", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method StringReturningAsyncMethod returned. Result = {result}"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }

    [Fact]
    public void TwoMethodsWithSameName_WithIntegerParameter()
    {
        // Arrange
        int inputParameter = 42;
        A.CallTo(() => _fakeService.TwoMethodsWithSameName(inputParameter)).DoesNothing();

        // Act
        _decorator.TwoMethodsWithSameName(inputParameter);

        // Assert
        A.CallTo(() => _fakeService.TwoMethodsWithSameName(inputParameter)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(333, firstWrite.EventId.Id);
        Assert.Equal("WithIntegerParam", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.LoggerName);
        Assert.Equal("Entering TwoMethodsWithSameName with parameters: i = 42", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("i", inputParameter),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering TwoMethodsWithSameName with parameters: i = {i}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(333, lastWrite.EventId.Id);
        Assert.Equal("WithIntegerParam", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.LoggerName);
        Assert.Equal("Method TwoMethodsWithSameName returned", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Method TwoMethodsWithSameName returned"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }
}
