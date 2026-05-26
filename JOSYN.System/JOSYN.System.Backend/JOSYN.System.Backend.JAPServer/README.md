# JOSYN.System.Backend.JAPServer

Part of the **JOSYN** (JobSystem Next) ecosystem — member of the `JOSYN.System.Backend`-Schicht.

`JOSYN.System.Backend.JAPServer` ist die **Backend-Server-Exe**. Sie startet den JIP-Named-Pipe-Server,
nimmt JAP-Anfragen von Job-Executables entgegen, dispatcht sie an die
`IJosynApplicationProtocol`-Implementierung und verwaltet den Server-Lifecycle — alles
über das JOSYN-Result-Pattern.

> **Hinweis:** Dies ist eine Executable, keine Bibliothek. Sie wird nicht als NuGet-Paket
> verteilt.

---

## Schnellstart

Bauen und starten. Den IPC-Session-Key als Kommandozeilen-Argument übergeben:

```
JOSYN.System.Backend.JAPServer.exe JOSYN-IPC <sessionKey>
```

Der Session-Key muss mit dem übereinstimmen, der an `PipesClient.ConnectAsync` auf der
Job-Seite übergeben wird. Beim Demo-Betrieb übernimmt `demo.cmd` das automatisch.

---

## Architektur

```mermaid
flowchart TD
    A["Host.Run(args)"] --> B["PipesServer\nJIP Named-Pipe-Server\nsession-isoliert per GUID-Key"]
    A --> C["JipDispatcher\nRegisterAll&lt;IJosynApplicationProtocol&gt;"]
    C --> D["JAPServer\nIJosynApplicationProtocol-Implementierung\nGetRawArguments / PutRawResult / PutError"]
```

**Transport:** `JOSYN.Foundation.JIP` Named Pipes (session-isoliert per GUID-Key).
**Anwendungsprotokoll:** `JOSYN.System.Shared.Contract.IJosynApplicationProtocol`.
**Dispatch:** `JipDispatcher.RegisterAll<T>` — kein manuelles What-String-Wiring.

---

## Exit-Codes

| Code | Bedeutung |
|---|---|
| `0` | Server erfolgreich terminiert |
| `1` | Fataler Fehler (fehlender Session-Key, IPC-Fehler, unbehandelte Exception) |

---

## Abhängigkeiten

| Paket | Rolle |
|---|---|
| `JOSYN.Foundation.ResultPattern` | Fehler-als-Wert-Pattern durchgängig |
| `JOSYN.Foundation.JIP` | Named-Pipe-IPC-Transport + JIP-Konventions-Layer |
| `JOSYN.System.Shared.Contract` | `IJosynApplicationProtocol`-Anwendungsprotokoll |
| `JOSYN.System.Shared.Log` | `LocalLog` für Protokollierung |

---

## Für Maintainer

### Bauen

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
```

*(Kein `pack.cmd` — dies ist eine Exe, kein NuGet-Paket.)*

### Hinweise

- **Session-Key via CLI:** Der Aufrufer übergibt `"JOSYN-IPC <sessionKey>"` als Argumente.
- **Reconnect standardmäßig:** Der Server akzeptiert nach einem Client-Disconnect erneut
  Verbindungen — bis ESC gedrückt wird.
- **ESC-Abbruch:** ESC an der Konsole beendet den Server nach dem Abschluss der aktuellen
  Verbindung.
- **`FakeReadArgumentsFromFile`** — hardcoded für den PoC-Scope; bewusst, kein Bug.
- **Demo-Session-Key:** `dea5611d-d740-437f-ad93-7a5dc5ae4299` (hardcoded in `launchSettings.json`).
- **Fehlermeldungen sind auf Deutsch** — projekt-weite Konvention.
- **`de-DE` Default-Kultur** — betrifft Zahlen- und Datumsformatierung.
