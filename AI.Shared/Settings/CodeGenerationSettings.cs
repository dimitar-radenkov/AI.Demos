namespace AI.Shared.Settings;

public sealed class CodeGenerationSettings
{
    public const string SectionName = "CodeGenerationSettings";

    public required string[] SystemPrompt { get; init; }

    public string GetSystemPrompt() => string.Join("\n", this.SystemPrompt);
}