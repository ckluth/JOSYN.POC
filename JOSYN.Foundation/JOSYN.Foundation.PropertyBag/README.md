# JOSYN.Foundation.PropertyBag

`JOSYN.Foundation.PropertyBag` ist ein logisches Repo im physikalischen Multi-Repo **JOSYN.Foundation**.

Es serialisiert und deserialisiert flache C#-`record`-Typen zu und aus String-Formaten —
sectionloses INI oder JSON — mit vollständiger Integration des JOSYN-Result-Patterns.
Designed für den Einsatz in JOSYN-IPC-Protokollen, wo strukturierte Daten als inspektierbarer
String über Named-Pipes transportiert werden.

> **Scope.** Dies ist keine General-Purpose-Bibliothek. Ihre spezifische Rolle ist die
> Serialisierung flacher Records und Methodenparameter für JOSYN's Named-Pipe-IPC-Kanal.

---

## Enthält

- `JOSYN.Foundation.PropertyBag.slnx`
  - `JOSYN.Foundation.PropertyBag` — Bibliothek (NuGet `1.0.0-preview01`)
  - `JOSYN.Foundation.PropertyBag.Test` — Testprojekt (NUnit 4.x)

## Bauen, Testen, Packen

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
.local-build\test.cmd           # Alle Tests ausführen
.local-build\pack.cmd           # NuGet-Paket → ..\..\Local Packages\
```

Vollständige Dokumentation: [`JOSYN.Foundation.PropertyBag/README.md`](JOSYN.Foundation.PropertyBag/README.md)
