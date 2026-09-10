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
forbids a bare `Then()`, which passed even when the act threw; `Then().DoesNotThrow()` is the explicit
spelling and no `Pass()` was added. A test where a SetupFailed was raised is exempt, and a spec TSpec built
into another spec's subject graph is checked by neither.

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

### 4. Set up a PROTECTED member on a generated mock
`HttpMessageHandler.SendAsync` is protected, the common case for every HTTP adapter; TSpec's `Given<T>().That(...)`
cannot reach it, so `Integration.Spec/OpenAi/OpenAiChatCompletion/WhenComplete` keeps a hand-written recording fake.
Proposal: Moq's `Protected()` surfaced through TSpec, or `TheMock<T>()` giving the `Mock<T>` behind `The<T>()`.
Done when that fake can be deleted and the handler is a TSpec mock with a from-arguments `Returns`.

### 5. Argument-blind setup by method name
Verification by name exists (`Then<T>("Method", Times.Once())`); setup does not. Every setup needs
`.That(_ => _.Method(It.IsAny<A>(), It.IsAny<B>(), ...))`, and the `It.IsAny` wall drowns the arranges (WhenGenerate,
WhenAnswer). Proposal: `Given<T>().That("Method").Returns(value)` and `.Returns((A a, B b) => ...)`, argument-blind,
mirroring the verification form. Done when WhenGenerate's arranges lose their `It.IsAny` lists.

### 6. Argument-blind SEQUENCES
`Given<T>().First().Returns(a).AndNext().Returns(b)` forces the full matcher signature; the service-wide default form
exists only for single returns. Proposal: the same name-based or matcher-free form for sequences.

### 7. Observe a sequence: `Tap` and from-arguments `Returns` on `First()/AndNext()`
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

### 9. A deviation passed as a ctor arg is invisible in the rendered Given
Design consequence, recorded so it is not re-asked: M5 decided a per-Given deviation is a Tag with a `Using` type
default, never a ctor arg (TESTING.md 5.3). If TSpec renders ctor args of the Given class one day, revisit.

## P3 - Generation and infrastructure

### 10. Find the production project by static reference, not by folder layout
TSpec finds the project a spec assembly describes by walking up from the binaries and stripping one suffix
(`Core.Spec` -> `Core`), so an out-of-tree build (`--artifacts-path`) fails EVERY spec, and the assembly name must
equal the project name. M5 lives with "always build in tree" and a Directory.Build.props note. Proposal: resolve
the described project from the spec assembly's static references (the `ProjectReference`, or the referenced
assembly's location), so build layout and assembly naming stop mattering.

### 11. Write `_specification/` with the checkout's line endings
The generator writes LF; on an autocrlf checkout every regenerated file shows as modified in `git status` even when
its content is unchanged (ten files "modified" for a one-file change, 2026-09-09). Proposal: preserve the existing
file's line endings, or honour `core.autocrlf`/`.gitattributes`.

### 12. `Integration.Spec` renders into the project-root file instead of its folder file
Its one spec in `OpenAi/OpenAiChatCompletion/` rendered into `Integration.md` with the subject block at the top of the
README, where `Core.Spec`'s folders each get their own file (`Engine.md`, `Workbench.md`). RootNamespace comes from
`Directory.Build.props` in both. Cause not found on 2026-09-08 - reproduce with a one-folder spec project.

### 13. README: the project description verbatim, and a component list per sub-domain
The csproj `<Description>` is pasted as-is: a multi-line element arrives with its indentation (M5 flattened its
description to one line to work around it). Proposal: trim and re-flow the description. And Joakim's wish: list
the main components (top five by claims, say) of each sub-domain under its row in the README, so the README is a
map and not only a count table.

## P4 - Rendering

### 14. Render the When/Given class doc-comment as section prose
The single biggest legibility win: every cryptic passage in the review traced to context that lives in a class
comment the generator drops (the transition-matrix comment on `WhenApplyARemappingEdit`, the "editing the base
population" comment on `WhenApplyPopulationEdits`). The sentence exists in the code; it needs a rendering channel.
Names and `Because` carry a lot but not the section-level "what this component is for". Proposal: the `///` or
`/* */` comment directly above a When or Given class renders as the paragraph under its heading.

### 15. Suppress the subject/return header when it adds nothing, or let the spec name it
"Subject under test: string / Return type: string" is noise for static-function subjects; a ValueTuple subject
renders as `ValueTuple<ReportPackage, IReadOnlyList<string>>` where the code says `(Package, Notes)`. Proposal:
omit the block when subject and return are primitives or identical to the When's generic arguments; render tuple
element names; or let the spec declare a display name.

### 16. An ordering hint for sections and Givens
Alphabetical order is deterministic but puts the happy path last in a rule catalogue ("compiles clean" after the
refusals). Proposal: an optional order attribute or a "first" marker on a Given; alphabetical stays the default.

### 17. A verbatim identifier (`string @lock`) defeats the arrangement stripping
A `[Theory]` parameter named with `@` (a keyword) rendered the claim with its whole
`Given(locks).Is(@lock).Then()...` prefix where a plain name renders "Result.Succeeded is false and ...". Strip the
`@` before matching.

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

### 19. Names and literals
- The humanizer splits a digit inside a word: `MeanHba1c` reads "mean hba 1c".
- A `const` renders by NAME wherever it appears ("contains TheChart"); a computed count renders expression AND value
  (`has count 'ChartsBefore.Count - 1' = 3`). Render the value, or both consistently.
- A humanized call nested in a list literal loses its parentheses: `[CallFor("x", Args), Demo]` renders
  "[call for "x", Args, Demo]" - three items where there are two.
- A single-line raw string literal keeps one `"` per side, so its inner quotes read as unescaped; a multi-line raw
  string renders its lines but keeps one `"` of the closing delimiter (`... ``` "]`).
- The from-arguments `Returns((a, b, c) => F(a))` overloads have no caller-expression parameter and render
  "returns retVal".
- A class folder two levels down joins the sub-folder into its heading ("Calc Calc Expression").
- `default(T)` mangles a type that is not a bare name: `default(DateTime?)` reads "default DateTime?)",
  with a stray closing paren, and `default(List<int>)` reads "default list int". Found while doing item
  3; `default(DateTime)` and the cast form of both are correct, so it is the `default(...)` parse.

### 20. `Does()` chains: wording and failure messages
`.and.not.Contain(x)` renders "and not contain x" where the sentence is "and does not contain x"; a failure after
`.and.` loses the actual's name ("Expected  to contain"). Same code path as item 1.

### 21. Point the trainwreck error at the idiom
DONE in 2.6.0. The message names the verb, the expression, and the rewrite: "No trainwrecks in And:
'Result.Length' chains a member on its subject. Hand over the root and chain the rest after it: And(Result).Length".

## Suggested order of work
1. Items 1, 2, 3 (P1) - each a day or less, each closes a class of silently wrong specs.
2. Items 4, 7, 5, 6, 8 (P2) - the mocking gaps; 4 and 7 remove the last hand-written fakes and tag-held scripts in M5.
3. Items 10, 11 (P3) - build-layout independence and line endings; 12 and 13 after.
4. Items 14, 15, 16 (P4) - the three rendering changes that change how a specification READS; then 17-21 as polish.

After each item, regenerate M5's `_specification/` and diff it: the rendering items are done when the named
passages read as the spec is written, the correctness items when the M5 workarounds can be removed.
