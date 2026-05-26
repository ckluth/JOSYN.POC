# JOSYN.POC – Copilot Instructions

JOSYN.POC ("JobSystem Next — PoC v2") is the **architecture showcase** successor to JOSYN.
It is a **physical multi-repo monorepo** targeting **.NET 10**, C# `latest`, `Nullable` enabled throughout.

**Repo character:** Mature PoC v2. Primary audience: colleagues as readers and reviewers.
Purpose: architecture reference design, collaboration and discussion ground, solo-development base for the next milestone.

## Repository Layout

```
JOSYN.POC/
├── .github/                             ← agent layer (persona, stories, artifacts)
├── .local-build/                        ← root orchestration scripts (build-all, test-all, demo)
├── docs/                                ← High-Level-Architecture.pptx
├── Local Packages/                      ← local NuGet feed (all packed libs output here)
├── JOSYN.Foundation/
│   ├── JOSYN.Foundation.ResultPattern/  ← foundation; referenced by everything  [NuGet 1.0.0-preview01]
│   ├── JOSYN.Foundation.PropertyBag/    ← record serializer; depends on ResultPattern  [NuGet 1.0.0-preview01]
│   └── JOSYN.Foundation.JIP/            ← named-pipe IPC; depends on ResultPattern  [NuGet 1.0.0-preview01]
└── JOSYN.System/
    ├── JOSYN.System.Frontend/           ← namespace grouping layer (no code)
    │   ├── JOSYN.System.Frontend.JobHost/  ← job developer library  [NuGet 1.0.0-preview01]
    │   └── JOSYN.MyDemoJob/             ← demo exe (not packed)
    ├── JOSYN.System.Backend/            ← namespace grouping layer (no code)
    │   └── JOSYN.System.Backend.JAPServer/ ← backend exe (not packed)
    └── JOSYN.System.Shared/             ← namespace grouping layer (no code)
        ├── JOSYN.System.Shared.Contract/ ← JAP protocol contract  [NuGet 1.0.0-preview01]
        └── JOSYN.System.Shared.Log/     ← LocalLog  [NuGet 1.0.0-preview01]
```

**Grouping layers** (`JOSYN.System.Frontend/`, `JOSYN.System.Backend/`, `JOSYN.System.Shared/`) are pure namespace containers — no code, just `.slnx` + scaffold. Concrete projects live one level deeper with fully-qualified names.

Each logical repo under `JOSYN.Foundation/` is self-contained with its own `.slnx` solution, `nuget.config`, and a `.local-build\` scripts folder.

## Build, Test & Pack

Each logical repo contains a `.local-build\` folder with the following scripts. Run them from anywhere — they locate the `.slnx` one level up automatically.

| Task | Script |
|------|--------|
| Build (Release, default) | `.local-build\build.cmd` or `.local-build\build.cmd Release` |
| Build (Debug) | `.local-build\build.cmd Debug` or `.local-build\build.debug.cmd` |
| Build (Release shortcut) | `.local-build\build.release.cmd` |
| Run all tests | `.local-build\test.cmd` → `dotnet test` |
| Single test by name | `dotnet test --filter "TestName"` (from logical repo root) |
| Pack NuGet | `.local-build\pack.cmd` → outputs to `..\..\Local Packages\` |

Build outputs go to `C:\Temp\VS.OUT\JOSYN\<ProjectName>\` (set in `Directory.Build.props`).  
**Test framework:** NUnit 4.x — `[TestFixture]` / `[Test]`.  
**Solution format:** `.slnx` (not `.sln`).

**Root `.local-build\` scripts (orchestration across all logical repos):**

| Script | Purpose |
|--------|---------|
| `build-all.cmd` | Crystal-clean: clear nupkg + NuGet cache → build+pack all in dependency order |
| `test-all.cmd` | `dotnet test` all solutions |
| `all.cmd` | `build-all.cmd` + `test-all.cmd` |
| `demo.cmd` | Launch JAPServer + MyDemoJob [Release] in separate console windows |
| `demo.debug.cmd` | Build Debug + launch both |

**NuGet dependency order** (critical — pack before referencing downstream):
```
1. JOSYN.Foundation.ResultPattern  → pack
2. JOSYN.Foundation.PropertyBag    → pack  (depends on 1)
3. JOSYN.Foundation.JIP            → pack  (depends on 1)
4. JOSYN.System.Shared             → pack Contract + Log  (depends on 1)
5. JOSYN.System.Frontend.JobHost   → pack  (depends on 1,2,3,4)
6. JOSYN.System.Backend.JAPServer  → build only — exe  (depends on 1,3,4)
```

## The Result Pattern — used everywhere

`JOSYN.Foundation.ResultPattern` is the single most important convention. **No exceptions are thrown up the call stack.** Every operation returns `Result` (void) or `Result<T>`.

```csharp
// Success
return Result.Success;
return Result<MyType>.Success(value);  // or implicit: return value;

