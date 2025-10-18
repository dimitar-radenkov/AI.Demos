using AI.Agents.Analysis;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AI.Agents.Pipeline.Executors;

public sealed class AnalystExecutor :
    ReflectingExecutor<AnalystExecutor>,
    IMessageHandler<string, Requirements>
{
    private readonly IAgent<RequirementsResult> queryAnalystAgent;
    private readonly ILogger<AnalystExecutor> logger;

    public AnalystExecutor(
        IAgent<RequirementsResult> queryAnalystAgent,
        ILogger<AnalystExecutor> logger)
        : base(nameof(AnalystExecutor))
    {
        this.queryAnalystAgent = queryAnalystAgent;
        this.logger = logger;
    }

    public async ValueTask<Requirements> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting analysis of query: \"{Query}\"", message);
        await context.QueueStateUpdateAsync("OriginalQuery", message, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await this.queryAnalystAgent.ExecuteAsync(
            message,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            logger.LogError("Failed to get requirements from analyst agent");
            throw new InvalidOperationException("Failed to get requirements from analyst agent.");
        }

        logger.LogInformation("Analysis completed in {ElapsedSeconds:F1}s - Task: {Task}",
            stopwatch.Elapsed.TotalSeconds, result.Data.Task);

        return result.Data;
    }
}