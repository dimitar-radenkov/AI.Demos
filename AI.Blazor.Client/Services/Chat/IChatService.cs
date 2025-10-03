namespace AI.Blazor.Client.Services.Chat;

public interface IChatService
{
    Task<string> GetResponse(
        string userInput,
        CancellationToken cancellationToken = default);
}
