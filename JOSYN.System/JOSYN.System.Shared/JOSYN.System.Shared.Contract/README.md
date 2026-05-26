# JOSYN.System.Shared.Contract

Part of the **JOSYN** (JobSystem Next) ecosystem — member of the `JOSYN.System.Shared` layer.

`JOSYN.System.Shared.Contract` definiert den Applikations-Vertrag zwischen den beiden
JOSYN-Systemkomponenten: dem **JobHost** (Frontend) und dem **JAPServer** (Backend).

Dieser Vertrag ist transportunabhängig — er beschreibt ausschließlich, *was* ausgetauscht
wird, nicht *wie* (das ist Aufgabe von `JOSYN.Foundation.JIP`).

> **Hinweis:** Dieses Paket ist der Nachfolger von `JOSYN.System.Contract`. Es lebt nun
> in der `JOSYN.System.Shared`-Schicht zusammen mit `JOSYN.System.Shared.Log`.

---

## Überblick

Das JOSYN Application Protocol (JAP) definiert einen minimalen, rohen Datenaustausch:

```
JobHost                                    JAPServer
   │── GetRawArguments() ──────────────►  │  (Job-Auftrag abholen)
   │◄── result ─────────────────────────  │
   │
   │── PutRawResult(result) ──────────►  │  (Ergebnis zurückmelden)
   │
   │── PutError(serializedError) ──────►  │  (Fehler melden, falls Pipe noch lebt)
```

Der Vertrag ist bewusst auf drei Methoden reduziert. Serialisierung und Deserialisierung
liegen im Verantwortungsbereich der jeweiligen Implementierung.

---

## Vertrag

### `IJosynApplicationProtocol`

| Methode | Beschreibung |
|---|---|
| `GetRawArguments()` | Ruft serialisierte Job-Argumente als String ab |
| `PutRawResult(string)` | Übermittelt das serialisierte Job-Ergebnis |
| `PutError(string)` | Übermittelt einen serialisierten `ErrorReport`, falls der Transport noch aktiv ist |

### `ErrorReport`

```csharp
record ErrorReport(
    string  Message,
    string? CallStack,
    string? ExceptionDetails,
    DateTimeOffset OccurredAt);
```

Wird via `PropertyBag` (INI-Format) serialisiert, bevor er über `PutError` übertragen wird.
Der Server protokolliert ihn; eine weitergehende Verarbeitung ist für spätere Ausbaustufen
vorgesehen.

---

## Fehlerrouting im JobHost

```
Pipe-Verbindungsfehler          →  LocalLog.Error(...)              (nur lokal)
Job-Fehler, Pipe noch aktiv     →  LocalLog.Error(...) + PutError   (lokal + remote)
PutError selbst fehlgeschlagen  →  LocalLog.Error(...)              (Fallback lokal)
```

---

## Für Maintainer

### Bauen, Testen, Packen

```
.local-build\build.cmd          # Release-Build (beide Shared-Projekte)
.local-build\build.cmd Debug    # Debug-Build
.local-build\pack.cmd           # NuGet-Pakete → ..\..\Local Packages\
```

### Abhängigkeiten

| Paket | Rolle |
|---|---|
| `JOSYN.Foundation.ResultPattern` | Fehler-als-Wert-Pattern |

### Referenced by

- `JOSYN.System.Frontend.JobHost`
- `JOSYN.System.Backend.JAPServer`

### Projektstruktur

```
JOSYN.System.Shared.Contract\
├── IJosynApplicationProtocol.cs    # JAP-Vertrag (3 Methoden)
└── ErrorReport.cs                  # Fehler-Payload-Record
```

---

*JOSYN.System.Shared.Contract — © 2026 HAEVG AG — MIT License*
