# TSpec.Architecture.CSharp plan

New capabilities for the package, measured against M5's `Architecture.Spec`. Names in the examples are
proposals.

| # | Capability | Value | Effort | Worth |
|---|---|---|---|---|
| 7 | A graph of sub-domains | 3 | 3 | 1.0 |

Value and Effort run 1–4. Value 1 serves one rule as a convenience; 4 is a capability the audience
expects of an architecture tool. Effort 1 is minutes with no decision; 4 needs a design session and a
breakdown. Worth = Value / Effort: at 1 or above, do it; below 1, it can wait.

Not planned, as zero value: built-in layout rules (Arrows, MemberBodies, LambdaBodies, ExpressionLines,
ConditionalBranches, ArgumentLists, ListEnds, Indentation, Characters) are code style, and
EnumerableMembers' "read exactly once" is a body's data flow; with `Root` and `Members` each stays a
short Roslyn rule in M5.

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

- *Serves:* SubdomainDependencies, both facts in effect: a folder no rule places may use nothing. Built
  on `Uses`, and `Within` over a graph that is not the project graph: TSpec public API, in
  `TSpec.Extensibility`, released first.
- *To decide:* folders or namespaces as the unit.
