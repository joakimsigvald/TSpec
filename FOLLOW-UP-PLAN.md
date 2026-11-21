# Follow-up

## A. Pipeline

| Id | Description | Effort | Value | Worth |
|---|---|---|---|---|
| A1 | **The reverse order of `Having` is unnamed in code.** `Having(c).Having(b)` runs b first, and 21 MyHotel comments explain it ("Setups run last-declared-first …"), while the page already reads "Having c after b". A continuation `.After(b)`, doing what `.Having(b)` does, would say it in code. | Small | Medium | 1 |

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
| D18 | **Given-sections lose the author's order.** `DocumentRenderer.Order` sorts them by complexity, then by name, though Facts keep declared order: MyHotel's `WhenGet` reads "departed yesterday" before "departs today", and "no such booking" comes first under `WhenCancel` but last under `WhenGet`. Whens in separate files have no declared order. | Small | Medium | 1 |
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
| G7 | **One fact per project costs a property per file and reads unbound.** 36 MyHotel files declare `TheoryData<string> Projects => Sources.Projects` for `[Theory, MemberData(nameof(Projects))]`, and the page reads `Source.Projects[project].Folders is acyclic`. An `[EachProject]` attribute would drop the property; reading "every project's Folders" needs an extension point in TSpec's renderer. | Medium | Medium | 0 |

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
