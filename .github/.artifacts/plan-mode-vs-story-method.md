# /plan Mode vs. Story Method

**Question:** Is the CLI's built-in `/plan` mode redundant with the Story Method? Should it be ignored in favour of the Story Method?

**Answer:** Yes — they are complementary in theory, but the Story Method is strictly superior for this workflow.

| | `/plan` mode | Story Method |
|---|---|---|
| Scope | Single session, single task | Multi-session, evolving story |
| Persistence | Temporary session folder — gone after session | `.github/stories/` — permanent, repo-committed |
| Purpose | User approves implementation steps before coding | Accumulates decisions, continuity, knowledge across sessions |
| Audience | User in the moment | User + future-user + AI in session N+7 |

**Conclusion:** `/plan` is a lightweight one-shot guardrail for users without a persistent collaboration method. The Story Method covers everything `/plan` does — and far more. Ignore `/plan` for good reasons.
