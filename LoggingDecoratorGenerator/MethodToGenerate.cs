using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    private const string LogLevelEnumFullName = "global::Microsoft.Extensions.Logging.LogLevel";

    public static int DefaultEventId { get; } = -1;

    public IMethodSymbol MethodSymbol { get; }

    public IReadOnlyList<Parameter> Parameters { get; }

    public string? LogLevel { get; }

    public int EventId { get; }

    public string EventName { get; }

    public string UniqueName { get; set; }

    public bool Awaitable { get; private set; }

    public bool HasReturnValue { get; private set; }

    public bool ReturnValueLogged { get; }

    public ITypeSymbol? UnwrappedReturnType { get; private set; }

    public bool MeasureDuration { get; private set; }

    public string? ExceptionTypeToLog { get; private set; }

    public string ExceptionLogLevel { get; private set; }

    public MethodToGenerate(
        IMethodSymbol methodSymbol,
        string? interfaceLogLevel,
        INamedTypeSymbol methodMarkerAttribute,
        INamedTypeSymbol notLoggedAttribute)
    {
        MethodSymbol = methodSymbol;
        UniqueName = methodSymbol.Name; // assume no overloads at first
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = interfaceLogLevel;
        EventId = DefaultEventId;
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
                    throw new CompilerErrorException();
                }

                switch (arg.Key)
                {
                    case Attributes.LogMethodLevelName when typedConstant.Value is int logLevel:
                        LogLevel = $"{LogLevelEnumFullName}.{LogLevelConverter.FromInt(logLevel)}";
                        break;
                    case Attributes.LogMethodEventIdName when typedConstant.Value is int eventId:
                        EventId = eventId;
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
                        ExceptionLogLevel = $"{LogLevelEnumFullName}.{LogLevelConverter.FromInt(exceptionLogLevel)}";
                        break;
                }
            }

            break;
        }

        var parameters = new List<Parameter>(capacity: methodSymbol.Parameters.Length);
        foreach (IParameterSymbol parameterSymbol in methodSymbol.Parameters)
        {
            parameters.Add(new(parameterSymbol, notLoggedAttribute));
        }
        Parameters = parameters;

        ReturnValueLogged = !methodSymbol.GetReturnTypeAttributes()
            .Any(attributeData => notLoggedAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default));
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
