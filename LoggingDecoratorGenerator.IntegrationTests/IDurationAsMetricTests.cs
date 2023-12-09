using FakeItEasy;
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace LoggingDecoratorGenerator.IntegrationTests;

// TODO: Allow to specify MeasureDuration in DecorateWithLogger.
// TODO: Add XML documentation for properties. Copy some from https://learn.microsoft.com/en-us/dotnet/core/diagnostics/compare-metric-apis
// TODO: Edit readme.md and NuGet readme to include new features.
// TODO: Add two good tests for 2 methods below.
// TODO: Test with dotnet-counters before publishing.
// TODO: In next PR, merge interface files with test files.
[DecorateWithLogger(ReportDurationAsMetric = true)]
public interface IDurationAsMetric
{
    // TODO: Don't specify MeasureDuration here and make true in DecorateWithLogger.
    // TODO: Add test when no collector is listening, i.e. verify that no metrics are reported. Make sure logging is enabled.
    // TODO: Test metrics when two different implementations of the same interface are decorated.
    [LogMethod(MeasureDuration = true, Level = LogLevel.Information, EventId = 10, EventName = "MyName")]
    Task<int> MethodMeasuresDurationAsync(DateTime myDateTimeParam);

    [LogMethod(MeasureDuration = false)]
    int MethodWithoutDuration(DateTime input);
}

public sealed class DurationAsMetricTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMeterFactory _meterFactory;
    private readonly MetricCollector<double> _metricCollector;

    private readonly FakeLogCollector _logCollector;
    private readonly IDurationAsMetric _fakeService;
    private readonly DurationAsMetricLoggingDecorator _decorator;

    public DurationAsMetricTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _meterFactory = _serviceProvider.GetRequiredService<IMeterFactory>();

        _metricCollector = new MetricCollector<double>(
            meterScope: _meterFactory,
            meterName: typeof(IDurationAsMetric).ToString(),
            instrumentName: "logging_decorator.method.duration");

        _logCollector = new();
        FakeLogger<IDurationAsMetric> logger = new(_logCollector);
        _fakeService = A.Fake<IDurationAsMetric>();
        _decorator = new DurationAsMetricLoggingDecorator(logger, _fakeService, _meterFactory);
    }

    // TODO: Add new lines in generated code for readability.

    public void Dispose() => _serviceProvider.Dispose();

    [Fact]
    public async Task WhenMethodMeasuresDuration_ReportsAsMetric()
    {
        // Arrange
        DateTime input = DateTime.UtcNow;
        int expectedReturnValue = Random.Shared.Next();
        A.CallTo(() => _fakeService.MethodMeasuresDurationAsync(input)).Returns(expectedReturnValue);

        // Act
        int actualReturnValue = await _decorator.MethodMeasuresDurationAsync(input);

        // Assert
        Assert.Equal(expectedReturnValue, actualReturnValue);
        A.CallTo(() => _fakeService.MethodMeasuresDurationAsync(input)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _logCollector.Count);

        IReadOnlyList<FakeLogRecord> writes = _logCollector.GetSnapshot();

        FakeLogRecord firstWrite = writes[0];
        Assert.Equal(10, firstWrite.Id.Id);
        Assert.Equal("MyName", firstWrite.Id.Name);
        Assert.Equal(LogLevel.Information, firstWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDurationAsMetric", firstWrite.Category);
        Assert.Equal($"Entering MethodMeasuresDurationAsync with parameters: myDateTimeParam = {input.ToString(DateTimeFormatInfo.InvariantInfo)}", firstWrite.Message);
        Assert.Null(firstWrite.Exception);
        Assert.Empty(firstWrite.Scopes);
        KeyValuePair<string, object>[] expectedBeforeWriteState = new[]
        {
            new KeyValuePair<string, object>("myDateTimeParam", input.ToString(DateTimeFormatInfo.InvariantInfo)),
            new KeyValuePair<string, object>("{OriginalFormat}", "Entering MethodMeasuresDurationAsync with parameters: myDateTimeParam = {myDateTimeParam}"),
        };
        Assert.Equivalent(expectedBeforeWriteState, firstWrite.StructuredState, strict: true);

        FakeLogRecord lastWrite = writes[1];
        Assert.Equal(10, lastWrite.Id.Id);
        Assert.Equal("MyName", lastWrite.Id.Name);
        Assert.Equal(LogLevel.Information, lastWrite.Level);
        Assert.Equal("LoggingDecoratorGenerator.IntegrationTests.IDurationAsMetric", lastWrite.Category);
        Assert.Equal($"Method MethodMeasuresDurationAsync returned. Result = {expectedReturnValue}", lastWrite.Message);
        Assert.Null(lastWrite.Exception);
        Assert.Empty(lastWrite.Scopes);
        KeyValuePair<string, object>[] expectedAfterWriteState = new[]
        {
            new KeyValuePair<string, object>("result", expectedReturnValue),
            new KeyValuePair<string, object>("{OriginalFormat}", "Method MethodMeasuresDurationAsync returned. Result = {result}"),
        };
        Assert.Equivalent(expectedAfterWriteState, lastWrite.StructuredState, strict: true);

        Histogram<double> histogram = Assert.IsType<Histogram<double>>(_metricCollector.Instrument);
        Assert.Equal("s", histogram.Unit);
        Assert.Equal("The duration of method invocations.", histogram.Description);
        IReadOnlyList<CollectedMeasurement<double>> collectedMeasurements = _metricCollector.GetMeasurementSnapshot();
        CollectedMeasurement<double> singleMeasurement = Assert.Single(collectedMeasurements);
        Assert.InRange(singleMeasurement.Value, double.Epsilon, (lastWrite.Timestamp - firstWrite.Timestamp).TotalSeconds);
        bool tagsMatch = singleMeasurement.MatchesTags(
            new KeyValuePair<string, object?>("logging_decorator.type", _fakeService.GetType().ToString()),
            new KeyValuePair<string, object?>("logging_decorator.method", nameof(IDurationAsMetric.MethodMeasuresDurationAsync)));
        Assert.True(tagsMatch);
        Assert.True(singleMeasurement.Timestamp > firstWrite.Timestamp && singleMeasurement.Timestamp < lastWrite.Timestamp);
    }

    [Fact]
    public void WhenMetricCollectionEnabled_MethodWithoutDuration_NoMetricIsReported()
    {
        // Arrange
        DateTime input = DateTime.UtcNow;
        int expectedReturnValue = Random.Shared.Next();
        A.CallTo(() => _fakeService.MethodWithoutDuration(input)).Returns(expectedReturnValue);

        // Act
        int actualReturnValue = _decorator.MethodWithoutDuration(input);

        // Assert
        Assert.Equal(expectedReturnValue, actualReturnValue);
        A.CallTo(() => _fakeService.MethodWithoutDuration(input)).MustHaveHappenedOnceExactly();

        Assert.Equal(2, _logCollector.Count);
        Histogram<double> histogram = Assert.IsType<Histogram<double>>(_metricCollector.Instrument);
        Assert.Equal(typeof(IDurationAsMetric).ToString(), histogram.Meter.Name);
        Assert.Equal("logging_decorator.method.duration", histogram.Name);
        Assert.True(histogram.Enabled);
        Assert.Empty(_metricCollector.GetMeasurementSnapshot());
    }
}
