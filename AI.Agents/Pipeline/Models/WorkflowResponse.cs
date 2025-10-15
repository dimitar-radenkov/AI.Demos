using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using AI.Core.Infrastructure;

namespace AI.Agents.Pipeline.Models;

public sealed class WorkflowResponse
{
    public required Requirements Requirements { get; init; }
    public required CodeArtifact Code { get; init; }
    public required CodeQuality QualityResult { get; init; }
    public required Explanation Explanation { get; init; }
}

public sealed record WorkflowResponseResult : OperationResult<WorkflowResponse, WorkflowResponseResult>;