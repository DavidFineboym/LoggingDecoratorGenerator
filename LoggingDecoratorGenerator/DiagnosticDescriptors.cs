using Microsoft.CodeAnalysis;

namespace Fineboym.Logging.Generator;

internal static class DiagnosticDescriptors
{
    public static DiagnosticDescriptor GenericInterfaceNotSupported { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0001",
        title: "Generic Interface",
        messageFormat: "Generic interfaces are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InterfaceNoNamespace { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0002",
        title: "Interface Without Namespace",
        messageFormat: "Interfaces without namespace are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NestedInterfaceNotSupported { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0003",
        title: "Nested Interface",
        messageFormat: "Nested interfaces are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor FileLocalInterfaceNotSupported { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0004",
        title: "File Local",
        messageFormat: "File local interfaces are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidMemberName { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0005",
        title: "Invalid Name",
        messageFormat: "Member names that start with _ are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor GenericMethodsNotSupported { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0006",
        title: "Generic Method",
        messageFormat: "Generic methods are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RefReturnNotSupported { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0007",
        title: "Method Returns by ref",
        messageFormat: "Ref returning methods are not supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor OnlyOrdinaryMethods { get; } = new DiagnosticDescriptor(
        id: "LOGDEC0008",
        title: "Only Ordinary Methods",
        messageFormat: "Only ordinary methods are supported by logging decorator.",
        category: "LoggingGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
