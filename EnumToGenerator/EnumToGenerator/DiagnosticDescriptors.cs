using Microsoft.CodeAnalysis;

namespace EnumToGenerator;

public static class DiagnosticDescriptors
{
    const string Category = "EnumTo";

    public static readonly DiagnosticDescriptor E0001 = new(
        id: Category + nameof(E0001),
        title: "invalid accessibility",
        messageFormat: "'public' or 'protected' or 'internal' or 'private' is allowed.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0002 = new(
        id: Category + nameof(E0002),
        title: "invalid syntax",
        messageFormat: "'partial' class required.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0003 = new(
        id: Category + nameof(E0003),
        title: "invalid primitive",
        messageFormat: "primitive not allowed.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0004 = new(
        id: Category + nameof(E0004),
        title: "invalid value",
        messageFormat: $@"specifying interface on valueType of '{EnumToGenerator.AtrEnumTo}'.
therefore, please specify type of class that implemented interface with '{EnumToGenerator.AtrEnumToValue}'.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor E0005 = new(
        id: Category + nameof(E0005),
        title: "empty enum",
        messageFormat: "extension class will not be generated because enum '{0}' has no members.",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
