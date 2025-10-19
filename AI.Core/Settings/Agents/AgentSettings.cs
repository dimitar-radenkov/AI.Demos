namespace AI.Core.Settings.Agents;

public class AgentSettings
{
    public required string Model { get; init; }
    public required string[] SystemPrompt { get; init; }
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }

    public string GetSystemPrompt() => string.Join("\n", this.SystemPrompt);
}

public static class AgentConfigurationSections
{
    private const string AgentsPrefix = "Agents";

    public const string Developer = $"{AgentsPrefix}:Developer";
    public const string QueryAnalyst = $"{AgentsPrefix}:QueryAnalyst";
    public const string QA = $"{AgentsPrefix}:QA";
    public const string Reviewer = $"{AgentsPrefix}:Reviewer";
    public const string Presenter = $"{AgentsPrefix}:Presenter";
}
