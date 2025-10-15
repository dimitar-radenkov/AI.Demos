using AI.Core.Infrastructure;

namespace AI.Agents.QualityAssurance;

public sealed class CodeQuality
{
    public required string Result { get; init; }
    public required TimeSpan Duration { get; init; }
}

public sealed record CodeQualityResult : OperationResult<CodeQuality, CodeQualityResult>;