using AI.Agents.CodeGeneration;
using AI.Agents.QualityAssurance;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class ReviewerExecutor : ReflectingExecutor<ReviewerExecutor>,
    IMessageHandler<CodeArtifact, CodeReview>
{
    private readonly IAgent<CodeReviewResult> codeReviewerAgent;

    public ReviewerExecutor(IAgent<CodeReviewResult> codeReviewerAgent)
        : base(nameof(ReviewerExecutor))
    {
        this.codeReviewerAgent = codeReviewerAgent;
    }

    public async ValueTask<CodeReview> HandleAsync(
        CodeArtifact message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var jsonInput = System.Text.Json.JsonSerializer.Serialize(message);
        var result = await this.codeReviewerAgent.ExecuteAsync(
            jsonInput, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get code review from reviewer agent.");
        }

        return result.Data;
    }
}