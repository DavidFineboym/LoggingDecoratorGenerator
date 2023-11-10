using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Globalization;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class SomeServiceTests
{
    private readonly FakeLogCollector _collector;
    private readonly ISomeService _fakeService;
    private readonly SomeServiceLoggingDecorator _decorator;

    public SomeServiceTests()
    {
        _collector = new();
        FakeLogger<ISomeService> logger = new(_collector);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(963397959, firstWrite.Id.Id);
        Assert.Equal("DateTimeReturningMethod", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.Category);
        Assert.Equal("Entering DateTimeReturningMethod with parameters: someDateTime = 12/12/2022 22:57:45", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, string>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, string>("someDateTime", dateTimeParameter.ToString(DateTimeFormatInfo.InvariantInfo)),
            new KeyValuePair<string, string>("{OriginalFormat}", "Entering DateTimeReturningMethod with parameters: someDateTime = {someDateTime}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(963397959, lastWrite.Id.Id);
        Assert.Equal("DateTimeReturningMethod", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.Category);
        Assert.Equal("Method DateTimeReturningMethod returned. Result = 09/06/2020 00:00:00", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, string>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, string>("result", expectedReturnValue.ToString(DateTimeFormatInfo.InvariantInfo)),
            new KeyValuePair<string, string>("{OriginalFormat}", "Method DateTimeReturningMethod returned. Result = {result}"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(1921103492, firstWrite.Id.Id);
        Assert.Equal("GetMySecretString", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.Category);
        Assert.Equal("Entering GetMySecretString with parameters: username = foo, password = [REDACTED], x = 42", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("username", username),
            new KeyValuePair<string, object>("x", x),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering GetMySecretString with parameters: username = {username}, password = [REDACTED], x = {x}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(1921103492, lastWrite.Id.Id);
        Assert.Equal("GetMySecretString", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.Category);
        Assert.Equal("Method GetMySecretString returned. Result = [REDACTED]", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Method GetMySecretString returned. Result = [REDACTED]")
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(0, firstWrite.Id.Id);
        Assert.Equal("StringReturningAsyncMethod", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Information, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.Category);
        Assert.Equal("Entering StringReturningAsyncMethod with parameters: s = SomeInputParameter", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("s", inputParameter),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering StringReturningAsyncMethod with parameters: s = {s}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(0, lastWrite.Id.Id);
        Assert.Equal("StringReturningAsyncMethod", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Information, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.Category);
        Assert.Equal("Method StringReturningAsyncMethod returned. Result = SomeReturnValue", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method StringReturningAsyncMethod returned. Result = {result}"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
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

        Assert.Equal(2, _collector.Count);

        IReadOnlyList<FakeLogRecord> writes = _collector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(333, firstWrite.Id.Id);
        Assert.Equal("WithIntegerParam", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", firstWrite.Category);
        Assert.Equal("Entering TwoMethodsWithSameName with parameters: i = 42", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("i", inputParameter),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering TwoMethodsWithSameName with parameters: i = {i}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(333, lastWrite.Id.Id);
        Assert.Equal("WithIntegerParam", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.ISomeService", lastWrite.Category);
        Assert.Equal("Method TwoMethodsWithSameName returned", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("{OriginalFormat}", "Method TwoMethodsWithSameName returned"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);
    }
}
