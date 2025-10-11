using AI.Agents.Models;

namespace AI.Agents.CodeGeneration;

public interface IDeveloperAgent
{
    Task<CodeArtifactResult> GenerateCodeAsync(Requirements requirements, CancellationToken cancellationToken = default);
}