namespace AI.Core.Settings;

public sealed class ScriptRunnerSettings
{
    public const string SectionName = "ScriptRunnerSettings";

    public required int MaxExecutionTimeSeconds { get; init; }
    public required int MaxConcurrentExecutions { get; init; }
    public required string[] AllowedNamespaces { get; init; }
    public required string[] AllowedAssemblies { get; init; }
}
