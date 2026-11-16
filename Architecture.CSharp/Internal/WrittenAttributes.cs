using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// Whether an attribute of a given name is written among some. Each written attribute is named by the type
/// the compiler resolves it to, or as written when it does not. A plain name matches that type's name, a
/// qualified one its full name; either side counts without the Attribute suffix and global::.
/// </summary>
internal static class WrittenAttributes
{
    private const string Suffix = "Attribute";
    private const string Global = "global::";

    internal static bool Has(IEnumerable<SourceNode<AttributeSyntax>> attributes, string name)
        => Normalized(name) is var asked && attributes.Any(attribute => Matches(Normalized(NameOf(attribute)), asked));

    private static bool Matches(IReadOnlyList<string> written, IReadOnlyList<string> asked)
        => asked.Count == 1 ? written[^1] == asked[0] : written.SequenceEqual(asked);

    private static string NameOf(SourceNode<AttributeSyntax> attribute)
        => attribute.File.Model.GetSymbolInfo(attribute.Syntax).Symbol?.ContainingType is { } type
            ? FullNameOf(type)
            : attribute.Syntax.Name.ToString();

    private static string FullNameOf(INamedTypeSymbol type)
        => type.ContainingType is { } owner ? $"{FullNameOf(owner)}.{type.Name}"
        : type.ContainingNamespace.IsGlobalNamespace ? type.Name
        : $"{type.ContainingNamespace.ToDisplayString()}.{type.Name}";

    private static IReadOnlyList<string> Normalized(string name)
    {
        var segments = (name.StartsWith(Global, StringComparison.Ordinal) ? name[Global.Length..] : name).Split('.');
        return [.. segments[..^1], Trimmed(segments[^1])];
    }

    private static string Trimmed(string name)
        => name.Length > Suffix.Length && name.EndsWith(Suffix, StringComparison.Ordinal)
            ? name[..^Suffix.Length]
            : name;
}
