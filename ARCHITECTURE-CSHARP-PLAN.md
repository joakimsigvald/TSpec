# TSpec.Architecture.CSharp plan

New capabilities for the package, measured against M5's `Architecture.Spec`: 21 rule classes, 29 facts.
Names in the examples are proposals.

| # | Capability | Value | Effort | Worth | After |
|---|---|---|---|---|---|
| 1 | A project's types | 2 | 1 | 2.0 | |
| 2 | The syntax tree as the build parsed it | 3 | 2 | 1.5 | |
| 3 | Needlessly public types | 3 | 2 | 1.5 | 4, 5 |
| 4 | Declared members | 4 | 3 | 1.33 | |
| 5 | Type uses | 4 | 4 | 1.0 | |
| 6 | Embedded lines | 1 | 1 | 1.0 | 2 |
| 7 | A graph of sub-domains | 3 | 3 | 1.0 | 5 |
| 8 | Nested types | 1 | 2 | 0.5 | |

Value and Effort run 1–4. Value 1 serves one rule as a convenience; 4 is a capability the audience
expects of an architecture tool. Effort 1 is minutes with no decision; 4 needs a design session and a
breakdown. Effort counts the item alone: what it needs first is in its own row, under After.
Worth = Value / Effort: at 1 or above, do it; below 1, it can wait.

Not planned, as zero value: built-in layout rules (Arrows, MemberBodies, LambdaBodies, ExpressionLines,
ConditionalBranches, ArgumentLists, ListEnds, Indentation, Characters) are code style, and
EnumerableMembers' "read exactly once" is a body's data flow; with 2 each stays a short Roslyn rule in
M5. FileLengths, Partials, ProjectDependencies and TypesPerFile's first two facts are served today.

## 1. A project's types

```csharp
public void A_partial_type_is_declared_in_one_file(string project)
    => Source.Projects[project].Types.Has().None(type => type.Files.Count > 1);
```

- *Serves:* Partials, and the project-level reads of 3, 5 and 7. Today it is
  `Files.SelectMany(file => file.Types).Distinct()`, and without the `Distinct` a partial type counts
  once per file.
- *To decide:* whether a failure still names the files, as Partials' "X in a, b" does.

## 2. The syntax tree as the build parsed it

```csharp
file.Syntax        // Roslyn's SyntaxTree: the one Types is read from
file.Line(node)    // counted from 1, as Lines is
```

- *Serves:* 18 of the 21 rule classes; replaces M5's `SourceFiles.cs`. The package already parses each
  file with the project's language version and defines from the debug information, then drops the tree;
  M5 parses it again with Roslyn's defaults, so a rule over nodes skips code under `#if DEBUG`.
- *To decide:* Roslyn types in the package's public API, which puts Roslyn's version under the package's
  stability promise; which positions beside `Line` (M5 uses `End`, `Column`, `LinesOf`).

## 3. Needlessly public types

```csharp
public void A_public_type_is_needed_outside_its_project(string project)
    => Source.Projects[project].Types.Where(type => type.IsPublic).Has().All(type => type.NeededOutside);
```

A public type is needed when another project names it, or a signature another assembly sees names it.

- *Serves:* PublicTypes, M5's longest rule at 167 lines. Needs who names a type (5), the types in
  visible signatures (4), and the assemblies a project opens its internals to, which M5 reads by loading
  the assembly.
- *To decide:* whether the spec a project opens its internals to counts as outside (M5: no).

## 4. Declared members

```csharp
public void A_method_takes_at_most_four_arguments(string project)
    => Source.Projects[project].Methods.Has().None(method => method.Parameters.Count > 4);
```

Each with its name, kind, visibility in effect (an interface's member is public, a public member of an
internal type is not), modifiers, attributes, parameters and their types, return type, and where it is:
a failure names `Core/Rooms/RoomService.cs:12 Book(5)`.

- *Serves:* Arities (both facts), AsyncSuffixes, TupleMembers, EnumerableMembers' first fact, and the
  signatures of 3; replaces M5's `Members.cs` and `Member.cs`.
- *To decide:* what counts — a record's positional parameters and a primary constructor (M5 counts them
  as constructors), a local function (Arities and AsyncSuffixes include them); a type as written, enough
  for a tuple or `IEnumerable<T>`, or resolved, which waits on 5's decision.

## 5. Type uses

```csharp
public void Chat_uses_nothing_of_Generation()
    => Source.Projects["Application"].Files.Where(file => file.Path.StartsWith("Chat/"))
        .Has().None(file => file.Uses.Any(type => type.Namespace.EndsWith(".Generation")));
```

`file.Uses`: the types of the build a file names.

- *Serves:* SubdomainDependencies (what a file names) and 3 (who names a type). The CDR's ApiLayerSpec
  counts a front file's contract namespaces the same way, by regex.
- *To decide:* how uses are read — the syntax as written, as M5 does (a name match: wrong for two types
  of one name, blind to `var` and extension calls); a semantic model (exact, but compiles the solution
  against every reference); or the built assemblies' metadata, as NetArchTest does (exact and fast,
  blind to `nameof`, mapped to files through the debug information).

## 6. Embedded lines

```csharp
file.EmbeddedLines   // the lines inside a multi-line raw string literal
```

- *Serves:* LineWidths, NestingDepth (both facts), Indentation; replaces M5's `EmbeddedLines.cs`. An
  embedded document or query keeps its own shape, so a rule over lines leaves it out.
- *To decide:* line numbers beside `Lines`, or lines that know whether they are code; whether a block
  comment's lines count (Indentation skips them, LineWidths does not).

## 7. A graph of sub-domains

```csharp
public void A_subdomain_uses_only_what_the_graph_draws_below_it()
    => Source.Projects["Application"].Folders.Is().Within(folder => folder switch
    {
        "Generation" or "Assistant" => ["Chat"],
        "Workbench" => ["Generation", "Chat"],
        _ => [],
    });
```

The architecture page draws it as it draws the project graph, with the table of what each may use.

- *Serves:* SubdomainDependencies, both facts in effect: a folder no rule places may use nothing.
  Needs 5, and `Within` over a graph that is not the project graph: TSpec public API, in
  `TSpec.Extensibility`, released first.
- *To decide:* folders or namespaces as the unit.

## 8. Nested types

`type.Nested`: the types declared inside a type, each with whether it is public in effect.

- *Serves:* TypesPerFile's third fact. CA1034 already reports a visible nested type and is set per
  project in `.editorconfig`; M5's exception for a record would need a suppression.
