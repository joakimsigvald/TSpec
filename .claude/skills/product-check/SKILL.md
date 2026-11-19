---
name: product-check
description: Review a built feature of a product under SampleProjects and propose adjustments. Use when product-design or product-refine invokes it, or the user asks to check or review a built feature.
context: fork
background: false
---

# Checking a feature

**Purpose:** find where the built feature departs from what the PO asked for, the design or the
coding standards, and propose adjustments in the order to make them.

Takes the product and the feature code: $ARGUMENTS. Paths are under `SampleProjects/<product>/`.
Unless `Features/Build/BUILD-NOTES-<code>.md` says the feature is built, there is nothing to check:
say so and stop. Check asks the PO nothing.

1. Read the `_specification` diffs of the behaviour specifications, new files included, against
   `Features/Define/REQUIREMENTS-<code>.md`. They need not match word for word, but must show no
   contradiction and no missing spec. `Architecture.Spec/_specification.md`, which states the
   structure, is read only as a diff, when the change updated it.
2. Review the code of the change against `CLAUDE.md` and `CODING-STANDARD.md`, with
   `CODING-STANDARD-BACKEND.md` or `CODING-STANDARD-FRONTEND.md` for each side it touches, and
   against general principles such as DRY and YAGNI.
3. Compare the code with `Features/Design/DESIGN-<code>.md`: it need not match exactly, but must not
   be more complicated than the design calls for. A deviation `BUILD-NOTES-<code>.md` gives a reason
   for is decided, unless the reason does not hold.
4. When the feature touches the UI, try it in the browser (`README.md`), and stop the application
   when you are done. A small change of UI logic is checked by the UI's tests alone.
5. Write `Features/Check/REVIEW-<code>.md`, replacing any earlier one: each departure as a numbered
   proposal, saying what is wrong and the adjustment, or that there is nothing to propose. Number
   them in the order to make them, so none redoes another's work. A misunderstanding that affects
   the whole feature is a proposal too, and says so. Leave out what
   `Features/Refine/REFINE-NOTES-<code>.md` lists as declined.
6. Report in a line how many proposals the review holds.
