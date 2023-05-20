using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class Parameter
{
    public IParameterSymbol Symbol { get; }

    public bool IsLogged { get; }

    public Parameter(IParameterSymbol symbol, INamedTypeSymbol notLoggedAttribute)
    {
        Symbol = symbol;
        IsLogged = true;

        foreach (AttributeData attributeData in symbol.GetAttributes())
        {
            if (notLoggedAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
            {
                IsLogged = false;

                break;
            }
        }
    }
}
