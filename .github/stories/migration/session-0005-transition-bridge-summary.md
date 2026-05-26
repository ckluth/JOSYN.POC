# Session 0005 — Transition Bridge

**Story:** migration  
**Session:** 0005  
**Type:** summary  
**Date:** 2026-05-26  
**Purpose:** Status-quo snapshot from the sealed JOSYN treasure. Comprehensive bridge document capturing everything relevant for JOSYN.POC's knowledge base. To be demolished after successful transfer.

---

## 1. The Big Picture

JOSYN.POC is a **job execution system**. A job is a standalone `.exe` whose single method (marked `[JobEntryPoint]`) is dispatched by the **JobHost** library. The JobHost connects to a **JAPServer** (a background process) via **JIP** (named-pipe IPC). The application-level protocol between them is **JAP** — three calls: `GetRawArguments()`, `PutRawResult(string)`, `PutError(string)`.

The PoC is **functionally complete and end-to-end runnable.** All NuGet packages are packed. Demo launches with `demo.cmd`.

---

## 2. Repository Map (Confirmed vs JOSYN.POC on disk)

```
JOSYN.POC/
├── .github/                             ← agent layer (already in place)
├── .local-build/                        ← root orchestration scripts
├── docs/                                ← High-Level-Architecture.pptx
├── Local Packages/                      ← local NuGet feed
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/  ✅ NuGet 1.0.0-preview01
│   ├── JOSYN.Foundation.PropertyBag/    ✅ NuGet 1.0.0-preview01
│   └── JOSYN.Foundation.JIP/            ✅ NuGet 1.0.0-preview01
└── JOSYN.System/
    ├── JOSYN.System.Frontend/           (namespace grouping layer)
    │   ├── JOSYN.System.Frontend.JobHost/  ✅ NuGet 1.0.0-preview01
    │   └── JOSYN.MyDemoJob/             demo exe (not packed)
    ├── JOSYN.System.Backend/            (namespace grouping layer)
    │   └── JOSYN.System.Backend.JAPServer/ exe, NOT packed
    └── JOSYN.System.Shared/             (namespace grouping layer)
        ├── JOSYN.System.Shared.Contract/ ✅ NuGet 1.0.0-preview01
        └── JOSYN.System.Shared.Log/     ✅ NuGet 1.0.0-preview01
```

**Confirmed absent:** `JOSYN.System.Contract/` — superseded, already gone from JOSYN.POC. ✅

**Grouping layers** (`JOSYN.System.Frontend/`, `JOSYN.System.Backend/`, `JOSYN.System.Shared/`) are pure namespace containers — no code, just `.slnx` + scaffold. Concrete projects live one level deeper with fully-qualified names.

---

## 3. Building Blocks — Purpose & Dependencies

### 3.1 `JOSYN.Foundation.ResultPattern`

**Purpose:** The error-handling foundation. No exceptions above the catch boundary. Every operation returns `Result` (void) or `Result<T>`. Failures carry error message + accumulated call chain (`CallerInfo` list).

**Dependencies:** None. Zero external dependencies.

**API at a glance:**
```csharp
return Result.Success;
return Result<T>.Success(value);        // or implicit: return value;
return Result.Error("Fehlermeldung");   // creates Error, implicitly converts
catch (Exception ex) { return ex; }    // lowest layer only
var inner = SomeOp(); if (!inner.Succeeded) return Result.Propagate(inner);
```

**Key rule:** `Result.Propagate(inner)` — never re-wrap a failure manually.

---

### 3.2 `JOSYN.Foundation.PropertyBag`

**Purpose:** Serializes/deserializes C# `record` types to/from `Dictionary<string,string>`, then to sectionless INI or JSON. Used to pass job arguments and results across the pipe.

**Dependencies:** `JOSYN.Foundation.ResultPattern`

**API at a glance:**
```csharp
PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
PropertyBag.Deserialize<MyRecord>(rawString);  // auto-detects: { → JSON, else INI
```

**Key facts:**
- Records only — validates via `<Clone>$` method presence
- Culture: `de-DE` (number/date formatting)
- Both record styles supported: init-property style AND primary-constructor (positional) style
- `DateTimeOffset` is a supported property type
- INI format is whitespace-exact — no trimming; leading spaces are captured
- Property name matching is case-insensitive on the first character when deserializing
- Culture setup is the **host process's responsibility** — the library does NOT set `DefaultThreadCurrentCulture`
- `JosynCulture.Default` = `de-DE` is a compile-time constant; do not make it runtime-configurable

---

### 3.3 `JOSYN.Foundation.JIP`

**Purpose:** JOSYN Interprocess Protocol. Named-pipe transport in two layers: raw byte transport + JSON request/response convention. Session-isolated (per session key).

**Dependencies:** `JOSYN.Foundation.ResultPattern`

