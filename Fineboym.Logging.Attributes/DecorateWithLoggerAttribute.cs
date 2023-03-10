using Microsoft.Extensions.Logging;
using System;

namespace Fineboym.Logging.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class DecorateWithLoggerAttribute : Attribute
    {
        public LogLevel Level { get; }

        public DecorateWithLoggerAttribute(LogLevel level = LogLevel.Information)
        {
            Level = level;
        }
    }
}