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
        /// Surrounds the method call by <see cref="global::System.Diagnostics.Stopwatch"/> and logs duration in milliseconds. Default is false.
        /// </summary>
        public bool MeasureDuration { get; set; }
    }
}