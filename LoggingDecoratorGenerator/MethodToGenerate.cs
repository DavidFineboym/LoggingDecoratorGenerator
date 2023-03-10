﻿using Fineboym.Logging.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    public IMethodSymbol MethodSymbol { get; }

    public string LogLevel { get; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public MethodToGenerate(IMethodSymbol methodSymbol, string interfaceLogLevel, INamedTypeSymbol methodMarkerAttribute)
    {
        MethodSymbol = methodSymbol;
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = interfaceLogLevel;

        foreach (AttributeData attributeData in methodSymbol.GetAttributes())
        {
            if (!methodMarkerAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
            {
                continue;
            }

            foreach (KeyValuePair<string, TypedConstant> arg in attributeData.NamedArguments)
            {
                TypedConstant typedConstant = arg.Value;
                if (typedConstant.Kind == TypedConstantKind.Error)
                {
                    break;
                }

                if (arg.Key == nameof(MethodLogAttribute.Level) && typedConstant.Value is int value)
                {
                    LogLevel = $"global::Microsoft.Extensions.Logging.LogLevel.{(LogLevel)value}";
                }
            }

            break;
        }
    }

    private void CheckReturnType(ITypeSymbol methodReturnType)
    {
        string returnTypeOriginalDef = methodReturnType.OriginalDefinition.ToString();
        // TODO : Convert to typeof(T).FullName
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
