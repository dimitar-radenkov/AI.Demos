namespace AI.Shared.Settings.Agents;

public sealed class AgentsSettings
{
    public const string SectionName = "Agents";

    public required CodeGenerationAgentSettings Developer { get; init; }
    public required QueryAnalystAgentSettings QueryAnalyst { get; init; }
    public required QAAgentSettings QA { get; init; }
}
