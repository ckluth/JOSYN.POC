using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log;

/// <summary>
/// Contract definition for the process-local file logger.
/// Writes entries to <c>&lt;LogDirectory&gt;\&lt;yyyy-MM-dd&gt;.log</c>.
/// Overloads with a <c>causer</c> parameter write to a subdirectory of the same name.
/// </summary>
public interface ILocalLog
{
    /// <summary>
    /// Root path for all log files of this process.
    /// Must be set before the first log call if a different storage location is desired.
    /// </summary>
    static abstract string LogDirectory { get; set; }

    /// <summary>
    /// Controls whether log entries are additionally written to the console.
    /// </summary>
    static abstract bool EnableConsoleOutput { get; set; }

    /// <summary>Writes an error entry.</summary>
    static abstract void Error(string message, string? callStack = null, string? exceptionDetails = null);

    /// <summary>Writes an error entry from a <see cref="Result"/>.</summary>
    static abstract void Error(Result result);

    /// <summary>Writes an error entry to the subdirectory of the specified causer.</summary>
    static abstract void Error(string causer, string message, string? callStack = null, string? exceptionDetails = null);

    /// <summary>Writes an error entry from a <see cref="Result"/> to the subdirectory of the specified causer.</summary>
    static abstract void Error(string causer, Result result);

    /// <summary>Writes an info entry.</summary>
    static abstract void Info(string message);

    /// <summary>Writes an info entry to the subdirectory of the specified causer.</summary>
    static abstract void Info(string causer, string message);
}
