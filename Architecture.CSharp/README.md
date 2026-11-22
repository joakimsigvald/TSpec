# TSpec.Architecture.CSharp

Architecture rules on a [TSpec](https://github.com/joakimsigvald/TSpec#readme) build: its projects and
their references, and the code inside them — their source files and folders, the types those declare
and their Roslyn syntax.

Put the architecture tests in a spec project of their own, say `Architecture.Spec`, that references
every project in the solution, and add the package to it:

```xml
<ItemGroup>
  <ProjectReference Include="..\**\*.csproj" Exclude="..\Architecture.Spec\**" />
</ItemGroup>
```

```shell
dotnet add package TSpec.Architecture.CSharp
```

A condensed reference for coding agents ships with the package, beside TSpec's:
`~/.nuget/packages/tspec.architecture.csharp/<version>/agent-reference.md`.

Opt in with `[assembly: AssemblyFixture(typeof(TSpec.Architecture.CSharp.ArchitectureDocument))]`, and a
clean full run writes `_specification.md` to the spec project: a diagram of the solution's projects and
their references, each linked to its specification where it has one, and the architecture tests below it.
Only `Spec` classes count, so derive the test classes from `Spec`.

Group these rules in folders by architecture domain — `Naming`, `Layout`, `Sizes` — not by the code they
read: a rule reads every project, so there is no part of the source for its folder to mirror. The
architecture page heads each folder and links each rule's class to its file.

Everything below is reached with `using TSpec.Architecture.CSharp;`. `Uses`, `Exposes`, `DependsOn`,
`DerivesFrom`, `HasAttribute`, `Gives`, `Takes`, `Symbol` and `TryFind` compile the project with Roslyn,
once in a run, and a use by reflection is not one the compiler sees.

## Projects

`Source.Projects` is read from the spec project's build: every project the build reaches, each with the
projects and packages it references directly. `Architecture.Spec` itself is among them as
`Source.Projects.CurrentSpec`, and the assertions below leave it out, since it references every project.

Assert that the graph holds only what the architecture allows, and that no project references what it
already reaches through another:

```csharp
public class ProjectDependencies : Spec
{
    [Fact]
    public void AreOnlyTheseAndNotRedundant()
        => Source.Projects.Is().Within(p => p switch
        {
            "." => ["Entry", "Infra", "P:Scalar.AspNetCore"],
            "Entry" or "Core" => ["Contract"],
            "Infra" => ["Core"],
            "Spec" => ["."],
            "Core.Spec" => ["Core"],
            _ => [],
        }, p => p.Segment(^1) == "Spec" ? ["P:*"] : ["P:Microsoft.*"]).and.NonRedundant();
}
```

- **Names are assembly names, under a common root within the solution.** For `MyHotel.Architecture.Spec`,
  `MyHotel.Entry` is `Entry`, `MyHotel` itself is `.`, and a project outside the root is `/` and its
  name. A package is `P:` and its name.
- **Rules add up**: a dependency passes if any rule allows it, so `_ => [...]` allows its targets to
  every project.
- **A target ending in `*`** allows every name that begins with what precedes it: `P:Microsoft.*`
  allows `P:Microsoft.Extensions.Logging` but not `P:Microsoft`.
- **`Within` allows; it does not require** — the compiler already demands the references the code needs.
- **`NonRedundant()` counts projects only**, and never a reference from `A.B` to `A`: `Core.Spec` must
  reference the `Core` it specifies directly.
- **A reference that produces no assembly**, such as one to an `.esproj`, is not in the graph.

For rules on layers rather than projects, switch on a segment of the name: `p.Segment(0)` is the
first, `p.Segment(^1)` the last and `p.Segment(..2)` the first two joined by a dot, as in
`p => p.Segment(0) switch { "Data" => ["Domain.*"], _ => [] }`.

For a rule the assertions don't cover, read the projects and assert on what you compute:
`Source.Projects["Data.Auth"]` gives one; each has its `Name`, its `AssemblyName`, its `Dependencies`
and what it `Reaches`, directly or through other projects, as the rules see them. A name that is no
project of the build throws `SetupFailed`. A project `IsSpecProject` when it reaches TSpec, and it
`OpensInternalsTo` the projects its `InternalsVisibleTo` names.

These moved here from TSpec in 1.6, where the old ones are deprecated.

## Project

A project's `Files` are those it compiles from its own folder, leaving out what the build generates
under `obj/` and files linked in from elsewhere. Its `Types`, `Members` and `Nodes<T>()` are those of all
its files, a partial type once. Its `AllTypes` are its types and those nested in them however deep.
`TryFind(symbol, out var type)` looks up the project's own type for a Roslyn type symbol, such as one a
member `Gives`. It ignores type arguments, and finds nothing for another project's type or the
framework's.

A project's `Folders` are the folders directly in its own that hold its files, each with the others it
uses: one of its files uses a type a file of the other declares. A file at the project's root is in no
folder. They are a [`DependencyGraph`](#dependency-graph), so `Within` takes rules on them as on the
projects, and the generated specification draws what the rules allow:

```csharp
[Fact]
public void A_subdomain_uses_only_those_below_it()
    => Source.Projects["Application"].Folders.Is().Within(folder => folder switch
    {
        "Generation" or "Assistant" => ["Chat"],
        "Workbench" => ["Generation", "Chat"],
        _ => [],
    });
```

Without rules, `Folders.Is().Acyclic()` keeps a project's folders one way.

## Dependency graph

A `DependencyGraph` holds named nodes, each with the names it depends on:
`new DependencyGraph("Namespace", dependencies)`, where the first argument says what a node is. A
project's `Folders` are one.

- `Is().Within(...)` asserts that each node depends only on what one of the rules allows it. The rules
  are written as for [`Source.Projects`](#projects).
- In the generated specification, the test draws what the rules allow above a table of what each node
  may use: an arrow for each target written in full, while a target ending in `*` is only in the table.
- `Is().Acyclic()` asserts that no nodes depend on each other around a loop, and a failure names the
  first: `A -> B -> A`.

It moved here from TSpec in 1.6, where the old one is deprecated.

## File

A file has its `Path` in the project, written with `/`, and its `Raw` text, and a failure names it by
project and path: `Data.Auth/Stores/UserStore.cs`. A file's `Folder`, `Name` and `Extension` are parts of
its `Path`: for `Stores/Cache/UserStore.cs`, `Stores`, the folder directly in the project's, empty at the
project's root; `UserStore`; and `cs`. Its `Types` are those declared at its top level, and its
`AllTypes` those and the types nested in them.

A file's `SourceLines` are its lines as an editor counts them. Each has its `File`, its `Number`, counted
from 1, and its `Text`, and a failure names it by file and line. A line `IsCode` unless it continues a
string or a comment that opens on an earlier line, such as the content of a raw string literal:

```csharp
[Fact]
public void A_line_of_code_is_at_most_120_characters()
    => Source.Projects.SelectMany(project => project.Files).SelectMany(file => file.SourceLines)
        .Has().None(line => line.IsCode && line.Text.Length > 120);
```

`Lines`, the same lines as text alone, is deprecated.

A file's `Root` is its Roslyn syntax as the build parsed it, so code under an `#if` the build left
out is not in it, and its `Model` is the compiler's view of that syntax: what a name refers to, the type
of an expression. A type the spec project does not run with, or one a source generator wrote, is
unresolved there. `SpanOf(node)` is where a node or token starts and ends, its lines and columns
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

A file's `Nodes<T>()` are its syntax nodes of one kind, in the order they are written: a directive such
as `#pragma` among them, nothing inside a documentation comment. Each has its `File`, its `Syntax` and
its `Span`, whose `Lines` are the lines it covers, and a failure names it by file and line:
`Data.Auth/Stores/UserStore.cs:12`.

```csharp
[Fact]
public void No_query_is_written_in_query_syntax()
    => Source.Projects["Core"].Nodes<QueryExpressionSyntax>().Is().Empty();
```

A file's `Comments` are its comments as a reader sees them: `//` lines in a row, each on its own line, are
one comment, as the lines of a `///` block are. Each has its `File`, its `Kind` (`Line`, `Block`,
`Documentation` or `DocumentationBlock`), its `Span`, its `Text`, and a failure names it by file and line.
A comment `IsTrailing` when it follows code on its line, and otherwise `Leads` the code below it: the
largest declaration or statement that starts there, its attributes included.

```csharp
[Fact]
public void A_comment_is_at_most_three_lines()
    => Source.Projects["Core"].Files.SelectMany(file => file.Comments)
        .Has().None(comment => comment.Kind == CommentKind.Line && comment.Span.Lines > 3);
```

A file's `Members` are those it declares (see [Member](#member)), and its `TopLevelSize` measures its
top-level statements as a member's `Size` is measured.

A file's `Uses` are the build's types its code uses: those it names, and those whose members it names,
such as an extension method's class or a constant's type; a nested type is one of its own.

```csharp
[Fact]
public void The_domain_uses_nothing_of_persistence()
    => Source.Projects["Core"].Files.Where(file => file.Path.StartsWith("Domain/"))
        .Has().None(file => file.Uses.Any(type => type.Namespace.EndsWith(".Persistence")));
```

A file `DependsOn()` the projects and packages whose types its code uses, named as the project rules name
them: `Contract`, `P:Dapper`. Its own project and the framework are none, and a project it reaches only
through another counts, unlike in its project's `Dependencies`. `DependsOn(name)` asks for one, and a name
ending in `*` matches as in `Within`:

```csharp
[Fact]
public void Only_the_connection_calls_Dapper()
    => Source.Projects["Data"].Files.Where(file => file.Path != "Store/Connection.cs")
        .Has().None(file => file.DependsOn("P:Dapper"));
```

A file is the same object every time, so an analysis TSpec doesn't have can be added to it as an
extension member that keeps what it computes per file in a `ConditionalWeakTable<SourceFile, T>`.

To test a rule's condition on examples, `Source.Snippet(code)` compiles a piece of code into a file of its
own, the way the build's files are compiled, so its names resolve and it can use the framework's types and
the build's. Hand its nodes or members to the same condition the rule runs. A snippet is in no project of
the build, and a failure names it `Snippet/Snippet.cs`:

```csharp
[Theory]
[InlineData("catch { }")]
[InlineData("catch (Exception) { }")]
public void A_catch_of_every_exception_breaks_the_rule(string clause)
    => TakesEverything(Source.Snippet($$"""
        using System;
        class C { void M() { try { } {{clause}} } }
        """).Nodes<CatchClauseSyntax>().Single()).Is().True();
```

## Type

A type has its `Name` (`Repository`), its `FullName` (`Data.Repository<T>`), `Namespace`, `Kind`,
`IsPublic`, the `Files` it is declared in, several for a partial type, its `Nested` types, declared
inside it, and the `DeclaringType` it is nested in, none at a file's top level. A nested type's `IsPublic` is in effect: false inside a type that is not public. A failure
names a type by its full name. `Types` are the top-level ones only, so a rule on every public type reads
`AllTypes`. A rule of one public type per file, adopted with today's breaches listed in `Allowed`:

```csharp
[Fact]
public void Only_the_listed_files_declare_more_than_one_public_type()
    => Source.Projects.SelectMany(project => project.Files)
        .Select(file => (file.ToString(), Public: file.Types.Count(type => type.IsPublic)))
        .Where(file => file.Public > 1)
        .Are(Allowed);
```

A type `Exposes` the build's types its signature names where other assemblies see it: its base type,
interfaces and constraints, and the types of its public and protected members, itself included.

A type `DerivesFrom` all types in its inheritance chain. For generic types, the generic argument is
ignored. A type does not derive from an interface. All types derive from `object`. A base the spec
project cannot reference is given by name instead, plain or fully qualified: `DerivesFrom("Entity")`.

```csharp
[Fact]
public void Our_exceptions_live_in_an_Exceptions_folder()
    => Source.Projects["Contract"].AllTypes.Where(type => type.DerivesFrom(typeof(Exception)))
        .Has().All(type => type.Files.All(file => file.Path.Contains("/Exceptions/")));
```

A type's `Attributes` are those written on it — on every declaration, for a partial type. `HasAttribute`
asks for one by name, in any form the code could write it: with or without the `Attribute` suffix, plain
or qualified. A plain name matches an attribute of that name in any namespace; a qualified one only the
attribute it names. So `HasAttribute("Serializable")` finds both `[System.Serializable]` and a
`[MyOwn.Serializable]`, while `HasAttribute("System.Serializable")` finds only the first, however the
code wrote it.

A type's `Symbol` is its Roslyn symbol, for example to see which interfaces it implements:
`type.Symbol.AllInterfaces`.

## Member

- A file's `Members` are all the members it declares, including those of nested types and local
  functions. A project's `Members` are those of all its files.
- A record's positional parameter is two members: a constructor parameter and a property.
- `Accessibility` is the accessibility in effect, not the one written: a public member of an internal
  class is internal.
- `Type` and `Parameters` are syntax, as written. For the types they resolve to, see `Gives` and
  `Takes` below.
- `DeclaringType` is the type the member is declared in. For a primary constructor it is that type
  itself; for a member of an extension block, the class around the block; for a delegate declared at
  a file's top level, none.
- `HasAttribute` asks for an attribute by name, as a type's does.
- `Symbol` is its Roslyn symbol. A positional property's is the property, not the parameter, a primary
  constructor's is the constructor, and a delegate's is the delegate type.
- A member `IsOriginal` when it chose its own name and signature: it neither overrides a base type's
  member nor implements an interface's. A naming rule should check only original members, since an
  override of `GetHashCode` cannot rename it.
- A failure names a member by file, line and name: `Data.Auth/Stores/UserStore.cs:12 Find`.

```csharp
[Fact]
public void No_method_name_ends_in_Async()
    => Source.Projects.SelectMany(project => project.Members)
        .Where(member => member.Kind == MemberKind.Method && member.IsOriginal)
        .Has().None(method => method.Name.EndsWith("Async"));
```

A member's `Size` measures its size, computed from its line count, indentation and complexity:

```csharp
[Fact]
public void No_member_is_over_size_240()
    => Source.Projects.SelectMany(project => project.Members).Has().None(member => member.Size > 240);
```

A member `Gives` a type when the type it returns or holds is that type, or has it written inside: as a
type argument, `Task<T>`'s too, a tuple's element or an array's. It `Takes` one when a parameter's type
does. A named type's own members are not inside it, so a `Booking` with a `Status` gives no enum, and a
type is not one it only implements: `List<int>` gives no `IEnumerable<>`. Each takes a kind, a type
whatever its type arguments, or a condition, and without one lists the types, each once. A member
`Carries` a type it gives or takes:

```csharp
[Fact]
public void No_contract_member_gives_an_enum_or_takes_a_list()
    => Source.Projects["Contract"].Members
        .Has().None(member => member.Gives(TypeKind.Enum) || member.Takes(typeof(List<>)));
```
