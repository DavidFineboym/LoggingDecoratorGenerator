//HintName: LogMethodAttribute.g.cs
#nullable enable
namespace Fineboym.Logging.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class LogMethodAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; set; } = Microsoft.Extensions.Logging.LogLevel.None;

        /// <summary>
        /// Gets the logging event id for the logging method.
        /// </summary>
        public int EventId { get; set; } = -1;

        /// <summary>
        /// Gets or sets the logging event name for the logging method.
        /// </summary>
        /// <remarks>
        /// This will equal the method name if not specified.
        /// </remarks>
        public string? EventName { get; set; }

        /// <summary>
        /// Surrounds the method call by <see cref="System.Diagnostics.Stopwatch"/>, default is <see langword="false"/>.
        /// If <see cref="DecorateWithLoggerAttribute.ReportDurationAsMetric"/> is <see langword="false"/>, then duration in milliseconds is included in the log message about method's return, otherwise separately as a metric in seconds.
        /// </summary>
        public bool MeasureDuration { get; set; }

        /// <summary>
        /// By default, exceptions are not logged and there is no try-catch block around the method call.
        /// Set this property to some exception type to log exceptions of that type.
        /// </summary>
        public System.Type? ExceptionToLog { get; set; }

        /// <summary>
        /// If <see cref="ExceptionToLog"/> is not null, then this controls log level for exceptions. Default is <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>.
        /// </summary>
        public Microsoft.Extensions.Logging.LogLevel ExceptionLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
    }
}