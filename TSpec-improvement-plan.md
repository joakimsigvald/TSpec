# TSpec improvement plan - from the M5 migration (TSpec 2.5.0, September 2026)

Everything the M5 team wished for while rewriting 38 test files (1130 claims) into TSpec: Joakim's notes, the
"For TSpec" and "Suggestions for TSpec" lists in `Epic-TSpec-and-architecture-tests.md`, and the PR review of
2026-09-09. Written for a Claude session in the TSpec repository: each item states the problem as M5 met it, a
proposal, and what "done" looks like. M5 is the acceptance suite - every item names the M5 spec that shows it,
and `Core.Spec`/`Integration.Spec` (1124 + 14 green on 2.5.0) must stay green after each change.

Vocabulary: a *pick* is `Has().OneItem(pred).that`; a *trainwreck* is a member chain inside `And(x)`, which TSpec
refuses ("No trainwrecks in Then/And") - the idiom is `.And(root).Member.Is(...)`; *gating* is running the
pipeline before reading a tag (`{ get { Then(); return The(tag).X; } }`).

Priority: P1 = wrong results or silently passing asserts; P2 = API gaps that forced hand-written workarounds;
P3 = generation and infrastructure; P4 = rendering polish. Within a group, in suggested order.

## P1 - Correctness

### 1. `Does().Contain(a).And(b)` asserts nothing about `b`
DONE in 2.6.0. `And(b)` after any chain is a legitimate subject switch (`.Is(1).And(other).Is(2)`), so a
compile error was not an option; the fault is a subject nothing is asserted on. TSpec now checks at teardown,
for every test that provided a When, that something was claimed: `Then(subject)`/`And(subject)` with nothing
asserted on it fails with a message stating the rule, and a test that asserts nothing at all fails too. That
forbids a bare `Then()`, which passed even when the act threw; `Then().Completes()` is the explicit
spelling and no `Pass()` was added. A test where a SetupFailed was raised is exempt, and a spec TSpec built
into another spec's subject graph is checked by neither.

Both `DoesNotThrow` forms went obsolete in the same version. An act either completes or throws, so the
parameterless one was a negative name for a positive fact and `Completes()` renders it as one. The generic
`DoesNotThrow<TError>()` had no sound reading: as written it passes when the act throws anything else — every
wrong outcome but one — and made to imply completion it states nothing `Completes()` does not, since
completing already entails throwing nothing. Its one call site in the repo was a rendering test riding on an
act that threw, and it is gone.

### 2. A tag read before the pipeline ran returns the type default silently
DONE in 2.6.0, as (c) — detection rather than a syntactic rule. The defect is wider than a Then: any read
before arrangement yields a placeholder, so `Given(b).Is(The(a) + 1)` was silently wrong too. TSpec now
records every slot read before the pipeline is arranged, and throws where an arrangement REPLACES what was
read, naming it: "the tag '_name' was read before the pipeline was arranged, so the read yielded a generated
value rather than the one arranged for it. Run the pipeline with Then() before reading it".

Replacement is the whole test, and getting there took two passes. "Read then arranged" is too crude: it
condemns `Given(Many<T>())`, where the read feeds the arrangement and the same values are stored back, and it
condemns an arrangement that mutates a value in place, where the read reference still points at the object the
pipeline uses. Both keep the test holding the right thing. So the check sits at the one place every value is
stored, compares what lands against what the slot already held, and runs only while arrangement is running —
after it, a fresh mention that grows a collection is not an overwrite of anything the test relied on.
(a) was rejected because the recommended structure puts `When` in an outer constructor and `Given` in nested
ones, so a read from a Given constructor would run the pipeline mid-arrangement; (b) was rejected as
unintuitive about who runs the lambda and when. M5's gated getters stay as they are — `Then(); ...` is now the
stated idiom, and the failure is loud instead of silent.

Also closed with it: a lambda handed to `Then(subject)`/`And(subject)` used to bind as a `Func<>` subject and
render as one. It is refused now, on the same ground as (b) — running the pipeline does not affect a lambda.

### 3. Two `Using(() => null)` factories render as two indistinguishable lines
DONE in 2.6.0, by rendering the type. There is no dedup by text: both lines are recorded, composed and
written to the document — verified on the chained, separate-statement, constructor, `For.Subject` and
value forms, and on a document rendered from two identical clauses. What M5 saw is that both lines read
the bare word "null", which names no type: two null factories for two dependencies stated one word each,
so a reader could not tell which was arranged, or that two were.

