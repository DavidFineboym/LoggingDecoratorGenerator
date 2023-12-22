//HintName: DecorateWithLoggerAttribute.g.cs
#nullable enable
namespace Fineboym.Logging.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class DecorateWithLoggerAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; }

        /// <summary>
        /// Surrounds all method calls by <see cref="System.Diagnostics.Stopwatch"/>, default is <see langword="false"/>.
        /// Can be overridden for each method by <see cref="LogMethodAttribute.MeasureDuration"/>.
        /// If <see cref="ReportDurationAsMetric"/> is <see langword="false"/>, then duration in milliseconds is included in the log message about method's return, otherwise separately as a metric in seconds.
        /// </summary>
        public bool MeasureDuration { get; set; }

        /// <summary>
        /// If a method measures duration and this is set to <see langword="true"/>, then the decorator will report the durations of method invocations as a metric using the System.Diagnostics.Metrics APIs.
        /// If <see langword="true"/>, the durations won't be reported in log messages and decorator class will require <see cref="System.Diagnostics.Metrics.IMeterFactory"/> in its constructor.
        /// It's available by targeting .NET 6+, or in older .NET Core and .NET Framework apps by adding a reference to the .NET System.Diagnostics.DiagnosticsSource 6.0+ NuGet package.
        /// For more info, see <see href="https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/metrics">ASP.NET Core metrics</see>,
        /// <see href="https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel">.NET observability with OpenTelemetry</see>,
        /// <see href="https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-collection">Collect metrics</see>.
        /// </summary>
        public bool ReportDurationAsMetric { get; set; }

        public DecorateWithLoggerAttribute(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Debug)
        {
            Level = level;
        }
    }
}