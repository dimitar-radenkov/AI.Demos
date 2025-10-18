using AI.Agents.QualityAssurance;

namespace AI.Agents.Pipeline.Models;

public sealed record ReviewerDecision
{
    public required CodeReview CodeReview { get; init; }
    public required string ExecutableCode { get; init; }
}

public sealed record CodeDisapprovedMessage
{
    public required string Comments { get; init; }
    public required string ExecutableCode { get; init; }
}

public sealed record CodeExecutionErrorMessage
{
    public required string Errors { get; init; }
    public required string ExecutableCode { get; init; }
}