namespace Fineboym.Logging.Generator;

internal static class Attributes
{
    private const string Namespace = "Fineboym.Logging.Attributes";

    public const string DecorateWithLoggerName = "DecorateWithLoggerAttribute";
    public const string DecorateWithLoggerFullName = $"{Namespace}.{DecorateWithLoggerName}";
    public const string ReportDurationAsMetricName = "ReportDurationAsMetric";
    public const string DecorateWithLogger = $$"""
        #nullable enable
        namespace {{Namespace}}
        {
            [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
            internal sealed class {{DecorateWithLoggerName}} : System.Attribute
            {
                public Microsoft.Extensions.Logging.LogLevel Level { get; }

                /// <summary>
                /// If a method measures duration and this is set to <see langword="true"/>, then the decorator will report the durations of method invocations as a metric using the System.Diagnostics.Metrics APIs.
                /// If <see langword="true"/>, the durations won't be reported in log messages and decorator class will require <see cref="System.Diagnostics.Metrics.IMeterFactory"/> in its constructor.
                /// It's available by targeting .NET 6+, or in older .NET Core and .NET Framework apps by adding a reference to the .NET System.Diagnostics.DiagnosticsSource 6.0+ NuGet package.
                /// For more info, see <see href="https://learn.microsoft.com/en-us/aspnet/core/log-mon/metrics/metrics">ASP.NET Core metrics</see>,
                /// <see href="https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel">.NET observability with OpenTelemetry</see>,
                /// <see href="https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-collection">Collect metrics</see>.
                /// </summary>
                public bool {{ReportDurationAsMetricName}} { get; set; }

                public {{DecorateWithLoggerName}}(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Debug)
                {
                    Level = level;
                }
            }
        }
        """;

    public const string LogMethodName = "LogMethodAttribute";
    public const string LogMethodFullName = $"{Namespace}.{LogMethodName}";
    public const string LogMethodLevelName = "Level";
    public const string LogMethodEventIdName = "EventId";
    public const string LogMethodEventNameName = "EventName";
    public const string LogMethodMeasureDurationName = "MeasureDuration";
    public const string LogMethodExceptionToLogName = "ExceptionToLog";
    public const string LogMethodExceptionLogLevelName = "ExceptionLogLevel";
    public const string LogMethod = $$"""
        #nullable enable
        namespace {{Namespace}}
        {
            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
            internal sealed class {{LogMethodName}} : System.Attribute
            {
                public Microsoft.Extensions.Logging.LogLevel {{LogMethodLevelName}} { get; set; } = Microsoft.Extensions.Logging.LogLevel.None;

                /// <summary>
                /// Gets the logging event id for the logging method.
                /// </summary>
                public int {{LogMethodEventIdName}} { get; set; } = -1;

                /// <summary>
                /// Gets or sets the logging event name for the logging method.
                /// </summary>
                /// <remarks>
                /// This will equal the method name if not specified.
                /// </remarks>
                public string? {{LogMethodEventNameName}} { get; set; }

                /// <summary>
                /// Surrounds the method call by <see cref="System.Diagnostics.Stopwatch"/>. Default is <see langword="false"/>.
                /// If <see cref="{{Namespace}}.{{DecorateWithLoggerName}}.{{ReportDurationAsMetricName}}"/> is <see langword="false"/>, then duration in milliseconds is included in the log message about method's return, otherwise separately as a metric in seconds.
                /// </summary>
                public bool {{LogMethodMeasureDurationName}} { get; set; }

                /// <summary>
                /// By default, exceptions are not logged and there is no try-catch block around the method call.
                /// Set this property to some exception type to log exceptions of that type.
                /// </summary>
                public System.Type? {{LogMethodExceptionToLogName}} { get; set; }

                /// <summary>
                /// If <see cref="{{LogMethodExceptionToLogName}}"/> is not null, then this controls log level for exceptions. Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>.
                /// </summary>
                public Microsoft.Extensions.Logging.LogLevel {{LogMethodExceptionLogLevelName}} { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
            }
        }
        """;

    public const string NotLoggedName = "NotLoggedAttribute";
    public const string NotLoggedFullName = $"{Namespace}.{NotLoggedName}";
    public const string NotLogged = $$"""
        #nullable enable
        namespace {{Namespace}}
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
            internal sealed class {{NotLoggedName}} : System.Attribute
            {
            }
        }
        """;
}
