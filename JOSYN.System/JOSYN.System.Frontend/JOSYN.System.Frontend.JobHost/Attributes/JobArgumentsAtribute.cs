namespace JOSYN.System.Frontend.JobHost.Attributes;

/// <summary>
/// Marks a class as the arguments type for a job.
/// Used to explicitly designate the parameter type of the <see cref="JobEntryPointAttribute"/>
/// method as job arguments.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class JobArgumentsAttribute() : Attribute { }