# TSpec — Long-Term Vision

**Status:** direction document, not a backlog. Written to be broken down into concrete work
later, in a session with access to the actual codebase.

---

## 1. The thesis

TSpec was built in 2023 to remove boilerplate from unit tests. That problem is solved well
enough. This document describes what TSpec becomes next, and why.

In an agentic development workflow, generation stops being the bottleneck and verification
becomes it. Code is produced faster than any human can read it, and review is harder than
writing because the reviewer must reconstruct an intent they never formed. The pressure is
therefore always toward reviewing less carefully, until "human in charge" quietly degrades
into rubber-stamping.

The bet: **the artifact a human reviews is neither the code nor the tests, but a system
specification mechanically derived from them.** The human reads a description of what the
system claims about itself, reviews the diff of that description between commits, and owns
the parts of it that state universal rules. The agent writes the implementation and the
example-level tests below that line.

Two properties make this different from an LLM summarising a codebase:

- It is **derived**, not authored — it cannot drift from the code, because it is a function
  of the code.
- It is **deterministic** — the same suite produces byte-identical output, so it can be
  committed, diffed, and enforced in CI.

Readability can be faked by a language model. Determinism cannot. That is the moat.

---

## 2. Principles

These govern implementation decisions. When a design question arises, resolve it against
these rather than by preference.

**Faithful rendering, not judgment.** The framework renders what the tests say and never
opines. A poorly named test renders as a poor sentence, the author sees it in their own
specification document, and the feedback arrives without a lint rule. The same logic covers
gaps: the document shows what *is* specified; the reader notices what is missing.

**Determinism above all.** Nothing that varies between runs may reach the page — not
execution order, not generated values, not sample counts, not timings. A document that
churns is not diffable, and if it is not diffable the whole thesis collapses.

**Claim, not confidence.** Render anything that changes *what is claimed*. Omit anything
that only changes *how sure we are*. "For any Cart with at least one item" is a weaker
claim and belongs in the document. "Verified over 100 cases" is evidence and belongs in
the run report and in failure output.

**Regular structure makes absence visible.** Same sections in the same order for every
method, parallel branches rendered at the same depth, nothing elided because it is sparse.
A missing row in a regular table is loud; a missing sentence in flowing prose is invisible.
Resist compressing thin branches to make the prose read better — that destroys the property
the document exists for.

**No false completeness, no false alarms.** A gap report that misses gaps teaches readers
that gaps are handled. A gap report that flags non-issues teaches readers to skim past
flags. Both are worse than silence. The framework does not assert that something is
missing, because that would require knowing what matters.

**Sublinear growth.** Examples grow one-for-one with the system; a suite twice the size
yields a document twice as long, which relocates the reading bottleneck instead of removing
it. Laws do not — one sentence covers an unbounded set of cases. The health metric for a
specification document is the share of its claims that are universal.

**The own-week test.** Every feature must make the author's own work better this month.
Features built for a hypothetical user are where solo tooling projects die.

---

## 3. The artifact

A committed, per-subject markdown document mirroring the production folder structure.

```
# PricingService

## Total(Cart)

### Always
- the result is never negative
- adding an Item never lowers the result

### Given a Cart with Items priced 10 and 20
- Then the result is 30

### Given an empty Cart
- Then the result is 0
```

`Always` and `Given` carry the modality structurally, so no annotation is needed to
distinguish a universal claim from a witnessed example.

---

## 4. Stage 1 — Specification generation

The near-term work. Delivers value on suites that already exist, with nobody writing a new
test.

The enabling observation: the rendering already exists. TSpec builds a full specification
today and discards it on success. This is a harvest from a normal test run, not a static
analyser over source.

- **Emit.** The pipeline is already built and disposed per test method. At disposal, write
  the rendered fragment to a sink. Do not attempt per-test outcome detection there — emit
  unconditionally and only *publish* the document if the whole run was green. A document
  asserting things that currently fail is worse than no document.
- **Hierarchy.** `Spec<TSubject>` gives the subject; the `When` expression gives the method
  under test; the base-type chain gives the branches (`WithItems` → `GivenCartExists` →
  `WhenPlaceOrder`). The recommended structure in README §6.1 is the document's outline.
- **Assembly.** Sort everything at assembly time; never let xUnit's parallel execution order
  reach the page. Sort by namespace, subject, method, branch path, requirement.
- **Content stability.** Nothing generated may print as a concrete value. "a Cart" is
  stable; `Cart { Id = 3f2a… }` churns the diff on every run.
- **Output.** Markdown, one file per subject, mirroring the production folder structure, so
  a change to one service touches one small file rather than a monolith.
- **Verify mode.** Regenerate and fail CI on any difference. Without this the document
  drifts within a month and nobody notices.

**Deliberately unsolved in v1:** hoisting shared `Given` clauses out of sibling leaves (a
common-prefix problem — ship it flat and look at real output before deciding it needs
solving); source-ordered requirements rather than alphabetical (the one place a source
generator eventually earns its keep); skipped tests (mark or omit, pick one).