**Two-layer design:**
- **Layer 1 — Transport** (`PipesServer`, `PipesClient`, `PipesProtocol`): bytes only; length-prefix framing (`int32` LE + raw bytes); two pipes per session (`req-pipe-<key>`, `res-pipe-<key>`)
- **Layer 2 — Convention** (`JipServer`, `JipClient`, `JipProtocol`, `JipDispatcher`): JSON `Request { What, Data? }` / `Response { Succeeded, Data?, Error? }`

**API at a glance:**
```csharp
// Server side
var args = new ServerStartArguments {
    SessionKey = sessionKey,
    HandleStringRequest = async req => await dispatcher.Dispatch(req),
    HandleErrorNotification = async (req, ex) => { /* log */ },
    IsCancellationRequested = () => Task.FromResult(shouldStop),
};
await PipesServer.RunAsync(args, reConnect: true);

// Client side
var pipes = (await PipesClient.ConnectAsync(sessionKey)).Value;
var response = await JipClient.SendAsync(pipes, "MethodName", optionalData);
await PipesClient.DisconnectAsync(pipes);

// Dispatcher (server side)
var dispatcher = new JipDispatcher()
    .Register("PING", Result<string?>.Success(null))
    .RegisterAll<IMyProtocol>(implementation);
```

**`IsCancellationRequested`** — converted internally to a polling `CancellationToken`; callers pass a simple async predicate; no `CancellationToken` management required.

**Known PoC limitation (documented, do not fix unless asked):**
- Protocol is single-in-flight — strictly sequential, no request IDs.

---

### 3.4 `JOSYN.System.Shared.Contract`

**Purpose:** The JAP protocol contract. Transport-agnostic. Shared by both Frontend and Backend.

**Dependencies:** `JOSYN.Foundation.ResultPattern` only (NOT JIP — contract is transport-agnostic)

**Contents:**
```csharp
public interface IJosynApplicationProtocol {
    Task<Result<string>> GetRawArguments();
    Task<Result>         PutRawResult(string rawResult);
    Task<Result>         PutError(string serializedErrorReport);
}

record ErrorReport(string Message, string? CallStack, string? ExceptionDetails, DateTimeOffset OccurredAt);
```

`ErrorReport` is serialized as **JSON** via PropertyBag when passed to `PutError` (INI was insufficient — it truncated multi-line `CallStack`/`ExceptionDetails`).

---

### 3.5 `JOSYN.System.Shared.Log`

**Purpose:** `LocalLog` — process-local file logger. Static. Never throws. Flush-on-write.

**Dependencies:** `JOSYN.Foundation.ResultPattern`

**Key facts:**
- `LogDirectory` is a settable `public static string` (used as test seam)
- `EnableConsoleOutput` flag mirrors output to Console
- Causer overloads write to `Path.Combine(LogDirectory, causer)` subfolder
- `FormatEntry` is `internal` (promoted for testability)
- `[NonParallelizable]` required in tests — `LogDirectory` and `EnableConsoleOutput` are shared static state
- **Ambiguity:** `LocalLog.Error(string, string)` is ambiguous between `Error(message, callStack)` and `Error(causer, message)` — always use named parameters

**⚠️ Open Question #1:** Ariadne v5 says log path is `%TEMP%\JOSYN\<ProcessName>\<date>.log`; Key Decisions say `<ExeDir>\logs\`. These contradict. Which is current?

---

### 3.6 `JOSYN.System.Frontend.JobHost`

**Purpose:** The job developer's library. A job author references this, marks one method `[JobEntryPoint]`, calls `Core.Run(args)` from `Program.cs` — the library handles everything else.

**Dependencies:** ResultPattern, PropertyBag, JIP, Shared.Contract, Shared.Log

**Job dispatch flow:**
```
Core.Run(args)
  ├── JAPClient.CreateConnectedClient(args)   parse sessionKey + JIP connect
  └── JobInvoker.InvokeJob(japClient)
       ├── FindJobFunction(IEnumerable<Type>)  [JobEntryPoint] via reflection
       ├── CreateInvocationArguments           GetRawArguments() → PropertyBag.Deserialize
       ├── [User's job method]                 pure business logic
       └── ProcessJobResult                    PropertyBag.Serialize → PutRawResult()
```

**Error routing:**
```
Pipe connection failed       → LocalLog.Error only                      exit -1
Job error, pipe alive        → LocalLog.Error + PutError to server      exit -2
PutError itself fails        → LocalLog.Error (fallback)                still exit -2
```

**Key design notes:**
- `JobInvoker.FindJobFunction` takes `IEnumerable<Type>` (not `Assembly` directly) — `InvokeJob` delegates via `GetExportedTypes()` at runtime
- `internal InvokeJob(IJosynApplicationProtocol, IEnumerable<Type>)` overload exists for testing — supplies types directly, bypasses assembly scanning
- Reflection in `JobInvoker` is **intentional design** — the extension point for job authors; do not flag it
- `JAPClient` is `internal` — implements `IJosynApplicationProtocol` via JIP transport
- `ArgumentsComparer` is `internal` delegate type

**⚠️ Open Question #2:** `ArgumentsComparer` — what is its actual purpose? Is it used anywhere, or placeholder?

