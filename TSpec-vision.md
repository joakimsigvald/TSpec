# TSpec — Long-Term Vision

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

**Claim, not mechanism or confidence.** Render anything that changes *what is claimed*. Omit
anything that only changes *how the test gets there* or *how sure we are*. `int?` claims
something `int` does not, so the `?` stays; `await` claims nothing, so it goes. "For any Cart
with at least one item" is a weaker claim and belongs in the document. "Verified over 100
cases" is evidence and belongs in the run report and in failure output.

**Regular structure makes absence visible.** Same sections in the same order for every
method, parallel branches rendered at the same depth, nothing elided because it is sparse.
A missing row in a regular table is loud; a missing sentence in flowing prose is invisible.
Resist compressing thin branches to make the prose read better — that destroys the property
the document exists for.

**No false completeness, no false alarms.** A gap report that misses gaps teaches readers
that gaps are handled. A gap report that flags non-issues teaches readers to skim past
flags. Both are worse than silence. The framework does not assert that something is
missing, because that would require knowing what matters.

**A test that cannot fail is not a claim.** A test that passes whatever the subject does
reads exactly like one that checks something, and in a suite nobody reads line by line,
nothing tells them apart. So TSpec refuses what it can tell is vacuous — a `When` with
nothing claimed, a check whose result nothing reads, a verification that counts no calls
whatever the subject does — and only that, since a guess would be a false alarm.

**Sublinear growth.** Examples grow one-for-one with the system; a suite twice the size
yields a document twice as long, which relocates the reading bottleneck instead of removing
it. Universal claims do not — one sentence covers an unbounded set of cases. The health
metric for a specification document is the share of its claims that are universal.

**Selected, not accumulated.** Every feature is chosen to serve this vision, and ideally
serves any suite rather than one. Evidence from real projects counts in its favour.

---

## 3. The artifact

A committed `_specification/` folder per spec project: markdown, one file per top-level
folder, rewritten by every full green run. Its headings are the test structure, which mirrors
the production code.

```markdown
## Booking Number Generator

### When next
`Next()`

#### Always
- **record it as the last used**\
  `IBookingNumberStore.SaveLastUsed(Result) was invoked once`

#### Given a number was issued
`IBookingNumberStore.LoadLastUsed() returns 10042`

- **issue the one after it** — `Result is 10043`

#### Given no number was issued yet
`IBookingNumberStore.LoadLastUsed() returns null`

- **issue the one after the seed** — `Result is 10001`
```

That is MyHotel's generated specification with the one change a law would make: the claim
each example made with its own number is stated once, under `Always`. `Always` and `Given`
carry the modality structurally, so no annotation is needed to tell a universal claim from a
witnessed example.

The diff of this folder is the review. Classified by claim — added, strengthened, weakened,
removed, renamed only — it would raise an alarm when a claim disappears, rather than rely on
a reader to notice a deletion.

The effort lands in the format, not the plumbing. The bottleneck is judgment about what reads
well, and readability is the entire product.

---

## 4. Universal claims

Universal claims make the document grow sublinearly, and they are the part the human owns.
They come in two kinds, on different axes: structural, about what the code may depend on, and
behavioural, about what it does. Keep them apart — a type can violate one and not the other.
Both must reach a document, or they become second-class next to the examples.

**Architecture** is the structural kind: rules such as "Entry never references Core", from the
project graph down to the types each project declares and uses. They are checked against the
graph the build actually produced, so they cannot drift the way a paragraph in a CLAUDE.md
can. An architecture spec writes a document of its own — its rules in words, and the graph
they hold over as a diagram. That is the system-wide view: above the per-subject documents,
never a concatenation of them.

**Laws** are the behavioural kind. Classify them by the execution shape they demand, because
that classification determines the whole combinator library and will otherwise leak into
every part of the design.

| Shape | Holds of | Examples | Execution |
|---|---|---|---|
| **Pointwise** | each individual outcome | `Positive`, `NonNull`, `WithinRange` | rides along on existing tests |
| **Protocol** | the interaction trace | `AtMostOnce`, `PrecededBy`, `NeverAfter` | rides along on existing tests |
| **Sequential** | the series of outcomes across calls | `Monotonic`, `Distinct`, `Increasing` | needs its own generated runs |
| **Comparative** | two executions with related inputs | `Idempotent`, `RoundTrip`, `Commutative`, monotone-in-input | needs its own generated runs |

The boundary between rows two and three is the cost cliff. Above it, nothing new has to run:
the outcome is already captured, and TSpec's mocks record every call they receive, though
`Then<T>` asks about one call at a time. Declare a rule over the whole record and every
existing test checks it — one rule, thousands of verifications, catching the protocol
violations that per-method examples structurally cannot see.

Below it lies a new execution engine. `A<Cart>()` is already a universally quantified
variable that happens to be instantiated exactly once: the specification already reads as a
universal claim, and only the execution is existential. Closing that gap — `[Fact]` runs the
pipeline once, `[Property]` runs it as the claim its English already makes — is the largest
piece of this vision, because it needs shrinking, and generation that a constraint steers
rather than filters.

Ship a vocabulary of named laws rather than a blank page. Property-based testing has been
about to catch on for twenty years, and the reason it has not is that inventing the right
invariant is often harder than writing the code. A named catalogue converts invention into
selection, which an agent does far more reliably than invention and a reviewer checks at a
glance. Business logic that appears to have no absolute invariants usually has relational
ones: the correct invoice total may be unknowable in the abstract, but adding a line item
must never lower it.

Laws attach to a **type**, not to a test — asserting that an auto-generated mock is
idempotent is vacuous. One declaration is then enforced in both directions: downward, against
every implementation of the interface, and upward, against every spec that mocks it. The
upward direction is the distinctive one. In agent-written suites a vacuous test usually hides
in the arrangement, not the assertion: a stub configured so `GetCart(id)` returns a cart whose
`Id` is not `id`. The test passes, proves nothing and reads fine. TSpec owns the mocking
layer, so an arrangement that contradicts a contract can fail as `SetupFailed`.

---

## 5. Descriptive and bound

Spec-driven development tools produce specifications that are **prescriptive and unbound** —
authored first, in natural language, with no mechanical link to the code, so they drift
silently. TSpec produces one that is **descriptive and bound** — derived from executable
tests, unable to drift, but describing only what is covered. Neither is complete:
requirements live outside the code, and no analysis of a test suite recovers them.

---

## 6. Out of scope

- **Gap detection from code alone.** Not feasible, and a patchy version is worse than none.
  Three of five enum values specified may be entirely correct; forcing the user to state
  things they do not care about produces noise, and noise trains readers to skim.
- **Naming lint.** The package cannot know what naming convention suits a given domain.
  Document best practice; let the rendered output supply the feedback.
- **Anything that flags a non-issue.** See the principles.