**Expect the effort to land in the format, not the plumbing.** Reflection, sorting, emission
and CI verification are mechanical. The rest is generating the document against a real
suite, disliking it, restructuring, and repeating. That loop does not compress, because the
bottleneck is judgment about what reads well, and readability is the entire product.

---

## 5. Stage 2 — Architecture testing

Decided 2026-08-10: architecture testing is next after Stage 1, ahead of the Laws arc (Stage 3
onward) and ahead of the deferred system-wide specification (Stage 7). It shares no machinery
with either, and it has a real target today — both are reasons to sequence it first, not reasons
to skip it.

**What it is.** Rules about the shape of the dependency graph between assemblies, namespaces and
types in the *production* code — not about runtime behaviour. "Entry must never reference Core."
"`IRoomStore` is declared in Core and implemented only in Infra." Checked by static analysis of
compiled assemblies; nothing runs. A different derivation mechanism than Stage 1's test-harvesting,
but the same moat property survives it: the rule is checked against the actual compiled graph, so
it cannot drift the way a paragraph in a CLAUDE.md file can.

**Why it jumps the queue:**

- **A concrete target exists today, and it is currently unenforced.**
  `SampleProjects/MyHotel/CLAUDE.md` states several dependency rules by hand — Entry and Core
  never reference each other, Core takes no dependency beyond Contract that would hurt its
  testability, `IRoomStore` lives in Core and is implemented in Infra. Project references catch
  the coarsest of these; everything finer is convention, policed only by whoever reads the file.
  That passes the own-week test on day one. The Laws arc cannot yet make the same claim — Stage 3
  has no named law and no suite to ride along on until the taxonomy below is more than a table.
- **It is small and self-contained.** No new execution engine, no shrinking, no generation — one
  static pass over compiled assemblies per rule.

**Rendering.** A checked rule renders under `Always`, alongside laws but not merged with them.
Structurally it resembles a Protocol law in the taxonomy below — a claim about the relationship
between components rather than about one outcome — but it does not fit that table's execution
column, since nothing runs. Treat it as its own kind, not a stretch of Protocol, if the taxonomy
is revisited.

**Relationship to Stage 5 (contracts on collaborators).** Different axis. A contract on a
collaborator is behavioural — does this implementation of `IMailSender` actually satisfy
idempotency at runtime. An architecture rule is structural — is this type even allowed to
reference that one. Do not merge the two; a type can violate one and not the other.

**Feeds the deferred system-wide view.** Checking a dependency-direction rule requires walking
the reference graph once. That walk is most of what Stage 7 would need to build a diagram from.
Building it here, against real enforcement, beats building it twice — once to enforce, once only
to describe.

---

## 6. Laws — the taxonomy

Laws are universal claims. They are what makes the document grow sublinearly. Classify them
by the execution shape they demand, because that classification determines the whole
combinator library and will otherwise leak into every part of the design.

| Shape | Holds of | Examples | Execution |
|---|---|---|---|
| **Pointwise** | each individual outcome | `Positive`, `NonNull`, `WithinRange` | rides along on existing tests |
| **Protocol** | the interaction trace | `AtMostOnce`, `PrecededBy`, `NeverAfter` | rides along on existing tests |
| **Sequential** | the series of outcomes across calls | `Monotonic`, `Distinct`, `Increasing` | needs its own generated runs |
| **Comparative** | two executions with related inputs | `Idempotent`, `RoundTrip`, `Commutative`, monotone-in-input | needs its own generated runs |

**The boundary between rows two and three is the cost cliff.** Everything above it reuses
machinery that Stage 1 builds anyway. Everything below it requires a new execution engine.

A further design commitment: **ship a vocabulary of named laws rather than a blank page.**
Property-based testing has been about to catch on for twenty years, and the reason it has
not is that inventing the right invariant is often harder than writing the code. A named
catalogue converts invention into selection — which a junior developer can do, which an
agent can do far more reliably than invention, and which a reviewer can check at a glance.

Business logic that appears to have no absolute invariants usually has *relational* ones.
The correct invoice total may be unknowable in the abstract, but adding a line item must
never lower it.

---

## 7. Stage 3 — Ride-along laws

Pointwise and protocol laws, checked during tests that already run.

No new execution model: the outcome is already captured, and Moq already records the full
interaction trace — TSpec currently only queries it pointwise through `Then<T>`. Declare
rules over the whole trace and every existing test in the suite checks them for free. One
rule, thousands of verifications, no new test code.

This is the compression argument made literal, and it catches the protocol violations that
per-method example tests structurally cannot see.

Laws must render into the specification document under `Always`, or they become
second-class citizens next to the examples and lose most of their value.

---

## 8. Stage 4 — Generative laws *(large, deferred)*

Sequential and comparative laws, each running the recorded pipeline many times.

Feasible because execution is already deferred and the pipeline is a recorded, reorderable
data structure rather than an imperative script — the same arrangement can be re-run over
many bindings without the user writing anything different. Ideally the syntax does not
change at all: `[Fact]` runs the pipeline once, `[Property]` runs it as the universal claim
its English already makes.

