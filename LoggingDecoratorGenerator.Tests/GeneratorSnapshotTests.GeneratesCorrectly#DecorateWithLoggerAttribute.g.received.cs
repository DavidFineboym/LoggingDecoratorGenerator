//HintName: DecorateWithLoggerAttribute.g.cs

namespace Fineboym.Logging.Generator
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class DecorateWithLoggerAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; }

        public DecorateWithLoggerAttribute(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Information)
        {
            Level = level;
        }
    }
}