using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AI.Agents.Presentation;

public sealed partial class PresenterAgent : IAgent<PresentationResult>
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public PresenterAgent(IOptions<AgentSettings> options)
    {
        var settings = options.Value;

        // Create JSON schema for structured output
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(Presentation));

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(schema)
        };

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "Presenter-Agent",
                    Instructions = settings.GetSystemPrompt(),
                    ChatOptions = chatOptions
                });

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<PresentationResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var presentationInput = JsonSerializer.Deserialize<PresentationInput>(input, JsonSerializerOptions.Default);
            
            if (presentationInput == null)
            {
                return PresentationResult.Failure("Failed to deserialize PresentationInput from input");
            }

            var prompt = BuildPresentationPrompt(presentationInput);
            var response = await this.agent.RunAsync(prompt, this.agentThread, cancellationToken: cancellationToken);

            // Parse the structured JSON response directly
            var presentation = JsonSerializer.Deserialize<Presentation>(response.Text, JsonSerializerOptions.Default);

            return presentation is null
                ? PresentationResult.Failure("Failed to parse presentation from agent response")
                : PresentationResult.Success(presentation);
        }
        catch (JsonException ex)
        {
            return PresentationResult.Failure($"Failed to deserialize: {ex.Message}");
        }
        catch (Exception ex)
        {
            return PresentationResult.Failure($"Failed to create presentation: {ex.Message}");
        }
    }

    private static string BuildPresentationPrompt(PresentationInput input)
    {
        var prompt = $"Present the code execution results in a user-friendly format.\n\n";
        prompt += $"User Request: {input.UserRequest}\n\n";
        prompt += "Execution Result:\n";
        prompt += $"- Return Value: {input.ExecutionResult.ReturnValue ?? "null"}\n";
        prompt += $"- Execution Time: {input.ExecutionResult.ExecutionTime.TotalMilliseconds}ms\n";
        prompt += "\nProvide a clear, concise presentation with a summary and formatted result.";

        return prompt;
    }
}
