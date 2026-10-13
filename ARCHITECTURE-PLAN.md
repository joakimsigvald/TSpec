# Architecture plan

What M5 and the CDR write by hand in their Architecture.Spec projects, made TSpec's, and the document an
architecture spec writes. Everything is read from the spec project's build — its deps.json, and the
assemblies and PDBs beside it — with no new dependency. Names in the examples are proposals; each step's
*To decide* lists what is open.

## Steps, in order

### 1. A project's source files

Each project's source files from TSpec, replacing M5's `Sources` helper; with `Are`, its allow-lists become
one line.

```csharp
private static readonly (string, int)[] Allowed =
[
    ("Application/Assistant/Assistant.cs", 7),
    ("Core/Reports/ReportRunner.cs", 7),
];

// M5's NestingDepth: exactly the listed files are too deep, each as deep as listed
[Fact]
public void Only_the_listed_files_are_indented_past_the_fifth_level()
    => Project.Dependencies.Projects.SelectMany(Sources.Of)
        .Select(source => (source.At, Depth: Deepest(source)))
        .Where(file => file.Depth > Limit)
        .Are(Allowed);

// M5's Sources.Of, without walking up to MedraveM5.slnx or assuming a folder per assembly name
public static IReadOnlyList<Source> Of(string project)
    => [.. Project.Dependencies.FilesOf(project).Sources.Select(Source.Read)];
```

- *Requirement:* a custom rule can ask, for every project in the graph and for the running spec project,
  its folder, its project file and its source files.
- *Motivation:* every M5 rule but the project graph reads its files through M5's own `Sources`, which walks
  up to `MedraveM5.slnx` and takes a project's folder to be named after its assembly; the CDR has its own
  convention (`Medrave.Cdr.Capabilities.Auth` is `Capabilities/Auth/Auth.csproj`).
- *Solution:* the files from the PDB beside each project's assembly in the spec project's output. It lists
  every file compiled into the project, interface-only ones too (checked on MyHotel's and M5's builds), and
  it is how TSpec already finds the spec project's own folder. The folder is the one most files sit under,
  walked up to its project file; generated files under `obj/` are left out, a linked file is kept.
- *Design:* an accessor on `ProjectGraph`, the project named as the rules see it. Open the PDB as
  `SourceLocations` does, from the file rather than a loaded module, and reuse `ProjectDirectory`'s majority
  vote without its fallback to the build output, which for a referenced project is the spec project's; read
  once per project. Tests read the running build, where TSpec is the one project, its sources including
  `Architecture/ProjectGraph.cs`. One clause in README §7 and the agent reference; a NEW note.
- *To decide:* the accessor's name and shape; whether a source path is relative to the project's folder or
  full; how the running spec project is named, since `Projects` leaves it out; the refusal when a project
  has no PDB, or a CI build mapped its paths away (`ContinuousIntegrationBuild` writes `/_/…`), saying what
  to set.

### 2. The types each project declares, and their files

Each project's classes, records, interfaces and enums, and the file each one is in.

```csharp
// M5's Partials, without Roslyn
[Theory, MemberData(nameof(Projects))]
public void A_partial_type_is_declared_in_one_file(string project)
    => Project.Dependencies.TypesOf(project)
        .Where(type => type.Files.Count > 1)
        .Select(type => $"{type.FullName} in {type.Files.Count} files")
        .Is().Empty();
```

- *Requirement:* a custom rule can ask which types a project declares, each with its namespace, its kind
  (class, record, struct, interface, enum, delegate), whether it is public, and the files it is written in.
- *Motivation:* M5's `TypesPerFile` (one public type per file, named after it) and `Partials` (a partial
  type in one file) parse every file with Roslyn to find the declarations, and its `PublicTypes` starts from
  them; the CDR finds its public interfaces with a regex. The compiled assembly lists them exactly.
- *Solution:* the type definitions of each project's assembly, read with `System.Reflection.Metadata`,
  which TSpec already uses for PDBs, without loading it. A type's files come from the PDB: its methods'
  sequence points, and the PDB's type documents for a part without methods.
- *Design:* sharing step 1's PDB reading. The kind from the type's flags and base type, a record by its
  `<Clone>$` method; public means public with every containing type public; compiler-generated types left
  out. Tests read TSpec's own types from the running build. A NEW note.
