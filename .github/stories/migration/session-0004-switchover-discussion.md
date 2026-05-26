# Session 0004 — Migration Switchover Discussion

**Date:** 2026-05-26
**Type:** discussion

---

## Context

With Phase 2 complete (substance + agent layer both migrated, 161/161 tests green), the question
arose: *when exactly is the safe moment to switch to JOSYN.POC and never look back?*

---

## Key Discussion Points

### The "Two Worlds" Problem

Straddling two repos is a dangerous state. The risk is not technical — it's cognitive: the AI (and
the human) can silently fill gaps by reaching back into JOSYN without noticing. The goal is to
leave this state as quickly as possible.

### Three Layers of "Looking Back"

| Layer | Description | Rule |
|-------|-------------|------|
| Physical file reads | AI reads files from `JOSYN\` while working in JOSYN.POC | **Forbidden. Hard line.** |
| Session history | AI queries session_store and pulls JOSYN-era sessions | **Forbidden.** The seeded sessions 0001–0003 exist to make this unnecessary. |
| Human as bridge | User recalls something and re-provides it verbally | **Allowed — once.** Triggers immediate write into JOSYN.POC. Never a recurring crutch. |

### "Never Look Back" as Proof of Migration Success

The rule is not just a discipline — it is the **diagnostic test**:

> If the AI can work effectively in JOSYN.POC using only what is in JOSYN.POC, the migration
> is complete. If it cannot — it found a gap. Fix the gap there. Never fill it by looking at JOSYN.

Every forced glance backward is a migration defect, not a feature.

### When Is the Safe Moment?

All structural prerequisites are already met:
- ✅ Code, build, tests (161/161)
- ✅ Agent layer (copilot-instructions rebuilt, persona, artifacts)
- ✅ Historical context seeded (migration sessions 0001–0003 in JOSYN.POC)
- ✅ README (Phase 3) is independent — reads JOSYN.POC's own code, needs nothing from JOSYN

**Conclusion: the safe moment is now.**

---

## Decisions Made

### Rule (two sentences)

> The AI working in JOSYN.POC **never reads files from JOSYN** and **never queries session
> history for JOSYN-era context**. If context is missing, the human provides it once — and it
> gets written into JOSYN.POC before the session continues.

### Switch Plan

1. Sync this session to both repos (last two-worlds operation)
2. Start fresh session from within JOSYN.POC
3. First task: write the showcase README — serves as live test of self-sufficiency
4. If the human bridge is needed, the gap is identified, written into JOSYN.POC, and closed

### Session Numbering

- JOSYN migration story closes at session **0004** (this file)
- JOSYN.POC migration story continues from session **0004** onward (README session)

---

## Migration Status After This Session

| Phase | Status |
|-------|--------|
| Phase 1 — Pre-migration fixes | ✅ Complete |
| Phase 2 — Substance migration | ✅ Session 0002 |
| Phase 2 — Agent layer migration | ✅ Session 0003 |
| Phase 3 — Showcase README | 🔲 First task in JOSYN.POC (session 0004 there) |
| **JOSYN migration story** | 🔒 **Closed at this session** |
