namespace AI.Agents.Presentation;

public sealed record Presentation
{
    public required string Summary { get; init; }

    public required string FormattedResult { get; init; }
}
