using AI.Agents.Pipeline.Executors;
using AI.Agents.Pipeline.Models;
using AI.Services.CodeExecution.Models;
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
        builder.AddEdge(reviewerExecutor, scriptExecutor);
        builder.AddEdge(scriptExecutor, presenterExecutor);

        //builder.AddSwitch(reviewerExecutor, sb =>
        //{
        //    sb.AddCase<ReviewerDecision>(d => d.CodeReview.IsApproved, scriptExecutor);
        //});

        //builder.AddSwitch(scriptExecutor, sb =>
        //{
        //    sb.AddCase<ExecutionResult>(r => r.IsSuccess, presenterExecutor);
        //});

        return builder.Build();
    }
}
