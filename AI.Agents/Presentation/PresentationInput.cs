using AI.Services.CodeExecution.Models;

namespace AI.Agents.Presentation;

public sealed class PresentationInput
{
    public required string UserRequest { get; init; }

    public required ExecutionDto ExecutionResult { get; init; }
}
