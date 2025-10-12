using AI.Agents.Analysis;

namespace AI.Agents.CodeGeneration;

public interface IDeveloperAgent
{
    Task<CodeArtifactResult> GenerateCode(Requirements requirements, CancellationToken cancellationToken = default);
}