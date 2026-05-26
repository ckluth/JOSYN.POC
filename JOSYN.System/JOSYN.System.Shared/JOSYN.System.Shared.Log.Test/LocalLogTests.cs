using NUnit.Framework;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.System.Shared.Log.Test;

[TestFixture]
[NonParallelizable]
public sealed class LocalLogTests
{
    private string _testDir = null!;
    private string _savedDir = null!;
    private bool _savedConsole;

    [SetUp]
    public void SetUp()
    {
        _savedDir = LocalLog.LogDirectory;
        _savedConsole = LocalLog.EnableConsoleOutput;

        _testDir = Path.Combine(
            Path.GetTempPath(),
            "JOSYN.System.Shared.Log.Test",
            TestContext.CurrentContext.Test.ID);
        Directory.CreateDirectory(_testDir);

        LocalLog.LogDirectory = _testDir;
        LocalLog.EnableConsoleOutput = false;
    }

    [TearDown]
    public void TearDown()
    {
        LocalLog.LogDirectory = _savedDir;
        LocalLog.EnableConsoleOutput = _savedConsole;

        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    // ── Format ────────────────────────────────────────────────────────────────

    [Test]
    public void Error_ContainsErrorLevelHeader()
    {
        LocalLog.Error("msg");

        Assert.That(ReadLog(_testDir), Does.Contain("[ERROR]"));
    }

    [Test]
    public void Info_ContainsInfoLevelHeader()
    {
        LocalLog.Info("msg");

        Assert.That(ReadLog(_testDir), Does.Contain("[INFO]"));
    }

    [Test]
    public void Error_ContainsMessage()
    {
        LocalLog.Error("Verbindung fehlgeschlagen.");

        Assert.That(ReadLog(_testDir), Does.Contain("Verbindung fehlgeschlagen."));
    }

    [Test]
    public void Error_AlwaysContains80DashSeparator()
    {
        LocalLog.Error("msg");

        Assert.That(ReadLog(_testDir), Does.Contain(new string('-', 80)));
    }

    [Test]
    public void Error_WithCallStack_IncludesCallStackSection()
    {
        LocalLog.Error("msg", callStack: "  at SomeClass.SomeMethod (File.cs:42)");

        var content = ReadLog(_testDir);
        Assert.That(content, Does.Contain("--- CallStack ---"));
        Assert.That(content, Does.Contain("at SomeClass.SomeMethod"));
    }

    [Test]
    public void Error_WithoutCallStack_OmitsCallStackSection()
    {
        LocalLog.Error("msg", callStack: null);

        Assert.That(ReadLog(_testDir), Does.Not.Contain("--- CallStack ---"));
    }

    [Test]
    public void Error_WithEmptyCallStack_OmitsCallStackSection()
    {
        LocalLog.Error("msg", callStack: "");

        Assert.That(ReadLog(_testDir), Does.Not.Contain("--- CallStack ---"));
    }

    [Test]
    public void Error_WithExceptionDetails_IncludesExceptionSection()
    {
        LocalLog.Error("msg", exceptionDetails: "System.IO.IOException: Zugriff verweigert.");

        var content = ReadLog(_testDir);
        Assert.That(content, Does.Contain("--- Exception ---"));
        Assert.That(content, Does.Contain("System.IO.IOException"));
    }

    [Test]
    public void Error_WithoutExceptionDetails_OmitsExceptionSection()
    {
        LocalLog.Error("msg", exceptionDetails: null);

        Assert.That(ReadLog(_testDir), Does.Not.Contain("--- Exception ---"));
    }

    // ── Routing ───────────────────────────────────────────────────────────────

    [Test]
    public void Error_NoCauser_WritesToLogDirectoryRoot()
    {
        LocalLog.Error("msg");

        Assert.That(File.Exists(TodaysLogFile(_testDir)), Is.True);
    }

    [Test]
    public void Info_NoCauser_WritesToLogDirectoryRoot()
    {
        LocalLog.Info("msg");

        Assert.That(File.Exists(TodaysLogFile(_testDir)), Is.True);
    }

    [Test]
    public void Error_WithCauser_WritesToCauserSubdirectory()
    {
        LocalLog.Error(causer: "JobHost", message: "msg");

        Assert.That(File.Exists(TodaysLogFile(_testDir, "JobHost")), Is.True);
        Assert.That(File.Exists(TodaysLogFile(_testDir)), Is.False);
    }

    [Test]
    public void Info_WithCauser_WritesToCauserSubdirectory()
    {
        LocalLog.Info("JobHost", "msg");

        Assert.That(File.Exists(TodaysLogFile(_testDir, "JobHost")), Is.True);
        Assert.That(File.Exists(TodaysLogFile(_testDir)), Is.False);
    }

    [Test]
    public void Error_DifferentCausers_WriteToSeparateSubdirectories()
    {
        LocalLog.Error(causer: "ServiceA", message: "msg A");
        LocalLog.Error(causer: "ServiceB", message: "msg B");

        Assert.That(ReadLog(_testDir, "ServiceA"), Does.Contain("msg A"));
        Assert.That(ReadLog(_testDir, "ServiceB"), Does.Contain("msg B"));
        Assert.That(ReadLog(_testDir, "ServiceA"), Does.Not.Contain("msg B"));
    }

    // ── Result overloads ──────────────────────────────────────────────────────

    [Test]
    public void Error_FromResult_ContainsErrorMessage()
    {
        var result = Result.Fail("Datenbankverbindung unterbrochen.");

        LocalLog.Error(result);

        Assert.That(ReadLog(_testDir), Does.Contain("Datenbankverbindung unterbrochen."));
    }

    [Test]
    public void Error_FromResult_ContainsCallStack()
    {
        var result = Result.Fail("msg");

        LocalLog.Error(result);

        Assert.That(ReadLog(_testDir), Does.Contain("--- CallStack ---"));
    }

    [Test]
    public void Error_FromResult_WithException_ContainsExceptionSection()
    {
        Result result = Result.Fail("msg", new InvalidOperationException("Ungültiger Zustand."));

        LocalLog.Error(result);

        var content = ReadLog(_testDir);
        Assert.That(content, Does.Contain("--- Exception ---"));
        Assert.That(content, Does.Contain("InvalidOperationException"));
    }

    [Test]
    public void Error_WithCauser_FromResult_RoutesToSubdirectory()
    {
        var result = Result.Fail("msg");

        LocalLog.Error(causer: "JobHost", result: result);

        Assert.That(File.Exists(TodaysLogFile(_testDir, "JobHost")), Is.True);
    }

    // ── Resilience ────────────────────────────────────────────────────────────

    [Test]
    public void Error_InvalidDirectory_DoesNotThrow()
    {
        LocalLog.LogDirectory = @"Z:\does\not\exist\at\all\";

        Assert.DoesNotThrow(() => LocalLog.Error("msg"));
    }

    [Test]
    public void Info_InvalidDirectory_DoesNotThrow()
    {
        LocalLog.LogDirectory = @"Z:\does\not\exist\at\all\";

        Assert.DoesNotThrow(() => LocalLog.Info("msg"));
    }

    // ── Console output ────────────────────────────────────────────────────────

    [Test]
    public void Error_WhenConsoleOutputEnabled_WritesToConsole()
    {
        LocalLog.EnableConsoleOutput = true;
        var originalOut = Console.Out;
        var sw = new StringWriter();
        try
        {
            Console.SetOut(sw);
            LocalLog.Error("Konsoleneintrag erwartet.");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.That(sw.ToString(), Does.Contain("Konsoleneintrag erwartet."));
    }

    [Test]
    public void Error_WhenConsoleOutputDisabled_DoesNotWriteToConsole()
    {
        LocalLog.EnableConsoleOutput = false;
        var originalOut = Console.Out;
        var sw = new StringWriter();
        try
        {
            Console.SetOut(sw);
            LocalLog.Error("Kein Konsoleneintrag.");
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        Assert.That(sw.ToString(), Is.Empty);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string TodaysLogFile(string dir, string? causer = null) =>
        causer is null
            ? Path.Combine(dir, $"{DateTimeOffset.Now:yyyy-MM-dd}.log")
            : Path.Combine(dir, causer, $"{DateTimeOffset.Now:yyyy-MM-dd}.log");

    private static string ReadLog(string dir, string? causer = null) =>
        File.ReadAllText(TodaysLogFile(dir, causer));
}
