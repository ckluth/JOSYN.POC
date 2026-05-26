# JOSYN.Foundation.PropertyBag

Serialisiert und deserialisiert flache C#-`record`-Typen zu und aus String-Formaten —
sectionloses INI oder JSON — mit vollständiger Integration des JOSYN-Result-Patterns.
Designed für den Einsatz in JOSYN-IPC-Protokollen, wo strukturierte Daten als inspektierbarer
String über Named-Pipes transportiert werden.

> **Scope.** Dies ist keine General-Purpose-Bibliothek. Ihre spezifische Rolle ist die
> Serialisierung flacher Records und Methodenparameter für JOSYN's Named-Pipe-IPC-Kanal.
> Für allgemeine JSON-Serialisierung: `System.Text.Json` direkt verwenden.

---

## Schnellstart

```csharp
// Record definieren — beide Schreibweisen funktionieren
public record JobRequest
{
    public required string JobId   { get; init; }
    public int             Retries { get; init; }
    public bool            Urgent  { get; init; }
}

var req = new JobRequest { JobId = "JOB-42", Retries = 3, Urgent = true };

// Zu INI serialisieren
var ini = PropertyBag.Serialize(req, IniDictionarySerializer.Serialize);
// JobId=JOB-42
// Retries=3
// Urgent=True

// Zu JSON serialisieren
var json = PropertyBag.Serialize(req, JsonDictionarySerializer.Serialize);
// {
//   "JobId": "JOB-42",
//   "Retries": "3",
//   "Urgent": "True"
// }

// Deserialisieren — Format wird automatisch erkannt
var result = PropertyBag.Deserialize<JobRequest>(ini.Value);
// result.Value.JobId   == "JOB-42"
// result.Value.Retries == 3
```

---

## API-Übersicht

Alle Methoden geben `Result` oder `Result<T>` zurück. Keine Exceptions propagieren nach oben.

### `PropertyBag` — Haupt-Einstiegspunkt

| Methode | Beschreibung |
|---------|-------------|
| `Serialize<TRecord>(record, serializer)` | Serialisiert eine Record-Instanz mit dem angegebenen Format-Serializer. |
| `Serialize(object, Type, serializer)` | Dasselbe, mit dem Typ zur Laufzeit (für Reflection-basierte Aufrufer). |
| `Deserialize<TRecord>(string)` | Erkennt Format automatisch, deserialisiert in einen stark typisierten Record. |
| `Deserialize(string, Type)` | Dasselbe, mit dem Zieltyp zur Laufzeit. Gibt `Result<object>` zurück. |
| `Deserialize(string, ParameterInfo[])` | Erkennt Format automatisch, deserialisiert in ein `object[]` ausgerichtet an den gegebenen Methodenparametern. Für Reflection-basierten Dispatch. |

### Format-Serializer

Als `serializer`-Argument an `PropertyBag.Serialize` übergeben:

| Serializer | Format |
|------------|--------|
| `IniDictionarySerializer.Serialize` | Sectionloses INI (`Key=Value`-Zeilen) |
| `JsonDictionarySerializer.Serialize` | Eingerücktes JSON mit String-Werten |

`IniDictionarySerializer` und `JsonDictionarySerializer` sind auch direkt nutzbar für
Low-Level-Dictionary-Zugriff.

### Format-Erkennung

`Deserialize` prüft das erste Nicht-Whitespace-Zeichen des Inputs: `{` → JSON, sonst INI.
Der Aufrufer muss das Format nicht separat tracken — beide Serializer erzeugen round-trip-fähige Ausgaben.

---

## Unterstützte Property-Typen

Alle Properties eines serialisierten Records müssen einem der folgenden Typen entsprechen.
Nullable-Wrapper (`T?`) sind für jeden Eintrag erlaubt. Alle `enum`-Typen werden unterstützt.

| Kategorie | Typen |
|---|---|
| Text | `string` |
| Zeichen | `char` |
| Boolean | `bool` |
| Integer | `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` |
| Fließkomma | `float`, `double`, `decimal` |
| Datum / Zeit | `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan` |
| Identität | `Guid` |
| Enum | beliebiger `enum`-Typ |

