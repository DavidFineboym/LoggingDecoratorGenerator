using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Fineboym.Logging.Generator;

[Generator]
public class DecoratorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attributes to the compilation
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource($"{Attributes.DecorateWithLoggerName}.g.cs", SourceText.From(Attributes.DecorateWithLogger, Encoding.UTF8));
            ctx.AddSource($"{Attributes.LogMethodName}.g.cs", SourceText.From(Attributes.LogMethod, Encoding.UTF8));
            ctx.AddSource($"{Attributes.NotLoggedName}.g.cs", SourceText.From(Attributes.NotLogged, Encoding.UTF8));
        });

        // Do a simple filter for interfaces
        IncrementalValuesProvider<InterfaceDeclarationSyntax> interfaceDeclarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                Attributes.DecorateWithLoggerFullName,
                predicate: static (node, _) => node is InterfaceDeclarationSyntax,
                transform: static (context, _) => context.TargetNode as InterfaceDeclarationSyntax)
            .Where(predicate: static m => m is not null)!;

        // Combine the selected interfaces with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<InterfaceDeclarationSyntax>)> compilationAndInterfaces
            = context.CompilationProvider.Combine(provider2: interfaceDeclarations.Collect());

        // Generate the source using the compilation and interfaces
        context.RegisterSourceOutput(source: compilationAndInterfaces, action: static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax> interfaces, SourceProductionContext context)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        IEnumerable<InterfaceDeclarationSyntax> distinctInterfaces = interfaces.Distinct();

        var p = new Parser(compilation, context.ReportDiagnostic, context.CancellationToken);
        IReadOnlyList<DecoratorClass> decoratorClasses = p.GetDecoratorClasses(distinctInterfaces);

        foreach (var decoratorClass in decoratorClasses)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            string source = SourceGenerationHelper.GenerateLoggingDecoratorClass(decoratorClass);
            context.AddSource(hintName: $"{decoratorClass.ClassName}.g.cs", sourceText: SourceText.From(text: source, encoding: Encoding.UTF8));
        }
    }
    // TODO : 1. Add example to README with Scrutor and DI, add test for this?
    // 3. Add diagnostics for unsupported things
    // 4. Make links more readable in README
}
