# MyHotel — instructions for coding agents

Applies to everything under `SampleProjects/MyHotel/`. What MyHotel is: [README.md](README.md).

**Read [CODING-STANDARD.md](CODING-STANDARD.md) and, for the code touched,
[CODING-STANDARD-BACKEND.md](CODING-STANDARD-BACKEND.md) or
[CODING-STANDARD-FRONTEND.md](CODING-STANDARD-FRONTEND.md) before editing code.**

## Architecture — Neat

Folders are named for the layer, assemblies are `MyHotel.<Layer>`. `Architecture.Spec` confirms
this section; its generated `_specification.md` is read only as a diff, when a change updates it.

| Project | Assembly | References | Holds |
|---|---|---|---|
| `Host` | `MyHotel.Host` | Entry, Infra | startup and DI, almost nothing else |
| `Entry` | `MyHotel.Entry` | Contract | the REST endpoints, as thin as possible |
| `Contract` | `MyHotel.Contract` | nothing | the models and interfaces Entry needs — the public shape |
| `Core` | `MyHotel.Core` | Contract | all the logic, and the interfaces it needs storage to satisfy |
| `Infra` | `MyHotel.Infra` | Core | storage and outward calls; thin, logic-free, declaring no interface |
| `Web` | — | nothing; calls the API over HTTP | a React UI, one folder per resource |

Web's dev server forwards `/rooms` and `/bookings` to Host (`vite.config.ts`), so a new top-level
route needs an entry there.

**Entry and Core never reference each other**, and the compiler is what enforces it. When Entry needs
a model that lives in Core there are two moves, both cheap: promote it to Contract, or duplicate it
in Contract and map. Never add the reference.

**`Architecture.Spec/Dependencies/ProjectDependencies.cs` states every reference the projects allow**, packages
included. One it does not list fails the suite, so adding a reference is an architecture
decision, made there.

**`Architecture.Spec` also checks the coding standard**, one fact per rule a tool can check from the
source, citing its rule. A red fact is fixed in the code; a rule that does not fit is changed in
`CODING-STANDARD-BACKEND.md` and its fact in the same change, never switched off in the source.

**Core is structured vertically** — subdomains that name their purpose (`Core/Rooms/`), not another
horizontal layer inside. Which may use which is `SubdomainDependencies`' graph: Bookings uses Rooms,
never the reverse, so a rule that has Rooms consult Bookings needs a decision there first. Beyond
Contract, Core takes no dependency that would hurt its testability.

**Contract is split the same way** — `Contract/Rooms/` and `Contract/Bookings/`; what both use
stays at the root.

**Storage is a JSON file, so rooms outlive the process.** `IRoomStore` is declared in Core and
implemented in Infra — whole-list load and save, because a store that knew how to find or replace one
room would be holding rules that belong to Core. The path comes from configuration
(`RoomStore:Path`), which is what lets a spec give each test its own file; see `TestApi`. A spec that
shares a store with another spec is a broken spec, not a slow one.

**An endpoint calls one service method and builds a response — nothing else.** Each is a named
handler method in Entry, never a lambda: a route parameter such as `bookingNumber` is bound by name,
and a lambda's parameter may not be that long (6c). `IRoomService` has one method per endpoint,
taking what the endpoint takes and named per 4c (`Rooms()`, `RoomFor(roomNumber)`). What would have
been an early return is thrown instead, so a method either returns the value the endpoint needs or
does not return at all. The exceptions are declared in Contract, because they are part of the shape
Entry codes against.

**`GlobalExceptionHandler` maps those exceptions to a status and an `ErrorBody`, and handles nothing
else.** The body's code is the exception's name. An unrecognised exception is returned to the
pipeline, which logs it and answers 500 — catching it would replace a real diagnostic with a guess. A
new Contract exception needs a case there, or it becomes a 500.

## Rules

**The product owner leads.** Build only what was asked for — no unrequested endpoints, models,
validation, error handling or configuration. If the next step looks obvious, say so in a sentence
and wait.

**Spec first**, as the repository's `CLAUDE.md` says. A compile error, or a 404 where you expected a
wrong value, means the behaviour has not been observed yet.

