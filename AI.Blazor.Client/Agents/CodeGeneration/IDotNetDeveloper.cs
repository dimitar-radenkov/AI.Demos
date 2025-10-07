namespace AI.Blazor.Client.Agents.CodeGeneration;

public interface IDotNetDeveloper
{
    Task<string> GenerateCodeAsync(string userInput, CancellationToken cancellationToken = default);
}