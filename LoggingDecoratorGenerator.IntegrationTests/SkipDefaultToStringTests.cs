using FakeItEasy;
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using static LoggingDecoratorGenerator.IntegrationTests.SkipDefaultToStringTests;

namespace LoggingDecoratorGenerator.IntegrationTests;

public class SkipDefaultToStringTests
{
    private readonly FakeLogCollector _collector;
    private readonly ISkipDefaultToStringService _fakeService;
    private readonly SkipDefaultToStringServiceLoggingDecorator _decorator;

    public SkipDefaultToStringTests()
    {
        _collector = new();
        LoggerFactory loggerFactory = new(new[] { new FakeLoggerProvider(_collector) });
        _fakeService = A.Fake<ISkipDefaultToStringService>();
        _decorator = new SkipDefaultToStringServiceLoggingDecorator(loggerFactory, _fakeService);
    }

    public class CustomTypeWithoutToString
    {
        public string Value { get; set; } = "";
    }

    public class CustomTypeWithToString
    {
        public string Value { get; set; } = "";
        public override string ToString() => $"Custom({Value})";
    }

    public class CustomTypeWithInheritedToString : CustomTypeWithToString
    {
    }

    public class CustomTypeWithHiddenToString
    {
        public new string ToString() => "hidden";
    }

    public enum ServiceStatus
    {
        Ready
    }

    [Fact]
    public void WhenParameterHasDefaultToString_ItIsOmittedFromLogs()
    {
        // Arrange
        var param = new CustomTypeWithoutToString { Value = "test" };
        A.CallTo(() => _fakeService.MethodWithCustomType(param)).DoesNothing();

        // Act
        _decorator.MethodWithCustomType(param);

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.True(new DecorateWithLoggerAttribute().SkipDefaultToString);
        Assert.Equal("Entering MethodWithCustomType", writes[0].Message);
    }

    [Fact]
    public void WhenParameterOverridesToString_ItIsIncludedInLogs()
    {
        // Arrange
        var param = new CustomTypeWithToString { Value = "test" };
        A.CallTo(() => _fakeService.MethodWithCustomToString(param)).DoesNothing();

        // Act
        _decorator.MethodWithCustomToString(param);

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithCustomToString with parameters: value = Custom(test)", writes[0].Message);
    }

    [Fact]
    public void WhenReturnValueHasDefaultToString_ItIsOmittedFromLogs()
    {
        // Arrange
        var returnValue = new CustomTypeWithoutToString { Value = "result" };
        A.CallTo(() => _fakeService.ReturnCustomType()).Returns(returnValue);

        // Act
        _decorator.ReturnCustomType();

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Method ReturnCustomType returned", writes[1].Message);
    }

    [Fact]
    public void WhenBuiltInTypeParameter_ItIsAlwaysLogged()
    {
        // Arrange
        int number = 42;
        string text = "hello";
        A.CallTo(() => _fakeService.MethodWithBuiltInTypes(number, text)).DoesNothing();

        // Act
        _decorator.MethodWithBuiltInTypes(number, text);

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithBuiltInTypes with parameters: number = 42, text = hello", writes[0].Message);
    }

    [Fact]
    public void WhenCombinedWithNotLogged_NotLoggedTakesPrecedence()
    {
        // Arrange
        var param = new CustomTypeWithToString { Value = "secret" };
        A.CallTo(() => _fakeService.MethodWithNotLogged(param)).DoesNothing();

        // Act
        _decorator.MethodWithNotLogged(param);

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithNotLogged with parameters: value = [REDACTED]", writes[0].Message);
    }

    [Fact]
    public async Task AsyncMethodWithDefaultToString_ParameterIsOmitted()
    {
        // Arrange
        var param = new CustomTypeWithoutToString { Value = "async" };
        var result = new CustomTypeWithToString { Value = "result" };
        A.CallTo(() => _fakeService.AsyncMethod(param)).Returns(result);

        // Act
        await _decorator.AsyncMethod(param);

        // Assert
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering AsyncMethod", writes[0].Message);
        Assert.Equal("Method AsyncMethod returned. Result = Custom(result)", writes[1].Message);
    }

