using AI.Shared.Infrastructure;

namespace AI.Agents.Models;

/// <summary>
/// Represents the complete response from the code assistant workflow.
/// Contains all artifacts and results from the end-to-end code generation and validation process.
/// </summary>
public sealed class WorkflowResponse
{
    /// <summary>
    /// Gets the requirements that were extracted from the user's request.
    /// This represents the analyzed understanding of what the user wants.
    /// </summary>
    public required Requirements Requirements { get; init; }

    /// <summary>
    /// Gets the code artifact that was generated based on the requirements.
    /// This contains the actual source code produced by the Developer agent.
    /// </summary>
    public required CodeArtifact Code { get; init; }

    /// <summary>
    /// Gets the results from validating and executing the generated code.
    /// This includes validation status, execution results, and any errors encountered.
    /// </summary>
    public required TestResult TestResult { get; init; }

    /// <summary>
    /// Gets the natural language explanations of the entire process and results.
    /// This provides user-friendly descriptions of what happened and what the results mean.
    /// </summary>
    public required Explanation Explanation { get; init; }

    /// <summary>
    /// Gets a value indicating whether the entire workflow completed successfully.
    /// True if all steps succeeded without critical errors, false otherwise.
    /// </summary>
    public required bool Success { get; init; }
}

/// <summary>
/// Represents the result of executing the complete code assistant workflow.
/// Contains either the workflow response with all generated artifacts or an error message if the workflow failed.
/// </summary>
public sealed record WorkflowResponseResult : OperationResult<WorkflowResponse, WorkflowResponseResult>
{
}