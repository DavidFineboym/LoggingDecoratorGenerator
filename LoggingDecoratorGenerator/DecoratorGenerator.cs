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
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource($"{Attributes.DecorateWithLoggerName}.g.cs", SourceText.From(Attributes.DecorateWithLogger, Encoding.UTF8));
            ctx.AddSource($"{Attributes.LogMethodName}.g.cs", SourceText.From(Attributes.LogMethod, Encoding.UTF8));
            ctx.AddSource($"{Attributes.NotLoggedName}.g.cs", SourceText.From(Attributes.NotLogged, Encoding.UTF8));
        });

        // Do a simple filter for interfaces
        IncrementalValuesProvider<InterfaceDeclarationSyntax> interfaceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select interfaces with attributes
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // select the interface with the [DecorateWithLogger] attribute
            .Where(predicate: static m => m is not null)!; // filter out attributed interfaces that we don't care about

        // Combine the selected interfaces with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<InterfaceDeclarationSyntax>)> compilationAndInterfaces
            = context.CompilationProvider.Combine(provider2: interfaceDeclarations.Collect());

        // Generate the source using the compilation and interfaces
        context.RegisterSourceOutput(source: compilationAndInterfaces, action: static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is InterfaceDeclarationSyntax m && m.AttributeLists.Count > 0;

    private static InterfaceDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a InterfaceDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        // loop through all the attributes on the interface
        foreach (AttributeListSyntax attributeListSyntax in interfaceDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == Attributes.DecorateWithLoggerFullName)
                {
                    return interfaceDeclarationSyntax;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<InterfaceDeclarationSyntax> interfaces, SourceProductionContext context)
    {
        if (interfaces.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        IEnumerable<InterfaceDeclarationSyntax> distinctInterfaces = interfaces.Distinct();

        // Convert each InterfaceDeclarationSyntax to an InterfaceToGenerate
        List<InterfaceToGenerate> interfacesToGenerate = GetTypesToGenerate(compilation, distinctInterfaces, context.CancellationToken);

        foreach (var interfaceToGenerate in interfacesToGenerate)
        {
            (string className, string source) = SourceGenerationHelper.GenerateLoggingDecoratorClass(interfaceToGenerate);
            context.AddSource(hintName: $"{className}.g.cs", sourceText: SourceText.From(text: source, encoding: Encoding.UTF8));
        }
    }

    private static List<InterfaceToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<InterfaceDeclarationSyntax> interfaces, CancellationToken ct)
    {
        // Create a list to hold our output
        var interfacesToGenerate = new List<InterfaceToGenerate>();
        // Get the semantic representation of our marker attribute
        INamedTypeSymbol? interfaceMarkerAttribute = compilation.GetTypeByMetadataName(Attributes.DecorateWithLoggerFullName);
        INamedTypeSymbol? methodMarkerAttribute = compilation.GetTypeByMetadataName(Attributes.LogMethodFullName);
        INamedTypeSymbol? notLoggedAttribute = compilation.GetTypeByMetadataName(Attributes.NotLoggedFullName);

        if (interfaceMarkerAttribute == null || methodMarkerAttribute == null || notLoggedAttribute == null)
        {
            // If this is null, the compilation couldn't find the marker attribute type
            // which suggests there's something very wrong! Bail out..
            return interfacesToGenerate;
        }

        foreach (InterfaceDeclarationSyntax interfaceDeclarationSyntax in interfaces)
        {
            // stop if we're asked to
            ct.ThrowIfCancellationRequested();

            // Get the semantic representation of the interface syntax
            SemanticModel semanticModel = compilation.GetSemanticModel(interfaceDeclarationSyntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
            {
                // something went wrong, bail out
                continue;
            }

            // Check if interfaceSymbol is a nested type
            if (interfaceSymbol.ContainingType != null)
            {
                // TODO : Emit error diagnostic because nested types are not supported
                continue;
            }

            string? interfaceLogLevel = ResolveInterfaceLogLevel(interfaceMarkerAttribute, interfaceSymbol);
            if (interfaceLogLevel == null)
            {
                continue;
            }

            // Create an InterfaceToGenerate for use in the generation phase
            interfacesToGenerate.Add(new InterfaceToGenerate(interfaceSymbol, interfaceDeclarationSyntax, interfaceLogLevel, methodMarkerAttribute, notLoggedAttribute));
        }

        return interfacesToGenerate;
    }

    private static string? ResolveInterfaceLogLevel(INamedTypeSymbol markerAttribute, INamedTypeSymbol interfaceSymbol)
    {
        foreach (AttributeData attributeData in interfaceSymbol.GetAttributes())
        {
            if (!markerAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
            {
                continue;
            }

            ImmutableArray<TypedConstant> args = attributeData.ConstructorArguments;

            // make sure we don't have any errors
            foreach (TypedConstant arg in args)
            {
                if (arg.Kind == TypedConstantKind.Error)
                {
                    // have an error, so don't try and do any generation
                    return null;
                }
            }

            if (args[0].Value is not int value)
            {
                return null;
            }

            return $"global::Microsoft.Extensions.Logging.LogLevel.{ConvertLogLevel(value)}";
        }

        return null;
    }

    public static string ConvertLogLevel(int value) => value switch
    {
        0 => "Trace",
        1 => "Debug",
        2 => "Information",
        3 => "Warning",
        4 => "Error",
        5 => "Critical",
        6 => "None",
        _ => value.ToString()
    };
}
