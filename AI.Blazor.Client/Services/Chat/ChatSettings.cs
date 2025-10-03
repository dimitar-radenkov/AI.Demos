namespace AI.Blazor.Client.Services.Chat;

public sealed class ChatSettings
{
    public const string SectionName = "ChatSettings";
    public required int MaxHistoryMessages { get; init; }
    public required string SystemPrompt { get; init; }
}
