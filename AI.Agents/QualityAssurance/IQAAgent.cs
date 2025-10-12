using AI.Agents.CodeGeneration;

namespace AI.Agents.QualityAssurance;

public interface IQAAgent
{
    Task<CodeQualityResult> Validate(CodeArtifact artifact, CancellationToken cancellationToken = default);
}