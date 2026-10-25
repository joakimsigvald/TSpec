# TSpec.Architecture.CSharp — Agent Reference

Condensed reference for AI coding agents writing architecture rules with TSpec.Architecture.CSharp (covers 1.4).
It reads the code inside the projects of a TSpec build; the projects and their references (`Source.Projects`, `Within`, `NonRedundant`) are TSpec's own, in TSpec's `agent-reference.md`.
Full documentation: [README.md](https://github.com/joakimsigvald/TSpec/blob/main/Architecture.CSharp/README.md).

- `using TSpec.Architecture.CSharp;`.
- `Uses`, `Exposes`, `DependsOn`, `Gives` and `Takes` compile the project once a run; reflection is not seen.

## Project

- `Files` are the source files it compiles from its own folder, leaving out what the build generates under `obj/` and files linked in from elsewhere.
- `Types` are those of all its files, a partial type once; `Members` and `Nodes<T>()` are all its files'.
- `AllTypes` (on a file too): its types and those nested in them, however deep. `Types` are the top-level ones only, so a rule on every public type reads `AllTypes`.
- `Folders`: a `DependencyGraph` of the folders directly in its own, each with the folders whose types its files use. A file at the project's root is in no folder. Assert with `Folders.Is().Within(...)`.

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

## Member

- `Accessibility` is in effect: no more than the types around it allow. `Type` and `Parameters` are syntax, as written. A record's positional parameters are its constructor's and its properties.
- `IsOriginal`: it neither implements an interface's member (explicitly or implicitly) nor overrides a base type's, so it sets its own name and signature; a naming rule reads only original members.
- `Gives` is the types it returns, including those inside generic types, tuples and arrays; `Takes` is the same for its parameters. With no argument they list the types (`ITypeSymbol`). Ask if one is included with one argument that is a `TypeKind`, a `typeof(List<>)` (type arguments ignored) or a condition. A type it only implements doesn't count: `List<int>` gives no `IEnumerable<>`.
- `Size`: its size, computed from line count, indentation and complexity. A rule sets only the ceiling: `Has().None(member => member.Size > 240)`.
- A failure names it `X/Path:line Name`.
