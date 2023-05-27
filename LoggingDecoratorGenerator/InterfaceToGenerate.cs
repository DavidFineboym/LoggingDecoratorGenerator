using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;

namespace Fineboym.Logging.Generator;

internal class InterfaceToGenerate
{
    public string Name { get; }

    public string DeclaredAccessibility { get; }

    public List<MethodToGenerate> Methods { get; }

    public string Namespace { get; }

    public string LogLevel { get; }

    public InterfaceToGenerate(
        INamedTypeSymbol interfaceSymbol,
        string interfaceNamespace,
        INamedTypeSymbol markerAttribute,
        INamedTypeSymbol methodMarkerAttribute,
        INamedTypeSymbol notLoggedAttribute)
    {
        LogLevel = ResolveInterfaceLogLevel(markerAttribute, interfaceSymbol) ?? throw new LogLevelResolutionException();
        Name = interfaceSymbol.Name;
        Namespace = interfaceNamespace;
        DeclaredAccessibility = SyntaxFacts.GetText(interfaceSymbol.DeclaredAccessibility);
        Methods = new List<MethodToGenerate>();

        AddMethods(interfaceSymbol.GetMembers(), LogLevel, methodMarkerAttribute, notLoggedAttribute);

        foreach (INamedTypeSymbol baseInterface in interfaceSymbol.AllInterfaces)
        {
            string? baseInterfaceLogLevel = ResolveInterfaceLogLevel(markerAttribute, baseInterface);
            AddMethods(baseInterface.GetMembers(), baseInterfaceLogLevel, methodMarkerAttribute, notLoggedAttribute);
        }

        // Once we've collected all methods for the given interface, check for overloads and provide unique names
        var methods = new Dictionary<string, int>(Methods.Count, StringComparer.Ordinal);
        foreach (MethodToGenerate m in Methods)
        {
            if (methods.TryGetValue(m.MethodSymbol.Name, out int currentCount))
            {
                m.UniqueName = $"{m.MethodSymbol.Name}{currentCount}";
                methods[m.MethodSymbol.Name] = currentCount + 1;
            }
            else
            {
                m.UniqueName = m.MethodSymbol.Name;
                methods[m.MethodSymbol.Name] = 1; //start from 1
            }
        }
    }

    private void AddMethods(ImmutableArray<ISymbol> interfaceMembers, string? logLevel, INamedTypeSymbol methodMarkerAttribute, INamedTypeSymbol notLoggedAttribute)
    {
        foreach (ISymbol member in interfaceMembers)
        {
            // TODO : Emit error diagnostic for interfaces with unsupported members
            if (member is IMethodSymbol method && !method.IsStatic && method.MethodKind == MethodKind.Ordinary)
            {
                Methods.Add(new MethodToGenerate(method, logLevel, methodMarkerAttribute, notLoggedAttribute));
            }
        }
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
                    throw new LogLevelResolutionException();
                }
            }

            if (args[0].Value is not int value)
            {
                throw new LogLevelResolutionException();
            }

            return $"global::Microsoft.Extensions.Logging.LogLevel.{LogLevelConverter.FromInt(value)}";
        }

        return null;
    }
}