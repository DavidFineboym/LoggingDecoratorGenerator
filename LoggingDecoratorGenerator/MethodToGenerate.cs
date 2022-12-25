using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    public IMethodSymbol MethodSymbol { get; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public MethodToGenerate(IMethodSymbol methodSymbol)
    {
        MethodSymbol = methodSymbol;
        CheckReturnType(methodSymbol.ReturnType);
    }

    private void CheckReturnType(ITypeSymbol methodReturnType)
    {
        IMethodSymbol? getAwaiterMethodCandidate = methodReturnType.GetMembers(name: "GetAwaiter")
            .OfType<IMethodSymbol>()
            .SingleOrDefault(static method => method.DeclaredAccessibility == Accessibility.Public
                                              && !method.IsAbstract
                                              && !method.IsStatic
                                              && method.Parameters.IsEmpty
                                              && method.TypeParameters.IsEmpty);

        if (getAwaiterMethodCandidate == null)
        {
            Awaitable = false;
            HasReturnValue = methodReturnType.SpecialType != SpecialType.System_Void;

            return;
        }

        string returnTypeFullName = getAwaiterMethodCandidate.ReturnType.OriginalDefinition.ToString();

        if (returnTypeFullName is "System.Runtime.CompilerServices.TaskAwaiter" or "System.Runtime.CompilerServices.ValueTaskAwaiter")
        {
            Awaitable = true;
            HasReturnValue = false;

            return;
        }

        if (returnTypeFullName is "System.Runtime.CompilerServices.TaskAwaiter<TResult>" or "System.Runtime.CompilerServices.ValueTaskAwaiter<TResult>")
        {
            Awaitable = true;
            HasReturnValue = true;
            UnwrappedReturnType = ((INamedTypeSymbol)getAwaiterMethodCandidate.ReturnType).TypeArguments[0];

            return;
        }

        Awaitable = false;
        HasReturnValue = methodReturnType.SpecialType != SpecialType.System_Void;
    }
}
