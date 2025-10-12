namespace AI.Agents;

internal static class JsonSerializerOptions
{
    public static readonly System.Text.Json.JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
    };
}
