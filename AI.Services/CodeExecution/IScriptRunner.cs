using AI.Services.CodeExecution.Models;

namespace AI.Services.CodeExecution;

/// <summary>
/// Service for dynamic C# code compilation and execution using Roslyn scripting.
/// WARNING: Code executes with full application permissions. Use only with trusted code.
/// </summary>
public interface IScriptRunner
{
    /// <summary>
    /// Executes C# code and returns the result.
    /// </summary>
    /// <param name="code">The C# code to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The execution result including return value and execution time.</returns>
    Task<ExecutionResult> ExecuteAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates C# code without executing it.
    /// </summary>
    /// <param name="code">The C# code to validate.</param>
    /// <returns>The validation result indicating compilation errors, if any.</returns>
    Task<ValidationResult> ValidateAsync(string code);
}
