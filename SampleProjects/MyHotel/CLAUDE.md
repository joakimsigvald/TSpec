# MyHotel — instructions for coding agents

Applies to everything under `SampleProjects/MyHotel/`; what MyHotel is: [README.md](README.md).
Before editing code, read [CODING-STANDARD.md](CODING-STANDARD.md) and the backend or frontend one
for the code touched.

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

- Entry and Core never reference each other. When Entry needs a model from Core, promote it to
  Contract, or duplicate it there and map.
- Every reference a project may take, packages included, is listed in
  `Architecture.Spec/Dependencies/ProjectDependencies.cs`: adding one is an architecture decision,
  made there.
- Core is split into subdomains that name their purpose (`Core/Rooms/`), and Contract the same way.
  Bookings may use Rooms, never the reverse (`SubdomainDependencies`), so a rule that has Rooms
  consult Bookings needs a decision there first. Core takes no dependency that would hurt its
  testability.
- Rooms and bookings are each stored in a JSON file. A store loads and saves the whole list, since
  one that could find or replace an item would hold rules that belong to Core. Its path comes from
  configuration, so each spec gets a file of its own; a spec that shares one is broken.
- An endpoint is a named handler method in Entry that calls one service method and builds the
  response. Not a lambda: a route parameter is bound by name, and a lambda's parameter may not be
  that long (6c). A service has one method per endpoint, and throws what would have been an early
  return, as an exception declared in Contract.
- `GlobalExceptionHandler` maps those exceptions to a status and an `ErrorBody` coded with the
  exception's name, so a new one needs a case there. Anything else is left to the pipeline, which
  logs it and answers 500.
- A red coding-standard fact in `Architecture.Spec` is fixed in the code; a rule that does not fit
  is changed in the standard together with its fact, never switched off in the source.
- Web's dev server forwards `/rooms` and `/bookings` to Host (`vite.config.ts`), so a new top-level
  route needs an entry there.

## Rules

- The product owner leads. Build only what was asked for, the smallest thing that works within the
  layers: no unrequested endpoints, models, validation, error handling or configuration, no
  repository per entity, and no interface without a mock or a second implementation that needs it.
  If the next step looks obvious, say so and wait.
- Spec first, as the repository's `CLAUDE.md` says: a compile error, or a 404 where a wrong value
  was expected, has not yet observed the behaviour.
- A claim is specified at the layer that owns it:
  - `Host.Spec`, subject `Hotel` over HTTP with its own data files, states the HTTP contract —
    routes, statuses, bodies — and what survives a restart.
  - `Core.Spec`, a Core class with its collaborators mocked, states the domain rules: what is
    unique, kept in order, refused. A folder per class, a `When…` class per method.
  - `Web` states what the user sees and what each action sends (`CODING-STANDARD-FRONTEND.md` 8).

  Contract and Entry get no specs: Entry's mapping is stated end to end. Never restate a status code
  in Core, or a domain rule over HTTP.
- A spec claims only what its `When` returns: an operation that reports a bare `bool` states the
  flag, and its effect under the read that observes it. Design Core's interfaces with that in mind.
- A refusal also states something only its handler could produce, such as the room number in the
  error body: a 404 alone holds just as well when the route does not exist.
- Do not pin rendering with `Specification.Is` here: `Core.Test` already locks it, and a rendering
  change shows in the committed `_specification` diff.

## Mechanics

- net10.0 only, warnings are errors. MyHotel's `.editorconfig` replaces the repository's and holds
  the backend standard's analyzer rules; `BannedSymbols.txt` bans the calls 5c and 8e forbid. Fix
  what they flag, never suppress it.
- Run a spec project with `dotnet test SampleProjects/MyHotel/Host.Spec` (or `Core.Spec`,
  `Architecture.Spec`), narrowed with `--filter-class`; Web's tests are in `TESTING.md`. Building
  the solution bundles Web, so it needs Node.
- The `_specification` files are regenerated by a green run: never edit them, and read their diff
  when reviewing a change.
- `Microsoft.OpenApi` stays on 2.x: `Microsoft.AspNetCore.OpenApi`'s source generator only compiles
  against 2.x's object model.
- Web's `typescript` stays on 6.x: with 7.x, Visual Studio's editor reads `.tsx` as plain
  TypeScript and flags every JSX element; the compiler itself is fine.
- TSpec usage: the repository's [agent-reference.md](../../agent-reference.md), and
  [Architecture.CSharp/agent-reference.md](../../Architecture.CSharp/agent-reference.md) for the
  rules on the code. Both are working copies, so they may run ahead of the packages MyHotel
  references.
