# SPECIFICATION.md generation — implementation plan

Target release: **2.1.0**. Realizes Stage 1 of [TSpec-vision.md](TSpec-vision.md) §4, scoped to
one document per Spec project, built on the 1.5.0 specification vocabulary with no changes to
how a single test renders.

**Start at [§10 Build log](#10-build-log)** for what is built and what is next. Sections 1–9 are
the design and stay authoritative; when a decision changes, they get corrected in place rather
than contradicted from the log. [§11](#11-f2--hoisting-shared-setup) is the staged plan for the
next piece of work.

## 1. Scope

**In:** collecting each passing test's existing specification text, checking the run was
complete and green, and writing one `SPECIFICATION.md` per Spec project.

**Out:** any change to the per-test specification text or the failure output; hoisting shared
`Given` clauses; laws; cross-assembly merging; a CLI tool or MSBuild orchestration.

**Governing constraint:** the per-test specification already has a purpose and is locked by
hundreds of expectations in the suite. This epic adds *structure alongside it*, never inside it.

## 2. Verified facts

Probed 2026-07-26 against xunit.v3 3.2.2 on net10.0 with a scratch project, under default
parallelism. These are the facts the design rests on — re-verify if the xunit version moves.

| Fact | Result |
|---|---|
| `TestContext.Current.TestState` at test-class `Dispose` | **Available.** Returns `Passed` / `Failed` / `Skipped` correctly, even though the XML doc says "only available after the test has finished running". `TestStatus` is still `Running` at that point — use `TestState.Result`, not `TestStatus`. |
| `[assembly: AssemblyFixture(T)]` disposal | **Runs last**, after every test, including when tests failed, under default parallelism. |
| Statically skipped test (`[Fact(Skip=...)]`) | Never constructs the test class. No `Dispose`, no trace. |
| Dynamically skipped test (`Assert.Skip`) | Constructs and disposes, reports `TestState.Result == Skipped`. |
| Test whose **constructor throws** | Never reaches `Dispose`. Counted `Failed` by the runner but **invisible** to per-test collection. |
| Reflecting `[Fact]` methods and their `Skip` at runtime | Works; gives the expected set. |

The constructor-throws row is why §5's completeness check is load-bearing rather than a
convenience: without it, one such test would let a document publish from a red run.

## 3. Decisions

**One `SPECIFICATION.md` at the Spec project root.** Not one per subject folder. Subjects become
headings within the single document. Reviewed as a whole, found with the same conventional
uppercase name as `README.md`, and globbable across a repo as `**/SPECIFICATION.md` — which is
also why no cross-assembly index is needed.

**Participation is by attribute, nearest declaration wins.** `[ExcludeFromSpecification]` and
`[Specification]` both apply at class or method level; the closest one to the test decides. Default
is include. Rationale (user, 2026-07-26): the framework cannot detect tests that were never
written, so absence in the document never certified coverage in the first place — an untagged
class is just another way to have no test. Opt-out polarity therefore costs nothing and keeps the
common case free of ceremony.

**Generation is enabled per project by one line**, `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`
in the Spec project. No environment variable, no MSBuild property, no run mode. An ordinary
`dotnet test` in a project without that line behaves exactly as today.

**The subject is derived by convention, then verified against the build graph.** The document
describes the production project, so TSpec has to name it. The subject name is the spec assembly
name with its last suffix stripped — `MyHotel.Spec` describes `MyHotel`, and *any* suffix works,
not a list of known ones. That name is then required to appear among the direct project references
the build recorded in the spec assembly's `deps.json`; if it does not, `SetupFailed` is thrown
before the first test, listing the references found. The version is the one the build resolved for
that project, i.e. `<Version>` in the production project file. Rejected: the `Spec<TSubject>` type
arguments (a black-box API spec's subject is `HttpClient`, so this collapses), and
`GetReferencedAssemblies()` filtered by `System.*`/`Microsoft.*` (swaps the user's naming
convention for Microsoft's). Decision (user, 2026-07-26): convention for the name, build graph for
the check — good enough for MVP, revisit if a project needs a different shape.

**Write only on a complete, green run; otherwise leave the file untouched.** A filtered run must
not truncate the document, and a red run must not publish one. Both are the same check (§5).

**Skipped tests do not exist.** They contribute nothing and are excluded from the expected set.
This closes vision §4's open item ("mark or omit, pick one") in favour of omit. Recording only
on `Passed` handles static skips, dynamic skips and failures uniformly with no special cases.

**Erasure is justified semantically, never by taste.** The recurring question in the specification
language is whether a token is a *claim about the subject* or a *mechanism of the test*; mechanism
is erased. Erased so far: `await`, `async` plus an explicit lambda return type, `!`, and `?.`
(rendered as a plain `.`). Kept: `?` on a type, because `int?` and `int` differ in what values can
occur — that is a claim, and the describer sees only source text so it cannot tell a nullable value
type from a nullable reference type. The `?` case is the rule's load-bearing one: it is where
erasure would have weakened a real statement, and that is what stopped it. Erasure is cumulative
and one-directional, and no test can ever report that too much has been erased.

**The document and the per-test specification render from one and the same text** (user,
2026-07-27). Not because it is simpler, but because the hundreds of `Specification.Is(...)`
expectations are what keep the document honest: with one renderer they are simultaneously the
document's regression tests, and every line in `SPECIFICATION.md` is text a passing test actually
produced. A second renderer would leave every requirement in the repo unpinned. The document may
add *structure around* the text — headings, grouping, hoisting — but never a different version of
it. Where a failure report wants more than the specification says (setup context, for instance),
it appends it; it does not render a variant.

**The specification text is used as rendered today**, including its 80-character wrapping.
Markdown reflows wrapped lines inside a list item, so this is expected to read acceptably.
Re-rendering unwrapped is *not* cheap — the recorded actions close over the `TextBuilder`
instance created in the `SpecificationContext` constructor, so a second pass with a different
builder needs the builder made swappable. Deferred to the §7 tuning loop; do it only if real
output demands it.

## 4. Structure to collect

One entry per passing, participating test, recorded at `Spec.Dispose()`:

All context is **names**. No type analysis, no expression parsing, no fallback rules.

| Field | Source | Note |
|---|---|---|
| Namespace | the test class's namespace | Mirrors the production folder structure under §6.1; the grouping key for subjects. |
| Class chain | concrete class name plus each base class name, walking `BaseType` | Yields `WithItems → GivenCartExists → WhenPlaceOrder`. The outermost entry is the `When*` class, i.e. the method under test; the rest is the branch path. |
| Test method name | `TestContext.Current.Test` | Requirement identity, sort key, dedup key. The bullet text is the rendered assertion, per vision §11 Q1. |
| Specification text | `Specification.ToString()` | Unchanged. Already contains the `When` line, so the method under test needs no separate extraction. |

The only thing the chain walk needs from `Spec` itself is a stop condition, so that
`Spec<ShoppingService>` and `object` don't end up in the branch path.

A project that doesn't follow §6.1 produces an oddly-shaped document. That is the vision's
faithful-rendering principle doing its job, not a defect to engineer around.

Collection is a static `ConcurrentBag` in TSpec, populated only when a `SpecificationDocument`
fixture exists. Theories produce one entry per case; identical specification text dedupes to one
line, which is the common outcome since generated values never appear in the text.

## 5. Completeness check

At `SpecificationDocument.Dispose()`, before writing anything:

1. Reflect over the assembly for every participating test method (`[Fact]`/`[Theory]` on a
   `Spec` subclass, after applying the nearest include/exclude attribute).
2. Subtract those with `Skip` set.
3. Compare that expected set against the set that reported in at `Dispose`.
4. Write only if they match exactly. Otherwise leave `SPECIFICATION.md` alone and report why.

Equality catches all three failure modes with one rule: a filtered run reports fewer, a failed
test never reports, and a constructor-throws test never reports. There is deliberately no
separate "was anything red" flag — non-passing tests simply never enter the collection.

## 6. Implementation phases

### Phase 1 — collection (library, small, independently testable)
- `SpecificationEntry` record and a static thread-safe collector.
- `Spec.Dispose()`: if a document fixture is active, the type participates, and
  `TestContext.Current.TestState?.Result == TestResult.Passed`, record an entry.
- `[Specification]` / `[ExcludeFromSpecification]` attributes, `Inherited = true` so a
  `When*` base covers its nested `Given*` classes (which derive from it under §6.1).
- Per-type participation and the class chain resolved once and cached; `Dispose` runs on every test.

### Phase 2 — completeness and write
- `SpecificationDocument` public assembly-fixture type.
- Expected-set reflection and the §5 comparison.
- Markdown rendering: subject headings, method sub-headings, branch path, bullet per requirement,
  everything sorted (namespace, subject, method, branch path, requirement) so parallel execution
  order cannot reach the page.
- ~~Locating the project directory: TSpec ships `build/TSpec.props` injecting
  `[AssemblyMetadata("TSpecProjectDirectory", "$(MSBuildProjectDirectory)")]`. **To verify** —
  fallback is walking up from `AppContext.BaseDirectory` for the `.csproj`.~~ **Settled**: the
  fallback alone is enough. Walking up for the first ancestor holding a `.csproj` works, and no
  props file ships. This matters beyond convenience — `.props` files do *not* flow over a
  `ProjectReference`, so a props-based mechanism would work for package consumers but not for
  this repo's own `MyHotel.Spec`, and the dogfooding path would stop matching the shipping path.
- Generated-file header naming the subject set and warning against hand-editing.

### Phase 3 — tune the format against MyHotel
The phase that does not compress (vision §12). Wire MyHotel up early; the format is the product.
Everything above is mechanical by comparison.

### Phase 4 — documentation
`README.md` and `TSpec-agent-reference.md` per CLAUDE.md's hard rule, including the agent
reference's "covers TSpec x.y" line.

## 7. Open questions — answer from real output, not in advance

1. Does the flat paragraph read acceptably as a bullet, or does the `Given` need hoisting to the
   branch heading? Siblings under a shared base produce byte-identical setup text, so
   collapse-identical is a string comparison; true prefix hoisting needs step-level decomposition
   and is a much larger change. Decide from MyHotel.
2. Does 80-character wrapping survive markdown rendering, or is the swappable `TextBuilder`
   worth building?
3. ~~Should TSpec's own `Core.Test` enable generation?~~ **Answered: no.** It is a framework
   testing itself; the document would describe TSpec's API rather than a domain. `Core.Test`
   carries no `AssemblyFixture` line and dogfooding happens on MyHotel.
5. Vision §11 Q4 — structural classification of the diff (claim added / strengthened / weakened /
   removed) — stays out of scope here.

## 8. Testing

The renderer and the completeness comparison are pure functions over `SpecificationEntry` lists
and get ordinary unit tests. The fixture wiring — attribute discovery, `TestState` at `Dispose`,
end-of-assembly ordering — cannot be self-tested from inside the same assembly and needs a
scratch project of the kind used for §2. Keep that project; it is the regression test for the
xunit facts.

## 9. Release train

| Version | Content | Size |
|---|---|---|
| **1.6.0** | `TODO.txt` line 1 — fail a test whose pipeline never ran. Minor, not a patch: a green suite going red must not arrive in a patch upgrade. | ~a day |
| **2.0.0** | Removals only — the three `[Obsolete]` members plus `IVerifyService`/`VerifyService`, per IMPROVEMENT-PLAN.md's destination table. | days |
| **2.1.0** | This plan. | the open-ended one |

Decoupled deliberately: the generator is purely additive and does not justify a major, and
bundling it would hold the deprecation cleanup hostage to the Phase 3 tuning loop.

## 10. Build log

Working record on branch `specification-generator`. Sections above are the design and stay
authoritative; this section is what has actually happened against it.

### Built, in order

| # | What | Notes |
|---|---|---|
| 1 | `MyHotel` + `MyHotel.Spec` | Minimal API in one `Program.cs`, Scalar UI at `/scalar`, `GET /version`. `ApiSpec<TResult>` gives every spec an `HttpClient` over `WebApplicationFactory<Program>`, so specs are black-box by default. |
| 2 | `MyHotel/README.md`, `MyHotel/CLAUDE.md` | Development rules for the reference app: PO leads, spec-first, black-box default, one project, simplistic until it hurts. Pointer added to root `CLAUDE.md` — needed because `MyHotel/CLAUDE.md` does not apply to `MyHotel.Spec/`. |
| 3 | `SpecificationDocument` assembly fixture | Opt-in by one line, per §3. Resolves subject and output path in its *constructor*, so a misconfigured project fails before the first test; writes at `Dispose`. |
| 4 | Subject resolution and rendering | `ProjectReferences` (direct project refs from `deps.json`), `SpecificationSubject` (derive + verify, §3), `ProjectDirectory` (walk up for the `.csproj`), `DocumentRenderer`. 17 unit tests in `Core.Test/Internal/Document`. |
| 5 | `/version` reads the assembly version | Driven out red-green; `<Version>` in `MyHotel.csproj` is now the single source, and feeds the generated document too. |
| 6 | `README.md` §7, agent-reference section | Phase 4 started early because CLAUDE.md makes docs part of the change, not a later step. Both marked work-in-progress. |
| 7 | Subject resolution hardened | Composition extracted to `PendingDocument.Prepare`, so the whole chain is testable against a real directory rather than only through a test run. Failure messages now state both expectations — naming (`.Spec` preferred, `.Test` fine) and a *direct* project reference — whichever half broke. 32 tests in `Core.Test/Internal/Document`, up from 17; the added ones cover the manifest-reading and directory-walking paths that previously had none. Mutation-checked: removing the reference check fails 8 of 12. |
| 8 | `.editorconfig`, build-enforced everywhere | `const` is PascalCase at every accessibility; other non-public fields are `_camelCase`. Enforced with `<EnforceCodeStyleInBuild>` + `IDE1006`, so a violation is a build error under `TreatWarningsAsErrors`. Renamed 7 `private const _camelCase` and 4 PascalCase fields in Core, then 26 more in `Core.Test`. Mutation-checked in both directions. |

**Naming in a spec project is a documentation decision, not a style one.** TSpec renders source
identifiers into the specification text tests pin with `Specification.Is(...)`, so renaming a field
or const in a spec project rewrites specifications and can shift their 80-character wrapping. The
rules are enforced in `Core.Test` regardless (user, 2026-07-26), and the net readability effect was
a wash rather than a loss — fields gained underscores, consts shed them:

```
Given IShoppingCartRepository.GetCart(CartId) returns new ShoppingCart { Id =
      CartId, Items = _cartItems ?? [] }      # CartId became const, so kept its readable name
Given IMyValueIntRepo.Get(the MyValueInt) tap(i => _tappedValue = i) returns
      RetVal                                  # was _retVal
```

The rule turned out to have a useful side effect: because `const` keeps PascalCase, a value that
*should* be const now reads better in the specification than one that is merely an unassigned
field. `WhenAddItem._cartId` was never reassigned; making it `protected const int CartId` was both
the more honest declaration and the better-rendering one. Underscores now mark genuine mutable
setup (`_cartItems`, `_cart`, `_checkout`), which is information the reader wants.

The wrapping happened not to shift through any of this. It is not guaranteed to survive the next
rename — the standing caution recorded in `.editorconfig`, and live input to §7 Q1/Q2 as
`MyHotel.Spec` grows.

| 9 | Phase 1 collection + rendering | `SpecificationEntry`, a `ConcurrentBag` collector inert unless the fixture switched it on, and recording in `Spec.Dispose()` when `TestState.Result == Passed`. Rendered as `## Subject` / `### Branch.Requirement` with the specification in a fenced block, sorted and deduplicated. A non-passing test marks the run and the file is left untouched, with the reason on stderr — verified by breaking a MyHotel spec. |

| 10 | Completeness check (§5) + provenance | `ExpectedRequirements` reflects every non-skipped `[Fact]`/`[Theory]` on every concrete `Spec` subclass; the document is written only when the reported set matches. Verified both ways on MyHotel — a `-method` filtered run now refuses and names what was missing. Header gained `Generated from MyHotel.Spec <mvid8>`, and the file is byte-identical across a full rebuild and rerun. |

**§5 is done, and the staleness question it raised is answered differently than proposed.** A
markdown file cannot detect that it is out of date; any stamp only says what it *was* generated
from, and something must still compare that to reality. Since the document is deterministic, that
comparison already exists and is exact:

```bash
dotnet test && git diff --exit-code -- "**/SPECIFICATION.md"
```

An assembly-version-lag heuristic (the original item 2) catches strictly less — it is blind to every
change within a version, which is where nearly all drift happens — and can be wrong in both
directions. Decision (user, 2026-07-26): stamp provenance for the reader, make CI the gate.

**The hash covers the spec assembly only, deliberately.** Header reads `Version 0.1.0, hash 750be2a0`.

Hashing production too was considered and rejected (user, 2026-07-26). The scenario it appears to
address — production code breaks a test, nobody reruns — is already caught by `dotnet test` failing;
the document was never the detector. What a production hash would add is a *certificate* of which
production build the requirements were verified against, at the cost of diffing `SPECIFICATION.md`
on every PR that touches production. That churn would drown the one signal the document exists for:
a changed line means a changed behaviour.

The spec-only hash was kept over dropping it entirely because it answers a question nothing else
does: **without it, a document that was never regenerated is indistinguishable from one that was
regenerated and came out unchanged.** The accepted cost is that a spec-project edit changing no
requirement still moves the header, so the CI gate asks for a rerun.

**The hash is sensitive to source line endings.** Found by accident: an edit-and-revert that left a
file as CRLF moved the hash, and it did not come back until the bytes did. So the gate requires a
`.gitattributes` normalising line endings (this repo has `* text=auto eol=lf`); without one, a
Windows checkout and a Linux checkout compile different bytes and the check fails on every run for a
reason that looks nothing like its cause. Documented in README §7.3. Nothing is wrong with the
determinism — it is doing exactly what it claims — but the failure mode is obscure enough to
deserve the warning.

**Not built, and deliberately so:** the pipeline that runs the gate. TSpec makes staleness
*detectable*; detecting it is one `git diff --exit-code` in whatever CI the consumer already has.

| 11 | MyHotel rooms, step 1 of 4 | `POST /rooms` and `GET /rooms/{roomNumber}` with their branches (created / conflict, found / not found), driven out red-green. 9 requirements, 6 lines of production code, everything still in `Program.cs`. `ApiSpec` now builds a **fresh `WebApplicationFactory` per test** — the shared one leaked in-memory rooms between tests. Remaining: list, delete, update. |

| 12 | Noise erased from the specification text | `await` and `async`/return-type added to the grammar so they parse and then peel; `!` and `?.` joined them. Erasure is a *describer* policy (`Expr.WithoutNoise`), not a parse-time drop, so one predicate decides what never reaches a specification. `?` on a type deliberately kept — see §3. |
| 13 | `Having` / `Until` keywords, and `ToSource()` | Setup and tear-down steps now render under the name of the method that produced them, closing F1. Separately, `Expr.ToSource()` rebuilds an expression from the tree instead of copying source text, so a 2+ parameter lambda — the one shape with no prose rendering — can no longer smuggle erased keywords back in via its parent's `Raw`. |

**Composing phrases before parsing was a live bug, and the suite caught it.** TSpec builds some
assertion phrases by prepending a word to the *raw* expression and parsing the whole splice —
`"by (it, i) => it + i"`. That is not C#, and the moment the grammar learned about lambda return
types it read `by` as one and swallowed it, silently turning `Numbers is distinct by …` into
`Numbers is distinct …`. Two pinned expectations failed on the first run.

The fix was not to model the connectives as syntax. Every other phrase in TSpec already composes
*after* describing (`$"throws {expectedExpr.Describe()}"`), and the four sites that did it backwards
were simply skipping that convention. Parsing now only ever sees real C#, which retires the whole
collision class instead of enumerating it. It also fixed a defect nobody had noticed: because
`wait () => The(_wait) ms` never parsed, its inner expression was never described — the line read
`The(_wait)` while the line directly beneath it read `the _state`.

**Renamed while there:** `ParseValue`/`ParseCall`/`ParseActual` → `Describe`/`DescribeCall`/
`DescribeActual`, and `ExpressionParser` → `ExpressionDescriber`. The methods return finished prose,
not a tree; parsing is the half the caller never sees. `Describe` carries no suffix because value
mode is not a peer of the other two — it is the default they both fall back to.

### Format findings from the first real document (§7 Q1/Q2 answers arriving)

Nine requirements across two subjects is finally enough to judge the format. Four findings, in the
order I would act on them.

**F1 — `Having` rendered as `After`. Fixed (build log row 13).** The keyword now matches the
pipeline method that produced the step, which was the whole of the defect: `Having` is what the
author wrote, so `Having` is what the reader should see.

```
When api.GetAsync("/rooms/{RoomNumber}")
Having api.PostAsJsonAsync("/rooms", new Room(RoomNumber, 2))
```

`Until` got the same treatment, for the same reason. The delay step needed one extra word — it
shares the setup list, so `After wait … ms` became `Having waited … ms` to stay grammatical.

**F2 — infrastructure `Using` lines dominate.** Every block opens with the same two lines:

```
Using owned api
  and owned api.CreateClient
```

That is 18 of roughly 45 content lines describing test wiring rather than hotel behaviour, and it
grows linearly with the suite. This is §7 Q1 made concrete, and it reframes the question: the issue
is not that identical `Given` clauses repeat, it is that **setup which is pure plumbing has no place
in a specification document at all**. Hoisting shared clauses to the branch heading would fix the
repetition; suppressing infrastructure setup would fix the relevance. They are different fixes and
we probably want both.

**F3 — constants render as their names, not their values.** `api.GetAsync("/rooms/{RoomNumber}")`
and `new Room(RoomNumber, 2)` — a reader cannot see that the room is 101. Faithful to the source and
useless as documentation. This is the deepest of the four because it questions the vision's
faithful-rendering principle: should the document show the *expression* or the *value*? Values are
what a specification reader wants; expressions are what the current design guarantees. Not urgent,
but it should be decided rather than drift.

**F4 — grouping works.** `## WhenAddRoom` with `### GivenNoSuchRoom.ThenRespondCreated` beneath it
reads cleanly, subjects separate properly, and sorting by branch keeps siblings adjacent. The
nesting-based chain (build log row 9) is vindicated: no scaffolding class appears in a heading.

### A testing lesson worth keeping

`GivenNoSuchRoom.ThenRespondNotFound` passed *before* any endpoint existed — an unmatched route also
returns 404. **An assertion that checks only an absence cannot distinguish "not implemented" from
"correctly absent."** The fix is to assert on something only the implementation can produce: return
404 *with a body* (`Results.Problem($"No room {roomNumber}", statusCode: 404)`) and assert on it.
Agreed as worth doing; not yet applied.

**Two deviations from §4.** The class chain follows **nesting (`DeclaringType`), not inheritance.**
The designed `BaseType` walk yields `ApiSpec\`1 → WhenGetVersion → GivenNothing` for MyHotel, because
a shared black-box base sits between the test and `Spec`; nesting gives `WhenGetVersion →
GivenNothing`, which is what §6.1's recommended structure actually expresses. Shared bases are
scaffolding, not specification structure. And **namespace is not yet collected** — with one subject
it would add a heading level for nothing. Both revisit when MyHotel has several subjects.

**The specification text needed LF normalisation.** `TextBuilder` appends `Environment.NewLine`, and
`Specification.ToString()` *documents* that it returns platform-native endings — so it is not a bug
to fix at the source, and changing it would break a published contract. The document normalises at
its own boundary instead, reusing the shipped `NormalizeLineEndings()` that the specification
assertions already use. This matters because the document is committed: without it a Windows run and
a Linux run would differ on every line, and the diff — the whole point of the artifact — would be
noise. Caught by git, not by a test, which is why there is now a test for it.

**Deviation from §6's ordering:** Phase 2's skeleton (fixture, location, identity, write) was built
before Phase 1's collection. A document that contains nothing but a header still exercises the
trigger, the output path and the subject rule end to end — the three things most likely to be
wrong about the environment rather than about the code. Collection then lands on a proven pipe.

**Current output.** `MyHotel.Spec/SPECIFICATION.md` is a header only: subject name, version, and a
do-not-edit comment. No requirements yet.

### Remaining, by decreasing priority

Items 1–3 of the original list (collection, completeness, rendering) are done — see the build log
above. What is left:

1. **Act on F2** — planned in [§11](#11-f2--hoisting-shared-setup). F1 is done. Decide F3 (names vs
   values) before it drifts; it remains a PO call, not mechanical work.
2. **Grow MyHotel, steps 2–4.** List, then delete, then update — in that order (delete is simpler,
   and delete+add covers what update does). Each step reviewed before the next. Adding rooms with
   varied bed counts will also show whether repetition gets worse or the grouping absorbs it.
3. **404 with a body.** Apply the lesson above so absence-only assertions become real requirements.
   One line per handler; do it with step 2.
4. **Opt-out attributes.** `[Specification]` / `[ExcludeFromSpecification]`, nearest declaration
   wins. Deferred from Phase 1 because nothing has yet needed to opt out.
5. **Scratch project for the §2 xunit facts (§8).** Fixture wiring, `TestState` at `Dispose` and
   end-of-assembly ordering cannot be self-tested from inside the same assembly. Currently those
   facts are verified only by MyHotel.Spec passing, which will not catch a regression precisely.
6. **Finish Phase 4 docs.** Drop the work-in-progress notes, update the agent reference's
   "covers TSpec x.y" line — which is stale *now*, since it says 1.5 while documenting post-1.5
   behaviour.
7. **Version and release decision.** `PackageVersion` and `PackageReleaseNotes` are untouched at
   1.5.0. This branch aims at 2.1.0, but 2.0.0 (the removals) has not happened. Needs the PO.

## 11. F2 — hoisting shared setup

Status: **awaiting sign-off.** Nothing built.

### 11.1 The problem, correctly framed

Nine requirements, and 18 of roughly 45 content lines are `Using owned api` / `and owned
api.CreateClient`. Suppressing them was proposed and rejected (user, 2026-07-27): the lines state
something true — every requirement is verified against a freshly built, owned, disposed API — and
deleting them would be an erasure with no semantic justification, which §3 forbids. Under
one-renderer it would also strip them from the failure output, where a leaked-fixture bug is exactly
what they diagnose.

So it is a **repetition** problem, not a noise problem. Repetition is the one thing the document can
fix on its own, because the document has structure the per-test text does not: headings.

### 11.2 Why this forces a two-phase engine

Today every step is `recording.Record(() => textBuilder.AddX(...))` — a closure over one shared
builder, run lazily at `ToString()`. Two properties of that make clause-level hoisting impossible:

**A clause is not a record call.** A mock clause is several: `AddMockSetup` starts a line,
`AddMockReturns`/`AddMockThrows` append with `AddWord`. `Given IMyService.GetValueAsync() first
returns 1 and next returns 2` is one hoistable expression built from three records. The builder call
kind is the clause boundary — a clause is one line-starting step plus every appending step that
follows it. That is what makes "regardless of line breaks or a leading `and`" mechanical.

**Lead words are baked in while describing.** `NextUsingWord()`, `NextGivenWord()`, `NextThenWord()`
and `GetMockName()` decide `Using` vs `and`, and whether to repeat a service name, inside the
closure. For an `and` to become a `Using` after hoisting, that decision has to happen where position
is known.

Hence the split (user, 2026-07-27):

| Phase | Produces | Knows about |
|---|---|---|
| 1 — describe | an ordered list of `Clause` (family, described body, continuations) | expressions, nothing about layout |
| 2 — render | text | lead words, mock-name elision, wrapping, indentation |

Phase 1's output is the structure served to *both* consumers: the per-test specification renders all
clauses in order; the document renders them grouped and hoisted. This keeps §3's one-renderer rule
intact — phase 2 applies the same positional rules at a new position, it is not a second renderer.

**Phase 2 is not a dumb formatter.** `_isChainOfAssertions` decides whether an assert starts a
sentence or appends to one, i.e. phase 2 decides structure as well as words. It is positional, so it
moves cleanly, but it must not be designed as pure formatting.

**Bonus:** phase 2 taking a builder as an argument answers §7 Q2 for free — re-rendering unwrapped
for markdown becomes a parameter rather than a project.

### 11.3 Hoisting rules

1. **Whole clauses only.** Never a line, never a fragment. Line breaks and indentation are phase-2
   artifacts and carry no meaning here.
2. **Complete family runs only, in stage 3.** All the `Using` clauses hoist or none do. This is what
   keeps a headless `and …` from ever being left behind, and it is why lead-word promotion is not
   needed until stage 4.
3. **Shared by every entry under the heading.** Exact match on the clause, no partial credit.
4. **At least three entries under that heading.** Evaluated *per level*, which is the point: at the
   root all nine MyHotel entries share `Using`, so it hoists even though `WhenGetVersion` has only
   two requirements — while that subject's `When`, shared by two, stays inline.
5. **At most two levels**, root and `##` subject. Settled (user, 2026-07-27); a third level would
   require splitting the branch out of the `###` heading, which is not needed by any current example.

Applied to MyHotel today, the `Using` run reaches the root and the `When` reaches `## WhenAddRoom`
and `## WhenGetRoom` but not `## WhenGetVersion`.

### 11.4 Stages

| # | Scope | Gate |
|---|---|---|
| 1 | Phase split, internal to the specification engine. Same text per test, document still fed the rendered string. | Suite green with **zero expectation edits**. Any text change is a refactor bug, not a decision. |
| 2 | Document builder consumes clauses instead of strings and rebuilds the same unhoisted document. | Regenerating `SPECIFICATION.md` yields a diff containing **only the hash line**. |
| 3 | Naive hoisting: one level, complete family runs, shared by all, ≥3 entries. Hoisted clauses simply appear under the shared heading. | Reviewed against the real document. |
| 4 | Full hoisting per §11.3: two levels, partial runs with lead-word promotion, per-level threshold. | Reviewed against the real document. |
| 5 | Bonus — revisit the algorithm if the output asks for it. | Deliberately unknown. |

Stages 1 and 2 have gates that are *checkable*; 3 and 4 end in a PO reading the document, which is
the only verdict that counts for a format decision (see §3 on what the pinned expectations can and
cannot tell us).

**Sizing.** Stage 1 is the bulk — 30 record sites (`SetupPhrases` 14, `AssertionPhrases` 12,
`ActionPhrases` 4) plus five pieces of positional state to relocate (`_givenCount`, `_usingCount`,
`_currentMockSetup`, `_thenCount`, `_isChainOfAssertions`). It is the largest single change in the
epic so far, but it is unusually *safe* grind: the pinned expectations depend on exact lead words,
wrapping and indentation, so the refactor either reproduces the text byte-for-byte or the suite says
where it did not. Everything after stage 1 is small by comparison — stage 2 is wiring across three
files, stage 3 is a grouping pass and an equality comparison, stage 4 adds promotion and the second
level.

**Stage 2 must not touch wrapping.** §7 Q2 becomes answerable once phase 2 takes a builder, but
changing the 80-character wrapping at stage 2 would destroy the only gate that stage has.

### 11.5 Formatting of a hoisted block

Nothing fancy (user, 2026-07-27): the hoisted clauses render as an ordinary fenced block directly
under their heading, before the first child heading, in the same style as a requirement block and
with no lead-in label. If that reads badly against the real document it is a stage 5 question.

### 11.6 Rejected alternative

Recording the family alongside the already-rendered text and promoting a leading `and` to its family
word when hoisting. Much smaller, and it would handle MyHotel. Rejected because it leaves the
mock-name elision as a latent hole, and because the phase split is worth having on its own merits —
it is the structure the engine should have had.
