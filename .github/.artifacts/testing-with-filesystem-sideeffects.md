# Testing Code with File System Side Effects

> A general discussion on strategies, trade-offs, and best practices.

---

## The Fundamental Tension

A test that touches the file system is not a unit test in the strict sense — it is an **integration test**, even if a narrow one. The real question is not "how do we avoid this?" but rather: *does it matter here, and what is the right trade-off?*

The answer depends on what the code actually does and what you actually want to verify.

---

## The Options

### Option 1 — Abstract the I/O Away

Introduce an `IFileWriter` (or `IFileSystem`) interface, inject it into the component under test, and supply a **fake** in tests that captures what would have been written.

**Pro**
- Tests become fast, hermetic, and repeatable
- No disk access; no environment dependency
- Trivially parallelisable

**Con**
- Adds an abstraction whose sole purpose is testability — not design clarity
- For a deliberately simple, static component this is often tail-wagging-the-dog
- You end up testing the mock, not the production path

**Mocking verdict:**  
Mocking makes sense when the abstraction has *independent design value*. If you are introducing `IFileWriter` purely to avoid touching disk in tests, you are paying abstraction debt to buy test purity. That trade-off is rarely worth it for infrastructure helpers.

---

### Option 2 — Accept Impurity, Use a Controlled Temp Directory

Write to a real, per-test directory. Verify file contents. Clean up in `[TearDown]`.

**Pro**
- No refactoring required
- Tests real end-to-end behaviour, including encoding and directory creation
- If the component already exposes a settable path property, that property *is* the seam — no DI framework needed

**Con**
- Slower than pure unit tests (milliseconds vs. microseconds — usually irrelevant)
- Environmental: disk must be writable
- Parallel tests need isolated directories per test (solvable)

---

### Option 3 — Extract and Test Only the Pure Parts

The logic-dense piece is typically the *formatter*, not the writer. Extract it (or expose it as `internal`) and test it without any I/O.

**Pro**
- Tests the real test target — the format logic — with zero I/O
- Extremely fast; no side effects

**Con**
- Structural refactoring for testability
- Routing (e.g. subdirectory selection) is still only verifiable through real I/O or a mock

---

## What Is the Real Test Target?

The real test target is **never** whether `File.AppendAllText` works — that is the runtime's responsibility. Equally, testing that a directory was created is testing the OS, not your code.

The questions worth testing are the *behavioural contracts* of your component. For a file-based logger, typical examples:

| Concern | Testable without I/O? |
|---|---|
| Does the formatted entry contain the right sections? | Yes — if formatter is extracted |
| Are optional sections omitted when empty? | Yes — if formatter is extracted |
| Does a `causer` parameter route output to a subdirectory? | Needs I/O or mock |
| Does an overload that accepts a domain object extract its fields correctly? | Needs I/O or mock (or formatter extraction) |
| Does a write failure stay silent? | Needs I/O (point at invalid path) |

---

## `TestContext.CurrentContext.WorkDirectory` vs `Path.GetTempPath()`

These are two different NUnit concepts that are often confused with a third — `TestContext.CurrentContext.TestDirectory`.

### `TestDirectory`

The directory containing the **test assembly** — i.e. the `bin/Debug/net10.0/` output folder.

- Pollutes build artifacts
- May not be writable in locked-down CI setups
- Has no semantic relationship to "a scratch area for this test run"

**Verdict: do not use for file output in tests.**

---

### `WorkDirectory`

NUnit's designated **output directory for the test run**. Defaults to the current working directory of the process at launch, but can be overridden via the `--work` CLI option or runner configuration. It is explicitly intended as a writable scratch area for test output.

- Is the right NUnit-native choice when you want runner-controlled output placement
- Can be redirected in CI pipelines without changing test code
- Still shared across all tests in the run — a per-test subdirectory is needed to avoid collisions in parallel runs

```csharp
var testDir = Path.Combine(
    TestContext.CurrentContext.WorkDirectory,
    "MyComponent.Tests",
    TestContext.CurrentContext.Test.ID);
```

