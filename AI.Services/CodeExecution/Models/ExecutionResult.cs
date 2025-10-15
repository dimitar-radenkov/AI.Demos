using AI.Core.Infrastructure;

namespace AI.Services.CodeExecution.Models;

public sealed record ValidationResult : OperationResult<ValidationResult>;

public sealed record ExecutionResult : OperationResult<ExecutionDto, ExecutionResult>;

public sealed record ExecutionDto
{
    public object? ReturnValue { get; init; }
    public required TimeSpan ExecutionTime { get; init; }
}