Records mit anderen Property-Typen (verschachtelte Records, Collections, Arrays usw.)
schlagen bei der Serialisierung mit einer informativen Fehlermeldung fehl.

---

## Nullable Properties

- Eine nullable Property (`T?`), die im Input fehlt, wird stillschweigend auf `null` gesetzt.
- Eine nullable Property mit leerem Wert (`Key=`) wird ebenfalls auf `null` gesetzt.
- Eine nicht-nullable Property, die im Input fehlt, erzeugt einen Fehler.

```csharp
public record Config
{
    public required string Host    { get; init; }
    public int?            Timeout { get; init; }   // optional — darf fehlen
}
```

---

## Enum-Serialisierung

Enums werden nach Name serialisiert (`Color.Green` → `"Green"`) und case-insensitiv
deserialisiert (`"green"` → `Color.Green`).

---

## INI-Format — Details

- **Nur sectionlos** für Record-Serialisierung — kein `[Section]`-Header.
- **Werte sind verbatim** — die rechte Seite von `=` wird exakt gespeichert und zurückgegeben.
  Ein manuell erstellter Eintrag `Key= value` behält das führende Leerzeichen.
- **Kommentare** — Zeilen mit `;` werden beim Deserialisieren ignoriert.
- **Doppelte Schlüssel** erzeugen einen Deserialisierungsfehler.

---

## JSON-Format — Details

- Ausgabe ist **eingerückt**; Enum-Werte als Strings.
- Der JSON-Input muss ein **flaches Objekt** mit ausschließlich String-Werten sein.
- Culture-aware Konverter für `DateTime`, `DateOnly`, `TimeOnly` und `decimal`
  (aktuell: `de-DE`).

---

## Kultur

Zahlen- und Datumsformatierung verwendet die **aktuelle Thread-Kultur** zum Zeitpunkt der
Serialisierung. Die kanonische JOSYN-Kultur ist `de-DE` (deklariert in `JosynCulture.Default`).

> **Wichtig:** `PropertyBag` setzt die Thread-Kultur **nicht** selbst. Jeder JOSYN-Host-Prozess
> setzt sie beim Start:
> ```csharp
> CultureInfo.DefaultThreadCurrentCulture   = JosynCulture.Default;
> CultureInfo.DefaultThreadCurrentUICulture = JosynCulture.Default;
> ```
> Serialisierte Daten und der lesende Prozess müssen dieselbe Kultur verwenden —
> sonst ist Round-Trip-Treue für Zahlen und Daten nicht garantiert.

---

## Einschränkungen

**Nur flache Records.** Verschachtelte Records und Collections (`List<T>`, Arrays usw.)
werden nicht unterstützt.

**Schlüssel-Matching ist case-sensitiv** bei der Record-Deserialisierung. Property-Namen
im Record (PascalCase) müssen exakt mit den Schlüsseln im serialisierten String übereinstimmen.
Die `ParameterInfo[]`-Überladung wendet ein Erster-Buchstabe-Toggle als Komfort an.

**Beide Record-Schreibweisen funktionieren** — Init-Property- und Primary-Constructor-Stil:

```csharp
// ✅ Init-Property-Stil
public record JobRequest
{
    public required string JobId  { get; init; }
    public int             Retries { get; init; }
}

// ✅ Primary-Constructor (positional) Stil
public record JobRequest(string JobId, int Retries);
```

---

## Delegate-Typen

Die Format-Plug-in-Punkte sind zwei Delegates, die `PropertyBag` von einem spezifischen
Format entkoppeln:

```csharp
// Dictionary<string, string> → string  (von Serialize genutzt)
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);

// string → Dictionary<string, string>  (intern von Deserialize genutzt)
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);
```

Eigene Serializer können durch Implementierung dieser Delegate-Signaturen eingesteckt werden.

---

## Parameter-Deserialisierung

`Deserialize(string raw, ParameterInfo[] parameters)` ist der Einstiegspunkt für Reflection-
basierten Dispatch. Er parst den Input und konstruiert ein `object[]`, das positional zu den
gegebenen Parametern ausgerichtet ist — bereit für `MethodBase.Invoke`.

- Schlüssel werden mit einem Erster-Buchstabe-Toggle gegen Parameter-Namen abgeglichen
  (z. B. `jobId` passt zu einem Parameter `JobId`).
