using AI.Shared.Infrastructure;

namespace AI.Agents.Analysis;

public sealed class Requirements
{
    public required string Task { get; init; }
    public required string[] Inputs { get; init; }
    public required string[] Outputs { get; init; }
    public required string[] Constraints { get; init; }
}

public sealed record RequirementsResult : OperationResult<Requirements, RequirementsResult>;