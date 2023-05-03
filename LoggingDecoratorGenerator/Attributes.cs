﻿namespace Fineboym.Logging.Generator;

internal static class Attributes
{
    public const string DecorateWithLogger = @"#nullable enable
namespace Fineboym.Logging.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class DecorateWithLoggerAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; }

        public DecorateWithLoggerAttribute(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Debug)
        {
            Level = level;
        }
    }
}";

    public const string LogMethod = @"#nullable enable
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
        /// Surrounds the method call by <see cref=""System.Diagnostics.Stopwatch""/> and logs duration in milliseconds. Default is false.
        /// </summary>
        public bool MeasureDuration { get; set; }

        /// <summary>
        /// By default, exceptions are not logged and there is no try-catch block around the method call.
        /// Set this property to some exception type to log exceptions of that type.
        /// </summary>
        public System.Type? ExceptionToLog { get; set; }

        /// <summary>
        /// If <see cref=""ExceptionToLog""/> is not null, then this controls log level for exceptions. Default is <see cref=""Microsoft.Extensions.Logging.LogLevel.Error""/>.
        /// </summary>
        public Microsoft.Extensions.Logging.LogLevel ExceptionLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
    }
}";

    public const string NotLogged = """
        #nullable enable
        namespace Fineboym.Logging.Attributes
        {
            [System.AttributeUsage(System.AttributeTargets.Parameter | System.AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = true)]
            internal sealed class NotLoggedAttribute : System.Attribute
            {
            }
        }
        """;
}