using AI.Shared.Infrastructure;

namespace AI.Agents.Models;

/// <summary>
/// Represents natural language explanations of code generation results.
/// Provides user-friendly explanations of what was generated, how it works, and what the results mean.
/// </summary>
public sealed class Explanation
{
    /// <summary>
    /// Gets a brief summary of the entire code generation and execution process.
    /// This is a high-level overview suitable for quick understanding.
    /// </summary>
    public required string Summary { get; init; }

    /// <summary>
    /// Gets detailed explanations of various aspects of the process.
    /// This may include step-by-step breakdowns, technical details, or additional context.
    /// </summary>
    public required string Details { get; init; }

    /// <summary>
    /// Gets an explanation of the generated code itself.
    /// Describes what the code does, how it works, and key implementation details.
    /// </summary>
    public required string CodeExplanation { get; init; }

    /// <summary>
    /// Gets an explanation of the execution results.
    /// Describes what happened when the code was run, what the output means, and any notable outcomes.
    /// </summary>
    public required string ResultExplanation { get; init; }
}

/// <summary>
/// Represents the result of generating explanations for the code generation workflow.
/// Contains either the explanation object or an error message if explanation generation failed.
/// </summary>
public sealed record ExplanationResult : OperationResult<Explanation, ExplanationResult>
{
}