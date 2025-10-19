using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;

namespace AI.Agents.Presentation;

public sealed partial class PresenterAgent : IAgent<PresentationResult>
{
    private readonly AIAgent agent;

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
    }

    public async Task<PresentationResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {                 
        var response = await this.agent.RunAsync(input, cancellationToken: cancellationToken);
        var presentation = response.Deserialize<Presentation>(JsonSerializerOptions.Default);

        return response is null
            ? PresentationResult.Failure("Failed to parse presentation from agent response")
            : PresentationResult.Success(presentation);
    }
}
