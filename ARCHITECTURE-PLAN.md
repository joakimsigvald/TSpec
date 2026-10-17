# Architecture plan

What lies between projects: the project graph, read from the spec project's build with no new dependency,
and the document an architecture spec writes. What lies inside a project is the package
TSpec.Architecture.CSharp's. Names in the examples are proposals; each step's *To decide* lists what is
open.

## Steps, in order

### 7. The rules on the architecture page

Since 5.1, `ArchitectureDocument` writes `_specification.md`: the solution's name and its built graph
as a diagram. The page does not yet say what the architecture allows.

MyHotel's page would go on, below the diagram:

````markdown
## Project dependencies

- **are only these and not redundant**

| Project | May reference |
|---|---|
| MyHotel | Entry, Infra, P:Scalar.AspNetCore, P:Microsoft.* |
| Contract | P:Microsoft.* |
| Core | Contract, P:Microsoft.* |
| Core.Spec | Core, P:* |
| Entry | Contract, P:Microsoft.* |
| Infra | Core, P:Microsoft.* |
| MyHotel.Spec | MyHotel, P:* |
````

- *Requirement:* the page states the architecture rules in words, and what each project may reference.
- *Motivation:* the diagram shows what the build is, not what the architecture allows; a reviewer still
  sees a rule change only as a diff of a switch expression.
- *Solution:* the test names state the rules in words, as in the other documents; for each `Within`, a
  table of what its rules allow each project — a rule is a function of the project, so it can be
  evaluated for every one. M5's page would show the sub-domains of `Application`.
- *Design:* the architecture tests are `Spec` classes, so the collector already holds their requirements
  when the page is written. `Within` records its evaluated rules in the run, as a `Spec` records its
  requirements, and the renderer adds them under the diagram in ordinal order. Needs a design session.
- *To decide:* one page or one per test class; names in the table as the rules see them (`.`, `Spec`) or
  as the diagram shows them; how a custom rule, a plain `Is().Empty()` over computed findings, reaches the
  page; whether `Redundant` does.

### 8. What a project reaches

Ask what a project depends on, directly or indirectly — the CDR needs it, M5 does not.

```csharp
[Fact]
public void Entry_never_reaches_Infra()
    => Source.Projects.Under("MyHotel")["Entry"].Reaches.Does().not.Contain("Infra");
```

- *Requirement:* a custom rule can ask what a project depends on directly or through other projects, named
  as the rules see them.
- *Motivation:* a project's `Dependencies` are its direct references only, and where `not.Redundant()` holds
  they omit everything reached through another project, so whether a host ships a project is not visible
  from them.
  TSpec computes the closure for `Redundant()` but keeps it private, so the CDR copies the walk
  (`Solution.Reached`) for three rules: each host's solution filter lists exactly its closure and the specs
  that reference into it; a store ships only into hosts that compose its module; no domain leaf could
  dissolve into a module.
- *Solution:* `Reaches` on `SourceProject`, with `Dependencies`' conventions: names as written under
  `Under(root)`, in ordinal order. Packages are included as the leaves they are in the graph, since a
  package's own dependencies are not in it; the project itself is not.
- *Design:* the graph hands each project its reach from the walk `Redundant()` uses, which works on full
  names, mapped out with `AsWritten` as `Dependencies` are. Tests in `Core.Test/Architecture/WhenReaches.cs`,
  on graphs built from a dictionary: a chain, a diamond reached once, the packages of reached projects, names
  under a root, a project with no references. One clause in README §7's custom-rule paragraph and in the
  agent reference's "Custom rules" line. A NEW note. Check: the CDR's three rules give today's findings on
  `Reaches`.
- *To decide:* the name.

## How to work a step

- Propose before code: the failing example, the behaviour, any new wording, and the docs line if a reader
  would otherwise get it wrong; for a rendering change, before and after.
- Test first. The suite green on all three frameworks, MyHotel green. Report and stop.
- Delete a step when it lands, and the plan with its last step.
