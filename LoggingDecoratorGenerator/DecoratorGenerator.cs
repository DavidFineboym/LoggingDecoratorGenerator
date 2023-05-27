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
            if (semanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax, ct) is not INamedTypeSymbol interfaceSymbol)
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

            try
            {
                InterfaceToGenerate @interface = new(interfaceSymbol, GetNamespace(interfaceDeclarationSyntax), interfaceMarkerAttribute, methodMarkerAttribute, notLoggedAttribute);
                // Create an InterfaceToGenerate for use in the generation phase
                interfacesToGenerate.Add(@interface);
            }
            catch (LogLevelResolutionException)
            {
                continue;
            }
        }

        return interfacesToGenerate;
    }

    // determine the namespace the class/enum/struct is declared in, if any
    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }
}
