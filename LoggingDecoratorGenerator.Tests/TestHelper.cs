using Fineboym.Logging.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace LoggingDecoratorGenerator.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create references for assemblies we require
        // We could add multiple references if required
        var loggerAssembly = typeof(Microsoft.Extensions.Logging.LogLevel).Assembly;
        List<PortableExecutableReference> references = new()
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(loggerAssembly.Location),
        };

        foreach (AssemblyName assemblyName in loggerAssembly.GetReferencedAssemblies())
        {
            references.Add(MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location));
        }

        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references);

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new DecoratorGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}
