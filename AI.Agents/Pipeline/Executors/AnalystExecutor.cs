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
        this.logger.LogInformation("Starting analysis");
        this.logger.LogInformation("  Input: \"{Query}\"", message);
        await context.QueueStateUpdateAsync("OriginalQuery", message, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await this.queryAnalystAgent.ExecuteAsync(
            message,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            this.logger.LogError("Failed to get requirements from analyst agent");
            throw new InvalidOperationException("Failed to get requirements from analyst agent.");
        }

        this.logger.LogInformation("Analysis completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output:");
        this.logger.LogInformation("    Task: {Task}", result.Data.Task);
        if (result.Data.Inputs.Length > 0)
        {
            this.logger.LogInformation("    Inputs: {Inputs}", string.Join(", ", result.Data.Inputs));
        }
        if (result.Data.Outputs.Length > 0)
        {
            this.logger.LogInformation("    Outputs: {Outputs}", string.Join(", ", result.Data.Outputs));
        }
        if (result.Data.Constraints.Length > 0)
        {
            this.logger.LogInformation("    Constraints: {Constraints}", string.Join(", ", result.Data.Constraints));
        }

        return result.Data;
    }
}