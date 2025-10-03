using AI.Client.Utils;
using AI.Shared.Settings;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var llmOptions = LlmSettings.Default;

var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: llmOptions.Model,
    apiKey: llmOptions.ApiKey,
    endpoint: new Uri($"{llmOptions.BaseUrl}/v1"));

var kernel = kernelBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var chatHistoryReducer = new ChatHistoryTruncationReducer(targetCount: 5);
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("You are a helpful assistant.");

while (true)
{
    var userPrompt = ConsoleHelper.ReadUserInput();
    if (userPrompt == "stop") break;

    var reducedChatHistory = await chatHistoryReducer.ReduceAsync(chatHistory);
    if(reducedChatHistory is not null)
    {
        chatHistory = [.. reducedChatHistory];
        ConsoleHelper.WriteWarning($"Chat history was reduced. History size {chatHistory.Count}.");
    }

    chatHistory.AddUserMessage(userPrompt!);
    var assitantResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

    ConsoleHelper.WriteAssistant(assitantResponse.Content!);
    chatHistory.AddAssistantMessage(assitantResponse.Content!);
}