using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class PolymorphicInterfacesTests
{
    private readonly TestSink _testSink;
    private readonly IDerivedInterface _fakeService;
    private readonly DerivedInterfaceLoggingDecorator _decorator;

    public PolymorphicInterfacesTests()
    {
        _testSink = new();
        TestLoggerFactory testLoggerFactory = new(_testSink, enabled: true);
        ILogger<IDerivedInterface> logger = new Logger<IDerivedInterface>(testLoggerFactory);
        _fakeService = A.Fake<IDerivedInterface>();
        _decorator = new DerivedInterfaceLoggingDecorator(logger, _fakeService);
    }

    [Fact]
    public async Task PassThroughMethodAsync()
    {
        // Arrange
        const int x = 1;
        const int y = 2;
        const int expectedResult = 3;
        ILogger<IDerivedInterface> fakeLogger = A.Fake<ILogger<IDerivedInterface>>();
        var decorator = new DerivedInterfaceLoggingDecorator(fakeLogger, _fakeService);
        A.CallTo(() => _fakeService.PassThroughMethodAsync(x, y)).Returns(expectedResult);

        // Act
        int result = await decorator.PassThroughMethodAsync(x, y);

        // Assert
        Assert.Equal(expectedResult, result);
        A.CallTo(() => _fakeService.PassThroughMethodAsync(x, y)).MustHaveHappenedOnceExactly();
        A.CallTo(fakeLogger).MustNotHaveHappened();
    }

    [Fact]
    public async Task MethodWithAttributeAsync()
    {
        // Arrange
        const float num = 42.3f;
        const string secret = "bar";
        const string expectedReturnValue = "returnValue";
        A.CallTo(() => _fakeService.MethodWithAttributeAsync(num, secret)).Returns(expectedReturnValue);

        // Act
        string actualReturn = await _decorator.MethodWithAttributeAsync(num, secret);

        // Assert
        Assert.Equal(expected: expectedReturnValue, actual: actualReturn);
        A.CallTo(() => _fakeService.MethodWithAttributeAsync(num, secret)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _testSink.Writes.Count);

        WriteContext firstWrite = _testSink.Writes.First();
        Assert.Equal(7, firstWrite.EventId.Id);
        Assert.Equal("MethodWithAttributeAsync", firstWrite.EventId.Name);
        Assert.Equal(LogLevel.Trace, firstWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDerivedInterface", firstWrite.LoggerName);
        Assert.Equal("Entering MethodWithAttributeAsync with parameters: num = 42.3, secret = [REDACTED]", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Null(firstWrite.Scope);
        var beforeWriteState = (IReadOnlyList<KeyValuePair<string, object>>)firstWrite.State;
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("num", num),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithAttributeAsync with parameters: num = {num}, secret = [REDACTED]"),
        };
        LogValuesAssert.Contains(expectedBeforeWriteState, beforeWriteState);

        WriteContext lastWrite = _testSink.Writes.Last();
        Assert.Equal(7, lastWrite.EventId.Id);
        Assert.Equal("MethodWithAttributeAsync", lastWrite.EventId.Name);
        Assert.Equal(LogLevel.Trace, lastWrite.LogLevel);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDerivedInterface", lastWrite.LoggerName);
        Assert.Equal("Method MethodWithAttributeAsync returned. Result = returnValue", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Null(lastWrite.Scope);
        var afterWriteState = (IReadOnlyList<KeyValuePair<string, object>>)lastWrite.State;
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithAttributeAsync returned. Result = {result}")
        };
        LogValuesAssert.Contains(expectedAfterWriteState, afterWriteState);
    }
}
