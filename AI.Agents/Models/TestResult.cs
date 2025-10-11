using AI.Shared.Infrastructure;

namespace AI.Agents.Models;

/// <summary>
/// Represents the result of validating and executing generated code.
/// Contains validation status, execution results, errors, and performance metrics.
/// </summary>
public sealed class TestResult
{
    /// <summary>
    /// Gets the validation status indicating whether the code passed static analysis.
    /// Examples: "Passed", "Failed", "Warning".
    /// </summary>
    public required string ValidationStatus { get; init; }

    /// <summary>
    /// Gets the execution status indicating the outcome of running the code.
    /// Examples: "Success", "Failed", "Timeout", "SecurityViolation".
    /// </summary>
    public required string ExecutionStatus { get; init; }

    /// <summary>
    /// Gets the output produced by executing the code.
    /// This contains the actual results or console output from the execution.
    /// </summary>
    public required string Output { get; init; }

    /// <summary>
    /// Gets any errors that occurred during validation or execution.
    /// This includes compilation errors, runtime exceptions, or security violations.
    /// </summary>
    public required string[] Errors { get; init; }

    /// <summary>
    /// Gets the time taken to execute the code.
    /// Measured in milliseconds or as a TimeSpan.
    /// </summary>
    public required TimeSpan ExecutionTime { get; init; }
}

/// <summary>
/// Represents the result of a QA validation and execution operation.
/// Contains either the test results or an error message if the operation failed.
/// </summary>
public sealed record TestResultResult : OperationResult<TestResult, TestResultResult>
{
}