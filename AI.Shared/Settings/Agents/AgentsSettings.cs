namespace AI.Shared.Settings.Agents;

public sealed class AgentsSettings
{
    public const string SectionName = "Agents";

    public required CodeGenerationAgentSettings DotNetDeveloper { get; init; }
    public required QueryAnalystAgentSettings QueryAnalyst { get; init; }
}
