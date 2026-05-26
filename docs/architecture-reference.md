# JOSYN.POC — Architecture Reference

> **Purpose:** Human-readable system overview for colleague-readers and reviewers.
> Authoritative as of session-0006 (2026-05-26). Intended starting point for the showcase README.

---

## The Big Picture

JOSYN.POC is a **job execution system**. A *job* is a standalone `.exe` whose single method (marked `[JobEntryPoint]`) is dispatched by the **JobHost** library. The JobHost connects to a **JAPServer** (a background process) via **JIP** (named-pipe IPC). The application-level protocol between them is **JAP** — three calls: `GetRawArguments()`, `PutRawResult(string)`, `PutError(string)`.

The PoC is **functionally complete and end-to-end runnable.** All NuGet packages are packed. Demo launches with `demo.cmd`.

---

## Repository Map

```
JOSYN.POC/
├── .github/                             ← agent layer (persona, stories, artifacts)
├── .local-build/                        ← root orchestration scripts
├── docs/                                ← High-Level-Architecture.pptx + this file
├── Local Packages/                      ← local NuGet feed
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/  [NuGet 1.0.0-preview01]
│   ├── JOSYN.Foundation.PropertyBag/    [NuGet 1.0.0-preview01]
│   └── JOSYN.Foundation.JIP/            [NuGet 1.0.0-preview01]
└── JOSYN.System/
    ├── JOSYN.System.Frontend/           (namespace grouping — no code)
    │   ├── JOSYN.System.Frontend.JobHost/  [NuGet 1.0.0-preview01]
    │   └── JOSYN.MyDemoJob/             demo exe (not packed)
    ├── JOSYN.System.Backend/            (namespace grouping — no code)
    │   └── JOSYN.System.Backend.JAPServer/ exe (not packed)
    └── JOSYN.System.Shared/             (namespace grouping — no code)
        ├── JOSYN.System.Shared.Contract/ [NuGet 1.0.0-preview01]
        └── JOSYN.System.Shared.Log/     [NuGet 1.0.0-preview01]
```

**Grouping layers** (`Frontend/`, `Backend/`, `Shared/`) are pure namespace containers — no code, just `.slnx` + scaffold. Concrete projects live one level deeper with fully-qualified names.

---

## Building Blocks

### `JOSYN.Foundation.ResultPattern`

**Purpose:** The error-handling foundation. No exceptions above the catch boundary. Every operation returns `Result` (void) or `Result<T>`. Failures carry an error message and an accumulated call chain (`CallerInfo` list).

**Dependencies:** None. Zero external dependencies.

```csharp
return Result.Success;
return Result<T>.Success(value);        // or implicit: return value;
return Result.Error("Fehlermeldung");   // creates Error, implicitly converts
catch (Exception ex) { return ex; }    // lowest layer only
var inner = SomeOp(); if (!inner.Succeeded) return Result.Propagate(inner);
```

`Result.Propagate(inner)` — never re-wrap a failure manually; always propagate.

---

### `JOSYN.Foundation.PropertyBag`

**Purpose:** Serializes/deserializes C# `record` types to/from `Dictionary<string,string>`, then to sectionless **INI** or **JSON** (auto-detected: `{` → JSON, else INI). Used to pass job arguments and results across the pipe.

**Dependencies:** ResultPattern

```csharp
PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
PropertyBag.Deserialize<MyRecord>(rawString);  // format auto-detected
```

- Records only — validates via `<Clone>$` method presence.
- Culture: `de-DE` — **set by the host process at startup**, not by the library.
- Both record styles: init-property style AND primary-constructor (positional) style.
- `DateTimeOffset` is a supported property type.
- INI format is whitespace-exact — no trimming.
- Property name matching is case-insensitive on the first character when deserializing.

---

### `JOSYN.Foundation.JIP`

**Purpose:** JOSYN Interprocess Protocol. Named-pipe transport in two layers: raw byte transport + JSON request/response convention. Session-isolated per session key.

**Dependencies:** ResultPattern

**Two-layer design:**
- **Layer 1 — Transport** (`PipesServer`, `PipesClient`, `PipesProtocol`): bytes only; length-prefix framing (`int32` LE + raw bytes); two pipes per session (`req-pipe-<key>`, `res-pipe-<key>`).
- **Layer 2 — Convention** (`JipServer`, `JipClient`, `JipProtocol`, `JipDispatcher`): JSON `Request { What, Data? }` / `Response { Succeeded, Data?, Error? }`.

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

`IsCancellationRequested` — converted internally to a polling `CancellationToken`; callers pass a simple async predicate.

**Known PoC limitation:** Protocol is single-in-flight — strictly sequential, no request IDs.

---

### `JOSYN.System.Shared.Contract`

**Purpose:** The JAP protocol contract. Transport-agnostic. Shared by both Frontend and Backend.

**Dependencies:** ResultPattern only (NOT JIP)

