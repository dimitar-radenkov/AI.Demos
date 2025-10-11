using System.Text.Json.Serialization;
using AI.Shared.Infrastructure;

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
    [JsonPropertyName("validation_status")]
    public required CodeValidationStatus ValidationStatus { get; init; }
    
    [JsonPropertyName("execution_status")]
    public required CodeExecutionStatus ExecutionStatus { get; init; }
    
    [JsonPropertyName("output")]
    public required string Output { get; init; }
    
    [JsonPropertyName("errors")]
    public required string[] Errors { get; init; }
    
    [JsonPropertyName("execution_time")]
    public required TimeSpan ExecutionTime { get; init; }
    
    [JsonPropertyName("ai_recommendations")]
    public string[] AiRecommendations { get; init; } = Array.Empty<string>();
    
    [JsonPropertyName("ai_approved")]
    public bool AiApproved { get; init; } = true;

    public bool IsSuccessful => ValidationStatus == CodeValidationStatus.Passed && ExecutionStatus == CodeExecutionStatus.Success;
}

public sealed record CodeQualityResult : OperationResult<CodeQuality, CodeQualityResult>;