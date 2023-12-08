using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Fineboym.Logging.Generator;

internal class DecoratorClassParser
{
    private readonly INamedTypeSymbol _interfaceMarkerAttribute;
    private readonly INamedTypeSymbol _methodMarkerAttribute;
    private readonly INamedTypeSymbol _notLoggedAttribute;
    private readonly Action<Diagnostic> _reportDiagnostic;
    private readonly CancellationToken _cancellationToken;

    public DecoratorClassParser(
        INamedTypeSymbol interfaceMarkerAttribute,
        INamedTypeSymbol methodMarkerAttribute,
        INamedTypeSymbol notLoggedAttribute,
        Action<Diagnostic> reportDiagnostic,
        CancellationToken cancellationToken)
    {
        _interfaceMarkerAttribute = interfaceMarkerAttribute;
        _methodMarkerAttribute = methodMarkerAttribute;
        _notLoggedAttribute = notLoggedAttribute;
        _reportDiagnostic = reportDiagnostic;
        _cancellationToken = cancellationToken;
    }

    public bool TryParseDecoratorClass(SemanticModel semanticModel, InterfaceDeclarationSyntax interfaceDeclaration, out DecoratorClass? decoratorClass)
    {
        decoratorClass = null;

        if (interfaceDeclaration.Arity > 0)
        {
            ReportDiagnostic(DiagnosticDescriptors.GenericInterfaceNotSupported, interfaceDeclaration.Identifier.GetLocation());
            return false;
        }

        string? nameSpace = GetNamespace(interfaceDeclaration);
        if (nameSpace == null)
        {
            ReportDiagnostic(DiagnosticDescriptors.InterfaceNoNamespace, interfaceDeclaration.Identifier.GetLocation());
            return false;
        }

        if (interfaceDeclaration.Parent is not BaseNamespaceDeclarationSyntax)
        {
            ReportDiagnostic(DiagnosticDescriptors.NestedInterfaceNotSupported, interfaceDeclaration.Identifier.GetLocation());
            return false;
        }

        if (semanticModel.GetDeclaredSymbol(interfaceDeclaration, _cancellationToken) is not INamedTypeSymbol interfaceSymbol)
        {
            // something went wrong, bail out
            return false;
        }

        if (interfaceSymbol.IsFileLocal)
        {
            ReportDiagnostic(DiagnosticDescriptors.FileLocalInterfaceNotSupported, interfaceDeclaration.Identifier.GetLocation());
            return false;
        }

        (string? interfaceLogLevel, bool durationAsMetric) = (null, false);
        List<MethodToGenerate> methods;
        try
        {
            (interfaceLogLevel, durationAsMetric) = ResolveInterfaceAttribute(interfaceSymbol);
            if (interfaceLogLevel == null)
            {
                return false;
            }

            methods = new List<MethodToGenerate>();

            if (!TryParseMembers(interfaceSymbol, interfaceLogLevel, methods))
            {
                return false;
            }

            foreach (INamedTypeSymbol baseInterface in interfaceSymbol.AllInterfaces)
            {
                (string? baseInterfaceLogLevel, _) = ResolveInterfaceAttribute(baseInterface);
                if (!TryParseMembers(baseInterface, baseInterfaceLogLevel, methods))
                {
                    return false;
                }
            }
        }
        catch (CompilerErrorException)
        {
            return false;
        }

        // Once we've collected all methods for the given interface, check for overloads and provide unique names
        var methodsMap = new Dictionary<string, int>(methods.Count, StringComparer.Ordinal);
        foreach (MethodToGenerate m in methods)
        {
            if (methodsMap.TryGetValue(m.MethodSymbol.Name, out int currentCount))
            {
                m.UniqueName = $"{m.MethodSymbol.Name}{currentCount}";
                methodsMap[m.MethodSymbol.Name] = currentCount + 1;
            }
            else
            {
                m.UniqueName = m.MethodSymbol.Name;
                methodsMap[m.MethodSymbol.Name] = 1; //start from 1
            }
        }

        decoratorClass = new(
            @namespace: nameSpace,
            interfaceName: interfaceSymbol.Name,
            declaredAccessibility: SyntaxFacts.GetText(interfaceSymbol.DeclaredAccessibility),
            logLevel: interfaceLogLevel,
            durationAsMetric,
            methods: methods);

        return true;
    }

