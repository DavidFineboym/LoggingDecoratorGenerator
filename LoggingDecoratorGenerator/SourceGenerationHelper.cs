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

    public static readonly SymbolDisplayFormat SymbolFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public const string DecorateWithLoggerAttribute = @"#nullable enable
namespace Fineboym.Logging.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class DecorateWithLoggerAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; }

        public DecorateWithLoggerAttribute(Microsoft.Extensions.Logging.LogLevel level = Microsoft.Extensions.Logging.LogLevel.Debug)
        {
            Level = level;
        }
    }
}";

    public const string LogMethodAttribute = @"#nullable enable
namespace Fineboym.Logging.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal sealed class LogMethodAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; set; } = Microsoft.Extensions.Logging.LogLevel.None;

        /// <summary>
        /// Gets the logging event id for the logging method.
        /// </summary>
        public int EventId { get; set; } = -1;

        /// <summary>
        /// Gets or sets the logging event name for the logging method.
        /// </summary>
        /// <remarks>
        /// This will equal the method name if not specified.
        /// </remarks>
        public string? EventName { get; set; }

        /// <summary>
        /// Surrounds the method call by <see cref=""System.Diagnostics.Stopwatch""/> and logs duration in milliseconds. Default is false.
        /// </summary>
        public bool MeasureDuration { get; set; }

        /// <summary>
        /// By default, exceptions are not logged and there is no try-catch block around the method call.
        /// Set this property to some exception type to log exceptions of that type.
        /// </summary>
        public System.Type? ExceptionToLog { get; set; }

        /// <summary>
        /// If <see cref=""ExceptionToLog""/> is not null, then this controls log level for exceptions. Default is <see cref=""Microsoft.Extensions.Logging.LogLevel.Error""/>.
        /// </summary>
        public Microsoft.Extensions.Logging.LogLevel ExceptionLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
    }
}";

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

        writer.Write($"public {(awaitable ? "async " : string.Empty)}{method.ReturnType.ToDisplayString(SymbolFormat)} {method.Name}(");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type.ToDisplayString(SymbolFormat)} {parameter.Name}");
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
                writer.WriteLine($"{(awaitable ? methodToGenerate.UnwrappedReturnType! : method.ReturnType).ToDisplayString(SymbolFormat)} __result;");
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

    private static void AppendBeforeMethodSection(IndentedTextWriter writer, string loggerDelegateBeforeVariable, MethodToGenerate methodToGenerate)
    {
        if (methodToGenerate.MeasureDuration)
        {
            writer.WriteLine("global::System.Diagnostics.Stopwatch? __stopwatch = null;");
        }

        writer.WriteLine($"if (_logger.IsEnabled({methodToGenerate.LogLevel}))");
        writer.StartBlock();

        IMethodSymbol method = methodToGenerate.MethodSymbol;

        writer.Write($"{loggerDelegateBeforeVariable}(_logger, ");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Name}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
            else if (i == method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine("null);");

        if (methodToGenerate.MeasureDuration)
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
    private static string AppendLoggerMessageDefineForBeforeCall(IndentedTextWriter writer, MethodToGenerate methodToGenerate)
    {
        IMethodSymbol methodSymbol = methodToGenerate.MethodSymbol;
        string loggerVariable = $"s_before{methodToGenerate.UniqueName}";
        AppendLoggerMessageDefineUpToFormatString(
            writer,
            methodSymbol.Parameters.Select(static p => p.Type).ToArray(),
            loggerVariable,
            methodToGenerate);
        writer.Write($"\"Entering {methodSymbol.Name}");
        for (int i = 0; i < methodSymbol.Parameters.Length; i++)
        {
            if (i == 0)
            {
                writer.Write(" with parameters: ");
            }
            IParameterSymbol parameter = methodSymbol.Parameters[i];
            writer.Write($"{parameter.Name} = {{{parameter.Name}}}");
            if (i < methodSymbol.Parameters.Length - 1)
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
            types.Add(returnType.ToDisplayString(SymbolFormat));
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
        IReadOnlyList<ITypeSymbol> types,
        string loggerVariable,
        MethodToGenerate methodToGenerate) => AppendLoggerMessageDefineUpToFormatString(writer,
                                                                                        types.Select(static t => t.ToDisplayString(SymbolFormat)).ToArray(),
                                                                                        loggerVariable,
                                                                                        methodToGenerate);

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