using AI.Shared.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.RegularExpressions;

namespace AI.Blazor.Client.Services.CodeGeneration;

public sealed class CodeGenerationService : ICodeGenerationService, IDisposable
{
    private ChatHistory chatHistory;
    private readonly ChatHistoryTruncationReducer chatReducer;
    private readonly IChatCompletionService chatCompletionService;
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public CodeGenerationService(
        Kernel kernel,
        IOptions<CodeGenerationSettings> options,
        IChatCompletionService chatCompletionService)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.Value);

        this.chatHistory = [];
        this.chatHistory.AddSystemMessage(options.Value.GetSystemPrompt());

        this.chatReducer = new ChatHistoryTruncationReducer(options.Value.MaxHistoryMessages);
        this.chatCompletionService = chatCompletionService ?? throw new ArgumentNullException(nameof(chatCompletionService));
    }

    public async Task<string> GenerateCode(string userInput, CancellationToken cancellationToken = default)
    {
        await this.semaphore.WaitAsync(cancellationToken);
        try
        {
            this.chatHistory = await this.chatHistory.ReduceIfNeededAsync(this.chatReducer, cancellationToken);

            this.chatHistory.AddUserMessage(userInput);

            var response = await this.chatCompletionService.GetChatMessageContentAsync(
                this.chatHistory,
                cancellationToken: cancellationToken);

            this.chatHistory.AddAssistantMessage(response.Content!);

            return ExtractCodeFromResponse(response.Content!);
        }
        finally
        {
            this.semaphore.Release();
        }
    }

    private static string ExtractCodeFromResponse(string response)
    {
        // Look for ```csharp code blocks first
        var csharpMatch = Regex.Match(response, @"```csharp\s*\n(.*?)\n```", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (csharpMatch.Success)
        {
            return csharpMatch.Groups[1].Value.Trim();
        }

        // Fallback: look for any code block
        var codeMatch = Regex.Match(response, @"```\s*\n(.*?)\n```", RegexOptions.Singleline);
        if (codeMatch.Success)
        {
            return codeMatch.Groups[1].Value.Trim();
        }

        // If no code blocks found, return the full response
        // This handles cases where the AI doesn't use markdown formatting
        return response.Trim();
    }

    public void Dispose()
    {
        this.semaphore?.Dispose();
    }
}