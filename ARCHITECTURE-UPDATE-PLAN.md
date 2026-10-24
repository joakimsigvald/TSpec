# Architecture update plan

New capabilities for TSpec's architecture rules and the package TSpec.Architecture.CSharp, measured
against M5's `Architecture.Spec`. Names in the examples are proposals.

| # | Capability | Value | Effort | Worth |
|---|---|---|---|---|
| 1 | Done but for: the package on TSpec 5.4.0 once released | 1 | 1 | 1.0 |
| 9 | Code lines apart from embedded text | 1 | 2 | 0.5 |
| 14 | Where a member starts past its attributes | 1 | 1 | 1.0 |

Value and Effort run 1–4. Value 1 serves one rule as a convenience; 4 is a capability the audience
expects of an architecture tool. Effort 1 is minutes with no decision; 4 needs a design session and a
breakdown. Worth = Value / Effort: at 1 or above, do it; below 1, it can wait.

## 1. Done but for: the package on TSpec 5.4.0 once released

`IsSpecProject` and `OpensInternalsTo` are in TSpec 5.4.0, and the package's `OpensInternalsTo` is
`[Obsolete]`. Left: once TSpec 5.4.0 is on NuGet, revert the TEMPORARY local feed in
`Architecture.CSharp.csproj` and `Architecture.CSharp.Test.csproj`, and reference TSpec `5.4.0` in place
of `5.4.0-local.3`.

## 9. Code lines apart from embedded text

The lines of a file that are code, without the content of a multi-line raw string literal: an embedded
query or document keeps its own width and indentation. Every rule on lines — width, nesting depth,
indentation, a method's size — now finds those lines itself.

- *Serves:* LineWidths, NestingDepth, Indentation, MethodSize.
- *Open:* it serves layout rules only, which the package has so far left to a short Roslyn rule of
  one's own; a line's kind, or a set of line numbers.

## 14. Where a member starts past its attributes

The line a member's head is on, past the attribute lists above it, for a rule on its length or layout.

- *Serves:* MethodSize, MemberBodies, Indentation.
