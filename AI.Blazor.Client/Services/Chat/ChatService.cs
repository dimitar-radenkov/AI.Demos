using AI.Shared.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Runtime.CompilerServices;

namespace AI.Blazor.Client.Services.Chat;

public sealed class ChatService : IChatService, IDisposable
{
    private ChatHistory chatHistory;
    private readonly ChatHistoryTruncationReducer chatReducer;
    private readonly Kernel kernel;
    private readonly IChatCompletionService chatCompletionService;
    private readonly OpenAIPromptExecutionSettings promptSettings;
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public ChatService(
        Kernel kernel,
        IOptions<ChatSettings> chatOptions,
        IChatCompletionService chatCompletionService)
    {
        this.chatHistory = [];
        this.chatHistory.AddSystemMessage(chatOptions.Value.SystemPrompt);
        
        this.chatReducer = new ChatHistoryTruncationReducer(chatOptions.Value.MaxHistoryMessages);
        this.kernel = kernel;
        this.chatCompletionService = chatCompletionService;
        this.promptSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }

    public async Task<string> GetResponse(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            this.chatHistory = await this.chatHistory.ReduceIfNeededAsync(this.chatReducer, cancellationToken);

            this.chatHistory.AddUserMessage(userInput);

            var response = await this.chatCompletionService.GetChatMessageContentAsync(
                this.chatHistory,
                executionSettings: this.promptSettings,
                kernel: this.kernel,               
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
            this.chatHistory = await this.chatHistory.ReduceIfNeededAsync(this.chatReducer, cancellationToken);

            this.chatHistory.AddUserMessage(userInput);

            var fullResponse = new System.Text.StringBuilder();

            await foreach (var chunk in this.chatCompletionService.GetStreamingChatMessageContentsAsync(
                this.chatHistory,
                executionSettings: this.promptSettings,
                kernel: this.kernel,
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
