using AI.Agents.Analysis;
using AI.Agents.CodeGeneration;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AI.Agents.Pipeline.Executors;

public sealed class DeveloperExecutor : ReflectingExecutor<DeveloperExecutor>,
    IMessageHandler<Requirements, CodeArtifact>
{
    private readonly IAgent<Requirements, CodeArtifactResult> developerAgent;

    public DeveloperExecutor(IAgent<Requirements, CodeArtifactResult> developerAgent)
        : base(nameof(DeveloperExecutor))
    {
        this.developerAgent = developerAgent;
    }

    public async ValueTask<CodeArtifact> HandleAsync(
        Requirements message, 
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var result = await this.developerAgent.ExecuteAsync(
            message, 
            cancellationToken: cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            throw new InvalidOperationException("Failed to get code artifact from developer agent.");
        }

        return result.Data;
    }
}