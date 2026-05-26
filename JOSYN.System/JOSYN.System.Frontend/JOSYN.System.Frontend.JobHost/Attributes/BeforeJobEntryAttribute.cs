namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Marks a method that is executed before the entry point of a job.
/// Suitable for initialization work such as setting up resources or
/// evaluating conditions (e.g. whether parallel execution is permitted)
/// before the actual job is started.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BeforeJobEntryPointAttribute() : Attribute { }