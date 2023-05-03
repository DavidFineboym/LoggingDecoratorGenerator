using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    private const string LogLevelEnumFullName = "global::Microsoft.Extensions.Logging.LogLevel";

    public IMethodSymbol MethodSymbol { get; }

    public string LogLevel { get; }

    public string EventId { get; }

    public string EventName { get; }

    public string UniqueName { get; set; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public bool MeasureDuration { get; private set; }

    public string? ExceptionTypeToLog { get; private set; }

    public string ExceptionLogLevel { get; private set; }

    public MethodToGenerate(IMethodSymbol methodSymbol, string interfaceLogLevel, INamedTypeSymbol methodMarkerAttribute)
    {
        MethodSymbol = methodSymbol;
        UniqueName = methodSymbol.Name; // assume no overloads at first
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = interfaceLogLevel;
        EventId = "-1";
        EventName = $"nameof({methodSymbol.Name})";
        ExceptionLogLevel = $"{LogLevelEnumFullName}.Error"; // default log level

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
                    case Attributes.LogMethodLevelName when typedConstant.Value is int logLevel:
                        LogLevel = $"{LogLevelEnumFullName}.{DecoratorGenerator.ConvertLogLevel(logLevel)}";
                        break;
                    case Attributes.LogMethodEventIdName when typedConstant.Value is int eventId:
                        EventId = eventId.ToString();
                        break;
                    case Attributes.LogMethodEventNameName when typedConstant.Value is string eventName:
                        EventName = $"\"{eventName}\"";
                        break;
                    case Attributes.LogMethodMeasureDurationName when typedConstant.Value is bool measureDuration:
                        MeasureDuration = measureDuration;
                        break;
                    case Attributes.LogMethodExceptionToLogName when typedConstant.Value is INamedTypeSymbol exceptionToLog:
                        ExceptionTypeToLog = exceptionToLog.ToFullyQualifiedDisplayString();
                        break;
                    case Attributes.LogMethodExceptionLogLevelName when typedConstant.Value is int exceptionLogLevel:
                        ExceptionLogLevel = $"{LogLevelEnumFullName}.{DecoratorGenerator.ConvertLogLevel(exceptionLogLevel)}";
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
