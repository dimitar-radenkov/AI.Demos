using AI.Agents.CodeGeneration;
using AI.Core.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Text.Json;

namespace AI.Agents.QualityAssurance;

public sealed partial class ReviewerAgent : IAgent<CodeArtifact, CodeReviewResult>
{
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

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

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<CodeReviewResult> ExecuteAsync(
        CodeArtifact artifact,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(artifact.Code))
            {
                return CodeReviewResult.Failure("Code artifact is empty or null");
            }

            var prompt = BuildReviewPrompt(artifact);
            var response = await this.agent.RunAsync(prompt, this.agentThread, cancellationToken: cancellationToken);

            // Parse the structured JSON response directly
            var codeReview = JsonSerializer.Deserialize<CodeReview>(response.Text, JsonSerializerOptions.Default);

            return codeReview is null
                ? CodeReviewResult.Failure("Failed to parse code review from agent response")
                : CodeReviewResult.Success(codeReview);
        }
        catch (JsonException ex)
        {
            return CodeReviewResult.Failure($"Failed to deserialize code review: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CodeReviewResult.Failure($"Failed to review code: {ex.Message}");
        }
    }

    private static string BuildReviewPrompt(CodeArtifact artifact)
    {
        var prompt = "Review the following C# code for quality, best practices, security, and maintainability:\n\n";
        prompt += $"```csharp\n{artifact.Code}\n```\n\n";

        if (artifact.Requirements is not null)
        {
            prompt += $"Original Requirements:\n";
            prompt += $"- Task: {artifact.Requirements.Task}\n";

            if (artifact.Requirements.Constraints.Length > 0)
            {
                prompt += $"- Constraints: {string.Join(", ", artifact.Requirements.Constraints)}\n";
            }
        }

        prompt += "\nProvide a thorough code review with your assessment.";

        return prompt;
    }
}
