using System.Text.Json.Serialization;
using AI.Core.Infrastructure;

namespace AI.Agents.QualityAssurance;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CodeValidationStatus
{
    Passed,
    Failed,
    Skipped
}

[JsonConverter(typeof(JsonStringEnumConverter))]
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
    public required string Result { get; init; }
    public required TimeSpan Duration { get; init; }
}

public sealed record CodeQualityResult : OperationResult<CodeQuality, CodeQualityResult>;