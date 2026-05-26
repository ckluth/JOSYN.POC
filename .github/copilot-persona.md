# Copilot Collaboration Persona

> **Agent-confirm protocol:** At the start of any session, the agent reads this file and briefly
> paraphrases its key principles back to the user before starting work. If anything has changed
> or the user wants to amend a principle, that is the moment to discuss it.

---

## Core Philosophy

**Functional-first C#.** Not F#, but C# written *as if it could be*. Every design choice is measured
against: "Would this pattern translate naturally into a functional language?" If not, there should
be a concrete reason for the exception.

---

## Principles — ranked by priority

### 1. Static wins (when in doubt)
- Prefer `static class` and `static` methods over instance methods.
- Instance types and OOP patterns are *tools*, pulled in only when they earn their place
  (e.g. a natural identity, mutable state that needs encapsulation, or a genuine polymorphism need).
- "In doubt — static wins" is the tiebreaker. The agent applies this without being asked.

### 2. Immutability by default
- Prefer `record` over `class`. Prefer `readonly` fields and `init`-only properties.
- Mutable state must be explicitly justified — never the path of least resistance.

### 3. Pure functions over side effects
- A method that does not mutate state and returns a value encoding its full result is the ideal.
- Side effects are isolated, named explicitly, and pushed to the edges of the call graph.

### 4. Errors as values, never exceptions
- The Result pattern (`Result` / `Result<T>`) is the single mechanism for propagating failures.
- `catch` blocks at the *bottom* of the call graph convert exceptions into `Result`; nothing
  above that layer throws or swallows.
- `Result.Propagate(inner)` accumulates the call chain — never re-wrap manually.

### 5. Interfaces as contracts, not polymorphism
- Public static types get an interface with `static abstract` members.
- These interfaces live in a `Contracts/` folder and serve as *API documentation and shape contracts*.
- Implementations reference them via `/// <inheritdoc cref="IXxx.Member"/>` (static classes) or
  `/// <inheritdoc/>` (non-static classes that formally implement the interface).

### 6. Explicit over magic
- No reflection-based wiring, no DI containers, no hidden conventions.
- If something happens, the call site should make it obvious.

### 7. Minimal surface area
- A type should expose only what is needed. Internal types stay internal.
- Fewer public members means fewer contracts to maintain — each one must earn its place.

---

## Code style preferences

| Topic | Preference |
|-------|-----------|
| Class kind | `static class` by default; `record` for data; `class` only when mutable state or OOP is needed |
| Nullability | `Nullable` enabled everywhere; `?` is deliberate, never defensive |
| Error handling | `Result` / `Result<T>` — no `throw`, no `try/catch` above the lowest layer |
| Comments | Only where the *why* is non-obvious; no restating of what the code already says |
| XML docs | On interfaces/contracts; implementations use `<inheritdoc>` |
| Language | Error messages in **German** (established project convention); XML docs and session files in **English** |
| Culture | Default thread culture is `de-DE`; affects number/date formatting |

---

## What the agent should do automatically

- When proposing a new type: start with `static class` and state why if choosing something else.
- When reviewing existing code: flag instance types that have no state and could be `static`.
- When writing XML docs: put them on the interface, not the implementation.
- When a failure path exists: return `Result.Error(...)` or `return ex;` — never `throw`.
- When propagating a failure upward: use `Result.Propagate(inner)` — never re-wrap.

---

## What the agent should NOT do

- Add OOP abstraction layers "just in case" future flexibility is needed.
- Reach for dependency injection when a static call or a delegate parameter suffices.
- Create mutable state when an immutable data pipeline would work.
- Write defensive `try/catch` blocks outside the lowest-layer boundary.
