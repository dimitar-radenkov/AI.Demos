using Microsoft.SemanticKernel.ChatCompletion;

namespace AI.Core.Extensions;

public static class ChatHistoryExtensions
{
    /// <summary>
    /// Reduces the chat history if needed to manage token limits.
    /// Returns the reduced history if reduction occurred, otherwise returns the original history.
    /// </summary>
    public static async Task<ChatHistory> ReduceIfNeededAsync(
        this ChatHistory chatHistory,
        ChatHistoryTruncationReducer reducer,
        CancellationToken cancellationToken = default)
    {
        var reducedMessages = await reducer.ReduceAsync(chatHistory, cancellationToken);
        return reducedMessages is not null ? [.. reducedMessages] : chatHistory;
    }
}
