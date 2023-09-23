using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fineboym.Logging.Generator;

internal sealed class Parser
{
    private readonly CancellationToken _cancellationToken;
    private readonly Compilation _compilation;
    private readonly Action<Diagnostic> _reportDiagnostic;

    public bool StopwatchGetElapsedTimeAvailable { get; private set; }

    public Parser(Compilation compilation, Action<Diagnostic> reportDiagnostic, CancellationToken cancellationToken)
    {
        _compilation = compilation;
        _cancellationToken = cancellationToken;
        _reportDiagnostic = reportDiagnostic;
    }

    public IReadOnlyList<DecoratorClass> GetDecoratorClasses(IEnumerable<InterfaceDeclarationSyntax> interfaces)
    {
        // Get the semantic representation of our marker attribute
        INamedTypeSymbol? interfaceMarkerAttribute = _compilation.GetBestTypeByMetadataName(Attributes.DecorateWithLoggerFullName);
        if (interfaceMarkerAttribute == null)
        {
            // nothing to do if this type isn't available
            return Array.Empty<DecoratorClass>();
        }

        INamedTypeSymbol? methodMarkerAttribute = _compilation.GetBestTypeByMetadataName(Attributes.LogMethodFullName);
        if (methodMarkerAttribute == null)
        {
            // nothing to do if this type isn't available
            return Array.Empty<DecoratorClass>();
        }

        INamedTypeSymbol? notLoggedAttribute = _compilation.GetBestTypeByMetadataName(Attributes.NotLoggedFullName);
        if (notLoggedAttribute == null)
        {
            // nothing to do if this type isn't available
            return Array.Empty<DecoratorClass>();
        }

        INamedTypeSymbol? stopwatchType = _compilation.GetBestTypeByMetadataName("System.Diagnostics.Stopwatch");
        if (stopwatchType == null)
        {
            return Array.Empty<DecoratorClass>();
        }

        StopwatchGetElapsedTimeAvailable = stopwatchType.GetMembers("GetElapsedTime")
            .OfType<IMethodSymbol>()
            .Where(ms => ms.DeclaredAccessibility == Accessibility.Public && ms.IsStatic)
            .Any();

        var results = new List<DecoratorClass>();
        DecoratorClassParser parser = new(interfaceMarkerAttribute, methodMarkerAttribute, notLoggedAttribute, _reportDiagnostic, _cancellationToken);

        // we enumerate by syntax tree, to minimize the need to instantiate semantic models (since they're expensive)
        foreach (IGrouping<SyntaxTree, InterfaceDeclarationSyntax> group in interfaces.GroupBy(static x => x.SyntaxTree))
        {
            SyntaxTree syntaxTree = group.Key;
            SemanticModel sm = _compilation.GetSemanticModel(syntaxTree);

            foreach (InterfaceDeclarationSyntax interfaceDec in group)
            {
                // stop if we're asked to
                _cancellationToken.ThrowIfCancellationRequested();

                if (!parser.TryParseDecoratorClass(sm, interfaceDec, out DecoratorClass? decorator))
                {
                    continue;
                }

                results.Add(decorator!);
            }
        }

        return results;
    }
}
