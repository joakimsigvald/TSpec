# SPECIFICATION.md generation — implementation plan

Target release: **2.1.0**. Realizes Stage 1 of [TSpec-vision.md](TSpec-vision.md) §4, scoped to
one document per Spec project, built on the 1.5.0 specification vocabulary with no changes to
how a single test renders.

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

**Write only on a complete, green run; otherwise leave the file untouched.** A filtered run must
not truncate the document, and a red run must not publish one. Both are the same check (§5).

**Skipped tests do not exist.** They contribute nothing and are excluded from the expected set.
This closes vision §4's open item ("mark or omit, pick one") in favour of omit. Recording only
on `Passed` handles static skips, dynamic skips and failures uniformly with no special cases.

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
- Locating the project directory: TSpec ships `build/TSpec.props` injecting
  `[AssemblyMetadata("TSpecProjectDirectory", "$(MSBuildProjectDirectory)")]`. **To verify** —
  fallback is walking up from `AppContext.BaseDirectory` for the `.csproj`.
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
3. Should TSpec's own `Core.Test` enable generation? It is a framework testing itself; the
   document would describe TSpec's API rather than a domain. Recommendation: leave it off,
   dogfood on MyHotel.
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
