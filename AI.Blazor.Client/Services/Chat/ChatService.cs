using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AI.Blazor.Client.Services.Chat;

public sealed class ChatService : IChatService
{
    private ChatHistory chatHistory;
    private readonly ChatHistoryTruncationReducer chatReducer;
    private readonly IChatCompletionService chatCompletionService;

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
}
