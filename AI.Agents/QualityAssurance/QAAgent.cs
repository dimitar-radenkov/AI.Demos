using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using AI.Agents.CodeGeneration;
using AI.Shared.Services.CodeExecution;
using AI.Shared.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;

namespace AI.Agents.QualityAssurance;

public sealed partial class QAAgent : IQAAgent
{
    private readonly ICodeExecutionService codeExecutionService;
    private readonly ILogger<QAAgent> logger;
    private readonly AIAgent agent;
    private readonly AgentThread agentThread;

    public QAAgent(
        IOptions<AgentsSettings> agentsSettings,
        ICodeExecutionService codeExecutionService,
        ILogger<QAAgent> logger)
    {
        this.codeExecutionService = codeExecutionService;
        this.logger = logger;

        var qaSettings = agentsSettings.Value.QA;

        // Create JSON schema for structured output
        var schema = AIJsonUtilities.CreateJsonSchema(typeof(AiReviewResult));

        var chatOptions = new ChatOptions
        {
            ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
                schema: schema,
                schemaName: "CodeReview",
                schemaDescription: "Code quality review with approval status and feedback")
        };

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(qaSettings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{qaSettings.BaseUrl}/v1")
            });

        this.agent = openAIClient
            .GetChatClient(qaSettings.Model)
            .CreateAIAgent(new ChatClientAgentOptions
            {
                Name = "QA-Agent",
                Instructions = qaSettings.GetSystemPrompt(),
                ChatOptions = chatOptions
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
                return CreateFailedResult(
                    CodeValidationStatus.Failed,
                    CodeExecutionStatus.NotExecuted,
                    stopwatch.Elapsed,
                    "Code is empty or null");
            }

            // Step 1: AI Code Review with structured output
            var reviewPrompt = $"Analyze the following C# code:\n\n```csharp\n{artifact.Code}\n```";
            var response = await this.agent.RunAsync(reviewPrompt, this.agentThread, cancellationToken: cancellationToken);

            this.logger.LogDebug("AI Review Response: {Response}", response.Text);

            // Deserialize structured output directly
            var reviewResult = response.Deserialize<AiReviewResult>(JsonSerializerOptions.Web);
            if (!reviewResult.Approved)
            {
                this.logger.LogWarning("AI rejected code. Issues: {Issues}", string.Join(", ", reviewResult.Issues));
                return CreateFailedResult(
                    CodeValidationStatus.Failed,
                    CodeExecutionStatus.NotExecuted,
                    stopwatch.Elapsed,
                    reviewResult.Issues.Concat(reviewResult.Warnings).ToArray());
            }

            // Step 2: Compile Validation
            var validationResult = await this.codeExecutionService.ValidateCode(artifact.Code);
            if (!validationResult.IsSuccess)
            {
                this.logger.LogWarning("Compilation failed: {Error}", validationResult.ErrorMessage);
                return CreateFailedResult(
                    CodeValidationStatus.Failed,
                    CodeExecutionStatus.NotExecuted,
                    stopwatch.Elapsed,
                    validationResult.ErrorMessage ?? "Compilation failed");
            }

            // Step 3: Execute Code
            var executionResult = await this.codeExecutionService.ExecuteCode(artifact.Code, cancellationToken);
            stopwatch.Stop();

            if (executionResult.IsSuccess && executionResult.Data != null)
            {
                this.logger.LogInformation("Execution succeeded in {Ms}ms", executionResult.Data.ExecutionTime.TotalMilliseconds);
                return CreateSuccessResult(
                    executionResult.Data.ReturnValue?.ToString() ?? string.Empty,
                    executionResult.Data.ExecutionTime,
                    reviewResult.Recommendations);
            }

            var errorMessage = executionResult.ErrorMessage ?? "Execution failed";
            var isTimeout = errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase);
            var executionStatus = isTimeout ? CodeExecutionStatus.Timeout : CodeExecutionStatus.Failed;

            return CreateFailedResult(
                CodeValidationStatus.Passed,
                executionStatus,
                stopwatch.Elapsed,
                errorMessage);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error during QA validation");
            return CodeQualityResult.Failure($"Unexpected error: {ex.Message}");
        }
    }



    private static CodeQualityResult CreateSuccessResult(string output, TimeSpan executionTime, string[] recommendations)
    {
        return CodeQualityResult.Success(new CodeQuality
        {
            ValidationStatus = CodeValidationStatus.Passed,
            ExecutionStatus = CodeExecutionStatus.Success,
            Output = output,
            Errors = Array.Empty<string>(),
            ExecutionTime = executionTime,
            AiRecommendations = recommendations,
            AiApproved = true
        });
    }

    private static CodeQualityResult CreateFailedResult(
        CodeValidationStatus validationStatus,
        CodeExecutionStatus executionStatus,
        TimeSpan executionTime,
        params string[] errors)
    {
        return CodeQualityResult.Success(new CodeQuality
        {
            ValidationStatus = validationStatus,
            ExecutionStatus = executionStatus,
            Output = string.Empty,
            Errors = errors,
            ExecutionTime = executionTime,
            AiRecommendations = Array.Empty<string>(),
            AiApproved = false
        });
    }

    private sealed record AiReviewResult
    {
        [JsonPropertyName("approved")]
        public bool Approved { get; init; }
        
        [JsonPropertyName("issues")]
        public string[] Issues { get; init; } = Array.Empty<string>();
        
        [JsonPropertyName("warnings")]
        public string[] Warnings { get; init; } = Array.Empty<string>();
        
        [JsonPropertyName("recommendations")]
        public string[] Recommendations { get; init; } = Array.Empty<string>();
    }
}