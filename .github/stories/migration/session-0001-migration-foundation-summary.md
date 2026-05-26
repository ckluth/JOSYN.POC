# Session 0001 — Migration Foundation

**Date:** 2026-05-26
**Type:** summary

---

## Context

The PoC phase is declared complete. JOSYN has reached a point where gold-plating threatens forward progress. The goal is a clean, sealed milestone in the current repo and a fresh start in **JOSYN.POC** — a new repo that serves as an *architecture showcase* and collaboration ground for colleagues (readers/reviewers, not contributors).

---

## New Repo Character

**JOSYN.POC** is a *mature PoC v2*. Its primary purpose is:

1. **Architecture showcase** — the complete layered system (Foundation + System layers) as a reference design
2. **Collaboration ground** — gives colleagues a structured artifact for discussion; marketing for continuing is a latent aspect
3. **Solo-development base** — single user continues development; colleagues are readers

The new repo will NOT be a NuGet library suite in the public sense. It is not yet an open collaboration workspace.

---

## Migration Scope

### What migrates

| Area | Content |
|------|---------|
| **Codebase** | Full `JOSYN.Foundation/` (ResultPattern, PropertyBag, JIP) + full `JOSYN.System/` (Frontend, Backend, Shared) |
| **Build infrastructure** | All `.local-build/` scripts (root + sub-repos), `Directory.Build.props`, `nuget.config`, `Local Packages/` |
| **Docs** | `docs/High-Level-Architecture.pptx` |
| **Legal** | `LICENCE`, `.gitignore` |
| **Agent layer** | `.github/copilot-persona.md`, `.github/.artifacts/` (story-method.md, session-opener-template.md, testing-with-filesystem-sideeffects.md, konvention_und_vertrag.md) |
| **Agent layer** | `.github/copilot-instructions.md` — rebuilt and updated (see below) |
| **Agent layer** | `.github/stories/migration/` — this story (remaining sessions) continues into the new repo |

### What does NOT migrate

| Area | Reason |
|------|--------|
| **Git history** | Fresh repo — clean slate |
| **Story session files** | The PoC journey (all stories: poc, ipc, result-pattern, property-bag, meta) stays in JOSYN as history |
| **`poc/evolution` branch** | New repo starts on `main` |

### Story Method treatment

The Story Method infrastructure fully migrates:
- All `.github/.artifacts/` documents
- `.github/copilot-persona.md`
- The `migration` story continues from where it left off (sessions already in JOSYN stay there; JOSYN.POC picks up the next session number)

The story session content (the journey) does not migrate — `_index.md` per story is also not migrated, since the stories themselves do not continue in the new repo.

A fresh `.github/stories/` is created in JOSYN.POC with only the active sessions of the migration story copied over.

---

## Pre-Migration Fixes (in JOSYN, before migrating)

The IPC story has documented open issues. Decision: **fix before migrating** — JOSYN.POC should start with a clean slate.

| Issue | Scope | Notes |
|-------|-------|-------|
| ~~Async request handler~~ | ~~`Func<byte[], byte[]>` → `Func<byte[], Task<byte[]>>`~~ | ✅ **Already fixed** — applied in `evolution/ipc-issue-blocking-requesthandler`; confirmed by code inspection 2026-05-26 |
| ~~`ClientPipes`/`ServerPipes` as `record`~~ | ~~Change to `sealed class`~~ | ✅ **Already fixed** — both are `sealed class` in current code; confirmed 2026-05-26 |
| JIP NuGet not packed | Run `.local-build/pack.cmd` for JIP | Simple |
| Single-in-flight protocol | Document as known limitation for PoC v2 | No protocol redesign planned |

**Also discovered during this session:** `.github/copilot-instructions.md` is outdated — still references the old `JOSYN.Core` structure. This must be updated before migration (it will be rebuilt as part of the agent-layer migration anyway).

---

## New Repo Qualities (JOSYN.POC)

| Aspect | Target |
|--------|--------|
| **Branch** | `main` (clean linear history) |
| **README** | Showcase document: architecture overview, layer diagram, building-block descriptions — written for colleague-readers |
| **copilot-instructions.md** | Fully updated: new repo name (`JOSYN.POC`), current structure (Foundation + System layers), all conventions carried over |
| **Code quality** | Same as JOSYN after pre-migration fixes (sealed types, English XML docs, Result pattern, 238+ tests passing) |
| **Story method** | Fully operational from session 1 in JOSYN.POC |

---

## Phase Plan

### Phase 1 — Pre-migration (in JOSYN)

| Session | Scope |
|---------|-------|
| ~~0002~~ | ~~IPC async handler rewrite~~ — **cancelled; already fixed** |
| 0002 | `ClientPipes`/`ServerPipes` → sealed class; pack JIP NuGet; build-all / test-all; seal JOSYN (tag + final commit) |

### Phase 2 — Physical migration

| Session | Scope |
|---------|-------|
| 0004 | Code migration (JOSYN → JOSYN.POC): all sub-repos, build scripts, infra |
| 0005 | Agent layer reconstruction in JOSYN.POC: copilot-instructions.md (updated), persona, artifacts, story-method |

### Phase 3 — New repo polish

| Session | Scope |
|---------|-------|
| 0006 | Showcase README for JOSYN.POC |
| 0007 | Validation: build-all, test-all, first commit in JOSYN.POC |

*Phase and session assignments are estimates — actual work may compress or expand across sessions.*

---

## Open Questions

| # | Question |
|---|---------|
| 1 | Async handler scope: full cascade rewrite (PipesServer + JipServer + JipDispatcher + Demo) in one session, or split across two? |
| 2 | Should the async handler also change the JipClient to be async throughout, or is the server side enough for PoC v2? |
| 3 | Does JOSYN need a visible "sealed milestone" marker (e.g. Git tag `v1.0-poc-milestone`, branch rename)? |
