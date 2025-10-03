using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AI.Blazor.Client.Services.Chat;

public sealed class ChatService : IChatService, IDisposable
{
    private ChatHistory chatHistory;
    private readonly ChatHistoryTruncationReducer chatReducer;
    private readonly IChatCompletionService chatCompletionService;
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public ChatService(
        IOptions<ChatSettings> chatOptions,
        IChatCompletionService chatCompletionService)
    {
        this.chatHistory = [];
        this.chatHistory.AddSystemMessage(chatOptions.Value.SystemPrompt);
        
        this.chatReducer = new ChatHistoryTruncationReducer(chatOptions.Value.MaxHistoryMessages);

        this.chatCompletionService = chatCompletionService;
    }

    public async Task<string> GetResponse(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            var reducedHistory = await this.chatReducer.ReduceAsync(this.chatHistory, cancellationToken);
            if (reducedHistory is not null)
            {
                this.chatHistory = [.. reducedHistory];
            }

            this.chatHistory.AddUserMessage(userInput);

            var response = await this.chatCompletionService.GetChatMessageContentAsync(
                this.chatHistory,
                cancellationToken: cancellationToken);

            this.chatHistory.AddAssistantMessage(response.Content!);

            return response.Content!;
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public async IAsyncEnumerable<string> GetStreamingResponse(
        string userInput,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            var reducedHistory = await this.chatReducer.ReduceAsync(this.chatHistory, cancellationToken);
            if (reducedHistory is not null)
            {
                this.chatHistory = [.. reducedHistory];
            }

            this.chatHistory.AddUserMessage(userInput);

            var fullResponse = new System.Text.StringBuilder();

            await foreach (var chunk in this.chatCompletionService.GetStreamingChatMessageContentsAsync(
                this.chatHistory,
                cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    fullResponse.Append(chunk.Content);
                    yield return chunk.Content;
                }
            }

            this.chatHistory.AddAssistantMessage(fullResponse.ToString());
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    public void Dispose()
    {
        this.semaphore?.Dispose();
    }
}
