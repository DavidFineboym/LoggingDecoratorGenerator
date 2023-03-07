using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    public IMethodSymbol MethodSymbol { get; }

    public string LogLevel { get; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public MethodToGenerate(IMethodSymbol methodSymbol, string logLevel)
    {
        MethodSymbol = methodSymbol;
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = logLevel;
    }

    private void CheckReturnType(ITypeSymbol methodReturnType)
    {
        string returnTypeOriginalDef = methodReturnType.OriginalDefinition.ToString();

        if (returnTypeOriginalDef is "System.Threading.Tasks.Task<TResult>" or "System.Threading.Tasks.ValueTask<TResult>")
        {
            Awaitable = true;
            HasReturnValue = true;
            UnwrappedReturnType = ((INamedTypeSymbol)methodReturnType).TypeArguments[0];

            return;
        }

        if (returnTypeOriginalDef is "System.Threading.Tasks.Task" or "System.Threading.Tasks.ValueTask")
        {
            Awaitable = true;
            HasReturnValue = false;

            return;
        }

        Awaitable = false;
        HasReturnValue = methodReturnType.SpecialType != SpecialType.System_Void;
    }
}
