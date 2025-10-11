using AI.Shared.Infrastructure;

namespace AI.Agents.QualityAssurance;

public enum CodeValidationStatus
{
    Passed,
    Failed,
    Skipped
}

public enum CodeExecutionStatus
{
    Success,
    Failed,
    Rejected,
    NotExecuted,
    Timeout
}

public sealed class CodeQuality
{
    public required CodeValidationStatus ValidationStatus { get; init; }
    public required CodeExecutionStatus ExecutionStatus { get; init; }
    public required string Output { get; init; }
    public required string[] Errors { get; init; }
    public required TimeSpan ExecutionTime { get; init; }
    public string[] AiRecommendations { get; init; } = Array.Empty<string>();
    public bool AiApproved { get; init; } = true;

    public bool IsSuccessful => ValidationStatus == CodeValidationStatus.Passed && ExecutionStatus == CodeExecutionStatus.Success;
}

public sealed record CodeQualityResult : OperationResult<CodeQuality, CodeQualityResult>;