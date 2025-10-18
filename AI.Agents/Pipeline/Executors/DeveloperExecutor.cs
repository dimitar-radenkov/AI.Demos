using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using AI.Services.CodeExecution.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text;

namespace AI.Agents.Pipeline.Executors;

public sealed class DeveloperExecutor : ReflectingExecutor<DeveloperExecutor>,
    IMessageHandler<Requirements, CodeArtifact>,
    IMessageHandler<ExecutionResult, CodeArtifact>
{
    private readonly IAgent<CodeArtifactResult> developerAgent;

    public DeveloperExecutor(IAgent<CodeArtifactResult> developerAgent)
        : base(nameof(DeveloperExecutor))
    {
        this.developerAgent = developerAgent;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        Requirements message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var developerPrompt = BuildCodeGenerationPrompt(message);
        //store original developer prompts for auditing
        await context.QueueStateUpdateAsync("developerPrompt", developerPrompt, cancellationToken);

        var result = await this.developerAgent.ExecuteAsync(
            developerPrompt, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        return result.Data;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        ExecutionResult message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var originalPrompt = await context.ReadStateAsync<string>("developerPrompt", cancellationToken);
        var feedbackPrompt = new StringBuilder();
        feedbackPrompt.AppendLine("The following code was executed with these results:");
        feedbackPrompt.AppendLine();
        feedbackPrompt.AppendLine($"Execution Output: {message.ErrorMessage}");
        feedbackPrompt.AppendLine();

        feedbackPrompt.AppendLine("Based on the execution results, please improve the original code.");
        feedbackPrompt.AppendLine();

        feedbackPrompt.AppendLine("Original Developer Prompt:");
        feedbackPrompt.AppendLine(originalPrompt);
        var result = await this.developerAgent.ExecuteAsync(
            feedbackPrompt.ToString(),
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        return result.Data;
    }

    private static string BuildCodeGenerationPrompt(Requirements requirements)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("Generate C# code that implements the following requirements:");
        prompt.AppendLine();
        prompt.AppendLine($"Task: {requirements.Task}");

        if (requirements.Inputs.Length > 0)
        {
            prompt.AppendLine($"Inputs: {string.Join(", ", requirements.Inputs)}");
        }

        if (requirements.Outputs.Length > 0)
        {
            prompt.AppendLine($"Outputs: {string.Join(", ", requirements.Outputs)}");
        }

        if (requirements.Constraints.Length > 0)
        {
            prompt.AppendLine($"Constraints: {string.Join(", ", requirements.Constraints)}");
        }

        prompt.AppendLine();
        prompt.AppendLine("Provide only the C# code implementation, properly formatted and with necessary using statements.");

        return prompt.ToString();
    }
}