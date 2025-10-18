using Microsoft.Agents.AI.Workflows;

namespace AI.Console.Client.Factories;

public interface IWorkflowFactory
{
    Workflow Create();
}
