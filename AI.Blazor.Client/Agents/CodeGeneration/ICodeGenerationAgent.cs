namespace AI.Blazor.Client.Agents.CodeGeneration;

public interface ICodeGenerationAgent
{
    Task<string> GenerateCodeAsync(string userInput, CancellationToken cancellationToken = default);
}