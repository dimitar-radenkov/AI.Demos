using AI.Agents.Analysis;

namespace AI.Agents.CodeGeneration;

public interface IDeveloperAgent
{
    Task<CodeArtifactResult> GenerateCodeAsync(Requirements requirements, CancellationToken cancellationToken = default);
}