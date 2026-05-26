# Changelog

Alle relevanten Änderungen an diesem Paket werden hier dokumentiert.  
Format orientiert sich an [Keep a Changelog](https://keepachangelog.com/de/1.0.0/).

---

## [Unreleased]

### Hinzugefügt

- `ToString()`-Override auf `Result` und `Result<T>`:
  - Erfolg: `[Erfolgreich]` bzw. `[Erfolgreich] {Wert}`
  - Fehler: `[Fehlgeschlagen] {Fehlermeldung}`, gefolgt von Aufrufkette und ggf. Ausnahme
  - Nützlich für Debugging, IDE-Watches und Test-Assertions
- 28 neue Unit-Tests für `ToString()` (insgesamt nun 141)

---

## [1.0.0-preview01] — 2026-05-24

Erste stabile Kandidatenversion unter dem neuen Namen `JOSYN.Foundation.ResultPattern`
(umbenannt von `JOSYN.Core.ResultPattern` im Rahmen der JOSYN-PoC-Strukturbereinigung).
Das Paket gilt als produktionsreif für den internen Einsatz; die Preview-Kennzeichnung
spiegelt den noch offenen Abnahme-Prozess wider.

### Hinzugefügt

- `Result` (void) und `Result<T>` (generisch) mit vollständiger Implementierung
  des Result-Patterns
- Automatischer Callstack-Aufbau über `Propagate()`
- `Error`-Hilfstyp für idiomatische Fehlerrückgabe (`return Result.Error("...")`)
- Implizite Konvertierungen: `Exception → Result`, `Error → Result`, `T → Result<T>`
- `ToResult()` / `ToResult<T>()` für typübergreifende Fehlerweiterleitung
- Vollständige XML-Dokumentation aller öffentlichen Member (IntelliSense)
- 113 Unit-Tests (NUnit 4.x), alle grün
- `README.md` mit Schnellstart, Kernkonzepten und Referenztabelle
- Build-Skripte unter `.local-build\`
- Nullable Reference Types durchgängig aktiviert

### Geändert

- Paketname und Namespaces von `JOSYN.Core.ResultPattern` auf `JOSYN.Foundation.ResultPattern` umgestellt
