using AI.Shared.Infrastructure;

namespace AI.Agents.Models;

/// <summary>
/// Represents a piece of generated code along with its metadata.
/// Contains the actual code, programming language, generation timestamp, and the requirements it was based on.
/// </summary>
public sealed class CodeArtifact
{
    /// <summary>
    /// Gets the generated code as a string.
    /// This is the actual source code produced by the Developer agent.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Gets the programming language of the generated code.
    /// Examples: "csharp", "python", "javascript", etc.
    /// </summary>
    public required string Language { get; init; }

    /// <summary>
    /// Gets the timestamp when this code was generated.
    /// Used for tracking and versioning purposes.
    /// </summary>
    public required DateTime GeneratedAt { get; init; }

    /// <summary>
    /// Gets the requirements that were used to generate this code.
    /// Links back to the original user request analysis.
    /// </summary>
    public required Requirements Requirements { get; init; }
}

/// <summary>
/// Represents the result of a code generation operation.
/// Contains either the successfully generated code artifact or an error message.
/// </summary>
public sealed record CodeArtifactResult : OperationResult<CodeArtifact, CodeArtifactResult>
{
}