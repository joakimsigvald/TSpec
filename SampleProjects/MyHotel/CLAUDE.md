# MyHotel — instructions for coding agents

**Purpose:** what an agent needs to work on MyHotel that no other document holds: the architecture,
which `Architecture.Spec` confirms, and the rules and mechanics of working here.

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
- Each resource is stored in a JSON file. A store loads and saves the whole list, since one that
  could find or replace an item would hold rules that belong to Core. Its path comes from
  configuration, so each spec gets a file of its own; a spec that shares one is broken.
- `GlobalExceptionHandler` maps the exceptions Contract declares to a status and an `ErrorBody`
  coded with the exception's name, so a new one needs a case there. Anything else is left to the
  pipeline, which logs it and answers 500.
- A red coding-standard fact in `Architecture.Spec` is fixed in the code; a rule that does not fit
  is changed in the standard together with its fact, never switched off in the source.
- Web's dev server forwards each of the API's top-level routes to Host (`vite.config.ts`), so a new
  one needs an entry there.

## Rules

- The product owner leads. Build only what was asked for, the smallest thing that works within the
  layers: no unrequested endpoints, models, validation, error handling or configuration, no
  repository per entity, and no interface without a mock or a second implementation that needs it.
  If the next step looks obvious, say so and wait.
- Spec first, as the repository's `CLAUDE.md` says, by the rules in [TESTING.md](TESTING.md): a
  compile error, or a 404 where a wrong value was expected, has not yet observed the behaviour.

## Mechanics

- net10.0 only, warnings are errors. MyHotel's `.editorconfig` replaces the repository's and holds
  the backend standard's analyzer rules; `BannedSymbols.txt` bans the calls 5c and 8e forbid. Fix
  what they flag, never suppress it.
- Building the solution bundles Web, so it needs Node.
- `Microsoft.OpenApi` stays on 2.x: `Microsoft.AspNetCore.OpenApi`'s source generator only compiles
  against 2.x's object model.
- Web's `typescript` stays on 6.x: with 7.x, Visual Studio's editor reads `.tsx` as plain
  TypeScript and flags every JSX element; the compiler itself is fine.
