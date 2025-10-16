using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class ReviewerExecutor : ReflectingExecutor<ReviewerExecutor>,
    IMessageHandler<CodeArtifact, CodeReview>
{
    private readonly IAgent<CodeArtifact, CodeReviewResult> codeReviewerAgent;

    public ReviewerExecutor(IAgent<CodeArtifact, CodeReviewResult> codeReviewerAgent)
        : base(nameof(ReviewerExecutor))
    {
        this.codeReviewerAgent = codeReviewerAgent;
    }

    public async ValueTask<CodeReview> HandleAsync(
        CodeArtifact message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await this.codeReviewerAgent.ExecuteAsync(
            message, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get code review from reviewer agent.");
        }

        return result.Data;
    }
}