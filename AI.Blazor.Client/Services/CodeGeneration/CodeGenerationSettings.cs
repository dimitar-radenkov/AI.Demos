namespace AI.Blazor.Client.Services.CodeGeneration;

public sealed class CodeGenerationSettings
{

    public const string SectionName = "CodeGenerationSettings";

    public required int MaxHistoryMessages { get; init; }    
    public required string[] SystemPrompt { get; init; }
    
    public string GetSystemPrompt() => string.Join("\n", SystemPrompt);
}
