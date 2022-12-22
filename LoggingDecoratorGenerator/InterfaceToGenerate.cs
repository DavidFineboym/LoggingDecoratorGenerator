using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LoggingDecoratorGenerator;

public class InterfaceToGenerate
{
    public INamedTypeSymbol Interface { get; }

    public List<IMethodSymbol> Methods { get; }

    public InterfaceDeclarationSyntax InterfaceDeclarationSyntax { get; }

    public InterfaceToGenerate(INamedTypeSymbol @interface, List<IMethodSymbol> methods, InterfaceDeclarationSyntax interfaceDeclarationSyntax)
    {
        Interface = @interface;
        Methods = methods;
        InterfaceDeclarationSyntax = interfaceDeclarationSyntax;
    }
}
