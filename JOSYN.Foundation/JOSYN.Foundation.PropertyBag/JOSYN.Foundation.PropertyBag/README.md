# JOSYN.Foundation.PropertyBag

Serializes and deserializes flat C# `record class` instances to and from string-based formats — sectionless INI or JSON — with full integration of the JOSYN Result pattern. Designed for use in JOSYN IPC protocols where structured data must travel as an inspectable string.

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
