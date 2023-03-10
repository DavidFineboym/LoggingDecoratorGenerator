//HintName: SomeServiceLoggingDecorator.g.cs
#nullable enable

namespace SomeFolder.SomeSubFolder
{
    public sealed class SomeServiceLoggingDecorator : SomeFolder.SomeSubFolder.ISomeService
    {
        private readonly global::Microsoft.Extensions.Logging.ILogger<SomeFolder.SomeSubFolder.ISomeService> _logger;
        private readonly SomeFolder.SomeSubFolder.ISomeService _decorated;
        
        public SomeServiceLoggingDecorator(global::Microsoft.Extensions.Logging.ILogger<SomeFolder.SomeSubFolder.ISomeService> logger, SomeFolder.SomeSubFolder.ISomeService decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_beforeVoidParameterlessMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define(global::Microsoft.Extensions.Logging.LogLevel.Debug, new global::Microsoft.Extensions.Logging.EventId(0, nameof(VoidParameterlessMethod)), "Entering VoidParameterlessMethod");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterVoidParameterlessMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define(global::Microsoft.Extensions.Logging.LogLevel.Debug, new global::Microsoft.Extensions.Logging.EventId(0, nameof(VoidParameterlessMethod)), "Method VoidParameterlessMethod returned");
        
        public global::System.Void VoidParameterlessMethod()
        {
            s_beforeVoidParameterlessMethod(_logger, null);
            _decorated.VoidParameterlessMethod();
            s_afterVoidParameterlessMethod(_logger, null);
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::OtherFolder.OtherSubFolder.Person, global::System.Exception?> s_beforeIntReturningMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32, global::OtherFolder.OtherSubFolder.Person>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(IntReturningMethod)), "Entering IntReturningMethod with parameters: x = {x}, person = {person}");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Exception?> s_afterIntReturningMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(IntReturningMethod)), "Method IntReturningMethod returned. Result = {result}");
        
        public global::System.Int32 IntReturningMethod(global::System.Int32 x, global::OtherFolder.OtherSubFolder.Person person)
        {
            s_beforeIntReturningMethod(_logger, x, person, null);
            var result = _decorated.IntReturningMethod(x, person);
            s_afterIntReturningMethod(_logger, result, null);
            return result;
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Int32, global::System.Exception?> s_beforeTaskReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32, global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(TaskReturningAsyncMethod)), "Entering TaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterTaskReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(TaskReturningAsyncMethod)), "Method TaskReturningAsyncMethod returned");
        
        public async global::System.Threading.Tasks.Task TaskReturningAsyncMethod(global::System.Int32 x, global::System.Int32 y)
        {
            s_beforeTaskReturningAsyncMethod(_logger, x, y, null);
            await _decorated.TaskReturningAsyncMethod(x, y).ConfigureAwait(false);
            s_afterTaskReturningAsyncMethod(_logger, null);
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Int32, global::System.Exception?> s_beforeTaskIntReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32, global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(TaskIntReturningAsyncMethod)), "Entering TaskIntReturningAsyncMethod with parameters: x = {x}, y = {y}");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Exception?> s_afterTaskIntReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(TaskIntReturningAsyncMethod)), "Method TaskIntReturningAsyncMethod returned. Result = {result}");
        
        public async global::System.Threading.Tasks.Task<global::System.Int32> TaskIntReturningAsyncMethod(global::System.Int32 x, global::System.Int32 y)
        {
            s_beforeTaskIntReturningAsyncMethod(_logger, x, y, null);
            var result = await _decorated.TaskIntReturningAsyncMethod(x, y).ConfigureAwait(false);
            s_afterTaskIntReturningAsyncMethod(_logger, result, null);
            return result;
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Int32, global::System.Exception?> s_beforeValueTaskReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32, global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(ValueTaskReturningAsyncMethod)), "Entering ValueTaskReturningAsyncMethod with parameters: x = {x}, y = {y}");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterValueTaskReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(ValueTaskReturningAsyncMethod)), "Method ValueTaskReturningAsyncMethod returned");
        
        public async global::System.Threading.Tasks.ValueTask ValueTaskReturningAsyncMethod(global::System.Int32 x, global::System.Int32 y)
        {
            s_beforeValueTaskReturningAsyncMethod(_logger, x, y, null);
            await _decorated.ValueTaskReturningAsyncMethod(x, y).ConfigureAwait(false);
            s_afterValueTaskReturningAsyncMethod(_logger, null);
        }
        
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Int32, global::System.Int32, global::System.Exception?> s_beforeValueTaskFloatReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Int32, global::System.Int32>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(ValueTaskFloatReturningAsyncMethod)), "Entering ValueTaskFloatReturningAsyncMethod with parameters: x = {x}, y = {y}");
        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Single, global::System.Exception?> s_afterValueTaskFloatReturningAsyncMethod = global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.Single>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(ValueTaskFloatReturningAsyncMethod)), "Method ValueTaskFloatReturningAsyncMethod returned. Result = {result}");
        
        public async global::System.Threading.Tasks.ValueTask<global::System.Single> ValueTaskFloatReturningAsyncMethod(global::System.Int32 x, global::System.Int32 y)
        {
            s_beforeValueTaskFloatReturningAsyncMethod(_logger, x, y, null);
            var result = await _decorated.ValueTaskFloatReturningAsyncMethod(x, y).ConfigureAwait(false);
            s_afterValueTaskFloatReturningAsyncMethod(_logger, result, null);
            return result;
        }
    }
}
