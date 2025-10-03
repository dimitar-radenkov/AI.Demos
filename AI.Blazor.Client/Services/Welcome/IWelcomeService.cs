namespace AI.Blazor.Client.Services.Welcome;

/// <summary>
/// Service for generating personalized welcome messages using Semantic Kernel.
/// Demonstrates kernel arguments by passing userName and currentDateTime.
/// </summary>
public interface IWelcomeService
{
    /// <summary>
    /// Generates a personalized welcome message.
    /// Demonstrates kernel arguments: userName and currentDateTime.
    /// </summary>
    Task<string> GenerateWelcomeMessageAsync(
        CancellationToken cancellationToken = default);
}