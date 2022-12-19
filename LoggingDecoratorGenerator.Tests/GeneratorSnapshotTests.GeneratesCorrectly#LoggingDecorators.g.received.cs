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
        
        public void SomeMethod(int x, OtherFolder.OtherSubFolder.Person person)
        {
            _logger.LogInformation("Entering SomeMethod");
            _decorated.SomeMethod(x, person);
        }
        
        public System.Threading.Tasks.Task SomeAsyncMethod(int x, int y)
        {
            _logger.LogInformation("Entering SomeAsyncMethod");
            return _decorated.SomeAsyncMethod(x, y);
        }
    }
}
