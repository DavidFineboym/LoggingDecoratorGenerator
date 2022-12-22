//HintName: SomeServiceLoggingDecorator.g.cs
namespace SomeFolder.SomeSubFolder
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
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, Exception?> s_beforeParameterlessMethod = Microsoft.Extensions.Logging.LoggerMessage.Define(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering ParameterlessMethod");
        
        public void ParameterlessMethod()
        {
            s_beforeParameterlessMethod(_logger, null);
            _decorated.ParameterlessMethod();
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, OtherFolder.OtherSubFolder.Person, Exception?> s_beforeVoidReturningMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, OtherFolder.OtherSubFolder.Person>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering VoidReturningMethod with parameters: x = {x}, person = {person}");
        
        public void VoidReturningMethod(int x, OtherFolder.OtherSubFolder.Person person)
        {
            s_beforeVoidReturningMethod(_logger, x, person, null);
            _decorated.VoidReturningMethod(x, person);
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, Exception?> s_beforeTaskReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering TaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public System.Threading.Tasks.Task TaskReturningAsyncMethod(int x, int y)
        {
            s_beforeTaskReturningAsyncMethod(_logger, x, y, null);
            return _decorated.TaskReturningAsyncMethod(x, y);
        }
    }
}
