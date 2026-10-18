using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TSpec.Architecture.CSharp.Internal;

/// <summary>
/// A project compiled from what its build compiled, against the assemblies the running spec project runs
/// with: the types a file's code uses, the types named in its signatures that other assemblies see, and the
/// assemblies it opens its internals to.
/// </summary>
internal sealed class CompiledProject
{
    private const string InternalsVisibleTo = "System.Runtime.CompilerServices.InternalsVisibleToAttribute";

    private static readonly ConcurrentDictionary<(string Project, string Assembly), Lazy<CompiledProject>> _compiled = new();
    private static readonly Lazy<IReadOnlyList<PortableExecutableReference>> _assemblies = new(AssembliesOfTheRun);

    private readonly CSharpCompilation _compilation;
    private readonly Lazy<IReadOnlySet<(TypeKey Owner, TypeKey Named)>> _signatures;
    private readonly Lazy<IReadOnlySet<string>> _friends;

    private CompiledProject(CSharpCompilation compilation)
    {
        _compilation = compilation;
        _signatures = new(() => new HashSet<(TypeKey, TypeKey)>(TypesIn(_compilation.Assembly.GlobalNamespace)
            .Where(IsSeenOutside)
            .SelectMany(type => SignatureOf(type).Select(named => (Key(type), Key(named))))));
        _friends = new(() => new HashSet<string>(_compilation.Assembly.GetAttributes()
            .Where(attribute => attribute.AttributeClass?.ToDisplayString() == InternalsVisibleTo)
            .Select(attribute => attribute.ConstructorArguments.FirstOrDefault().Value).OfType<string>()
            .Select(friend => friend.Split(',')[0].Trim())));
    }

    internal static CompiledProject Of(SourceProject project)
        => _compiled.GetOrAdd((project.Name, project.AssemblyName), _ => new(() => Compile(project))).Value;

    internal IReadOnlySet<string> Friends => _friends.Value;

    internal IEnumerable<TypeKey> NamedBySignaturesOf(TypeKey type)
        => _signatures.Value.Where(signature => signature.Owner == type).Select(signature => signature.Named);

    private static CompiledProject Compile(SourceProject project)
        => new(CSharpCompilation.Create(
            project.AssemblyName,
            ProjectSources.SourceOf(project).Trees,
            _assemblies.Value.Where(assembly => !IsBuiltFrom(project, assembly)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)
                .WithMetadataImportOptions(MetadataImportOptions.Internal)));

    // The project's own assembly would declare its types a second time.
    private static bool IsBuiltFrom(SourceProject project, PortableExecutableReference assembly)
        => string.Equals(
            Path.GetFileNameWithoutExtension(assembly.FilePath), project.AssemblyName, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<PortableExecutableReference> AssembliesOfTheRun()
        => [.. ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(path => MetadataReference.CreateFromFile(path))];

    internal IEnumerable<TypeKey> UsedIn(SyntaxTree tree)
    {
        var model = _compilation.GetSemanticModel(tree);
        return tree.GetRoot().DescendantNodes()
            .Where(node => node is SimpleNameSyntax or ImplicitObjectCreationExpressionSyntax)
            .SelectMany(node => Bound(model.GetSymbolInfo(node)))
            .Select(TypeOf).OfType<INamedTypeSymbol>()
            .Select(Key);
    }

    // A name in a nameof binds to a method group, and so to candidates only.
    private static IEnumerable<ISymbol> Bound(SymbolInfo info) => info.Symbol is { } symbol ? [symbol] : info.CandidateSymbols;

    // A local, a parameter or a local function belongs to the code around it, not to a type it could use.
    private static INamedTypeSymbol? TypeOf(ISymbol symbol)
        => symbol switch
        {
            IAliasSymbol alias => TypeOf(alias.Target),
            INamedTypeSymbol type => type,
            IMethodSymbol { MethodKind: MethodKind.LocalFunction } => null,
            IMethodSymbol or IPropertySymbol or IFieldSymbol or IEventSymbol => symbol.ContainingType,
            _ => null,
        };

    private static IEnumerable<INamedTypeSymbol> TypesIn(INamespaceSymbol @namespace)
        => @namespace.GetTypeMembers().SelectMany(AndNested).Concat(@namespace.GetNamespaceMembers().SelectMany(TypesIn));

    private static IEnumerable<INamedTypeSymbol> AndNested(INamedTypeSymbol type)
        => [type, .. type.GetTypeMembers().SelectMany(AndNested)];

    private static bool IsSeenOutside(ISymbol symbol)
        => symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal
            && (symbol.ContainingType is null || IsSeenOutside(symbol.ContainingType));

    private static IEnumerable<INamedTypeSymbol> SignatureOf(INamedTypeSymbol type)
        => new[] { type.BaseType }.OfType<ITypeSymbol>()
            .Concat(type.Interfaces)
            .Concat(Constraints(type.TypeParameters))
            .Concat(type.GetMembers().Where(IsSeenMember).SelectMany(TypesOf))
            .SelectMany(Named);

    // A nested type is a type of its own, with a signature of its own.
    private static bool IsSeenMember(ISymbol member) => member is not INamedTypeSymbol && IsSeenOutside(member);

    private static IEnumerable<ITypeSymbol> TypesOf(ISymbol member)
        => member switch
        {
            IMethodSymbol method => [method.ReturnType, .. TypesOf(method.Parameters), .. Constraints(method.TypeParameters)],
            IPropertySymbol property => [property.Type, .. TypesOf(property.Parameters)],
            IFieldSymbol field => [field.Type],
            IEventSymbol @event => [@event.Type],
            _ => [],
        };

    private static IEnumerable<ITypeSymbol> TypesOf(ImmutableArray<IParameterSymbol> parameters)
        => parameters.Select(parameter => parameter.Type);

    private static IEnumerable<ITypeSymbol> Constraints(ImmutableArray<ITypeParameterSymbol> parameters)
        => parameters.SelectMany(parameter => parameter.ConstraintTypes);

    private static IEnumerable<INamedTypeSymbol> Named(ITypeSymbol type)
        => type switch
        {
            INamedTypeSymbol named => [named, .. named.TypeArguments.SelectMany(Named)],
            IArrayTypeSymbol array => Named(array.ElementType),
            IPointerTypeSymbol pointer => Named(pointer.PointedAtType),
            IFunctionPointerTypeSymbol pointer => TypesOf(pointer.Signature).SelectMany(Named),
            _ => [],
        };

    private static TypeKey Key(INamedTypeSymbol type) => TypeKey.Of(type.OriginalDefinition);
}
