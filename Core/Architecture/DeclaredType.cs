namespace TSpec.Architecture;

/// <summary>
/// A type declared at the top level of its files; a nested type stays with its owner
/// </summary>
public sealed class DeclaredType
{
    private readonly IReadOnlyList<string> _typeParameters;

    internal DeclaredType(
        string name, IReadOnlyList<string> typeParameters, string @namespace, DeclaredKind kind, bool isPublic,
        IReadOnlyList<SourceFile> files)
    {
        Name = name;
        _typeParameters = typeParameters;
        Namespace = @namespace;
        Kind = kind;
        IsPublic = isPublic;
        Files = files;
    }

    /// <summary>
    /// The type's name without its type parameters, as its file is named: "Repository"
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type's namespace, empty in the global namespace
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The type's name qualified with its namespace, with its type parameters: "Data.Repository&lt;T&gt;"
    /// </summary>
    public string FullName => Namespace.Length == 0 ? Declared : $"{Namespace}.{Declared}";

    // Repository and Repository<T> are two types, so their full names must differ.
    private string Declared => _typeParameters.Count == 0 ? Name : $"{Name}<{string.Join(", ", _typeParameters)}>";

    /// <summary>
    /// What the type is
    /// </summary>
    public DeclaredKind Kind { get; }

    /// <summary>
    /// Whether the type is public
    /// </summary>
    public bool IsPublic { get; }

    /// <summary>
    /// The files the type is declared in, in the ordinal order of their paths: more than one for a
    /// partial type
    /// </summary>
    public IReadOnlyList<SourceFile> Files { get; }

    /// <summary>
    /// The type as a failure names it: its full name
    /// </summary>
    public override string ToString() => FullName;
}
