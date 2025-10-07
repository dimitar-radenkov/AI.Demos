namespace AI.Blazor.Client.Agents.CodeGeneration;

public interface IDotNetDeveloperAgent
{
    Task<string> GenerateCodeAsync(string userInput, CancellationToken cancellationToken = default);
}