A null now reads as the type it stands for. `default(T)` already rendered "default T", so a cast over
null — which states the same fact and nothing else — renders "null T" rather than "(T)null", everywhere
a value is described: "When null DateTime?", "Given IMyValueIntRepo returns null int[]?". Other casts
still print as written, since only over null does a cast say nothing but the type. Where the spec wrote
a bare null and there is no cast to read the type from, `Using` supplies the type it is using the value
for. Both spellings meet at one, and it is the one the rest of the Using family already uses.

## P2 - Mocking and subject construction

### 4, 5, 6. Set up a mocked call by METHOD NAME — as steps
A first attempt at all three at once was written and REVERTED on 2026-09-10: it worked for the cases
it was written against and broke on four it was not, and it grew `GivenThatCommonContinuation` to 368
lines of `object`-typed Moq dispatch. The patch is not kept; what it established is, because it was
established by probing Moq rather than by reasoning, and it does not need re-deriving.

**What is known.** Every setup route ends at the same Moq type, `IReturnsThrows<TService, TReturns>`:
`mock.Setup(expr)`, `mock.Setup(expr built by reflection)`, and `mock.Protected().Setup<T>(name,
matchers)` alike. So the outcome vocabulary — `Returns`, `Throws`, `Tap`, `First`/`AndNext` — needs no
change whatever names the call. `Protected()` REFUSES a public member ("Method X is public. Use
strong-typed Expect overload instead"), so accessibility has to pick the route, but it picks it below
the API. `Callback` works on a protected setup, so `Tap` reaches a protected member. Moq needs one
`ItExpr.IsAny<T>()` per parameter, so the parameter types must be found by reflection on either route.
Covering N overloads means N Moq setups; since item 7 they share one queue, so "the first call" is the
first call to the method whichever overload took it.

**What broke, and is not to be repeated.** An `out`/`ref` parameter crashed with a raw
`ArgumentException` ("The type 'System.String&' may not be used as a type argument") — no `SetupFailed`,
no method name. `ReturnsDefault()` failed on a by-name void call where the expression form works, since
`That("X")` on a `Task`-returning method gives `TReturns = Void` while `That(_ => _.X())` gives
`TReturns = Task`. A property (`get_Name` is the real name) and a generic method (whose return type is
`T`) both failed with "has no method 'X' returning Y", which reads as a typo rather than as an
unsupported kind of member. A non-virtual protected member failed with Moq's raw "Unsupported
expression" text, never saying it must be virtual.

**Decided.** Return type matches EXACTLY — so by-name and expression forms are NOT interchangeable
where a method returns a subtype of what the test asks for, and that is accepted. A name covers every
overload of it, as a type-wide `Returns` covers every method. A from-arguments `Returns` states a
signature and narrows the name to the overload matching it. The rendering names the method and no
arguments — "Given IChat.Complete returns …" — as a verification by name already reads.

#### Step 1 (item 4) — DONE in 2.6.0. Protected members only.
`Given<TService>().ThatProtected<TReturns>(name)`, and `ThatProtected(name)` where the member answers
with nothing. The name is deliberate: general setup by name is NOT implemented, so a spelling that
promised it would have lied. Everything Moq-specific lives in `ProtectedMember`, one file, since Moq
may be replaced later — what it hands back is an ordinary setup, so `Returns`, `Throws`, `Tap`,
`First`/`AndNext` know nothing about how the call was named. The rendering says what it says for any
other call: accessibility is a fact about the mock, not about the behaviour.

Refused, each naming the limit it met rather than reporting a missing member — every one of these was
a case the reverted attempt got wrong: an OVERLOADED member (a name states no arguments, so nothing
could say which overload was meant, and setting up all of them is a guess the test never made), a
GENERIC member (a name carries no type argument), a member taking a parameter by REF or OUT, a member
that is not virtual or abstract (nothing can intercept it), and a PUBLIC member (pointed at the
expression form). The return type is matched exactly, and where it does not match the failure says
what the member actually returns. A protected PROPERTY works — Moq reaches one by name too.

Two things this deliberately does NOT do, recorded so they are not re-litigated: an overloaded
protected member cannot be set up at all, and a setup states nothing until an outcome is given it, so
a refusal is raised when `Returns` is stated rather than when the member is named.

Found and fixed alongside it: `ReturnsDefault()` failed on ANY call answering with nothing, named or
written as an expression, since Moq's void setup has no Returns to ask. The default of nothing is
nothing, so it now states what `Returns()` states. That bug predated all of this.

ORIGINAL PLAN:

#### Step 1 (item 4) — protected members only. The one with the evidence behind it.
`Integration.Spec/OpenAi/OpenAiChatCompletion/WhenComplete` keeps a hand-written recording fake because
`HttpMessageHandler.SendAsync` is protected and no lambda can name it. This is the only part of 4/5/6
with a named workaround waiting on it. Scope: `That<TReturns>(name)` / `That(name)` resolving NON-PUBLIC
virtual or abstract members through `Protected()`. A public member named this way is refused, pointing at
the expression form that already works for it — which keeps the "are the two forms interchangeable"
question out of this step entirely. Refuse `out`/`ref`, properties and generic methods by name, each
saying what is actually unsupported. Say "must be virtual or abstract" where Moq cannot intercept.
Done when that fake is deleted and the handler is a TSpec mock with a `Tap`.

#### Step 2 (item 5) — public members by name. Optional; weigh before starting.
`Any<T>()` shipped in 2.5.0, so the `It.IsAny` wall this item was written about was already half gone
when it was written — M5 could have written `Any<Prompt>(), Any<Options>()`. What remains is brevity and
overload coverage, against building expressions by reflection and the exact-vs-assignable mismatch with
the expression form. Skippable. Do it only if a real spec is worse without it. M5's done condition, if
it is taken: `WhenGenerate`'s and `WhenAnswer`'s arranges lose their `It.IsAny` lists.

#### Step 3 (item 6) — sequences by name. Falls out; tests only.
`That(name)` returns the same continuation, so `First()`/`AndNext()` are already on it. Nothing to build
after step 1 or 2 but the tests that pin it. M5's complaint was that the matcher-free form existed only
for single returns, never for a sequence.

### 22. `Tap(a).Tap(b)` silently drops the first tap
DONE in 2.6.0. Moq's `Callback` keeps one callback per setup, so a second `Tap` replaced the first
rather than joining it — the tap ran, the earlier one just never did. The sequence path already
composed its steps' taps, so the two paths now share that: a tap is folded into the taps before it,
and outside a sequence the folded action is what reaches Moq. The stated text was wrong the same way
and for the same reason — only the last continuation carried a tap expression — so a continuation now
carries the list, and every tap is stated, in order:

    Given IMyValueIntRepo.Get(any int) tap(() => _seen.Add("first"))
          tap(() => _seen.Add("second")) returns "x"

It predated item 7, which only made `Tap` reachable in more places.

### 7. Observe a sequence: `Tap` and from-arguments `Returns` on `First()/AndNext()`
DONE in 2.6.0, by TSpec owning the sequence. The cause was that `First()` switched to Moq's
`SetupSequence`, whose `ISetupSequentialResult<T>` is not a setup and has no `Callback` — so a
sequence could state what each call answers or what it was asked, never both. A sequence is now a
TSpec queue behind ONE ordinary `Setup`, driven by `Returns(() => queue.Next())` (a call answering
with nothing is driven by the callback instead, since it has no `Returns` to ask). `Tap` and
`First` moved to `IGivenThatCommonContinuation`, so a tap reads the same before `First` as after
it, and a tap inside a sequence belongs to the step it precedes — it fires on the call that step
answers and no other. Past its last step a sequence still answers with the type's default, as
Moq's own did. The `ISetupSequentialResult` half of the dispatch in `GivenThatCommonContinuation`
is gone, and both derived continuations lost their duplicated `Tap` overloads: one
`Callback(InvocationAction)` reports every invocation, so a step of any arity reads its own call.

Left open: the from-arguments `Returns` overloads work on a sequence in the implementation, but are
declared on `IGivenThatContinuation`, which `First()`/`AndNext()` do not return. Exposing them
means either putting them on the common continuation — where they are meaningless for a void call,
whose `TReturns` is `Void` — or giving `IGivenThatReturnsContinuation` a type parameter so
`AndNext()` can return the specific continuation. `Tap` alone meets this item's done condition, so
the choice was not forced here.

ORIGINAL REPORT:
`Tap` and the from-arguments `Returns` live on the unsequenced continuation; `First()` moves to a continuation that
has neither, `AndNext()` has none. So a scripted chat whose calls must be read back is a TAG holding the script
behind ONE from-arguments `Returns` that also records the call (ReportPackageGenerator, ReportAssistant specs).
Proposal: `Tap` on the sequence, or a TSpec-owned `Captured<T>` readable after the run. Done when
`Given(replies).Is([ABrokenPackage, TheDemoPackage])` can become a `First().Returns().AndNext()` sequence with
the sent messages still readable.

### 8. `Using` by PARAMETER NAME, and "leave this optional argument at its default"
A subject with two same-typed primitive ctor args (`apiKey`, `model`) cannot be generated - TSpec cannot tell them
apart by type - so it is built by a factory. An optional ctor arg (`reasoningEffort`, `logUsage`) TSpec would GENERATE
must be pinned by a null factory per type. Proposal: `Using("sk-test", For.Parameter("apiKey"))` and a way to say
"default" for a named optional arg. Related, Joakim: HONOUR the constructor's default values when auto-generating
an object instead of generating a value for every parameter. Done when `WhenComplete`'s `Using` factory goes.

#### 8a. Honour the constructor's default values — DONE in 2.6.0

Building the subject, a parameter that declares a default is filled only with what the TEST said —
a `Using` value or factory, a registered conversion or value space, or a mock the test has already
set up. Where the test said nothing, the default stands, and nothing TSpec would otherwise INVENT
reaches such a parameter: `Svc(IRepo repo, bool logUsage = false, int retries = 3)` is built
`false`/`3` where it used to come out `True`/`2`.

No precedence rule of its own was needed. `DataGenerator.TryCreateFromSetup` asks the setup-driven
strategies (`TypeConversionStrategy`, then `DefaultStrategy`) and stops there, so a
`Using<int>().From<int>().StartingAt(6)` value space beats a default of 3. The rejected alternative
was "take the default when it happens to fall inside the space", which would turn behaviour on a
coincidence between two unrelated declarations.

**An optional parameter of a MOCKABLE type gets the mock only if the test arranged one**
(`MockRegistry.HasMock`), and is null otherwise. Read strictly, an unarranged mock is not something
the setup asked for. The accepted cost: arranging it after the act comes too late, since the subject
is constructed once every arrangement has run — a spec that only VERIFIES an optional dependency
verifies a mock that was never injected.

**Subject scope only** (`For.Subject`) — which is the whole subject constructor graph, not just the
SUT's own constructor: a component the subject takes as an argument is built the same way. Test data
from `A<T>()` is still generated, defaults and all. Re-asked and re-confirmed: an input model is a
WITNESS, and a witness has to be distinguishable. Honour `Ref = ""` there and
`Result.Ref.Is(The<Order>().Ref)` passes against an empty string while proving nothing — the subject
gets more truthful, the input gets weaker.

A property fed by a honoured default is no longer refilled by
`ObjectStrategy.PopulatePublicProperties`, which had been half-undoing the feature on a record
subject: `Size = 7` survived, `Verbose = false` and `Label = null` were replaced, because only a
value equal to the type's zero looks like an empty slot.

A null default on a non-mockable type IS honoured (`string? connection = null` becomes null): if the
subject dereferences it that is an NRE where the test used to pass, but it was passing on a fiction
production would not produce. Constructor choice is unaffected — greediest still wins. A `params`
array and an `[Optional]` parameter with no value have no `HasDefaultValue` and stay generated.
Reflection reports the default of a struct that is not a primitive as null, so `DateTime stamp =
default` is turned back into the zeroed value before the constructor sees it.

The blast radius was behavioural and silent — no specification text changed, in Core.Test or in
either MyHotel suite.

### 9. A deviation passed as a ctor arg is invisible in the rendered Given
Design consequence, recorded so it is not re-asked: M5 decided a per-Given deviation is a Tag with a `Using` type
default, never a ctor arg (TESTING.md 5.3). If TSpec renders ctor args of the Given class one day, revisit.

## P3 - Generation and infrastructure

### 10. Find the production project by static reference, not by folder layout
The OUTPUT FOLDER half is DONE in 2.6.1. The two mechanisms this item bundles are independent, and only
one of them was broken: `deps.json` sits beside the binaries and travels with them, so subject resolution
was never affected by where the build wrote them. What broke was `ProjectDirectory`, which inferred the
SOURCE tree from the BINARIES — the one thing a build is free to relocate.

Reproduced, both ways, before touching anything. Copying `MyHotel.Spec`'s output to a folder with no
`.csproj` above it failed all 52 tests in the fixture constructor, on a message about project layout
rather than about any test. The quieter one is worse: with the output under
`fake/artifacts/bin/MyHotel.Spec/debug` and an unrelated `Unrelated.csproj` at `fake/`, the suite went
GREEN and wrote a full `_specification/` into the unrelated project, every link rendered
`../C:/Development/…`. That is the `UseArtifactsOutput` default layout in any repo whose root is a project.

The spec classes' own source files are what says where the project is. `SpecClasses.SourcesOf` reads them
from the PDB — the same read the heading links already do — and the project is the nearest `.csproj`
above the directory MOST of them are in, so a linked file or one from a shared project is outvoted rather
than followed, and the outermost wins a tie. The binaries walk stays as the fallback for a build that
wrote no debug information; where NEITHER answers, nothing is written and the reason is printed, rather
than failing the run — decided 2026-09-10: no test claims the specification, so nothing fails over it.
Naming or referencing the subject wrongly still fails before the first test, which is a rule about the
spec project itself.

Found while proving it and fixed with it: `SourceLink.FoundUnder` could match a file outside the root.
The whole of an absolute Windows path is a tail of itself, and `Path.Combine(root, "C:\…")` gives the
rooted path back — so any file that existed anywhere on the drive was linked as `../C:/…`. A rooted tail
is rejected now.

One correction to what this item said: "the assembly name must equal the project name" does not hold.
`deps.json` keys projects by ASSEMBLY name throughout — TSpec's own `Core.csproj` appears as `TSpec/2.6.0`
— and `ProjectReferences` compares assembly name to assembly name, so it is consistent. The project FILE
name only ever mattered to the walk-up that is now gone. `SubjectDescription` is the one place that
assumes the two agree, and it degrades to no description rather than failing.

STILL OPEN, and unrelated to the build layout: derive the SUBJECT from the direct project references
rather than from the naming rule — where there is exactly one direct project reference, use it and keep
the suffix rule only as a tiebreak. Worth checking what it buys first: `MyHotel.Spec` has three direct
project references (`MyHotel`, `MyHotel.Contract`, `TSpec`), and two even once TSpec is a package
reference, so the "exactly one" case only helps a two-project solution.

### 11. Write `_specification/` with the checkout's line endings
The generator writes LF; on an autocrlf checkout every regenerated file shows as modified in `git status` even when
its content is unchanged (ten files "modified" for a one-file change, 2026-09-09).

**Decided:** preserve the existing file's line ending where the file exists, LF otherwise. Small; do it.
No `core.autocrlf`/`.gitattributes` reading — the file already on disk is the answer.

### 12. `Integration.Spec` renders into the project-root file instead of its folder file
Its one spec in `OpenAi/OpenAiChatCompletion/` rendered into `Integration.md` with the subject block at the top of the
README, where `Core.Spec`'s folders each get their own file (`Engine.md`, `Workbench.md`). RootNamespace comes from
`Directory.Build.props` in both.

**Cause found.** Files are grouped by NAMESPACE, not by folder. The root depth is the assembly name's segments
when every namespace starts with it, and otherwise the common prefix of all namespaces. With a RootNamespace from
`Directory.Build.props` and a single spec, that common prefix is the whole namespace, so the area comes out empty
and the spec lands in the root file. `Core.Spec` has several namespaces, so its prefix stops earlier and it works
by luck. **Fix:** derive the area from the source path relative to the spec project — which is what the docs
promise — with the namespace as fallback when there is no PDB. The path is available now that item 10 finds the project
from the source files, but this did NOT ride along with it: regrouping the files is a rendering change and
wants a before/after render on both MyHotel suites before it is pinned.

### 13. README: the project description verbatim, and a component list per sub-domain
The csproj `<Description>` is pasted as-is: a multi-line element arrives with its indentation (M5 flattened its
description to one line to work around it). And Joakim's wish: list
the main components (top five by claims, say) of each sub-domain under its row in the README, so the README is a
map and not only a count table.

**Decided:** trim and re-flow the description — small, do. The component list is a RENDERING decision and the PO
owns it: the recommendation is yes, top subjects by claim count under each row, shown as a before/after render
before anything is pinned.

## P4 - Rendering

### 14. Render the When/Given class doc-comment as section prose
The single biggest legibility win: every cryptic passage in the review traced to context that lives in a class
comment the generator drops (the transition-matrix comment on `WhenApplyARemappingEdit`, the "editing the base
population" comment on `WhenApplyPopulationEdits`). The sentence exists in the code; it needs a rendering channel.
Names and `Because` carry a lot but not the section-level "what this component is for". The `///` or
`/* */` comment directly above a When or Given class renders as the paragraph under its heading.

**Decided: read the SOURCE FILE, not the XML doc file.** The XML file would need documentation generation enabled
in every spec project and brings CS1591 noise; the PDB already tells us the file and the class's constructor line,
so reading the source is zero configuration and works today. Do it — the biggest legibility win of the P4 set.

### 15. Suppress the subject/return header when it adds nothing, or let the spec name it
"Subject under test: string / Return type: string" is noise for static-function subjects; a ValueTuple subject
renders as `ValueTuple<ReportPackage, IReadOnlyList<string>>` where the code says `(Package, Notes)`.

**Decided: do two of the three.** Omit the block when both types are primitive or string, and render tuple element
names — the compiler does emit them on the class for a base type. The third, letting a spec declare a display
name, is dropped: new API surface for a rendering nicety.

### 16. An ordering hint for sections and Givens
Alphabetical order is deterministic but puts the happy path last in a rule catalogue ("compiles clean" after the
refusals). Proposal: an optional order attribute or a "first" marker on a Given; alphabetical stays the default.

**Decided: NOT now.** It is new attribute surface that exists only for rendering, and alphabetical is a contract
users can predict. Revisit after item 14 lands, if M5 still wants it.

### 17. A verbatim identifier (`string @lock`) defeats the arrangement stripping
A `[Theory]` parameter named with `@` (a keyword) rendered the claim with its whole
`Given(locks).Is(@lock).Then()...` prefix where a plain name renders "Result.Succeeded is false and ...".

**Decided:** strip the `@` in the preprocessor, before matching. Trivial; do it.

### 18. Picks: render a picking helper by its name, keep indexers, stop the duplicated prefix
- A helper that picks (`Column(alias) => TheColumns.Has().OneItem(c => c.As == alias).that`) expands its whole inner
  chain at every use, with the lambda's PARAMETER NAME ("has one item c.As == alias that Column("hba1c").Kind is").
  Render the helper by its own name once (`column "hba1c".Kind is ...`).
- A helper call is humanized only when it is the WHOLE actual (`Chart("x")` reads "chart "x" is like"; `Column("sbp").Kind`
  keeps its source text).
- An indexer after a pick is dropped: `Result.Has().OneItem().that[ColumnAt("hba1c")].Is(82m)` reads "Result has one
  item that is 82m".
- Two picks over one base pick render the base twice ("Root is a CohortGroup that" x 2; "package.Query.Population is a
  InlinePopulation that package.Query.Population is a InlinePopulation that Children is equal to ChildrenBefore").
- A pick through a protected property renders both the inner assertion and the property name.
- Article: "a InlinePopulation" -> "an".

**Decided: one sub-bullet at a time, each with a before/after render** — the article, the duplicated base pick and
the dropped indexer first. "Render the helper by its own name" is PARKED until the others are done: it is the hard
one, because the helper EXECUTES its inner `Has().OneItem` and that call records itself.

### 19. Names and literals
- The humanizer splits a digit inside a word: `MeanHba1c` reads "mean hba 1c".
- A `const` renders by NAME wherever it appears ("contains TheChart"); a computed count renders expression AND value
  (`has count 'ChartsBefore.Count - 1' = 3`). Render the value, or both consistently.
- A humanized call nested in a list literal loses its parentheses: `[CallFor("x", Args), Demo]` renders
  "[call for "x", Args, Demo]" - three items where there are two.
- A single-line raw string literal keeps one `"` per side, so its inner quotes read as unescaped; a multi-line raw
  string renders its lines but keeps one `"` of the closing delimiter (`... ``` "]`).
- The from-arguments `Returns((a, b, c) => F(a))` overloads have no caller-expression parameter, so the
  clause states no answer at all. Reported against 2.6.0 on 2026-09-10 as a rendering DIFF from 2.5.0, and
  confirmed by probe — `Returns<int>(a => $"{a}")` renders `returns "{a}"`, `Returns((int a, int b) =>
  $"{a + b}")` renders `returns` and stops.

  One defect, two symptoms, and 2.6.0 changed which one shows. In 2.5.0 each overload called
  `continuation.Returns(() => retVal)` with no expression argument, so the INNER `Returns`'s
  `[CallerArgumentExpression]` captured the source text of TSpec's own internal lambda: the
  specification read "returns retVal", naming a private variable that appears nowhere in the test.
  2.6.0 folded the five overloads into one `Computed(...)` (item 7's sequence work) which passes the
  expression explicitly, and it is `null` for arities 2 to 5 — so the leaked word became nothing.
  Neither states what the test arranged; the empty one at least says nothing false. There is no
  rendering test for arities 2 to 5, which is why both slid.
- A class folder two levels down joins the sub-folder into its heading ("Calc Calc Expression").
- `default(T)` mangles a type that is not a bare name: `default(DateTime?)` reads "default DateTime?)",
  with a stray closing paren, and `default(List<int>)` reads "default list int". Found while doing item
  3; `default(DateTime)` and the cast form of both are correct, so it is the `default(...)` parse.

**Decided:** the digit split, the list-literal parentheses, the raw strings, the `default(...)` parse and the
heading two levels down are all parser or humanizer fixes — do them. The from-arguments `Returns` overloads for 2
to 5 arguments simply lack the caller-expression parameter that the 1-argument one has: trivial, do — add it to
the five interface overloads and pass it through `Computed`, and pin all five with a rendering test so the
clause cannot go quiet again. Every `Tap` arity already has the parameter, so the fault is confined to these five. `const` by
NAME versus VALUE is a policy call for the PO; the recommendation is to render the value.

### 20. `Does()` chains: wording and failure messages
`.and.not.Contain(x)` renders "and not contain x" where the sentence is "and does not contain x"; a failure after
`.and.` loses the actual's name ("Expected  to contain"). Same code path as item 1.

**Cause and fix.** The continuation blanks the actual's expression deliberately, so the specification does not
repeat it — but the FAILURE MESSAGE reads the same field. Keep the name for the message and blank it only for the
specification. Do it.

### 21. Point the trainwreck error at the idiom
DONE in 2.6.0. The message names the verb, the expression, and the rewrite: "No trainwrecks in And:
'Result.Length' chains a member on its subject. Hand over the root and chain the rest after it: And(Result).Length".

## Suggested order of work
1. ~~Items 1, 2, 3 (P1)~~ — done in 2.6.0, with 7 and 21.
2. Remaining P2: item 8's `For.Parameter` half — 8a shipped in 2.6.0. Steps 2 and 3 of 4/5/6 (setup by name in the general case) are NOT planned —
   too complicated for the value, and a possible move off Moq would reopen the design anyway. Steps 2 and 3 of 4/5/6 are skippable — decide after step 1 lands, not before.
3. Items 10, 11 (P3) - build-layout independence DONE in 2.6.1; item 11 (line endings) next, then 12,
   which no longer rides along with 10 — locating the project by source file does not by itself change
   how files are GROUPED, and regrouping by folder is a rendering change to show before/after. Then 13.
4. Items 14, 15 (P4) - the rendering changes that change how a specification READS; then 17-20 as polish.
   Item 16 is deferred by decision, not by order: revisit only after 14 has landed.

Lesson from the reverted 4/5/6 attempt (2026-09-10): a mocking change is only as good as the member
kinds it was tried against. Before claiming one works, probe it against a property, a generic method,
an `out` parameter, an overload set, a non-virtual member and a `Task`-returning void — the six that
broke it. Each step above lands on its own, with the suite green, before the next is started.

After each item, regenerate M5's `_specification/` and diff it: the rendering items are done when the named
passages read as the spec is written, the correctness items when the M5 workarounds can be removed.
