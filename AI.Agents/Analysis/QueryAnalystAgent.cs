using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AI.Agents.Analysis;

public sealed partial class QueryAnalystAgent : IAgent<string, RequirementsResult>
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public QueryAnalystAgent(IOptions<AgentSettings> options)
    {
        var settings = options.Value;
        // Create JSON schema for structured output
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(Requirements));

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(schema: schema)
        };

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "Query-Analyst-Agent",
                    Instructions = settings.GetSystemPrompt(),
                    ChatOptions = chatOptions
                });

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<RequirementsResult> ExecuteAsync(
        string userRequest,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.agent.RunAsync(userRequest, this.agentThread, cancellationToken: cancellationToken);

            // Parse the structured JSON response directly
            var requirements = JsonSerializer.Deserialize<Requirements>(response.Text, JsonSerializerOptions.Default);

            return requirements is null
                ? RequirementsResult.Failure("Failed to parse requirements from agent response")
                : RequirementsResult.Success(requirements);
        }
        catch (JsonException ex)
        {
            return RequirementsResult.Failure($"Failed to deserialize requirements: {ex.Message}");
        }
        catch (Exception ex)
        {
            return RequirementsResult.Failure($"Failed to analyze request: {ex.Message}");
        }
    }
}