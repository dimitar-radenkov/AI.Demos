using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Agents.Pipeline.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AI.Agents.Pipeline.Executors;

public sealed class DeveloperExecutor : ReflectingExecutor<DeveloperExecutor>,
    IMessageHandler<Requirements, CodeArtifact>,
    IMessageHandler<CodeDisapprovedMessage, CodeArtifact>,
    IMessageHandler<CodeExecutionErrorMessage, CodeArtifact>
{
    private readonly IAgent<CodeArtifactResult> developerAgent;
    private readonly ILogger<DeveloperExecutor> logger;

    public DeveloperExecutor(
        IAgent<CodeArtifactResult> developerAgent,
        ILogger<DeveloperExecutor> logger)
        : base(nameof(DeveloperExecutor))
    {
        this.developerAgent = developerAgent;
        this.logger = logger;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        Requirements message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Generating code for task: {Task}", message.Task);

        var developerPrompt = message.FromRequirements();
        await context.QueueStateUpdateAsync("developerPrompt", developerPrompt, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await this.developerAgent.ExecuteAsync(
            developerPrompt,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        logger.LogInformation("Code generated in {ElapsedSeconds:F1}s ({CodeLength} characters)",
            stopwatch.Elapsed.TotalSeconds, result.Data.Code.Length);

        return result.Data;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        CodeDisapprovedMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Regenerating code based on disapproval feedback");

        var prompt = message.FromDisapprovedMessage(context);
        var result = await this.developerAgent.ExecuteAsync(
            prompt.Result,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        return result.Data!;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        CodeExecutionErrorMessage message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogInformation("Regenerating code based on execution error feedback");
        var prompt = message.FromCodeExecutionErrorMessage(context);
        var result = await this.developerAgent.ExecuteAsync(
            prompt.Result,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        return result.Data!;
    }
}

public static class DeveloperExecutorExtensions
{
    public static string FromRequirements(this Requirements requirements)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Task: {requirements.Task}");
        if (requirements.Inputs.Length > 0)
        {
            builder.AppendLine($"Inputs: {string.Join(", ", requirements.Inputs)}");
        }

        if (requirements.Outputs.Length > 0)
        {
            builder.AppendLine($"Outputs: {string.Join(", ", requirements.Outputs)}");
        }

        if (requirements.Constraints.Length > 0)
        {
            builder.AppendLine($"Constraints: {string.Join(", ", requirements.Constraints)}");
        }

        return builder.ToString();
    }

    public static async Task<string> FromDisapprovedMessage(this CodeDisapprovedMessage message, IWorkflowContext context)
    {
        var originalPrompt = await context.ReadStateAsync<string>("developerPrompt");
        var feedbackPrompt = new StringBuilder();
        feedbackPrompt.AppendLine("The following code was executed with these results:");
        feedbackPrompt.AppendLine();
        feedbackPrompt.AppendLine($"Execution Output: {message.Comments}");
        feedbackPrompt.AppendLine();

        feedbackPrompt.AppendLine("Based on the execution results, please improve the original code.");
        feedbackPrompt.AppendLine();

        feedbackPrompt.AppendLine("Original Developer Prompt:");
        feedbackPrompt.AppendLine(originalPrompt);

        return feedbackPrompt.ToString();
    }

    public static async Task<string> FromCodeExecutionErrorMessage(this CodeExecutionErrorMessage message, IWorkflowContext context)
    {
        var originalPrompt = await context.ReadStateAsync<string>("developerPrompt");
        var feedbackPrompt = new StringBuilder();
        feedbackPrompt.AppendLine("The following code was executed with these errors:");
        feedbackPrompt.AppendLine();
        feedbackPrompt.AppendLine($"Error Message: {message.Errors}");
        feedbackPrompt.AppendLine();
        feedbackPrompt.AppendLine("Based on the error message, please improve the original code.");
        feedbackPrompt.AppendLine();
        feedbackPrompt.AppendLine("Original Developer Prompt:");
        feedbackPrompt.AppendLine(originalPrompt);
        return feedbackPrompt.ToString();
    }
}