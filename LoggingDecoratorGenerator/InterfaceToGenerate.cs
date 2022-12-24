﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Fineboym.Logging.Generator;

internal class InterfaceToGenerate
{
    public INamedTypeSymbol Interface { get; }

    public List<MethodToGenerate> Methods { get; }

    public InterfaceDeclarationSyntax InterfaceDeclarationSyntax { get; }

    public string Namespace { get; }

    public InterfaceToGenerate(INamedTypeSymbol interfaceSymbol, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        Interface = interfaceSymbol;
        Methods = new List<MethodToGenerate>();
        InterfaceDeclarationSyntax = interfaceDeclarationSyntax;
        Namespace = GetNamespace(interfaceDeclarationSyntax);

        // Get all the members in the interface
        ImmutableArray<ISymbol> interfaceMembers = interfaceSymbol.GetMembers();

        foreach (ISymbol member in interfaceMembers)
        {
            if (member is IMethodSymbol method && !method.IsStatic && method.MethodKind == MethodKind.Ordinary)
            {
                Methods.Add(new MethodToGenerate(method));
            }
        }
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