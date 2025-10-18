using AI.Agents.Presentation;
using AI.Services.CodeExecution.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using System.Text;

namespace AI.Agents.Pipeline.Executors;

public sealed class PresenterExecutor : ReflectingExecutor<PresenterExecutor>,
    IMessageHandler<ExecutionResult, AI.Agents.Presentation.Presentation>
{
    private readonly IAgent<PresentationResult> codePresenterAgent;

    public PresenterExecutor(IAgent<PresentationResult> codePresenterAgent)
        : base(nameof(PresenterExecutor))
    {
        this.codePresenterAgent = codePresenterAgent;
    }

    public async ValueTask<AI.Agents.Presentation.Presentation> HandleAsync(
        ExecutionResult executionResult, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var prompt = await this.GeneratePrompt(
            executionResult, 
            context, 
            cancellationToken);

        var result = await this.codePresenterAgent.ExecuteAsync(
            prompt, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get presentation from presenter agent.");
        }

        return result.Data;
    }

    private async Task<string> GeneratePrompt(
        ExecutionResult executionResult,
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
        prompt.AppendLine($"Output: {executionResult.Data}");
        prompt.AppendLine($"Errors: {executionResult.ErrorMessage}");
        prompt.AppendLine();
        prompt.AppendLine("Please provide a clear and concise summary suitable for end-users.");
        return prompt.ToString();
    }
}