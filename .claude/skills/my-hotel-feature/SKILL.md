---
name: my-hotel-feature
description: Build a MyHotel feature in four phases — Define, Design, Build, Verify. Use when the user wants to build something in MyHotel ("I want to build xyz", "let's build xyz" or similar, where MyHotel is the context, from the session or named in the prompt), or names MyHotel and its next feature.
---

# Building a MyHotel feature

Paths are under `SampleProjects/MyHotel/`. Define and Design each leave an artefact in `Features/`,
kept as the feature's history and named with a code you make from the initial prompt: at most 8
characters, mostly a-z, possibly with a numeric suffix.

## Define

1. Read `CLAUDE.md`, `GLOSSARY.md` and the generated `Host.Spec/_specification/` and
   `Core.Spec/_specification/`.
2. Ask about the feature until you know exactly what the PO wants: one question at a time, each
   concrete enough to answer in a line. A new term's name is agreed here.
3. Once the questions show the feature touches the UI, run the application and study the page
   (`TESTING.md`).
4. Close with a short summary of the feature in `Features/REQUIREMENTS-<code>.md`, for the PO to
   confirm.

## Design

1. Study the relevant code, `CODING-STANDARD.md` with the backend or frontend one for the code
   touched, and `Architecture.Spec/_specification.md`.
2. Summarise the design in `Features/DESIGN-<code>.md`, so the PO understands how it will be built:
   what parts, what new interactions, any restructuring. No implementation details, and nothing every feature does anyway,
   such as writing specs.
3. Wait for the go-ahead, or the PO's questions.

## Build

Test first, as `CLAUDE.md` says. For how, read `TESTING.md` and, at the repository root,
`agent-reference.md`, with `Architecture.CSharp/agent-reference.md` when the change touches
`Architecture.Spec`.

If the design does not hold, stop and state the problem briefly. The PO decides whether the
restructuring is built in, or the code changes are reverted and we return to Design. Either way
`DESIGN-<code>.md` is updated to the design that gets built.

## Verify

1. Run all suites: `Host.Spec`, `Core.Spec`, `Architecture.Spec` and Web's tests.
2. Read the `_specification` diffs, new files included, against `REQUIREMENTS-<code>.md`.
   They need not match word for word, but must show no contradiction and no missing spec.
3. When the feature touches the UI, try it in the browser (`TESTING.md`). A small change of UI logic
   is verified by Web's tests alone.
4. Fix what is small. A misunderstanding that affects the whole feature goes back to Define: report
   it, clear it up with the PO and update `REQUIREMENTS-<code>.md`, then adjust `DESIGN-<code>.md`
   and the build. Nothing is reverted at this stage.
5. Hand over to the PO for manual testing and commit.
