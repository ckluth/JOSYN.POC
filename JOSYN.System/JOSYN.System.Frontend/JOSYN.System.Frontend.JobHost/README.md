# JOSYN.System.Frontend

Part of the **JOSYN** (JobSystem Next) ecosystem.

`JOSYN.System.Frontend` is the **job-side runtime library**. Every job executable links against this
package. It handles the IPC connection to the JAPServer, retrieves job arguments, invokes the
user-defined job method via reflection, and sends the result back — all using the JOSYN Result
pattern for error handling.

---

## Motivation

Job executables should contain only business logic. All bootstrapping — connecting to the server,
deserialising arguments, serialising results — is handled by this library. A minimal job exe
reduces to a single `Program.cs` line:

```csharp
return await JOSYN.System.Frontend.Core.Run(args);
```

The job author then marks exactly one `public static` method with `[JobEntryPoint]` and the
runtime takes care of the rest.

---

## Quick Start

### 1 — Job Executable (`Program.cs`)

```csharp
return await JOSYN.System.Frontend.Core.Run(args);
```

### 2 — Job Implementation

```csharp
using JOSYN.System.Frontend.Attributes;

public static class MyJob
{
    [JobEntryPoint]
    public static MyResult Execute(MyArguments args)
    {
        return new MyResult { Message = "Echo: " + args.Msg };
    }
}
```

Argument and result types must be `record` types supported by `JOSYN.Foundation.PropertyBag`.

---

## Architecture

```
Job.exe
 └── Core.Run(args)
      ├── JAPClient          — connects to JAPServer via JIP named pipes
      └── JobInvoker
           ├── FindEntryPointAssembly   — locates the entry assembly
           ├── FindJobFunction          — finds [JobEntryPoint] via reflection
           ├── CreateInvocationArguments — deserialises raw args via PropertyBag
           ├── [JobEntryPoint method]   — your business logic
           └── ProcessJobResult         — serialises result via PropertyBag, sends back
```

**Transport**: `JOSYN.Foundation.JIP` named pipes (session-isolated via GUID key).  
**Application protocol**: `JOSYN.System.Contract.IJosynApplicationProtocol`.  
**Serialisation**: `JOSYN.Foundation.PropertyBag` (INI or JSON, auto-detected).

---

## Attribute Reference

| Attribute | Target | Purpose |
|---|---|---|
| `[JobEntryPoint]` | Method | Marks the single job entry method. Exactly one per assembly. |
| `[BeforeJobEntryPoint]` | Method | Runs before the job; for setup / parallel-execution checks. |
| `[JobArguments]` | Class | Marks a class as the argument type for a job. |
| `[JobResult]` | Class | Marks a class as the result type of a job. |
| `[ParallelExecutionAllowed]` | Method | Declares that the job may run in parallel. |

---

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | Job completed successfully |
| `-1` | IPC connection failed (server not reachable) |
| `-2` | Job invocation failed (runtime or business error) |

---

## Dependencies

| Package | Role |
|---|---|
| `JOSYN.Foundation.ResultPattern` | Error-as-value pattern throughout |
| `JOSYN.Foundation.JIP` | Named-pipe IPC transport |
| `JOSYN.Foundation.PropertyBag` | Argument / result serialisation |
| `JOSYN.System.Contract` | `IJosynApplicationProtocol` application protocol |

---

## Maintainer Notes

- **One `[JobEntryPoint]` per assembly** — the runtime errors if zero or more than one is found.
- **Reflection is intentional** — `JobInvoker` uses `Assembly.GetEntryAssembly()` to locate the
  job method. This is the defined extension point; it is not DI-based wiring.
- **Error messages are in German** — project-wide convention.
- **`de-DE` default culture** — affects number and date formatting in PropertyBag serialisation.
