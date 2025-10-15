namespace AI.Core.Settings;

public sealed class CodeExecutionSettings
{
    public const string SectionName = "CodeExecutionSettings";

    public required int MaxExecutionTimeSeconds { get; init; }
    public required int MaxConcurrentExecutions { get; init; }
    public required string[] AllowedNamespaces { get; init; }
    public required string[] AllowedAssemblies { get; init; }
    public bool EnableScriptCaching { get; init; }
    public int MaxCacheSize { get; init; }
}
