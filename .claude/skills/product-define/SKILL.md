---
name: product-define
description: Define a feature of a product under SampleProjects — the first of five phases, Define, Design, Build, Check, Refine. Use when the user wants to build something in a product under SampleProjects, such as MyHotel ("I want to build xyz", "let's build xyz" or similar, where the product is the context, from the session or named in the prompt), or names the product and its next feature.
---

# Defining a feature

**Purpose:** agree with the PO on exactly what the feature is, written down as its requirements.

## Product and feature

1. Settle the product: a folder under `SampleProjects/`, such as `MyHotel`. Paths below are under
   it. Unless the PO named it, guess it from the context and ask with two buttons in the next-step
   box: the guess and `Other`. If the PO's product is not one of the folders, say so and stop.
2. Make the feature's code from the PO's description: at most 8 characters, mostly a-z. If
   `Features/` already holds that code, add the first free numeric suffix: 2, 3, ….

## Define

1. Read `CLAUDE.md`, `GLOSSARY.md` and the behaviour specifications: the `_specification/` of every
   `*.Spec` project but `Architecture.Spec`, which states the structure. Each one's `README.md`
   opens with a description of the project it specifies. Start from those indexes and the section
   captions: read in full what the feature concerns, and skim or skip the rest.
2. Ask about the feature until you know exactly what the PO wants: one question at a time, each
   concrete enough to answer in a line. A new term's name is agreed here.
3. If seeing the application would help you understand the requirement, run it and study it
   (`README.md`), and stop it when you are done. Only then: it is not a step of every feature.
4. Write a short summary of the feature to `Features/Define/REQUIREMENTS-<code>.md`, then show it to
   the PO in full, not linked or buried in a longer answer. The reply is a short status line, a
   `---` separator and the summary. Under it ask, with buttons, whether to go ahead to Design or
   keep defining: in the next-step box, never the question dialog, which can hide the summary. To
   keep defining, ask what the PO wants to change, not questions of your own.
5. On go-ahead, invoke `product-design` with the product and the code, such as `MyHotel roompick`.
