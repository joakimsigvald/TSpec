# MyHotel — instructions for coding agents

Applies to everything under `SampleProjects/MyHotel/`. What MyHotel is: [README.md](README.md).

## Architecture — Neat

Folders are named for the layer, assemblies are `MyHotel.<Layer>`. `MyHotel` keeps its name and
plays Host, because the deployable is the application.

| Project | Assembly | References | Holds |
|---|---|---|---|
| `MyHotel` | `MyHotel` | everything | Host: startup and DI, almost nothing else |
| `Entry` | `MyHotel.Entry` | Contract | the REST endpoints, as thin as possible |
| `Contract` | `MyHotel.Contract` | nothing | the models and interfaces Entry needs — the public shape |
| `Core` | `MyHotel.Core` | Contract | all the logic, and the interfaces it needs storage to satisfy |
| `Infra` | `MyHotel.Infra` | Core | storage and outward calls; thin and logic-free |

**Entry and Core never reference each other**, and the compiler is what enforces it. When Entry needs
a model that lives in Core there are two moves, both cheap: promote it to Contract, or duplicate it
in Contract and map. Never add the reference.

**Core is structured vertically** — subdomains that name their purpose (`Core/Rooms/`), not another
horizontal layer inside. Beyond Contract, Core takes no dependency that would hurt its testability.

**Storage is a JSON file, so rooms outlive the process.** `IRoomStore` is declared in Core and
implemented in Infra — whole-list load and save, because a store that knew how to find or replace one
room would be holding rules that belong to Core. The path comes from configuration
(`RoomStore:Path`), which is what lets a spec give each test its own file; see `TestApi`. A spec that
shares a store with another spec is a broken spec, not a slow one.

**An endpoint calls one service method and builds a response — nothing else.** `IRoomService` has one
method per endpoint, taking what the endpoint takes. What would have been an early return is thrown
instead, so a method either returns the value the endpoint needs or does not return at all. The
exceptions are declared in Contract, because they are part of the shape Entry codes against.

**`ContractExceptionHandler` maps those exceptions to responses, and handles nothing else.** An
unrecognised exception is returned to the pipeline, which logs it and answers 500 — catching it would
replace a real diagnostic with a guess. A new Contract exception needs a case there, or it becomes a
500.

## Rules

**The product owner leads.** Build only what was asked for — no unrequested endpoints, models,
validation, error handling or configuration. If the next step looks obvious, say so in a sentence
and wait.

**Spec first.** Write the spec → run it → watch it fail *for the right reason* → smallest
implementation → full suite. A compile error, or a 404 where you expected a wrong value, means the
behaviour has not been observed yet. Never write production code that no failing spec asked for.

**Spec at the layer that owns the claim.** Two suites, stating different things:

- `MyHotel.Spec` — black-box, subject `Hotel`: the running application reached over HTTP, with its
  own room file. States the *HTTP contract* — routes, status codes, response bodies — and, because
  `Hotel` can restart, what survives one.
- `Core.Spec` — subject is a Core type with its collaborators mocked, no HTTP. States the *domain
  rules*: what is unique, what order things are kept in, what is refused. One folder per class under
  test (`RoomService/`), one `When…` class per method.

`Contract` and `Entry` hold no logic and get no specs — Entry's mapping is stated end-to-end. Do not
narrow a spec to a layer when the behaviour is only observable end-to-end, and do not restate a
status code in Core or a domain rule over HTTP.

**Assertions only see `Result`.** TSpec exposes no subject to the test, so a spec can claim only what
its `When` returns. An operation reporting a bare `bool` can state only that flag; its *effect* has
to be stated under the read that observes it. Worth knowing before designing an interface Core.Spec
will have to describe.

**Smallest thing that works, within the layout.** The layers are fixed; what goes in them is not.
Extract a type inside a layer only when leaving it where it is makes the *current* change harder. No
interface without a mock or a second implementation that needs it, no repository per entity, no
configuration. Storage stays in memory until asked otherwise.

**The document is the artifact, not the per-test text.** MyHotel exists to exercise
`SPECIFICATION.md`; per-test rendering is already locked by the expectations in `Core.Test`. Do not
add `Specification.Is("""…""")` here to pin rendering — it duplicates that cover and brings a failure
mode of its own, since reading the specification from inside a test freezes it mid-test. A rendering
change is caught where it matters: in the committed document's diff.

## Mechanics

- net10.0 only. `TreatWarningsAsErrors` everywhere — fix warnings, don't suppress them.
- `dotnet test` swallows xunit v3 output. Build, then run the exe — one per Spec project (filter with
  `-class MyHotel.Spec.WhenGetVersion`):

  ```bash
  dotnet build SampleProjects/MyHotel/MyHotel.Spec -f net10.0
  ```

  ```bash
  SampleProjects/MyHotel/MyHotel.Spec/bin/Debug/net10.0/MyHotel.Spec.exe
  ```

  ```bash
  dotnet build SampleProjects/MyHotel/Core.Spec -f net10.0
  ```

  ```bash
  SampleProjects/MyHotel/Core.Spec/bin/Debug/net10.0/MyHotel.Core.Spec.exe
  ```

- Update README.md's endpoint table whenever an endpoint is added, removed, or changes contract.
- Each Spec project generates its own `SPECIFICATION.md` from a green run. Never hand-edit one;
  commit the regenerated file and read its diff as part of reviewing the change.
- TSpec usage: [TSpec-agent-reference.md](../../TSpec-agent-reference.md) — referenced by project, so
  always the working copy.
