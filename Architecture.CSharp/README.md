# TSpec.Architecture.CSharp

Architecture rules on the code inside the projects of a [TSpec](https://github.com/joakimsigvald/TSpec#readme)
build: their source files, the types those declare and their Roslyn syntax. The projects and their
references are TSpec's own, in [its README's Architecture section](https://github.com/joakimsigvald/TSpec#7-architecture).

Install the package in your architecture spec project:
```shell
dotnet add package TSpec.Architecture.CSharp
```

With `using TSpec.Architecture.CSharp;`, a project's `Files` are those it compiles from its own folder,
leaving out what the build generates under `obj/` and files linked in from elsewhere. A file has its
`Path` in the project, written with `/`, its `Lines` and its `Raw` text, and a failure names it by
project and path: `Data.Auth/Stores/UserStore.cs`. Its `Types` are those declared at its top level,
each with its `Name` (`Repository`), its `FullName` (`Data.Repository<T>`), `Namespace`, `Kind`,
`IsPublic`, the `Files` it is declared in, several for a partial type, and its `Nested` types, declared
inside it. A nested type's `IsPublic` is in effect: false inside a type that is not public. A failure
names a type by its full name. A project's `Types` are those of all its files, a partial type once. A
rule of one public type per file, adopted with today's breaches listed in `Allowed`:

```csharp
[Fact]
public void Only_the_listed_files_declare_more_than_one_public_type()
    => Source.Projects.SelectMany(project => project.Files)
        .Select(file => (file.ToString(), Public: file.Types.Count(type => type.IsPublic)))
        .Where(file => file.Public > 1)
        .Are(Allowed);
```

A file's `Root` is its Roslyn syntax as the build parsed it, so code under an `#if` the build left
out is not in it, and `SpanOf(node)` is where a node or token starts and ends, its lines and columns
counted from 1:

```csharp
[Fact]
public void No_arrow_starts_a_line()
    => Source.Projects.SelectMany(project => project.Files)
        .SelectMany(file => file.Root.DescendantTokens()
            .Where(token => token.IsKind(SyntaxKind.EqualsGreaterThanToken))
            .Where(arrow => file.SpanOf(arrow).Start.Line > file.SpanOf(arrow.GetPreviousToken()).End.Line)
            .Select(arrow => $"{file}:{file.SpanOf(arrow).Start.Line}"))
        .Is().Empty();
```

A file's `Members` are those it declares, a nested type's and local functions too, and a project's are
those of all its files. A record's positional parameters are its constructor's and its properties. A
member's `Accessibility` is the one it has in effect, no more than the types around it allow: a public
member of an internal class is internal. Its `Type` and `Parameters` are syntax, as written, and a
failure names it by file, line and name: `Data.Auth/Stores/UserStore.cs:12 Find`.

```csharp
[Fact]
public void No_method_name_ends_in_Async()
    => Source.Projects.SelectMany(project => project.Members)
        .Where(member => member.Kind == MemberKind.Method)
        .Has().None(method => method.Name.EndsWith("Async"));
```

A file's `Uses` are the build's types its code uses: those it names, and those whose members it names,
such as an extension method's class or a constant's type; a nested type is one of its own. A
type `Exposes` the build's types its signature names where other assemblies see it: its base type,
interfaces and constraints, and the types of its public and protected members, itself included. Both
compile the project with Roslyn, once in a run, and a use by reflection is not one the compiler sees. A
project `OpensInternalsTo` the projects its `InternalsVisibleTo` names.

```csharp
[Fact]
public void The_domain_uses_nothing_of_persistence()
    => Source.Projects["Core"].Files.Where(file => file.Path.StartsWith("Domain/"))
        .Has().None(file => file.Uses.Any(type => type.Namespace.EndsWith(".Persistence")));
```

A file is the same object every time, so an analysis TSpec doesn't have can be added to it as an
extension member that keeps what it computes per file in a `ConditionalWeakTable<SourceFile, T>`.
