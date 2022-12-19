using Microsoft.CodeAnalysis;
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
            string className = $"{interfaceToGenerate.Interface.Name}LoggingDecorator";
            string interfaceFullName = $"{interfaceToGenerate.Interface}";
            string loggerType = $"Microsoft.Extensions.Logging.ILogger<{interfaceFullName}>";

            writer.WriteLine($"public sealed class {className} : {interfaceFullName}");
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
                AppendMethod(writer, method);
            }

            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
        }

        writer.Flush();

        return stringWriter.ToString();
    }

    private static void AppendMethod(IndentedTextWriter writer, IMethodSymbol method)
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
        writer.WriteLine($"_logger.LogInformation(\"Entering {method.Name}\");");
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
}