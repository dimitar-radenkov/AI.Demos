namespace AI.Blazor.Client.Services.CodeGeneration;

public interface ICodeGenerationService
{
    Task<string> GenerateCode(
        string userInput,
        CancellationToken cancellationToken = default);
}