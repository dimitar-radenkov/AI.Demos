namespace AI.Blazor.Client.Services.Welcome;

public sealed class WelcomeSettings
{
    public const string SectionName = "WelcomeSettings";

    public required string UserName { get; init; }
}