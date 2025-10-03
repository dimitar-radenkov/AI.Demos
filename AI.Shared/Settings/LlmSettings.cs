namespace AI.Shared.Settings;

public sealed class LlmSettings
{
    public const string SectionName = "LlmSettings";

    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string Model { get; init; }

    public static LlmSettings Default => new()
    {
        BaseUrl = "http://127.0.0.1:55443",
        ApiKey = "your_api_key",
        Model = "openai/gpt-oss-20b"
    };
}