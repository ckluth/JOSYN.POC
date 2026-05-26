namespace JOSYN.System.Shared.Contract;

/// <summary>
/// Contract definition for a structured error report.
/// Transmitted from the JobHost (frontend) to the JAPServer (backend)
/// when a job error occurred but the pipe transport was still functional.
/// </summary>
public interface IErrorReport
{
    /// <summary>Identifier of the failed job or process.</summary>
    string Causer { get; init; }

    /// <summary>Error message.</summary>
    string Message { get; init; }

    /// <summary>Optional serialized call stack.</summary>
    string? CallStack { get; init; }

    /// <summary>Optional serialized exception details.</summary>
    string? ExceptionDetails { get; init; }

    /// <summary>The point in time when the error occurred.</summary>
    DateTimeOffset OccurredAt { get; init; }
}
