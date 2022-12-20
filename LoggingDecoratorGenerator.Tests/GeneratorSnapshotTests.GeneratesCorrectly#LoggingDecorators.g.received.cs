//HintName: LoggingDecorators.g.cs
namespace LoggingDecoratorGenerator
{
    public sealed class SomeServiceLoggingDecorator : SomeFolder.SomeSubFolder.ISomeService
    {
        private readonly Microsoft.Extensions.Logging.ILogger<SomeFolder.SomeSubFolder.ISomeService> _logger;
        private readonly SomeFolder.SomeSubFolder.ISomeService _decorated;
        
        public SomeServiceLoggingDecorator(Microsoft.Extensions.Logging.ILogger<SomeFolder.SomeSubFolder.ISomeService> logger, SomeFolder.SomeSubFolder.ISomeService decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, OtherFolder.OtherSubFolder.Person, Exception?> s_BeforeSomeMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, OtherFolder.OtherSubFolder.Person>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering SomeMethod with parameters: x = {x}, person = {person}");
        
        public void SomeMethod(int x, OtherFolder.OtherSubFolder.Person person)
        {
            _logger.LogInformation("Entering SomeMethod");
            _decorated.SomeMethod(x, person);
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, Exception?> s_BeforeSomeAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering SomeAsyncMethod with parameters: x = {x}, y = {y}");
        
        public System.Threading.Tasks.Task SomeAsyncMethod(int x, int y)
        {
            _logger.LogInformation("Entering SomeAsyncMethod");
            return _decorated.SomeAsyncMethod(x, y);
        }
    }
}
