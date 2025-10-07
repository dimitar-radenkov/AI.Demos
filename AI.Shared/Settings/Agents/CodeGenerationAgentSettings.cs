namespace AI.Shared.Settings.Agents;

public sealed class CodeGenerationAgentSettings
{
    public required string Model { get; init; }
    public required string[] SystemPrompt { get; init; }
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }

    public string GetSystemPrompt() => string.Join("\n", SystemPrompt);
}
