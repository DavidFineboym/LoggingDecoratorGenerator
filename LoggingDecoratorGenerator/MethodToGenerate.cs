﻿using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    public IMethodSymbol MethodSymbol { get; }

    public string LogLevel { get; }

    public string EventId { get; }

    public string EventName { get; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public bool MeasureDuration { get; private set; }

    public MethodToGenerate(IMethodSymbol methodSymbol, string interfaceLogLevel, INamedTypeSymbol methodMarkerAttribute)
    {
        MethodSymbol = methodSymbol;
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = interfaceLogLevel;
        EventId = "-1";
        EventName = $"nameof({methodSymbol.Name})";

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

                switch (arg.Key)
                {
                    case "Level" when typedConstant.Value is int logLevel:
                        LogLevel = $"global::Microsoft.Extensions.Logging.LogLevel.{DecoratorGenerator.ConvertLogLevel(logLevel)}";
                        break;
                    case "EventId" when typedConstant.Value is int eventId:
                        EventId = eventId.ToString();
                        break;
                    case "EventName" when typedConstant.Value is string eventName:
                        EventName = $"\"{eventName}\"";
                        break;
                    case "MeasureDuration" when typedConstant.Value is bool measureDuration:
                        MeasureDuration = measureDuration;
                        break;
                }
            }

            break;
        }
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
