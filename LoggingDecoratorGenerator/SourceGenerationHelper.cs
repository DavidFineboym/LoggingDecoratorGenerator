using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;

namespace Fineboym.Logging.Generator;

internal static class SourceGenerationHelper
{
    private const string LogEnabledBoolVar = "__logEnabled";
    private const string DurationMetricEnabledBoolVar = "__metricEnabled";
    private const string ElapsedTimeVar = "__elapsedTime";

    private static readonly string s_generatedCodeAttribute =
               $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(" +
               $"\"{typeof(SourceGenerationHelper).Assembly.GetName().Name}\", " +
               $"\"{typeof(SourceGenerationHelper).Assembly.GetName().Version}\")]";

    public static string GenerateLoggingDecoratorClass(DecoratorClass decoratorClass, bool stopwatchGetElapsedTimeAvailable)
    {
        using StringWriter stringWriter = new();
        using IndentedTextWriter writer = new(stringWriter, "    ");
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine($"namespace {decoratorClass.Namespace}");
        writer.StartBlock();

        string interfaceName = decoratorClass.InterfaceName;
        string loggerType = $"global::Microsoft.Extensions.Logging.ILogger<{interfaceName}>";

        writer.WriteLine(s_generatedCodeAttribute);
        writer.WriteLine($"{decoratorClass.DeclaredAccessibility} sealed class {decoratorClass.ClassName} : {interfaceName}");
        writer.StartBlock();
        writer.WriteLine($"private readonly {loggerType} _logger;");
        writer.WriteLine($"private readonly {interfaceName} _decorated;");
        if (decoratorClass.NeedsDurationAsMetric)
        {
            writer.WriteLine("private readonly global::System.Diagnostics.Metrics.Histogram<double> _methodDuration;");
        }
        writer.WriteLineNoTabs(null);

        AppendConstructor(writer, decoratorClass, loggerType);

        foreach (MethodToGenerate methodToGenerate in decoratorClass.Methods)
        {
            writer.WriteLineNoTabs(null);

            if (methodToGenerate.LogLevel == null)
            {
                AppendPassThroughMethod(writer, methodToGenerate);
            }
            else
            {
                string loggerDelegateBeforeVariable = AppendLoggerMessageDefineForBeforeCall(writer, methodToGenerate);
                string loggerDelegateAfterVariable = AppendLoggerMessageDefineForAfterCall(writer, methodToGenerate, decoratorClass.DurationAsMetric);
                AppendMethod(writer, methodToGenerate, loggerDelegateBeforeVariable, loggerDelegateAfterVariable, stopwatchGetElapsedTimeAvailable, decoratorClass.DurationAsMetric);
            }
        }

        if (!stopwatchGetElapsedTimeAvailable && decoratorClass.SomeMethodMeasuresDuration)
        {
            writer.WriteLineNoTabs(null);
            AppendGetElapsedTimeSection(writer);
        }

        writer.EndBlock();
        writer.EndBlock();

        writer.Flush();

        return stringWriter.ToString();
    }

    private static void AppendGetElapsedTimeSection(IndentedTextWriter writer)
    {
        writer.WriteLine("private static readonly double s_timestampToTicks = global::System.TimeSpan.TicksPerSecond / (double)global::System.Diagnostics.Stopwatch.Frequency;");
        writer.WriteLineNoTabs(null);
        writer.WriteLine("private static global::System.TimeSpan __GetElapsedTime__(long startTimestamp)");
        writer.StartBlock();
        writer.WriteLine("var end = global::System.Diagnostics.Stopwatch.GetTimestamp();");
        writer.WriteLine("var timestampDelta = end - startTimestamp;");
        writer.WriteLine("var ticks = (long)(s_timestampToTicks * timestampDelta);");
        writer.WriteLine("return new global::System.TimeSpan(ticks);");
        writer.EndBlock();
    }

    private static void AppendConstructor(IndentedTextWriter writer, DecoratorClass decClass, string loggerType)
    {
        writer.WriteLine($"public {decClass.ClassName}(");
        writer.Indent++;
        writer.WriteLine($"{loggerType} logger,");
        writer.Write($"{decClass.InterfaceName} decorated");
        if (decClass.NeedsDurationAsMetric)
        {
            writer.WriteLine(",");
            writer.Write("global::System.Diagnostics.Metrics.IMeterFactory meterFactory");
        }
        writer.WriteLine(")");
        writer.Indent--;
        writer.StartBlock();
        writer.WriteLine("_logger = logger;");
        writer.WriteLine("_decorated = decorated;");
        if (decClass.NeedsDurationAsMetric)
        {
            writer.WriteLine($"var meterOptions = new global::System.Diagnostics.Metrics.MeterOptions(name: typeof({decClass.InterfaceName}).ToString());");
            writer.WriteLine("var meter = meterFactory.Create(meterOptions);");
            writer.WriteLine("var tags = new global::System.Diagnostics.TagList();");
            writer.WriteLine("tags.Add(key: \"logging_decorator.type\", value: decorated.GetType().ToString());");
            writer.WriteLine("_methodDuration = meter.CreateHistogram<double>(");
            writer.Indent++;
            writer.WriteLine("name: \"logging_decorator.method.duration\",");
            writer.WriteLine("unit: \"s\",");
            writer.WriteLine("description: \"The duration of method invocations.\",");
            writer.WriteLine("tags);");
            writer.Indent--;
        }
        writer.EndBlock();
    }

    private static void AppendPassThroughMethod(IndentedTextWriter writer, MethodToGenerate methodToGenerate)
    {
        var method = methodToGenerate.MethodSymbol;
        AppendMethodSignature(writer, methodToGenerate);
        writer.Indent++;
        writer.Write("=> ");
        AppendCallToDecoratedInstance(writer, method);
        writer.WriteLine(';');
        writer.Indent--;
    }

    private static void AppendMethod(
        IndentedTextWriter writer,
        MethodToGenerate methodToGenerate,
        string loggerDelegateBeforeVariable,
        string loggerDelegateAfterVariable,
        bool stopwatchGetElapsedTimeAvailable,
        bool durationAsMetric)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool awaitable = methodToGenerate.Awaitable;
        bool hasReturnValue = methodToGenerate.HasReturnValue;
        AppendMethodSignature(writer, methodToGenerate);
        writer.StartBlock();

        AppendBeforeMethodSection(writer, loggerDelegateBeforeVariable, methodToGenerate, durationAsMetric);

        if (methodToGenerate.ExceptionTypeToLog != null)
        {
            if (hasReturnValue)
            {
                writer.WriteLine($"{(awaitable ? methodToGenerate.UnwrappedReturnType! : method.ReturnType).ToFullyQualifiedDisplayString()} __result;");
            }

            writer.WriteLine("try");
            writer.StartBlock();
        }
        else if (hasReturnValue)
        {
            writer.Write("var ");
        }

        if (hasReturnValue)
        {
            writer.Write("__result = ");
        }

        if (awaitable)
        {
            writer.Write("await ");
        }

        AppendCallToDecoratedInstance(writer, method);

        if (awaitable)
        {
            writer.Write(".ConfigureAwait(false)");
        }
        writer.WriteLine(';');

        if (methodToGenerate.ExceptionTypeToLog != null)
        {
            writer.EndBlock();
            writer.WriteLine($"catch ({methodToGenerate.ExceptionTypeToLog} __e)");
            writer.StartBlock();
            writer.WriteLine("global::Microsoft.Extensions.Logging.LoggerExtensions.Log(");
            writer.Indent++;
            writer.WriteLine("_logger,");
            writer.WriteLine($"{methodToGenerate.ExceptionLogLevel},");
            writer.WriteLine($"new global::Microsoft.Extensions.Logging.EventId({methodToGenerate.EventId}, {methodToGenerate.EventName}),");
            writer.WriteLine("__e,");
            writer.WriteLine($"\"{method.Name} failed\");");
            writer.Indent--;
            writer.WriteLineNoTabs(null);
            writer.WriteLine("throw;");
            writer.EndBlock();
        }

        writer.WriteLineNoTabs(null);

        AppendAfterMethodSection(writer, loggerDelegateAfterVariable, methodToGenerate, stopwatchGetElapsedTimeAvailable, durationAsMetric);

        writer.EndBlock();
    }

