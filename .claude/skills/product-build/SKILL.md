---
name: product-build
description: Build a designed feature of a product under SampleProjects. Use when product-design invokes it, or the user asks to build a feature that has its design.
context: fork
background: false
---

# Building a feature

**Purpose:** a working solution: the feature built as designed, with every suite green.

Takes the product and the feature code: $ARGUMENTS. Paths are under `SampleProjects/<product>/`.
Without `Features/Design/DESIGN-<code>.md` there is nothing to build: say so and stop. Build asks the
PO nothing.

1. Read `CLAUDE.md`, `Features/Define/REQUIREMENTS-<code>.md` and
   `Features/Design/DESIGN-<code>.md`.
2. Build test first, as `CLAUDE.md` says. For how, read `TESTING.md` and, at the repository root,
   `agent-reference.md`, with `Architecture.CSharp/agent-reference.md` when the change touches
   `Architecture.Spec`. If the design does not hold, stop building and go on to step 4.
3. Run every suite `TESTING.md` names until all are green. `Host.Spec` runs the application
   in-process over HTTP, so it is slower than the rest.
4. Write `Features/Build/BUILD-NOTES-<code>.md`, replacing any earlier one. It opens with how the
   build ended: built, or stopped and why. Then each deviation from the design you made on your
   own, with its reason, or that there were none.
5. Report how the build ended in a line or two.
