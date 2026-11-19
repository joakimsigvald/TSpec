---
name: product-trim-docs
description: Keep the documents of a product under SampleProjects in shape, each holding what its Purpose says and consistent with the others and the code. Use when the user asks to trim a product's documents ("trim the documents" or similar, where the product is the context), or says yes when product-refine offers it after a feature that changed one of them.
---

# Trimming a product's documents

**Purpose:** keep a product's hand-written documents in shape: each holds only what its purpose
says, and none contradicts another or the code.

Takes the product: a folder under `SampleProjects/`, such as `MyHotel`. Unless the PO named it, or
it is the product of a feature just finished, guess it from the context and ask with two buttons in
the next-step box: the guess and `Other`. If the PO's product is not one of the folders, say so and
stop. The documents are its `CLAUDE.md`,
`README.md`, `GLOSSARY.md`, `TESTING.md`, `BACKLOG.md` and the three `CODING-STANDARD*.md`, and the
skills `.claude/skills/product-*`, this one included. Not the `Features/` artefacts, and nothing
generated.

1. Read each document, and the documents and code its content refers to.
2. Compare each with the Purpose at its top, with the other documents and with the code. Look for:
   - content its purpose does not cover, or that another document's purpose does;
   - the same thing said in two places: it stays where its purpose fits;
   - a statement another document or the code contradicts.
3. Make the obvious changes: cut what plainly does not belong, move what plainly belongs elsewhere,
   and fix what is plainly stale by saying it more generally, never by adding to it.
4. Report the changes in a few lines. Take the rest to the PO one at a time, quoting the text as it
   stands and as you would write it, with buttons in the next-step box. Then `## Trim finished`.