**CLI convention:** Server passes `"JOSYN-IPC <sessionKey>"` as args to the client exe.

---

### 3.7 `JOSYN.System.Backend.JAPServer`

**Purpose:** The backend process. Bridges the two sides: starts, listens on named pipe, launches job exe, handles the three JAP calls, shuts down cleanly.

**Dependencies:** ResultPattern, JIP, Shared.Contract, Shared.Log

**Key internals:**
- `Program.cs` → `Host.Run(args)` (one line)
- `Host.cs` — server lifecycle, ESC-key shutdown (`WasEscapePressed`), graceful drain, all logging via LocalLog
- `JAPServer.cs` — `IJosynApplicationProtocol` implementation (3 methods)
- `FakeReadArgumentsFromFile` — **intentionally fake/hardcoded** for PoC scope; not a bug

**This is an exe, NOT a NuGet package.** Never packed.

**Demo session key (hardcoded in both `launchSettings.json`):**
`dea5611d-d740-437f-ad93-7a5dc5ae4299`

---

### 3.8 `JOSYN.MyDemoJob`

**Purpose:** Demo job exe. One method marked `[JobEntryPoint]`. Lives under `JOSYN.System.Frontend/` in the solution but is NOT packed. `Program.cs` is one line: `Core.Run(args)`.

---

## 4. Test Projects — Current State

6 test projects confirmed present in JOSYN.POC:

| Project | Notes |
|---|---|
| `JOSYN.Foundation.ResultPattern.Test` | |
| `JOSYN.Foundation.PropertyBag.Test` | |
| `JOSYN.Foundation.JIP.Test` | |
| `JOSYN.Foundation.JIP.Demo.ServerExe.Test` | |
| `JOSYN.System.Frontend.JobHost.Test` | Refactored: 1 project (was 3); `JobInvokerTestSupport.cs` holds internal stubs; `JobInvokerTests.cs` |
| `JOSYN.System.Shared.Log.Test` | `LocalLogTests.cs` (I/O, NonParallelizable) + `LocalLogFormatEntryTests.cs` (pure) |

**⚠️ Test count uncertain:** Story docs show various counts across sessions (161 → 238 → ~276). Recommend running `test-all.cmd` to establish current baseline before any work.

---

## 5. Build System

**Dependency order (critical for NuGet):**
```
1. JOSYN.Foundation.ResultPattern  → pack
2. JOSYN.Foundation.PropertyBag    → pack  (depends on 1)
3. JOSYN.Foundation.JIP            → pack  (depends on 1)
4. JOSYN.System.Shared             → build → pack Contract + Log  (depends on 1)
5. JOSYN.System.Frontend.JobHost   → pack  (depends on 1,2,3,4)
6. JOSYN.System.Backend.JAPServer  → build only — exe  (depends on 1,3,4)
```

**Root `.local-build/` scripts:**

| Script | Purpose |
|---|---|
| `build-all.cmd` | Crystal-clean: clear nupkg + NuGet cache → build+pack all 6 in order |
| `test-all.cmd` | `dotnet test` all solutions |
| `all.cmd` | `build-all.cmd` + `test-all.cmd` |
| `demo.cmd` | Launch JAPServer + MyDemoJob [Release] in separate console windows |
| `demo.debug.cmd` | Build Debug + launch both |

---

## 6. Conventions (Non-Negotiable)

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified |
| Data | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` — lowest layer only |
| Propagation | `Result.Propagate(inner)` — never re-wrap |
| Interfaces | `static abstract` members in `Contracts/` folder; `/// <inheritdoc cref="IXxx"/>` on static impls, `/// <inheritdoc/>` on non-static |
| Error messages | **German** |
| XML docs | English; on interfaces/contracts; `<inheritdoc>` on implementations |
| Culture | `de-DE` default — applied by host process at startup |
| Nullability | `Nullable` enabled; `?` is deliberate |
| Namespace pragma | `#pragma warning disable/restore IDE0130` when folder path ≠ namespace |
| csproj templates | 3 canonical templates: NuGet Library / Exe / Test |

---

## 7. Open Items (What Is Not Yet Done)

1. 🔲 **Showcase README** — root `README.md` is a one-liner placeholder; needs architecture overview for colleague-readers
2. 🔲 **Phase 2 planning** — async JIP, multi-job scheduling, real argument source, proper process management *(future horizon)*

---

## 8. Open Questions

1. **`LocalLog` path:** Ariadne v5 says `%TEMP%\JOSYN\<ProcessName>\<date>.log`; Key Decisions say `<ExeDir>\logs\`. Which is current?
2. **`ArgumentsComparer`:** Internal delegate type in JobHost — what is its actual purpose? Is it used anywhere, or a placeholder?
3. **`v0.1.0-poc` git tag:** Was this ever applied to JOSYN, or still pending?
4. **Showcase README:** Is this still the immediate next task, or has something else moved to the top of the queue?

---

*Bridge document — to be demolished after all content has been transferred into JOSYN.POC's `copilot-instructions.md` and `_index.md`.*
