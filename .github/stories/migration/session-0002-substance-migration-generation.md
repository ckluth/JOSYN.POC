# Session 0002 — Pre-Migration Audit & Substance Migration

**Date:** 2026-05-26
**Type:** generation

---

## Scope

This session covers two consecutive phases:

1. **Pre-migration audit** — verifying the state of JOSYN before any files move; correcting stale information across story indexes and instructions
2. **Substance migration** — physical copy of all code, build infrastructure, and docs from JOSYN → JOSYN.POC

The agent layer (copilot-instructions, persona, artifacts, story seeding) is explicitly **out of scope** — handled in session 0003.

---

## Part 1: Pre-Migration Audit (in JOSYN)

The migration story `_index.md` and session-0001 listed three pre-migration fixes. Each was verified against the actual code. Results:

---

### 1.1 Async Request Handler

**Claimed:** `Func<byte[], byte[]>` (sync) must be changed to `Func<byte[], Task<byte[]>>` (async).

**Verification:**

| File | Finding |
|------|---------|
| `PipesServer.cs` | Handler signature: `Func<byte[], Task<byte[]>>` ✅ |
| `ServerStartArguments.cs` | `HandleStringRequest: Func<string, Task<string>>?`, `HandleRawRequest: Func<byte[], Task<byte[]>>?` ✅ |
| `RequestLoopAsync` | `response = await processRequest(requestBytes)` ✅ |
| `JipServer.cs` | Both sync-wrapping and proper `async` overload of `WrapHandler` present ✅ |
| `JipDispatcher.cs` | All handlers `Func<string?, Task<Result<string?>>>`, constructor uses async overload ✅ |
| `JipClient.cs` | Always was async ✅ |

**Conclusion: Already fixed.** Applied in branch `evolution/ipc-issue-blocking-requesthandler`; reviewed in `ipc/archives/archive-001-first-stable-poc/ipc-discussion-session-002.md`. The IPC `_index.md` Open Questions section was simply outdated.

**Documents updated:**
- `ipc/_index.md` — moved "Async request handler" from Open Questions to new **Resolved** section
- `migration/_index.md` — added Key Decision "Async handler already fixed"; struck out related open question
- `migration/session-0001-…-summary.md` — struck out async handler row in pre-migration fixes table; collapsed sessions 0002+0003 into single session 0002

---

### 1.2 ClientPipes / ServerPipes: record → sealed class

**Claimed:** Both types are `record` but should be `sealed class`.

**Verification:**

| File | Finding |
|------|---------|
| `Types/ClientPipes.cs` | `public sealed class ClientPipes : IClientPipes` ✅ |
| `Types/ServerPipes.cs` | `public sealed class ServerPipes : IServerPipes` ✅ |

`ClientPipes` additionally carries a private `int isBusy` field with `Interlocked`-based `TrySetBusy()` / `ClearBusy()` — correct mutable state justifying `sealed class` over `record`.

**Conclusion: Already fixed.**

**Documents updated:**
- `ipc/_index.md` — struck out record→sealed class from Open Questions
- `migration/_index.md` — added Key Decision "ClientPipes/ServerPipes already fixed"
- `migration/session-0001-…-summary.md` — struck out that row in the fixes table

---

### 1.3 JOSYN.Core → JOSYN.Foundation in copilot-instructions.md

**Finding:** `copilot-instructions.md` still referenced the old `JOSYN.Core` structure (deleted months ago) and listed two already-fixed limitations as current.

**Changes made to `JOSYN/.github/copilot-instructions.md`:**

