using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using AI.Agents.CodeGeneration;
using AI.Shared.Services.CodeExecution;
using AI.Shared.Settings.Agents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

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

        var openAIClient = new OpenAIClient(
            new ApiKeyCredential(qaSettings.ApiKey),
            new OpenAIClientOptions
            {
                Endpoint = new Uri($"{qaSettings.BaseUrl}/v1")
            });

        this.agent = openAIClient
            .GetChatClient(qaSettings.Model)
            .CreateAIAgent(instructions: qaSettings.GetSystemPrompt());

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

            // Step 1: AI Code Review
            var reviewPrompt = $"Analyze the following C# code:\n\n```csharp\n{artifact.Code}\n```";
            var response = await this.agent.RunAsync(reviewPrompt, this.agentThread, cancellationToken: cancellationToken);

            this.logger.LogDebug("AI Review Response: {Response}", response.Text);

            var reviewResult = ParseAiReview(response.Text);
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

    private static AiReviewResult ParseAiReview(string aiResponse)
    {
        try
        {
            // Extract JSON from markdown code blocks if present
            var jsonContent = aiResponse;
            if (aiResponse.Contains("```json"))
            {
                var startIndex = aiResponse.IndexOf("```json") + 7;
                var endIndex = aiResponse.IndexOf("```", startIndex);
                if (endIndex > startIndex)
                {
                    jsonContent = aiResponse.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }
            else if (aiResponse.Contains("```"))
            {
                var startIndex = aiResponse.IndexOf("```") + 3;
                var endIndex = aiResponse.IndexOf("```", startIndex);
                if (endIndex > startIndex)
                {
                    jsonContent = aiResponse.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }

            var review = JsonSerializer.Deserialize<AiReviewResult>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return review ?? new AiReviewResult { Approved = false, Issues = ["Failed to parse AI review response"] };
        }
        catch
        {
            return new AiReviewResult { Approved = false, Issues = ["Invalid AI review response format"] };
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
        public bool Approved { get; init; }
        public string[] Issues { get; init; } = Array.Empty<string>();
        public string[] Warnings { get; init; } = Array.Empty<string>();
        public string[] Recommendations { get; init; } = Array.Empty<string>();
    }
}