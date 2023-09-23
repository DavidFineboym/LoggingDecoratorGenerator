using FakeItEasy;
using Microsoft.Extensions.Logging;
using OtherFolder.OtherSubFolder;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class InformationLevelInterfaceTests
{
    private readonly TestSink _testSink;
    private readonly IInformationLevelInterface _fakeService;
    private readonly InformationLevelInterfaceLoggingDecorator _decorator;

    public InformationLevelInterfaceTests()
    {
        _testSink = new();
        TestLoggerFactory testLoggerFactory = new(_testSink, enabled: true);
        ILogger<IInformationLevelInterface> logger = new Logger<IInformationLevelInterface>(testLoggerFactory);
        _fakeService = A.Fake<IInformationLevelInterface>();
        _decorator = new InformationLevelInterfaceLoggingDecorator(logger, _fakeService);
    }

    [Fact]
    public async Task MethodWithoutAttribute()
    {
        // Arrange
        int x = 42;
        int y = 43;
        float expectedReturnValue = 42.43f;
        A.CallTo(() => _fakeService.MethodWithoutAttribute(x, y)).Returns(expectedReturnValue);

        // Act
        float actualReturn = await _decorator.MethodWithoutAttribute(x, y);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.MethodWithoutAttribute(x, y)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(1514124652, firstWrite.EventId.Id);
        Assert.Equal("MethodWithoutAttribute", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.LoggerName);
        Assert.Equal("Entering MethodWithoutAttribute with parameters: x = 42, y = 43", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("x", x),
            new KeyValuePair<string, object>("y", y),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithoutAttribute with parameters: x = {x}, y = {y}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(1514124652, lastWrite.EventId.Id);
        Assert.Equal("MethodWithoutAttribute", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.LoggerName);
        Assert.Equal("Method MethodWithoutAttribute returned. Result = 42.43", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithoutAttribute returned. Result = {result}"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }

    [Fact]
    public void MethodWithAttribute()
    {
        // Arrange
        Person firstInput = new("foo", 30);
        int secondInput = 33;
        Person expectedReturnValue = new("bar", 42);
        A.CallTo(() => _fakeService.MethodWithAttribute(firstInput, secondInput)).Returns(expectedReturnValue);

        // Act
        Person actualReturn = _decorator.MethodWithAttribute(firstInput, secondInput);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.MethodWithAttribute(firstInput, secondInput)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(100, firstWrite.EventId.Id);
        Assert.Equal("SomePersonEventName", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.LoggerName);
        Assert.Equal("Entering MethodWithAttribute with parameters: person = Person { Name = foo, Age = 30 }, someNumber = 33", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("person", firstInput),
            new KeyValuePair<string, object>("someNumber", secondInput),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithAttribute with parameters: person = {person}, someNumber = {someNumber}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(100, lastWrite.EventId.Id);
        Assert.Equal("SomePersonEventName", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.LoggerName);
        Assert.Equal("Method MethodWithAttribute returned. Result = Person { Name = bar, Age = 42 }", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithAttribute returned. Result = {result}"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }

    [Fact]
    public void MethodShouldNotBeLoggedBecauseOfLogLevel()
    {
        // Arrange
        ILogger<IInformationLevelInterface> fakeLogger = A.Fake<ILogger<IInformationLevelInterface>>();
        var decorator = new InformationLevelInterfaceLoggingDecorator(fakeLogger, _fakeService);
        A.CallTo(() => fakeLogger.IsEnabled(LogLevel.Information)).Returns(false);
        A.CallTo(() => _fakeService.MethodShouldNotBeLoggedBecauseOfLogLevel()).DoesNothing();

        // Act
        decorator.MethodShouldNotBeLoggedBecauseOfLogLevel();

        // Assert
        A.CallTo(() => _fakeService.MethodShouldNotBeLoggedBecauseOfLogLevel()).MustHaveHappenedOnceExactly();
        A.CallTo(() => fakeLogger.IsEnabled(LogLevel.Information)).MustHaveHappenedOnceExactly();
        A.CallTo(fakeLogger).Where(call => call.Method.Name == nameof(ILogger.Log)).MustNotHaveHappened();
    }

    [Fact]
    public async Task MethodWithMeasuredDurationAsync()
    {
        // Arrange
        DateOnly inputParam = DateOnly.FromDayNumber(1_000);
        Person expectedReturnValue = new("bar", 42);
        A.CallTo(() => _fakeService.MethodWithMeasuredDurationAsync(inputParam)).Returns(expectedReturnValue);

        // Act
        Person actualReturn = await _decorator.MethodWithMeasuredDurationAsync(inputParam);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.MethodWithMeasuredDurationAsync(inputParam)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(1711224704, firstWrite.EventId.Id);
        Assert.Equal("MethodWithMeasuredDurationAsync", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.LoggerName);
        Assert.Equal("Entering MethodWithMeasuredDurationAsync with parameters: someDate = 09/28/0003", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("someDate", inputParam),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithMeasuredDurationAsync with parameters: someDate = {someDate}"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(1711224704, lastWrite.EventId.Id);
        Assert.Equal("MethodWithMeasuredDurationAsync", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.LoggerName);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithMeasuredDurationAsync returned. Result = {result}. DurationInMilliseconds = {durationInMilliseconds}"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
        double? duration = afterWriteState.SingleOrDefault(kvp => kvp.Key == "durationInMilliseconds").Value as double?;
        Assert.True(duration > 0, $"Duration should be greater than 0, but was {duration}");
        Assert.Equal($"Method MethodWithMeasuredDurationAsync returned. Result = Person {{ Name = bar, Age = 42 }}. DurationInMilliseconds = {duration}", lastWrite.Message);
    }

    [Fact]
    public async Task MethodThrowsAndLogsExceptionAsync()
    {
        // Arrange
        InvalidOperationException expectedException = new("someMessage");
        A.CallTo(() => _fakeService.MethodThrowsAndLogsExceptionAsync()).ThrowsAsync(expectedException);

        // Act and Assert
        InvalidOperationException actualException = await Assert.ThrowsAsync<InvalidOperationException>(() => _decorator.MethodThrowsAndLogsExceptionAsync());
        Assert.Equal(expectedException, actualException);
        A.CallTo(() => _fakeService.MethodThrowsAndLogsExceptionAsync()).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(777, firstWrite.EventId.Id);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Information, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.LoggerName);
        Assert.Equal("Entering MethodThrowsAndLogsExceptionAsync", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodThrowsAndLogsExceptionAsync"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(777, lastWrite.EventId.Id);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Error, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.LoggerName);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync failed", lastWrite.Message);
        Assert.Equal(expectedException, lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "MethodThrowsAndLogsExceptionAsync failed"),
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }
}