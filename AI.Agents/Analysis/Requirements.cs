using AI.Core.Infrastructure;

namespace AI.Agents.Analysis;

public sealed record Requirements
{
    public required string Task { get; init; }
    public required string[] Inputs { get; init; }
    public required string[] Outputs { get; init; }
    public required string[] Constraints { get; init; }

    public override string ToString()
    {
        return $"""
            Task: {this.Task}
            Inputs: {string.Join(", ", this.Inputs)}
            Outputs: {string.Join(", ", this.Outputs)}
            Constraints: {string.Join(", ", this.Constraints)}
            """;
    }
}

public sealed record RequirementsResult : OperationResult<Requirements, RequirementsResult>;