Note that `A<Cart>()` is *already* a universally quantified variable that happens to be
instantiated exactly once. The specification text already reads as a universal claim. Only
the execution is existential. Closing that gap is the whole of this stage.

Two hard problems sit here, and together they are larger than everything above:

- **Shrinking.** A shrinker per type, integrated with the `Using<T>().From<S>()` conversion
  pipeline, sequences and semantic types. Subtle enough that a slightly wrong shrinker
  produces confidently misleading counterexamples.
- **Constrained generation.** Making `Satisfies` *steer* generation rather than filter it.
  Rejection sampling is easy and bad.

Do not begin this stage until Stages 1 and 3 are in real daily use.

---

## 9. Stage 5 — Contracts on collaborators *(deferred)*

Laws attach to a **type**, not to a test. This resolves the subject-versus-collaborator
ambiguity: asserting that an auto-generated mock is idempotent is vacuous.

```csharp
public class MailSenderLaws : Laws<IMailSender>
{
    [Law] public void SendingTwiceSendsOnce()
        => Invariant(_ => _.Send(The(message))).Is().Idempotent();
}
```

One declaration, enforced in both directions:

- **Downward**, against every implementation of the interface — does the real thing satisfy
  it?
- **Upward**, against every spec that mocks the interface — has an arrangement been
  configured that contradicts it?

The upward direction is the distinctive one. In agent-authored suites the mirror problem
usually lives in the *arrangement*, not the assertion: a stub configured so `GetCart(id)`
returns a cart whose `Id` is not `id`. The test passes, proves nothing, and reads fine.
Almost nobody validates arrangements, and TSpec owns the mocking layer. A contradicted
arrangement should fail as `SetupFailed` — the concept and the distinction between invalid
setup and unmet assertion already exist.

---

## 10. Stage 6 — Requirements matching *(opt-in, far horizon)*

The specification describes what is *tested*, not what is *required*. Requirements live
outside the code and no analysis of a test suite can recover them.

The wider tooling landscape is splitting along exactly this line. Spec-driven development
tools produce specifications that are **prescriptive and unbound** — authored first, in
natural language, with no mechanical link to the code, so they drift silently. TSpec
produces one that is **descriptive and bound** — derived from executable tests, unable to
drift, but only ever describing what is covered.

Neither is complete, and the gap between them is the unsolved problem in the space. An
opt-in tool that takes a requirements source as input and matches it against the generated
specification is the natural long-term position — and possibly the most defensible thing in
this plan. It is explicitly not MVP material.

---

## 11. Stage 7 — System-wide specification *(deferred)*

Not designed yet. The shape as currently imagined: an aggregate view above the per-subject
documents Stage 1 produces — architecture-level, spanning the whole system, potentially including
a diagram of the dependency graph — rather than a replacement for them. Sublinear growth applies
here too: a system-wide document that just concatenates every per-subject file is not an
improvement.

**Deliberately sequenced behind Stage 2.** Architecture testing already has to extract the
production dependency graph to check its rules; a diagram is close to a side-output of that
extraction rather than a separate effort. Attempting this first would mean building the
extraction twice — once with nothing yet to check it against.

---

## 12. Explicitly out of scope

- **Gap detection from code alone.** Not feasible, and a patchy version is worse than none.
  Three of five enum values specified may be entirely correct; forcing the user to state
  things they do not care about produces noise, and noise trains readers to skim.
- **Naming lint.** The package cannot know what naming convention suits a given domain.
  Document best practice; let the rendered output supply the feedback.
- **Anything that flags a non-issue.** See the principles.

---

## 13. Open questions

Requiring judgment, not to be guessed by an implementer:

1. When a requirement's rendered assertion and its method name disagree, which becomes the
   sentence in the document? (Current lean: the assertion, per faithful rendering — with
   `because` promoted from optional flavour to the primary carrier of intent.)
2. How much repetition of shared `Given` context is tolerable before hoisting is worth
   building? Answer from real output, not in advance.
3. Does the document belong in the test project, the solution root, or a docs folder — and
   what makes the diff most useful in a pull request?
4. Should the diff be classified structurally — claim added / strengthened / weakened /
   removed / rename only — so that a disappearing law raises an alarm mechanically rather
   than relying on a reader noticing a deletion?

---

## 14. Notes for the implementation session

Establish this first, before planning Stage 1 in detail: **how separable is the current
renderer?** If the specification text is produced as a string along the failure path,
generalising it into a structured record that can be sorted, grouped and re-rendered is
straightforward. If it is woven into assertion failure handling, that refactor is most of
Stage 1 on its own. This single fact swings the estimate more than anything else.

Then: build Stage 1 end-to-end against a real suite before adding anything from the Laws arc —
Stage 3 onward. Stage 2 (architecture testing) is next in sequence, ahead of Laws; see its
rationale above. The document has to exist and be looked at before its shape can be argued about
usefully.
