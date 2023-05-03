using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal static class SymbolExtensions
{
    private static readonly SymbolDisplayFormat s_symbolFormat = SymbolDisplayFormat.FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string ToFullyQualifiedDisplayString(this ISymbol symbol) => symbol.ToDisplayString(s_symbolFormat);
}