- Nullable Parameter, die im Input fehlen, werden auf `null` gesetzt.
- Nicht-nullable Parameter, die im Input fehlen, erzeugen einen Fehler.

---

## Abhängigkeiten

- `JOSYN.Foundation.ResultPattern` — das Result-Pattern durchgängig eingesetzt.
- `.NET 10` / C# `latest`.

---

## Für Maintainer

### Bauen, Testen, Packen

```
.local-build\build.cmd          # Release-Build
.local-build\build.cmd Debug    # Debug-Build
.local-build\test.cmd           # Alle Tests ausführen
.local-build\pack.cmd           # NuGet-Paket → ..\..\Local Packages\
```

### Projektstruktur

```
JOSYN.Foundation.PropertyBag\
├── PropertyBag.cs
├── Contracts\
│   └── IPropertyBag.cs
├── Serializers\
│   ├── IniDictionarySerializer.cs
│   └── JsonDictionarySerializer.cs
├── Support\
│   ├── SupportedPropertyTypes.cs
│   └── JosynCulture.cs
└── Delegates\
    ├── DictionaryToStringSerializer.cs
    └── StringToDictionarySerializer.cs
```

---

*JOSYN.Foundation.PropertyBag — © 2026 HAEVG AG — MIT License*

> **Scope.** This is not a general-purpose serialization library. Its specific role is serializing flat records and method parameter sets for use in JOSYN's named-pipe IPC channel. For general JSON serialization, use `System.Text.Json` directly.

---

## Quick start

```csharp
// Define a record — both styles work
public record JobRequest
{
    public required string JobId  { get; init; }
    public int             Retries { get; init; }
    public bool            Urgent  { get; init; }
}

var req = new JobRequest { JobId = "JOB-42", Retries = 3, Urgent = true };

// Serialize to INI
var ini = PropertyBag.Serialize(req, IniDictionarySerializer.Serialize);
// JobId=JOB-42
// Retries=3
// Urgent=True

// Serialize to JSON
var json = PropertyBag.Serialize(req, JsonDictionarySerializer.Serialize);
// {
//   "JobId": "JOB-42",
//   "Retries": "3",
//   "Urgent": "True"
// }

// Deserialize — format is auto-detected
var result = PropertyBag.Deserialize<JobRequest>(ini.Value);
// result.Value.JobId  == "JOB-42"
// result.Value.Retries == 3
```

---

## API overview

All methods return `Result` or `Result<T>`. No exceptions propagate up the call stack.

### `PropertyBag` — main entry point

| Method | Description |
|--------|-------------|
| `Serialize<TRecord>(record, serializer)` | Serializes a record instance to a string using the given format serializer. |
| `Serialize(object, Type, serializer)` | Same, with the type supplied at runtime (for reflection-driven callers). |
| `Deserialize<TRecord>(string)` | Auto-detects format, deserializes into a strongly-typed record. |
| `Deserialize(string, Type)` | Same, with the target type supplied at runtime. Returns `Result<object>`. |
| `Deserialize(string, ParameterInfo[])` | Auto-detects format, deserializes into an `object[]` aligned with the given method parameters. For reflection-based dispatch. |

### Format serializers

Pass one of these as the `serializer` argument to `PropertyBag.Serialize`:

| Serializer | Format |
|------------|--------|
| `IniDictionarySerializer.Serialize` | Sectionless INI (`Key=Value` lines) |
| `JsonDictionarySerializer.Serialize` | Indented JSON with string values |

`IniDictionarySerializer` and `JsonDictionarySerializer` are also usable directly when lower-level dictionary access is needed.

### Format auto-detection

`Deserialize` inspects the first non-whitespace character of the input: if it is `{`, JSON is assumed; otherwise INI is assumed. The caller does not need to track or pass the format — output from either serializer round-trips cleanly.

---

## Supported property types

All properties of a serialized record must be one of the following types. Nullable wrappers (`T?`) are accepted for any entry in this list. All `enum` types are supported regardless of their underlying integer type.

| Category | Types |
|---|---|
| Text | `string` |
| Character | `char` |
| Boolean | `bool` |
| Integer | `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong` |
| Floating-point | `float`, `double`, `decimal` |
| Date / Time | `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan` |
| Identity | `Guid` |
| Enum | any `enum` type |

