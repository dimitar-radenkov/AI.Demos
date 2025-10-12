using AI.Shared.Infrastructure;

namespace AI.Agents.Analysis;

public sealed class Explanation
{
    public required string Summary { get; init; }
    public required string Details { get; init; }
    public required string CodeExplanation { get; init; }
    public required string ResultExplanation { get; init; }
}

public sealed record ExplanationResult : OperationResult<Explanation, ExplanationResult>;