    [Fact]
    public void WhenToStringOverrideIsInherited_ParametersAndResultAreLogged()
    {
        CustomTypeWithInheritedToString value = new() { Value = "test" };
        A.CallTo(() => _fakeService.MethodWithInheritedToString(value)).Returns(value);

        CustomTypeWithInheritedToString result = _decorator.MethodWithInheritedToString(value);

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithInheritedToString with parameters: value = Custom(test)", writes[0].Message);
        Assert.Equal("Method MethodWithInheritedToString returned. Result = Custom(test)", writes[1].Message);
    }

    [Fact]
    public void WhenEnumType_ParametersAndResultAreLogged()
    {
        A.CallTo(() => _fakeService.MethodWithEnum(ServiceStatus.Ready)).Returns(ServiceStatus.Ready);

        ServiceStatus result = _decorator.MethodWithEnum(ServiceStatus.Ready);

        Assert.Equal(ServiceStatus.Ready, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithEnum with parameters: value = Ready", writes[0].Message);
        Assert.Equal("Method MethodWithEnum returned. Result = Ready", writes[1].Message);
    }

    [Fact]
    public void WhenToStringIsHiddenRatherThanOverridden_ParametersAndResultAreOmitted()
    {
        CustomTypeWithHiddenToString value = new();
        A.CallTo(() => _fakeService.MethodWithHiddenToString(value)).Returns(value);

        CustomTypeWithHiddenToString result = _decorator.MethodWithHiddenToString(value);

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithHiddenToString", writes[0].Message);
        Assert.Equal("Method MethodWithHiddenToString returned", writes[1].Message);
    }

    [Fact]
    public void WhenReturnValueIsNotLogged_WithToStringOverride_ItIsRedacted()
    {
        CustomTypeWithToString value = new() { Value = "secret" };
        A.CallTo(() => _fakeService.ReturnNotLoggedCustomToString()).Returns(value);

        CustomTypeWithToString result = _decorator.ReturnNotLoggedCustomToString();

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Method ReturnNotLoggedCustomToString returned. Result = [REDACTED]", writes[1].Message);
        Assert.DoesNotContain(writes[1].StructuredState!, static entry => entry.Key == "result");
    }

    [Fact]
    public void WhenReturnValueIsNotLogged_WithDefaultToString_ItIsOmitted()
    {
        CustomTypeWithoutToString value = new() { Value = "secret" };
        A.CallTo(() => _fakeService.ReturnNotLoggedCustomType()).Returns(value);

        CustomTypeWithoutToString result = _decorator.ReturnNotLoggedCustomType();

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Method ReturnNotLoggedCustomType returned", writes[1].Message);
        Assert.DoesNotContain(writes[1].StructuredState!, static entry => entry.Key == "result");
    }

    [Fact]
    public void WhenParametersAreMixed_OmittedValuesLeaveNoExtraSeparatorsOrStructuredFields()
    {
        CustomTypeWithoutToString omitted = new() { Value = "omitted" };
        CustomTypeWithToString secret = new() { Value = "secret" };

        _decorator.MethodWithMixedTypes(omitted, 42, omitted, secret, omitted);

        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithMixedTypes with parameters: number = 42, secret = [REDACTED]", writes[0].Message);
        Assert.DoesNotContain(writes[0].StructuredState!, static entry => entry.Key is "first" or "middle" or "secret" or "last");
    }

    [Fact]
    public async Task WhenValueTaskResultHasDefaultToString_ItIsOmitted()
    {
        CustomTypeWithoutToString value = new() { Value = "result" };
        A.CallTo(() => _fakeService.ReturnCustomTypeValueTask()).Returns(new ValueTask<CustomTypeWithoutToString>(value));

        CustomTypeWithoutToString result = await _decorator.ReturnCustomTypeValueTask();

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Method ReturnCustomTypeValueTask returned", writes[1].Message);
    }

    [Fact]
    public void WhenDeclaredTypeIsObject_RuntimeToStringOverrideDoesNotEnableLogging()
    {
        object value = "hello";
        A.CallTo(() => _fakeService.MethodWithObject(value)).Returns(value);

        object result = _decorator.MethodWithObject(value);

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithObject", writes[0].Message);
        Assert.Equal("Method MethodWithObject returned", writes[1].Message);
        Assert.DoesNotContain(writes[0].StructuredState!, static entry => entry.Key == "value");
        Assert.DoesNotContain(writes[1].StructuredState!, static entry => entry.Key == "result");
    }

    [Fact]
    public void WhenDeclaredTypeIsInterface_RuntimeToStringOverrideDoesNotEnableLogging()
    {
        IFormattable value = 42;
        A.CallTo(() => _fakeService.MethodWithInterface(value)).Returns(value);

        IFormattable result = _decorator.MethodWithInterface(value);

        Assert.Same(value, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        Assert.Equal("Entering MethodWithInterface", writes[0].Message);
        Assert.Equal("Method MethodWithInterface returned", writes[1].Message);
        Assert.DoesNotContain(writes[0].StructuredState!, static entry => entry.Key == "value");
        Assert.DoesNotContain(writes[1].StructuredState!, static entry => entry.Key == "result");
    }

    [Fact]
    public void WhenSkipDefaultToStringIsDisabled_DefaultParameterAndReturnValuesAreLogged()
    {
        using LoggerFactory loggerFactory = new(new[] { new FakeLoggerProvider(_collector) });
        IIncludeDefaultToStringService fakeService = A.Fake<IIncludeDefaultToStringService>();
        IncludeDefaultToStringServiceLoggingDecorator decorator = new(loggerFactory, fakeService);
        CustomTypeWithoutToString value = new() { Value = "input" };
        CustomTypeWithoutToString returnValue = new() { Value = "result" };
        A.CallTo(() => fakeService.Echo(value)).Returns(returnValue);

        CustomTypeWithoutToString result = decorator.Echo(value);

        Assert.Same(returnValue, result);
        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);
        string typeName = typeof(CustomTypeWithoutToString).FullName!;
        Assert.Equal($"Entering Echo with parameters: value = {typeName}", writes[0].Message);
        Assert.Equal($"Method Echo returned. Result = {typeName}", writes[1].Message);
        Assert.Contains(writes[0].StructuredState!, entry => entry.Key == "value" && entry.Value == typeName);
        Assert.Contains(writes[1].StructuredState!, entry => entry.Key == "result" && entry.Value == typeName);
    }

    [Fact]
    public async Task WhenMeasureDurationEnabled_SkipDefaultToStringStillWorks()
    {
        // Arrange
        var param = new CustomTypeWithoutToString { Value = "input" };
        var result = new CustomTypeWithoutToString { Value = "result" };
        A.CallTo(() => _fakeService.MeasuredDurationMethod(param)).Returns(result);

        // Act
        var actualResult = await _decorator.MeasuredDurationMethod(param);

        // Assert
        Assert.Equal(result, actualResult);
        A.CallTo(() => _fakeService.MeasuredDurationMethod(param)).MustHaveHappenedOnceExactly();

        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);

        // First write should omit parameter value
        var firstWrite = writes[0];
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("Entering MeasuredDurationMethod", firstWrite.Message);
        Assert.Empty(firstWrite.Scopes);

        // Last write should include duration but omit result value
        var lastWrite = writes[1];
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Empty(lastWrite.Scopes);

        var afterWriteState = lastWrite.StructuredState;
        KeyValuePair<string, string>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, string>("{OriginalFormat}", "Method MeasuredDurationMethod returned. DurationInMilliseconds = {durationInMilliseconds}"),
        };

