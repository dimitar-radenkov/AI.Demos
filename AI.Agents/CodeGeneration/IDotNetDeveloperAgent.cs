namespace AI.Agents.CodeGeneration;

public interface IDotNetDeveloperAgent
{
    Task<string> GenerateCodeAsync(string userInput, CancellationToken cancellationToken = default);
}