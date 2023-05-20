using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;

namespace Fineboym.Logging.Generator;

internal static class SourceGenerationHelper
{
    private static readonly string s_generatedCodeAttribute =
               $"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(" +
               $"\"{typeof(SourceGenerationHelper).Assembly.GetName().Name}\", " +
               $"\"{typeof(SourceGenerationHelper).Assembly.GetName().Version}\")]";

    public static (string className, string source) GenerateLoggingDecoratorClass(InterfaceToGenerate interfaceToGenerate)
    {
        using StringWriter stringWriter = new();
        using IndentedTextWriter writer = new(stringWriter, "    ");
        writer.WriteLine("#nullable enable");
        writer.WriteLine();
        writer.WriteLine($"namespace {interfaceToGenerate.Namespace}");
        writer.WriteLine("{");
        writer.Indent++;

        string interfaceName = interfaceToGenerate.Interface.Name;
        string className = $"{(interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName)}LoggingDecorator";
        string interfaceFullName = $"{interfaceToGenerate.Interface}";
        string loggerType = $"global::Microsoft.Extensions.Logging.ILogger<{interfaceFullName}>";

        writer.WriteLine(s_generatedCodeAttribute);
        writer.WriteLine($"{SyntaxFacts.GetText(interfaceToGenerate.Interface.DeclaredAccessibility)} sealed class {className} : {interfaceFullName}");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine($"private readonly {loggerType} _logger;");
        writer.WriteLine($"private readonly {interfaceFullName} _decorated;");
        writer.WriteLineNoTabs(null);

        AppendConstructor(writer, className, interfaceFullName, loggerType);

        foreach (MethodToGenerate methodToGenerate in interfaceToGenerate.Methods)
        {
            writer.WriteLineNoTabs(null);
            string loggerDelegateBeforeVariable = AppendLoggerMessageDefineForBeforeCall(writer, methodToGenerate);
            string loggerDelegateAfterVariable = AppendLoggerMessageDefineForAfterCall(writer, methodToGenerate);
            AppendMethod(writer, methodToGenerate, loggerDelegateBeforeVariable, loggerDelegateAfterVariable);
        }

        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");

        writer.Flush();

        return (className, stringWriter.ToString());
    }

    private static void AppendConstructor(IndentedTextWriter writer, string className, string interfaceFullName, string loggerType)
    {
        writer.WriteLine($"public {className}({loggerType} logger, {interfaceFullName} decorated)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("_logger = logger;");
        writer.WriteLine("_decorated = decorated;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void AppendMethod(IndentedTextWriter writer, MethodToGenerate methodToGenerate, string loggerDelegateBeforeVariable, string loggerDelegateAfterVariable)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool awaitable = methodToGenerate.Awaitable;
        bool hasReturnValue = methodToGenerate.HasReturnValue;

        writer.Write($"public {(awaitable ? "async " : string.Empty)}{method.ReturnType.ToFullyQualifiedDisplayString()} {method.Name}(");
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
        writer.WriteLine("{");
        writer.Indent++;

        AppendBeforeMethodSection(writer, loggerDelegateBeforeVariable, methodToGenerate);

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
            writer.WriteLine("throw;");
            writer.EndBlock();
        }

        AppendAfterMethodSection(writer, loggerDelegateAfterVariable, methodToGenerate);

        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void AppendBeforeMethodSection(IndentedTextWriter writer, string loggerDelegateBeforeVariable, MethodToGenerate method)
    {
        if (method.MeasureDuration)
        {
            writer.WriteLine("global::System.Diagnostics.Stopwatch? __stopwatch = null;");
        }

        writer.WriteLine($"if (_logger.IsEnabled({method.LogLevel}))");
        writer.StartBlock();

        writer.Write($"{loggerDelegateBeforeVariable}(_logger, ");
        foreach (Parameter loggedParameter in method.Parameters.Where(static p => p.IsLogged))
        {
            writer.Write($"{loggedParameter.Symbol.Name}, ");
        }
        writer.WriteLine("null);");

        if (method.MeasureDuration)
        {
            writer.WriteLine("__stopwatch = global::System.Diagnostics.Stopwatch.StartNew();");
        }

        writer.EndBlock();
    }

    private static void AppendAfterMethodSection(IndentedTextWriter writer, string loggerDelegateAfterVariable, MethodToGenerate methodToGenerate)
    {
        writer.WriteLine($"if (_logger.IsEnabled({methodToGenerate.LogLevel}))");
        writer.StartBlock();

        writer.Write($"{loggerDelegateAfterVariable}(_logger, ");
        if (methodToGenerate.HasReturnValue)
        {
            writer.Write("__result, ");
        }

        if (methodToGenerate.MeasureDuration)
        {
            writer.Write("__stopwatch?.Elapsed.TotalMilliseconds, ");
        }

        writer.WriteLine("null);");

        writer.EndBlock();

        if (methodToGenerate.HasReturnValue)
        {
            writer.WriteLine("return __result;");
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

    private static string AppendLoggerMessageDefineForAfterCall(IndentedTextWriter writer, MethodToGenerate methodToGenerate)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool hasReturnValue = methodToGenerate.HasReturnValue;
        bool awaitable = methodToGenerate.Awaitable;

        string loggerVariable = $"s_after{methodToGenerate.UniqueName}";

        List<string> types = new();

        if (hasReturnValue)
        {
            ITypeSymbol returnType = awaitable ? methodToGenerate.UnwrappedReturnType! : method.ReturnType;
            types.Add(returnType.ToFullyQualifiedDisplayString());
        }

        if (methodToGenerate.MeasureDuration)
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
            writer.Write(". Result = {result}");
        }

        if (methodToGenerate.MeasureDuration)
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