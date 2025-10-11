using AI.Shared.Services.CodeExecution;
using AI.Shared.Services.CodeExecution.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace AI.Blazor.Client.Components.Pages;

/// <summary>
/// Test page for the Code Execution Agent functionality.
/// Provides an interactive interface to test dynamic C# code execution.
/// </summary>
public partial class CodeExecutionTest : ComponentBase
{
    [Inject]
    private ICodeExecutionService CodeExecutionService { get; set; } = default!;

    [Inject]
    private ILogger<CodeExecutionTest> Logger { get; set; } = default!;

    /// <summary>
    /// The C# code to execute.
    /// </summary>
    protected string Code { get; set; } = "1 + 2";

    /// <summary>
    /// The result of code execution.
    /// </summary>
    protected ExecutionResult? ExecutionResult { get; set; }

    /// <summary>
    /// The result of code validation.
    /// </summary>
    protected ValidationResult? ValidationResult { get; set; }

    /// <summary>
    /// Indicates whether code is currently being executed or validated.
    /// </summary>
    protected bool IsExecuting { get; set; }

    /// <summary>
    /// Executes the C# code using the Code Execution Agent.
    /// </summary>
    protected async Task ExecuteCode()
    {
        ExecutionResult = null;
        ValidationResult = null;
        IsExecuting = true;

        try
        {
            ExecutionResult = await CodeExecutionService.ExecuteCode(Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing code");
            ExecutionResult = ExecutionResult.Failure($"Unexpected error: {ex.Message}");
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Validates the C# code without executing it.
    /// </summary>
    protected async Task ValidateCode()
    {
        ExecutionResult = null;
        ValidationResult = null;
        IsExecuting = true;

        try
        {
            ValidationResult = await CodeExecutionService.ValidateCode(Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error validating code");
            ValidationResult = ValidationResult.Failure($"Validation error: {ex.Message}");
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Loads an example code snippet into the code editor.
    /// </summary>
    /// <param name="exampleCode">The example code to load.</param>
    protected void LoadExample(string exampleCode)
    {
        Code = exampleCode;
        ExecutionResult = null;
        ValidationResult = null;
    }
}