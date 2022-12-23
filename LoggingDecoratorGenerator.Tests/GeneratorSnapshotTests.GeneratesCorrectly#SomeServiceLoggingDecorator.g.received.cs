//HintName: SomeServiceLoggingDecorator.g.cs
#nullable enable

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
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, System.Exception?> s_beforeVoidParameterlessMethod = Microsoft.Extensions.Logging.LoggerMessage.Define(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering VoidParameterlessMethod");
        
        public void VoidParameterlessMethod()
        {
            s_beforeVoidParameterlessMethod(_logger, null);
            _decorated.VoidParameterlessMethod();
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, OtherFolder.OtherSubFolder.Person, System.Exception?> s_beforeIntReturningMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, OtherFolder.OtherSubFolder.Person>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering IntReturningMethod with parameters: x = {x}, person = {person}");
        
        public int IntReturningMethod(int x, OtherFolder.OtherSubFolder.Person person)
        {
            s_beforeIntReturningMethod(_logger, x, person, null);
            var result = _decorated.IntReturningMethod(x, person);
            return result;
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, System.Exception?> s_beforeTaskReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering TaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public async System.Threading.Tasks.Task TaskReturningAsyncMethod(int x, int y)
        {
            s_beforeTaskReturningAsyncMethod(_logger, x, y, null);
            await _decorated.TaskReturningAsyncMethod(x, y);
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, System.Exception?> s_beforeTaskIntReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering TaskIntReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public async System.Threading.Tasks.Task<int> TaskIntReturningAsyncMethod(int x, int y)
        {
            s_beforeTaskIntReturningAsyncMethod(_logger, x, y, null);
            var result = await _decorated.TaskIntReturningAsyncMethod(x, y);
            return result;
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, System.Exception?> s_beforeValueTaskReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering ValueTaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public async System.Threading.Tasks.ValueTask ValueTaskReturningAsyncMethod(int x, int y)
        {
            s_beforeValueTaskReturningAsyncMethod(_logger, x, y, null);
            await _decorated.ValueTaskReturningAsyncMethod(x, y);
        }
        
        private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, int, int, System.Exception?> s_beforeValueTaskFloatReturningAsyncMethod = Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(Microsoft.Extensions.Logging.LogLevel.Information, 0, "Entering ValueTaskFloatReturningAsyncMethod with parameters: x = {x}, y = {y}");
        
        public async System.Threading.Tasks.ValueTask<float> ValueTaskFloatReturningAsyncMethod(int x, int y)
        {
            s_beforeValueTaskFloatReturningAsyncMethod(_logger, x, y, null);
            var result = await _decorated.ValueTaskFloatReturningAsyncMethod(x, y);
            return result;
        }
    }
}
