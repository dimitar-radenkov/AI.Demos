using AI.Shared.Infrastructure;

namespace AI.Agents.Models;

/// <summary>
/// Represents the analyzed requirements extracted from a user request.
/// Contains the core task, inputs, outputs, and constraints for code generation.
/// </summary>
public sealed class Requirements
{
    /// <summary>
    /// Gets the main task or objective that needs to be accomplished.
    /// This is the primary goal extracted from the user's request.
    /// </summary>
    public required string Task { get; init; }

    /// <summary>
    /// Gets the input parameters or data that the code should accept.
    /// These define what information the generated code will work with.
    /// </summary>
    public required string[] Inputs { get; init; }

    /// <summary>
    /// Gets the expected outputs or results that the code should produce.
    /// These define what the generated code should return or accomplish.
    /// </summary>
    public required string[] Outputs { get; init; }

    /// <summary>
    /// Gets any constraints or limitations that must be considered.
    /// These include performance requirements, security constraints, or other restrictions.
    /// </summary>
    public required string[] Constraints { get; init; }
}

/// <summary>
/// Represents the result of a requirements analysis operation.
/// Contains either the successfully analyzed requirements or an error message.
/// </summary>
public sealed record RequirementsResult : OperationResult<Requirements, RequirementsResult>
{
}