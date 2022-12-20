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
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, Exception?> s_BeforeParameterlessMethod = Microsoft.Extensions.Logging.LoggerMessage.Define(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering ParameterlessMethod");
        
        public void ParameterlessMethod()
        {
            s_BeforeParameterlessMethod(_logger, null);
            _decorated.ParameterlessMethod();
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, OtherFolder.OtherSubFolder.Person, Exception?> s_BeforeVoidReturningMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, OtherFolder.OtherSubFolder.Person>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering VoidReturningMethod with parameters: x = {x}, person = {person}");
        
        public void VoidReturningMethod(int x, OtherFolder.OtherSubFolder.Person person)
        {
            s_BeforeVoidReturningMethod(_logger, x, person, null);
            _decorated.VoidReturningMethod(x, person);
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, Exception?> s_BeforeTaskReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering TaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public System.Threading.Tasks.Task TaskReturningAsyncMethod(int x, int y)
        {
            s_BeforeTaskReturningAsyncMethod(_logger, x, y, null);
            return _decorated.TaskReturningAsyncMethod(x, y);
        }
    }
}