        Assert.Equivalent(expectedAfterWriteState, afterWriteState);

        string? durationString = afterWriteState!.SingleOrDefault(kvp => kvp.Key == "durationInMilliseconds").Value;
        Assert.True(double.TryParse(durationString, out double duration));
        Assert.Equal($"Method MeasuredDurationMethod returned. DurationInMilliseconds = {duration}", lastWrite.Message);
        Assert.InRange(duration, double.Epsilon, lastWrite.Timestamp.Subtract(firstWrite.Timestamp).TotalMilliseconds);
    }

    [Fact]
    public async Task WhenMeasureDurationEnabled_WithToStringOverride_ParametersAndResultAreLogged()
    {
        // Arrange
        var param = new CustomTypeWithToString { Value = "input" };
        var result = new CustomTypeWithToString { Value = "output" };
        A.CallTo(() => _fakeService.MeasuredDurationMethodWithToString(param)).Returns(result);

        // Act
        var actualResult = await _decorator.MeasuredDurationMethodWithToString(param);

        // Assert
        Assert.Equal(result, actualResult);
        A.CallTo(() => _fakeService.MeasuredDurationMethodWithToString(param)).MustHaveHappenedOnceExactly();

        var writes = _collector.GetSnapshot();
        Assert.Equal(2, writes.Count);

        // First write should include parameter value
        var firstWrite = writes[0];
        Assert.Equal(LogLevel.Debug, firstWrite.Level);
        Assert.Equal("Entering MeasuredDurationMethodWithToString with parameters: value = Custom(input)", firstWrite.Message);
        Assert.Empty(firstWrite.Scopes);

        // Last write should include both result and duration
        var lastWrite = writes[1];
        Assert.Equal(LogLevel.Debug, lastWrite.Level);
        Assert.Empty(lastWrite.Scopes);

        var afterWriteState = lastWrite.StructuredState;
        KeyValuePair<string, string>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, string>("result", result.ToString()),
            new KeyValuePair<string, string>("{OriginalFormat}", "Method MeasuredDurationMethodWithToString returned. Result = {result}. DurationInMilliseconds = {durationInMilliseconds}"),
        };

        Assert.Equivalent(expectedAfterWriteState, afterWriteState);

        string? durationString = afterWriteState!.SingleOrDefault(kvp => kvp.Key == "durationInMilliseconds").Value;
        Assert.True(double.TryParse(durationString, out double duration));
        Assert.Equal($"Method MeasuredDurationMethodWithToString returned. Result = Custom(output). DurationInMilliseconds = {duration}", lastWrite.Message);
        Assert.InRange(duration, double.Epsilon, lastWrite.Timestamp.Subtract(firstWrite.Timestamp).TotalMilliseconds);
    }
}

