namespace MyDemoJob;

public sealed record MyJobConfig
{
    public required string Item1 { get; init; }
    public required string Item2 { get; init; }
    public int Item3 { get; init; }
    public bool Item4 { get; init; }
}