Records containing any other property type (nested records, collections, arrays, etc.) will fail serialization with an informative error message listing the unsupported properties.

---

## Nullable properties

- A nullable property (`T?`) that is missing from the input string is silently set to `null` — no error.
- A nullable property present in the input with an empty value (`Key=`) is also set to `null`.
- A non-nullable property that is missing from the input causes an error.

```csharp
public record Config
{
    public required string Host    { get; init; }
    public int?            Timeout { get; init; }   // optional — may be absent
}
```

---

## Enum serialization

Enums are serialized by name (`Color.Green` → `"Green"`) and deserialized case-insensitively (`"green"` → `Color.Green`).

---

## INI format details

- **Sectionless only** for record serialization — no `[Section]` headers are written or expected by `PropertyBag`.
- **Values are verbatim** — the right-hand side of `=` is stored and returned exactly as written. A hand-crafted entry `Key= value` captures the leading space. The caller is responsible for the exact content.
- **Comments** — lines starting with `;` are ignored during deserialization.
- **Duplicate keys** cause a deserialization error.
- `IniDictionarySerializer` additionally supports sectioned INI (`Dictionary<string, Dictionary<string, string>>`) for callers that need multi-section documents directly.

---

## JSON format details

- Output is **indented** with enum values written as strings.
- The JSON must represent a **flat object** where every value is a JSON string. Nested objects or non-string values are not supported as input.
- Culture-aware converters are applied for `DateTime`, `DateOnly`, `TimeOnly`, and `decimal`, using the current thread culture (default: `de-DE`).

---

## Culture

Number and date formatting uses the **current thread culture** at the time of serialization/deserialization. The canonical JOSYN culture is `de-DE` (declared in `JosynCulture.Default` in `JOSYN.Foundation.PropertyBag`), so `decimal` values serialize with a comma decimal separator (`3,14`), and dates follow German locale conventions.

> **Important:** `PropertyBag` does not set the thread culture itself. Every JOSYN host process applies `JosynCulture.Default` at startup:
> ```csharp
> CultureInfo.DefaultThreadCurrentCulture   = JosynCulture.Default;
> CultureInfo.DefaultThreadCurrentUICulture = JosynCulture.Default;
> ```
> Serialized data and the process that reads it must use the same culture, or round-trip fidelity for numbers and dates is not guaranteed. `JosynCulture.Default` is the single source of truth — see its XML documentation before changing it.

---

## Constraints

**Flat records only.** Nested records and all collection types (`List<T>`, arrays, etc.) are not supported.

**Key matching is case-sensitive** for record deserialization. Property names in the record (PascalCase) must match the keys in the serialized string exactly. The `ParameterInfo[]` overload applies a first-character case toggle as a convenience, but the record overload does not.

**Both record styles work** — init-property and primary-constructor (positional):

```csharp
// ✅ Works for serialize AND deserialize — init-property style
public record JobRequest
{
    public required string JobId  { get; init; }
    public int             Retries { get; init; }
}

// ✅ Works for serialize AND deserialize — primary-constructor (positional) style
public record JobRequest(string JobId, int Retries);
```

---

## Delegate types

The format plug-in points are two delegates, which decouple `PropertyBag` from any specific format:

```csharp
// Converts Dictionary<string, string> → string  (used by Serialize)
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);

// Converts string → Dictionary<string, string>  (used internally by Deserialize)
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);
```

Custom serializers can be plugged in by implementing these delegate signatures.

---

## Parameter deserialization

`Deserialize(string raw, ParameterInfo[] parameters)` is the entry point for reflection-based dispatch. It parses the input and constructs an `object[]` positionally aligned with the given parameter array, ready to pass to `MethodBase.Invoke`.

- Keys are matched against parameter names with a first-character case toggle (e.g., `jobId` matches a parameter named `JobId`).
- Nullable parameters absent from the input are set to `null`.
- Non-nullable parameters absent from the input cause an error.

---

## Dependencies

- `JOSYN.Foundation.ResultPattern` — the Result pattern used throughout.
- `.NET 10` / C# `latest`.

---

## Status

`genesis` / `poc` — internal package for the JOSYN ecosystem. API may change between versions.
