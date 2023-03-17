using Microsoft.Extensions.Logging;
using System;

namespace Fineboym.Logging.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class LogMethodAttribute : Attribute
    {
        public LogLevel Level { get; set; } = LogLevel.None;

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
    }
}