    private bool TryParseMembers(INamedTypeSymbol interfaceSymbol, string? logLevel, List<MethodToGenerate> methods)
    {
        foreach (ISymbol member in interfaceSymbol.GetMembers())
        {
            if (member is INamedTypeSymbol)
            {
                // Not interested in nested types as they won't affect the generated code
                continue;
            }

            if (member.IsStatic)
            {
                // Not interested in static members as they can only be referred from the interface instance type
                continue;
            }

            if (member.Name[0] == '_')
            {
                // can't have member names that start with _ since that can lead to conflicting symbol names
                // because the generated symbols start with _
                ReportDiagnostic(DiagnosticDescriptors.InvalidMemberName, member.Locations[0]);
                return false;
            }

            if (member is IMethodSymbol methodSymbol && methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                if (methodSymbol.Arity > 0)
                {
                    ReportDiagnostic(DiagnosticDescriptors.GenericMethodsNotSupported, methodSymbol.Locations[0]);
                    return false;
                }

                if (methodSymbol.ReturnsByRef || methodSymbol.ReturnsByRefReadonly)
                {
                    ReportDiagnostic(DiagnosticDescriptors.RefReturnNotSupported, methodSymbol.Locations[0]);
                    return false;
                }

                MethodToGenerate decMethod = new(methodSymbol, logLevel, _methodMarkerAttribute, _notLoggedAttribute);
                methods.Add(decMethod);
            }
            else
            {
                ReportDiagnostic(DiagnosticDescriptors.OnlyOrdinaryMethods, member.Locations[0]);
                return false;
            }
        }

        return true;
    }

    // determine the namespace the class/enum/struct is declared in, if any
    private static string? GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string? nameSpace = null;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    private (string? logLevel, bool durationAsMetric) ResolveInterfaceAttribute(INamedTypeSymbol interfaceSymbol)
    {
        foreach (AttributeData attributeData in interfaceSymbol.GetAttributes())
        {
            if (!_interfaceMarkerAttribute.Equals(attributeData.AttributeClass, SymbolEqualityComparer.Default))
            {
                continue;
            }

            ImmutableArray<TypedConstant> args = attributeData.ConstructorArguments;

            // make sure we don't have any errors
            foreach (TypedConstant arg in args)
            {
                if (arg.Kind == TypedConstantKind.Error)
                {
                    // have an error, so don't try and do any generation
                    throw new CompilerErrorException();
                }
            }

            if (args[0].Value is not int logLevelValue)
            {
                throw new CompilerErrorException();
            }

            bool reportDurationAsMetric = false;
            foreach (KeyValuePair<string, TypedConstant> arg in attributeData.NamedArguments)
            {
                TypedConstant typedConstant = arg.Value;
                if (typedConstant.Kind == TypedConstantKind.Error)
                {
                    throw new CompilerErrorException();
                }

                if (arg.Key == Attributes.ReportDurationAsMetricName && typedConstant.Value is bool value)
                {
                    reportDurationAsMetric = value;
                    break;
                }
            }

            return ($"global::Microsoft.Extensions.Logging.LogLevel.{LogLevelConverter.FromInt(logLevelValue)}", reportDurationAsMetric);
        }

        return (null, false);
    }

    private void ReportDiagnostic(DiagnosticDescriptor desc, Location? location, params object?[]? messageArgs)
    {
        _reportDiagnostic(Diagnostic.Create(desc, location, messageArgs));
    }
}

internal class DecoratorClass
{
    public string Namespace { get; }

    public string InterfaceName { get; }

    public string DeclaredAccessibility { get; }

    public string LogLevel { get; }

    public bool DurationAsMetric { get; }

    public IReadOnlyList<MethodToGenerate> Methods { get; }

    public string ClassName { get; }

    public bool SomeMethodMeasuresDuration { get; }

    public bool NeedsDurationAsMetric => SomeMethodMeasuresDuration && DurationAsMetric;

    public DecoratorClass(string @namespace, string interfaceName, string declaredAccessibility, string logLevel, bool durationAsMetric, IReadOnlyList<MethodToGenerate> methods)
    {
        Namespace = @namespace;
        InterfaceName = interfaceName;
        DeclaredAccessibility = declaredAccessibility;
        LogLevel = logLevel;
        DurationAsMetric = durationAsMetric;
        Methods = methods;
        ClassName = $"{(interfaceName[0] == 'I' ? interfaceName.Substring(1) : interfaceName)}LoggingDecorator";
        SomeMethodMeasuresDuration = methods.Any(static m => m.MeasureDuration);
    }
}