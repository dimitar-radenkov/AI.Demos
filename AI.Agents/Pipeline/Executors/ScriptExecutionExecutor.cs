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
        this.logger.LogInformation("Starting script execution");
        this.logger.LogInformation("  Input: Executing {CodeLength} characters of approved code", message.Code.Length);

        var stopwatch = Stopwatch.StartNew();
        var executionResult = await this.scriptRunner.ExecuteAsync(
            message.Code,
            cancellationToken);
        stopwatch.Stop();

        if (executionResult.IsSuccess)
        {
            this.logger.LogInformation("Script execution completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
            this.logger.LogInformation("  Output:");
            this.logger.LogInformation("    Status: SUCCESS");
            this.logger.LogInformation("    Result: {Result}", executionResult.Data);
        }
        else
        {
            this.logger.LogError("Script execution failed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
            this.logger.LogError("  Output:");
            this.logger.LogError("    Status: FAILED");
            this.logger.LogError("    Error: {ErrorMessage}", executionResult.ErrorMessage);
        }

        return new ExecutionArtifact
        {
            Code = message.Code,
            Result = executionResult
        };
    }
}
