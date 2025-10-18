using AI.Agents.Analysis;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class AnalystExecutor : 
    ReflectingExecutor<AnalystExecutor>,
    IMessageHandler<string, Requirements>
{
    private readonly IAgent<RequirementsResult> queryAnalystAgent;

    public AnalystExecutor(IAgent<RequirementsResult> queryAnalystAgent)
        : base(nameof(AnalystExecutor))
    {
        this.queryAnalystAgent = queryAnalystAgent;
    }

    public async ValueTask<Requirements> HandleAsync(
        string message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        await context.QueueStateUpdateAsync("OriginalQuery", message, cancellationToken);

        var result = await this.queryAnalystAgent.ExecuteAsync(
            message, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get requirements from analyst agent.");
        }

        return result.Data;
    }
}