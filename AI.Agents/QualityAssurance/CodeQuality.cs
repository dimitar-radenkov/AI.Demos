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
}

public sealed record CodeQualityResult : OperationResult<CodeQuality, CodeQualityResult>;