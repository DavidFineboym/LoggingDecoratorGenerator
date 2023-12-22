using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal class MethodToGenerate
{
    private const string LogLevelEnumFullName = "global::Microsoft.Extensions.Logging.LogLevel";

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
    // TODO: Refactor to MethodParser and add diagnostics for unsupported parameters- ref, out...
    public MethodToGenerate(
        IMethodSymbol methodSymbol,
        string? interfaceLogLevel,
        bool interfaceMeasureDuration,
        INamedTypeSymbol methodMarkerAttribute,
        INamedTypeSymbol notLoggedAttribute)
    {
        MethodSymbol = methodSymbol;
        UniqueName = methodSymbol.Name; // assume no overloads at first
        CheckReturnType(methodSymbol.ReturnType);
        LogLevel = interfaceLogLevel;
        MeasureDuration = interfaceMeasureDuration;
        bool suppliedEventId = false;
        string? suppliedEventName = null;

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
                        suppliedEventId = true;
                        break;
                    case Attributes.LogMethodEventNameName when typedConstant.Value is string eventName:
                        suppliedEventName = eventName;
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

        if (!suppliedEventId)
        {
            EventId = GetNonRandomizedHashCode(string.IsNullOrWhiteSpace(suppliedEventName) ? methodSymbol.Name : suppliedEventName!);
        }

        EventName = string.IsNullOrWhiteSpace(suppliedEventName) ? $"nameof({methodSymbol.Name})" : $"\"{suppliedEventName}\"";

        if (ExceptionLogLevel == default)
        {
            ExceptionLogLevel = $"{LogLevelEnumFullName}.Error"; // default log level
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

    /// <summary>
    /// Returns a non-randomized hash code for the given string.
    /// We always return a positive value.
    /// </summary>
    private static int GetNonRandomizedHashCode(string s)
    {
        uint result = 2166136261u;
        foreach (char c in s)
        {
            result = (c ^ result) * 16777619;
        }
        return Math.Abs((int)result);
    }
}
