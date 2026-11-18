---
name: my-hotel-trim-docs
description: Keep MyHotel's documents in shape, each holding what its Purpose says and consistent with the others and the code. Use when the user asks to trim MyHotel's documents ("trim the documents" or similar), or says yes when my-hotel-feature offers it after a feature that changed one of them.
---

# Trimming MyHotel's documents

**Purpose:** keep MyHotel's hand-written documents in shape: each holds only what its purpose says,
and none contradicts another or the code.

The documents are `CLAUDE.md`, `README.md`, `GLOSSARY.md`, `TESTING.md`, `BACKLOG.md` and the three
`CODING-STANDARD*.md` under `SampleProjects/MyHotel/`, and the skills `.claude/skills/my-hotel-*`,
this one included. Not the `Features/` artefacts, and nothing generated.

1. Read each document, and the documents and code its content refers to.
2. Compare each with the Purpose at its top, with the other documents and with the code. Look for:
   - content its purpose does not cover, or that another document's purpose does;
   - the same thing said in two places: it stays where its purpose fits;
   - a statement another document or the code contradicts.
3. Make the obvious changes: cut what plainly does not belong, move what plainly belongs elsewhere,
   and fix what is plainly stale by saying it more generally, never by adding to it.
4. Report the changes in a few lines. Take the rest to the PO one at a time, quoting the text as it
   stands and as you would write it, with buttons in the next-step box. Then `## Trim finished`.
