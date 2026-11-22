using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TSpec.Architecture.CSharp.Internal;

namespace TSpec.Architecture.CSharp;

/// <summary>
/// A type declared at the top level of its files, or nested in another type as one of its Nested types
/// </summary>
public sealed class DeclaredType
{
    private readonly IReadOnlyList<string> _typeParameters;
    private readonly Lazy<IReadOnlyList<DeclaredType>> _exposes;
    private readonly Lazy<INamedTypeSymbol> _symbol;
    private readonly Lazy<IReadOnlyList<INamedTypeSymbol>> _baseTypes;

    internal DeclaredType(
        string name, IReadOnlyList<string> typeParameters, string @namespace, DeclaredKind kind, bool isPublic,
        IReadOnlyList<SourceFile> files, IReadOnlyList<SourceNode<MemberDeclarationSyntax>> declarations,
        DeclaredType? owner)
    {
        Name = name;
        _typeParameters = typeParameters;
        Namespace = @namespace;
        Kind = kind;
        IsPublic = isPublic;
        Files = files;
        Declarations = declarations;
        DeclaringType = owner;
        _exposes = new(() => BuildTypes.ExposedBy(this));
        _symbol = new(SymbolOf);
        _baseTypes = new(() => BaseTypes.Of(Symbol));
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
        => DeclaringType is not null ? $"{DeclaringType.FullName}.{Declared}"
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
    /// The type's Roslyn symbol, for example to see which interfaces it implements. Its project is
    /// compiled to find out, once in a run
    /// </summary>
    public INamedTypeSymbol Symbol => _symbol.Value;

    /// <summary>
    /// The types of the build the type's signature names where other assemblies see it, in the ordinal
    /// order of their full names: its base type, interfaces and constraints, and the types of its public
    /// and protected members; itself too, when one of its members names it. None for a type other
    /// assemblies do not see. Its project is compiled to find out, once in a run
    /// </summary>
    public IReadOnlyList<DeclaredType> Exposes => _exposes.Value;

    /// <summary>
    /// The files of the build whose code uses the type, as their <see cref="SourceFile.Uses"/> say: its own
    /// files too, where they name it. In the ordinal order of their projects and paths. Every project of the
    /// build is compiled to find out, once in a run
    /// </summary>
    public IReadOnlyList<SourceFile> UsedBy => TypeUsers.Of(this);

    /// <summary>
    /// The attributes written on the type, on every declaration of a partial type
    /// </summary>
    public IReadOnlyList<AttributeSyntax> Attributes => [.. AttributeNodes.Select(attribute => attribute.Syntax)];

    /// <summary>
    /// Whether an attribute of the given name is written on the type, however the code wrote it. The name
    /// counts with or without the Attribute suffix; plain, it matches an attribute of that name in any
    /// namespace, qualified only the one it names. Its project is compiled to find out, once in a run
    /// </summary>
    /// <param name="name">The attribute's name: "Serializable", "System.SerializableAttribute"</param>
    public bool HasAttribute(string name) => WrittenAttributes.Has(AttributeNodes, name);

    /// <summary>
    /// Whether the type derives from the given type: it is the type's base type, or a base type's own base,
    /// however deep and whatever its type arguments. An interface the type implements is no base. The
    /// type's project is compiled to find out, once in a run
    /// </summary>
    /// <param name="type">The base type: <c>typeof(Exception)</c>, <c>typeof(Container&lt;&gt;)</c></param>
    public bool DerivesFrom(Type type) => _baseTypes.Value.Any(baseType => MemberTypes.IsOf(baseType, type));

    /// <summary>
    /// Whether the type derives from a base type of the given name or full name, for a base the rule
    /// cannot name at runtime, as <c>DerivesFrom(Type)</c> reads the bases
    /// </summary>
    /// <param name="name">The base type's name: "Entity", "MyApp.Domain.Entity"</param>
    public bool DerivesFrom(string name)
        => _baseTypes.Value.Any(baseType => baseType.Name == name || baseType.ToDisplayString() == name);

    /// <summary>
    /// The types declared inside the type, in the ordinal order of their full names
    /// </summary>
    public IReadOnlyList<DeclaredType> Nested { get; internal set; } = [];

    /// <summary>
    /// The type the type is nested in, directly; none for a type at the top level of its files
    /// </summary>
    public DeclaredType? DeclaringType { get; }

    internal IReadOnlyList<SourceNode<MemberDeclarationSyntax>> Declarations { get; }

    private IEnumerable<SourceNode<AttributeSyntax>> AttributeNodes
        => Declarations.SelectMany(declaration => declaration.Syntax.AttributeLists
            .SelectMany(list => list.Attributes)
            .Select(attribute => new SourceNode<AttributeSyntax>(declaration.File, attribute)));

    internal IEnumerable<DeclaredType> AndNested => [this, .. Nested.SelectMany(type => type.AndNested)];

    internal int Arity => _typeParameters.Count;

    /// <summary>
    /// The type as a failure names it: its full name
    /// </summary>
    public override string ToString() => FullName;

    private INamedTypeSymbol SymbolOf()
        => (INamedTypeSymbol)Declarations[0].File.Model.GetDeclaredSymbol(Declarations[0].Syntax)!;
}
