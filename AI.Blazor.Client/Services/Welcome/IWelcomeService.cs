namespace AI.Blazor.Client.Services.Welcome;

public interface IWelcomeService
{
    Task<string> GenerateWelcomeMessage(
        CancellationToken cancellationToken = default);
}