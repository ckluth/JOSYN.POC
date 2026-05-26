# JOSYN.System.Shared.Log

Part of the **JOSYN** (JobSystem Next) ecosystem — member of the `JOSYN.System.Shared` layer.

`JOSYN.System.Shared.Log` stellt einen prozess-lokalen Datei-Logger bereit, der von beiden
JOSYN-EXE-Prozessen (`JobHost`, `JAPServer`) genutzt wird.

---

## Überblick

Jeder JOSYN-EXE-Prozess schreibt sein eigenes Log in:

```
%TEMP%\JOSYN\<ProcessName>\<yyyy-MM-dd>.log
```

Einträge werden **sofort auf Platte geflusht** (kein Puffer). Schreibfehler werden
stillschweigend ignoriert — der Logger darf den Host-Prozess niemals zum Absturz bringen.
Im **Debug-Build** wird zusätzlich auf die Konsole geschrieben.

---

## API

```csharp
// Fehlereintrag — String-Variante
LocalLog.Error("Verbindung fehlgeschlagen.", callStack: "...", exceptionDetails: "...");

// Fehlereintrag — Result-Variante (extrahiert Message, CallStack, Exception automatisch)
LocalLog.Error(result);

// Info-Eintrag
LocalLog.Info("Server terminiert.");
```

---

## Log-Format

```
[2026-05-25 11:43:12 +02:00] [ERROR]
Verbindung fehlgeschlagen.
--- CallStack ---
  → Host.RunServer  (Host.cs:43)
  → ...
--- Exception ---
  System.IO.IOException: ...
--------------------------------------------------------------------------------
```

---

## Fehlerrouting-Prinzip

```
Pipe nicht erreichbar   →  LocalLog.Error(...)                     (nur lokal)
Job-Fehler              →  LocalLog.Error(...) + PutError via JAP  (lokal + remote)
PutError fehlgeschlagen →  LocalLog.Error(...) Fallback            (nur lokal)
```

Der Logger ist nicht für das Routing selbst verantwortlich — das liegt im Aufrufer
(`Core.cs` im JobHost). `LocalLog` schreibt ausschließlich lokal.

---

## Für Maintainer

### Bauen, Testen, Packen

```
.local-build\build.cmd          # Release-Build (beide Shared-Projekte)
.local-build\pack.cmd           # NuGet-Pakete → ..\..\Local Packages\
```

### Abhängigkeiten

| Paket | Rolle |
|---|---|
| `JOSYN.Foundation.ResultPattern` | `Result`-Überladung von `LocalLog.Error` |

### Geplante Erweiterungen (nach PoC-Freeze)

- Log-Rotation / Cleanup-Mechanismus
- Konfigurierbare Log-Pfade
- Strukturierte Log-Einträge (JSON Lines)

---

*JOSYN.System.Shared.Log — © 2026 HAEVG AG — MIT License*
