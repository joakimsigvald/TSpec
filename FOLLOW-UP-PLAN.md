# Follow-up

## A. Pipeline

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| A1 | **`Having` and `Until` state their order only by rule.** `Having(c).Having(b)` runs b first, and 20 MyHotel comments explain it ("Setups run last-declared-first …"). `After` and `Before` would say it by meaning — `When(b).After(a).Before(c)` — replacing `Having` and `Until`, which are deprecated. Open: whether the page keeps "Having … after" and "Until … before", and how a delay ("Having waited 100 ms") reads. | Medium | Medium | 0 |

## B. Assert

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| B3 | **`Like`, `LowerCase()` and `UpperCase()` use the machine's culture.** Decide whether they mean the user's culture or invariant. | Small | Small | 0 |
| B4 | **A lenient `Are`.** `Are` and `EquivalentTo` are exact; a lenient form beside them could read well, but its slack is ambiguous: xUnit's non-strict lets the actual hold more, while `guests.Are(invited)` reads as the actual holding less (an invited guest who did not come). | Small | Small | 0 |
| B5 | **`Contain` takes only a string.** A refusal writes `.Message.Does().Contain($"{The(_booking).BookingNumber}")` and reads `Message contains "{the Booking's BookingNumber}"`; an overload for any value, formatted invariantly, would read `contains the Booking's BookingNumber`. | Small | Small | 0 |

## C. Arrange

Each waits for a real spec that is worse without it.

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| C7 | **No `because` on a verification.** `Then()` takes one; `Then<TService>(…)` does not. | Small | Small | 0 |
| C11 | **A protected property's set cannot be set up.** A setup by name reaches its read only. | Small | Small | 0 |
| C12 | **A generated value is valid only by luck.** MyHotel spells out `new BookingPeriod(Today, LaterDay)` where any valid period would do, and `A<Room>()` has a valid bed count only while the counter is below 10. `Using<BookingPeriod>().Where(p => p.From < p.To)` could retry generation until the rule holds, for generated values only, never provided ones; retrying cannot reach a range the counter has passed. | Medium | Medium | 0 |

## D. Specification

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| D1 | **`A<T>()` before a vowel.** `o.Is().A<ApplicationException>()` reads `O is a ApplicationException`. `An<T>()` reads right; the article could instead follow the type name. | Trivial | Zero | 0 |
| D3 | **`default(T)` of a nullable or generic type.** `default(DateTime?)` reads `default DateTime?)` and `default(List<int>)` reads `default list int`; `default(DateTime)` reads right. | Small | Small | 0 |
| D8 | **A run of digits splits apart.** `SplitWords` never advances `prev`, so every digit starts a word and only the acronym merge rejoins single ones: `Item10x` reads `item 1 0x`. Advancing it also turns `GivenX10` from `given x10` into `given x 10`. | Small | Small | 0 |
| D11 | **A lone capital at the start of a title splits off.** `TSpec` reads `# T Spec`, `EShop` `# E Shop`. Fix it in `AsTitle` only, since `GivenASnake` must still read `given a snake` and `MyHTTPServer` `My HTTP Server`. | Small | Small | 0 |
| D15 | **Given-headings and bullets could link to their line** once Visual Studio's Markdown preview follows a `#L` anchor. `SourceLocations` already reads lines. | Small | Small | 0 |
| D17 | **A reader cannot tell from the index what a file states.** An agent picks what to read from the `README.md` index and the captions, but the index gives only counts: MyHotel's Host.Spec shows `[Bookings](Bookings.md) \| 6 \| 20 \| 34`, not its six acts. | Medium | Medium | 0 |
| D19 | **A lone Given loses its arrangement to the act.** Over a single branch `Requirement.Shared` lifts all of it, so "When add room with nine beds" holds the setup and "Given no such room" heads nothing. Lifting only the act would also sink a base class's `Using`, since the page cannot tell where a clause was written. | Small | Small | 0 |

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
| G11 | **Tokens have no `Nodes<T>()`.** Arrows walks each node's `ChildTokens()`, Indentation and Characters walk `DescendantTokens()` and format `file:line` themselves. `Tokens(SyntaxKind)`, each with `File` and `Span` and named `X/Path:line`, would let them read `Has().None(...)`. | Small | Small | 0 |
| G12 | **A failure names a breach but not what was measured.** A dozen MyHotel facts, MethodSize and Arities among them, build `"file:line note"` strings for `Is().Empty()`, since `Has().None` would drop the size, the count or the part that broke — so the agent reference's `Has().None(member => member.Size > 240)` is the form MyHotel avoids. Likely TSpec's assertions, not the package. | Medium | Medium | 0 |
| G13 | **A file knows only its top-level folder.** MyHotel derives the rest three ways: `ToString().Contains("/Mirrors/")`, `Path.Split('/')[..^1]` and `Split('/') is [.., "Exceptions", _]`. A file's folders as segments would serve all three, named so as not to clash with a project's `Folders` graph. | Small | Small | 0 |
| G16 | **No rule reads a project's build settings.** Suppressions leaves a project file's `NoWarn` to review, though it switches a warning off as `#pragma` does. The compilation options the package reads for the language version record `nullable` too, but not `NoWarn` or warnings-as-errors, which the build would have to record. | Medium | Medium | 0 |

## H. Templates

All four ship in one package, `TSpec.Templates`, released whenever TSpec or TSpec.Architecture.CSharp is.

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| H1 | **A Neat service starts as a copy of MyHotel.** Nothing creates the five projects, `Host.Spec`, `Core.Spec` and an `Architecture.Spec` that checks the layering, so each start strips out the hotel (`SubdomainDependencies` names Rooms and Bookings), and Neat is described only in MyHotel's `CLAUDE.md`. A `dotnet new neat-service` template, its spec projects built from H3's and H4's rather than copied. | Medium | High | 1 |
| H2 | **A Neat web application has no template either.** `dotnet new neat-webapp` would add MyHotel's React `Web` (Vite, ESLint, `Web.esproj`) to H1's template rather than copy it. | Medium | Medium | 0 |
| H3 | **A spec project is set up by hand.** Beside `dotnet add package TSpec` it needs `xunit.v3`, an `Exe` output, global usings for `Xunit`, `TSpec` and `TSpec.Assert`, and `[assembly: AssemblyFixture(typeof(SpecificationDocument))]`, which README gives only in §6.2. A `dotnet new tspec` template. | Small | Medium | 1 |
| H4 | **An architecture spec project is set up by hand.** It needs the `ProjectReference` glob that excludes itself, `ArchitectureDocument` as assembly fixture and `Microsoft.CodeAnalysis.CSharp` at least the SDK's compiler, as MyHotel's `Architecture.Spec.csproj` notes. A `dotnet new tspec-architecture` template. | Small | Medium | 1 |

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
