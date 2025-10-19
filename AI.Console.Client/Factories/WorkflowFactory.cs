using AI.Agents.Pipeline.Executors;
using AI.Agents.Pipeline.Models;
using Microsoft.Agents.AI.Workflows;

namespace AI.Console.Client.Factories;

public sealed class WorkflowFactory : IWorkflowFactory
{
    private readonly AnalystExecutor analystExecutor;
    private readonly DeveloperExecutor developerExecutor;
    private readonly ReviewerExecutor reviewerExecutor;
    private readonly ScriptExecutionExecutor scriptExecutor;
    private readonly PresenterExecutor presenterExecutor;

    public WorkflowFactory(
        AnalystExecutor analystExecutor,
        DeveloperExecutor developerExecutor,
        ReviewerExecutor reviewerExecutor,
        ScriptExecutionExecutor scriptExecutor,
        PresenterExecutor presenterExecutor)
    {
        this.analystExecutor = analystExecutor;
        this.developerExecutor = developerExecutor;
        this.reviewerExecutor = reviewerExecutor;
        this.scriptExecutor = scriptExecutor;
        this.presenterExecutor = presenterExecutor;
    }

    public Workflow Create()
    {
        var builder = new WorkflowBuilder(this.analystExecutor);

        builder.AddEdge(this.analystExecutor, this.developerExecutor);
        builder.AddEdge(this.developerExecutor, this.reviewerExecutor);

        builder.AddSwitch(this.reviewerExecutor, sb =>
        {
            sb.AddCase<ReviewArtifact>(reviewArtifact => reviewArtifact!.IsApproved, this.scriptExecutor);
            sb.AddCase<ReviewArtifact>(reviewArtifact => reviewArtifact!.IsApproved == false, this.developerExecutor);
        });

        builder.AddSwitch(this.scriptExecutor, sb =>
        {
            sb.AddCase<ExecutionArtifact>(executionArtifact => executionArtifact!.Result.IsSuccess, this.presenterExecutor);
            sb.AddCase<ExecutionArtifact>(executionArtifact => executionArtifact!.Result.IsSuccess == false, this.developerExecutor);
        });

        return builder.Build();
    }
}
