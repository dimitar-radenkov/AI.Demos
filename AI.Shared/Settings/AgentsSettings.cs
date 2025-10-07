using AI.Shared.Settings.Agents;

namespace AI.Shared.Settings;

public sealed class AgentsSettings
{
    public const string SectionName = "Agents";

    public required CodeGenerationAgentSettings DotNetDeveloper { get; init; }
}
