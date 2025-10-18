using AI.Agents.Pipeline.Executors;
using AI.Agents.Pipeline.Models;
using AI.Agents.QualityAssurance;
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
        var builder = new WorkflowBuilder(analystExecutor);

        builder.AddEdge(analystExecutor, developerExecutor);
        builder.AddEdge(developerExecutor, reviewerExecutor);

        builder.AddSwitch(reviewerExecutor, sb =>
        {
            sb.AddCase<ReviewArtifact>(d => d.IsApproved, scriptExecutor);
            sb.AddCase<ReviewArtifact>(d => !d.IsApproved, developerExecutor);
        });

        builder.AddSwitch(scriptExecutor, sb =>
        {
            sb.AddCase<ExecutionArtifact>(a => a.Result.IsSuccess, presenterExecutor);
            sb.AddCase<ExecutionArtifact>(a => !a.Result.IsSuccess, developerExecutor);
        });

        return builder.Build();
    }
}