// Failure
return Result.Fail("error message");
return Result.Error("message");        // creates Error struct, implicitly converts to Result

// Failure from exception — always in catch blocks
catch (Exception ex) { return ex; }   // implicit conversion, captures caller info

// Propagate up a call chain (appends CallerInfo to the call stack)
var inner = SomeOperation();
if (!inner.Succeeded) return Result.Propagate(inner);

// Consume
if (!result.Succeeded)
{
    Console.WriteLine(result.ErrorMessage);
    Console.WriteLine(result.CallStackAsString);
}
var value = result.Value;  // only access after Succeeded == true
```

Always use `Propagate()` instead of re-wrapping a failure — it accumulates the call chain.

## PropertyBag

Serializes/deserializes C# **`record`** types (not plain classes — checked via `<Clone>$` method) to/from `Dictionary<string, string>`, then to **sectionless INI** or **JSON** (auto-detected by checking if input starts with `{`).

```csharp
var result = PropertyBag.Serialize(myRecord, IniDictionarySerializer.Serialize);
var result = PropertyBag.Serialize(myRecord, JsonDictionarySerializer.Serialize);
var result = PropertyBag.Deserialize<MyRecord>(rawString);  // format auto-detected
```

- Default culture is **`de-DE`** — affects number/date formatting.
- Culture setup is the **host process's responsibility** — the library does NOT set `DefaultThreadCurrentCulture`.
- `JosynCulture.Default` = `de-DE` is a compile-time constant; do not make it runtime-configurable.
- Only types listed in `SupportedPropertyTypes.cs` are valid record property types. `DateTimeOffset` is supported.
- Both record styles work: **init-property style** AND **primary-constructor (positional) style**.
- Property name matching is case-insensitive on the first character when deserializing parameters.
- INI format is whitespace-exact — no trimming; leading spaces are captured as-is.

## JIP (Named Pipes)

Session-isolated named-pipe communication between processes. Each session uses **two pipes**: one for requests (`req-pipe-<key>`), one for responses (`res-pipe-<key>`). Messages are **length-prefixed** (`int32` + bytes, little-endian).

```csharp
// Server — starts a client exe, then processes requests
var args = new ServerStartArguments
{
    SessionKey        = sessionKey,
    HandleStringRequest = async req => await dispatcher.Dispatch(req),
    HandleErrorNotification = async (req, ex) => { /* log */ },
    IsCancellationRequested = () => Task.FromResult(shouldStop),
};
await PipesServer.RunAsync(args, reConnect: true);

// Client
var pipes = (await PipesClient.ConnectAsync(sessionKey)).Value;
var response = await JipClient.SendAsync(pipes, "MethodName", optionalData);
await PipesClient.DisconnectAsync(pipes);

// JIP dispatcher (server side)
var dispatcher = new JipDispatcher()
    .Register("PING",    Result<string?>.Success(null))
    .RegisterAll<IMyProtocol>(implementation);
```

The `IsCancellationRequested: Func<Task<bool>>?` parameter is converted internally to a polling `CancellationToken`. Callers pass a simple async predicate; no `CancellationToken` management required.

**Known PoC limitations:**
- Protocol is single-in-flight (strictly sequential, no request IDs).

**Note:** Async handlers (`Func<string, Task<string>>` / `Func<byte[], Task<byte[]>>`), `sealed class` for `ClientPipes`/`ServerPipes`, and JipDispatcher are all fully in place.

## System Building Blocks

### `JOSYN.System.Shared.Contract`

Transport-agnostic JAP protocol contract. Shared by Frontend and Backend. Depends on ResultPattern only (NOT JIP).

```csharp
public interface IJosynApplicationProtocol {
    Task<Result<string>> GetRawArguments();
    Task<Result>         PutRawResult(string rawResult);
    Task<Result>         PutError(string serializedErrorReport);
}

record ErrorReport(string Message, string? CallStack, string? ExceptionDetails, DateTimeOffset OccurredAt);
```

`ErrorReport` is serialized as **JSON** via PropertyBag when passed to `PutError` — INI was insufficient for multi-line `CallStack`/`ExceptionDetails`.

---

### `JOSYN.System.Shared.Log`

`LocalLog` — process-local file logger. Static. Never throws. Flush-on-write.

- **Log path: `<ExeDir>\logs\`** — the `%TEMP%`-based path is obsolete, ignore it.
- `LogDirectory` is a settable `public static string` — used as test seam.
- `EnableConsoleOutput` flag mirrors output to Console.
- Causer overloads write to `Path.Combine(LogDirectory, causer)` subfolder.
- `FormatEntry` is `internal` (promoted for testability).
- `[NonParallelizable]` required in tests — `LogDirectory` and `EnableConsoleOutput` are shared static state.
- ⚠️ `LocalLog.Error(string, string)` is ambiguous between `Error(message, callStack)` and `Error(causer, message)` — **always use named parameters**.

---

### `JOSYN.System.Frontend.JobHost`

The job developer's library. Job author references this, marks one method `[JobEntryPoint]`, calls `Core.Run(args)` from `Program.cs` — the library handles everything else.

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

**Key notes:**
- `JAPClient` is `internal` — implements `IJosynApplicationProtocol` via JIP transport.
- CLI args format: `"JOSYN-IPC <sessionKey>"` (server passes this when launching the job exe).
- Reflection in `JobInvoker` is **intentional design** — the job-author extension point; do not flag it.
- `internal InvokeJob(IJosynApplicationProtocol, IEnumerable<Type>)` overload exists for testing — supplies types directly, bypasses assembly scanning.
- `ArgumentsComparer<T>` — internal delegate, **deliberate placeholder** for a future conditional parallel execution feature. Do not remove.

---

### `JOSYN.System.Backend.JAPServer` (exe — not packed)

Backend process. Connects the two sides: listens on named pipe, launches the job exe, handles the three JAP calls, shuts down cleanly.

- `Program.cs` → `Host.Run(args)` (one line).
- `Host.cs` — server lifecycle, ESC-key shutdown (`WasEscapePressed`), graceful drain, all logging via LocalLog.
- `JAPServer.cs` — `IJosynApplicationProtocol` implementation (3 methods).
- `FakeReadArgumentsFromFile` — hardcoded for PoC scope; **intentional, not a bug**.
- **Demo session key** (hardcoded in `launchSettings.json` of both exe projects): `dea5611d-d740-437f-ad93-7a5dc5ae4299`

---

### `JOSYN.MyDemoJob` (exe — not packed)

Demo job exe. One method marked `[JobEntryPoint]`. `Program.cs` is one line: `Core.Run(args)`.

## Key Conventions

- **Static entry points** — `PipesServer`, `PipesClient`, `PipesProtocol`, `JipServer`, `JipClient`, `JipDispatcher`, and `PropertyBag` are all static classes (or `sealed class` for the pipes types). Interfaces (`IPipesServer`, etc.) exist as API-contract documentation using C# 11 `static abstract` members.
- **Namespace pragma** — files whose folder path doesn't match their namespace use `#pragma warning disable/restore IDE0130` around the `namespace` declaration.
- **Local NuGet feed** — inter-repo dependencies are resolved via `..\..\Local Packages\` (each `nuget.config` points here). Pack a dependency before referencing it from another logical repo.
- **Error messages are in German** — maintain this for consistency (`"Verbindung durch Aufrufer abgebrochen."`, `"kein Callstack"`, etc.).
- **Story Method** — stories live under `.github\stories\` using a two-level structure: **story directory** → **session files**.

  **Story directory** = a named folder for a subject area, e.g. `result-pattern\` or `ipc\`. Session files accumulate here with no setup overhead — just start writing them.

  **Session file naming:** `session-NNNN-[short-description]-[type].md`
  - `NNNN` — zero-padded **4-digit** index, continuous per story (never resets after archiving)
  - `short-description` — 2–4 word kebab-case hint of the content
  - `type` — one of: `discussion` | `summary` | `conclusion` | `analysis` | `generation` | `opener`
  - The directory already carries story context — the filename must **not** repeat it.
  - Examples: `session-0001-make-or-buy-summary.md`, `session-0002-async-handler-analysis.md`
  - Each session appends a new file — never overwrites an old one.

  **Story index (`_index.md`):** each story folder has a `_index.md` maintained entirely by the AI.
  - **Read it first** at the start of any session in that story — it gives full context without opening individual session files
  - **Create it** on the first save in a story that doesn't have one yet
  - **Update it silently** on every subsequent save — no separate instruction from the user needed
  - Contains three sections:
    - `Key Decisions` — firm conclusions that future sessions must not contradict without knowing about them
    - `Open Questions` — unresolved threads a future session might pick up
    - `Sessions` — one-line-per-session table (sequence number, filename, one-sentence summary); archived sessions are listed with `[archived]` tag
  - `_index.md` is **never archived** — it stays in the story root and carries forward across chapters
  - `_index.md` has no `session-NNNN` prefix and is not a session file

  **Session opener:** an optional structured prompt the user places in the story folder before a session starts, to kick off a focused and prepared session:
  - Named `session-NNNN-opener[-short-description].md` (type is `opener`; short-description is optional)
  - The user is responsible for placing it in the correct story folder with the correct name
  - At session start, the AI reads it first, paraphrases it briefly, and asks for clarification if anything is unclear, then begins working
  - Openers are purely optional; sessions without them work exactly as before
  - The session result file produced from an opener is numbered **NNNN** (the opener's own number — the user is responsible for a correctly incremented session number); never re-derive the next number from the file listing when an opener is present
  - A blank template is at `.github\.artifacts\session-opener-template.md`

  **Opener format** (five sections; Constraints and Expected Artifacts are optional):
  ```
  ## Meta
  - Story: <story-name>
  - Session: NNNN
  - Short description: <2-4 word kebab-case>

  ## Background
  1–3 sentences of context — what led to this session.

  ## Goals
  Numbered list — what "done" looks like. Each goal should be verifiable.

  ## Constraints          ← omit if none
  Specific rules: output paths, naming, language, things to avoid.

  ## Expected Artifacts   ← omit for pure discussion sessions
  List of files to produce, with path and one-line description.
  ```

  **Archiving:** when the user says *"archive the current chapter"* (optionally *"as \<name\>"*):
  1. Move all session files currently in the story root into `archives\archive-NNN[-optional-name]\` (3-digit archive counter, optional suffix).
  2. Session numbering in the story **continues from where it left off** — no reset.
  3. If there are 3 or more sessions in the batch, offer (do not require): *"Want a brief conclusion file in the archive?"* — if yes, negotiate content on the fly, no fixed structure.
  4. Archives are sealed after creation — never modified.

  **Directory layout example:**
  ```
  .github\stories\
    result-pattern\
      session-0004-new-story-discussion.md   ← active, continues after archive
      archives\
        archive-001-first-iteration\          ← sealed
          session-0001-make-or-buy-summary.md
          session-0002-...md
          session-0003-...md
          conclusion.md                       ← optional, only if requested
  ```

- **`docs/architecture-reference.md` is a central living document** — keep it up-to-date whenever any session makes changes that affect the system's structure, building blocks, dependencies, conventions, or build system. Updating it is not optional; it is part of completing any such change.
- **Session save trigger** — whenever the user says *"save this session"*, *"write a summary"*, *"log this"*, or similar: always propose a filename following the pattern above and ask for confirmation before writing. Example: *"Shall I save this as `.github\stories\result-pattern\session-0001-make-or-buy-summary.md`?"*
  - **Story Method reference** — the complete human-readable description is at `.github\.artifacts\story-method.md`
