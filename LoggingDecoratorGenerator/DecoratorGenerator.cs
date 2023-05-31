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

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        IEnumerable<InterfaceDeclarationSyntax> distinctInterfaces = interfaces.Distinct();

        // Convert each InterfaceDeclarationSyntax to an InterfaceToGenerate
        List<InterfaceToGenerate> interfacesToGenerate = GetTypesToGenerate(compilation, distinctInterfaces, context.CancellationToken);

        foreach (var interfaceToGenerate in interfacesToGenerate)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
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
