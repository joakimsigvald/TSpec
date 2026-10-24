# Architecture update plan

New capabilities for TSpec's architecture rules and the package TSpec.Architecture.CSharp, measured
against M5's `Architecture.Spec`. Names in the examples are proposals.

| # | Capability | Value | Effort | Worth |
|---|---|---|---|---|
| 1 | Done but for: the package on TSpec 5.4.0 once released | 1 | 1 | 1.0 |
| 7 | A member's body | 1 | 1 | 1.0 |
| 8 | A capitalised word in a snake_case name | 2 | 2 | 1.0 |
| 9 | Code lines apart from embedded text | 1 | 2 | 0.5 |
| 11 | A file's folder | 1 | 1 | 1.0 |
| 14 | Where a member starts past its attributes | 1 | 1 | 1.0 |

Value and Effort run 1–4. Value 1 serves one rule as a convenience; 4 is a capability the audience
expects of an architecture tool. Effort 1 is minutes with no decision; 4 needs a design session and a
breakdown. Worth = Value / Effort: at 1 or above, do it; below 1, it can wait.

## 1. Done but for: the package on TSpec 5.4.0 once released

`IsSpecProject` and `OpensInternalsTo` are in TSpec 5.4.0, and the package's `OpensInternalsTo` is
`[Obsolete]`. Left: once TSpec 5.4.0 is on NuGet, revert the TEMPORARY local feed in
`Architecture.CSharp.csproj` and `Architecture.CSharp.Test.csproj`, and reference TSpec `5.4.0` in place
of `5.4.0-local.3`.

## 7. A member's body

On `DeclaredMember`, its `Body`: a block or an arrow's expression, none for an abstract member or an
auto-property. A rule on what a body does now finds it by the member's kind.

- *Serves:* EnumerableMembers, MethodSize.

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

## 11. A file's folder

```csharp
Source.Projects["Core"].Files.Where(file => file.Folder == "Specs")
```

The folder a file is in, as the project's `Folders` names it: the one directly in the project's own. A
rule now cuts it from the file's `Path`, as in `file.Path.StartsWith("Specs/")`.

- *Serves:* SubdomainDependencies' "Specs uses no Contract type"; FolderCycles cut it too, before
  `Acyclic`.
- *Open:* what a file at the project's root, in no folder, has for it.

## 14. Where a member starts past its attributes

The line a member's head is on, past the attribute lists above it, for a rule on its length or layout.

- *Serves:* MethodSize, MemberBodies, Indentation.
