# Follow-up

## A. Pipeline

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|

## B. Assert

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| B3 | **`Like`, `LowerCase()` and `UpperCase()` use the machine's culture.** Decide whether they mean the user's culture or invariant. | Small | Small | 0 |
| B4 | **A lenient `Are`.** `Are` and `EquivalentTo` are exact; a lenient form beside them could read well, but its slack is ambiguous: xUnit's non-strict lets the actual hold more, while `guests.Are(invited)` reads as the actual holding less (an invited guest who did not come). | Small | Small | 0 |

## C. Arrange

Each waits for a real spec that is worse without it.

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| C7 | **No `because` on a verification.** `Then()` takes one; `Then<TService>(…)` does not. | Small | Small | 0 |
| C11 | **A protected property's set cannot be set up.** A setup by name reaches its read only. | Small | Small | 0 |

## D. Specification

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| D1 | **`A<T>()` before a vowel.** `o.Is().A<ApplicationException>()` reads `O is a ApplicationException`. `An<T>()` reads right; the article could instead follow the type name. | Trivial | Zero | 0 |
| D3 | **`default(T)` of a nullable or generic type.** `default(DateTime?)` reads `default DateTime?)` and `default(List<int>)` reads `default list int`; `default(DateTime)` reads right. | Small | Small | 0 |
| D5 | **A negation after `and` loses its verb.** `"abc".Does().Contain("a").and.not.Contain("x")` reads `and not contain "x"` rather than `and does not contain "x"`. | Small | Small | 0 |
| D8 | **A run of digits splits apart.** `SplitWords` never advances `prev`, so every digit starts a word and only the acronym merge rejoins single ones: `Item10x` reads `item 1 0x`. Advancing it also turns `GivenX10` from `given x10` into `given x 10`. | Small | Small | 0 |
| D11 | **A lone capital at the start of a title splits off.** `TSpec` reads `# T Spec`, `EShop` `# E Shop`. Fix it in `AsTitle` only, since `GivenASnake` must still read `given a snake` and `MyHTTPServer` `My HTTP Server`. | Small | Small | 0 |
| D15 | **Given-headings and bullets could link to their line** once Visual Studio's Markdown preview follows a `#L` anchor. `SourceLocations` already reads lines. | Small | Small | 0 |

## E. Documentation

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|

## F. Core.Test

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| F3 | **Nothing tests the generator's xUnit wiring directly** — fixture wiring, `TestState` at `Dispose`, end-of-assembly ordering — which cannot be tested from inside the same assembly. A scratch project could; today only the MyHotel suites passing covers it. | Medium | Medium | 0 |
| F6 | **Document a spec as the subject of another spec** in README §6, for authors of shared base specs. | Small | Small | 0 |

## G. Architecture

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| G1 | **First-class source lines.** A `SourceLine` with `File`, `Number`, `Text`, `Indent` and `IsEmbedded` (inside a multi-line raw string), named `X/Path:line` on failure as nodes are. MyHotel's `LineWidths`, `NestingDepth`, `Indentation` and `Characters` each rebuild `(text, index)` tuples, and two need the `EmbeddedLines` helper. | Medium | Medium | 0 |
| G2 | **A base type on `DeclaredType`.** `type.DerivesFrom(typeof(Spec))` / `DerivesFrom("Exception")`, semantic and transitive. MyHotel's `CoreSpecs` falls back to the `When` naming convention for want of it, and `ExceptionTypes` chases base-type names through syntax. | Small | Medium | 1 |
| G3 | **Counted-breach failure rendering.** An assertion over a site collection that renders count, first site and an optional remedy — nine MyHotel specs repeat the `Breach`/`Sites` pair formatting `"{count} X, the first at {first}; {instead}"` around `Is().Null()`. | Medium | Medium | 0 |
| G4 | **One fact per project without the boilerplate.** Expose the project names as `TheoryData<string>`, or a `[Theory, EachProject]` data attribute; MyHotel repeats the `Projects` property and `MemberData` pair 25 times, with a `Sources` helper only to feed it. | Small | Medium | 1 |
| G5 | **Comments as data.** `file.Comments`, each with `Kind` (line, block, doc), the lines it covers, whether it stands on its own line, and the member it leads. MyHotel's `Comments` is 120 lines of trivia-walking, and `Indentation` re-derives comment lines. | Medium | Medium | 0 |
| G6 | **Declaring type and attributes.** `member.DeclaringType`, and `HasAttribute("Theory")` on members and types handling the `Attribute` suffix and qualification. MyHotel's `Arities` misses `TheoryAttribute`, `WireTypes` climbs syntax ancestors to find a member's record, `Suppressions` keeps its own suffix list. | Small | Medium | 1 |

## Reading and updating this document

- Add an item only once it is verified against the code: the problem and a short example. Give it
  the next free number in its section.
- Do items with Worth above 0. Keep items at 0. Remove items below 0.
- Delete an item's row when it is done. Never renumber.
- Re-estimate an item when what is known about it changes, and recompute its Worth.

Effort:

- **Trivial** — a few lines, no decision, about a minute of agentic work.
- **Small** — a few minutes of agentic work, no hard questions.
- **Medium** — one go, with some design decisions.
- **Large** — needs a design session and a breakdown into smaller items.

Value, to TSpec's users: Zero, Small, Medium, High.

Worth = floor(2^(V−E) − 1), counting each scale 0–3:

| Value \ Effort | Trivial | Small | Medium | Large |
|---|---|---|---|---|
| **Zero** | 0 | −1 | −1 | −1 |
| **Small** | 1 | 0 | −1 | −1 |
| **Medium** | 3 | 1 | 0 | −1 |
| **High** | 7 | 3 | 1 | 0 |
