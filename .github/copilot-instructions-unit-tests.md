# Unit Test Quality Attributes

> **Version:** 1.0 — .NET 10, C# 14, NUnit 4.x
>
> Sections marked **[NUnit]** are NUnit-specific. All other rules apply regardless of test framework.

## Coverage

Cover every path where undetected misbehaviour is realistically possible — not every line.
Required: positive cases, negative cases, edge cases, and boundary values.

## Naming

**Test methods:** `SubjectUnderTest_Scenario_ExpectedResult`

- For operators or constructors use a descriptive subject:
  `ImplicitConversion_FromException_SetsCallerInfo`
- For properties:
  `CallStackAsString_OnSuccess_ReturnsNoCallstackMessage`

**Test classes and files:** `<SubjectUnderTest>Tests.cs`  
Split into multiple files when a subject has clearly distinct logical concerns
(e.g. `ResultTests.cs`, `ResultTestsPropagate.cs`).

## Structure

- **Arrange-Act-Assert** — three distinct phases; separate with a blank line unless the
  test is trivially a single statement.
- **Prefer fluent assertions** (`Assert.That(x, Is.EqualTo(y))`). **[NUnit]**
- **One observable outcome per test** — multiple physical `Assert.That` calls are fine
  when they jointly verify a single outcome (e.g. "CallerInfo was populated with file and
  line"). A good test: would an assertion failure on any one of them make the others
  redundant to report? If yes, they belong together. If no, split the test.
- **No flow control** — no loops, conditionals, or branching. Local helper functions and
  static factory methods inside or next to a test are fine; they must not alter *which*
  assertions execute.
- **Tests are independent** — no shared mutable state, no reliance on execution order.
  `[SetUp]` **[NUnit]** is acceptable for shared, truly read-only construction (e.g. a
  fresh subject instance). Never use it to share state that individual tests mutate.
- **`[TestCase]`** **[NUnit]** — use when the same behaviour must hold across multiple
  input/output pairs and the test name + parameter value together are self-explanatory.
  Prefer it over copy-pasted test methods.

## Exception Testing

**[NUnit]** Use `Assert.Throws<TException>(() => ...)` or the `Does.Throw` constraint.
Do not use `[ExpectedException]`.

## Determinism

Tests must produce the same result on every run regardless of execution order, time,
machine, or randomness. Tests that touch the file system, clock, or environment are
integration tests — keep them in a separate assembly or mark them explicitly.

## Skipping Tests

Do not commit `[Ignore]`-decorated **[NUnit]** tests without a reason string. A permanently
ignored test is dead weight; delete it or fix it.

## Test Doubles

- **Stub** — canned return value; use to control a dependency's output.
- **Mock** — interaction verification; use only when the *call itself* — not its
  observable side effect — is the behaviour under test. Reach for stubs first.
- **Fake** — lightweight working implementation; use for stateful dependencies
  (repositories, in-memory queues).
- Avoid over-mocking: if every collaborator is replaced, you are testing wiring, not
  behaviour.

## Comments in Tests

Only add a comment if the purpose is genuinely not obvious from the test name and
assertions alone.

## General AI Advice!

- Your context might be one of these:
	- Analyze this new building-block and create a fresh, first, high-quality test suite for it.
	- I have made changes in the code, which is targeted by those tests. Analyze the changes and update the tests accordingly.
	- I have an already existing test suite; i want a discussion about the quality. Analyze the test suite and suggest improvements.

- Don't be a pleaser! Don't write tests just to increase coverage or satisfy a checklist. Write them to verify meaningful behaviour and edge cases.
- Never write pseudo unit tests that would be more of an integration test. If you can't write a proper unit test, just say it. Don't try to force it.
- Don't reach for "all green" at any cost; if you detect errors or flaws in the test-targets, write tests that expose those issues and that help fixing them. 
- Don't be afraid to take into consideration to delete tests that no longer serve a purpose or that are too brittle to maintain. Focus on quality, not quantity.
- Don't be afraid to refactor tests when the production code changes. Tests are code too, and they should be kept clean and maintainable.
- When in any doubt: interrupt your proceeding and contact your human!
- After an analysis or edit session, create a temporary summary of what you have done for documentation purposes. Your target is the human who always will review your work.
- Always remember: be a good and responsible servant - you are not the last decision maker, that's the one in front of the keyboard...




