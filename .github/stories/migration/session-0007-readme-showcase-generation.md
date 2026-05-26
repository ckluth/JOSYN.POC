# Session 0007 — README Showcase Generation

**Story:** migration  
**Date:** 2026-05-26  
**Type:** generation

---

## Goal

Write and update all READMEs in JOSYN.POC to showcase quality — German-language,
architecture-first, with Mermaid diagrams where sensible.

---

## Scope

| File | Action |
|---|---|
| `README.md` (root) | Stub → full showcase (German, Mermaid system flow + dependency graph + JAP sequence) |
| `JOSYN.Foundation.PropertyBag/README.md` | Stub → full German logical-repo description |
| `JOSYN.Foundation.PropertyBag/JOSYN.Foundation.PropertyBag/README.md` | English → German (full content) |
| `JOSYN.Foundation.JIP/JOSYN.Foundation.JIP/README.md` | Fix outdated known-limitations (ClientPipes/ServerPipes are sealed class); ASCII → Mermaid sequence diagram |
| `JOSYN.System.Shared.Log/README.md` | Fix log path (`%TEMP%\…` → `<ExeDir>\logs\`); fix EnableConsoleOutput description |
| `JOSYN.System.Shared.Contract/README.md` | Fix ErrorReport serialization format (INI → JSON); ASCII → Mermaid sequence diagram |
| `JOSYN.System.Frontend.JobHost/README.md` | English → German rewrite; fix package ref (`JOSYN.System.Contract` → `JOSYN.System.Shared.Contract`); ASCII → Mermaid flowchart |
| `JOSYN.System.Backend.JAPServer/README.md` | English → German rewrite; fix package ref; ASCII → Mermaid flowchart |

**Not touched:** `JOSYN.Foundation.ResultPattern/README.md` — already correct, complete, and German.

---

## Key Decisions

- All READMEs: **German** (consistent with the existing high-quality ones: ResultPattern, JIP, Contract)
- Root README: **showcase-first** (architecture overview for colleague-readers, not a quickstart)
- Mermaid diagrams added to: root (system flow, dependency graph, JAP sequence), JIP (two-pipe sequence), Contract (JAP sequence), JobHost (dispatch flowchart), JAPServer (dispatch flowchart)
- PropertyBag logical repo root README: now describes the logical repo + links to project-level README
- PropertyBag project README: promoted to full German content (translation of the English original)

---

## Corrections Applied

| Finding | Fix |
|---|---|
| JIP known-limitations listed `ClientPipes`/`ServerPipes as record` — already fixed to `sealed class` | Row removed |
| JIP known-limitations listed "kein direktes async-Interface" — async handlers already in place | Row removed |
| Log README: log path was `%TEMP%\JOSYN\…` (obsolete) | Updated to `<ExeDir>\logs\` |
| Log README: said "Debug-Build only" for console output | Updated: `EnableConsoleOutput` flag, set by both `Core.cs` (JobHost) and `Host.cs` (JAPServer) |
| Contract README: ErrorReport said "via PropertyBag (INI-Format)" | Updated to JSON (INI insufficient for multiline CallStack/ExceptionDetails) |
| JobHost/JAPServer READMEs: referenced `JOSYN.System.Contract` (old name) | Updated to `JOSYN.System.Shared.Contract` |
| JobHost README: attribute `[ParallelExecutionAllowed]` (wrong) | Corrected to `[ParallelExecutionAllowed(bool)]` (takes isAllowed parameter) |

---

## Mermaid Diagram Summary

| File | Diagram type | Shows |
|---|---|---|
| Root README | `flowchart LR` | System components + data flow |
| Root README | `sequenceDiagram` | JAP 3-call protocol |
| Root README | `graph TD` | NuGet dependency graph |
| JIP README | `sequenceDiagram` | Two-pipe client↔server exchange |
| Contract README | `sequenceDiagram` | JAP 3-call protocol (detail) |
| JobHost README | `flowchart TD` | Core.Run dispatch chain |
| JAPServer README | `flowchart TD` | Host.Run server architecture |
