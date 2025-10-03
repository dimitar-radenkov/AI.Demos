namespace AI.Blazor.Client.Services.Chat;

public interface IChatService : IDisposable
{
    Task<string> GetResponse(
        string userInput,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<string> GetStreamingResponse(
        string userInput,
        CancellationToken cancellationToken = default);
}