| Section | Before | After |
|---------|--------|-------|
| Repository Layout | `JOSYN.Core/` with 3 sub-repos; `JOSYN.JobRunner/`; `JOSYN.System/SessionServer/` | `JOSYN.Foundation/` (3 repos) + `JOSYN.System/` (3 repos) + `.local-build/` |
| Result Pattern intro | "JOSYN.Core.ResultPattern is…" | "JOSYN.Foundation.ResultPattern is…" |
| IPC Known Limitations | Listed async handler + record types as open issues | Single-in-flight only; async/sealed noted as already in place |
| Static entry points | Listed `PipesServer`, `PipesClient`, `PipesProtocol`, `PropertyBag` | Added `JipServer`, `JipClient`, `JipDispatcher` |

---

### 1.4 Phase Plan correction

After all three pre-migration items were confirmed already fixed, the phase plan in session-0001 was corrected:

- Session "0002 — IPC async handler rewrite" → **cancelled, struck out**
- Sessions 0002+0003 of the original plan → collapsed into **single session 0002** (this session)

---

## Part 2: Substance Migration (JOSYN → JOSYN.POC)

**Target:** `C:\Users\chris\OneDrive\DevGit\JOSYN.POC`  
**Method:** Pure file copy — no code changes.

### Files Copied

| Source | Destination | Detail |
|--------|-------------|--------|
| `JOSYN\JOSYN.Foundation\` | `JOSYN.POC\JOSYN.Foundation\` | 113 files — ResultPattern, PropertyBag, JIP (all projects + tests + demo) |
| `JOSYN\JOSYN.System\` | `JOSYN.POC\JOSYN.System\` | 74 files — Frontend, Backend, Shared |
| `JOSYN\.local-build\` | `JOSYN.POC\.local-build\` | `all.cmd`, `build-all.cmd`, `demo.cmd`, `demo.debug.cmd`, `test-all.cmd` |
| `JOSYN\docs\` | `JOSYN.POC\docs\` | `High-Level-Architecture.pptx` |
| `JOSYN\Local Packages\` | `JOSYN.POC\Local Packages\` | `cleanup-nuget-cache.cmd` (NuGet packages are generated, not source-controlled) |

**Not touched:** `.gitignore`, `LICENCE`, `README.md` — already existed in JOSYN.POC from repo init.

### Verification

#### Build-all (`JOSYN.POC\.local-build\build-all.cmd`)

| Sub-Repo | Result |
|----------|--------|
| JOSYN.Foundation.ResultPattern | ✅ 0 errors |
| JOSYN.Foundation.PropertyBag | ✅ 0 errors |
| JOSYN.Foundation.JIP | ✅ 0 errors |
| JOSYN.System.Shared (Contract + Log) | ✅ 0 errors |
| JOSYN.System.Backend | ✅ 0 errors |
| JOSYN.System.Frontend | ✅ 0 errors |

**All 6 sub-repos built successfully.**

#### Pack — NuGet packages generated in `JOSYN.POC\Local Packages\`

| Package | Version |
|---------|---------|
| `JOSYN.Foundation.ResultPattern` | `1.0.0-preview01` |
| `JOSYN.Foundation.PropertyBag` | `1.0.0-preview01` |
| `JOSYN.Foundation.JIP` | `1.0.0-preview01` |
| `JOSYN.System.Shared.Contract` | `1.0.0-preview01` |
| `JOSYN.System.Shared.Log` | `1.0.0-preview01` |
| `JOSYN.System.Frontend.JobHost` | `1.0.0-preview01` |

**7 packages total** (Shared produces 2).

#### Test-all (`JOSYN.POC\.local-build\test-all.cmd`)

| Suite | Passed | Failed | Skipped |
|-------|--------|--------|---------|
| `JOSYN.Foundation.ResultPattern.Test` | 141 | 0 | 0 |
| `JOSYN.Foundation.PropertyBag.Test` | 61 | 0 | 0 |
| `JOSYN.Foundation.JIP.Test` | 48 | 0 | 0 |
| `JOSYN.Foundation.JIP.Demo.ServerExe.Test` | 1 | 0 | 0 |
| **Total** | **161** | **0** | **0** |

**161/161 tests passing. Substance migration verified.**
