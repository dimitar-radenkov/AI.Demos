using AI.Agents.QualityAssurance;

namespace AI.Agents.Pipeline.Models;

public sealed record ReviewerDecision
{
    public required CodeReview CodeReview { get; init; }
    public required string ExecutableCode { get; init; }
}