**Verdict: reasonable NUnit-native choice, but coupled to the NUnit runner.**

---

### `Path.GetTempPath()`

The OS-provided temporary directory. Always writable, purpose-built for transient output, and completely framework-agnostic — works identically under NUnit, xUnit, MSTest, or no framework at all.

The only risk is path collisions in parallel runs — solved identically to `WorkDirectory`:

```csharp
var testDir = Path.Combine(
    Path.GetTempPath(),
    "MyComponent.Tests",
    TestContext.CurrentContext.Test.ID);   // NUnit unique test ID, safe for parallel runs
```

Leftover directories on test failure are harmless — the OS cleans up temp eventually, and explicit cleanup in `[TearDown]` handles the normal case.

**Verdict: the more portable choice; preferred when the test project might run outside NUnit or in contexts where `WorkDirectory` is not configured.**

---

### When to choose which

| | `TestDirectory` | `WorkDirectory` | `Path.GetTempPath()` |
|---|---|---|---|
| Intended for output | ✗ | ✓ | ✓ |
| Framework-agnostic | ✓ | ✗ | ✓ |
| CI-configurable location | ✗ | ✓ | ✗ |
| Always available | ✓ | ✓ | ✓ |
| Recommended for file output | ✗ | situational | ✓ |

---

## Best Practices — Condensed

1. **Identify the real test target first.** Never test framework or OS behaviour.
2. **Use the seam that already exists.** A settable path property, a delegate parameter, or a `Func<>` argument is often sufficient. You do not always need a full interface.
3. **Accept impurity when the cost of abstracting is higher than the cost of the side effect.** A component that writes to a temp directory and cleans up after itself is a perfectly reasonable integration-style unit test.
4. **Isolate per test.** Each test gets its own directory — prevents bleed between tests and makes failures obvious.
5. **Clean up in `[TearDown]`, not `[SetUp]`.** `[TearDown]` runs even when `[SetUp]` fails. Deleting in `[SetUp]` risks removing debris that was itself a useful failure signal. Prefer deleting in `[TearDown]` and accepting leftover directories on unexpected crashes.
6. **Never introduce a mock solely to avoid a file write.** Mocks are for verifying *interactions with collaborators*, not for bypassing inconvenient infrastructure.
7. **Consider `[NonParallelizable]`** on test fixtures that share static mutable state (e.g. a static path property). Per-test directories reduce the risk but do not eliminate the race on the shared property itself.

### Typical fixture skeleton (NUnit)

```csharp
[TestFixture]
[NonParallelizable]   // if the component under test holds static state
public sealed class FileWritingComponentTests
{
    private string _testDir = null!;

    [SetUp]
    public void SetUp()
    {
        _testDir = Path.Combine(
            Path.GetTempPath(),
            "MyComponent.Tests",
            TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(_testDir);

        MyComponent.OutputDirectory = _testDir;   // redirect the seam
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    [Test]
    public void Error_WithCallStack_IncludesCallStackSection()
    {
        MyComponent.Error("Something failed.", callStack: "at Foo() in Foo.cs:12");

        var content = File.ReadAllText(TodaysLogFile(_testDir));

        Assert.That(content, Does.Contain("--- CallStack ---"));
        Assert.That(content, Does.Contain("at Foo() in Foo.cs:12"));
    }

    [Test]
    public void Error_WithoutCallStack_OmitsCallStackSection()
    {
        MyComponent.Error("Something failed.");

        var content = File.ReadAllText(TodaysLogFile(_testDir));

        Assert.That(content, Does.Not.Contain("--- CallStack ---"));
    }

    [Test]
    public void Error_InvalidDirectory_DoesNotThrow()
    {
        MyComponent.OutputDirectory = @"Z:\does\not\exist\";

        Assert.DoesNotThrow(() => MyComponent.Error("msg"));
    }

    private static string TodaysLogFile(string dir) =>
        Path.Combine(dir, $"{DateTimeOffset.Now:yyyy-MM-dd}.log");
}
```