// Interface definition for the tests
[DecorateWithLogger]
public interface ISkipDefaultToStringService
{
    void MethodWithCustomType(CustomTypeWithoutToString value);
    void MethodWithCustomToString(CustomTypeWithToString value);
    CustomTypeWithoutToString ReturnCustomType();
    void MethodWithBuiltInTypes(int number, string text);
    void MethodWithNotLogged([NotLogged] CustomTypeWithToString value);
    Task<CustomTypeWithToString> AsyncMethod(CustomTypeWithoutToString param);
    CustomTypeWithInheritedToString MethodWithInheritedToString(CustomTypeWithInheritedToString value);
    ServiceStatus MethodWithEnum(ServiceStatus value);
    CustomTypeWithHiddenToString MethodWithHiddenToString(CustomTypeWithHiddenToString value);

    [return: NotLogged]
    CustomTypeWithToString ReturnNotLoggedCustomToString();

    [return: NotLogged]
    CustomTypeWithoutToString ReturnNotLoggedCustomType();

    void MethodWithMixedTypes([NotLogged] CustomTypeWithoutToString first, int number, CustomTypeWithoutToString middle, [NotLogged] CustomTypeWithToString secret, CustomTypeWithoutToString last);
    ValueTask<CustomTypeWithoutToString> ReturnCustomTypeValueTask();
    object MethodWithObject(object value);
    IFormattable MethodWithInterface(IFormattable value);

    [LogMethod(MeasureDuration = true)]
    Task<CustomTypeWithoutToString> MeasuredDurationMethod(CustomTypeWithoutToString value);

    [LogMethod(MeasureDuration = true)]
    Task<CustomTypeWithToString> MeasuredDurationMethodWithToString(CustomTypeWithToString value);
}

[DecorateWithLogger(SkipDefaultToString = false)]
public interface IIncludeDefaultToStringService
{
    CustomTypeWithoutToString Echo(CustomTypeWithoutToString value);
}
