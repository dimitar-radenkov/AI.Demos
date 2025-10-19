using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Models;
using AI.Agents.QualityAssurance;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AI.Agents.Pipeline.Executors;

public sealed class ReviewerExecutor : ReflectingExecutor<ReviewerExecutor>,
    IMessageHandler<CodeArtifact, ReviewArtifact>
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

    public async ValueTask<ReviewArtifact> HandleAsync(
        CodeArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Starting code review");
        this.logger.LogInformation("  Input: Reviewing {Code} code", message.Code);

        var reviewPrompt = message.ToReviewPrompt();

        var stopwatch = Stopwatch.StartNew();
        var agentResult = await this.codeReviewerAgent.ExecuteAsync(
            reviewPrompt,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!agentResult.IsSuccess)
        {
            this.logger.LogError("Failed to get code review from reviewer agent");
            throw new InvalidOperationException("Failed to get code review from reviewer agent.");
        }

        this.logger.LogInformation("Code review completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output:");
        this.logger.LogInformation("    Decision: {Decision}", agentResult.Data!.IsApproved ? "APPROVED" : "DISAPPROVED");
        this.logger.LogInformation("    Comments: {Comments}", agentResult.Data.Comments);

        return new ReviewArtifact
        {
            Code = message.Code,
            IsApproved = agentResult.Data!.IsApproved,
            Feedback = agentResult.Data.Comments
        };
    }
}

public static class ReviewerExecutorExtensions
{
    public static string ToReviewPrompt(this CodeArtifact codeArtifact)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("Please review the following C# Roslyn script:");
        prompt.AppendLine();
        prompt.AppendLine("Original Requirements:");
        prompt.AppendLine(codeArtifact.Requirements);
        prompt.AppendLine();
        prompt.AppendLine("Generated Code:");
        prompt.AppendLine("```csharp");
        prompt.AppendLine(codeArtifact.Code);
        prompt.AppendLine("```");
        prompt.AppendLine();
        prompt.AppendLine("Please review this code and determine if it:");
        prompt.AppendLine("1. Correctly implements the requirements");
        prompt.AppendLine("2. Is a valid Roslyn script (no 'return', no classes, no Main method)");
        prompt.AppendLine("3. Uses correct C# operator precedence");
        prompt.AppendLine("4. Is appropriately simple and clean");

        return prompt.ToString();
    }
}