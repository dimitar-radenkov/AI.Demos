namespace AI.Core.Settings.Agents;

public sealed class AgentsSettings
{
    public const string SectionName = "Agents";

    public required AgentSettings Developer { get; init; }
    public required AgentSettings QueryAnalyst { get; init; }
    public required AgentSettings QA { get; init; }
    public required AgentSettings Reviewer { get; init; }
}
