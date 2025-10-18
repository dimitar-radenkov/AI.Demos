using AI.Services.CodeExecution.Models;

namespace AI.Agents.Pipeline.Models;

public sealed record ExecutionArtifact
{
    public required string Code { get; init; }
    public required ExecutionResult Result { get; init; }
}