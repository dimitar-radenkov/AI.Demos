using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Models;
using AI.Agents.QualityAssurance;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class ReviewerExecutor : ReflectingExecutor<ReviewerExecutor>,
    IMessageHandler<CodeArtifact, ReviewerDecision>
{
    private readonly IAgent<CodeReviewResult> codeReviewerAgent;

    public ReviewerExecutor(IAgent<CodeReviewResult> codeReviewerAgent)
        : base(nameof(ReviewerExecutor))
    {
        this.codeReviewerAgent = codeReviewerAgent;
    }

    public async ValueTask<ReviewerDecision> HandleAsync(
        CodeArtifact message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var jsonInput = System.Text.Json.JsonSerializer.Serialize(message);
        var agentResult = await this.codeReviewerAgent.ExecuteAsync(
            jsonInput, 
            cancellationToken: cancellationToken);

        if (!agentResult.IsSuccess || agentResult.Data is null)
        {
            throw new InvalidOperationException("Failed to get code review from reviewer agent.");
        }

        var result = new ReviewerDecision
        {
            CodeReview = agentResult.Data,
            ExecutableCode = message.Code
        };

        return result;
    }
}