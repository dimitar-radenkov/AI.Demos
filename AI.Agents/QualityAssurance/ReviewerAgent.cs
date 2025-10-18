using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AI.Agents.QualityAssurance;

public sealed partial class ReviewerAgent : IAgent<CodeReviewResult>
{
    private readonly AIAgent agent;

    public ReviewerAgent(IOptions<AgentSettings> options)
    {
        var settings = options.Value;
        // Create JSON schema for structured output
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(CodeReview));

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(settings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{settings.BaseUrl}/v1")
            });

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
                schema: schema,
                schemaName: "CodeReview",
                schemaDescription: "Structured code review with approval status and detailed comments")
        };

        this.agent = openAIClient
            .GetChatClient(settings.Model)
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "Reviewer-Agent",
                    Instructions = settings.GetSystemPrompt(),
                    ChatOptions = chatOptions
                });
    }

    public async Task<CodeReviewResult> ExecuteAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.agent.RunAsync(input, cancellationToken: cancellationToken);
            var codeReview = response.Deserialize<CodeReview>(JsonSerializerOptions.Default);

            return codeReview is null
                ? CodeReviewResult.Failure("Failed to parse code review from agent response")
                : CodeReviewResult.Success(codeReview);
        }
        catch (JsonException ex)
        {
            return CodeReviewResult.Failure($"Failed to deserialize: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CodeReviewResult.Failure($"Failed to review code: {ex.Message}");
        }
    }
}
