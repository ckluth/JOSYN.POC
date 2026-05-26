# JOSYN.System.Backend

Part of the **JOSYN** (JobSystem Next) ecosystem.

`JOSYN.System.Backend` is the **backend server executable**. It starts the JIP named-pipe
server, receives JAP requests from job executables, dispatches them to the
`IJosynApplicationProtocol` implementation, and manages the server lifecycle — all using
the JOSYN Result pattern for error handling.

> **Note:** This is an executable, not a library. It is not distributed as a NuGet package.

---

## Quick Start

Build and run. Pass the IPC session key as a command-line argument:

```
JOSYN.System.Backend.JAPServer.exe JOSYN-IPC <sessionKey>
```

The session key must match the one passed to `PipesClient.ConnectAsync` on the job side.

---

## Architecture

```
Server.exe
 └── Host.Run(args)
      ├── PipesServer          — JIP named-pipe server (session-isolated via GUID key)
      └── JipDispatcher
           ├── RegisterAll<IJosynApplicationProtocol>   — maps JAP methods via reflection
           └── JAPServer                                — IJosynApplicationProtocol impl
```

**Transport**: `JOSYN.Foundation.JIP` named pipes (session-isolated via GUID key).  
**Application protocol**: `JOSYN.System.Contract.IJosynApplicationProtocol`.  
**Dispatch**: `JipDispatcher.RegisterAll<T>` — zero manual What-string wiring.

---

## Exit Codes

| Code | Meaning |
|---|---|
| `0` | Server terminated successfully |
| `1` | Fatal error (missing session key, IPC failure, unhandled exception) |

---

## Dependencies

| Package | Role |
|---|---|
| `JOSYN.Foundation.ResultPattern` | Error-as-value pattern throughout |
| `JOSYN.Foundation.JIP` | Named-pipe IPC transport + JIP convention layer |
| `JOSYN.System.Contract` | `IJosynApplicationProtocol` application protocol |

---

## Maintainer Notes

- **Session key via CLI**: the caller passes `"JOSYN-IPC <sessionKey>"` as arguments.
- **Reconnect by default**: the server re-accepts connections after a client disconnects
  until ESC is pressed.
- **ESC cancellation**: pressing ESC at the console terminates the server after the current
  connection closes.
- **Error messages are in German** — project-wide convention.
- **`de-DE` default culture** — affects number and date formatting in PropertyBag serialisation.