- *To decide:* the names and what a type carries; how a generic type is named (`Repository<T>` or
  ``Repository`1``).

### 3. Every use of a type, and the file that writes it

Which types each type uses, and in which file — what steps 4–6 are built on.

```csharp
// One edge of M5's sub-domain rule: Workbench never uses Assistant
[Fact]
public void Workbench_does_not_use_Assistant()
    => Project.Dependencies.UsesOf("Application")
        .Where(use => use.From.StartsWith("Medrave.M5.Application.Workbench.", StringComparison.Ordinal))
        .Where(use => use.Type.StartsWith("Medrave.M5.Application.Assistant.", StringComparison.Ordinal))
        .Select(use => $"{use.File}: {use.From} uses {use.Type}")
        .Is().Empty();
```

- *Requirement:* a custom rule can ask which types each type uses, in its own project and in others, and in
  which file each use is written.
- *Motivation:* M5's `SubdomainDependencies` keeps the folders of `Application` one way — Chat at the
  bottom, Generation and Assistant on Chat, Workbench on Generation and Chat — by matching the first segment
  of every qualified name with Roslyn, in 75 lines; its `PublicTypes` matches identifiers across projects in
  155, so any identifier spelled like a type counts as a use of it. The CDR runs regexes over usings and
  tokens. The project graph cannot answer it, since `not.Redundant()` removes the edges a project still
  uses through another.
- *Solution:* each type's uses read from its signatures, its attributes and its methods' IL, and placed in a
  file through the PDB's sequence points. Compiler-generated types — lambdas' closures, async and iterator
  state machines — count as the type that wrote them; a used type in another project resolves to it by
  assembly name.
- *Design:* an IL reader over each method body that collects the type, method and field tokens of its
  instructions and its local-variable signature, mapping a method or field to its declaring type; and a
  signature decoder (`System.Reflection.Metadata`'s type provider) for signatures and generic arguments,
  which steps 5 and 6 reuse. A use is kept once per file. Check: M5's `SubdomainDependencies` computed on it
  gives today's findings. M5 builds only with the CDR checkout on the branch it expects. The reader is a
  second reading mechanism of some 150 lines: propose its size before writing it. Blind spots: `nameof`
  and a `const` read compile to literals, and a `typeof` in an attribute is a name in its blob.
- *To decide:* how the uses are exposed — on step 2's types, or as rows of type, used type and file;
  whether uses of package and framework types are kept too, for rules such as "Core uses nothing from
  `System.Data`"; whether to read `typeof` out of attribute blobs; where M5's ports run — a local branch of
  M5 pointing at TSpec's working build, as MyHotel's `lib/` does, or a scratch copy.

### 4. Namespace dependencies

Rules for which namespaces may use which — M5's sub-domain rule (Chat, Generation, Assistant, Workbench) as
one switch.

```csharp
// M5's SubdomainDependencies; the second rule lets each sub-domain use itself
[Fact]
public void A_subdomain_uses_only_the_ones_below_it()
    => Project.Dependencies.NamespacesOf("Application").Under("Medrave.M5.Application")
        .Is().Within(ns => ns.Segment(0) switch
        {
            "Generation" or "Assistant" => ["Chat*"],
            "Workbench" => ["Generation*", "Chat*"],
            _ => [],
        }, ns => [$"{ns.Segment(0)}*"]);
```

- *Requirement:* assert which namespaces may use which, with the same `Within` as for projects.
- *Motivation:* the most common architecture rule (NetArchTest and ArchUnitNET are built around it), and
  M5's newest rule is one: 75 lines of syntax matching, where M5's project graph is one switch. M5's
  CODING-STANDARD makes every folder a sub-domain (9a), so the same rule fits its other projects.
- *Solution:* a graph whose nodes are namespaces and whose edges are step 3's uses — namespace A uses
  namespace B when a type in A uses a type in B — with the project graph's mechanism: `Under`, `Segment`,
  `Within`, the trailing `*`, and a failure that names each offending edge.
- *Design:* build the edges from step 3's uses, and share `ProjectGraph`'s name handling and `Within` rather
  than copy them: either `ProjectGraph` takes namespace nodes, or the shared part moves to a base both
  graphs use. Tests mirror `WhenWithin` and `WhenUnder` with namespace names. Check: M5's
  `SubdomainDependencies` as a switch gives today's findings. A NEW note.
- *To decide:* the scope — one project's namespaces or all; whether the namespaces under a sub-domain fold
  into it, so `Workbench.Edits` counts as `Workbench`; whether edges to namespaces outside the root are
  kept; and whether `Within` can require every node to be placed, as M5's "every subdomain is placed in the
  graph" does, which a rule given as a function cannot show.

### 5. A public type no other project uses

A ready-made rule that such a type should be internal — M5's `PublicTypes`.

```csharp
// M5's PublicTypes; Contract is public by purpose
[Fact]
public void No_type_is_public_that_only_its_own_project_uses()
    => Project.Dependencies.Is().not.NeedlesslyPublic(except: ["Contract"]);
```

- *Requirement:* one assertion that finds each public type no other project uses.
- *Motivation:* M5 holds every production project to it (CODING-STANDARD 3b: anything that can be internal
  is) in 155 lines of identifier matching; the CDR asks a narrower form of its contract interfaces.
  Exposing no more than is used is common advice for an application's projects.
- *Solution:* an assertion on the graph over step 3's uses, with M5's exceptions: a type named in a public
  signature of its own project must stay public, and a use from the project's own spec does not count,
  since `InternalsVisibleTo` opens internals to it — read from the assembly's attributes, where M5 reads the
  csproj's text.
- *Design:* step 3's signature decoder over the public members of public types (base types, interfaces,
  member types, parameters, constraints). Port M5's `PublicTypes` first and diff its findings with today's,
  then build it in. A finding names the type, e.g. `Core.Orders.OrderMapper is public but no other project
  uses it`; `Fix` says why. A NEW note.
- *To decide:* the assertion's name; how a project whose public types are its purpose, such as M5's
  Contract or a library, is left out.

### 6. What exposed members take and return

Which types each public member takes and returns, for rules like M5's "no tuple in a public member".

```csharp
// M5's TupleMembers, without Roslyn
[Theory, MemberData(nameof(Projects))]
public void No_member_takes_or_returns_a_tuple(string project)
    => Project.Dependencies.TypesOf(project)
        .SelectMany(type => type.Members)
        .Where(member => !member.IsPrivate)
        .Where(member => member.Signature.Any(type => type.Contains("System.ValueTuple")))
        .Select(member => member.Name)
        .Is().Empty();
```

- *Requirement:* a custom rule can ask, for each type, its members — methods, constructors, properties,
  fields, events — each with its visibility and the types its signature takes and returns.
- *Motivation:* M5 checks with Roslyn that no public or internal member takes or returns a tuple
  (`TupleMembers`) or returns an `IEnumerable<T>` (`EnumerableMembers`); its boundary rule — nothing crosses
  as an enum, a custom value type, a time type other than `DateOnly`, `object` or `dynamic`
  (CODING-STANDARD 6b) — is unchecked. Rules on what an API exposes, such as "the contract carries records
  and primitives only", are common in architecture tools.
- *Solution:* each member of step 2's types with its visibility and its signature, decoded from metadata,
  which holds the resolved types: an alias or a `global using` in the source cannot hide what a member
  takes.
- *Design:* step 3's signature decoder over every member. A property stands for its accessors,
  compiler-generated members are left out, and a record's positional parameters are its properties. A tuple
  is a `ValueTuple`; `dynamic` is `object` marked by a `DynamicAttribute`, which the decoder reads. Port
  M5's two rules and write 6b on it, and diff. A NEW note.
- *To decide:* how a signature's types are exposed — names such as `System.ValueTuple<int, string>`, or a
  small type model; whether nullable annotations are read.

### 7. The architecture document

An architecture spec writes a readable page: its rules, and a diagram of the project graph.

```csharp
[assembly: AssemblyFixture(typeof(ArchitectureDocument))]
```

MyHotel's page would begin:

````markdown
# MyHotel architecture

## Project dependencies

- **are only these and not redundant**

| Project | May reference |
|---|---|
| . | Entry, Infra, P:Scalar.AspNetCore, P:Microsoft.* |
| Contract | P:Microsoft.* |
| Core | Contract, P:Microsoft.* |
| Core.Spec | Core, P:* |
| Entry | Contract, P:Microsoft.* |
| Infra | Core, P:Microsoft.* |
| Spec | ., P:* |

```mermaid
flowchart TD
    Root["."] --> Entry & Infra
    Entry --> Contract
    Infra --> Core --> Contract
    Spec --> Root
    CoreSpec["Core.Spec"] --> Core
```
````

- *Requirement:* an architecture spec writes a document of its own into `_specification/` on every green
  run: its rules in words, and the graph they hold over as a diagram.
- *Motivation:* architecture rules are the claims a human owns, and a claim that reaches no document is
  second-class next to the examples. Today a reviewer sees an architecture change only as a diff of a
  switch expression. An architecture spec writes nothing: the document's subject is derived from the spec
  project's name, `Architecture` for M5's, which no project is, so M5 leaves `_specification/` out; and the
  document is built only from `Spec` classes, while architecture tests are plain classes.
- *Solution:* a page derived from what the architecture tests asserted: the built graph as a Mermaid
  diagram, and for each `Within`, what its rules allow each project — a rule is a function of the project,
  so it can be evaluated for every one. The test names state the rules in words, as in the other
  documents. M5's page would show its projects and the sub-domains of `Application`.
- *Design:* `Within` and `Redundant` record the graph, its root and the evaluated rules in the run, as a
  `Spec` records its requirements; a renderer beside `DocumentRenderer` writes the page with nodes and
  edges in ordinal order, so it diffs line by line; written only on a full green run, as now. Needs a
  design session and its own breakdown before code.
- *To decide:* how an architecture spec opts in without a subject; one page or one per test class; the
  built graph, the allowed graph or both; whether packages appear in the diagram; how a custom rule, a
  plain `Is().Empty()` over computed findings, reaches the page.

### 8. What a project reaches

Ask what a project depends on, directly or indirectly — the CDR needs it, M5 does not.

```csharp
[Fact]
public void Entry_never_reaches_Infra()
    => Project.Dependencies.Under("MyHotel").ReachedFrom("Entry").Does().not.Contain("Infra");
```

- *Requirement:* a custom rule can ask what a project depends on directly or through other projects, named
  as the rules see them.
- *Motivation:* `graph["X"]` gives direct references only, and where `not.Redundant()` holds they omit
  everything reached through another project, so whether a host ships a project is not visible from them.
  TSpec computes the closure for `Redundant()` but keeps it private, so the CDR copies the walk
  (`Solution.Reached`) for three rules: each host's solution filter lists exactly its closure and the specs
  that reference into it; a store ships only into hosts that compose its module; no domain leaf could
  dissolve into a module.
- *Solution:* a public `ReachedFrom(project)` on `ProjectGraph` with the indexer's conventions: names as
  written under `Under(root)`, in ordinal order. Packages are included as the leaves they are in the graph,
  since a package's own dependencies are not in it; the project itself is not.
- *Design:* rename the private walk `Redundant()` uses, which works on full names, to free the name. The
  public method maps the name in with `FullName`, walks, and maps out with `AsWritten`, as the indexer
  does. Tests in `Core.Test/Architecture/WhenReachedFrom.cs`, built with the `ProjectGraph` initializer: a
  chain, a diamond reached once, the packages of reached projects, names under a root, a project with no
  references. One clause in README §7's custom-rule paragraph and in the agent reference's "Custom rules"
  line. A new API, so a minor version with a NEW note. Check: the CDR's three rules give today's findings
  on `ReachedFrom`.
- *To decide:* the name; and whether an unknown project reaches nothing, as `graph["X"]` gives nothing
  today, or both refuse it, since a misspelt host makes a rule pass on nothing.

### 9. Port the CDR's usage rules and diff

Rewrite the CDR's hand-made usage rules on steps 3 and 8, to check they work there too.

```csharp
// The CDR's "every contract interface is consumed by an entry or host", its exceptions left out
private static readonly HashSet<string> UsedByEntryOrHost =
    [.. Projects.Where(project => project.IsEntry() || project.IsHost())
        .SelectMany(project => Graph.UsesOf(project))
        .Select(use => use.Type)];

[Fact]
public void Every_contract_interface_is_used_by_an_entry_or_host()
    => Projects.Where(project => project.IsContract())
        .SelectMany(project => Graph.TypesOf(project))
        .Where(type => type is { Kind: TypeKind.Interface, IsPublic: true })
        .Where(type => !UsedByEntryOrHost.Contains(type.FullName))
        .Select(type => type.FullName)
        .Is().Empty();
```

- *Requirement:* evidence that steps 3 and 8 serve the CDR's rules as they serve M5's.
- *Motivation:* the CDR's rules have shapes M5's lack — contract interfaces consumed by an entry or host,
  stores shipped only with their module, domain leaves that could dissolve, and an entry file speaking to
  at most one contract namespace — all matched from source text, comments included, "so the tolerance is
  toward passing". A second consumer is what shows a reading is general.
- *Solution:* rewrite the four rules on step 3's uses and step 8's reach, run them against the CDR's build,
  and compare not only pass and fail but the sets each computes on the way: the interfaces an entry or host
  uses, the hosts that ship each store, the modules that use each leaf, the contract namespaces each entry
  file uses.
- *Design:* old and new side by side, one scratch test per rule printing both sets and their differences,
  each classed as a reader bug, an old-rule error or a blind spot. The outcome is a report; nothing in
  TSpec changes unless it shows a gap.
- *To decide:* where the port runs, as for M5's in step 3.

## How to work a step

- Propose before code: the failing example, the behaviour, any new wording, and the docs line if a reader
  would otherwise get it wrong; for a rendering change, before and after.
- Test first. The suite green on all three frameworks, MyHotel green. Report and stop.
- Delete a step when it lands, and the plan with its last step.
