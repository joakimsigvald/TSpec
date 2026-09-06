# Plan

The notes of 2026-09-05, in build order.

## 1. Printer defects

Small, independent, each one a wrong line in a real document. Test row first.

1. ~~**`??` loses an operand.**~~ Done, and no operator is special. Three causes, all fixed:
   every operator node took its raw text before its operand was consumed, so the text stopped at
   the operator (`Binary`, `IsAs`, `Assign`, `Conditional`, `Unary`, `Cast` — each now composes its
   text from its own parts, and `RawFrom` survives only for the malformed-ternary `Unknown` and the
   cast backtrack); an unwrapped parenthesized expression dropped the parentheses that made it one
   thing; and the value path, which rebuilds a binary from its operands, did not put them back.
2. ~~**Raw string keeps its extra quotes.**~~ Not a bug. `"""{"system":"ICD-10-SE"}"""` renders
   `"{"system":"ICD-10-SE"}"`, which is the pinned ruling — the delimiter run is stripped correctly
   and "how the author delimited it is mechanism; the same text is the same claim"
   ([WhenDescribe.cs:113](Core.Test/Internal/Specification/ExpressionDescriber/WhenDescribe.cs:113)).
   Reopen only as a rendering decision: whether quote-heavy content should keep the source's
   delimiter run, since delimiter and content are then the same character.
3. ~~**Stray space before the because-comma.**~~ Fixed in 2.2.2, verified 2026-09-05: the note
   predates that release. `NoStraySpaceAfterNull` pinned only the `is null` branch, so
   `NoStraySpaceAfterNotNull` now pins the reported one.
4. ~~**No split at letter→digit.**~~ Done — `IsWordStart`.

## 2. What the document cannot say

