# Plan

The notes of 2026-09-05, in build order.

## 1. Printer defects

Small, independent, each one a wrong line in a real document. Test row first.

1. ~~**`??` loses an operand.**~~ Done, and no operator is special. `Binary` took its raw text
   before its right operand was consumed, so the text stopped at the operator; and an unwrapped
   parenthesized expression dropped the parentheses that made it one thing, which it now keeps.
   **Left open:** the same evaluation-order slip at five more sites —
   `IsAs` ([BinaryRule.cs:52](Core/Internal/Specification/ExpressionParsing/Parse/BinaryRule.cs:52)),
   `Assign`, `Conditional`, `Unary` (×2) and `Cast`. Latent, since each has a describe path that
   rebuilds from its children; they show only where something falls back to `Raw`.
2. **Raw string keeps its extra quotes.** `"""{"system":"ICD-10-SE"…}"""` renders with mangled
   quoting. [Expr.cs:35](Core/Internal/Specification/ExpressionParsing/Expressions/Expr.cs:35) strips
   one quote per end; `LiteralScanner.QuoteRun` already knows how many there are.
3. **Stray space before the because-comma.** `Result.SpecXml is not null ,`. Regenerate against
   2.2.2 first — that release claims this fix; if it survives, it is the `IsAs` path.
4. ~~**No split at letter→digit.**~~ Done — `IsWordStart`.

## 2. What the document cannot say

The two real gaps. Both additive: a suite that uses neither sees no change.

5. **Class doc-comment as section prose.** Biggest legibility win — the sentence exists in the code
   and has no channel. Read it from the spec assembly's XML doc file (beside the dll, where
   `PendingDocument.Prepare` already reads deps.json); `T:Namespace.WhenAddRoom` → `<summary>` at that
   heading. Opt-in by construction: no doc file, no prose. Prose is not a claim — keep it out of
   hoisting and out of `Requirement`.
6. **`[Theory]` rows.** Today a theory renders once from the parameter *names* and states nothing:
   `When identifier.AsWords()` / `Then Result is expected`. Two parts —
   get the row values from `TestContext.Current` (or fall back to the display name, which has them),
   and aggregate rows *before*
   [Requirement.cs:34](Core/Internal/Document/Requirement.cs:34), whose `DistinctBy` drops three
   rows of four today. Render as one requirement with a table of values.
   **Decide:** table under one bullet, or one bullet per row. Table, I think — four rows are one rule.

## 3. The id in the header

7. **Make it optional.** Opt out, header carries the version alone. Pure addition.
8. **Should a referenced project move it?** Today yes —
   [PendingDocument.cs:23](Core/Internal/Document/PendingDocument.cs:23) digests the closure from the
   subject. Packages are already excluded, so only projects are in question, and excluding them
   undoes what 2.2.1 set out to do. **Decide.** If 7 ships, this may not be worth changing.

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
