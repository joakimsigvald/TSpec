---
name: product-design
description: Design a defined feature of a product under SampleProjects. Use when product-define invokes it, or the user asks to design a feature that has its requirements.
---

# Designing a feature

**Purpose:** agree with the PO on how the feature will be built, and see it built that way.

Takes the product and the feature code. Paths are under `SampleProjects/<product>/`. Without
`Features/Define/REQUIREMENTS-<code>.md` there is nothing to design: say so and stop.

1. Read `Features/Define/REQUIREMENTS-<code>.md`, unless it is in the conversation already, and
   study the relevant code. `Architecture.Spec/_specification.md` opens with how the projects
   reference each other, and each behaviour specification's `_specification/README.md` with a
   description of its project. Read `CODING-STANDARD.md` with `CODING-STANDARD-BACKEND.md` or
   `CODING-STANDARD-FRONTEND.md` for each side the feature touches.
2. Summarise the design in `Features/Design/DESIGN-<code>.md`, so the PO understands how it will be
   built: what parts, what new interactions, any restructuring. No implementation details, and
   nothing every feature does anyway, such as writing specs.
3. Show it to the PO in full: a short status line, a `---` separator and the design. Under it ask,
   with buttons in the next-step box, whether to go ahead to Build or keep designing. To keep
   designing, ask what the PO wants to change. Answer any questions the PO asks about the design
   before going on.
4. On go-ahead, invoke `product-build` with the product and the code.
5. When it reports the feature built, invoke `product-check` and then `product-refine`, each with
   the product and the code.
6. When it reports a stop, read `Features/Build/BUILD-NOTES-<code>.md` and decide how to go on,
   asking the PO when the choice is theirs: how the design changes, and whether the code Build left
   is kept or reverted. Update `DESIGN-<code>.md` to match, and go back to step 4.
