using NUnit.Framework;
using JOSYN.Foundation.ResultPattern;
using JOSYN.System.Frontend.JobHost.Attributes;

namespace JOSYN.System.Frontend.JobHost.Test;

[TestFixture]
public sealed class JobInvokerTests
{
    // ── Entry point discovery ──────────────────────────────────────────────────

    [Test]
    public async Task InvokeJob_NoEntryPoint_Fails()
    {
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), Array.Empty<Type>());

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain(nameof(JobEntryPointAttribute)));
    }

    [Test]
    public async Task InvokeJob_MultipleEntryPoints_Fails()
    {
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), [typeof(StubJobAlpha), typeof(StubJobBeta)]);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Mehrere"));
    }

    // ── Successful invocation ──────────────────────────────────────────────────

    [Test]
    public async Task InvokeJob_VoidEntryPoint_Succeeds()
    {
        var result = await JobInvoker.InvokeJob(new FakeProtocol(), [typeof(StubVoidJob)]);

        Assert.That(result.Succeeded, Is.True);
    }
}

