using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using OtherFolder.OtherSubFolder;
using System.Globalization;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class InformationLevelInterfaceTests
{
    private readonly FakeLogCollector _collector;
    private readonly IInformationLevelInterface _fakeService;
    private readonly InformationLevelInterfaceLoggingDecorator _decorator;

    public InformationLevelInterfaceTests()
    {
        _collector = new();
        FakeLogger<IInformationLevelInterface> logger = new(_collector);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(1514124652, firstWrite.Id.Id);
        Assert.Equal("MethodWithoutAttribute", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Information, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.Category);
        Assert.Equal("Entering MethodWithoutAttribute with parameters: x = 42, y = 43", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("x", x),
            new KeyValuePair<string, object>("y", y),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithoutAttribute with parameters: x = {x}, y = {y}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(1514124652, lastWrite.Id.Id);
        Assert.Equal("MethodWithoutAttribute", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Information, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.Category);
        Assert.Equal("Method MethodWithoutAttribute returned. Result = 42.43", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(lastWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithoutAttribute returned. Result = {result}"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        var firstWrite = writes[0];
        Assert.Equal(100, firstWrite.Id.Id);
        Assert.Equal("SomePersonEventName", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.Category);
        Assert.Equal("Entering MethodWithAttribute with parameters: person = Person { Name = foo, Age = 30 }, someNumber = 33", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, string>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, string>("person", firstInput.ToString()),
            new KeyValuePair<string, string>("someNumber", secondInput.ToString()),
            new KeyValuePair<string, string>("{OriginalFormat}", "Entering MethodWithAttribute with parameters: person = {person}, someNumber = {someNumber}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        var lastWrite = writes[1];
        Assert.Equal(100, lastWrite.Id.Id);
        Assert.Equal("SomePersonEventName", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.Category);
        Assert.Equal("Method MethodWithAttribute returned. Result = Person { Name = bar, Age = 42 }", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(lastWrite.Scopes);
        KeyValuePair<string, string>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, string>("result", expectedReturnValue.ToString()),
            new KeyValuePair<string, string>("{OriginalFormat}", "Method MethodWithAttribute returned. Result = {result}"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        var firstWrite = writes[0];
        Assert.Equal(1711224704, firstWrite.Id.Id);
        Assert.Equal("MethodWithMeasuredDurationAsync", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Information, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.Category);
        Assert.Equal("Entering MethodWithMeasuredDurationAsync with parameters: someDate = 09/28/0003", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, string>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, string>("someDate", inputParam.ToString(DateTimeFormatInfo.InvariantInfo)),
            new KeyValuePair<string, string>("{OriginalFormat}", "Entering MethodWithMeasuredDurationAsync with parameters: someDate = {someDate}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        var lastWrite = writes[1];
        Assert.Equal(1711224704, lastWrite.Id.Id);
        Assert.Equal("MethodWithMeasuredDurationAsync", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Information, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.Category);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(lastWrite.Scopes);
        var afterWriteState = lastWrite.StructuredState;
        KeyValuePair<string, string>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, string>("result", expectedReturnValue.ToString()),
            new KeyValuePair<string, string>("{OriginalFormat}", "Method MethodWithMeasuredDurationAsync returned. Result = {result}. DurationInMilliseconds = {durationInMilliseconds}"),
        };
        Assert.Equivalent(expectedAfterWriteState, afterWriteState);
        string? durationString = afterWriteState!.SingleOrDefault(kvp => kvp.Key == "durationInMilliseconds").Value;
        Assert.True(double.TryParse(durationString, out double duration));
        Assert.Equal($"Method MethodWithMeasuredDurationAsync returned. Result = Person {{ Name = bar, Age = 42 }}. DurationInMilliseconds = {duration}", lastWrite.Message);
        Assert.InRange(duration, double.Epsilon, lastWrite.Timestamp.Subtract(firstWrite.Timestamp).TotalMilliseconds);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        var firstWrite = writes[0];
        Assert.Equal(777, firstWrite.Id.Id);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Information, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", firstWrite.Category);
        Assert.Equal("Entering MethodThrowsAndLogsExceptionAsync", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodThrowsAndLogsExceptionAsync"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        var lastWrite = writes[1];
        Assert.Equal(777, lastWrite.Id.Id);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Error, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IInformationLevelInterface", lastWrite.Category);
        Assert.Equal("MethodThrowsAndLogsExceptionAsync failed", lastWrite.Message);
        Assert.Equal(expectedException, lastWrite.Exception);
        Assert.Empty(lastWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "MethodThrowsAndLogsExceptionAsync failed"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
    }
}