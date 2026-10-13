# Architecture plan

What M5 and the CDR write by hand in their Architecture.Spec projects, made TSpec's, and the document an
architecture spec writes. Everything is read from the spec project's build — its deps.json, and the
assemblies and PDBs beside it — with no new dependency.

## Steps, in order

2. **What a project reaches.** `ProjectGraph` walks the graph for `Redundant()` but keeps the walk private
   (`ReachedFrom`); the CDR copies it to compare each host's closure with its solution filter, and to find
   the hosts that ship a store.
3. **A project's folder, project file and source files.** M5 and the CDR walk up to their `.slnx` and map
   assembly names to folders by their own conventions: the CDR's `Medrave.Cdr.Capabilities.Auth` is
   `Capabilities/Auth/Auth.csproj`. A project's PDB lists every file compiled into it, interface-only ones
   too, which is how TSpec already finds the spec project's folder. Leave out generated files under `obj/`;
   include the running spec project, since M5 checks its own sources. Example: the CDR's "a host has no
   source file but `Program.cs`".
4. **The types each project declares, and those it uses from other projects.** Each type's namespace, kind,
   visibility and file, and the types of other projects each project uses, read from the compiled
   assemblies (`System.Reflection.Metadata`, already used for PDBs) and exposed for custom rules, like
   `graph["X"]`. M5's `PublicTypes` approximates it in 155 lines of identifier matching, the CDR's usage
   rules with regexes over usings and tokens; the project graph cannot serve, since the redundancy rule
   removes edges a project still uses. Blind spots: `nameof` and a `const` read compile to literals, and a
   `typeof` in an attribute is a name in its blob.
5. **Port the consumers' usage rules and diff.** M5's `PublicTypes` and the CDR's three usage rules (contract
   interfaces consumed by an entry or host, stores shipped only with their module, domain leaves that could
   dissolve) rewritten on step 4, run against their builds, findings compared with today's. M5 builds only
   with the CDR checkout on the branch it expects.
6. **A built-in rule where step 5 shows the same query twice** — likely M5's: a public type no other project
   uses. A type named in a public signature of its own project counts as used; a use from its own spec does
   not, since `InternalsVisibleTo` opens internals to it — read from the assembly, where M5 reads the csproj.
7. **Uses within a project, and the file that writes each.** M5's `SubdomainDependencies` keeps the folders
   of `Application` one way — Chat at the bottom, Generation and Assistant on Chat, Workbench on Generation
   and Chat — by reading every qualified name with Roslyn; the CDR's "an entry file speaks to at most one
   contract namespace" reads usings per file. Read each type's uses from its signatures, attributes and
   method bodies' IL, and place each in its file through the PDB's sequence points; the PDB places a type
   without methods too. A second reading mechanism: port both rules and diff, as in step 5.
8. **Namespace dependencies.** `Within` over the namespaces of a project, as over projects, so M5's subdomain
   graph is a switch like its project graph. M5 also requires every folder to be placed in its graph, which
   a rule given as a function cannot show.
9. **The architecture document.** An architecture spec writes none: its subject is derived from its name,
   `Architecture` for M5's, and no project is that, so M5 leaves it out. It should state the rules in words
   and draw the graph they hold over as a Mermaid diagram — the built graph, and what the rules allow each
   project, which can be listed since a rule is a function of the project. The architecture tests are plain
   classes, not `Spec`s. Needs a design session and its own breakdown first.
10. **Findings that only shrink.** M5 writes the same ratchet four times (`LineWidths`, `NestingDepth`,
    `ArgumentLists`, `TypesPerFile`; over sixty files in `NestingDepth` alone): a listed file is allowed its
    count, an unlisted or grown one fails, and a listed file that has come clean or no longer exists fails
    until it leaves the list. Any rule adopted on existing code needs it, `Within` too. To decide: findings
    keyed by file with a count, as M5's, or by their text; the list in code or in a committed file. TSpec
    never adds to the list, or the rule could not fail.
11. **What exposed members take and return.** M5 checks with Roslyn that no public or internal member takes
    or returns a tuple (`TupleMembers`) or returns an `IEnumerable<T>` (`EnumerableMembers`), and its
    boundary rule — nothing crosses as an enum, a custom value type, a time type other than `DateOnly`,
    `object` or `dynamic` (CODING-STANDARD 6b) — is unchecked. Each member's visibility and signature, read
    beside step 4's types.

## How to work a step

- Propose before code: the failing example, the behaviour, any new wording, and the docs line if a reader
  would otherwise get it wrong; for a rendering change, before and after.
- Test first. The suite green on all three frameworks, MyHotel green. Report and stop.
- Delete a step when it lands, and the plan with its last step.
