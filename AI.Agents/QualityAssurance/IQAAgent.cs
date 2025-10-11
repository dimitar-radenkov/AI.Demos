using AI.Agents.CodeGeneration;

namespace AI.Agents.QualityAssurance;

public interface IQAAgent
{
    Task<CodeQualityResult> ValidateAndTestAsync(CodeArtifact artifact, CancellationToken cancellationToken = default);
}