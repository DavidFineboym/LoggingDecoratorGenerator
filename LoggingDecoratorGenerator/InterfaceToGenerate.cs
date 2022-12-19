using Microsoft.CodeAnalysis;

namespace LoggingDecoratorGenerator;

public class InterfaceToGenerate
{
    public INamedTypeSymbol Interface { get; }

    public List<IMethodSymbol> Methods { get; }

    public InterfaceToGenerate(INamedTypeSymbol @interface, List<IMethodSymbol> methods)
    {
        Interface = @interface;
        Methods = methods;
    }
}
