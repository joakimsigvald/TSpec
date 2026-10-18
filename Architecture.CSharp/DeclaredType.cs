using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A type declared at the top level of its files, or nested in another type as one of its Nested types
/// </summary>
public sealed class DeclaredType
{
    private readonly IReadOnlyList<string> _typeParameters;
    private readonly Lazy<IReadOnlyList<DeclaredType>> _exposes;

    internal DeclaredType(
        string name, IReadOnlyList<string> typeParameters, string @namespace, DeclaredKind kind, bool isPublic,
        IReadOnlyList<SourceFile> files, DeclaredType? owner)
    {
        Name = name;
        _typeParameters = typeParameters;
        Namespace = @namespace;
        Kind = kind;
        IsPublic = isPublic;
        Files = files;
        Owner = owner;
        _exposes = new(() => BuildTypes.ExposedBy(this));
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
    /// The type's name qualified with its namespace and the types it is nested in, with its type
    /// parameters: "Data.Repository&lt;T&gt;"
    /// </summary>
    public string FullName
        => Owner is not null ? $"{Owner.FullName}.{Declared}"
        : Namespace.Length == 0 ? Declared
        : $"{Namespace}.{Declared}";

    // Repository and Repository<T> are two types, so their full names must differ.
    private string Declared => _typeParameters.Count == 0 ? Name : $"{Name}<{string.Join(", ", _typeParameters)}>";

    /// <summary>
    /// What the type is
    /// </summary>
    public DeclaredKind Kind { get; }

    /// <summary>
    /// Whether the type is public in effect: declared public, inside types that all are
    /// </summary>
    public bool IsPublic { get; }

    /// <summary>
    /// The files the type is declared in, in the ordinal order of their paths: more than one for a
    /// partial type
    /// </summary>
    public IReadOnlyList<SourceFile> Files { get; }

    /// <summary>
    /// The types of the build the type's signature names where other assemblies see it, in the ordinal
    /// order of their full names: its base type, interfaces and constraints, and the types of its public
    /// and protected members; itself too, when one of its members names it. None for a type other
    /// assemblies do not see. Its project is compiled to find out, once in a run
    /// </summary>
    public IReadOnlyList<DeclaredType> Exposes => _exposes.Value;

    /// <summary>
    /// The types declared inside the type, in the ordinal order of their full names
    /// </summary>
    public IReadOnlyList<DeclaredType> Nested { get; internal set; } = [];

    internal DeclaredType? Owner { get; }

    internal int Arity => _typeParameters.Count;

    /// <summary>
    /// The type as a failure names it: its full name
    /// </summary>
    public override string ToString() => FullName;
}
