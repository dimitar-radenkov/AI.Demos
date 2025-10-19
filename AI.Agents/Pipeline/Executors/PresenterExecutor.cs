using AI.Agents.Pipeline.Models;
using AI.Agents.Presentation;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AI.Agents.Pipeline.Executors;

public sealed class PresenterExecutor : ReflectingExecutor<PresenterExecutor>,
    IMessageHandler<ExecutionArtifact, AI.Agents.Presentation.Presentation>
{
    private readonly IAgent<PresentationResult> codePresenterAgent;
    private readonly ILogger<PresenterExecutor> logger;

    public PresenterExecutor(
        IAgent<PresentationResult> codePresenterAgent,
        ILogger<PresenterExecutor> logger)
        : base(nameof(PresenterExecutor))
    {
        this.codePresenterAgent = codePresenterAgent;
        this.logger = logger;
    }

    public async ValueTask<AI.Agents.Presentation.Presentation> HandleAsync(
        ExecutionArtifact executionArtifact,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Starting presentation formatting");
        this.logger.LogInformation("  Input: Execution result=\"{Result}\"", executionArtifact.Result.Data);

        var prompt = await executionArtifact.FromExecutionResult(context, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await this.codePresenterAgent.ExecuteAsync(
            prompt,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            this.logger.LogError("Failed to get presentation from presenter agent");
            throw new InvalidOperationException("Failed to get presentation from presenter agent.");
        }

        this.logger.LogInformation("Presentation formatting completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output:");
        this.logger.LogInformation("    Summary: {Summary}", result.Data.Summary);
        this.logger.LogInformation("    Result: {Result}", result.Data.FormattedResult);

        return result.Data;
    }
}

public static class ExecutionArtifactExtensions
{
    public static async Task<string> FromExecutionResult(
        this ExecutionArtifact artifact,
        IWorkflowContext context,
        CancellationToken cancellationToken)
    {
        var originalQueryTask = await context.ReadStateAsync<string>("OriginalQuery", cancellationToken);
        var prompt = new StringBuilder();

        prompt.AppendLine("Create a user-friendly presentation based on the following code execution results:");
        prompt.AppendLine();
        prompt.AppendLine($"Original User Query: {originalQueryTask}");
        prompt.AppendLine();
        prompt.AppendLine("Code Execution Results:");
        prompt.AppendLine($"Output: {artifact.Result.Data}");
        prompt.AppendLine($"Errors: {artifact.Result.ErrorMessage}");
        prompt.AppendLine();
        prompt.AppendLine("Please provide a clear and concise summary suitable for end-users.");

        return prompt.ToString();
    }
}