using System.Reflection;
using System.Text;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log;

/// <inheritdoc cref="ILocalLog"/>
public static class LocalLog
{
    /// <inheritdoc cref="ILocalLog.LogDirectory"/>
    public static string LogDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "logs");

    /// <inheritdoc cref="ILocalLog.EnableConsoleOutput"/>
    public static bool EnableConsoleOutput { get; set; } = false;

    /// <inheritdoc cref="ILocalLog.Error(string, string?, string?)"/>
    public static void Error(string message, string? callStack = null, string? exceptionDetails = null)
    {
        var entry = FormatEntry("ERROR", message, callStack, exceptionDetails);
        WriteToFile(LogDirectory, entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Red);
    }

    /// <inheritdoc cref="ILocalLog.Error(Result)"/>
    public static void Error(Result result) =>
        Error(result.ErrorMessage ?? string.Empty, result.CallStackAsString, result.Exception?.ToString());

    /// <inheritdoc cref="ILocalLog.Error(string, string, string?, string?)"/>
    public static void Error(string causer, string message, string? callStack = null, string? exceptionDetails = null)
    {
        var entry = FormatEntry("ERROR", message, callStack, exceptionDetails);
        WriteToFile(Path.Combine(LogDirectory, causer), entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Red);
    }

    /// <inheritdoc cref="ILocalLog.Error(string, Result)"/>
    public static void Error(string causer, Result result) =>
        Error(causer, result.ErrorMessage ?? string.Empty, result.CallStackAsString, result.Exception?.ToString());

    /// <inheritdoc cref="ILocalLog.Info(string)"/>
    public static void Info(string message)
    {
        var entry = FormatEntry("INFO", message);
        WriteToFile(LogDirectory, entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Gray);
    }

    /// <inheritdoc cref="ILocalLog.Info(string, string)"/>
    public static void Info(string causer, string message)
    {
        var entry = FormatEntry("INFO", message);
        WriteToFile(Path.Combine(LogDirectory, causer), entry);
        if (EnableConsoleOutput)
            WriteToConsole(entry, ConsoleColor.Gray);
    }

    // -------------------------------------------------------------------------

    internal static string FormatEntry(string level, string message, string? callStack = null, string? exceptionDetails = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss zzz}] [{level}]");
        sb.AppendLine(message);
        if (!string.IsNullOrEmpty(callStack))
        {
            sb.AppendLine("--- CallStack ---");
            sb.AppendLine(callStack);
        }
        if (!string.IsNullOrEmpty(exceptionDetails))
        {
            sb.AppendLine("--- Exception ---");
            sb.AppendLine(exceptionDetails);
        }
        sb.AppendLine(new string('-', 80));
        return sb.ToString();
    }

    private static void WriteToFile(string directory, string entry)
    {
        try
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{DateTimeOffset.Now:yyyy-MM-dd}.log");
            File.AppendAllText(path, entry, Encoding.UTF8);
        }
        catch
        {
            // Schreibfehler dürfen den Host-Prozess nicht gefährden.
        }
    }

    private static void WriteToConsole(string entry, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(entry);
        Console.ResetColor();
    }
}