    private static void AppendCallToDecoratedInstance(IndentedTextWriter writer, IMethodSymbol method)
    {
        writer.Write($"_decorated.{method.Name}(");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Name}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.Write(')');
    }

    private static void AppendMethodSignature(IndentedTextWriter writer, MethodToGenerate methodToGenerate)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool awaitable = methodToGenerate.Awaitable;
        bool passThrough = methodToGenerate.LogLevel == null;

        writer.Write($"public {(awaitable && !passThrough ? "async " : string.Empty)}{method.ReturnType.ToFullyQualifiedDisplayString()} {method.Name}(");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type.ToFullyQualifiedDisplayString()} {parameter.Name}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine(")");
    }

    private static void AppendBeforeMethodSection(IndentedTextWriter writer, string loggerDelegateBeforeVariable, MethodToGenerate method, bool durationAsMetric)
    {
        writer.WriteLine($"var {LogEnabledBoolVar} = _logger.IsEnabled({method.LogLevel});");

        if (method.MeasureDuration)
        {
            if (durationAsMetric)
            {
                writer.WriteLine($"var {DurationMetricEnabledBoolVar} = _methodDuration.Enabled;");
            }
            writer.WriteLine("global::System.Int64 __startTimestamp = 0;");
        }

        writer.WriteLineNoTabs(null);

        writer.WriteLine($"if ({LogEnabledBoolVar})");
        writer.StartBlock();

        writer.Write($"{loggerDelegateBeforeVariable}(_logger, ");
        foreach (Parameter loggedParameter in method.Parameters.Where(static p => p.IsLogged))
        {
            writer.Write($"{loggedParameter.Symbol.Name}, ");
        }
        writer.WriteLine("null);");

        const string startTimestampInit = "__startTimestamp = global::System.Diagnostics.Stopwatch.GetTimestamp();";
        if (method.MeasureDuration && !durationAsMetric)
        {
            writer.WriteLine(startTimestampInit);
        }

        writer.EndBlock();

        writer.WriteLineNoTabs(null);

        if (method.MeasureDuration && durationAsMetric)
        {
            writer.WriteLine($"if ({DurationMetricEnabledBoolVar})");
            writer.StartBlock();
            writer.WriteLine(startTimestampInit);
            writer.EndBlock();

            writer.WriteLineNoTabs(null);
        }
    }

    private static void AppendAfterMethodSection(
        IndentedTextWriter writer,
        string loggerDelegateAfterVariable,
        MethodToGenerate methodToGenerate,
        bool stopwatchGetElapsedTimeAvailable,
        bool durationAsMetric)
    {
        if (methodToGenerate.MeasureDuration && durationAsMetric)
        {
            writer.WriteLine($"if ({DurationMetricEnabledBoolVar})");
            writer.StartBlock();
            AppendGetElapsedTime(writer, stopwatchGetElapsedTimeAvailable);
            writer.WriteLine($"_methodDuration.Record({ElapsedTimeVar}.TotalSeconds,");
            writer.Indent++;
            writer.WriteLine($"new global::System.Collections.Generic.KeyValuePair<string, object?>(\"logging_decorator.method\", nameof({methodToGenerate.MethodSymbol.Name})));");
            writer.Indent--;
            writer.EndBlock();

            writer.WriteLineNoTabs(null);
        }

        writer.WriteLine($"if ({LogEnabledBoolVar})");
        writer.StartBlock();
        bool loggingDuration = methodToGenerate.MeasureDuration && !durationAsMetric;
        if (loggingDuration)
        {
            AppendGetElapsedTime(writer, stopwatchGetElapsedTimeAvailable);
        }

        writer.Write($"{loggerDelegateAfterVariable}(_logger, ");
        if (methodToGenerate.HasReturnValue && methodToGenerate.ReturnValueLogged)
        {
            writer.Write("__result, ");
        }

        if (loggingDuration)
        {
            writer.Write($"{ElapsedTimeVar}.TotalMilliseconds, ");
        }

        writer.WriteLine("null);");

        writer.EndBlock();

        if (methodToGenerate.HasReturnValue)
        {
            writer.WriteLineNoTabs(null);
            writer.WriteLine("return __result;");
        }
    }

    private static void AppendGetElapsedTime(IndentedTextWriter writer, bool stopwatchGetElapsedTimeAvailable)
    {
        if (!stopwatchGetElapsedTimeAvailable)
        {
            writer.WriteLine($"var {ElapsedTimeVar} = __GetElapsedTime__(__startTimestamp);");
        }
        else
        {
            writer.WriteLine($"var {ElapsedTimeVar} = global::System.Diagnostics.Stopwatch.GetElapsedTime(__startTimestamp);");
        }
    }

    private static void StartBlock(this IndentedTextWriter writer)
    {
        writer.WriteLine('{');
        writer.Indent++;
    }

    private static void EndBlock(this IndentedTextWriter writer)
    {
        writer.Indent--;
        writer.WriteLine('}');
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="method"></param>
    /// <returns>Variable name of logger delegate.</returns>
    private static string AppendLoggerMessageDefineForBeforeCall(IndentedTextWriter writer, MethodToGenerate method)
    {
        IMethodSymbol methodSymbol = method.MethodSymbol;
        string loggerVariable = $"s_before{method.UniqueName}";
        AppendLoggerMessageDefineUpToFormatString(
            writer,
            method.Parameters.Where(static p => p.IsLogged).Select(static p => p.Symbol.Type.ToFullyQualifiedDisplayString()).ToArray(),
            loggerVariable,
            method);
        writer.Write($"\"Entering {methodSymbol.Name}");
        for (int i = 0; i < method.Parameters.Count; i++)
        {
            if (i == 0)
            {
                writer.Write(" with parameters: ");
            }

            Parameter parameter = method.Parameters[i];
            IParameterSymbol parameterSymbol = parameter.Symbol;
            if (parameter.IsLogged)
            {
                writer.Write($"{parameterSymbol.Name} = {{{parameterSymbol.Name}}}");
            }
            else
            {
                writer.Write($"{parameterSymbol.Name} = [REDACTED]");
            }

            if (i < method.Parameters.Count - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine("\",");
        FinishByLogDefineOptions(writer);

        return loggerVariable;
    }

    private static string AppendLoggerMessageDefineForAfterCall(IndentedTextWriter writer, MethodToGenerate methodToGenerate, bool durationAsMetric)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool hasReturnValue = methodToGenerate.HasReturnValue;
        bool awaitable = methodToGenerate.Awaitable;

        string loggerVariable = $"s_after{methodToGenerate.UniqueName}";

        List<string> types = new();

        if (hasReturnValue && methodToGenerate.ReturnValueLogged)
        {
            ITypeSymbol returnType = awaitable ? methodToGenerate.UnwrappedReturnType! : method.ReturnType;
            types.Add(returnType.ToFullyQualifiedDisplayString());
        }

        bool loggingDuration = methodToGenerate.MeasureDuration && !durationAsMetric;
        if (loggingDuration)
        {
            types.Add("double?");
        }

        AppendLoggerMessageDefineUpToFormatString(
            writer,
            types,
            loggerVariable,
            methodToGenerate);

        writer.Write($"\"Method {method.Name} returned");
        if (hasReturnValue)
        {
            if (methodToGenerate.ReturnValueLogged)
            {
                writer.Write(". Result = {result}");
            }
            else
            {
                writer.Write(". Result = [REDACTED]");
            }
        }

        if (loggingDuration)
        {
            writer.Write(". DurationInMilliseconds = {durationInMilliseconds}");
        }

        writer.WriteLine("\",");
        FinishByLogDefineOptions(writer);

        return loggerVariable;
    }

    private static void AppendLoggerMessageDefineUpToFormatString(
        IndentedTextWriter writer,
        IReadOnlyList<string> types,
        string loggerVariable,
        MethodToGenerate methodToGenerate)
    {
        writer.Write("private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, ");
        for (int i = 0; i < types.Count; i++)
        {
            writer.Write(types[i]);
            writer.Write(", ");
        }
        writer.WriteLine($"global::System.Exception?> {loggerVariable}");
        writer.Indent++;
        writer.Write("= global::Microsoft.Extensions.Logging.LoggerMessage.Define");

        for (int i = 0; i < types.Count; i++)
        {
            if (i == 0)
            {
                writer.Write("<");
            }

            writer.Write(types[i]);
            if (i < types.Count - 1)
            {
                writer.Write(", ");
            }
            else if (i == types.Count - 1)
            {
                writer.Write(">");
            }
        }

        writer.WriteLine("(");
        writer.Indent++;
        writer.WriteLine($"{methodToGenerate.LogLevel},");
        writer.WriteLine($"new global::Microsoft.Extensions.Logging.EventId({methodToGenerate.EventId}, {methodToGenerate.EventName}),");
    }

    private static void FinishByLogDefineOptions(IndentedTextWriter writer)
    {
        writer.WriteLine("new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });");
        writer.Indent -= 2;
        writer.WriteLineNoTabs(null);
    }
}