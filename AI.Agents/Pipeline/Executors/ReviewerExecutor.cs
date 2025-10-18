using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Models;
using AI.Agents.QualityAssurance;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AI.Agents.Pipeline.Executors;

public sealed class ReviewerExecutor : ReflectingExecutor<ReviewerExecutor>,
    IMessageHandler<CodeArtifact, ReviewerDecision>
{
    private readonly IAgent<CodeReviewResult> codeReviewerAgent;
    private readonly ILogger<ReviewerExecutor> logger;

    public ReviewerExecutor(
        IAgent<CodeReviewResult> codeReviewerAgent,
        ILogger<ReviewerExecutor> logger)
        : base(nameof(ReviewerExecutor))
    {
        this.codeReviewerAgent = codeReviewerAgent;
        this.logger = logger;
    }

    public async ValueTask<ReviewerDecision> HandleAsync(
        CodeArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting code review");

        var jsonInput = System.Text.Json.JsonSerializer.Serialize(message);

        var stopwatch = Stopwatch.StartNew();
        var agentResult = await this.codeReviewerAgent.ExecuteAsync(
            jsonInput,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!agentResult.IsSuccess || agentResult.Data is null)
        {
            logger.LogError("Failed to get code review from reviewer agent");
            throw new InvalidOperationException("Failed to get code review from reviewer agent.");
        }

        if (!agentResult.Data.IsApproved)
        {
            await context.SendMessageAsync(
                new CodeDisapprovedMessage { ExecutableCode = message.Code, Comments = agentResult.Data.Comments },
                cancellationToken);
        }

        var result = new ReviewerDecision
        {
            CodeReview = agentResult.Data,
            ExecutableCode = message.Code
        };

        return result;
    }
}