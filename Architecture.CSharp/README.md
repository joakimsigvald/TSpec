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
project and path: `Data.Auth/Stores/UserStore.cs`. Its `Types` are those declared at its top level, a
nested type staying with its owner, each with its `Name` (`Repository`), its `FullName`
(`Data.Repository<T>`), `Namespace`, `Kind`, `IsPublic` and the `Files` it is declared in, several for
a partial type; a failure names a type by its full name. A project's `Types` are those of all its
files, a partial type once. A rule of one public type per file, adopted with today's breaches listed in
`Allowed`:

```csharp
[Fact]
public void Only_the_listed_files_declare_more_than_one_public_type()
    => Source.Projects.SelectMany(project => project.Files)
        .Select(file => (file.ToString(), Public: file.Types.Count(type => type.IsPublic)))
        .Where(file => file.Public > 1)
        .Are(Allowed);
```

A file's `Root` is its Roslyn syntax as the build parsed it, so code under an `#if` the build left
out is not in it, and `Line(node)` is the line a node starts on:

```csharp
[Fact]
public void No_method_name_ends_in_Async()
    => Source.Projects.SelectMany(project => project.Files)
        .SelectMany(file => file.Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(method => method.Identifier.Text.EndsWith("Async"))
            .Select(method => $"{file}:{file.Line(method)}"))
        .Is().Empty();
```

A file is the same object every time, so an analysis TSpec doesn't have can be added to it as an
extension member that keeps what it computes per file in a `ConditionalWeakTable<SourceFile, T>`.
