---
name: product-refine
description: Make the adjustments a feature's review proposes, as the PO agrees, and finish the feature. Use when product-design invokes it, or the user asks to apply a feature's review.
---

# Refining a feature

**Purpose:** a finished solution: the proposals of the review made as the PO agrees, and the feature
handed to the PO.

Takes the product and the feature code. Paths are under `SampleProjects/<product>/`. Without
`Features/Check/REVIEW-<code>.md` there is nothing to refine: say so and stop.

1. Go through the review's proposals. An obvious fix needs no question. Put each other proposal to
   the PO on its own: the proposal in a line or two, and whether to make it, with Yes and No
   buttons in the next-step box. A misunderstanding that affects the whole feature is cleared up
   with the PO instead; nothing is reverted at this stage. Add each proposal the PO declines to
   `Features/Refine/REFINE-NOTES-<code>.md`.
2. Make the obvious fixes and the agreed proposals in the review's order, test first as `CLAUDE.md`
   says. One that changes the behaviour in a significant way also updates
   `Features/Define/REQUIREMENTS-<code>.md`, and one that changes the design
   `Features/Design/DESIGN-<code>.md`.
3. If anything changed, run every suite `TESTING.md` names until all are green, invoke
   `product-check` with the product, the code and `recheck`, and start again from step 1 with its
   review.
   `Host.Spec` runs the application in-process over HTTP, so it is slower than the rest.
4. Otherwise the feature is finished. Hand over with a clear status: the feature is built and
   checked, and what was refined on the way, if anything. End the reply with a `## Next step`
   caption and under it the PO's step: test it and commit. If the feature changed a document
   `product-trim-docs` covers, ask there whether to run it.
