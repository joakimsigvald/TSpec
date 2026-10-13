# Architecture plan

The rules M5 and the CDR write by hand in their Architecture.Spec projects, read from what TSpec already
reads in the spec project's build: its deps.json, and the assemblies and PDBs beside it. No new dependency.

## Steps, in order

1. **A failure lists every finding.** A failed `Is().Empty()` prints at most five items, each cut at 80
   characters, on one line after the asserted expression, so M5's `… 7 levels, 6 allowed - extract a method`
   loses its remedy. `Within` and `Redundant` print theirs the same way.
2. **What a project reaches.** The CDR walks the graph itself to compare each host's closure with its
   solution filter; `ProjectGraph` has that walk, private (`ReachedFrom`).
3. **A project's folder and source files.** M5 and the CDR find them by walking up to their `.slnx` and
   mapping project names to folders by convention. A project's PDB lists the files compiled into it, which
   is how TSpec already finds the spec project's folder; generated files under `obj/` are left out.
   Example: the CDR's "a host has no source file but `Program.cs`".
4. **The types each project declares, and those it uses from other projects.** Read from the type
   references in the compiled assemblies (`System.Reflection.Metadata`, already used for PDBs) and exposed
   for custom rules, like `graph["X"]`. M5's `PublicTypes` approximates it in 155 lines of identifier
   matching, the CDR's usage rules with regexes over usings and tokens; the project graph cannot serve,
   since the redundancy rule removes edges a project still uses. Blind spots: `nameof`, a `const` read and
   `typeof` in an attribute leave no reference.
5. **Port the consumers' usage rules and diff.** M5's `PublicTypes` and the CDR's three usage rules
   (contract interfaces consumed by an entry or host, stores shipped only with their module, domain leaves
   that could dissolve) rewritten on step 4, run against their builds, findings compared with today's.
6. **A built-in rule where step 5 shows the same query twice** — likely M5's: a public type no other
   project uses. A type named in a public signature of its own project counts as used; a use from its own
   spec does not, since `InternalsVisibleTo` opens internals to it.
7. **Per file.** The CDR's "an entry file speaks to at most one contract namespace" reads usings per file;
   step 4's references can be placed in files through the PDB's sequence points.

## How to work a step

- Propose before code: the failing example, the behaviour, any new wording, and the docs line if a reader
  would otherwise get it wrong; for a rendering change, before and after.
- Test first. The suite green on all three frameworks, MyHotel green. Report and stop.
- Delete a step when it lands, and the plan with its last step.
