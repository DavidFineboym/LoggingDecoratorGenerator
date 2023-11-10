using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class PolymorphicInterfacesTests
{
    private readonly FakeLogCollector _collector;
    private readonly IDerivedInterface _fakeService;
    private readonly DerivedInterfaceLoggingDecorator _decorator;

    public PolymorphicInterfacesTests()
    {
        _collector = new();
        FakeLogger<IDerivedInterface> logger = new(_collector);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(7, firstWrite.Id.Id);
        Assert.Equal("MethodWithAttributeAsync", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Trace, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDerivedInterface", firstWrite.Category);
        Assert.Equal("Entering MethodWithAttributeAsync with parameters: num = 42.3, secret = [REDACTED]", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("num", num),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodWithAttributeAsync with parameters: num = {num}, secret = [REDACTED]"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(7, lastWrite.Id.Id);
        Assert.Equal("MethodWithAttributeAsync", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Trace, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDerivedInterface", lastWrite.Category);
        Assert.Equal("Method MethodWithAttributeAsync returned. Result = returnValue", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodWithAttributeAsync returned. Result = {result}")
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
    }
}
