using AI.Agents.Pipeline.Models;
using AI.Services.CodeExecution;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AI.Agents.Pipeline.Executors;

public sealed class ScriptExecutionExecutor : ReflectingExecutor<ScriptExecutionExecutor>,
    IMessageHandler<ReviewArtifact, ExecutionArtifact>
{
    private readonly IScriptRunner scriptRunner;
    private readonly ILogger<ScriptExecutionExecutor> logger;

    public ScriptExecutionExecutor(
        IScriptRunner scriptRunner,
        ILogger<ScriptExecutionExecutor> logger)
        : base(nameof(ScriptExecutionExecutor))
    {
        this.scriptRunner = scriptRunner;
        this.logger = logger;
    }

    public async ValueTask<ExecutionArtifact> HandleAsync(
        ReviewArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing C# script ('''{Code}''')", message.Code);

        var stopwatch = Stopwatch.StartNew();
        var executionResult = await this.scriptRunner.ExecuteAsync(
            message.Code,
            cancellationToken);
        stopwatch.Stop();

        if (executionResult.IsSuccess)
        {
            logger.LogInformation("Execution completed in {ElapsedSeconds:F1}s - Result: {Result}",
                stopwatch.Elapsed.TotalSeconds, executionResult.Data);
        }
        else
        {
            logger.LogError("Execution failed in {ElapsedSeconds:F1}s - Error: {ErrorMessage}",
                stopwatch.Elapsed.TotalSeconds, executionResult.ErrorMessage);
        }

        return new ExecutionArtifact
        {
            Code = message.Code,
            Result = executionResult
        };
    }
}