5. ~~**`[Theory]` rows as a table.**~~ Done, 2026-09-05. Requirements settled with the PO, below.

   **The requirement.** One `[Theory]` fed by `[InlineData]` renders as **one bullet and one
   table**, whatever its data and assertions look like. Today it renders as one contentless bullet
   when no parameter reaches the clause text — `GivenAtMost`'s two rows both say `count = 2`, and
   `numbers` is never mentioned — and as several same-named bullets when one does: `GivenCount`
   gives `'count' = 1` and `'count' = 2`. Both are the same defect. The document has nowhere to put
   a row, so it either drops the data or repeats the claim.

   **The abstraction: a theory parameter is a hole, not a value.** Wherever TSpec would print a
   value that came from a theory parameter, the *document* prints the parameter's name. Every row
   then composes identical text by construction — no stripping, no matching on syntax — the
   existing fold collapses them, and the table supplies what the holes stand for. The **per-test**
   specification keeps its values, so a failing row still reads `Numbers has count 'count' = 2`.
   That, and only that, is what a step has to carry twice.

   ```
   - **count** — `Numbers has count 'count'`

     | count | numbers |
     | ----- | ------- |
     | 1     | [1]     |
     | 2     | [1, 3]  |
   ```

   Ruled, and not to be reopened while building:

   - **`[InlineData]` only.** Data living in a separate file is not specification, so a theory fed
     by `MemberData` or `ClassData` renders as it does today. One that mixes them counts as neither.
   - **Automatic** for every such theory: no attribute, no opt-out, no row cap. The 88-row theory in
     `WhenDescribe` gets an 88-row table, because that is what it verifies.
   - **One column per parameter**, headers the C# identifiers verbatim so they match the `'count'`
     in the clause letter for letter. A trailing `params` array is one column holding the collected
     values.
   - **Rows in `[InlineData]` declaration order**, so an author's grouping survives.
   - **Only rows that ran and passed.** A skipped row is absent and unmarked: the document states
     what a green run verified and nothing else.
   - **Cells through `FormatValue`**, made culture-invariant — one convention for the whole
     document: strings quoted, `null`, `true`/`false`, collections `[a, b, …]`.
   - **A padded markdown table**, indented two spaces under the bullet with a blank line above, so
     it is aligned in the raw file and a real table once rendered.
   - **Document only.** `Specification` and every `Specification.Is(…)` pin keep their values. A pin
     that breaks does so because a hole reached the per-test text by mistake.
   - **MyHotel untouched.** It has no `[InlineData]`, so both committed documents stay
     byte-identical. **Did not hold**, found 2026-09-06: the document committed in `da121e7` gave
     `ThenReturnTheBookingWithTheNumberItWasGiven` — a `[Fact]` — a one-row `roomNumber` table, the
     `Booking` constructor's parameter name having been read as a hole. The code at that commit is
     right; regenerating there produces no table, so the committed file was written by an
     intermediate build during 5d and never regenerated after the expression-level fix landed. It
     was corrected by the 2.5.0 regeneration. Nothing was watching, which is the point of item 8.

   Work items, each test-first:

   - ~~**5a Culture-invariant values.**~~ Done. `FormatValue` fell through to `value.ToString()`,
     and two more paths interpolated a value raw: the `expected` half of every failure message
     ([Constraint.cs:171](Core/Assert/Continuations/Constraint.cs:171)) and the `'count' = 1` of
     [EnumerableConstraint.cs:21](Core/Assert/Continuations/Enumerable/EnumerableConstraint.cs:21).
     All three now go through `InvariantText`, which formats dates as `yyyy-MM-dd HH:mm:ss` — PO's
     ruling, and what sv-SE already produced, so no pinned text moved. ~~Left standing: eleven date
     tests build their *expected* string with `$"{date}"`, so they still read the running machine's
     culture and would fail outside sv-SE.~~ Done in 5i, along with three defects it uncovered.
   - ~~**5b Read the row.**~~ Done — [TheoryRow.cs](Core/Internal/Document/TheoryRow.cs). The plan
     had the wrong source: `IXunitTestMethod.TestMethodArguments` is empty, being about generic
     resolution. The running row is `IXunitTest.TestMethodArguments` from
     `TestContext.Current.Test`, and it comes back already resolved — a trailing `params` array
     collected — so there is no mapping to do, one value per parameter. The declaration index is
     found by comparing the run against each `InlineDataAttribute.Data`, both flattened first,
     since an author may spell a `params` argument loose or as one array. Nothing is read unless
     every data attribute is `InlineData`.
   - ~~**5c Carry it.**~~ Done. `SpecificationEntry` gains `Row`, and what a run hands to the
     document moved out of `Collect` into `Spec.Reported()` — the collector only decides whether to
     keep it. That seam is what lets a test read an entry without switching the process-wide
     collector on mid-run.
   - ~~**5d Punch the holes.**~~ Done, and `SpecificationStep` did not have to change. A value that
     came from a theory parameter is bracketed with a marker where it is injected, in the same
     idiom as [Wrap](Core/Internal/Specification/Wrap.cs) — see
     [Hole.cs](Core/Internal/Specification/Hole.cs). The per-test specification and the failure
     message keep what the markers enclose; `Requirement.From` drops it, which is what makes the
     rows of a theory describe themselves identically and fold. One text, two resolutions, so no
     second body to thread through the thirteen places that compose one.

     Whether an expression is a hole is asked of `SpecificationContext.IsHole`, which reads the
     running theory's parameter names once per test — expression-level, so a `[Fact]` with a local
     of the same name is untouched.

     `Express` in [EnumerableConstraint.cs:21](Core/Assert/Continuations/Enumerable/EnumerableConstraint.cs:21)
     turned out to be the only injection point in the suite. Verified empirically rather than by
     reading: uncomment the fixture in `Core.Test/AssemblyInfo.cs`, run the suite, and count
     same-named bullets within a section —

     ```
     awk '/^#/{s++} /^- \*\*/{ n=$0; sub(/^- \*\*/,"",n); i=index(n,"**"); print s "@@" substr(n,1,i-1) }' \
       Core.Test/SPECIFICATION.md | sort | uniq -c | awk '$1>1'
     ```

     which went from 6 repeated groups to none. `When count` fell from 22 bullets to 16, one per
     test method, and the only lines the document lost anywhere were that section's duplicates. A
     named value that is *not* a theory parameter still states itself (`Result has count 'the int'
     = 2`), which is the distinction the marker exists to draw.
   - ~~**5e Group instead of dedupe.**~~ Done. `Requirement.From` groups on the key it used to
     de-duplicate by and collects the rows, ordered by declaration index since rows report in
     whatever order a parallel run finished them. `Requirement` gains `Rows`. Regenerating
     `Core.Test`'s document changed nothing but the source id, the rows having no renderer yet.
   - ~~**5f Render it.**~~ Done — [TableSegment.cs](Core/Internal/Document/RenderPipeline/TableSegment.cs).
     The table stands **directly under the bullet**, ahead of a claim that took a fence of its own
     (PO's ruling, 2026-09-05): what the rows were is read before what is claimed of them. A claim
     that fits on the bullet's own line is read before either, so the item owns its table rather
     than the renderer placing a segment after it. Indented two spaces so the list survives it,
     headers padded, `|` escaped and line breaks flattened so no value can end a cell or a row.

     A table closes its item with a blank line and a heading opens with one, which left a gap in 30
     places — the document had an unstated invariant of **one blank line at most**, which
     `DocumentRenderer` now holds to.
   - ~~**5g Width.**~~ Done. Columns divide the 87 places the indent and the bars leave, equally,
     and take the lesser of that share and what they need; a value longer than its share is cut and
     ends in an ellipsis. Verified on the regenerated document: 129 tables, 25 cut cells, widest
     table line 89 characters.

     **Later work, and worth doing.** Equal shares waste the page where columns differ. `count fail`
     is the case to look at — `count` uses 5 of its 26 places while `errorMessage` is cut at 26 with
     most of the message gone:

     ```
     | count | errorMessage               | numbers              |
     | 2     | "Expected numbers to have… | [1]                  |
     ```

     Columns sized to what each needs would give `errorMessage` about 55. Wrapping a cell rather
     than cutting it is the other half of the same refinement.
   - ~~**5h Ship it.**~~ Done. README gains "Theories become tables" under 6.2; the agent reference
     gains one bullet, this being presentation rather than a mechanism to learn, and its covers-line
     moves to 2.4. `PackageVersion` 2.4.0 — 2.3.0 was packed but never uploaded, so its notes stay
     and the new work joins them. Suite green on all three frameworks; both MyHotel documents
     regenerate byte-identical, as ruled.
   - ~~**5i One document in every culture.**~~ Done 2026-09-06, finishing what 5a left standing.
     The eleven date tests now build their expected text with `InvariantText()` rather than
     `$"{date}"` — the claim is what TSpec's formatter produces, not what the ambient culture does.

     **The suite now runs in en-US**, set by a module initializer in `Core.Test/AssemblyInfo.cs`.
     This is the whole lesson: sv-SE writes dates exactly the way TSpec's convention does, so for
     as long as the suite ran only here, nothing could ever disagree. Pinning the suite to a culture
     that *matches* would have made that permanent; pinning it to one that differs makes a leak fail
     on the next run. Ruled out as an alternative to the eleven call sites for the same reason.

     Three defects it uncovered, each fixed test-first and each user-facing:

     - **A type that formats itself leaked the culture.** `InvariantText` fell through to
       `value.ToString()` for anything not `IFormattable`, and a record's generated `ToString`
       renders its members with the ambient culture — so a document said `9/5/2026 1:45:00 PM` on
       en-US. Fixed by wearing a culture for that call.
     - **The formats were stated twice.** That culture and the four `DateTime`/`DateTimeOffset`/
       `DateOnly`/`TimeOnly` branches said the same thing in two places. Collapsed: `InvariantText`
       is now `null`, `IFormattable` through `_documentCulture`, everything else through
       `Formatted`, and the convention lives on the culture alone. Guarded by pins for all four
       types, written first and unmoved by the change.
     - **Casing and searching read the machine.** `ToLower()`/`ToUpper()` in the renderer
       ([StringExtensions.cs:96](Core/Internal/Specification/StringExtensions.cs:96) and
       [:152](Core/Internal/Specification/StringExtensions.cs:152)) made tr-TR write `then list
       ıtems` — **503 failures**, and a document that differs by where it was generated, which is
       the one thing the freshness gate cannot survive. `Contain`/`StartWith`/`EndWith` inherited
       xunit's current-culture comparison, so Thai collation found `###` in text holding none and
       every culture found a zero-width joiner anywhere. Both now invariant and ordinal; the
       explicit-comparison overloads still mean what they say.

     Verified across eleven cultures — th-TH, tr-TR, az-AZ, en-US, sv-SE, de-DE, fr-FR, ja-JP,
     ar-SA, ko-KR, zh-CN — green in all, on all three frameworks, with both MyHotel documents
     byte-identical.

     **Left standing:** the four `ToLower()`/`ToUpper()` calls in
     [IsString.cs:23](Core/Assert/Continuations/String/IsString.cs:23). Nothing failed on them, and
     whether `Is().LowerCase()` means "lowercase in the user's culture" is a semantic to decide
     rather than a bug to fix.

## 3. The id in the header

6. ~~**Make it optional.**~~ and 7. ~~**Should a referenced project move it?**~~ Both answered by
   one ruling, PO's, 2026-09-06: **the id is gone, and the header carries the subject's version
   alone.**

   The principle that settles it: *the specification gives an honest description of what is tested
   and nothing more.* Verifying that the behaviour is correct belongs to the tests, coverage to a
   coverage gate, effectiveness to Stryker. So the document names what it renders — the tests — and
   an id naming the implementation's source had the header speak for something the document does
   not describe.

   That principle retires the id rather than re-rooting it at the spec project. The body is already
   a deterministic function of the test source, which is what makes
   `dotnet test && git diff --exit-code` work at all — regenerating and diffing the whole file *is*
   the up-to-date check, and a hash over the same source only restates it. The two cases come out
   asymmetric: where a test change alters a claim the body moves and the hash adds nothing, and
   where it alters no claim (a renamed private helper, a reformat, a test whose requirement folds
   into an existing one) the body correctly sits still and the hash moves alone — reporting that
   mechanism changed, which is the one thing the document exists to erase.

   Gone with it: `SourceDigest`, `ProjectReferences.ClosureFrom` and the graph it walked, and the
   `obj/` exclusion that kept the commit id out. `ProjectReferences` is now only what names the
   subject and states its version. Both MyHotel documents lost the `+id` from their header.

   No opt-out, and none wanted: whether a header comment carries eight hex characters is not a
   property of anyone's requirements, so there are no domain grounds on which to configure it.

8. ~~**A test skipped while it ran counted as missing.**~~ Fixed 2026-09-06, found while ruling on
   the above. `ExpectedRequirements` reads `FactAttribute.Skip`, which is empty for a runtime skip
   (`Assert.Skip`, or a mapped `SkipExceptions`), so such a test was expected, reported nothing —
   its result is `Skipped`, not `Passed` — and left `Missing` non-empty. The document was then not
   written **at all**, on a run that exited 0, so the freshness gate compared a stale file against
   itself and passed. Not one stale commit: the document stopped updating for as long as the skip
   stood.

   `Collect` now routes on the result — `Passed` records, `Skipped` marks the requirement excused —
   and `Missing` subtracts both. A skip excuses itself and nothing else.

   Verified against MyHotel rather than by reading, the branch being untestable from inside the
   same assembly: a temporary `Assert.Skip` in `Core.Spec` left the document unwritten with the
   branch disabled, and rewrote it with the branch live, both on a green 33-test run.

## 4. The arrange surface

9. **Setup by method name, arguments blind.** The verify side has it —
   `.And<IEventQueue>(nameof(IEventQueue.MarkFailed), Never)`. `Given<T>().That(…)` takes only an
   expression, so every parameter must be spelled with `It.IsAny`.
10. **Sequenced setup without the wall.** `Given<IChatCompletion>().First().Returns(…).AndNext()…`.
    `First()` sits on `IGivenThatContinuation`, reachable only after `.That(expr)`; the service-wide
    `Returns` has no sequence. Same resolution rule as 9, so build it after.
11. **`It.IsAny<T>()` reads as "any T".** Rendering change, so it re-pins — do it after 9 and 10,
    which remove most occurrences, and re-pin once.

## 5. Header and navigation

12. **Suppress `Subject under test: string` / `Return type: string` for a static function.**
    [CodeSegment.cs:11](Core/Internal/Document/RenderPipeline/CodeSegment.cs:11). **Decide the rule:**
    "the subject is not an argument of the act" — which `TestIdentity.Declares` already reasons
    about — or let the spec declare a display name.
13. **Heading links to its test class/method.** Needs a path story first: the run has no file paths,
    the PDB has the compiler's absolute ones, and embedding those is churn. Unshaped until there is a
    relativization stable across machines.

## 6. Ordering

14. **An ordering hint**, so the happy path can come before the refusals — today
    `ComplexityNumber` then key
    ([DocumentRenderer.cs:77](Core/Internal/Document/RenderPipeline/DocumentRenderer.cs:77)).
    Opt-in only: changing the default reflows every document. **Decide where the hint is written** —
    attribute on the class, or a number the spec declares.

## 7. Parked

15. **Class doc-comment as section prose.** Parked, PO's ruling 2026-09-05: comments are not
    verifiable the way test code is, they lie once they drift, and inviting them into the
    specification invites the pollution with them. The gap it addressed — section-level "what this
    component is for" — stands; it wants a channel that a test can keep honest.
