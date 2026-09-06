# Plan

Open work first, in build order. What is finished is listed at the end.

## 1. The arrange surface

1. **Sequenced setup without the wall.** `Given<IChatCompletion>().First().Returns(…).AndNext()…`,
   service-wide, no method named. `First()` sits on `IGivenThatContinuation`, reachable only after
   `.That(expr)`; the service-wide `Returns` rides on Moq's return default, which has no sequence,
   so this has to enumerate the interface's matching methods and `SetupSequence` each. Naming the
   method with `Any` arguments — `.That(_ => _.Complete(Any<string>())).First()` — works today.

## 2. Header and navigation

2. **Suppress `Subject under test: string` / `Return type: string` for a static function.**
   [CodeSegment.cs:11](Core/Internal/Document/RenderPipeline/CodeSegment.cs:11). **Decide the rule:**
   "the subject is not an argument of the act" — which `TestIdentity.Declares` already reasons
   about — or let the spec declare a display name.
## 3. Ordering

3. **An ordering hint**, so the happy path can come before the refusals — today
   `ComplexityNumber` then key
   ([DocumentRenderer.cs:77](Core/Internal/Document/RenderPipeline/DocumentRenderer.cs:77)).
   Opt-in only: changing the default reflows every document. **Decide where the hint is written** —
   attribute on the class, or a number the spec declares.

## 4. Smaller

4. **A cut cell could wrap instead.** Sizing columns to their content took most of the waste out,
   but a value longer than the page still ends in an ellipsis. Wrapping within a cell is the other
   half of that refinement — and needs a rule for what a wrapped row looks like, since a row broken
   across lines stops being a row.
5. **`Is().LowerCase()` and `UpperCase()` case with the machine's culture**
   ([IsString.cs:23](Core/Assert/Continuations/String/IsString.cs:23), four calls). Nothing fails on
   it. **Decide** whether these mean "lowercase in the user's culture" or invariantly, unlike the
   renderer, which is now invariant.

## 5. Parked

6. **Class doc-comment as section prose.** Parked, PO's ruling 2026-09-05: comments are not
   verifiable the way test code is, they lie once they drift, and inviting them into the
   specification invites the pollution with them. The gap it addressed — section-level "what this
   component is for" — stands; it wants a channel that a test can keep honest.

## Done

**A subject heading links to its file.** By a path relative to the spec project — the root every
reader of the file shares — read from the portable PDB beside the spec assembly with
`System.Reflection.Metadata` from the shared framework, so no package was added. A file outside the
spec project, such as a shared base in another project, is not linked; namespace headings name no
class and carry none. A build that maps source paths (`ContinuousIntegrationBuild`) records `/_/…`
from the repository root, which is nowhere on disk; such a file is found under the spec project by
the longest tail of its path that is a file there, so the CI freshness diff sees the same document
a local build writes — verified byte-identical on MyHotel.Core.Spec. 2026-09-06.

**Lines, and links on given-headings and bullets — parked.** Built and taken out the same day, PO's
ruling: a link that does not work is worse than none. The `#L` anchor is followed by GitHub, VS Code
and Markdown Editor v2, but Visual Studio's own preview treats any fragment on a file link as an
in-page anchor and does nothing — one missed check in `LinkNavigationHelper.NavigateTo`, in
`Microsoft.VisualStudio.Markdown.Platform.dll`, worth a Developer Community ticket. Without a line, a
given-class or a method sits partway down a file, so those links went too. `SourceLocations` still
reads lines (a method is where its body starts, an async method's in its state machine's `MoveNext`,
a class at its constructor), pinned in `WhenLocateSource`, so reinstating is a rendering change once
Visual Studio follows the anchor. 2026-09-06.

**`Any<T>()` in a mock call means any T.** `Any` yields a value that cannot be retrieved again, so a
setup or verification written with `Any<int>()` could never match — the real call never carried
that value. An expression visitor now swaps each parameterless `Any<T>()` for `It.IsAny<T>()` before
the expression reaches Moq's `Setup`, `SetupSequence` or `Verify`, and `It.IsAny<T>()` renders as
"any T" so both forms read the same and the document hints at the shorter one. Replaces setup by
method name, dropped: the exact call is unambiguous where a name is not (overloads, generics, ref
parameters, the return type), and `Any` removes the length that made the name attractive. A method
named `_` as a two-character alias compiles but was rejected: three underscores in three roles on
one line. 2026-09-06.

**`??` loses an operand.** Every operator node took its raw text before its operand was consumed, so
the text stopped at the operator. Each now composes its text from its own parts.

**Raw string keeps its extra quotes.** Not a bug — the delimiter run is stripped correctly, and how
the author delimited a string is mechanism. Reopen only as a rendering decision: whether quote-heavy
content should keep the source's delimiter run.

**Stray space before the because-comma.** Already fixed in 2.2.2; the note predated it. The reported
branch is now pinned too.

**No split at letter→digit.** `IsWordStart`.

**A `[Theory]` repeated itself.** It rendered as one contentless bullet when no parameter reached the
clause, and as several same-named bullets when one did — the document had nowhere to put a row. A
value that came from a theory parameter is now a hole in the document text, so every row composes
identical text and folds into one bullet, with the rows beneath it as a table. 2026-09-05.

**Values read the machine's culture.** Three paths interpolated a value raw; all now go through
`InvariantText`, dates as `yyyy-MM-dd HH:mm:ss`.

**One document in every culture.** The suite had only ever run in sv-SE, which writes dates exactly
the way TSpec's convention does, so nothing could disagree. It now runs in en-US, which does, and
three defects fell out: a record's generated `ToString` rendered its members in the ambient culture;
the date formats were stated twice, once as patterns and once as a culture, now collapsed onto the
culture alone; and casing and string search read the machine — `ToLower`/`ToUpper` in the renderer
made Turkish write `then list ıtems` (503 failures), and `Contain`/`StartWith`/`EndWith` inherited
xunit's culture-sensitive comparison. Green in eleven cultures. 2026-09-06.

**Table columns shared the page equally**, which wasted it wherever columns differed — `count fail`
gave `count` 26 places for a single digit while cutting `errorMessage` at 26 with most of the message
gone. A column now takes only what it needs and leaves the rest to the columns that have more to say:
`errorMessage` gets 55 and the shortest row fits uncut. A table holds at most eight columns and no
column is narrower than five, so a row can never outrun the page — a theory with more parameters
than that fails generation with a message naming them. 2026-09-06.

**The id in the header.** Removed, PO's ruling: the specification describes what is tested and
nothing more, so the document names what it renders. The body is already a deterministic function of
the test source — regenerating and diffing it *is* the freshness check — and a source hash restated
that while moving on implementation changes that altered no claim. `SourceDigest` and
`ProjectReferences.ClosureFrom` went with it. No opt-out: whether a header carries eight hex
characters is not a property of anyone's requirements. 2026-09-06.

**A test skipped while it ran counted as missing.** `ExpectedRequirements` reads `FactAttribute.Skip`,
which is empty for a runtime `Assert.Skip`, so such a test was expected and never reported — and the
document was then not written **at all**, on a run that exited 0, so the freshness gate compared a
stale file against itself and passed. `Collect` now routes on the result and `Missing` subtracts
skips. 2026-09-06.

**A time-dependent test failed on a busy machine.** Waiting less than a delay asserts that *not
enough* time has passed, which no scheduler guarantees. The test now skips when the machine stretched
the wait past the delay. 2026-09-06.
