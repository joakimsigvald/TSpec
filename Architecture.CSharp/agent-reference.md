# TSpec.Architecture.CSharp — Agent Reference

Condensed reference for AI coding agents writing architecture rules with TSpec.Architecture.CSharp (covers 1.6).
It reads the projects of a TSpec build, their references, and the code inside them.
Full documentation: [README.md](https://github.com/joakimsigvald/TSpec/blob/main/Architecture.CSharp/README.md).

- Put the tests in a spec project of their own that references every project: `<ProjectReference Include="..\**\*.csproj" Exclude="..\Architecture.Spec\**" />`.
- `using TSpec.Architecture.CSharp;` — not `TSpec.Architecture` too, whose `Source` and `Segment` are deprecated and would clash.
- Group rules in folders by architecture domain (`Naming`, `Layout`, `Sizes`), not mirroring the source: a rule reads every project. The namespace follows the folder; the architecture page heads each folder.
- `Uses`, `Exposes`, `DependsOn`, `DerivesFrom`, `HasAttribute`, `Gives` and `Takes` compile the project once a run; reflection is not seen.

## Projects

`Source.Projects` lists every project its build reaches, with their direct project and package references. The spec project itself is `Source.Projects.CurrentSpec`; `Within` and `NonRedundant` leave it out.

```csharp
[Fact]
public void AreOnlyTheseAndNotRedundant()
    => Source.Projects.Is().Within(p => p switch
    {
        "." => ["Entry", "Infra"],
        "Entry" or "Core" => ["Contract"],
        "Infra" => ["Core"],
        "Spec" => ["."],
        "Core.Spec" => ["Core"],
        _ => [],
    }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]).and.NonRedundant();
```

- Names are assembly names under a common root within the solution. `root.X` → `X`, the root → `.`, other projects → `/Name`. Packages are always `P:Name`. A reference that produces no assembly (an `.esproj`) is not in the graph.
- Rules add up: a dependency passes if any rule allows it; `_ => [...]` applies to every project. A trailing `*` matches the rest (`P:Moq.*` does not match `P:Moq`). `Within` allows, never requires.
- `NonRedundant()`: no project references a project it already reaches through another; packages, and `A.B -> A` (`Core.Spec -> Core`), are exempt.
- Layer rules: switch on `p.Segment(0)` (also `Segment(^1)`, `Segment(..2)`).
- Custom rules: `Source.Projects["X"]` (an unknown name throws `SetupFailed`) is a `SourceProject` with `Name`, `AssemblyName`, `Dependencies` and `Reaches` (directly or through other projects, packages included), named as the rules see them. `IsSpecProject`: it reaches TSpec, whatever its name. `OpensInternalsTo`: the projects its `InternalsVisibleTo` names.
- Moved here from TSpec in 1.6, where the old ones are deprecated.

## Project

- `Files` are the source files it compiles from its own folder, leaving out what the build generates under `obj/` and files linked in from elsewhere.
- `Types` are those of all its files, a partial type once; `Members` and `Nodes<T>()` are all its files'.
- `AllTypes` (on a file too): its types and those nested in them, however deep. `Types` are the top-level ones only, so a rule on every public type reads `AllTypes`.
- `Folders`: a `DependencyGraph` of the folders directly in its own, each with the folders whose types its files use. A file at the project's root is in no folder. Assert with `Folders.Is().Within(...)`.

## Dependency graph

- `new DependencyGraph("Namespace", dependencies)`: each node's name and the names it depends on; the first argument heads the table. `Is().Within(...)` takes the rules `Source.Projects` takes.
- Its test draws what the rules allow above its table in the generated specification. A target ending in `*` draws no arrow.
- `Is().Acyclic()`: no nodes depend on each other around a loop; a failure names the first (`A -> B -> A`).
- Moved here from TSpec in 1.6, where the old one is deprecated: `using TSpec.Architecture.CSharp;`.

## File

- `Path` in the project (written with `/`), `Lines`, `Raw` text and the `Types` declared at its top level. A failure names it `X/Path`.
- `Folder`, `Name` and `Extension` are parts of `Path`: for `Stores/Cache/UserStore.cs`, `Stores` (the top-level folder, as `Folders` names it; empty at the project's root), `UserStore` and `cs`.
- `Root` is its Roslyn syntax as the build parsed it, so code under an `#if` the build left out is not in it. `Model` is Roslyn's `SemanticModel` of that syntax; a type the spec project does not run with is unresolved. `file.SpanOf(node)` is where a node or token starts and ends: a `Start` and an `End`, each a `Line` and `Column` counted from 1.
- `Nodes<T>()`: its syntax nodes of one kind in written order, directives included, nothing inside a documentation comment. Each has `File`, `Syntax` and `Span` (with `Lines`), and a failure names it `X/Path:line`.
- `Members`: those it declares, nested types' and local functions too.
- `TopLevelSize`: its top-level statements measured as one member's `Size`.
- `Uses`: the build's types its code uses, through a member too (an extension call, a constant). A nested type is one of its own.
- `DependsOn()`: the projects and packages whose types its code uses (`Contract`, `P:Dapper`), not its own project nor the framework; one reached only through another project counts. `DependsOn(name)` asks for one, a trailing `*` matching as in `Within`.
- To add an analysis, cache it per file in a `ConditionalWeakTable<SourceFile, T>` behind an extension member; a file is the same object every time.

## Type

- `Name` (no type parameters), `FullName`, `Namespace`, `Kind`, `IsPublic`, `Files` and `Nested` (the types declared inside it). A nested type's `IsPublic` is in effect. A failure names it by its full name.
- `Exposes`: the build's types named in its signature that other assemblies see, itself included.
- `DerivesFrom(typeof(Exception))`, `DerivesFrom("Entity")`: the given type (or name/full name) is its base type or a base's own base, however deep, type arguments ignored. An implemented interface is no base.
- `Attributes` (written on every part of a partial) and `HasAttribute(name)`: the name counts with or without the `Attribute` suffix. Plain (`"Serializable"`), it matches that name in any namespace; qualified (`"System.Serializable"`), only that attribute — however the code wrote it.

## Member

- `Accessibility` is in effect: no more than the types around it allow. `Type` and `Parameters` are syntax, as written. A record's positional parameters are its constructor's and its properties.
- `IsOriginal`: it neither implements an interface's member (explicitly or implicitly) nor overrides a base type's, so it sets its own name and signature; a naming rule reads only original members.
- `DeclaringType`: the type it is declared in — the type itself for a primary constructor, the class for an extension block's member, null at the file's top level. `HasAttribute(name)` as a type's.
- `Gives` is the types it returns, including those inside generic types, tuples and arrays; `Takes` is the same for its parameters. With no argument they list the types (`ITypeSymbol`). Ask if one is included with one argument that is a `TypeKind`, a `typeof(List<>)` (type arguments ignored) or a condition. A type it only implements doesn't count: `List<int>` gives no `IEnumerable<>`.
- `Size`: its size, computed from line count, indentation and complexity. A rule sets only the ceiling: `Has().None(member => member.Size > 240)`.
- A failure names it `X/Path:line Name`.
