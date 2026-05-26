# The Story Method
### A lightweight, git-native approach to persistent AI session context

*Current version — supersedes `session-0002-session-results-method-description.md`.*  
*Reflects: new opener format (session-0004) and opener template.*

---

## The Problem This Solves

When working with an AI coding assistant on an evolving project, context gets lost between sessions. You end up re-explaining decisions, re-researching conclusions, and re-establishing shared understanding every time you start fresh.

This method gives you a lightweight, git-native way to accumulate and preserve AI session outputs — without bureaucratic overhead. Everything is plain Markdown in your repository. No special tooling. No external systems.

---

## Core Concept

All story sessions live under `.github\stories\` in your repository. This folder is part of your git history, so context survives terminal restarts, machine switches, and time.

The structure has two levels:
1. **Story directory** — a named subject area (e.g. `result-pattern\`, `ipc\`, `meta\`)
2. **Session files** — one file per meaningful session, accumulated over time

There is no setup step. You don't "open" a story formally. You create a file in a story folder and you're working.

---

## Session File Naming

```
session-NNNN-[short-description]-[type].md
```

| Part | Description |
|---|---|
| `NNNN` | 4-digit zero-padded sequence number, **continuous per story** (never resets after archiving) |
| `short-description` | 2–4 word kebab-case hint of the content |
| `type` | What kind of output this file is (see table below) |

**Valid types:**

| Type | When to use |
|---|---|
| `discussion` | Back-and-forth exploration, no firm conclusion yet |
| `summary` | Condensed record of what was decided or produced |
| `conclusion` | Final answer / decision on a question |
| `analysis` | Deep investigation of a specific subject |
| `generation` | Session primarily produced an artifact (code, doc, config) |
| `opener` | Structured prompt prepared by the user before a session starts |

**The directory already carries story context — never repeat it in the filename.**

✅ `result-pattern\session-0001-make-or-buy-summary.md`  
❌ `result-pattern\result-pattern-session-0001-make-or-buy-summary.md`

---

## The Story Index (`_index.md`)

Each story folder contains a `_index.md` file — a living summary of the story, maintained entirely by the AI.

**Purpose:** the AI reads it at session start to instantly know what was decided, what's still open, and how many sessions have happened — without opening individual session files. It's the AI's memory of the story.

**Contents:**

```markdown
# Story: result-pattern

## Key Decisions
- **Keep custom implementation** (session-0004) — FluentResults/ErrorOr don't fit the model

## Open Questions
- Should Result<T> expose .Value with a guard throw, or stay pure-value?

## Sessions
| # | File | Summary |
|---|---|---|
| 0001 | session-0001-pre-finalization-analysis-discussion.md | Found 5 issues pre-finalization |
| 0002 | session-0002-pre-finalization-fixes-discussion.md | Applied all 5 fixes |
```

**Sections:**

| Section | Content |
|---|---|
| `Key Decisions` | Firm conclusions — things future sessions must not contradict without knowing they exist |
| `Open Questions` | Unresolved threads — things a future session might pick up |
| `Sessions` | One-line index: sequence number, filename, one-sentence summary |

**Rules:**
- Created by the AI on the first save in a story that doesn't have one yet
- Updated automatically by the AI on every subsequent save — no trigger needed from you
- Never a session file itself — no `session-NNNN` prefix
- Never archived — stays in the story root and carries forward across chapters
- Archived sessions remain in the Sessions table with an `[archived]` tag

---

## Session Openers

An opener is an optional structured prompt you prepare before a session. It lets you kick off a focused, well-scoped session without having to explain everything from scratch in chat.

**When to use one:** when you have something specific and structured to set up — a new feature, a planned refactoring, a design discussion with clear goals. For continuation sessions where the `_index.md` already gives the AI enough context, an opener is usually unnecessary.

**Naming:** `session-NNNN-opener[-short-description].md`  
**Example:** `session-0004-opener-process-refinement.md`

The session result file produced in that session is numbered **NNNN** — the same number as the opener. You are responsible for correct numbering; the AI never re-derives the next session number from the file listing when an opener is present.

### Opener Format

Openers use five sections. **Constraints** and **Expected Artifacts** are optional — omit them entirely if not needed.

```markdown
# Session Opener

## Meta
- **Story:** <story-name>
- **Session:** NNNN
- **Short description:** <2-4 word kebab-case>

## Background
1–3 sentences of context — what situation, problem, or prior work led to this session.

## Goals
1. First concrete goal — verifiable when done
2. Second concrete goal

## Constraints
Specific rules: output file paths, naming conventions, language requirements, things to avoid.
(Omit this section if there are no constraints.)

