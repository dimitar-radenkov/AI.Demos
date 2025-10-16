using AI.Agents.Presentation;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class PresenterExecutor : ReflectingExecutor<PresenterExecutor>,
    IMessageHandler<PresentationInput, AI.Agents.Presentation.Presentation>
{
    private readonly IAgent<PresentationInput, PresentationResult> codePresenterAgent;

    public PresenterExecutor(IAgent<PresentationInput, PresentationResult> codePresenterAgent)
        : base(nameof(PresenterExecutor))
    {
        this.codePresenterAgent = codePresenterAgent;
    }

    public async ValueTask<AI.Agents.Presentation.Presentation> HandleAsync(
        PresentationInput message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await this.codePresenterAgent.ExecuteAsync(
            message, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get presentation from presenter agent.");
        }

        return result.Data;
    }
}