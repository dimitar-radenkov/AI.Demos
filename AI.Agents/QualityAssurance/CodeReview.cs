using AI.Core.Infrastructure;

namespace AI.Agents.QualityAssurance;

public sealed class CodeReview
{
    public required bool IsApproved { get; init; }
    public required string Comments { get; init; }
}

public sealed record CodeReviewResult : OperationResult<CodeReview, CodeReviewResult>;