## Expected Artifacts
- `path\to\file.md` — brief description of this file's purpose
(Omit this section for pure discussion sessions that produce no files.)
```

**Section intent:**

| Section | Purpose |
|---|---|
| `Meta` | Machine-readable identity block — story, session number, filename hint |
| `Background` | Context, not specification — gives the AI the "why" in 1–3 sentences |
| `Goals` | The north star — what "done" looks like, independently verifiable |
| `Constraints` | Guard rails — rules the AI must follow that aren't obvious from context |
| `Expected Artifacts` | Exit checklist — the AI verifies every listed file exists before closing the session |

### Opener Template

A blank, annotated template is available at:  
**`.github\.artifacts\session-opener-template.md`**

Copy it, fill it in, and save it to the relevant story folder before starting the session.

### What the AI Does with an Opener

1. Reads the opener at the start of the session
2. Briefly paraphrases it to confirm understanding
3. Asks for clarification if anything is unclear
4. Then begins working

---

## Triggering a Save

Say something like: *"save this session"*, *"write a summary"*, *"log this"*, *"write a conclusion"*.

The AI will **propose a filename** and wait for your confirmation before writing anything. You can accept the suggestion or correct it.

---

## Archiving

Sessions accumulate in the story root over time. When a batch forms a natural unit (a milestone, an iteration, a decision cycle) and you're ready to close it:

**Say:** *"archive the current chapter"*  
**Optionally:** *"archive the current chapter as first-iteration"*

The AI will:
1. Move all session files currently in the story root into:  
   `archives\archive-NNN\` or `archives\archive-NNN-optional-name\`  
   (3-digit counter, increments per story)
2. Session numbering **continues from where it left off** — never resets
3. If the batch contained 3 or more sessions, the AI will offer (not require): *"Want a brief conclusion file in the archive?"* — if yes, content is negotiated on the fly

Archives are **sealed after creation** — never modified.

---

## Directory Layout — Full Example

```
.github\stories\
  result-pattern\                                              ← active story
    _index.md                                                  ← AI-maintained, never archived
    session-0001-pre-finalization-analysis-discussion.md
    session-0002-pre-finalization-fixes-discussion.md
    session-0003-stabilization-finalization-discussion.md
    session-0004-make-or-buy-summary.md

  ipc\                                                         ← active story with archives
    _index.md
    session-0005-echo-demo-generation.md
    session-0006-jipdispatcher-summary.md
    archives\
      archive-001-first-stable-poc\                            ← sealed
        session-0001-...md
        ...
      archive-002-stable-poc-v2\                               ← sealed
        ...

  meta\                                                        ← active story
    _index.md
    session-0001-session-process-description.md
    session-0004-opener.md                                     ← opener (placed by user)
    session-0004-process-refinement-generation.md              ← result produced from opener
```

---

## Rules at a Glance

| Rule | Detail |
|---|---|
| Never overwrite | Each session appends a new file — existing files are immutable |
| Never reset numbering | 4-digit counter per story, continuous across archives |
| Never modify archives | Sealed after creation |
| Filename never repeats folder | The directory is the story context |
| AI always proposes, you confirm | No file is written without your approval |
| Conclusion is optional | Only on explicit request; negotiated on the fly; no fixed structure |
| Opener is optional | User prepares it; AI reads, paraphrases, then works; result file gets opener's number |
| `_index.md` is maintained by AI | Created on first save, updated on every subsequent save |
| `_index.md` is never archived | Stays in story root, carries forward across chapters |

---

## Setting This Up for a Repository

Add the following to your `.github\copilot-instructions.md` (or `AGENTS.md`):

```
Stories are stored under `.github\stories\<story>\`.
Session file naming: session-NNNN-[short-description]-[type].md
(NNNN = 4-digit zero-padded, continuous per story; type = discussion|summary|conclusion|analysis|generation|opener)

Opener: if a session-NNNN-opener[-short-description].md exists at session start, read it first,
paraphrase briefly, ask for clarification if needed, then proceed.
Opener format: ## Meta (Story, Session, Short description) / ## Background / ## Goals /
## Constraints (optional) / ## Expected Artifacts (optional).
The result file is numbered NNNN (the opener's own number — user is responsible for correct
numbering). Never re-derive the next number from the file listing when an opener is present.
A blank template is at .github\.artifacts\session-opener-template.md.

Story index: each story folder has a _index.md maintained by the AI. Read it at session start
for instant context. Create it on first save if absent; update it on every subsequent save
(Key Decisions, Open Questions, Sessions table). Never archive _index.md.

Archiving: "archive the current chapter [as <name>]" → move session files to
archives\archive-NNN[-name]\, offer conclusion if 3+ sessions. _index.md stays in story root.
Session numbering never resets.
```

---

## Closing Reflection

Most "AI memory" solutions are either black-box embeddings the human can't inspect, or heavyweight external systems. What this method gives you is:

- **Fully transparent** — it's just files you can read, edit, move, diff, and version in git
- **Human-readable first** — you are never locked out of your own context
- **AI-maintainable** — the AI updates the index as a side effect of normal work
- **Zero infrastructure** — no database, no API, no sync service

The interesting idea underneath it: *the file system as shared working memory between human and AI, with agreed conventions on structure.* Simple enough to survive contact with reality. Opinionated enough to actually be useful.
