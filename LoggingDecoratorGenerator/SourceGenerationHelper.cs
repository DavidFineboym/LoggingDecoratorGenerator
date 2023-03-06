﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;

namespace Fineboym.Logging.Generator;

internal static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace Fineboym.Logging.Generator
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    internal sealed class DecorateWithLoggerAttribute : System.Attribute
    {
        public Microsoft.Extensions.Logging.LogLevel Level { get; set; } = Microsoft.Extensions.Logging.LogLevel.Information;
    }
}";

    private static readonly SymbolDisplayFormat s_symbolFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static (string className, string source) GenerateLoggingDecoratorClass(InterfaceToGenerate interfaceToGenerate)
    {
        // TODO : Generate if log level enabled by myself like .NET and pass false to Define method.
        // new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true }
        // TODO : Allow specifying log level in attribute, on interface and then on method level for more granular control.
        // TODO : Allow specifying event ID on method level only.
        // TODO : Check generic interfaces and also generic methods in interfaces
        // TODO : Read Andrew's post about shipping the attribute together with the generator and how to do it.
        // TODO : Check non-public access modifiers 
        // TODO : Add GeneratedCodeAttribute where needed
        // TODO : Allow user to omit log parameters or return value from log
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

        writer.WriteLine($"{SyntaxFacts.GetText(interfaceToGenerate.Interface.DeclaredAccessibility)} sealed class {className} : {interfaceFullName}");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine($"private readonly {loggerType} _logger;");
        writer.WriteLine($"private readonly {interfaceFullName} _decorated;");
        writer.WriteLine();

        AppendConstructor(writer, className, interfaceFullName, loggerType);

        foreach (MethodToGenerate methodToGenerate in interfaceToGenerate.Methods)
        {
            writer.WriteLine();
            string loggerDelegateBeforeVariable = AppendLoggerMessageDefineForBeforeCall(writer, methodToGenerate.MethodSymbol);
            string loggerDelegateAfterVariable = AppendLoggerMessageDefineForAfterCall(writer, methodToGenerate);
            writer.WriteLine();
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

        writer.Write($"public {(awaitable ? "async " : string.Empty)}{method.ReturnType.ToDisplayString(s_symbolFormat)} {method.Name}(");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type.ToDisplayString(s_symbolFormat)} {parameter.Name}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine(")");
        writer.WriteLine("{");
        writer.Indent++;
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
        if (hasReturnValue)
        {
            writer.Write("var result = ");
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

        writer.Write($"{loggerDelegateAfterVariable}(_logger, ");
        if (hasReturnValue)
        {
            writer.WriteLine("result, null);");
            writer.WriteLine("return result;");
        }
        else
        {
            writer.WriteLine("null);");
        }
        writer.Indent--;
        writer.WriteLine("}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="method"></param>
    /// <returns>Variable name of logger delegate.</returns>
    private static string AppendLoggerMessageDefineForBeforeCall(IndentedTextWriter writer, IMethodSymbol method)
    {
        string loggerVariable = $"s_before{method.Name}";
        AppendLoggerMessageDefineUpToFormatString(writer, method.Parameters.Select(static p => p.Type).ToArray(), loggerVariable, method.Name);
        writer.Write($"\"Entering {method.Name}");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            if (i == 0)
            {
                writer.Write(" with parameters: ");
            }
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Name} = {{{parameter.Name}}}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine("\");");

        return loggerVariable;
    }

    private static string AppendLoggerMessageDefineForAfterCall(IndentedTextWriter writer, MethodToGenerate methodToGenerate)
    {
        IMethodSymbol method = methodToGenerate.MethodSymbol;
        bool hasReturnValue = methodToGenerate.HasReturnValue;
        bool awaitable = methodToGenerate.Awaitable;

        string loggerVariable = $"s_after{method.Name}";
        AppendLoggerMessageDefineUpToFormatString(
            writer,
            hasReturnValue ? new[] { awaitable ? methodToGenerate.UnwrappedReturnType! : method.ReturnType } : Array.Empty<ITypeSymbol>(),
            loggerVariable,
            method.Name);
        writer.Write($"\"Method {method.Name} returned");
        if (hasReturnValue)
        {
            writer.Write(". Result = {result}");
        }
        writer.WriteLine("\");");

        return loggerVariable;
    }

    private static void AppendLoggerMessageDefineUpToFormatString(IndentedTextWriter writer, IReadOnlyList<ITypeSymbol> types, string loggerVariable, string eventName)
    {
        writer.Write("private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, ");
        for (int i = 0; i < types.Count; i++)
        {
            ITypeSymbol type = types[i];
            writer.Write(type.ToDisplayString(s_symbolFormat));
            writer.Write(", ");
        }
        writer.Write($"global::System.Exception?> {loggerVariable} = global::Microsoft.Extensions.Logging.LoggerMessage.Define");
        for (int i = 0; i < types.Count; i++)
        {
            if (i == 0)
            {
                writer.Write("<");
            }
            ITypeSymbol type = types[i];
            writer.Write(type.ToDisplayString(s_symbolFormat));
            if (i < types.Count - 1)
            {
                writer.Write(", ");
            }
            else if (i == types.Count - 1)
            {
                writer.Write(">");
            }
        }
        writer.Write($"(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof({eventName})), ");
    }
}