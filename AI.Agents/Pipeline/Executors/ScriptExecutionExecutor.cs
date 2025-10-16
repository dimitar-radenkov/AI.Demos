using AI.Agents.CodeGeneration;
using AI.Services.CodeExecution;
using AI.Services.CodeExecution.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class ScriptExecutionExecutor : ReflectingExecutor<ScriptExecutionExecutor>,
    IMessageHandler<CodeArtifact, ExecutionResult>
{
    private readonly IScriptRunner scriptRunner;

    public ScriptExecutionExecutor(IScriptRunner scriptRunner)
        : base(nameof(ScriptExecutionExecutor))
    {
        this.scriptRunner = scriptRunner;
    }

    public async ValueTask<ExecutionResult> HandleAsync(
        CodeArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var executionResult = await this.scriptRunner.ExecuteAsync(
            message.Code,
            cancellationToken);

        return executionResult;
    }
}