**Spec at the layer that owns the claim.** Three suites, stating different things:

- `Host.Spec` — black-box, subject `Hotel`: the running application reached over HTTP, with its
  own room file. States the *HTTP contract* — routes, status codes, response bodies — and, because
  `Hotel` can restart, what survives one.
- `Core.Spec` — subject is a Core type with its collaborators mocked, no HTTP. States the *domain
  rules*: what is unique, what order things are kept in, what is refused. One folder per class under
  test (`RoomService/`), one `When…` class per method.
- `Web` — states what the user sees for what the API answers, and what each action sends
  (`CODING-STANDARD-FRONTEND.md` 8).

`Contract` and `Entry` hold no logic and get no specs — Entry's mapping is stated end-to-end. Do not
narrow a spec to a layer when the behaviour is only observable end-to-end, and do not restate a
status code in Core or a domain rule over HTTP.

**Assertions only see `Result`.** TSpec exposes no subject to the test, so a spec can claim only what
its `When` returns. An operation reporting a bare `bool` can state only that flag; its *effect* has
to be stated under the read that observes it. Worth knowing before designing an interface Core.Spec
will have to describe.

**An assertion that checks only an absence proves nothing.** `Result.StatusCode is NotFound` holds
just as well when the route does not exist, so every refusal also states something only a handler
could produce — `say which room` reads the error body for the room number. Statuses the framework
itself never returns (409, 400) do not have the problem.

**Smallest thing that works, within the layout.** The layers are fixed; what goes in them is not.
Extract a type inside a layer only when leaving it where it is makes the *current* change harder. No
interface without a mock or a second implementation that needs it, no repository per entity, no
configuration.

**The document is the artifact, not the per-test text.** MyHotel exists to exercise
the generated `_specification/` folder; per-test rendering is already locked by the expectations in `Core.Test`. Do not
add `Specification.Is("""…""")` here to pin rendering — it duplicates that cover and brings a failure
mode of its own, since reading the specification from inside a test freezes it mid-test. A rendering
change is caught where it matters: in the committed files' diff.

## Mechanics

- net10.0 only. `TreatWarningsAsErrors` everywhere — fix warnings, don't suppress them.
- MyHotel's `.editorconfig` replaces the repository's (`root = true`): it holds the analyzer rules of
  `CODING-STANDARD-BACKEND.md`, each citing its section, after Medrave M5's. `BannedSymbols.txt` bans
  the calls 5c and 8e forbid, in every C# project (`Directory.Build.props`).
- Run each Spec project with `dotnet test` (filter with `--filter-class MyHotel.Host.Spec.WhenGetVersion`):

  ```bash
  dotnet test SampleProjects/MyHotel/Host.Spec
  ```

  ```bash
  dotnet test SampleProjects/MyHotel/Core.Spec
  ```

  ```bash
  dotnet test SampleProjects/MyHotel/Architecture.Spec
  ```

- Web's tests run with npm (`TESTING.md`); `dotnet test` does not run them. Building the solution
  type-checks and bundles Web, so it needs Node.

- **`Microsoft.OpenApi` stays on the 2.x line.** `Microsoft.AspNetCore.OpenApi`'s source generator
  emits code that only compiles against 2.x's object model; 3.x breaks the build.
- **Web's `typescript` stays on the 6.x line.** With 7.x, Visual Studio's editor reads `.tsx` as
  plain TypeScript and flags every JSX element; the compiler itself is fine.
- Update README.md's endpoint table whenever an endpoint is added, removed, or changes contract.
- `Host.Spec` and `Core.Spec` each generate their own `_specification/` folder from a green run, and
  `Architecture.Spec` its `_specification.md`. Never hand-edit a file in them; commit the regenerated
  files and read their diff as part of reviewing the change.
- TSpec usage: [agent-reference.md](../../agent-reference.md), and for the architecture rules on the
  code [Architecture.CSharp/agent-reference.md](../../Architecture.CSharp/agent-reference.md) — the
  working copies, so they may run ahead of the packages MyHotel references.