```csharp
public interface IJosynApplicationProtocol {
    Task<Result<string>> GetRawArguments();
    Task<Result>         PutRawResult(string rawResult);
    Task<Result>         PutError(string serializedErrorReport);
}

record ErrorReport(string Message, string? CallStack, string? ExceptionDetails, DateTimeOffset OccurredAt);
```

`ErrorReport` is serialized as **JSON** via PropertyBag — INI was insufficient for multi-line `CallStack`/`ExceptionDetails`.

---

### `JOSYN.System.Shared.Log`

**Purpose:** `LocalLog` — process-local file logger. Static. Never throws. Flush-on-write.

**Dependencies:** ResultPattern

- Log path: **`<ExeDir>\logs\`**
- `LogDirectory` — settable `public static string`; test seam.
- `EnableConsoleOutput` — mirrors output to Console.
- Causer overloads write to `Path.Combine(LogDirectory, causer)` subfolder.
- `FormatEntry` is `internal` (promoted for testability); test class marked `[NonParallelizable]`.
- `LocalLog.Error(string, string)` is ambiguous — always use named parameters.

---

### `JOSYN.System.Frontend.JobHost`

**Purpose:** The job developer's library. Job author references this, marks one method `[JobEntryPoint]`, calls `Core.Run(args)` from `Program.cs` — the library handles everything else.

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

| Failure point | Action | Exit code |
|---|---|---|
| Pipe connection failed | LocalLog.Error only | -1 |
| Job error, pipe alive | LocalLog.Error + PutError to server | -2 |
| PutError itself fails | LocalLog.Error (fallback) | -2 |

- CLI args format: `"JOSYN-IPC <sessionKey>"` (server passes this when launching the job exe).
- Reflection in `JobInvoker` is **intentional design** — the job-author extension point.
- `JAPClient` is `internal` — implements `IJosynApplicationProtocol` via JIP transport.
- `ArgumentsComparer<T>` — internal delegate, deliberate placeholder for future conditional parallel execution.

---

### `JOSYN.System.Backend.JAPServer` (exe — not packed)

**Purpose:** The backend process. Bridges the two sides: starts, listens on named pipe, launches the job exe, handles the three JAP calls, shuts down cleanly.

**Dependencies:** ResultPattern, JIP, Shared.Contract, Shared.Log

- `Program.cs` → `Host.Run(args)` (one line).
- `Host.cs` — lifecycle, ESC-key shutdown, graceful drain, LocalLog throughout.
- `JAPServer.cs` — `IJosynApplicationProtocol` implementation (3 methods).
- `FakeReadArgumentsFromFile` — hardcoded for PoC scope; intentional, not a bug.
- **Demo session key:** `dea5611d-d740-437f-ad93-7a5dc5ae4299` (hardcoded in `launchSettings.json`).

---

### `JOSYN.MyDemoJob` (exe — not packed)

**Purpose:** Demo job exe. One method marked `[JobEntryPoint]`. `Program.cs` is one line: `Core.Run(args)`.

---

## Test Projects

| Project | Notes |
|---|---|
| `JOSYN.Foundation.ResultPattern.Test` | |
| `JOSYN.Foundation.PropertyBag.Test` | |
| `JOSYN.Foundation.JIP.Test` | |
| `JOSYN.Foundation.JIP.Demo.ServerExe.Test` | |
| `JOSYN.System.Frontend.JobHost.Test` | 1 project; `JobInvokerTestSupport.cs` + `JobInvokerTests.cs` |
| `JOSYN.System.Shared.Log.Test` | `LocalLogTests.cs` (I/O, NonParallelizable) + `LocalLogFormatEntryTests.cs` (pure) |

Run `test-all.cmd` for current test count.

---

## Build & Run

**NuGet dependency order:**
```
1. JOSYN.Foundation.ResultPattern  → pack
2. JOSYN.Foundation.PropertyBag    → pack
3. JOSYN.Foundation.JIP            → pack
4. JOSYN.System.Shared             → pack (Contract + Log)
5. JOSYN.System.Frontend.JobHost   → pack
6. JOSYN.System.Backend.JAPServer  → build only (exe)
```

Run `build-all.cmd` from `.local-build\` for a crystal-clean full rebuild. Run `demo.cmd` to launch the end-to-end demo.

---

## Key Design Conventions

| Topic | Rule |
|---|---|
| Default type | `static class` — instance only if justified |
| Data | `record` over `class`; `init`-only properties |
| Error handling | `Result`/`Result<T>` — never `throw` above catch boundary |
| Exception boundary | `catch (Exception ex) { return ex; }` — lowest layer only |
| Propagation | `Result.Propagate(inner)` — never re-wrap |
| Interfaces | `static abstract` members in `Contracts/` folder |
| Error messages | **German** |
| XML docs | English; on interfaces; `<inheritdoc>` on implementations |
| Culture | `de-DE` — applied by host process at startup |
| Nullability | `Nullable` enabled; `?` is deliberate |
