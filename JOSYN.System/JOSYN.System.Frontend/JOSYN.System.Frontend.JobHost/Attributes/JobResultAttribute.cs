namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Marks a class as the result type of a job.
/// Used to explicitly designate the return type of the <see cref="JobEntryPointAttribute"/>
/// method as the job result.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JobResultAttribute() : Attribute { }