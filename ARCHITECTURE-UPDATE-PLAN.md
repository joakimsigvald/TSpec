# Architecture update plan

New capabilities for TSpec's architecture rules and the package TSpec.Architecture.CSharp, measured
against M5's `Architecture.Spec`. Names in the examples are proposals.

| # | Capability | Value | Effort | Worth |
|---|---|---|---|---|
| 1 | Done but for: the package on TSpec 5.4.0 once released | 1 | 1 | 1.0 |
| 6 | Who uses a type | 2 | 2 | 1.0 |
| 7 | A member's body, head and inherited name | 2 | 2 | 1.0 |
| 8 | A capitalised word in a snake_case name | 2 | 2 | 1.0 |
| 9 | Code lines apart from embedded text | 1 | 2 | 0.5 |
| 10 | A file's semantic model | 2 | 1 | 2.0 |
| 11 | A file's folder | 1 | 1 | 1.0 |

Value and Effort run 1–4. Value 1 serves one rule as a convenience; 4 is a capability the audience
expects of an architecture tool. Effort 1 is minutes with no decision; 4 needs a design session and a
breakdown. Worth = Value / Effort: at 1 or above, do it; below 1, it can wait.

## 1. Done but for: the package on TSpec 5.4.0 once released

`IsSpecProject` and `OpensInternalsTo` are in TSpec 5.4.0, and the package's `OpensInternalsTo` is
`[Obsolete]`. Left: once TSpec 5.4.0 is on NuGet, revert the TEMPORARY local feed in
`Architecture.CSharp.csproj` and `Architecture.CSharp.Test.csproj`, and reference TSpec `5.4.0` in place
of `5.4.0-local.3`.

## 6. Who uses a type

```csharp
Source.Projects["Core"].Types.Where(type => type.IsPublic)
    .Has().All(type => type.UsedBy.Any(file => file.Project != type.Project));
```

`DeclaredType.Project`, and `UsedBy`, the files whose code uses it: `Uses` the other way round. A rule
that a type is public only when another project needs it now scans every file of every other project
for each type. With it, a project's types together with their nested ones, which a rule on every public
type needs and now builds by recursing on `Nested`.

- *Serves:* PublicTypes, TypesPerFile; SubdomainDependencies reads a type's project from its files.
- *Open:* `ExposedBy` too, the types whose signatures name it; the name for the types with their nested
  ones.

## 7. A member's body, head and inherited name

On `DeclaredMember`: its `Body`, a block or an arrow's expression, none for an abstract member or an
auto-property; whether its name and signature are a base type's or an interface's — an override, an
explicit interface implementation — which a naming rule (no `Async` suffix, no `Get` prefix) exempts;
and where it starts past its attributes, for a rule on its length or layout.

- *Serves:* EnumerableMembers and MethodSize (body); AsyncSuffixes, GetPrefixes, MutableCollections and
  MemberOrder (inherited); MethodSize, MemberBodies and Indentation (head).
- *Open:* an implicit interface implementation counts as inherited only with the compiler's view, as
  `Gives` has it;
  all three, or the one with the most use first.

## 8. A capitalised word in a snake_case name

`No_method_carries_the_Async_suffix` reads "No method carries the, async suffix", and
`In_Core_Specs_uses_no_Contract_type` reads "In, core, specs uses no, contract type": an underscore
before a capital starts a clause (`Exists_ButIsAlreadyBooked` reads "exists, but is already booked"), so
a snake_case name cannot hold a proper noun or an acronym. Test-name rendering in Core
(`StringExtensions.Clauses`), not architecture.

- *Open:* how to tell a snake_case name from one in clauses; whether it moves to FOLLOW-UP-PLAN's D.

## 9. Code lines apart from embedded text

The lines of a file that are code, without the content of a multi-line raw string literal: an embedded
query or document keeps its own width and indentation. Every rule on lines — width, nesting depth,
indentation, a method's size — now finds those lines itself.

- *Serves:* LineWidths, NestingDepth, Indentation, MethodSize.
- *Open:* it serves layout rules only, which the package has so far left to a short Roslyn rule of
  one's own; a line's kind, or a set of line numbers.

## 10. A file's semantic model

```csharp
file.Model.GetSymbolInfo(argument.Expression).Symbol
```

A file's Roslyn `SemanticModel` beside its `Root`, from the compilation `Uses` and `Exposes` already
build, for what a signature does not show: code in a method's body and attributes. Without it a rule
matches names in the text — an identifier spelled `StringComparer`, `Dapper` or `Theory` — and a type
of one's own with the same name fools it.

- *Serves:* DefaultComparers, DapperCalls, Arities' `Theory`, Suppressions.
- *Open:* whether to give it at all, or leave these rules to names; the compilation is against the run's
  assemblies, so a type the spec project does not run with, or one a source generator wrote in memory,
  stays unresolved.

## 11. A file's folder

```csharp
Source.Projects["Core"].Files.Where(file => file.Folder == "Specs")
```

The folder a file is in, as the project's `Folders` names it: the one directly in the project's own. A
rule now cuts it from the file's `Path`, as in `file.Path.StartsWith("Specs/")`.

- *Serves:* SubdomainDependencies' "Specs uses no Contract type"; FolderCycles cut it too, before
  `Acyclic`.
- *Open:* what a file at the project's root, in no folder, has for it.
