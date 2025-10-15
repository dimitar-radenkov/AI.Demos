using AI.Services.CodeExecution.Models;

namespace AI.Services.CodeExecution;

/// <summary>
/// Service for dynamic C# code compilation and execution using Roslyn.
///  WARNING: Code executes with full application permissions. Use only with trusted code.
/// </summary>
public interface ICodeExecutionService
{
    Task<ExecutionResult> ExecuteCode(string code, CancellationToken cancellationToken = default);

    Task<ValidationResult> ValidateCode(string code);
}
