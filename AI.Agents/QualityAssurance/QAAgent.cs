using AI.Agents.CodeGeneration;
using AI.Shared.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;

namespace AI.Agents.QualityAssurance;

public sealed partial class QAAgent : IQAAgent
{
    private readonly ILogger<QAAgent> logger;
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public QAAgent(
        IOptions<AgentsSettings> agentsSettings,
        QAPlugin qaPlugin,
        ILogger<QAAgent> logger)
    {
        this.logger = logger;

        var qaSettings = agentsSettings.Value.QA;

        // Create JSON schema for structured output
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(CodeQuality));

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(qaSettings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{qaSettings.BaseUrl}/v1")
            });

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
                schema: schema,
                schemaName: "CodeQuality",
                schemaDescription: "Code quality assessment with validation, execution results, and AI recommendations"),
            Tools =
            [
                AIFunctionFactory.Create(qaPlugin.ValidateCode),
                AIFunctionFactory.Create(qaPlugin.ExecuteCode)
            ]
        };

        this.agent = openAIClient
            .GetChatClient(qaSettings.Model)
            .CreateAIAgent(
                new ChatClientAgentOptions
                {
                    Name = "QA-Agent",
                    Instructions = qaSettings.GetSystemPrompt(),
                    ChatOptions = chatOptions,
                });

        this.agentThread = this.agent.GetNewThread();
    }

    public async Task<CodeQualityResult> ValidateAndTestAsync(CodeArtifact artifact, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            this.logger.LogInformation("Starting AI-powered QA validation for {CodeLength} characters", artifact.Code?.Length ?? 0);

            if (string.IsNullOrWhiteSpace(artifact.Code))
            {
                return CodeQualityResult.Failure("Code is empty or null");
            }

            var prompt = $"Analyze and test this C# code using ValidateCode and ExecuteCode tools, then provide your CodeQuality assessment:\n\n{artifact.Code}";

            var response = await this.agent.RunAsync(prompt, this.agentThread, cancellationToken: cancellationToken);
            var quality = response.Deserialize<CodeQuality>(JsonSerializerOptions.Web);

            stopwatch.Stop();

            this.logger.LogInformation(
                "QA Review: Approved={Approved}, Validation={ValidationStatus}, Execution={ExecutionStatus}",
                quality.AiApproved,
                quality.ValidationStatus,
                quality.ExecutionStatus);

            // Update ExecutionTime from stopwatch
            var result = new CodeQuality
            {
                ValidationStatus = quality.ValidationStatus,
                ExecutionStatus = quality.ExecutionStatus,
                Output = quality.Output,
                Errors = quality.Errors,
                ExecutionTime = stopwatch.Elapsed,
                AiRecommendations = quality.AiRecommendations,
                AiApproved = quality.AiApproved
            };

            return CodeQualityResult.Success(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error during QA validation");
            return CodeQualityResult.Failure($"Unexpected error: {ex.Message}");
        }
    }
}