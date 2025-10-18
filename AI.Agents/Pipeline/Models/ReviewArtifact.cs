namespace AI.Agents.Pipeline.Models;

public sealed record ReviewArtifact
{
    public required string Code { get; init; }
    public required bool IsApproved { get; init; }
    public required string Feedback { get; init; }
}
