using AI.Blazor.Client.Agents.CodeExecution.Models;

namespace AI.Blazor.Client.Agents.CodeExecution;

/// <summary>
/// Agent for dynamic C# code compilation and execution using Roslyn.
///  WARNING: Code executes with full application permissions. Use only with trusted code.
/// </summary>
public interface ICodeExecutionAgent
{
    Task<ExecutionResult> ExecuteCode(string code, CancellationToken cancellationToken = default);

    Task<ValidationResult> ValidateCode(string code);
}