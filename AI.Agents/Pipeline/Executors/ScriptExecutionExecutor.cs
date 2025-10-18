using AI.Agents.Pipeline.Models;
using AI.Services.CodeExecution;
using AI.Services.CodeExecution.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AI.Agents.Pipeline.Executors;

public sealed class ScriptExecutionExecutor : ReflectingExecutor<ScriptExecutionExecutor>,
    IMessageHandler<ReviewerDecision, ExecutionResult>
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

    public async ValueTask<ExecutionResult> HandleAsync(
        ReviewerDecision message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Executing C# script ({CodeLength} characters)", message.ExecutableCode.Length);

        var stopwatch = Stopwatch.StartNew();
        var executionResult = await this.scriptRunner.ExecuteAsync(
            message.ExecutableCode,
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

            await context.SendMessageAsync(
                new CodeExecutionErrorMessage { Errors  = executionResult.ErrorMessage!, ExecutableCode  = message.ExecutableCode },
                cancellationToken);
        }

        return executionResult;
    }
}
