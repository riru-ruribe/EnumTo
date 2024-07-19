using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace EnumToGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class EnumToGenerator : IIncrementalGenerator
{
    const string Ns = "EnumTo";
    public const string AtrEnumTo = "EnumToExtendableAttribute";
    const string AtrEnumToDisp = $"{Ns}.{AtrEnumTo}";
    const string AtrEnumToName = "EnumToNameAttribute";
    const string AtrEnumToNameDisp = $"{Ns}.{AtrEnumToName}";
    public const string AtrEnumToValue = "EnumToValueAttribute";
    const string AtrEnumToValueDisp = $"{Ns}.{AtrEnumToValue}";
    const string DefaultPrimitive = "int";
    static readonly HashSet<string> Primitives = new()
    {
        "sbyte",
        "byte",
        "short",
        "ushort",
        "int",
        "uint",
        "long",
        "ulong",
        "float",
        "double",
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx => ctx.AddSource($"{Ns}.g.cs", PostAtr()));

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            AtrEnumToDisp,
            static (node, token) => node is EnumDeclarationSyntax,
            static (context, token) => context
        );
        context.RegisterSourceOutput(source, Emit);
    }

    static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        var typeNode = (EnumDeclarationSyntax)source.TargetNode;
        var e = typeSymbol.Name;

        var accessibility = typeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            Accessibility.Public => "public",
            _ => null,
        };
        if (string.IsNullOrEmpty(accessibility))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0001, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var prmTypeName = DefaultPrimitive;
        var valTypeName = DefaultPrimitive;
        var isValIF = false;
        foreach (var atr in typeSymbol.GetAttributes())
        {
            if (atr?.AttributeClass?.ToDisplayString() == AtrEnumToDisp &&
                atr.ConstructorArguments.Length > 0)
            {
                switch (atr.ConstructorArguments.Length)
                {
                    case 1:
                        if (atr.ConstructorArguments[0].Value?.ToString() is { } name)
                        {
                            prmTypeName = name;
                            valTypeName = name;
                        }
                        break;
                    case 2:
                        if (atr.ConstructorArguments[0].Value?.ToString() is { } name1)
                        {
                            prmTypeName = name1;
                        }
                        if (atr.ConstructorArguments[1].Value?.ToString() is { } name2)
                        {
                            valTypeName = name2;
                            // FIXME: find out if there is a way to get it.
                            // if interface, can not new 'valTypeName' directly.
                            var className = name2.Contains('.') ? name2.Split('.').Last() : name2;
                            isValIF = className[0] == 'I' && char.IsUpper(className[1]);
                        }
                        break;
                }
                break;
            }
        }

        var a_SwitchSb = new StringBuilder();
        var b_InstSb = new StringBuilder();
        var b_SwitchSb = new StringBuilder();
        var b_EnumerableSb = new StringBuilder();
        var c_InstSb = new StringBuilder();
        var c_SwitchSb = new StringBuilder();
        var c_EnumerableSb = new StringBuilder();
        int memberCnt = 0;
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                var value = field.ConstantValue;
                if (value == null) continue;
                var prms = new List<string>();
                var strValue = member.Name;
                foreach (var atr in member.GetAttributes())
                {
                    if (atr?.AttributeClass?.ToDisplayString() == AtrEnumToNameDisp &&
                        atr.ConstructorArguments.Length == 1 &&
                        atr.ConstructorArguments[0].Value?.ToString() is { } argName1)
                    {
                        strValue = argName1;
                    }
                    if (atr?.AttributeClass?.ToDisplayString() == AtrEnumToValueDisp &&
                        atr.ConstructorArguments.Length == 1
                        /* params object[] values */)
                    {
                        foreach (var tc in atr.ConstructorArguments[0].Values)
                        {
                            if (tc.Type?.ToString() is not { } typeName ||
                                tc.Value?.ToString() is not { } valueName)
                            {
                                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0003, typeNode.Identifier.GetLocation(), typeSymbol.Name));
                                return;
                            }
                            if (typeName == "string")
                                prms.Add($"\"{valueName}\"");
                            else
                                prms.Add(valueName + PrimitiveSuffix(typeName));
                        }
                    }
                }
                if (a_SwitchSb.Length > 0) a_SwitchSb.AppendLine();
                if (b_InstSb.Length > 0) b_InstSb.AppendLine();
                if (b_SwitchSb.Length > 0) b_SwitchSb.AppendLine();
                if (b_EnumerableSb.Length > 0) b_EnumerableSb.AppendLine();
                if (c_InstSb.Length > 0) c_InstSb.AppendLine();
                if (c_SwitchSb.Length > 0) c_SwitchSb.AppendLine();
                if (c_EnumerableSb.Length > 0) c_EnumerableSb.AppendLine();
                var prmLen = prms.Count;
                if (prmLen > 1) // not primitives
                {
                    int x = 0;
                    var s = valTypeName;
                    if (isValIF) // the beginning of 'params object[] values' is typeof(interface)
                    {
                        x = 1;
                        s = prms[0];
                    }
                    b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                    b_InstSb.Append($$"""    static readonly {{valTypeName}} _b{{memberCnt}} = new {{s}}(""");
                    for (int i = x; i < prmLen; i++)
                    {
                        if (i > x) b_InstSb.Append(", ");
                        b_InstSb.Append(prms[i]);
                    }
                    b_InstSb.Append($$""");""");
                }
                else if (prmLen > 0) // primitive or custom class
                {
                    if (Primitives.Contains(valTypeName))
                    {
                        b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                        b_InstSb.Append($$"""    const {{valTypeName}} _b{{memberCnt}} = {{prms[0]}};""");
                    }
                    else
                    {
                        if (isValIF)
                        {
                            b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                            b_InstSb.Append($$"""    static readonly {{valTypeName}} _b{{memberCnt}} = new {{prms[0]}}();""");
                        }
                        else
                        {
                            b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                            b_InstSb.Append($$"""    static readonly {{valTypeName}} _b{{memberCnt}} = new {{valTypeName}}({{prms[0]}});""");
                        }
                    }
                }
                else // primitive or custom class
                {
                    if (Primitives.Contains(valTypeName))
                    {
                        b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                        b_InstSb.Append($$"""    const {{valTypeName}} _b{{memberCnt}} = {{value}};""");
                    }
                    else
                    {
                        if (isValIF)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0004, typeNode.Identifier.GetLocation(), typeSymbol.Name));
                            return;
                        }
                        else
                        {
                            b_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _b{{memberCnt}},""");
                            b_InstSb.Append($$"""    static readonly {{valTypeName}} _b{{memberCnt}} = new {{valTypeName}}();""");
                        }
                    }
                }
                a_SwitchSb.Append($$"""            {{value}} => {{e}}.{{member.Name}},""");
                b_EnumerableSb.Append($$"""            {{memberCnt}} => _b{{memberCnt}},""");
                c_InstSb.Append($$"""    const string _c{{memberCnt}} = "{{strValue}}";""");
                c_SwitchSb.Append($$"""            {{e}}.{{member.Name}} => _c{{memberCnt}},""");
                c_EnumerableSb.Append($$"""            {{memberCnt}} => _c{{memberCnt}},""");
                memberCnt++;
            }
        }

        if (b_SwitchSb.Length <= 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.E0005, typeNode.Identifier.GetLocation(), typeSymbol.Name));
            return;
        }

        var Is_PrmFunc = "";
        var IsSafely_PrmFunc = "";
        if (prmTypeName != valTypeName)
        {
            Is_PrmFunc = $$"""
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(this {{e}} e, {{prmTypeName}} k) => e == a1(k);
""";
            IsSafely_PrmFunc = $$"""
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSafely(this {{e}} e, {{prmTypeName}} k) => e == a2(k);
""";
        }

        var b_Default = "";
        if (Primitives.Contains(valTypeName))
        {
            switch (valTypeName)
            {
                case "sbyte": b_Default = "sbyte.MinValue"; break;
                case "byte": b_Default = "byte.MaxValue"; break;
                case "short": b_Default = "short.MinValue"; break;
                case "ushort": b_Default = "ushort.MaxValue"; break;
                case "int": b_Default = "int.MinValue"; break;
                case "uint": b_Default = "uint.MaxValue"; break;
                case "long": b_Default = "long.MinValue"; break;
                case "ulong": b_Default = "ulong.MaxValue"; break;
                case "float": b_Default = "float.MinValue"; break;
                case "double": b_Default = "double.MinValue"; break;
            }
        }
        else if (isValIF)
        {
            b_Default = "null";
        }
        else if (valTypeName == "UnityEngine.Color32") // not implemented 'IEquatable'
        {
            b_Default = "(UnityEngine.Color)default";
        }
        else
        {
            b_Default = "default";
        }

        var isNamespace = !typeSymbol.ContainingNamespace.IsGlobalNamespace;

        context.AddSource($"{e}To.g.cs", $$"""
// <auto-generated/>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
{{(isNamespace ? $$"""namespace {{typeSymbol.ContainingNamespace}} {""" : "")}}
{{accessibility}} static class {{e}}To
{
    const {{e}} eMinVal = ({{e}})short.MinValue;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static {{e}} a1({{prmTypeName}} k)
    {
        return k switch
        {
{{a_SwitchSb}}
            _ => throw new IndexOutOfRangeException($"[{{e}}To] {{prmTypeName}} {k}"),
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static {{e}} a2({{prmTypeName}} k)
    {
        return k switch
        {
{{a_SwitchSb}}
            _ => eMinVal,
        };
    }
{{b_InstSb}}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static {{valTypeName}} b1({{e}} e)
    {
        return e switch
        {
{{b_SwitchSb}}
            _ => throw new IndexOutOfRangeException($"[{{e}}To] {e}"),
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static {{valTypeName}} b2({{e}} e)
    {
        return e switch
        {
{{b_SwitchSb}}
            _ => {{b_Default}},
        };
    }
    public struct ValueEnumerator : IEnumerator<{{valTypeName}}>, IEnumerable<{{valTypeName}}>
    {
        int i;
        public bool MoveNext() => ++i < Length;
        public void Reset() { i = -1; }
        public {{valTypeName}} Current => i switch
        {
{{b_EnumerableSb}}
            _ => throw new IndexOutOfRangeException(),
        };
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public ValueEnumerator GetEnumerator() => this;
        IEnumerator<{{valTypeName}}> IEnumerable<{{valTypeName}}>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        public ValueEnumerator(int i) { this.i = i; }
    }
{{c_InstSb}}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string c1({{e}} e)
    {
        return e switch
        {
{{c_SwitchSb}}
            _ => throw new IndexOutOfRangeException($"[{{e}}To] {e}"),
        };
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string c2({{e}} e)
    {
        return e switch
        {
{{c_SwitchSb}}
            _ => string.Empty,
        };
    }
    public struct NameEnumerator : IEnumerator<string>, IEnumerable<string>
    {
        int i;
        public bool MoveNext() => ++i < Length;
        public void Reset() { i = -1; }
        public string Current => i switch
        {
{{c_EnumerableSb}}
            _ => throw new IndexOutOfRangeException(),
        };
        object IEnumerator.Current => Current;
        public void Dispose() { }
        public NameEnumerator GetEnumerator() => this;
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        public NameEnumerator(int i) { this.i = i; }
    }
    public const int Length = {{memberCnt}};
{{Is_PrmFunc}}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Is(this {{e}} e, {{valTypeName}} v) => v.Equals(b1(e));
{{IsSafely_PrmFunc}}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSafely(this {{e}} e, {{valTypeName}} v) => v.Equals(b2(e));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static {{valTypeName}} GetValue({{prmTypeName}} k) => b1(a1(k));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static {{valTypeName}} GetValue(this {{e}} e) => b1(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static {{valTypeName}} GetValueSafely({{prmTypeName}} k) => b2(a2(k));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static {{valTypeName}} GetValueSafely(this {{e}} e) => b2(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueEnumerator GetValues() => new(-1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName({{prmTypeName}} k) => c1(a1(k));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetName(this {{e}} e) => c1(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetNameSafely({{prmTypeName}} k) => c2(a2(k));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetNameSafely(this {{e}} e) => c2(e);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NameEnumerator GetNames() => new(-1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Defined({{prmTypeName}} k) => a2(k) != eMinVal;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Defined(this {{e}} e) => b2(e) != {{b_Default}};
}
{{(isNamespace ? "}" : "")}}
""");
    }

    static string PrimitiveSuffix(string primitive)
    {
        return primitive switch
        {
            "long" => "L",
            "ulong" => "L",
            "float" => "f",
            _ => "",
        };
    }

    static string PostAtr()
    {
        return $$"""
using System;
namespace {{Ns}}
{
    /// <summary>
    /// extension class will be generated.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum, AllowMultiple = false)]
    internal sealed class {{AtrEnumTo}} : Attribute
    {
        public {{AtrEnumTo}}() { /* default primitive is {{DefaultPrimitive}} */ }
        public {{AtrEnumTo}}(Type primitiveType) { }
        public {{AtrEnumTo}}(Type primitiveType, Type valueType) { }
    }

    /// <summary>
    /// can replace member name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrEnumToName}} : Attribute
    {
        public {{AtrEnumToName}}() { }
        public {{AtrEnumToName}}(string dispName) { }
    }

    /// <summary>
    /// can set 'constructor arguments' or 'primitive initial value'.<br/>
    /// but constructor arguments only primitive.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class {{AtrEnumToValue}} : Attribute
    {
        public {{AtrEnumToValue}}() { }
        public {{AtrEnumToValue}}(params object[] values) { }
    }
}
""";
    }
}
