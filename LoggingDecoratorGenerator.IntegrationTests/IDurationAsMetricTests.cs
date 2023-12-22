using FakeItEasy;
using Fineboym.Logging.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace LoggingDecoratorGenerator.IntegrationTests;

[DecorateWithLogger(MeasureDuration = true, ReportDurationAsMetric = true)]
public interface IDurationAsMetric
{
    [LogMethod(Level = LogLevel.Information, EventId = 10, EventName = "MyName")]
    Task<int> MethodMeasuresDurationAsync(DateTime myDateTimeParam);

    [LogMethod(MeasureDuration = false)]
    int MethodWithoutDuration(DateTime input);
}

public sealed class DurationAsMetricTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IMeterFactory _meterFactory;
    private readonly MetricCollector<double> _metricCollector;

    private readonly FakeLogger<IDurationAsMetric> _logger;
    private readonly FakeLogCollector _logCollector;
    private readonly IDurationAsMetric _fakeService;
    private readonly DurationAsMetricLoggingDecorator _decorator;

    public DurationAsMetricTests()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddMetrics();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _meterFactory = _serviceProvider.GetRequiredService<IMeterFactory>();

        _logCollector = new();
        _logger = new(_logCollector);
        _fakeService = A.Fake<IDurationAsMetric>();
        _decorator = new DurationAsMetricLoggingDecorator(_logger, _fakeService, _meterFactory);

        _metricCollector = new MetricCollector<double>(
            meterScope: _meterFactory,
            meterName: _fakeService.GetType().ToString(),
            instrumentName: "logging_decorator.method.duration");
    }

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
        Assert.Null(histogram.Tags);
        Assert.Null(histogram.Meter.Tags);
        Assert.Null(histogram.Meter.Version);

        IReadOnlyList<CollectedMeasurement<double>> collectedMeasurements = _metricCollector.GetMeasurementSnapshot();
        CollectedMeasurement<double> singleMeasurement = Assert.Single(collectedMeasurements);
        Assert.InRange(singleMeasurement.Value, double.Epsilon, (lastWrite.Timestamp - firstWrite.Timestamp).TotalSeconds);
        bool tagsMatch = singleMeasurement.MatchesTags(
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
        Assert.Equal(_fakeService.GetType().ToString(), histogram.Meter.Name);
        Assert.Equal("logging_decorator.method.duration", histogram.Name);
        Assert.True(histogram.Enabled);
        Assert.Empty(_metricCollector.GetMeasurementSnapshot());
    }

    [Fact]
    public async Task WhenTwoImplementationsOfSameInterface_ReportToDifferentMeters_DifferentInstruments()
    {
        // Arrange
        MetricCollector<double>? firstCollector = null, secondCollector = null;
        using MeterListener meterListener = new();
        meterListener.InstrumentPublished = (instrument, listener) =>
        {
            Assert.Null(instrument.Meter.Tags);
            Assert.Null(instrument.Meter.Version);

            Histogram<double> histogram = Assert.IsType<Histogram<double>>(instrument);
            Assert.Equal("s", histogram.Unit);
            Assert.Equal("The duration of method invocations.", histogram.Description);
            Assert.Null(histogram.Tags);

            if (instrument.Meter.Name == typeof(FirstImplementation).ToString())
            {
                firstCollector = new MetricCollector<double>(histogram);
            }
            else if (instrument.Meter.Name == typeof(SecondImplementation).ToString())
            {
                secondCollector = new MetricCollector<double>(histogram);
            }
        };

        meterListener.Start();

        var decorator1 = new DurationAsMetricLoggingDecorator(_logger, new FirstImplementation(), _meterFactory);
        var decorator2 = new DurationAsMetricLoggingDecorator(_logger, new SecondImplementation(), _meterFactory);

        // Act
        await decorator1.MethodMeasuresDurationAsync(DateTime.UtcNow);
        await decorator2.MethodMeasuresDurationAsync(DateTime.UtcNow);

        // Assert
        Assert.NotNull(firstCollector?.Instrument);
        Assert.NotNull(secondCollector?.Instrument);

        Assert.NotSame(firstCollector.Instrument.Meter, secondCollector.Instrument.Meter);
        Assert.NotSame(firstCollector.Instrument, secondCollector.Instrument);

        CollectedMeasurement<double> firstImplementationMeasurement = Assert.Single(firstCollector.GetMeasurementSnapshot());
        CollectedMeasurement<double> secondImplementationMeasurement = Assert.Single(secondCollector.GetMeasurementSnapshot());

        KeyValuePair<string, object?> firstImplementationTag = Assert.Single(firstImplementationMeasurement.Tags);
        KeyValuePair<string, object?> secondImplementationTag = Assert.Single(secondImplementationMeasurement.Tags);

        Assert.Equal(new KeyValuePair<string, object?>("logging_decorator.method", nameof(IDurationAsMetric.MethodMeasuresDurationAsync)), firstImplementationTag);
        Assert.Equal(firstImplementationTag, secondImplementationTag);
    }

    private class FirstImplementation : IDurationAsMetric
    {
        public Task<int> MethodMeasuresDurationAsync(DateTime myDateTimeParam)
        {
            return Task.FromResult(1);
        }

        public int MethodWithoutDuration(DateTime input)
        {
            throw new NotImplementedException();
        }
    }

    private class SecondImplementation : IDurationAsMetric
    {
        public Task<int> MethodMeasuresDurationAsync(DateTime myDateTimeParam)
        {
            return Task.FromResult(2);
        }

        public int MethodWithoutDuration(DateTime input)
        {
            throw new NotImplementedException();
        }
    }
}