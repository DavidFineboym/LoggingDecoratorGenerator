﻿//HintName: SomeServiceLoggingDecorator.g.cs
#nullable enable

namespace SomeFolder.SomeSubFolder
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Fineboym.Logging.Generator", "1.6.0.0")]
    public sealed class SomeServiceLoggingDecorator : ISomeService
    {
        private readonly global::Microsoft.Extensions.Logging.ILogger<ISomeService> _logger;
        private readonly ISomeService _decorated;

        public SomeServiceLoggingDecorator(global::Microsoft.Extensions.Logging.ILogger<ISomeService> logger, ISomeService decorated)
        {
            _logger = logger;
            _decorated = decorated;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_beforeVoidParameterlessMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define(
                global::Microsoft.Extensions.Logging.LogLevel.Trace,
                new global::Microsoft.Extensions.Logging.EventId(101, "foo"),
                "Entering VoidParameterlessMethod",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, double?, global::System.Exception?> s_afterVoidParameterlessMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<double?>(
                global::Microsoft.Extensions.Logging.LogLevel.Trace,
                new global::Microsoft.Extensions.Logging.EventId(101, "foo"),
                "Method VoidParameterlessMethod returned. DurationInMilliseconds = {durationInMilliseconds}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public void VoidParameterlessMethod()
        {
            global::System.Diagnostics.Stopwatch? __stopwatch = null;
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                s_beforeVoidParameterlessMethod(_logger, null);
                __stopwatch = global::System.Diagnostics.Stopwatch.StartNew();
            }
            _decorated.VoidParameterlessMethod();
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Trace))
            {
                s_afterVoidParameterlessMethod(_logger, __stopwatch?.Elapsed.TotalMilliseconds, null);
            }
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, global::OtherFolder.OtherSubFolder.Person, global::System.Exception?> s_beforeIntReturningMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int, global::OtherFolder.OtherSubFolder.Person>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(IntReturningMethod)),
                "Entering IntReturningMethod with parameters: x = {x}, person = {person}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, global::System.Exception?> s_afterIntReturningMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(IntReturningMethod)),
                "Method IntReturningMethod returned. Result = {result}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public int IntReturningMethod(int x, global::OtherFolder.OtherSubFolder.Person person)
        {
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_beforeIntReturningMethod(_logger, x, person, null);
            }
            var __result = _decorated.IntReturningMethod(x, person);
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_afterIntReturningMethod(_logger, __result, null);
            }
            return __result;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, int, global::System.Exception?> s_beforeTaskReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(TaskReturningAsyncMethod)),
                "Entering TaskReturningAsyncMethod with parameters: x = {x}, y = {y}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterTaskReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(TaskReturningAsyncMethod)),
                "Method TaskReturningAsyncMethod returned",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.Task TaskReturningAsyncMethod(int x, int y)
        {
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_beforeTaskReturningAsyncMethod(_logger, x, y, null);
            }
            await _decorated.TaskReturningAsyncMethod(x, y).ConfigureAwait(false);
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_afterTaskReturningAsyncMethod(_logger, null);
            }
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, int, global::System.Exception?> s_beforeTaskIntReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(TaskIntReturningAsyncMethod)),
                "Entering TaskIntReturningAsyncMethod with parameters: x = {x}, y = {y}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, global::System.Exception?> s_afterTaskIntReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(TaskIntReturningAsyncMethod)),
                "Method TaskIntReturningAsyncMethod returned. Result = {result}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.Task<int> TaskIntReturningAsyncMethod(int x, int y)
        {
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_beforeTaskIntReturningAsyncMethod(_logger, x, y, null);
            }
            var __result = await _decorated.TaskIntReturningAsyncMethod(x, y).ConfigureAwait(false);
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_afterTaskIntReturningAsyncMethod(_logger, __result, null);
            }
            return __result;
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, int, global::System.Exception?> s_beforeValueTaskReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(ValueTaskReturningAsyncMethod)),
                "Entering ValueTaskReturningAsyncMethod with parameters: x = {x}, y = {y}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.Exception?> s_afterValueTaskReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(ValueTaskReturningAsyncMethod)),
                "Method ValueTaskReturningAsyncMethod returned",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.ValueTask ValueTaskReturningAsyncMethod(int x, int y)
        {
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_beforeValueTaskReturningAsyncMethod(_logger, x, y, null);
            }
            await _decorated.ValueTaskReturningAsyncMethod(x, y).ConfigureAwait(false);
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_afterValueTaskReturningAsyncMethod(_logger, null);
            }
        }

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, int, int, global::System.Exception?> s_beforeValueTaskFloatReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<int, int>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(ValueTaskFloatReturningAsyncMethod)),
                "Entering ValueTaskFloatReturningAsyncMethod with parameters: x = {x}, y = {y}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, float, global::System.Exception?> s_afterValueTaskFloatReturningAsyncMethod
            = global::Microsoft.Extensions.Logging.LoggerMessage.Define<float>(
                global::Microsoft.Extensions.Logging.LogLevel.Debug,
                new global::Microsoft.Extensions.Logging.EventId(-1, nameof(ValueTaskFloatReturningAsyncMethod)),
                "Method ValueTaskFloatReturningAsyncMethod returned. Result = {result}",
                new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

        public async global::System.Threading.Tasks.ValueTask<float> ValueTaskFloatReturningAsyncMethod(int x, int y)
        {
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_beforeValueTaskFloatReturningAsyncMethod(_logger, x, y, null);
            }
            var __result = await _decorated.ValueTaskFloatReturningAsyncMethod(x, y).ConfigureAwait(false);
            if (_logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Debug))
            {
                s_afterValueTaskFloatReturningAsyncMethod(_logger, __result, null);
            }
            return __result;
        }
    }
}
