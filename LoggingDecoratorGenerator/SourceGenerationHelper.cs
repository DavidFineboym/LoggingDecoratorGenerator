﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom.Compiler;

namespace LoggingDecoratorGenerator;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace LoggingDecoratorGenerator
{
    [System.AttributeUsage(System.AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class DecorateAttribute : System.Attribute
    {
    }
}";

    public static string GenerateLoggingDecoratorsClass(List<InterfaceToGenerate> interfacesToGenerate)
    {
        using StringWriter stringWriter = new();
        using IndentedTextWriter writer = new(stringWriter, "    ");
        writer.WriteLine("namespace LoggingDecoratorGenerator");
        writer.WriteLine("{");
        writer.Indent++;

        foreach (var interfaceToGenerate in interfacesToGenerate)
        {
            string interfaceName = interfaceToGenerate.Interface.Name;
            string className = $"{(interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName)}LoggingDecorator";
            string interfaceFullName = $"{interfaceToGenerate.Interface}";
            string loggerType = $"Microsoft.Extensions.Logging.ILogger<{interfaceFullName}>";

            writer.WriteLine($"{SyntaxFacts.GetText(interfaceToGenerate.Interface.DeclaredAccessibility)} sealed class {className} : {interfaceFullName}");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine($"private readonly {loggerType} _logger;");
            writer.WriteLine($"private readonly {interfaceFullName} _decorated;");
            writer.WriteLine();
            writer.WriteLine($"public {className}({loggerType} logger, {interfaceFullName} decorated)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("_logger = logger;");
            writer.WriteLine("_decorated = decorated;");
            writer.Indent--;
            writer.WriteLine("}");

            foreach (IMethodSymbol method in interfaceToGenerate.Methods)
            {
                writer.WriteLine();
                string loggerDelegateVariable = AppendLoggerMessageDefine(writer, method);
                writer.WriteLine();
                AppendMethod(writer, method, loggerDelegateVariable);
            }

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
        }

        writer.Flush();

        return stringWriter.ToString();
    }

    private static void AppendMethod(IndentedTextWriter writer, IMethodSymbol method, string loggerDelegateVariable)
    {
        writer.Write($"public {method.ReturnType} {method.Name}(");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type} {parameter.Name}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        writer.WriteLine(")");
        writer.WriteLine("{");
        writer.Indent++;
        writer.Write($"{loggerDelegateVariable}(_logger, ");
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
        if (method.ReturnType.SpecialType != SpecialType.System_Void)
        {
            writer.Write("return ");
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
        writer.WriteLine(");");
        writer.Indent--;
        writer.WriteLine("}");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="method"></param>
    /// <returns>Variable name of logger delegate.</returns>
    private static string AppendLoggerMessageDefine(IndentedTextWriter writer, IMethodSymbol method)
    {
        writer.Write("private static readonly System.Action<Microsoft.Extensions.Logging.ILogger, ");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
            else if (i == method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
        }
        string loggerVariable = $"s_Before{method.Name}";
        writer.Write($"Exception?> {loggerVariable} = Microsoft.Extensions.Logging.LoggerMessage.Define");
        for (int i = 0; i < method.Parameters.Length; i++)
        {
            if (i == 0)
            {
                writer.Write("<");
            }
            IParameterSymbol parameter = method.Parameters[i];
            writer.Write($"{parameter.Type}");
            if (i < method.Parameters.Length - 1)
            {
                writer.Write(", ");
            }
            else if (i == method.Parameters.Length - 1)
            {
                writer.Write(">");
            }
        }
        writer.Write($"(Microsoft.Extensions.Logging.LogLevel.Information, 0, \"Entering {method.Name}");
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
        writer.Write("\");");

        writer.WriteLine();

        return loggerVariable;
    }
}