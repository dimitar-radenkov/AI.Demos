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
    IMessageHandler<ReviewArtifact, CodeArtifact>,
    IMessageHandler<ExecutionArtifact, CodeArtifact>
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
        this.logger.LogInformation("Starting code generation");
        this.logger.LogInformation("  Input: Task=\"{Task}\"", message.Task);

        var developerPrompt = message.FromRequirements();
        await context.QueueStateUpdateAsync("developerPrompt", developerPrompt, cancellationToken);

        var stopwatch = Stopwatch.StartNew();
        var result = await this.developerAgent.ExecuteAsync(
            developerPrompt,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            this.logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        this.logger.LogInformation("Code generation completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output: Generated {CodeLength} characters of code", result.Data.Code.Length);

        return result.Data;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        ReviewArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogWarning("FEEDBACK LOOP: Code review disapproved - regenerating code");
        this.logger.LogInformation("  Input: Reviewer feedback=\"{Feedback}\"", message.Feedback);

        var stopwatch = Stopwatch.StartNew();
        var prompt = message.FromDisapprovedMessage(context);
        var result = await this.developerAgent.ExecuteAsync(
            prompt.Result,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            this.logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        this.logger.LogInformation("Code regeneration completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output: Generated {CodeLength} characters of revised code", result.Data.Code.Length);

        return result.Data!;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        ExecutionArtifact message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogWarning("FEEDBACK LOOP: Code execution failed - regenerating code");
        this.logger.LogInformation("  Input: Execution error=\"{ErrorMessage}\"", message.Result.ErrorMessage ?? "Unknown error");

        var stopwatch = Stopwatch.StartNew();
        var prompt = message.FromCodeExecutionErrorMessage(context);
        var result = await this.developerAgent.ExecuteAsync(
            prompt.Result,
            cancellationToken: cancellationToken);
        stopwatch.Stop();

        if (!result.IsSuccess || result.Data is null)
        {
            this.logger.LogError("Failed to get code artifact from developer agent");
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        this.logger.LogInformation("Code regeneration completed in {ElapsedSeconds:F1}s", stopwatch.Elapsed.TotalSeconds);
        this.logger.LogInformation("  Output: Generated {CodeLength} characters of fixed code", result.Data.Code.Length);

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

    public static async Task<string> FromDisapprovedMessage(this ReviewArtifact message, IWorkflowContext context)
    {
        var originalPrompt = await context.ReadStateAsync<string>("developerPrompt");
        var feedbackPrompt = new StringBuilder();

        feedbackPrompt.AppendLine("The following code was reviewed and disapproved:");
        feedbackPrompt.AppendLine(message.Code);
        feedbackPrompt.AppendLine("Reviewer Feedback:");
        feedbackPrompt.AppendLine(message.Feedback);
        feedbackPrompt.AppendLine("Please revise the code to address the reviewer's feedback, considering the original requirements:");
        feedbackPrompt.AppendLine(originalPrompt);

        return feedbackPrompt.ToString();
    }

    public static async Task<string> FromCodeExecutionErrorMessage(this ExecutionArtifact message, IWorkflowContext context)
    {
        var originalPrompt = await context.ReadStateAsync<string>("developerPrompt");
        var feedbackPrompt = new StringBuilder();
        feedbackPrompt.AppendLine("The following code encountered an execution error:");
        feedbackPrompt.AppendLine(message.Code);
        feedbackPrompt.AppendLine("Error Message:");
        feedbackPrompt.AppendLine(message.Result.ErrorMessage ?? "No error message provided.");
        feedbackPrompt.AppendLine("Please revise the code to fix the error, considering the original requirements:");
        feedbackPrompt.AppendLine(originalPrompt);

        return feedbackPrompt.ToString();
    }
}