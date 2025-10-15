using AI.Agents.Analysis;
using AI.Core.Infrastructure;

namespace AI.Agents.CodeGeneration;

public sealed class CodeArtifact
{
    public required string Code { get; init; }
    public required string Language { get; init; }
    public required DateTime GeneratedAt { get; init; }
    public required Requirements Requirements { get; init; }
}

public sealed record CodeArtifactResult : OperationResult<CodeArtifact, CodeArtifactResult>;