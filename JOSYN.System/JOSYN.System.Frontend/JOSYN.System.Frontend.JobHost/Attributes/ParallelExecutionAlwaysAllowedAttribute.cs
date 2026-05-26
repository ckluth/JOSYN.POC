namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Declares that the associated job method may be executed in parallel.
/// The <paramref name="isAllowed"/> parameter controls whether parallel execution is
/// enabled (<c>true</c>, default) or explicitly disabled (<c>false</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ParallelExecutionAllowedAttribute(bool isAllowed = true) : Attribute
{
    /// <summary>Indicates whether parallel execution is permitted.</summary>
    public bool IsAllowed => isAllowed;
}